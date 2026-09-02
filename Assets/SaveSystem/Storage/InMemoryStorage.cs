using System;
using System.Collections.Generic;
using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Хранилище в памяти: тесты, отладка и режимы, где сохраняться на диск не нужно.
    /// Резервную копию ведёт так же, как файловое, — иначе на нём не проверить восстановление.
    /// </summary>
    public sealed class InMemoryStorage : IPersistentStorage
    {
        private readonly Dictionary<StorageKey, byte[]> _data = new Dictionary<StorageKey, byte[]>();
        private readonly Dictionary<StorageKey, byte[]> _backups = new Dictionary<StorageKey, byte[]>();

        public bool SupportsBackgroundIo => true;

        public bool Exists(StorageKey key) => _data.ContainsKey(key);

        public byte[] Read(StorageKey key) => _data.TryGetValue(key, out byte[] bytes) ? bytes : null;

        public byte[] ReadBackup(StorageKey key) => _backups.TryGetValue(key, out byte[] bytes) ? bytes : null;

        public void Write(StorageKey key, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (_data.TryGetValue(key, out byte[] previous))
                _backups[key] = previous;

            _data[key] = bytes;
        }

        public void Delete(StorageKey key)
        {
            _data.Remove(key);
            _backups.Remove(key);
        }

        /// <summary>
        /// Подменяет данные, не трогая резервную копию. Нужно тестам и отладке, чтобы
        /// воспроизвести повреждённый основной файл при целой копии.
        /// </summary>
        public void SetRaw(StorageKey key, byte[] bytes) => _data[key] = bytes;

        public IReadOnlyList<string> EnumerateSlots()
        {
            var slots = new List<string>();

            foreach (StorageKey key in _data.Keys)
            {
                if (!slots.Contains(key.Slot.Name))
                    slots.Add(key.Slot.Name);
            }

            return slots;
        }

        public void DeleteSlot(SaveSlot slot)
        {
            var keys = new List<StorageKey>();

            foreach (StorageKey key in _data.Keys)
            {
                if (key.Slot.Equals(slot))
                    keys.Add(key);
            }

            for (int i = 0; i < keys.Count; i++)
                Delete(keys[i]);
        }
    }
}
