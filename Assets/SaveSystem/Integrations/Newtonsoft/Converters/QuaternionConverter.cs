#if SAVESYSTEM_NEWTONSOFT
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>Quaternion как {"x":0,"y":0,"z":0,"w":1}.</summary>
    public sealed class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return Quaternion.identity;

            JObject json = JObject.Load(reader);

            return new Quaternion(
                json.Value<float?>("x") ?? 0f,
                json.Value<float?>("y") ?? 0f,
                json.Value<float?>("z") ?? 0f,
                json.Value<float?>("w") ?? 1f);
        }
    }
}
#endif
