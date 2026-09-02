namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Выбор настроек под текущую сборку. Разделение ровно одно: в редакторе и dev-билде сейв
    /// читаемый, в релизе — зашифрованный.
    /// Платформенные ветки (WebGL и консоли, где вместо файлов нужен PlayerPrefs или платформенное
    /// API) добавляются сюда же.
    /// </summary>
    public static class StorageOptions
    {
#if UNITY_EDITOR
        public static readonly IFileStorageOptions Default = new EditorStorageOptions();
#elif DEVELOPMENT_BUILD
        public static readonly IFileStorageOptions Default = new DevelopmentStorageOptions();
#else
        public static readonly IFileStorageOptions Default = new ReleaseStorageOptions();
#endif
    }
}
