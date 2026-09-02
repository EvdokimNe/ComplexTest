namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Результат загрузки. <see cref="Status"/> говорит, что произошло, <see cref="Data"/> —
    /// с чем можно продолжать игру: при сбое это объект от <see cref="IDefaultsProvider{T}"/>,
    /// а если провайдер не зарегистрирован — null.
    /// </summary>
    public readonly struct LoadResult<T> where T : class
    {
        public readonly LoadStatus Status;
        public readonly T Data;
        public readonly string Error;

        private LoadResult(LoadStatus status, T data, string error)
        {
            Status = status;
            Data = data;
            Error = error;
        }

        /// <summary>Данные пришли из сохранения, а не из дефолтов.</summary>
        public bool IsSuccess => Status == LoadStatus.Loaded || Status == LoadStatus.RecoveredFromBackup;

        public bool HasData => Data != null;

        public static LoadResult<T> Loaded(T data) => new LoadResult<T>(LoadStatus.Loaded, data, null);

        public static LoadResult<T> RecoveredFromBackup(T data, string error) =>
            new LoadResult<T>(LoadStatus.RecoveredFromBackup, data, error);

        public static LoadResult<T> NotFound(T fallback) => new LoadResult<T>(LoadStatus.NotFound, fallback, null);

        public static LoadResult<T> Failed(LoadStatus status, T fallback, string error) =>
            new LoadResult<T>(status, fallback, error);

        public override string ToString() => Error == null ? Status.ToString() : Status + ": " + Error;
    }
}
