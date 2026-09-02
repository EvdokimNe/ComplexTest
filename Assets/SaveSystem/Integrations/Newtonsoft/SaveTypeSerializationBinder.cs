#if SAVESYSTEM_NEWTONSOFT
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using SaveSystem.SaveSystem.Core;

namespace SaveSystem.SaveSystem.Json
{
    /// <summary>
    /// Пишет в файл идентификатор из [SaveType] вместо assembly-qualified имени типа.
    /// Без этого TypeNameHandling.Auto превращает переименование класса или переезд в другую
    /// сборку в потерю сохранений, а сам файл — в инструкцию «создай мне вот этот тип»,
    /// что при чтении чужого сейва небезопасно.
    /// Типы без [SaveType] разрешаются штатным механизмом Newtonsoft.
    /// </summary>
    public sealed class SaveTypeSerializationBinder : ISerializationBinder
    {
        private readonly object _lock = new object();
        private Dictionary<string, Type> _typesById;

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            var attribute = serializedType.GetCustomAttribute<SaveTypeAttribute>(inherit: false);

            if (attribute != null)
            {
                assemblyName = null;
                typeName = attribute.Id;
                return;
            }

            assemblyName = serializedType.Assembly.GetName().Name;
            typeName = serializedType.FullName;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyName) && TryResolveById(typeName, out Type type))
                return type;

            string qualified = string.IsNullOrEmpty(assemblyName) ? typeName : typeName + ", " + assemblyName;
            return Type.GetType(qualified, throwOnError: true);
        }

        private bool TryResolveById(string id, out Type type)
        {
            lock (_lock)
            {
                _typesById ??= BuildIndex();
                return _typesById.TryGetValue(id, out type);
            }
        }

        /// <summary>
        /// Индекс строится один раз при первой полиморфной загрузке: проход по сборкам дорогой,
        /// но случается не чаще одного раза за сессию.
        /// </summary>
        private static Dictionary<string, Type> BuildIndex()
        {
            var index = new Dictionary<string, Type>(StringComparer.Ordinal);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (Type type in types)
                {
                    if (type == null)
                        continue;

                    var attribute = type.GetCustomAttribute<SaveTypeAttribute>(inherit: false);

                    if (attribute != null)
                        index[attribute.Id] = type;
                }
            }

            return index;
        }
    }
}
#endif
