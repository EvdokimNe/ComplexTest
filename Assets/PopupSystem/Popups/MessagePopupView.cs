using System;
using PopupSystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PopupSystem.Popups
{
    public sealed class MessagePopupView : MonoBehaviour, IPopupView<MessagePopupData>
    {
        [SerializeField] private PopupType _type;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _body;
        [SerializeField] private PopupButtonView[] _buttons;

        public PopupType Type => _type;

        public GameObject GameObject => gameObject;

        public void Bind(MessagePopupData data, Action<PopupResult> complete)
        {
            _title.text = data.Title;
            _body.text = data.Body;

            var count = Mathf.Min(data.Buttons.Count, _buttons.Length);

            for (var i = 0; i < _buttons.Length; i++)
            {
                var active = i < count;
                _buttons[i].gameObject.SetActive(active);

                if (!active)
                    continue;

                var index = i;
                var button = data.Buttons[i];

                _buttons[i].Bind(button.Label, () =>
                {
                    button.OnClick?.Invoke();
                    complete(new PopupResult(index, button.Payload));
                });
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_buttons != null && _buttons.Length != MessagePopupData.MaxButtons)
                Debug.LogError(
                    $"{name}: expected {MessagePopupData.MaxButtons} buttons, found {_buttons.Length}.",
                    this);
        }
#endif
    }
}
