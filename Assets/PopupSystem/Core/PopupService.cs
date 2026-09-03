using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PopupSystem.View;
using UnityEngine;

namespace PopupSystem.Core
{
    public sealed class PopupService : IPopupService
    {
        private readonly IPopupViewProvider _provider;
        private readonly PopupRoot _root;

        private readonly Dictionary<PopupType, Stack<IPopupView>> _pool = new();
        private readonly Dictionary<PopupType, UniTask<GameObject>> _loading = new();
        private readonly List<OpenPopup> _open = new();

        public event Action PopupOpened;
        public event Action PopupClosed;

        public bool HasOpenPopups => _open.Count > 0;

        public PopupService(IPopupViewProvider provider, PopupRoot root)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _root = root ? root : throw new ArgumentNullException(nameof(root));
        }

        public async UniTask<PopupResult> ShowAsync<TData>(
            PopupType type, TData data, bool blockScreen = false, CancellationToken ct = default)
        {
            var view = await RentAsync(type, ct);

            if (view is not IPopupView<TData> typed)
            {
                Return(type, view);
                throw new InvalidOperationException(
                    $"Popup '{type}' is {view.GetType().Name}, which does not accept {typeof(TData).Name}.");
            }

            var completion = new UniTaskCompletionSource<PopupResult>();

            _open.Add(new OpenPopup(
                view,
                view.GameObject.GetComponent<CanvasGroup>(),
                blockScreen,
                completion));

            view.GameObject.transform.SetAsLastSibling();
            view.GameObject.SetActive(true);
            typed.Bind(data, result => completion.TrySetResult(result));

            RefreshStack();
            PopupOpened?.Invoke();

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                ct, _root.gameObject.GetCancellationTokenOnDestroy());

            PopupResult result;
            try
            {
                result = await completion.Task.AttachExternalCancellation(linked.Token);
            }
            catch (OperationCanceledException)
            {
                result = PopupResult.Dismissed;
            }

            var index = _open.FindIndex(open => open.Completion == completion);
            if (index >= 0)
                _open.RemoveAt(index);

            Return(type, view);
            RefreshStack();
            PopupClosed?.Invoke();

            return result;
        }

        public async UniTask PrewarmAsync(PopupType type, int count = 1, CancellationToken ct = default)
        {
            var prefab = await LoadAsync(type, ct);
            var pooled = GetPool(type);

            for (var i = pooled.Count; i < count; i++)
                pooled.Push(Instantiate(prefab, type));
        }

        public void Release(PopupType type)
        {
            if (_pool.Remove(type, out var pooled))
            {
                foreach (var view in pooled)
                    UnityEngine.Object.Destroy(view.GameObject);
            }

            _loading.Remove(type);
            _provider.Unload(type);
        }

        private async UniTask<IPopupView> RentAsync(PopupType type, CancellationToken ct)
        {
            var pooled = GetPool(type);
            if (pooled.Count > 0)
                return pooled.Pop();

            var prefab = await LoadAsync(type, ct);
            return Instantiate(prefab, type);
        }

        private void Return(PopupType type, IPopupView view)
        {
            view.GameObject.SetActive(false);
            GetPool(type).Push(view);
        }

        private UniTask<GameObject> LoadAsync(PopupType type, CancellationToken ct)
        {
            if (_loading.TryGetValue(type, out var pending))
                return pending;

            var load = _provider.LoadAsync(type, ct).Preserve();
            _loading[type] = load;
            return load;
        }

        private IPopupView Instantiate(GameObject prefab, PopupType type)
        {
            var instance = UnityEngine.Object.Instantiate(prefab, _root.Container, false);
            instance.SetActive(false);

            if (!instance.TryGetComponent<IPopupView>(out var view))
                throw new InvalidOperationException(
                    $"Prefab for '{type}' has no {nameof(IPopupView)} component.");

            if (!instance.TryGetComponent<CanvasGroup>(out _))
                instance.AddComponent<CanvasGroup>();

            return view;
        }

        private void RefreshStack()
        {
            var blockScreen = false;

            for (var i = 0; i < _open.Count; i++)
            {
                _open[i].Group.interactable = i == _open.Count - 1;
                blockScreen |= _open[i].BlockScreen;
            }

            _root.SetBlockerActive(blockScreen);
        }

        private Stack<IPopupView> GetPool(PopupType type)
        {
            if (!_pool.TryGetValue(type, out var pooled))
                _pool[type] = pooled = new Stack<IPopupView>();

            return pooled;
        }

        private readonly struct OpenPopup
        {
            public readonly IPopupView View;
            public readonly CanvasGroup Group;
            public readonly bool BlockScreen;
            public readonly UniTaskCompletionSource<PopupResult> Completion;

            public OpenPopup(
                IPopupView view,
                CanvasGroup group,
                bool blockScreen,
                UniTaskCompletionSource<PopupResult> completion)
            {
                View = view;
                Group = group;
                BlockScreen = blockScreen;
                Completion = completion;
            }
        }
    }
}
