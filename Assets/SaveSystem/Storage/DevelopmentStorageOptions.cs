using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Dev-билд: штатная папка устройства, но без шифрования — QA должен уметь достать сейв
    /// с тестового девайса и приложить его к баг-репорту.
    /// </summary>
    public sealed class DevelopmentStorageOptions : IFileStorageOptions
    {
        public string SavesDirectory => SavePaths.Persistent;

        public string SaveExt => "save";

        public VersionHandling VersionHandling => VersionHandling.Automatic;

        public IPersistentStorage CreateStorage() => new FileStorage(SavesDirectory, SaveExt);
    }
}
