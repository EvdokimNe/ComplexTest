namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Что делать, когда версия схемы в файле не совпадает с версией текущего типа.</summary>
    public enum VersionHandling
    {
        /// <summary>Версия не проверяется. Годится для настроек, где поля только добавляются.</summary>
        Ignore = 0,

        /// <summary>
        /// Файл старее текущей версии — читается как есть (сюда встраиваются миграции),
        /// файл новее — отклоняется как <see cref="LoadStatus.VersionTooNew"/>.
        /// </summary>
        Automatic = 1,

        /// <summary>Любое расхождение версий — отказ. Для данных, где нет права на ошибку.</summary>
        Strict = 2
    }
}
