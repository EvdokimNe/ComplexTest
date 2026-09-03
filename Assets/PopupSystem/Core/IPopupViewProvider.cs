using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PopupSystem.Core
{
    public interface IPopupViewProvider
    {
        UniTask<GameObject> LoadAsync(PopupType type, CancellationToken ct = default);

        void Unload(PopupType type);
    }
}
