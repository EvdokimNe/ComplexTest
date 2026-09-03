namespace PopupSystem.Core
{
    public readonly struct PopupResult
    {
        private readonly object _payload;

        public readonly int Index;

        public readonly bool WasDismissed;

        public PopupResult(int index, object payload = null)
        {
            Index = index;
            WasDismissed = false;
            _payload = payload;
        }

        private PopupResult(bool dismissed)
        {
            Index = -1;
            WasDismissed = dismissed;
            _payload = null;
        }

        public static PopupResult Dismissed => new PopupResult(dismissed: true);

        public T As<T>() => _payload is T typed ? typed : default;
    }
}
