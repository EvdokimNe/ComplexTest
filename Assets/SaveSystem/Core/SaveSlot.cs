using System;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Слот сохранения: "default", "slot_1", "autosave". Один слот — одна папка на диске,
    /// внутри неё лежат файлы сегментов.
    /// </summary>
    public readonly struct SaveSlot : IEquatable<SaveSlot>
    {
        public const string DefaultName = "default";

        public readonly string Name;

        public SaveSlot(string name)
        {
            Name = name;
        }

        public static SaveSlot Default => new SaveSlot(DefaultName);

        public bool IsValid => !string.IsNullOrWhiteSpace(Name);

        public static implicit operator SaveSlot(string name) => new SaveSlot(name);

        public bool Equals(SaveSlot other) => string.Equals(Name, other.Name, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is SaveSlot other && Equals(other);

        public override int GetHashCode() => Name == null ? 0 : StringComparer.Ordinal.GetHashCode(Name);

        public override string ToString() => Name ?? string.Empty;
    }
}
