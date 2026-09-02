using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Настройки хранилища под конкретную сборку: где лежат файлы, с каким расширением,
    /// насколько строго проверяется версия схемы и чем данные пишутся.
    /// </summary>
    public interface IFileStorageOptions
    {
        string SavesDirectory { get; }

        string SaveExt { get; }

        VersionHandling VersionHandling { get; }

        /// <summary>
        /// Собирает носитель: файловое хранилище, при необходимости завёрнутое в декораторы
        /// (шифрование, сжатие, телеметрия).
        /// </summary>
        IPersistentStorage CreateStorage();
    }
}
