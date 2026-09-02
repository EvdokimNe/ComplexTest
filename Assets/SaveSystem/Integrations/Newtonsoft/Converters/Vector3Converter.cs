#if SAVESYSTEM_NEWTONSOFT
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>
    /// Vector3 как {"x":0,"y":0,"z":0}. Без конвертера Newtonsoft уходит в свойства структуры
    /// (normalized, magnitude, sqrMagnitude) и пишет в сейв мусор на каждое значение.
    /// </summary>
    public sealed class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            JObject json = JObject.Load(reader);

            return new Vector3(
                json.Value<float?>("x") ?? 0f,
                json.Value<float?>("y") ?? 0f,
                json.Value<float?>("z") ?? 0f);
        }
    }
}
#endif
