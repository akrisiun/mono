using Mono.Reflection;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Mono.Entity
{
    using System.ComponentModel;

    public class NameProperties
    {
        public static List<SqlParameter> Parse(object namedParam, int skipFrom = 0)
        {
            var properties = new NameProperties(namedParam);
            var listParam = new List<SqlParameter>();

#if NET451|| NET40 || WPF
            if (properties.List.Count == 0)
                return null;


            foreach (string itemName in properties.Names(skipFrom))
            {
                // Mono.Reflection.Utils
                var val = Utils.GetPropertyValue(namedParam, itemName);
                if (val != null)
                    listParam.Add(SqlProc.AddWithValue("@" + itemName, val));
            }
#endif
            return listParam;
        }

        public NameProperties(object paramNamed)
        {
            // System.ComponentModel
            list = TypeDescriptor.GetProperties(paramNamed);
//#if NET451 || NET40 || WPF
//               list = TypeDescriptor.GetProperties(paramNamed);
//#endif
        }

        PropertyDescriptorCollection list;
        public PropertyDescriptorCollection List { get { return list; } }

// #if NET451 || NET40 || WPF

        public IEnumerable<string> Names(int iFrom = 1)
        {
            foreach (PropertyDescriptor item in list)
            {
                if (list.IndexOf(item) >= iFrom)
                    yield return item.Name;
            }
        }

// #endif

        public string FirstName()
        {
            if (list == null || list.Count == 0) return null;

            return list[0].Name;
        }

        public object GetValue(object paramObj, string propertyName)
        {
            return Utils.GetPropertyValue(paramObj, propertyName);
        }

    }

}
