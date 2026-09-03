using UnityEngine;

namespace GameplayStateLogic
{
    /// <summary>
    /// Базовый класс геймплейных сущностей: враги, интерактивные объекты, сюжетные актёры.
    /// Сущность сама встаёт на учёт, пока участвует в геймплее, и снимается с него, как только
    /// выключена, уничтожена или завершена. Поэтому реестр физически не может содержать мёртвую
    /// или неактивную ссылку, а потребителю не нужны ни фильтрация, ни проверки на null.
    /// </summary>
    public abstract class GameplayEntity : MonoBehaviour
    {
        /// <summary>
        /// Сущность отыграла свою роль. При этом она может оставаться на сцене и быть видимой —
        /// поэтому состояние хранится отдельно от активности <see cref="GameObject"/>.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Выводит сущность из геймплейной логики, не выключая объект.
        /// </summary>
        public void Complete()
        {
            if (IsCompleted)
                return;

            IsCompleted = true;
            GameplayEntityRegistry.Unregister(this);
        }

        protected virtual void OnEnable()
        {
            if (!IsCompleted)
                GameplayEntityRegistry.Register(this);
        }

        /// <remarks>
        ///Один колбэк закрывает сразу и «выключена», и «уничтожена».
        /// </remarks>
        protected virtual void OnDisable()
        {
            GameplayEntityRegistry.Unregister(this);
        }
    }
}
