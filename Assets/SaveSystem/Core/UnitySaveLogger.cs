using System;
using UnityEngine;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Логгер по умолчанию — в консоль Unity с общим префиксом.</summary>
    public sealed class UnitySaveLogger : ISaveLogger
    {
        private const string Prefix = "[SaveSystem] ";

        public void Warning(string message) => Debug.LogWarning(Prefix + message);

        public void Error(string message, Exception exception = null)
        {
            Debug.LogError(Prefix + message);

            if (exception != null)
                Debug.LogException(exception);
        }
    }
}
