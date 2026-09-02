using System;
using System.IO;
using NUnit.Framework;
using SaveSystem.SaveSystem.Core;
using SaveSystem.SaveSystem.Storage;

namespace SaveSystem.Tests
{
    public sealed class FileStorageTests
    {
        private string _directory;
        private FileStorage _storage;
        private readonly StorageKey _key = new StorageKey(SaveSlot.Default, new SaveId("progress"));

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "SaveSystemTests_" + Guid.NewGuid().ToString("N"));
            _storage = new FileStorage(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }

        [Test]
        public void Write_creates_missing_directory_and_roundtrips_bytes()
        {
            byte[] expected = { 1, 2, 3, 4 };
            _storage.Write(_key, expected);

            Assert.IsTrue(_storage.Exists(_key));
            Assert.AreEqual(expected, _storage.Read(_key));
        }

        [Test]
        public void Second_write_keeps_previous_data_as_backup()
        {
            _storage.Write(_key, new byte[] { 1 });
            _storage.Write(_key, new byte[] { 2 });

            Assert.AreEqual(new byte[] { 2 }, _storage.Read(_key));
            Assert.AreEqual(new byte[] { 1 }, _storage.ReadBackup(_key));
        }

        [Test]
        public void Delete_removes_main_backup_and_temp_files()
        {
            _storage.Write(_key, new byte[] { 1 });
            _storage.Write(_key, new byte[] { 2 });
            _storage.Delete(_key);

            Assert.IsFalse(_storage.Exists(_key));
            Assert.IsNull(_storage.ReadBackup(_key));
        }
    }
}
