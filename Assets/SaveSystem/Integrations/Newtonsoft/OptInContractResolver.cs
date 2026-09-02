#if SAVESYSTEM_NEWTONSOFT
using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>
    /// Opt-in модель хранилищ: в файл попадает только то, что помечено [DataMember] у типа
    /// с [DataContract]. Новое поле не утекает в сохранение само по себе — его добавляют осознанно.
    /// Резолвер добавляет к штатному поведению Newtonsoft запись в приватные сеттеры: типичное
    /// «[DataMember] public int Level { get; private set; }» иначе сериализуется, но не читается.
    /// </summary>
    public sealed class OptInContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.Writable || member.GetCustomAttribute<DataMemberAttribute>() == null)
                return property;

            property.Writable = IsWritable(member);
            return property;
        }

        private static bool IsWritable(MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo property:
                    return property.GetSetMethod(nonPublic: true) != null;
                case FieldInfo field:
                    return !field.IsInitOnly;
                default:
                    return false;
            }
        }
    }
}
#endif
