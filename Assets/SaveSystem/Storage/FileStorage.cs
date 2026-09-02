using System;
using System.Collections.Generic;
using System.IO;
using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Файлы на диске: {SavesDirectory}/{slot}/{id}.{ext}. Слот — папка, сегмент — файл,
    /// поэтому сегменты сохраняются и удаляются независимо друг от друга.
    /// </summary>
    public sealed class FileStorage : IPersistentStorage
    {
        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        private readonly string _savesDirectory;
        private readonly string _extension;

        public FileStorage(string savesDirectory, string extension = "save")
        {
            if (string.IsNullOrWhiteSpace(savesDirectory))
                throw new ArgumentException("Не задана папка сохранений.", nameof(savesDirectory));

            _savesDirectory = savesDirectory;
            _extension = string.IsNullOrWhiteSpace(extension) ? "save" : extension.TrimStart('.');
        }

        public bool SupportsBackgroundIo => true;

        public bool Exists(StorageKey key) => File.Exists(FilePath(key));

        public byte[] Read(StorageKey key) => ReadFile(FilePath(key));

        public byte[] ReadBackup(StorageKey key) => ReadFile(FilePath(key) + BackupSuffix);

        /// <inheritdoc />
        /// <remarks>
        /// Прямая запись поверх файла обрезала бы его в ноль: краш в этот момент уничтожает
        /// прогресс. Поэтому данные уходят во временный файл, сбрасываются на физический диск и
        /// подменяют старый файл одной атомарной операцией ФС. Предыдущая версия при этом
        /// становится резервной копией.
        /// </remarks>
        public void Write(StorageKey key, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            string path = FilePath(key);
            string temp = path + TempSuffix;
            string backup = path + BackupSuffix;

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? _savesDirectory);

            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (!File.Exists(path))
            {
                File.Move(temp, path);
                return;
            }

            try
            {
                File.Replace(temp, path, backup, ignoreMetadataErrors: true);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceManually(temp, path, backup);
            }
            catch (NotSupportedException)
            {
                ReplaceManually(temp, path, backup);
            }
        }

        public void Delete(StorageKey key)
        {
            string path = FilePath(key);

            DeleteFile(path);
            DeleteFile(path + BackupSuffix);
            DeleteFile(path + TempSuffix);
        }

        public IReadOnlyList<string> EnumerateSlots()
        {
            if (!Directory.Exists(_savesDirectory))
                return Array.Empty<string>();

            string[] directories = Directory.GetDirectories(_savesDirectory);
            var slots = new List<string>(directories.Length);

            for (int i = 0; i < directories.Length; i++)
                slots.Add(Path.GetFileName(directories[i]));

            return slots;
        }

        public void DeleteSlot(SaveSlot slot)
        {
            string directory = SlotDirectory(slot);

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }

        private string SlotDirectory(SaveSlot slot) => Path.Combine(_savesDirectory, ValidateSegment(slot.Name));

        private string FilePath(StorageKey key) =>
            Path.Combine(SlotDirectory(key.Slot), ValidateSegment(key.Id.Value) + "." + _extension);

        private static byte[] ReadFile(string path) => File.Exists(path) ? File.ReadAllBytes(path) : null;

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>Запасной путь для платформ без File.Replace. Окно уязвимости здесь есть, но оно короче записи.</summary>
        private static void ReplaceManually(string temp, string path, string backup)
        {
            DeleteFile(backup);
            File.Move(path, backup);
            File.Move(temp, path);
        }

        /// <summary>
        /// Слот и идентификатор становятся частями пути, поэтому разделители и прочие
        /// недопустимые символы отсекаются здесь, а не превращаются в запись мимо папки сейвов.
        /// </summary>
        private static string ValidateSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
                throw new ArgumentException("Пустое имя слота или сегмента.");

            if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Недопустимые символы в имени: '" + segment + "'.");

            return segment;
        }
    }
}
