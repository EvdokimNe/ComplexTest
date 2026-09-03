using System.Collections.Generic;
using GameplayStateLogic;
using UnityEngine;

namespace GameplayStateLogicExample
{
    /// <summary>
    /// Показывает три способа выбывания сущности — уничтожена, выключена, завершена — и то, что
    /// потребитель реагирует на изменения, а не опрашивает сцену.
    /// </summary>
    public class GameplayEntityDemo : MonoBehaviour
    {
        // Переиспользуется между запросами, чтобы GetActive ничего не аллоцировал.
        private readonly List<Enemy> _buffer = new List<Enemy>();

        private void OnEnable()
        {
            GameplayEntityRegistry.Registered += OnEntityRegistered;
            GameplayEntityRegistry.Unregistered += OnEntityUnregistered;
        }

        private void OnDisable()
        {
            GameplayEntityRegistry.Registered -= OnEntityRegistered;
            GameplayEntityRegistry.Unregistered -= OnEntityUnregistered;
        }

        [ContextMenu("Log active enemies")]
        private void LogActiveEnemies()
        {
            GameplayEntityRegistry.GetActive(_buffer);
            Debug.Log($"Активных врагов: {_buffer.Count} из {GameplayEntityRegistry.ActiveCount} сущностей");
        }

        [ContextMenu("Destroy first enemy")]
        private void DestroyFirst()
        {
            if (TryGetFirst(out Enemy enemy))
                Destroy(enemy.gameObject);
        }

        [ContextMenu("Disable first enemy")]
        private void DisableFirst()
        {
            if (TryGetFirst(out Enemy enemy))
                enemy.gameObject.SetActive(false);
        }

        [ContextMenu("Complete first enemy")]
        private void CompleteFirst()
        {
            if (TryGetFirst(out Enemy enemy))
                enemy.Complete();
        }

        private bool TryGetFirst(out Enemy enemy)
        {
            GameplayEntityRegistry.GetActive(_buffer);
            enemy = _buffer.Count > 0 ? _buffer[0] : null;

            return enemy != null;
        }

        private static void OnEntityRegistered(GameplayEntity entity) =>
            Debug.Log($"Встала на учёт: {entity.name}");

        private static void OnEntityUnregistered(GameplayEntity entity) =>
            Debug.Log($"Снялась с учёта: {entity.name}");
    }
}
