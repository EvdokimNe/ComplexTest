namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Результат обращения к носителю: пустой ответ «сохранения нет» и сбой чтения — разные
    /// вещи, и путать их нельзя, иначе ошибка диска выглядит как первый запуск игры.
    /// </summary>
    internal readonly struct ReadOutcome
    {
        public readonly byte[] Bytes;
        public readonly bool Failed;

        private ReadOutcome(byte[] bytes, bool failed)
        {
            Bytes = bytes;
            Failed = failed;
        }

        /// <summary>Носитель ничего не вернул. Файл нулевой длины сюда не относится — он повреждён.</summary>
        public bool IsEmpty => Bytes == null;

        public static ReadOutcome Read(byte[] bytes) => new ReadOutcome(bytes, false);

        public static ReadOutcome Broken() => new ReadOutcome(null, true);
    }
}
