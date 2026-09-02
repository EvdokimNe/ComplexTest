using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SaveSystem.SaveSystem.Serialization;
using SaveSystem.SaveSystem.Storage;
using UnityEngine;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Склейка трёх слоёв: дефолты — сериализатор — конверт — носитель. Здесь же живёт политика
    /// восстановления: повреждённый основной файл заменяется резервной копией, а если и она
    /// не читается — данными из <see cref="IDefaultsProvider{T}"/>.
    /// </summary>
    public sealed class SaveService : ISaveService
    {
        private readonly IPersistentStorage _storage;
        private readonly IDataSerializer _serializer;
        private readonly VersionHandling _versionHandling;
        private readonly ISaveLogger _logger;
        private readonly Dictionary<Type, Func<object>> _defaults = new Dictionary<Type, Func<object>>();

        // Одна операция с носителем за раз: две параллельные записи в один файл дают битый сейв.
        private readonly SemaphoreSlim _ioGate = new SemaphoreSlim(1, 1);

        public SaveService(
            IPersistentStorage storage,
            IDataSerializer serializer,
            VersionHandling versionHandling = VersionHandling.Automatic,
            ISaveLogger logger = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _versionHandling = versionHandling;
            _logger = logger ?? NullSaveLogger.Instance;
        }

        public bool Exists(StorageKey key)
        {
            ValidateKey(key);
            return _storage.Exists(key);
        }

        public SaveResult Save<T>(StorageKey key, T data) where T : class
        {
            ValidateKey(key);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] bytes;
            try
            {
                bytes = Encode(data);
            }
            catch (Exception exception)
            {
                return Fail(key, "не удалось сериализовать данные", exception);
            }

            try
            {
                _storage.Write(key, bytes);
                return SaveResult.Saved(bytes.Length);
            }
            catch (Exception exception)
            {
                return Fail(key, "не удалось записать сохранение", exception);
            }
        }

        public async Task<SaveResult> SaveAsync<T>(StorageKey key, T data, CancellationToken cancellationToken = default)
            where T : class
        {
            ValidateKey(key);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] bytes;
            try
            {
                // Сериализация остаётся на вызывающем потоке: JsonUtility и Application.version
                // доступны только из главного потока Unity.
                bytes = Encode(data);
            }
            catch (Exception exception)
            {
                return Fail(key, "не удалось сериализовать данные", exception);
            }

            await _ioGate.WaitAsync(cancellationToken);
            try
            {
                if (_storage.SupportsBackgroundIo)
                    await Task.Run(() => _storage.Write(key, bytes), cancellationToken);
                else
                    _storage.Write(key, bytes);

                return SaveResult.Saved(bytes.Length);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Fail(key, "не удалось записать сохранение", exception);
            }
            finally
            {
                _ioGate.Release();
            }
        }

        public LoadResult<T> Load<T>(StorageKey key) where T : class
        {
            ValidateKey(key);
            return Complete<T>(key, ReadSafe(key, backup: false));
        }

        public async Task<LoadResult<T>> LoadAsync<T>(StorageKey key, CancellationToken cancellationToken = default)
            where T : class
        {
            ValidateKey(key);

            ReadOutcome outcome;
            await _ioGate.WaitAsync(cancellationToken);
            try
            {
                outcome = _storage.SupportsBackgroundIo
                    ? await Task.Run(() => ReadSafe(key, backup: false), cancellationToken)
                    : ReadSafe(key, backup: false);
            }
            finally
            {
                _ioGate.Release();
            }

            // Разбор данных снова на главном потоке — по той же причине, что и сериализация.
            return Complete<T>(key, outcome);
        }

        public void Delete(StorageKey key)
        {
            ValidateKey(key);

            try
            {
                _storage.Delete(key);
            }
            catch (Exception exception)
            {
                _logger.Error(key + ": не удалось удалить сохранение", exception);
            }
        }

        public IReadOnlyList<SaveSlot> GetSlots()
        {
            IReadOnlyList<string> names = _storage.EnumerateSlots();
            var slots = new List<SaveSlot>(names.Count);

            for (int i = 0; i < names.Count; i++)
                slots.Add(new SaveSlot(names[i]));

            return slots;
        }

        public void DeleteSlot(SaveSlot slot)
        {
            if (!slot.IsValid)
                throw new ArgumentException("Имя слота не может быть пустым.", nameof(slot));

            try
            {
                _storage.DeleteSlot(slot);
            }
            catch (Exception exception)
            {
                _logger.Error(slot + ": не удалось удалить слот", exception);
            }
        }

        public void RegisterDefaults<T>(IDefaultsProvider<T> provider) where T : class
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _defaults[typeof(T)] = () => provider.CreateDefault();
        }

        private byte[] Encode<T>(T data) where T : class
        {
            SaveTypeDescriptor descriptor = SaveTypeInfo.Of(typeof(T));
            byte[] payload = _serializer.Serialize(data, typeof(T));

            var header = new SaveHeader
            {
                f = _serializer.FormatId,
                v = descriptor.Version,
                t = descriptor.Id,
                h = Fnv1a64.ComputeHex(payload),
                app = Application.version,
                utc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };

            return EnvelopeCodec.Encode(header, payload);
        }

        private LoadResult<T> Complete<T>(StorageKey key, ReadOutcome primary) where T : class
        {
            if (primary.IsEmpty && !primary.Failed)
                return LoadResult<T>.NotFound(CreateDefault<T>());

            LoadStatus status = LoadStatus.Corrupted;
            string error = "носитель не отдал данные";

            if (!primary.IsEmpty && TryDecode(primary.Bytes, out T data, out status, out error))
                return LoadResult<T>.Loaded(data);

            // Резервная копия помогает только против повреждения. Чужой формат, чужой тип
            // и версия из будущего лежат и в ней — пробовать её там незачем.
            if (status == LoadStatus.Corrupted)
            {
                ReadOutcome backup = ReadSafe(key, backup: true);

                if (!backup.IsEmpty && TryDecode(backup.Bytes, out T recovered, out _, out _))
                {
                    _logger.Warning(key + ": основной файл повреждён (" + error +
                                    "), данные подняты из резервной копии");
                    return LoadResult<T>.RecoveredFromBackup(recovered, error);
                }
            }

            _logger.Error(key + ": " + status + " — " + error);
            return LoadResult<T>.Failed(status, CreateDefault<T>(), error);
        }

        private bool TryDecode<T>(byte[] bytes, out T data, out LoadStatus status, out string error) where T : class
        {
            data = null;

            if (!EnvelopeCodec.TryDecode(bytes, out SaveEnvelope envelope, out error))
            {
                status = LoadStatus.Corrupted;
                return false;
            }

            SaveHeader header = envelope.Header;
            SaveTypeDescriptor descriptor = SaveTypeInfo.Of(typeof(T));

            if (!string.Equals(header.f, _serializer.FormatId, StringComparison.Ordinal))
            {
                status = LoadStatus.FormatMismatch;
                error = "файл записан форматом '" + header.f + "', текущий — '" + _serializer.FormatId + "'";
                return false;
            }

            if (!string.IsNullOrEmpty(header.t) && !string.Equals(header.t, descriptor.Id, StringComparison.Ordinal))
            {
                status = LoadStatus.TypeMismatch;
                error = "в файле лежит тип '" + header.t + "', ожидался '" + descriptor.Id + "'";
                return false;
            }

            if (!IsVersionAcceptable(header.v, descriptor.Version, out status, out error))
                return false;

            if (!string.IsNullOrEmpty(header.h) &&
                !string.Equals(header.h, Fnv1a64.ComputeHex(envelope.Payload), StringComparison.Ordinal))
            {
                status = LoadStatus.Corrupted;
                error = "не сходится контрольная сумма: файл обрезан или повреждён";
                return false;
            }

            try
            {
                data = _serializer.Deserialize(envelope.Payload, typeof(T)) as T;
            }
            catch (Exception exception)
            {
                status = LoadStatus.Corrupted;
                error = "данные не разбираются: " + exception.Message;
                return false;
            }

            if (data == null)
            {
                status = LoadStatus.Corrupted;
                error = "данные разобрались в null";
                return false;
            }

            status = LoadStatus.Loaded;
            error = null;
            return true;
        }

        private bool IsVersionAcceptable(int fileVersion, int currentVersion, out LoadStatus status, out string error)
        {
            status = LoadStatus.Loaded;
            error = null;

            if (_versionHandling == VersionHandling.Ignore || fileVersion == currentVersion)
                return true;

            if (fileVersion > currentVersion)
            {
                status = LoadStatus.VersionTooNew;
                error = "сохранение из более новой версии игры: схема " + fileVersion +
                        ", поддерживается " + currentVersion;
                return false;
            }

            if (_versionHandling == VersionHandling.Strict)
            {
                status = LoadStatus.VersionMismatch;
                error = "схема файла " + fileVersion + ", ожидается " + currentVersion + " (режим Strict)";
                return false;
            }

            // Automatic: старая схема читается как есть. Сюда встраивается миграция —
            // конвертация payload из версии fileVersion в currentVersion перед разбором.
            _logger.Warning("сохранение схемы " + fileVersion + " читается как " + currentVersion +
                            ": миграция не зарегистрирована");
            return true;
        }

        private ReadOutcome ReadSafe(StorageKey key, bool backup)
        {
            try
            {
                byte[] bytes = backup ? _storage.ReadBackup(key) : _storage.Read(key);
                return ReadOutcome.Read(bytes);
            }
            catch (Exception exception)
            {
                _logger.Error(key + ": ошибка чтения носителя", exception);
                return ReadOutcome.Broken();
            }
        }

        private T CreateDefault<T>() where T : class
        {
            return _defaults.TryGetValue(typeof(T), out Func<object> factory) ? (T)factory() : null;
        }

        private SaveResult Fail(StorageKey key, string message, Exception exception)
        {
            _logger.Error(key + ": " + message, exception);
            return SaveResult.Failed(message + ": " + exception.Message);
        }

        private static void ValidateKey(StorageKey key)
        {
            if (!key.IsValid)
                throw new ArgumentException("StorageKey должен содержать непустые слот и идентификатор.", nameof(key));
        }
    }
}
