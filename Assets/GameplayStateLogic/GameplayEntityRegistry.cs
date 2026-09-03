using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayStateLogic
{
    /// <summary>
    /// Ведёт учёт геймплейных сущностей, активных прямо сейчас.
    ///
    /// Инвариант намеренно жёсткий: сущность лежит здесь, только пока включена, жива и не
    /// завершена. Всё остальное следует из него — запросу не нужны ни фильтрация, ни проверки на
    /// null, ни периодическая чистка накопившихся мёртвых ссылок.
    /// </summary>
    public static class GameplayEntityRegistry
    {
        private static readonly List<GameplayEntity> ActiveEntities = new List<GameplayEntity>();

        /// <summary>Сущность стала активной.</summary>
        public static event Action<GameplayEntity> Registered;

        /// <summary>Сущность перестала быть активной — по любой причине.</summary>
        public static event Action<GameplayEntity> Unregistered;

        /// <summary>
        /// Активные сущности. Только для чтения, чтобы снаружи нельзя было испортить реестр.
        /// Если потребитель меняет состояние сущностей во время обхода — сначала копия,
        /// см. <see cref="GetActive{T}"/>.
        /// </summary>
        public static IReadOnlyList<GameplayEntity> Active => ActiveEntities;

        public static int ActiveCount => ActiveEntities.Count;

        public static void Register(GameplayEntity entity)
        {
            // Contains — O(n). Unity и так парно вызывает OnEnable/OnDisable, так что проверка
            // защищает лишь от ручной повторной регистрации; на масштабе сцены цена незаметна.
            if (entity == null || ActiveEntities.Contains(entity))
                return;

            ActiveEntities.Add(entity);
            Registered?.Invoke(entity);
        }

        public static void Unregister(GameplayEntity entity)
        {
            if (entity == null || !ActiveEntities.Remove(entity))
                return;

            Unregistered?.Invoke(entity);
        }

        /// <summary>
        /// Заполняет <paramref name="buffer"/> активными сущностями нужного типа.
        /// Буфер передаётся снаружи: повторные запросы не аллоцируют, а работа по копии позволяет
        /// безопасно выключать и уничтожать сущности прямо во время обхода результата.
        /// </summary>
        public static void GetActive<T>(List<T> buffer) where T : GameplayEntity
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.Clear();

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                if (ActiveEntities[i] is T typed)
                    buffer.Add(typed);
            }
        }

        /// <remarks>
        /// При отключённом Domain Reload («Enter Play Mode Options») статика переживает выход из
        /// плеймода, и следующая сессия стартовала бы с сущностями предыдущей.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            ActiveEntities.Clear();
            Registered = null;
            Unregistered = null;
        }
    }
}
