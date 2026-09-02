using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SaveSystem.SaveSystem.Core;
using SaveSystem.SaveSystem.Serialization;
using SaveSystem.SaveSystem.Storage;
using UnityEngine.TestTools;

namespace SaveSystem.Tests
{
    /// <summary>
    /// Проверяется главное требование к сохранениям: любой испорченный файл заканчивается
    /// понятным статусом и живым объектом, а не исключением посреди загрузки уровня.
    /// </summary>
    public class SaveServiceTests
    {
        private static readonly StorageKey Key = new StorageKey(SaveSlot.Default, new SaveId("progress"));

        private InMemoryStorage _storage;
        private SaveService _service;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryStorage();
            _service = new SaveService(_storage, new UnityJsonSerializer(), VersionHandling.Automatic, NullSaveLogger.Instance);
            _service.RegisterDefaults(new ActivatorDefaultsProvider<TestProgress>());
        }

        [Test]
        public void Saved_data_is_loaded_back()
        {
            SaveResult saved = _service.Save(Key, new TestProgress { Level = 12, PlayerName = "Ева" });

            Assert.IsTrue(saved.IsSuccess, saved.Error);
            Assert.Greater(saved.BytesWritten, 0);

            LoadResult<TestProgress> loaded = _service.Load<TestProgress>(Key);

            Assert.AreEqual(LoadStatus.Loaded, loaded.Status);
            Assert.AreEqual(12, loaded.Data.Level);
            Assert.AreEqual("Ева", loaded.Data.PlayerName);
        }

        [Test]
        public void Missing_save_returns_defaults()
        {
            LoadResult<TestProgress> loaded = _service.Load<TestProgress>(Key);

            Assert.AreEqual(LoadStatus.NotFound, loaded.Status);
            Assert.IsNotNull(loaded.Data, "без сохранения игра должна стартовать с дефолтов");
            Assert.AreEqual(0, loaded.Data.Level);
        }

        [Test]
        public void Empty_file_is_corrupted()
        {
            _storage.Write(Key, new byte[0]);

            Assert.AreEqual(LoadStatus.Corrupted, _service.Load<TestProgress>(Key).Status);
        }

        [Test]
        public void Truncated_file_is_corrupted()
        {
            _service.Save(Key, new TestProgress { Level = 3 });

            byte[] file = _storage.Read(Key);
            var truncated = new byte[file.Length - 4];
            System.Buffer.BlockCopy(file, 0, truncated, 0, truncated.Length);
            _storage.SetRaw(Key, truncated);

            LoadResult<TestProgress> loaded = _service.Load<TestProgress>(Key);

            Assert.AreEqual(LoadStatus.Corrupted, loaded.Status);
            Assert.IsNotNull(loaded.Data);
        }

        [Test]
        public void Tampered_payload_is_corrupted()
        {
            _service.Save(Key, new TestProgress { Level = 3 });

            byte[] file = _storage.Read(Key);
            file[file.Length - 2] ^= 0x20;
            _storage.SetRaw(Key, file);

            Assert.AreEqual(LoadStatus.Corrupted, _service.Load<TestProgress>(Key).Status);
        }

        [Test]
        public void File_from_another_serializer_is_rejected()
        {
            _storage.SetRaw(Key, Envelope("binary", "test-progress", 2, "{\"Level\":3}"));

            Assert.AreEqual(LoadStatus.FormatMismatch, _service.Load<TestProgress>(Key).Status);
        }

        [Test]
        public void File_with_another_type_is_rejected()
        {
            _storage.SetRaw(Key, Envelope("json", "inventory", 2, "{\"Level\":3}"));

            Assert.AreEqual(LoadStatus.TypeMismatch, _service.Load<TestProgress>(Key).Status);
        }

        [Test]
        public void File_from_newer_build_is_rejected()
        {
            _storage.SetRaw(Key, Envelope("json", "test-progress", 99, "{\"Level\":3}"));

            Assert.AreEqual(LoadStatus.VersionTooNew, _service.Load<TestProgress>(Key).Status);
        }

        [Test]
        public void Older_schema_is_read_in_automatic_mode()
        {
            _storage.SetRaw(Key, Envelope("json", "test-progress", 1, "{\"Level\":3}"));

            LoadResult<TestProgress> loaded = _service.Load<TestProgress>(Key);

            Assert.AreEqual(LoadStatus.Loaded, loaded.Status);
            Assert.AreEqual(3, loaded.Data.Level);
        }

        [Test]
        public void Older_schema_is_rejected_in_strict_mode()
        {
            var strict = new SaveService(_storage, new UnityJsonSerializer(), VersionHandling.Strict, NullSaveLogger.Instance);
            _storage.SetRaw(Key, Envelope("json", "test-progress", 1, "{\"Level\":3}"));

            Assert.AreEqual(LoadStatus.VersionMismatch, strict.Load<TestProgress>(Key).Status);
        }

        [Test]
        public void Broken_main_file_is_recovered_from_backup()
        {
            _service.Save(Key, new TestProgress { Level = 1, PlayerName = "первое" });
            _service.Save(Key, new TestProgress { Level = 2, PlayerName = "второе" });

            _storage.SetRaw(Key, Encoding.UTF8.GetBytes("мусор без заголовка"));

            LoadResult<TestProgress> loaded = _service.Load<TestProgress>(Key);

            Assert.AreEqual(LoadStatus.RecoveredFromBackup, loaded.Status);
            Assert.AreEqual(1, loaded.Data.Level, "из копии должно подняться предыдущее сохранение");
        }

        [Test]
        public void Slots_do_not_see_each_other()
        {
            var first = new StorageKey(new SaveSlot("slot_1"), new SaveId("progress"));
            var second = new StorageKey(new SaveSlot("slot_2"), new SaveId("progress"));

            _service.Save(first, new TestProgress { Level = 1 });
            _service.Save(second, new TestProgress { Level = 2 });

            Assert.AreEqual(1, _service.Load<TestProgress>(first).Data.Level);
            Assert.AreEqual(2, _service.Load<TestProgress>(second).Data.Level);

            IReadOnlyList<SaveSlot> slots = _service.GetSlots();
            Assert.AreEqual(2, slots.Count);
        }

        [Test]
        public void Deleting_slot_keeps_other_slots()
        {
            var first = new StorageKey(new SaveSlot("slot_1"), new SaveId("progress"));
            var second = new StorageKey(new SaveSlot("slot_2"), new SaveId("progress"));

            _service.Save(first, new TestProgress { Level = 1 });
            _service.Save(second, new TestProgress { Level = 2 });

            _service.DeleteSlot(new SaveSlot("slot_1"));

            Assert.AreEqual(LoadStatus.NotFound, _service.Load<TestProgress>(first).Status);
            Assert.AreEqual(LoadStatus.Loaded, _service.Load<TestProgress>(second).Status);
        }

        /// <summary>
        /// Асинхронный путь проверяется корутиной: продолжение возвращается в главный поток
        /// Unity, и блокирующее ожидание в тесте повесило бы редактор.
        /// </summary>
        [UnityTest]
        public IEnumerator Async_roundtrip_returns_same_data()
        {
            var save = _service.SaveAsync(Key, new TestProgress { Level = 42, PlayerName = "async" });
            while (!save.IsCompleted)
                yield return null;

            Assert.IsTrue(save.Result.IsSuccess, save.Result.Error);

            var load = _service.LoadAsync<TestProgress>(Key);
            while (!load.IsCompleted)
                yield return null;

            Assert.AreEqual(LoadStatus.Loaded, load.Result.Status);
            Assert.AreEqual(42, load.Result.Data.Level);
        }

        private static byte[] Envelope(string format, string typeId, int version, string payloadJson)
        {
            byte[] payload = Encoding.UTF8.GetBytes(payloadJson);
            var header = new SaveHeader
            {
                f = format,
                v = version,
                t = typeId,
                h = Fnv1a64.ComputeHex(payload),
                app = "test",
                utc = "2026-09-02T00:00:00Z"
            };

            return EnvelopeCodec.Encode(header, payload);
        }
    }
}
