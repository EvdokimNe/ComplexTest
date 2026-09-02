using System;
using System.Collections.Generic;
namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Разбирает <see cref="SaveTypeAttribute"/> с кешированием: рефлексия отрабатывает один раз
    /// на тип, дальше сохранение и загрузка идут по словарю.
    /// </summary>
    public static class SaveTypeInfo
    {
        private static readonly Dictionary<Type, SaveTypeDescriptor> Cache = new Dictionary<Type, SaveTypeDescriptor>();

        public static SaveTypeDescriptor Of(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            lock (Cache)
            {
                if (Cache.TryGetValue(type, out SaveTypeDescriptor cached))
                    return cached;

                var attribute = (SaveTypeAttribute)Attribute.GetCustomAttribute(type, typeof(SaveTypeAttribute), false);
                SaveTypeDescriptor descriptor = attribute != null
                    ? new SaveTypeDescriptor(attribute.Id, attribute.Version)
                    : new SaveTypeDescriptor(type.FullName, 1);

                Cache[type] = descriptor;
                return descriptor;
            }
        }
    }
}
