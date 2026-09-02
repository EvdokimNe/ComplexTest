using System;
using System.Collections.Generic;
using System.Text;
using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Декоратор поверх любого носителя: данные уходят на него в шифрованном виде.
    /// Здесь намеренно простой XOR — он закрывает ровно одну задачу: сделать сейв нечитаемым
    /// для игрока, открывшего файл блокнотом.
    /// TODO: для релиза заменить на AES, ключ держать не в сборке, а рядом с идентификатором
    /// устройства или на сервере; от подмены данных защищает не шифр, а HMAC в заголовке.
    /// </summary>
    public sealed class EncryptedStorage : IPersistentStorage
    {
        private readonly IPersistentStorage _inner;
        private readonly byte[] _key;

        public EncryptedStorage(IPersistentStorage inner, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Пустой ключ шифрования.", nameof(key));

            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _key = Encoding.UTF8.GetBytes(key);
        }

        public bool SupportsBackgroundIo => _inner.SupportsBackgroundIo;

        public bool Exists(StorageKey key) => _inner.Exists(key);

        public byte[] Read(StorageKey key) => Transform(_inner.Read(key));

        public byte[] ReadBackup(StorageKey key) => Transform(_inner.ReadBackup(key));

        public void Write(StorageKey key, byte[] bytes) => _inner.Write(key, Transform(bytes));

        public void Delete(StorageKey key) => _inner.Delete(key);

        public IReadOnlyList<string> EnumerateSlots() => _inner.EnumerateSlots();

        public void DeleteSlot(SaveSlot slot) => _inner.DeleteSlot(slot);

        /// <summary>XOR симметричен, поэтому одна операция работает в обе стороны.</summary>
        private byte[] Transform(byte[] bytes)
        {
            if (bytes == null)
                return null;

            var result = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
                result[i] = (byte)(bytes[i] ^ _key[i % _key.Length]);

            return result;
        }
    }
}
