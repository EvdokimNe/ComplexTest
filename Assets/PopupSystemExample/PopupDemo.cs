using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using PopupSystem.Catalog;
using PopupSystem.Core;
using PopupSystem.Popups;
using PopupSystem.View;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PopupSystem.Example
{
    public sealed class PopupDemo : MonoBehaviour
    {
        private const string Log = "[Popups]";

        [SerializeField] private PopupRoot _root;
        [SerializeField] private PopupCatalog _catalog;

        private IPopupService _popups;
        private CancellationTokenSource _cancellable;
        private string _lastResult = "—";

        private enum Branch
        {
            Lie,
            Truth,
            Silence,
            Threaten,
            Leave,
        }

        private void Awake() => _popups = new PopupService(_catalog, _root);

        private void Start() => PrewarmAsync().Forget();

        private void OnDestroy() => _cancellable?.Dispose();

        private async UniTaskVoid PrewarmAsync()
        {
            var watch = Stopwatch.StartNew();
            await _popups.PrewarmAsync(PopupType.Default);
            Debug.Log($"{Log} prewarmed {PopupType.Default} in {watch.ElapsedMilliseconds} ms");
            Debug.Log($"{Log} {PopupType.Default2} left cold, it will load on demand");
        }

        private void OnGUI()
        {
            using var area = new GUILayout.AreaScope(new Rect(16, 16, 340, 420));

            GUILayout.Label($"open: {_popups.HasOpenPopups}    last: {_lastResult}");

            if (Button("1 кнопка — минимум, без блокера"))
                SingleButtonAsync().Forget();

            if (Button("2 кнопки — колбэки"))
                ConfirmAsync().Forget();

            if (Button("5 кнопок — вертикальный, payload"))
                StoryChoiceAsync().Forget();

            if (Button("Те же данные → оба префаба"))
                SameDataBothLayoutsAsync().Forget();

            if (Button("Показать и отменить через 2 с"))
                CancelledAsync().Forget();

            if (Button("6-я кнопка → исключение"))
                TooManyButtons();
        }

        private static bool Button(string label) => GUILayout.Button(label, GUILayout.Height(34));

        private async UniTaskVoid SingleButtonAsync()
        {
            var data = new MessagePopupData()
                .Title("Сохранено")
                .Body("Прогресс записан в слот 1.")
                .Button("Понятно");

            Report("single", await _popups.ShowAsync(PopupType.Default, data));
        }

        private async UniTaskVoid ConfirmAsync()
        {
            var data = new MessagePopupData()
                .Title("Выйти в меню?")
                .Body("Несохранённый прогресс будет потерян.")
                .Button("Отмена", onClick: () => Debug.Log($"{Log} callback: cancel"))
                .Button("Выйти", onClick: () => Debug.Log($"{Log} callback: exit"));

            Report("confirm", await _popups.ShowAsync(PopupType.Default, data, blockScreen: true));
        }

        private async UniTaskVoid StoryChoiceAsync()
        {
            var data = new MessagePopupData()
                .Title("Стражник ждёт ответа")
                .Body("Он смотрит на тебя уже слишком долго. Молчание тоже будет ответом, " +
                      "и, кажется, не тем, который стоит давать в этом коридоре.")
                .Button("Солгать", payload: Branch.Lie)
                .Button("Сказать правду", payload: Branch.Truth)
                .Button("Промолчать", payload: Branch.Silence)
                .Button("Пригрозить", payload: Branch.Threaten)
                .Button("Уйти", payload: Branch.Leave);

            var result = await _popups.ShowAsync(PopupType.Default2, data, blockScreen: true);

            _lastResult = result.WasDismissed ? "dismissed" : result.As<Branch>().ToString();
            Debug.Log($"{Log} story choice → {_lastResult} (index {result.Index})");
        }

        private async UniTaskVoid SameDataBothLayoutsAsync()
        {
            var data = new MessagePopupData()
                .Title("Одни и те же данные")
                .Body("Этот экземпляр MessagePopupData сейчас будет показан двумя разными префабами.")
                .Button("Дальше")
                .Button("Ага");

            Report("horizontal", await _popups.ShowAsync(PopupType.Default, data, blockScreen: true));
            Report("vertical", await _popups.ShowAsync(PopupType.Default2, data, blockScreen: true));
        }

        private async UniTaskVoid CancelledAsync()
        {
            _cancellable?.Dispose();
            _cancellable = new CancellationTokenSource();
            _cancellable.CancelAfterSlim(TimeSpan.FromSeconds(2));

            var data = new MessagePopupData()
                .Title("Закроется сам")
                .Body("Через две секунды токен отменится и попап закроется без выбора.")
                .Button("Успеть нажать");

            Report("cancelled", await _popups.ShowAsync(
                PopupType.Default, data, blockScreen: true, ct: _cancellable.Token));
        }

        private void TooManyButtons()
        {
            try
            {
                var data = new MessagePopupData();

                for (var i = 0; i <= MessagePopupData.MaxButtons; i++)
                    data.Button($"Кнопка {i}");
            }
            catch (InvalidOperationException exception)
            {
                _lastResult = "rejected";
                Debug.Log($"{Log} validation: {exception.Message}");
            }
        }

        private void Report(string label, PopupResult result)
        {
            _lastResult = result.WasDismissed ? "dismissed" : $"index {result.Index}";
            Debug.Log($"{Log} {label} → {_lastResult}");
        }
    }
}
