using System;
using SaveSystem.SaveSystem.Core;

// SaveService автоматически использует этот ID и версию в заголовке файла.
namespace SaveSystemExample
{
    [SaveType("save-example", version: 1)]
    [Serializable]
    public class SaveExample
    {
        public string PlayerName = "Player";
        public int Level = 1;
        public int Coins;
        public bool IntroFinished;
    }

// Атрибут необязателен: в этом случае SaveService использует Type.FullName и версию 1.
    [Serializable]
    public class SaveExampleWithoutAttribute
    {
        public int HighScore;
    }
}
