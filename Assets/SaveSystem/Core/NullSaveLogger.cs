using System;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Молчаливый логгер — для тестов и для случаев, когда сообщения не нужны.</summary>
    public sealed class NullSaveLogger : ISaveLogger
    {
        public static readonly NullSaveLogger Instance = new NullSaveLogger();

        public void Warning(string message)
        {
        }

        public void Error(string message, Exception exception = null)
        {
        }
    }
}
