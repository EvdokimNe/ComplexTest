namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Результат сохранения. Ошибка носителя — это статус, а не исключение наружу.</summary>
    public readonly struct SaveResult
    {
        public readonly bool IsSuccess;

        /// <summary>Сколько байт ушло на носитель. Полезно логировать: сейв имеет привычку пухнуть.</summary>
        public readonly int BytesWritten;

        public readonly string Error;

        private SaveResult(bool isSuccess, int bytesWritten, string error)
        {
            IsSuccess = isSuccess;
            BytesWritten = bytesWritten;
            Error = error;
        }

        public static SaveResult Saved(int bytesWritten) => new SaveResult(true, bytesWritten, null);

        public static SaveResult Failed(string error) => new SaveResult(false, 0, error);

        public override string ToString() => IsSuccess ? "Saved " + BytesWritten + " B" : "Failed: " + Error;
    }
}
