namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Чем закончилась загрузка. Ожидаемые сбои возвращаются статусом, а не исключением.</summary>
    public enum LoadStatus
    {
        /// <summary>Данные прочитаны из основного файла.</summary>
        Loaded = 0,

        /// <summary>Сохранения нет. Не ошибка: первый запуск, новый слот, удалённый сегмент.</summary>
        NotFound = 1,

        /// <summary>Основной файл не читается, данные подняты из резервной копии.</summary>
        RecoveredFromBackup = 2,

        /// <summary>Файл повреждён: пустой, обрезанный, не сходится хеш или не разбирается payload.</summary>
        Corrupted = 3,

        /// <summary>Файл записан другим сериализатором (например, json вместо бинаря).</summary>
        FormatMismatch = 4,

        /// <summary>Файл содержит данные другого типа.</summary>
        TypeMismatch = 5,

        /// <summary>Файл сохранён более новой версией игры и не может быть прочитан.</summary>
        VersionTooNew = 6,

        /// <summary>
        /// Версия схемы в файле не совпадает с текущей, а <see cref="VersionHandling.Strict"/>
        /// запрещает читать такие данные.
        /// </summary>
        VersionMismatch = 7
    }
}
