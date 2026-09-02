using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Редактор: сейвы лежат рядом с проектом и не шифруются — их должно быть удобно открыть,
    /// прочитать и поправить руками при разборе бага.
    /// </summary>
    public sealed class EditorStorageOptions : IFileStorageOptions
    {
        public string SavesDirectory => SavePaths.ProjectLocal;

        public string SaveExt => "save";

        public VersionHandling VersionHandling => VersionHandling.Automatic;

        public IPersistentStorage CreateStorage() => new FileStorage(SavesDirectory, SaveExt);
    }
}
