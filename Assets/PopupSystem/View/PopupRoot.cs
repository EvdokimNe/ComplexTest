using UnityEngine;
using UnityEngine.UI;

namespace PopupSystem.View
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    public sealed class PopupRoot : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private Image _blocker;

        public RectTransform Container => _container;

        private void Awake() => _blocker.gameObject.SetActive(false);

        public void SetBlockerActive(bool active) => _blocker.gameObject.SetActive(active);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_container == null)
                Debug.LogError($"{name}: popup container is not assigned.", this);

            if (_blocker != null && !_blocker.raycastTarget)
                Debug.LogError($"{name}: blocker needs raycastTarget enabled.", this);
        }
#endif
    }
}
