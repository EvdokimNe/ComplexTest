using SaveSystem.SaveSystem.Serialization;
using SaveSystem.SaveSystem.Storage;
using UnityEngine;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Сборка сервиса по умолчанию для проектов без DI-контейнера. Там, где контейнер есть,
    /// состав лучше задавать в нём — см. интеграцию с VContainer.
    /// </summary>
    public static class SaveSystemFactory
    {
        public static ISaveService CreateDefault() => Create(StorageOptions.Default);

        public static ISaveService Create(SaveSystemConfiguration configuration, ISaveLogger logger = null)
        {
            if (configuration == null)
                throw new System.ArgumentNullException(nameof(configuration));

            return new SaveService(
                configuration.CreateStorage(),
                configuration.CreateSerializer(),
                configuration.VersionHandling,
                logger ?? new UnitySaveLogger());
        }

        public static ISaveService Create(
            IFileStorageOptions options,
            IDataSerializer serializer = null,
            ISaveLogger logger = null)
        {
            return new SaveService(
                options.CreateStorage(),
                serializer ?? new UnityJsonSerializer(Application.isEditor),
                options.VersionHandling,
                logger ?? new UnitySaveLogger());
        }
    }
}
