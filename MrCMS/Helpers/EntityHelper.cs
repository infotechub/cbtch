using System;
using System.Linq;
using MrCMS.Entities;

namespace MrCMS.Helpers
{
    public static class EntityHelper
    {
        public static T ShallowCopy<T>(this T entity) where T : SystemEntity
        {
            Type type = entity.GetType();
            System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> propertyInfos =
                type.GetProperties()
                    .Where(
                        info =>
                        info.CanWrite &&
                        !(info.PropertyType.IsGenericType &&
                          info.PropertyType.GetGenericArguments().Any(arg => arg.IsSubclassOf(typeof (SystemEntity)))) &&
                        !info.PropertyType.IsSubclassOf(typeof (SystemEntity)));

            T shallowCopy = Activator.CreateInstance(type) as T;
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
            {
                propertyInfo.SetValue(shallowCopy, propertyInfo.GetValue(entity, null), null);
            }
            shallowCopy.Id = 0;
            return shallowCopy;
        }
    }
}