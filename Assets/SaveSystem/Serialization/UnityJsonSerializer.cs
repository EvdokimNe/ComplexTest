using System;
using System.Text;
using UnityEngine;
namespace SaveSystem.SaveSystem.Serialization
{
    /// <summary>
    /// Сериализатор по умолчанию — JsonUtility, без внешних зависимостей.
    /// Ограничения движкового json стоит знать заранее: только поля (не свойства), тип и его поля
    /// должны быть [Serializable], нет словарей, полиморфизма и null-строк.
    /// Как только этого мало — подключается Newtonsoft из Integrations, интерфейс тот же.
    /// </summary>
    public sealed class UnityJsonSerializer : IDataSerializer
    {
        private readonly bool _prettyPrint;

        /// <param name="prettyPrint">
        /// Форматировать с отступами. В редакторе и dev-билде удобно читать глазами,
        /// в релизе лишний вес файла.
        /// </param>
        public UnityJsonSerializer(bool prettyPrint = false)
        {
            _prettyPrint = prettyPrint;
        }

        public string FormatId => "json";

        public byte[] Serialize(object data, Type type)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, _prettyPrint));
        }

        public object Deserialize(byte[] payload, Type type)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            return JsonUtility.FromJson(Encoding.UTF8.GetString(payload), type);
        }
    }
}
