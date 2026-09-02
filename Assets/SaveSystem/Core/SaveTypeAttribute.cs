using System;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Стабильный идентификатор типа и версия его схемы. Оба значения уходят в заголовок файла,
    /// поэтому класс можно переименовать или перенести в другую сборку, не потеряв сохранения.
    /// Без атрибута идентификатором становится Type.FullName, а версией — 1.
    /// </summary>
    /// <example>
    /// [SaveType("player-progress", version: 2)]
    /// public class PlayerProgress { ... }
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class SaveTypeAttribute : Attribute
    {
        /// <summary>Идентификатор типа в файле. Менять нельзя — это ломает старые сохранения.</summary>
        public string Id { get; }

        /// <summary>Версия схемы данных. Увеличивается при несовместимом изменении полей.</summary>
        public int Version { get; }

        public SaveTypeAttribute(string id, int version = 1)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("SaveType id не может быть пустым.", nameof(id));

            if (version < 1)
                throw new ArgumentOutOfRangeException(nameof(version), "Версия схемы начинается с 1.");

            Id = id;
            Version = version;
        }
    }
}
