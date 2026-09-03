using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace PopupSystem.Core
{
    public interface IPopupService
    {
        event Action PopupOpened;
        event Action PopupClosed;

        bool HasOpenPopups { get; }

        UniTask<PopupResult> ShowAsync<TData>(
            PopupType type, TData data, bool blockScreen = false, CancellationToken ct = default);

        UniTask PrewarmAsync(PopupType type, int count = 1, CancellationToken ct = default);

        void Release(PopupType type);
    }
}
