using System.IO;
using UnityEngine;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Кешированные пути к папкам сохранений.
    /// Application.persistentDataPath и dataPath читаются только из главного потока, а запись
    /// уходит в пул — поэтому значения снимаются один раз при старте и дальше берутся отсюда.
    /// </summary>
    public static class SavePaths
    {
        private const string FolderName = "Saves";

        static SavePaths()
        {
            Persistent = Path.Combine(Application.persistentDataPath, FolderName);
            ProjectLocal = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath, FolderName);
        }

        /// <summary>Штатное место сохранений на устройстве.</summary>
        public static string Persistent { get; private set; }

        /// <summary>Папка рядом с проектом: в редакторе сейвы удобно смотреть и чистить руками.</summary>
        public static string ProjectLocal { get; private set; }

        /// <summary>Прогревает кеш на главном потоке до того, как к нему обратится фоновая запись.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Warmup()
        {
        }
    }
}
