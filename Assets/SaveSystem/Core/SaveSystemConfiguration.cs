using System;
using SaveSystem.SaveSystem.Serialization;
using SaveSystem.SaveSystem.Storage;
using UnityEngine;
#if SAVESYSTEM_NEWTONSOFT
using SaveSystem.SaveSystem.Json;
#endif

namespace SaveSystem.SaveSystem.Core
{
    public enum SaveSerializerKind
    {
        UnityJson = 0,
        NewtonsoftJson = 1
    }

    public enum SaveStorageKind
    {
        BuildDefault = 0,
        PlayerPrefs = 1
    }

    /// <summary>Project-level composition settings used by <see cref="SaveSystemFactory"/>.</summary>
    public sealed class SaveSystemConfiguration : ScriptableObject
    {
        [SerializeField] private SaveSerializerKind serializer = SaveSerializerKind.UnityJson;
        [SerializeField] private SaveStorageKind storage = SaveStorageKind.BuildDefault;
        [SerializeField] private VersionHandling versionHandling = VersionHandling.Automatic;
        [SerializeField] private bool prettyPrintInEditor = true;

        public SaveSerializerKind Serializer => serializer;
        public SaveStorageKind Storage => storage;
        public VersionHandling VersionHandling => versionHandling;

        public IDataSerializer CreateSerializer()
        {
            switch (serializer)
            {
                case SaveSerializerKind.NewtonsoftJson:
#if SAVESYSTEM_NEWTONSOFT
                    return new NewtonsoftJsonSerializer();
#else
                    Debug.LogWarning("SaveSystem: Newtonsoft выбран в настройках, но интеграция отключена. Используется UnityJson.");
                    return new UnityJsonSerializer(prettyPrintInEditor && Application.isEditor);
#endif
                default:
                    return new UnityJsonSerializer(prettyPrintInEditor && Application.isEditor);
            }
        }

        public IPersistentStorage CreateStorage()
        {
            return storage == SaveStorageKind.PlayerPrefs
                ? new PlayerPrefsStorage()
                : StorageOptions.Default.CreateStorage();
        }
    }
}
