using System;
using UnityEngine;

namespace PopupSystem.Core
{
    public enum PopupType
    {
        Default,
        Default2,
    }

    public interface IPopupView
    {
        PopupType Type { get; }

        GameObject GameObject { get; }
    }

    public interface IPopupView<in TData> : IPopupView
    {
        void Bind(TData data, Action<PopupResult> complete);
    }
}
