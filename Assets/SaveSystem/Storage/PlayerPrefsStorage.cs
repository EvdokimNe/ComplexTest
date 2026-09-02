using System;
using System.Collections.Generic;
using SaveSystem.SaveSystem.Core;
using UnityEngine;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Хранилище поверх PlayerPrefs: байты уезжают в base64. Нужно там, где файловой системы
    /// нет или она недоступна напрямую — WebGL, часть консолей, быстрые прототипы.
    /// PlayerPrefs не умеет перечислять ключи, поэтому список записей ведётся отдельным индексом.
    /// </summary>
    public sealed class PlayerPrefsStorage : IPersistentStorage
    {
        private const char IndexSeparator = ';';

        private readonly string _prefix;

        public PlayerPrefsStorage(string prefix = "save.")
        {
            _prefix = prefix ?? string.Empty;
        }

        /// <summary>PlayerPrefs доступен только из главного потока.</summary>
        public bool SupportsBackgroundIo => false;

        public bool Exists(StorageKey key) => PlayerPrefs.HasKey(PrefsKey(key));

        public byte[] Read(StorageKey key)
        {
            string prefsKey = PrefsKey(key);

            if (!PlayerPrefs.HasKey(prefsKey))
                return null;

            string encoded = PlayerPrefs.GetString(prefsKey);

            try
            {
                return Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                // Значение испорчено на уровне носителя — отдаём как есть, дальше сработает
                // проверка конверта и вернёт Corrupted.
                return System.Text.Encoding.UTF8.GetBytes(encoded);
            }
        }

        /// <summary>PlayerPrefs пишет значение целиком, поэтому резервная копия здесь не нужна.</summary>
        public byte[] ReadBackup(StorageKey key) => null;

        public void Write(StorageKey key, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            PlayerPrefs.SetString(PrefsKey(key), Convert.ToBase64String(bytes));
            AddToIndex(key);
            PlayerPrefs.Save();
        }

        public void Delete(StorageKey key)
        {
            PlayerPrefs.DeleteKey(PrefsKey(key));
            RemoveFromIndex(key);
            PlayerPrefs.Save();
        }

        public IReadOnlyList<string> EnumerateSlots()
        {
            var slots = new List<string>();

            foreach (string entry in ReadIndex())
            {
                string slot = SlotOf(entry);

                if (slot.Length > 0 && !slots.Contains(slot))
                    slots.Add(slot);
            }

            return slots;
        }

        public void DeleteSlot(SaveSlot slot)
        {
            List<string> index = ReadIndex();
            var kept = new List<string>(index.Count);

            for (int i = 0; i < index.Count; i++)
            {
                if (string.Equals(SlotOf(index[i]), slot.Name, StringComparison.Ordinal))
                    PlayerPrefs.DeleteKey(_prefix + index[i]);
                else
                    kept.Add(index[i]);
            }

            WriteIndex(kept);
            PlayerPrefs.Save();
        }

        private string PrefsKey(StorageKey key) => _prefix + Entry(key);

        private string IndexKey => _prefix + "__index";

        private static string Entry(StorageKey key) => key.Slot.Name + "/" + key.Id.Value;

        private static string SlotOf(string entry)
        {
            int separator = entry.IndexOf('/');
            return separator > 0 ? entry.Substring(0, separator) : string.Empty;
        }

        private List<string> ReadIndex()
        {
            string raw = PlayerPrefs.GetString(IndexKey, string.Empty);

            if (raw.Length == 0)
                return new List<string>();

            return new List<string>(raw.Split(IndexSeparator));
        }

        private void WriteIndex(List<string> entries) =>
            PlayerPrefs.SetString(IndexKey, string.Join(IndexSeparator.ToString(), entries));

        private void AddToIndex(StorageKey key)
        {
            List<string> index = ReadIndex();
            string entry = Entry(key);

            if (index.Contains(entry))
                return;

            index.Add(entry);
            WriteIndex(index);
        }

        private void RemoveFromIndex(StorageKey key)
        {
            List<string> index = ReadIndex();

            if (index.Remove(Entry(key)))
                WriteIndex(index);
        }
    }
}
