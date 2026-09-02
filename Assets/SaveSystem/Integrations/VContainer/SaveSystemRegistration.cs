#if SAVESYSTEM_VCONTAINER
using SaveSystem.SaveSystem.Core;
using SaveSystem.SaveSystem.Serialization;
using SaveSystem.SaveSystem.Storage;
using UnityEngine;
using VContainer;

namespace SaveSystem.SaveSystem.DependencyInjection
{
    /// <summary>
    /// Регистрация модуля в VContainer одной строкой:
    /// builder.RegisterSaveSystem();
    /// Состав задаётся здесь, а не внутри модуля: сменить сериализатор или носитель — значит
    /// передать другой аргумент, не трогая код фич.
    /// </summary>
    public static class SaveSystemRegistration
    {
        public static IContainerBuilder RegisterSaveSystem(
            this IContainerBuilder builder,
            IFileStorageOptions options = null,
            IDataSerializer serializer = null)
        {
            IFileStorageOptions resolvedOptions = options ?? StorageOptions.Default;
            IDataSerializer resolvedSerializer = serializer ?? new UnityJsonSerializer(Application.isEditor);

            builder.RegisterInstance(resolvedOptions).As<IFileStorageOptions>();
            builder.RegisterInstance(resolvedSerializer).As<IDataSerializer>();
            builder.Register<ISaveLogger>(_ => new UnitySaveLogger(), Lifetime.Singleton);
            builder.Register<IPersistentStorage>(_ => resolvedOptions.CreateStorage(), Lifetime.Singleton);

            builder.Register<ISaveService>(
                container => new SaveService(
                    container.Resolve<IPersistentStorage>(),
                    container.Resolve<IDataSerializer>(),
                    resolvedOptions.VersionHandling,
                    container.Resolve<ISaveLogger>()),
                Lifetime.Singleton);

            return builder;
        }
    }
}
#endif
