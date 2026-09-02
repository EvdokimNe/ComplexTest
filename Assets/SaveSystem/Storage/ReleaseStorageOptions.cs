using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Релиз: штатная папка устройства, файлы зашифрованы.
    /// Ключ здесь лежит в сборке — этого хватает против случайной правки сейва, но не против
    /// целенаправленного взлома. Реальный проект берёт ключ с сервера или из Keystore.
    /// </summary>
    public sealed class ReleaseStorageOptions : IFileStorageOptions
    {
        private const string EncryptionKey = "save-system-default-key";

        public string SavesDirectory => SavePaths.Persistent;

        public string SaveExt => "save";

        public VersionHandling VersionHandling => VersionHandling.Automatic;

        public IPersistentStorage CreateStorage() =>
            new EncryptedStorage(new FileStorage(SavesDirectory, SaveExt), EncryptionKey);
    }
}
