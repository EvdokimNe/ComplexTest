using System;
using SaveSystem.SaveSystem.Core;

namespace SaveSystem.Tests
{
    /// <summary>Данные для тестов: версия схемы 2, чтобы было чему не совпасть с файлом.</summary>
    [Serializable]
    [SaveType("test-progress", version: 2)]
    public class TestProgress
    {
        public int Level;
        public string PlayerName;
    }
}
