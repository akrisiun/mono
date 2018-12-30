using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Mono.Entity
{
    public static class DataReaderMap
    {
        public static IEnumerable<T> Map<T>(this DbDataReader dbReader)
            where T : new()
        {
            var projectedType = typeof(T);
            IEnumerable<PropertyInfo> properties = null;

#if NET451 || NET40
            // PropertyInfo[] properties = null;
            properties = projectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#else
            properties = RuntimeReflectionExtensions.GetRuntimeProperties(projectedType);
#endif

            if (!dbReader.HasRows)
                yield return default(T); // return Enumerable.Empty<T>;

            foreach (IDataRecord dbDataRecord in dbReader)
            {
                var entity = ConvertDataRecord<T>(dbDataRecord, properties);
                yield return entity;
            }
        }

        private static T ConvertDataRecord<T>(IDataRecord dbDataRecord, IEnumerable<PropertyInfo> properties)
            where T : new()
        {
            var entity = new T();
            foreach (var propertyInfo in properties)
            {
                object value = null;
                try
                {
                    // Exceptions: System.IndexOutOfRangeException:
                    //     No column with the specified name was found.
                    value = dbDataRecord[propertyInfo.Name];
                }
                catch { }
                if (value == null)
                    continue;

                var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var newValue = value is DBNull || value == null ? null : Convert.ChangeType(value, propertyType);

                propertyInfo.SetValue(entity, newValue, new object[] { });
            }
            return entity;
        }
    }

}
