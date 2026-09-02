using System;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Адрес данных внутри хранилища: слот + сегмент. Путь на диске из ключа собирает
    /// хранилище, а не вызывающий код — иначе PlayerPrefs, консоли и облако не подставить.
    /// </summary>
    public readonly struct StorageKey : IEquatable<StorageKey>
    {
        public readonly SaveSlot Slot;
        public readonly SaveId Id;

        public StorageKey(SaveSlot slot, SaveId id)
        {
            Slot = slot;
            Id = id;
        }

        public StorageKey(SaveId id) : this(SaveSlot.Default, id)
        {
        }

        public bool IsValid => Slot.IsValid && Id.IsValid;

        public bool Equals(StorageKey other) => Slot.Equals(other.Slot) && Id.Equals(other.Id);

        public override bool Equals(object obj) => obj is StorageKey other && Equals(other);

        public override int GetHashCode() => (Slot.GetHashCode() * 397) ^ Id.GetHashCode();

        public override string ToString() => Slot + "/" + Id;
    }
}
