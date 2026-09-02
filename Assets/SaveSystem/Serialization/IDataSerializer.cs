using System;
namespace SaveSystem.SaveSystem.Serialization
{
    /// <summary>
    /// Превращает объект в байты и обратно. Единственный шов, который меняется при переходе
    /// на Newtonsoft, MessagePack или собственный бинарный формат — остальной модуль не трогается.
    /// Свои настройки каждая реализация держит при себе (см. JsonProfileConfig у Newtonsoft).
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Идентификатор формата, попадает в заголовок файла. Загрузка чужим сериализатором
        /// отсекается по нему до разбора данных.
        /// </summary>
        string FormatId { get; }

        byte[] Serialize(object data, Type type);

        object Deserialize(byte[] payload, Type type);
    }
}
