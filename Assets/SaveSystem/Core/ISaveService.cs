using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Точка входа модуля: сохранить, загрузить, удалить. Ничего не знает про автосейв и про то,
    /// когда именно данные пора писать — этим занимается слой выше, который дёргает сервис.
    /// Асинхронные методы вызываются из главного потока: сериализация упирается в Unity API,
    /// в пул уходит только работа с носителем.
    /// </summary>
    public interface ISaveService
    {
        bool Exists(StorageKey key);

        /// <summary>
        /// Загружает данные. Ожидаемые сбои (нет файла, файл повреждён, чужой формат) приходят
        /// статусом в <see cref="LoadResult{T}"/>, а не исключением.
        /// </summary>
        LoadResult<T> Load<T>(StorageKey key) where T : class;

        SaveResult Save<T>(StorageKey key, T data) where T : class;

        void Delete(StorageKey key);

        Task<LoadResult<T>> LoadAsync<T>(StorageKey key, CancellationToken cancellationToken = default) where T : class;

        Task<SaveResult> SaveAsync<T>(StorageKey key, T data, CancellationToken cancellationToken = default) where T : class;

        IReadOnlyList<SaveSlot> GetSlots();

        void DeleteSlot(SaveSlot slot);

        /// <summary>
        /// Задаёт, что вернуть, если сохранения нет или оно не читается. Без провайдера
        /// <see cref="LoadResult{T}.Data"/> в этих случаях остаётся null.
        /// </summary>
        void RegisterDefaults<T>(IDefaultsProvider<T> provider) where T : class;
    }
}
