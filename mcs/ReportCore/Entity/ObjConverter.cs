using Mono.Reflection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;

namespace Mono.Entity
{
#if XLSX && !WEB
    using Mono.Internal;
#endif

    public static class ObjConverter
    {
        public static T Clone<T>(this object[] objVal)
        {
            T valArray = (T)objVal.Clone();

            return valArray;
        }

        public static ExpandoObject ToExpando(this object[] objVal, DbDataReader reader, DbDataMapHelper<ExpandoObject> helper = null)
        {
            helper = helper ?? new DbDataMapHelper<ExpandoObject>();
            helper.GetProperties(reader);
            Tuple<int[], PropertyInfo[]> map = helper.GetMap();
            return ToExpando(objVal, map.Item1, map.Item2);
        }

        public static ExpandoObject ToExpando(this object[] objVal, Dictionary<string, SqlFieldInfo> fieldsDict)
        {
            IDictionary<string, object> res = new ExpandoObject();

            int i = -1;
            foreach (string keyName in fieldsDict.Keys)
            {
                i++;
                if (!string.IsNullOrWhiteSpace(keyName))
                {
                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                        res.Add(keyName, objVal[i]);
                    else
                        res.Add(keyName, null);
                }
            }
            return res as ExpandoObject;
        }

        public static ExpandoObject ToExpando(this object[] objVal, int[] map, PropertyInfo[] properties)
        {
            IDictionary<string, object> res = new ExpandoObject();
            for (int i = 0; i < map.Length; i++)
                if (i == 0 || map[i] > 0)
                {
                    PropertyInfo info = properties[map[i]];
                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                        res.Add(info.Name, objVal[i]);
                }

            return res as ExpandoObject;
        }

        public static ExpandoObject ToExpando(this object[] objVal, SqlField[] fields)
        {
            IDictionary<string, object> res = new ExpandoObject();
            for (int i = 0; i < fields.Length; i++)
                if (i == 0 || !string.IsNullOrWhiteSpace(fields[i].Name))
                {
                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                        res.Add(fields[i].Name, objVal[i]);
                    else
                        res.Add(fields[i].Name, null);
                }

            return res as ExpandoObject;
        }

        public static ExpandoObject ToExpando(this object obj, SqlField[] fields)
        {
            IDictionary<string, object> res = null;
            if (obj is Array && (obj as object[]) != null)
                res = ToExpando(obj as object[], fields);
            else if (obj is ExpandoObject)
            {
                res = new ExpandoObject();
                var resObj = obj as IDictionary<string, object>;
                var num = resObj.GetEnumerator();

                for (int i = 0; i < fields.Length; i++)
                {
                    var name = fields[i].Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = string.Format("__{0}", i);

                    object objVal = null;
                    if (num.MoveNext())
                        objVal = num.Current.Value;

                    if (objVal != null && !objVal.Equals(DBNull.Value))
                        res.Add(name, objVal);
                    else
                        res.Add(name, null);
                }
            }

            if (res == null)
            {
                res = new ExpandoObject();
                for (int i = 0; i < fields.Length; i++)
                    if (i == 0 || !string.IsNullOrWhiteSpace(fields[i].Name))
                    {
                        var objVal = obj.GetPropertyValue(fields[i].Name);
                        if (objVal != null && !objVal.Equals(DBNull.Value))
                            res.Add(fields[i].Name, objVal);
                        else
                            res.Add(fields[i].Name, null);
                    }
            }

            return res as ExpandoObject;
        }

        public static T SingleString<T>(this object objVal, string property = "xml")
        {
            T val = Activator.CreateInstance<T>();
            val.SetValue<string>(property, objVal.ToStringIfNull(String.Empty));
            return val;
        }

        public static T ToType<T>(this object[] objVal, int[] map, PropertyInfo[] properties)
        {
            T val = Activator.CreateInstance<T>();
            objVal.FillValues<T>(val, map, properties);
            return val;
        }

        public static void FillValues<T>(this object[] objVal, T val, int[] map, PropertyInfo[] properties)
        {
            if (properties.Length == 0)
            {
                var valOne = objVal[0];
                if (val is ExpandoObject) // typeof(T) == typeof(ExpandoObject))
                    val.SetValue<string>("xml", valOne.ToStringIfNull(String.Empty));
                return;
            }

            for (int i = 0; i < map.Length; i++)
                if (i == 0 || map[i] > 0)
                {
                    PropertyInfo info = properties[map[i]];
                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                        info.SetValue(val, objVal[i], null);
                }
        }

        public static T ToType<T>(this ExpandoObject obj) where T : class, new()
        {
            T val = Activator.CreateInstance<T>();
            ExpandoConvert.CloneToObj(obj, val);  //  CloneTo<T>(obj, ref val);
            return val;
        }
    }
}