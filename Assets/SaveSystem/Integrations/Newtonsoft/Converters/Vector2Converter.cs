#if SAVESYSTEM_NEWTONSOFT
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>Vector2 как {"x":0,"y":0}.</summary>
    public sealed class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            JObject json = JObject.Load(reader);

            return new Vector2(
                json.Value<float?>("x") ?? 0f,
                json.Value<float?>("y") ?? 0f);
        }
    }
}
#endif
