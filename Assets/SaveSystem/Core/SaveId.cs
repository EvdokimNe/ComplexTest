using System;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Идентификатор сегмента сохранения: "progress", "settings", "quests".
    /// Становится именем файла внутри слота, поэтому не должен содержать разделителей пути.
    /// </summary>
    public readonly struct SaveId : IEquatable<SaveId>
    {
        public readonly string Value;

        public SaveId(string value)
        {
            Value = value;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Value);

        public static implicit operator SaveId(string value) => new SaveId(value);

        public bool Equals(SaveId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is SaveId other && Equals(other);

        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value ?? string.Empty;
    }
}
