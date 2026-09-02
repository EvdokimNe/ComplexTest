#if SAVESYSTEM_NEWTONSOFT
using System;
using System.Text;
using Newtonsoft.Json;
using SaveSystem.SaveSystem.Serialization;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>
    /// Сериализатор на Newtonsoft. Берётся вместо JsonUtility, когда нужны словари, свойства,
    /// приватные поля, полиморфизм или собственные конвертеры.
    /// FormatId отличается от движкового json намеренно: форматы несовместимы, и старый файл
    /// должен отсекаться понятным <see cref="LoadStatus.FormatMismatch"/>, а не мусором в данных.
    /// </summary>
    public sealed class NewtonsoftJsonSerializer : IDataSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftJsonSerializer(JsonProfileConfig config = null)
        {
            _settings = (config ?? new JsonProfileConfig()).Settings;
        }

        public string FormatId => "json.net";

        public byte[] Serialize(object data, Type type)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, type, _settings));
        }

        public object Deserialize(byte[] payload, Type type)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(payload), type, _settings);
        }
    }
}
#endif
