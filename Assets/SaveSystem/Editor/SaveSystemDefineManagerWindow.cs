using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using PackageManagerInfo = UnityEditor.PackageManager.PackageInfo;

namespace SaveSystem.Editor
{
    /// <summary>Окно для включения optional-интеграций SaveSystem по build target.</summary>
    public sealed class SaveSystemDefineManagerWindow : EditorWindow
    {
        private sealed class Integration
        {
            public string Name;
            public string PackageName;
            public string Define;
            public string Description;
        }

        private static readonly Integration[] Integrations =
        {
            new Integration
            {
                Name = "Newtonsoft JSON",
                PackageName = "com.unity.nuget.newtonsoft-json",
                Define = "SAVESYSTEM_NEWTONSOFT",
                Description = "Расширенная JSON-сериализация: словари, свойства и конвертеры Unity-типов."
            },
            new Integration
            {
                Name = "UniTask",
                PackageName = "com.cysharp.unitask",
                Define = "SAVESYSTEM_UNITASK",
                Description = "Обёртки SaveAsync и LoadAsync, возвращающие UniTask."
            },
            new Integration
            {
                Name = "MemoryPack",
                PackageName = "com.cysharp.memorypack",
                Define = "SAVESYSTEM_MEMORYPACK",
                Description = "Зарезервировано для будущего бинарного сериализатора."
            },
            new Integration
            {
                Name = "VContainer",
                PackageName = "jp.hadashikick.vcontainer",
                Define = "SAVESYSTEM_VCONTAINER",
                Description = "Регистрация ISaveService и его зависимостей в DI-контейнере."
            }
        };

        private NamedBuildTarget _target;
        private HashSet<string> _defines = new HashSet<string>(StringComparer.Ordinal);
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Save System/Define Manager")]
        private static void Open()
        {
            var window = GetWindow<SaveSystemDefineManagerWindow>("SaveSystem Defines");
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        private void OnEnable() => RefreshDefines();

        private void OnGUI()
        {
            DrawHeader();
            DrawTarget();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (Integration integration in Integrations)
                DrawIntegration(integration);
            EditorGUILayout.EndScrollView();

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("SaveSystem integrations", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Включает код опциональных интеграций через defines для выбранной платформы.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawTarget()
        {
            NamedBuildTarget currentTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!currentTarget.Equals(_target))
            {
                _target = currentTarget;
                RefreshDefines();
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Build target", GUILayout.Width(90));
                EditorGUILayout.LabelField(_target.TargetName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.Width(75)))
                    RefreshDefines();
            }

            EditorGUILayout.Space(6);
        }

        private void DrawIntegration(Integration integration)
        {
            bool packageInstalled = PackageManagerInfo.FindForPackageName(integration.PackageName) != null;
            bool enabled = _defines.Contains(integration.Define);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(integration.Name, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    GUIContent status = new GUIContent(packageInstalled ? "Package installed" : "Package missing");
                    Color previousColor = GUI.color;
                    GUI.color = packageInstalled ? new Color(0.65f, 0.95f, 0.68f) : new Color(1f, 0.72f, 0.55f);
                    GUILayout.Label(status, EditorStyles.miniButton, GUILayout.Width(112));
                    GUI.color = previousColor;
                }

                EditorGUILayout.LabelField(integration.Description, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(4);

                using (new EditorGUI.DisabledScope(!packageInstalled))
                {
                    bool nextEnabled = EditorGUILayout.ToggleLeft(
                        new GUIContent("Enable integration", integration.Define), enabled);
                    if (nextEnabled != enabled)
                        SetEnabled(integration.Define, nextEnabled);
                }

                if (!packageInstalled)
                    EditorGUILayout.LabelField("Install the package before enabling this integration.", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(4);
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Enable available"))
                    SetAllAvailable(true);
                if (GUILayout.Button("Disable all"))
                    SetAllAvailable(false);
            }

            EditorGUILayout.Space(8);
        }

        private void RefreshDefines()
        {
            _target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string values = PlayerSettings.GetScriptingDefineSymbols(_target);
            _defines = new HashSet<string>(
                values.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                StringComparer.Ordinal);
            Repaint();
        }

        private void SetAllAvailable(bool enabled)
        {
            foreach (Integration integration in Integrations)
            {
                if (PackageManagerInfo.FindForPackageName(integration.PackageName) != null)
                    SetEnabled(integration.Define, enabled, false);
            }

            ApplyDefines();
        }

        private void SetEnabled(string define, bool enabled, bool apply = true)
        {
            if (enabled)
                _defines.Add(define);
            else
                _defines.Remove(define);

            if (apply)
                ApplyDefines();
        }

        private void ApplyDefines()
        {
            PlayerSettings.SetScriptingDefineSymbols(_target, string.Join(";", _defines.OrderBy(value => value, StringComparer.Ordinal)));
            Repaint();
        }
    }
}
