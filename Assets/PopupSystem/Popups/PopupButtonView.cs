using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PopupSystem.Popups
{
    [RequireComponent(typeof(Button))]
    public sealed class PopupButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;

        private Action _onClick;

        private void Awake() => _button.onClick.AddListener(() => _onClick?.Invoke());

        public void Bind(string label, Action onClick)
        {
            _label.text = label;
            _onClick = onClick;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_label == null)
                _label = GetComponentInChildren<TextMeshProUGUI>(true);
        }
#endif
    }
}
