using System.Collections.Generic;
using UnityEngine;

namespace GameplayStateLogic.Example
{
    /// <summary>
    /// Рантайм-панель для проверки поведения реестра: три способа выбывания сущности —
    /// уничтожена, выключена, завершена — и возврат выключенной обратно в игру.
    ///
    /// Демо держит собственный список всего созданного. Это не дублирование реестра, а иллюстрация
    /// границы ответственности: реестр знает только активных, а «все существующие» — забота того,
    /// кто сущности создаёт.
    /// </summary>
    public sealed class GameplayEntityDemo : MonoBehaviour
    {
        private const string Log = "[Entities]";

        private readonly List<Enemy> _spawned = new List<Enemy>();
        private readonly List<Enemy> _buffer = new List<Enemy>();
        private readonly List<string> _display = new List<string>();

        private int _counter;
        private int _destroyedCount;
        private string _lastEvent = "—";

        private void OnEnable()
        {
            GameplayEntityRegistry.Registered += OnRegistered;
            GameplayEntityRegistry.Unregistered += OnUnregistered;
        }

        private void OnDisable()
        {
            GameplayEntityRegistry.Registered -= OnRegistered;
            GameplayEntityRegistry.Unregistered -= OnUnregistered;
        }

        private void OnGUI()
        {
            // Число контролов должно совпадать между Layout и Repaint одного кадра, иначе Unity
            // ругается на Mismatched LayoutGroup. Поэтому список строится один раз на Layout.
            if (Event.current.type == EventType.Layout)
                RebuildDisplay();

            using var area = new GUILayout.AreaScope(new Rect(16, 16, 360, 520));

            GUILayout.Label($"в реестре: {GameplayEntityRegistry.ActiveCount}    из них врагов: {_buffer.Count}");
            GUILayout.Label($"создано: {_spawned.Count}    уничтожено: {_destroyedCount}");
            GUILayout.Label($"последнее событие: {_lastEvent}");

            GUILayout.Space(8);

            if (Button("Создать врага"))
                Spawn();

            if (Button("Выключить активного"))
                DisableFirstActive();

            if (Button("Включить выключенного"))
                EnableFirstDisabled();

            if (Button("Завершить активного — Complete()"))
                CompleteFirstActive();

            if (Button("Уничтожить активного"))
                DestroyFirstActive();

            if (Button("Убрать всех"))
                Clear();

            GUILayout.Space(8);
            GUILayout.Label("Созданные сущности:");

            foreach (string line in _display)
                GUILayout.Label($"    {line}");
        }

        private void RebuildDisplay()
        {
            GameplayEntityRegistry.GetActive(_buffer);

            _display.Clear();

            foreach (Enemy enemy in _spawned)
                _display.Add(Describe(enemy));
        }

        private static bool Button(string label) => GUILayout.Button(label, GUILayout.Height(30));

        private void Spawn()
        {
            var host = new GameObject($"Enemy {++_counter}");
            host.transform.SetParent(transform);

            // AddComponent сразу вызывает OnEnable, поэтому сущность встаёт на учёт здесь же.
            _spawned.Add(host.AddComponent<Enemy>());
        }

        private void DisableFirstActive()
        {
            if (TryGetFirstActive(out Enemy enemy))
                enemy.gameObject.SetActive(false);
        }

        /// <remarks>
        /// Завершённая сущность на учёт не вернётся, даже если её включить: <c>OnEnable</c>
        /// проверяет <see cref="GameplayEntity.IsCompleted"/>.
        /// </remarks>
        private void EnableFirstDisabled()
        {
            foreach (Enemy enemy in _spawned)
            {
                if (enemy != null && !enemy.gameObject.activeSelf)
                {
                    enemy.gameObject.SetActive(true);
                    return;
                }
            }
        }

        private void CompleteFirstActive()
        {
            if (TryGetFirstActive(out Enemy enemy))
                enemy.Complete();
        }

        private void DestroyFirstActive()
        {
            if (!TryGetFirstActive(out Enemy enemy))
                return;

            _spawned.Remove(enemy);
            _destroyedCount++;

            Destroy(enemy.gameObject);
        }

        private void Clear()
        {
            foreach (Enemy enemy in _spawned)
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }

            _spawned.Clear();
            _counter = 0;
            _destroyedCount = 0;
        }

        private bool TryGetFirstActive(out Enemy enemy)
        {
            GameplayEntityRegistry.GetActive(_buffer);
            enemy = _buffer.Count > 0 ? _buffer[0] : null;

            return enemy != null;
        }

        private static string Describe(Enemy enemy)
        {
            if (enemy == null)
                return "— уничтожена";

            if (enemy.IsCompleted)
                return $"{enemy.name} — завершена";

            return enemy.gameObject.activeSelf
                ? $"{enemy.name} — активна"
                : $"{enemy.name} — выключена";
        }

        private void OnRegistered(GameplayEntity entity)
        {
            _lastEvent = $"{entity.name} встала на учёт";
            Debug.Log($"{Log} registered: {entity.name}");
        }

        private void OnUnregistered(GameplayEntity entity)
        {
            _lastEvent = $"{entity.name} снялась с учёта";
            Debug.Log($"{Log} unregistered: {entity.name}");
        }
    }
}
