using System.Collections.Generic;
using SaveSystem.SaveSystem.Core;
namespace SaveSystem.SaveSystem.Storage
{
    /// <summary>
    /// Носитель: байты по ключу. Ничего не знает о формате данных — этим занят сериализатор.
    /// </summary>
    public interface IPersistentStorage
    {
        /// <summary>
        /// Можно ли выполнять чтение и запись вне главного потока. У файла — да, у PlayerPrefs — нет.
        /// Сервис по этому флагу решает, уводить ли операцию в пул потоков.
        /// </summary>
        bool SupportsBackgroundIo { get; }

        bool Exists(StorageKey key);

        /// <summary>Читает данные по ключу. Возвращает null, если сохранения нет.</summary>
        byte[] Read(StorageKey key);

        /// <summary>
        /// Читает резервную копию предыдущего сохранения. Возвращает null, если копии нет или
        /// носитель их не ведёт.
        /// </summary>
        byte[] ReadBackup(StorageKey key);

        /// <summary>
        /// Записывает данные по ключу. Реализация обязана гарантировать, что при
        /// прерывании (краш, потеря питания) на носителе останется либо предыдущее
        /// состояние, либо новое — но не частично записанное.
        /// </summary>
        void Write(StorageKey key, byte[] bytes);

        void Delete(StorageKey key);

        IReadOnlyList<string> EnumerateSlots();

        void DeleteSlot(SaveSlot slot);
    }
}
