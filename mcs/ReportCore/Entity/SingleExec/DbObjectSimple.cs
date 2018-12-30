using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Diagnostics;
using System.Reflection;
using System.Data.SqlClient;

namespace Mono.Entity
{
    using KeyReaderCmdEnum = System.Tuple<SqlDataReader, SqlCommand, IEnumerator>;

    public class DbObjectSimpleNext : DbObjectSimple, IReaderNextResult, ILastError
    {
        public DbDataReader Reader { get; set; }
        public IDbConnection DbConnection { get; set; }
        public IDbCommand DbCommand { get; set; }
        IDbConnection IReaderNextResult.Connection { get { return this.DbConnection; } }

        // public DataTable GetSchemaTable { get { return Reader.GetSchemaTable(); } }

        public bool ReaderAvailable { [DebuggerStepThrough] get { return Reader != null && DbCheck.IsReaderAvailable(Reader); } }
        public Func<Tuple<DbDataReader, IDbConnection>> GetReader { get { return null; } }

        public bool NextResult()
        {
            return ReaderAvailable && Reader.NextResult();
        }
    }

    public class DbObjectSimple : IDataMapHelper<object[]>, ILastError, IEnumerator<object[]> // , IDbObject
    {
        public DbObjectSimple()
        {
            RecordNumber = -1;
        }

        public Type Type { get { return typeof(object[]); } }
        internal int iLen;
        internal string[] FieldNames;
        public Exception LastError { get; set; }
        DbDataReader IDataMapHelper<object[]>.Reader { get { return null; } }   // no reader 

        public virtual IDataMapHelper<object[]> GetProperties(DbDataReader dataReader, Action<Exception> onError = null)
        {
            iLen = dataReader.IsClosed ? 0 : dataReader.FieldCount;
            FieldNames = new string[iLen];
            for (int i = 0; i < iLen; i++)
                FieldNames[i] = dataReader.GetName(i);
            return this;
        }

        public SqlField[] GetFields(DbDataReader dataReader)
        {
            return SqlFieldArray.GetArray(dataReader);
        }

        public virtual object[] SetValues(DbDataReader reader, object[] objVal) { return DbRecordArray(reader, objVal.Length); }
        public object[] SetValues(object[] objVal) { return objVal; }
        public object[] DbRecordArray() { return new object[] { }; }    // empty
        public object[] DbRecordArray(int len)
        {
            var array = new object[] { };
            Array.Resize(ref array, len);
            return array;
        }    // empty

        public object[] DbRecordArray(DbDataReader reader, int len)
        {
            object[] array = new object[] { };
            if (reader != null)
            {
                Array.Resize(ref array, reader.FieldCount);
                if (!reader.IsClosed && reader.HasRows) // && Reader.RecordsAffected > 0)
                {
                    bool lError = false;
                    try
                    {
                        reader.GetValues(array);
                    }
                    catch (Exception ex) { this.LastError = ex; lError = true; }
                    if (lError)
                        return null;
                }
            }
            if (len > 0 && len != array.Length)
                Array.Resize(ref array, len);
            return array;   // (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public int? GetOrdinal(DbDataReader reader, string columnName) { return reader.GetOrdinal(columnName); }

        public object[] Current { get; set; }
        public int RecordNumber { get; set; }
        public virtual bool MoveNext() { return false; }

        object IEnumerator.Current { get { return Current; } }
        void IEnumerator.Reset() { }
        public virtual void Dispose() { Current = null; }
    }

    public static class DbObjectSimpleStatic
    {
        public static IDataMapHelper<T> GetProperties<T>(DbDataReader dataReader) where T : class
        {
            Type type = typeof(T); // this.Type;
            if (dataReader == null || dataReader.IsClosed)
                return null;

            int iLen = dataReader.FieldCount;
            Mono.Guard.Check(iLen > 0, "DataMapper FieldCount error");

            var helper = new MapHelperSimpleProperties<T>();
            return helper.GetProperties(dataReader);
        }

        public static T SetValues<T>(DbDataReader reader, IDataMapHelper<T> helper, object[] objVal) where T : class
        {
            return helper.SetValues(reader, objVal);
        }

    }

    internal class MapHelperSimpleProperties<T> : IDataMapHelper<T> // , where T : class 
    {
        internal int[] map;
        // internal SqlField[] 
        internal Dictionary<string, SqlFieldInfo> fields;
        internal PropertyInfo[] properties;
        DbDataReader IDataMapHelper<T>.Reader { get { return null; } } // no reader available

        public void Dispose() { map = null; properties = null; fields = null; }

        public T SetValues(DbDataReader dataReader, object[] objVal)
        {
            if (objVal == null || objVal.Length == 0)
                return default(T);

            if (typeof(T).IsArray)
            {
                T valArray = objVal == null || objVal.Length == 0 ? default(T)
                           : (T)objVal.Clone(); //  Activator.CreateInstance(typeof(T), objVal.Length);
                // default(T) : non-static method requires a target
                // object[] array = valArray as object[];
                return valArray;
            }
            else if (typeof(T).Equals(typeof(ExpandoObject)))
            {
                IDictionary<string, object> obj = new ExpandoObject();
                var numer = fields.GetEnumerator();
                int index = -1;
                while (numer.MoveNext() && index < objVal.Length)
                    obj.Add(numer.Current.Key, objVal[(++index)]);

                return (T)obj;
            }

            T val = Activator.CreateInstance<T>();

            for (int i = 0; i < map.Length; i++)
                if (i == 0 || map[i] > 0 && i < properties.Length)
                {
                    PropertyInfo info = properties[map[i]];
                    var baseType = info.PropertyType;

                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                    {
                        object value = objVal[i];
                        if (!baseType.IsClass && baseType.IsGenericType) // && baseType.GenericTypeArguments.Length == 1)
                        {
                            if (baseType == typeof(Nullable<Int32>))
                                baseType = typeof(Int32);
                            //  baseType = typeof(Nullable baseType.GenericTypeArguments[0];
                        }

                        var type = value.GetType();
                        if (type == typeof(byte) && baseType == typeof(int))
                        {
                            value = Convert.ToInt32(value);
                        }
                        else if (type == typeof(short) && baseType == typeof(int))
                        {
                            value = Convert.ToInt32(value);
                        }
                        else if (type == typeof(decimal) && baseType == typeof(int))
                        {
                            value = Convert.ToInt32(value);
                        }
                        else if (type == typeof(string) && baseType == typeof(int))
                        {
                            Int32 result = 0;
                            if (Int32.TryParse(value as string, out result))
                                value = result;
                        }

                        info.SetValue(val, value, null);
                    }
                }

            return val;
        }

        public object[] DbRecordArray()
        {
            return (object[])Array.CreateInstance(typeof(object), map.Length);
        }

        public object[] DbRecordArray(int iLen)
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public SqlField[] GetFields(DbDataReader dataReader)
        {
            return SqlFieldArray.GetArray(dataReader);
        }

        public Type Type { get { return typeof(T); } }

        public int RecordNumber { get; protected set; }

        public IDataMapHelper<T> GetProperties(DbDataReader dataReader, Action<Exception> onDublicateField = null)
        {
            var helper = this;
            this.RecordNumber = 0;

            int iLen = dataReader.IsClosed ? 0 : dataReader.FieldCount;
            helper.map = (int[])Array.CreateInstance(typeof(int), iLen);
            helper.fields = SqlFieldArray.GetFields(dataReader, onDublicateField);
            helper.properties = Type.GetProperties(
                         BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < helper.properties.Length; i++)
            {
                for (int j = 0; j < iLen; j++)
                    if (dataReader.GetName(j).Equals(helper.properties[i].Name))
                        helper.map[j] = i;
            }
            return helper as IDataMapHelper<T>;
        }

        // public SqlField[] GetFields(DbDataReader dataReader) { return SqlField.GetArray(dataReader); }

    }

}
