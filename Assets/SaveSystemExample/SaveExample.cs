using System;
using System.Runtime.Serialization;
using SaveSystem.SaveSystem.Core;

// SaveService автоматически использует этот ID и версию в заголовке файла.
namespace SaveSystemExample
{
    [SaveType("save-example", version: 1)]
    [DataContract]
    [Serializable]
    public class SaveExample
    {
        [DataMember] public string PlayerName = "Player";
        [DataMember] public int Level = 1;
        [DataMember] public int Coins;
        [DataMember] public bool IntroFinished;
    }

// Атрибут SaveType необязателен: в этом случае SaveService использует Type.FullName и версию 1.
    [Serializable]
    [DataContract]
    public class SaveExampleWithoutAttribute
    {
        [DataMember] public int HighScore;
    }
}
