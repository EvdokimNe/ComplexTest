using SaveSystem.SaveSystem.Core;
using UnityEngine;

namespace SaveSystemExample
{
    /// <summary>Минимальная демонстрация Save/Load без обязательной UI-сцены.</summary>
    public sealed class SaveExampleController : MonoBehaviour
    {
        private static readonly StorageKey Key = new StorageKey(SaveSlot.Default, new SaveId("example"));
        private static readonly StorageKey UntypedKey = new StorageKey(SaveSlot.Default, new SaveId("example-without-attribute"));

        [SerializeField] private SaveExample data = new SaveExample();
        [SerializeField] private SaveExampleWithoutAttribute dataWithoutAttribute = new SaveExampleWithoutAttribute();
        [SerializeField] private SaveSystemConfiguration configuration;
        private ISaveService _service;
        private string _message = "Ready";
        private Vector2 _scrollPosition;

        private void Awake() => _service = configuration != null
            ? SaveSystemFactory.Create(configuration)
            : SaveSystemFactory.CreateDefault();

        private void OnGUI()
        {
            float height = Mathf.Min(500f, Screen.height - 40f);
            GUILayout.BeginArea(new Rect(20, 20, 420, height), GUI.skin.box);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Label("SaveSystem example");
            GUILayout.Space(4);
            GUILayout.Label("Save with [SaveType]");
            data.PlayerName = GUILayout.TextField(data.PlayerName);
            int.TryParse(GUILayout.TextField(data.Level.ToString()), out data.Level);
            int.TryParse(GUILayout.TextField(data.Coins.ToString()), out data.Coins);
            data.IntroFinished = GUILayout.Toggle(data.IntroFinished, "Intro finished");

            if (GUILayout.Button("Save"))
                _message = _service.Save(Key, data).IsSuccess ? "Saved" : "Save failed";
            if (GUILayout.Button("Load"))
            {
                LoadResult<SaveExample> result = _service.Load<SaveExample>(Key);
                if (result.Data != null)
                    data = result.Data;
                _message = result.Status.ToString();
            }
            if (GUILayout.Button("Delete"))
            {
                _service.Delete(Key);
                _message = "Deleted";
            }

            GUILayout.Space(8);
            GUILayout.Label("Save without [SaveType]");
            int.TryParse(GUILayout.TextField(dataWithoutAttribute.HighScore.ToString()), out dataWithoutAttribute.HighScore);
            if (GUILayout.Button("Save without [SaveType]"))
                _message = _service.Save(UntypedKey, dataWithoutAttribute).IsSuccess ? "Saved without attribute" : "Save failed";
            if (GUILayout.Button("Load without [SaveType]"))
            {
                LoadResult<SaveExampleWithoutAttribute> result = _service.Load<SaveExampleWithoutAttribute>(UntypedKey);
                if (result.Data != null)
                    dataWithoutAttribute = result.Data;
                _message = "Without attribute: " + result.Status;
            }
            GUILayout.Label(_message);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
