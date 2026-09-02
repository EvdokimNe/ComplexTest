using SaveSystem.SaveSystem.Core;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor
{
    /// <summary>Project Settings page for the configuration asset consumed by SaveSystemFactory.</summary>
    internal static class SaveSystemProjectSettingsProvider
    {
        private const string AssetDirectory = "Assets/Settings";
        private const string AssetPath = AssetDirectory + "/SaveSystemConfiguration.asset";

        [SettingsProvider]
        private static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/Save System", SettingsScope.Project)
            {
                label = "Save System",
                guiHandler = _ => DrawGui()
            };

            return provider;
        }

        [MenuItem("Assets/Create/Save System/Configuration")]
        private static void CreateConfigurationFromMenu()
        {
            SaveSystemConfiguration configuration = LoadOrCreate();
            Selection.activeObject = configuration;
            EditorGUIUtility.PingObject(configuration);
        }

        private static void DrawGui()
        {
            SaveSystemConfiguration configuration = AssetDatabase.LoadAssetAtPath<SaveSystemConfiguration>(AssetPath);
            if (configuration == null)
            {
                EditorGUILayout.HelpBox(
                "Configuration asset не создан. Без него SaveSystemFactory использует настройки по умолчанию для текущего build.",
                    MessageType.Info);

                if (GUILayout.Button("Create SaveSystemConfiguration"))
                    LoadOrCreate();
                return;
            }

            EditorGUILayout.LabelField("Runtime configuration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Передайте этот asset в SaveSystemFactory.Create(configuration) или назначьте его в bootstrap/DI.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);

            var serialized = new SerializedObject(configuration);
            EditorGUILayout.PropertyField(serialized.FindProperty("serializer"));
            EditorGUILayout.PropertyField(serialized.FindProperty("storage"));
            EditorGUILayout.PropertyField(serialized.FindProperty("versionHandling"));
            EditorGUILayout.PropertyField(serialized.FindProperty("prettyPrintInEditor"));

            serialized.ApplyModifiedProperties();

            if (configuration.Serializer == SaveSerializerKind.NewtonsoftJson &&
                !HasDefine("SAVESYSTEM_NEWTONSOFT"))
            {
                EditorGUILayout.HelpBox(
                    "Newtonsoft выбран, но define SAVESYSTEM_NEWTONSOFT выключен. " +
                    "В рантайме будет использован UnityJson.", MessageType.Warning);
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Select configuration asset"))
            {
                Selection.activeObject = configuration;
                EditorGUIUtility.PingObject(configuration);
            }
        }

        private static SaveSystemConfiguration LoadOrCreate()
        {
            SaveSystemConfiguration existing = AssetDatabase.LoadAssetAtPath<SaveSystemConfiguration>(AssetPath);
            if (existing != null)
                return existing;

            if (!AssetDatabase.IsValidFolder(AssetDirectory))
                AssetDatabase.CreateFolder("Assets", "Settings");

            var created = ScriptableObject.CreateInstance<SaveSystemConfiguration>();
            AssetDatabase.CreateAsset(created, AssetPath);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static bool HasDefine(string define)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            return System.Array.IndexOf(defines.Split(';'), define) >= 0;
        }
    }
}
