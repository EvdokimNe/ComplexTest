using System;
namespace SaveSystem.SaveSystem.Serialization
{
    /// <summary>
    /// Заголовок файла сохранения — первая строка, отделённая от данных переводом строки.
    /// Читается и проверяется до разбора данных и не зависит от формата под ним, поэтому имена
    /// полей короткие: заголовок пишется в каждый файл.
    /// </summary>
    /// <example>
    /// {"f":"json","v":2,"t":"player-progress","h":"9f2a41c8d0b3e517","app":"1.0.0","utc":"2026-09-02T10:14:03Z"}
    /// </example>
    [Serializable]
    public struct SaveHeader
    {
        /// <summary>FormatId сериализатора: "json", "json.net", "binary".</summary>
        public string f;

        /// <summary>Версия схемы данных из [SaveType].</summary>
        public int v;

        /// <summary>Стабильный идентификатор типа из [SaveType].</summary>
        public string t;

        /// <summary>FNV-1a 64 от данных в hex. Контроль целостности, не защита от подмены.</summary>
        public string h;

        /// <summary>Application.version на момент сохранения. Нужен при разборе жалоб игроков.</summary>
        public string app;

        /// <summary>Время сохранения в UTC, ISO-8601. По нему выбирают свежий слот и мержат облако.</summary>
        public string utc;
    }
}
