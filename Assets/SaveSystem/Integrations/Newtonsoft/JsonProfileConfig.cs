#if SAVESYSTEM_NEWTONSOFT
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>
    /// Настройки Newtonsoft для сохранений. Лежат рядом со своим сериализатором: базовый модуль
    /// про них не знает, и каждый новый сериализатор приносит свой конфиг тем же способом.
    /// </summary>
    public sealed class JsonProfileConfig
    {
        public JsonSerializerSettings Settings { get; }

        public JsonProfileConfig(IEnumerable<JsonConverter> converters = null)
        {
            Settings = CreateSettings(converters);
        }

        private static JsonSerializerSettings CreateSettings(IEnumerable<JsonConverter> converters)
        {
            var settings = new JsonSerializerSettings
            {
                // В редакторе и dev-билде файл читают глазами, в релизе экономят место.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Formatting = Formatting.Indented,
#else
                Formatting = Formatting.None,
#endif

                // Циклическая ссылка в данных сохранения — всегда ошибка модели, а не повод
                // молча обрезать граф.
                ReferenceLoopHandling = ReferenceLoopHandling.Error,

                // Значения по умолчанию в файл не пишутся: сейв меньше и читаемее.
                // Обратная сторона — «поля нет» и «поле равно default» неразличимы,
                // это важно помнить при написании миграций.
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,

                // Полиморфизм нужен (список предметов, состояния квестов), но имена типов
                // в файл пишет биндер — assembly-qualified имена ломаются при рефакторинге.
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new SaveTypeSerializationBinder(),

                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new OptInContractResolver()
            };

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new Vector2Converter());
            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new QuaternionConverter());
            settings.Converters.Add(new ColorConverter());

            if (converters != null)
            {
                foreach (JsonConverter converter in converters)
                    settings.Converters.Add(converter);
            }

            return settings;
        }
    }
}
#endif
