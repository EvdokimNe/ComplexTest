#if SAVESYSTEM_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.SaveSystem.Core;

namespace SaveSystem.SaveSystem.Async
{
    /// <summary>
    /// Мост в UniTask для проектов, которые целиком живут на нём.
    /// Базовый модуль сознательно остаётся на Task: он не должен тянуть за собой пакет.
    /// AsUniTask не создаёт лишних аллокаций на горячем пути — сохранение и так упирается в диск.
    /// </summary>
    public static class SaveServiceUniTaskExtensions
    {
        public static UniTask<LoadResult<T>> LoadUniTask<T>(
            this ISaveService service,
            StorageKey key,
            CancellationToken cancellationToken = default) where T : class
        {
            return service.LoadAsync<T>(key, cancellationToken).AsUniTask();
        }

        public static UniTask<SaveResult> SaveUniTask<T>(
            this ISaveService service,
            StorageKey key,
            T data,
            CancellationToken cancellationToken = default) where T : class
        {
            return service.SaveAsync(key, data, cancellationToken).AsUniTask();
        }
    }
}
#endif
