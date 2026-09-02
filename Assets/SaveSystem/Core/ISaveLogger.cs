using System;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Куда модуль сообщает о повреждённых и невоспроизводимых сохранениях. Отдельный интерфейс,
    /// чтобы ядро не звало Debug.Log напрямую: в тестах это подмена, в проекте — точка,
    /// откуда такие события уходят в телеметрию.
    /// </summary>
    public interface ISaveLogger
    {
        void Warning(string message);

        void Error(string message, Exception exception = null);
    }
}
