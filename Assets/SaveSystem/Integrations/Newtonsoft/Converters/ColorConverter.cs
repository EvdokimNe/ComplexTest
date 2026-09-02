#if SAVESYSTEM_NEWTONSOFT
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>Color как {"r":1,"g":1,"b":1,"a":1}. Альфа по умолчанию 1: непрозрачный цвет.</summary>
    public sealed class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(value.r);
            writer.WritePropertyName("g");
            writer.WriteValue(value.g);
            writer.WritePropertyName("b");
            writer.WriteValue(value.b);
            writer.WritePropertyName("a");
            writer.WriteValue(value.a);
            writer.WriteEndObject();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            JObject json = JObject.Load(reader);

            return new Color(
                json.Value<float?>("r") ?? 0f,
                json.Value<float?>("g") ?? 0f,
                json.Value<float?>("b") ?? 0f,
                json.Value<float?>("a") ?? 1f);
        }
    }
}
#endif
