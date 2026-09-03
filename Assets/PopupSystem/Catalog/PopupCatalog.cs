using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PopupSystem.Core;
using UnityEngine;

namespace PopupSystem.Catalog
{
    [CreateAssetMenu(fileName = "PopupCatalog", menuName = "Popups/Popup Catalog")]
    public sealed class PopupCatalog : ScriptableObject, IPopupViewProvider
    {
        [SerializeField] private GameObject[] _prefabs = Array.Empty<GameObject>();

        private Dictionary<PopupType, GameObject> _byType;

        public UniTask<GameObject> LoadAsync(PopupType type, CancellationToken ct = default)
        {
            _byType ??= BuildIndex();

            if (!_byType.TryGetValue(type, out var prefab))
                throw new InvalidOperationException($"Popup '{type}' is not registered in {name}.");

            return UniTask.FromResult(prefab);
        }

        public void Unload(PopupType type)
        {
        }

        private Dictionary<PopupType, GameObject> BuildIndex()
        {
            var index = new Dictionary<PopupType, GameObject>(_prefabs.Length);

            foreach (var prefab in _prefabs)
            {
                if (prefab == null)
                    continue;

                if (!prefab.TryGetComponent<IPopupView>(out var view))
                    throw new InvalidOperationException(
                        $"{name}: prefab '{prefab.name}' has no {nameof(IPopupView)} component.");

                index[view.Type] = prefab;
            }

            return index;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _byType = null;

            var seen = new HashSet<PopupType>();

            foreach (var prefab in _prefabs)
            {
                if (prefab == null)
                    continue;

                if (!prefab.TryGetComponent<IPopupView>(out var view))
                {
                    Debug.LogError($"{name}: '{prefab.name}' has no {nameof(IPopupView)}.", this);
                    continue;
                }

                if (!seen.Add(view.Type))
                    Debug.LogError($"{name}: '{view.Type}' is declared by more than one prefab.", this);
            }
        }
#endif
    }
}
