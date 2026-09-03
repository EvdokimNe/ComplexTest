using System;
using System.Collections.Generic;

namespace PopupSystem.Popups
{
    public sealed class MessagePopupData
    {
        public const int MaxButtons = 5;

        public string Title;
        public string Body;

        public readonly List<PopupButtonData> Buttons = new(MaxButtons);
    }

    public readonly struct PopupButtonData
    {
        public readonly string Label;
        public readonly Action OnClick;
        public readonly object Payload;

        public PopupButtonData(string label, Action onClick, object payload)
        {
            Label = label;
            OnClick = onClick;
            Payload = payload;
        }
    }

    public static class MessagePopupExtensions
    {
        public static MessagePopupData Title(this MessagePopupData data, string title)
        {
            data.Title = title;
            return data;
        }

        public static MessagePopupData Body(this MessagePopupData data, string body)
        {
            data.Body = body;
            return data;
        }

        public static MessagePopupData Button(
            this MessagePopupData data, string label, Action onClick = null, object payload = null)
        {
            if (data.Buttons.Count >= MessagePopupData.MaxButtons)
                throw new InvalidOperationException(
                    $"A message popup supports up to {MessagePopupData.MaxButtons} buttons.");

            data.Buttons.Add(new PopupButtonData(label, onClick, payload));
            return data;
        }
    }
}
