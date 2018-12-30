
using Mono.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;

namespace Mono.Entity
{
#if XLSX && !WEB && !NPOI_ORIGIN
    using Mono.Internal;
#endif

    public static class DbConverter
    {
        public static T Clone<T>(this object[] objVal)
        {
            T valArray = (T)objVal.Clone();
            //  Activator.CreateInstance(typeof(T), objVal.Length);
            // default(T) : non-static method requires a target
            // object[] array = valArray as object[];
            return valArray;
        }

        // #if NET451 || NETCORE || SNEX // || WPF || WEB // || 
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
            foreach (string keyName in fieldsDict.Keys) {
                i++;
                if (!string.IsNullOrWhiteSpace(keyName)) {
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
            for (int i = 0; i < map.Length; i++) {
                if (i == 0 || map[i] > 0) {
                    PropertyInfo info = properties[map[i]];
                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                        res.Add(info.Name, objVal[i]);
                }
            }

            return res as ExpandoObject;
        }

        public static ExpandoObject ToExpando(this object[] objVal, SqlField[] fields)
        {
            IDictionary<string, object> res = new ExpandoObject();
            for (int i = 0; i < fields.Length; i++)
                if (i == 0 || !string.IsNullOrWhiteSpace(fields[i].Name)) {
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
            else if (obj is ExpandoObject) {
                res = new ExpandoObject();
                var resObj = obj as IDictionary<string, object>;
                var num = resObj.GetEnumerator();

                for (int i = 0; i < fields.Length; i++) {
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

            // #if !NETCORE // || 
            if (res == null) {
                res = new ExpandoObject();
                for (int i = 0; i < fields.Length; i++)
                    if (i == 0 || !string.IsNullOrWhiteSpace(fields[i].Name)) {
                        var objVal = obj.GetPropertyValue(fields[i].Name);
                        if (objVal != null && !objVal.Equals(DBNull.Value))
                            res.Add(fields[i].Name, objVal);
                        else
                            res.Add(fields[i].Name, null);
                    }
            }

            return res as ExpandoObject;
        }
        // #endif

        public static T SingleString<T>(this object objVal, string property = "xml")
        {
            T val = Activator.CreateInstance<T>();
            val.SetValue<string>(property, objVal.ToStringIfNull(String.Empty));
            return val;
        }

        public static T ToType<T>(this object[] objVal, int[] map, PropertyInfo[] properties)
        {
            T val = Activator.CreateInstance<T>();
            FillValues<T>(objVal, val, map, properties);
            return val;
        }

        public static void FillValues<T>(object[] objVal, T val, int[] map, PropertyInfo[] properties)
        {
            if (properties == null || properties.Length == 0) {
                var valOne = objVal[0];
                if (val is ExpandoObject || val is IDictionary<string, object>) // typeof(T) == typeof(ExpandoObject))
                    val.SetValue<string>("xml", StringConvert.ToStringIfNull(valOne, String.Empty));
                return;
            }

            if (map == null || map.Length == 0)
                return;
            for (int i = 0; i < map.Length; i++)
                if (i == 0 || map[i] > 0) {
                    PropertyInfo info = properties[map[i]];
                    if (objVal[i] != null && !objVal[i].Equals(DBNull.Value))
                        info.SetValue(val, objVal[i], null);
                }
        }

#if NET451
        public static T ToType<T>(this ExpandoObject obj) where T : class, new()
        {
            T val = Activator.CreateInstance<T>();
            ExpandoConvert.CloneToObj(obj, val);  //  CloneTo<T>(obj, ref val);
            return val;
        }
#endif
    }

    public interface IDbObject : IDisposable, IEnumerator, IEnumerator<object[]>, ILastError
    {
        DbDataReader Reader { get; }
        IDbConnection Connection { get; set; }

        object[] First {
            get;
            // set; 
        }
        int RecordNumber { get; set; }
        bool Prepare();

        IEnumerator<object[]> GetEnumerator();
    }

    public class DbDataMapHelper : ILastError
    {
        protected IDataMapHelper<object[]> mapper;

        protected IEnumerator<object[]> numerator;

        protected IDbObject dbObj;
        protected DbDataMapHelper(IDbObject _dbObj) { this.dbObj = _dbObj; }
        public IDbObject DbObj { get { return dbObj; } }

        //#if NET451 || WEB
        public DbObjectSimple DbObjectMapper { get { return dbObj as DbObjectSimple ?? mapper as DbObjectSimple; } }
        // #endif

        public IEnumerator<object[]> Worker { get { return dbObj.GetEnumerator(); } }
        public DbDataReader Reader { get { return dbObj == null ? null : dbObj.Reader; } }


        public virtual void Dispose() { if (dbObj != null) dbObj.Dispose(); }
        public Exception LastError { get; set; }
    }

    public class DbDataMapHelper<T> : DbDataMapHelper, IDataMapHelper<T>, IFirstRecord<T>
    {
        #region Properties

        public DbDataMapHelper(IDbObject dbObj = null)
            : base(dbObj)
        {
            iLen = 0;
            map = null;
            properties = null;
        }

#if NET451
        public DbDataMapHelper(DbObjectSimple dbo)
            : base(null)
        {
            iLen = dbo.iLen;
            map = null;
            properties = null;
            mapper = dbo;
        }
#endif
        public Type Type {
            get { return typeof(T); }
        }

        protected int iLen;
        protected int[] map;
        protected PropertyInfo[] properties;
        protected Dictionary<string, SqlFieldInfo> fields;

        #endregion

        #region MapHelper transform values to object

        public IDataMapHelper<T> GetProperties(DbDataReader dataReader, Action<Exception> onError = null)
        {
            Type type = this.Type;
            if (dataReader == null || dataReader.IsClosed)
                return null;
            return GetProperties(dataReader, onError, type);
        }

        public THelper SetProperties<THelper>(DbDataReader dataReader, Type type) where THelper : class, IDataMapHelper
        {
            this.GetProperties(dataReader, null, type);
            return (THelper)(object)this;
        }

        public DbDataMapHelper<T> GetProperties(DbDataReader dataReader, Action<Exception> onError, Type type)
        {
            if (dataReader == null || dataReader.IsClosed) {
                iLen = 0;
                return this;
            }
            iLen = dataReader.FieldCount;
            this.RecordNumber = 0;
            Mono.Guard.Check(iLen > 0, "DataMapper FieldCount error");

            map = (int[])Array.CreateInstance(typeof(int), iLen);

            type = type ?? Type;
#if NET451 || WPF || WEB
            properties = type.GetProperties(
                         BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                for (int j = 0; j < iLen; j++)
                    if (dataReader.GetName(j).Equals(properties[i].Name))
                        map[j] = i;
            }
#endif
            fields = SqlFieldArray.GetFields(dataReader);
            return this;
        }

        public Tuple<int[], PropertyInfo[]> GetMap() { return new Tuple<int[], PropertyInfo[]>(this.map, this.properties); }

        public int? GetOrdinal(string columnName)
        {
            if (!this.fields.ContainsKey(columnName))
                return null;
            return this.fields[columnName].Ordinal;
        }

        public object GetField(string columnName, object[] itemArray)
        {
            int? ordinal = GetOrdinal(columnName);
            return ordinal.HasValue ?
                   (itemArray[ordinal.Value] == DBNull.Value ? null : itemArray[ordinal.Value])
                   : null;
        }

        public T SetValuesXml(DbDataReader reader, object[] objVal)
        {
            if (objVal == null)
                return default(T);
            int iLen = objVal.Length;
            if (iLen == 1 && typeof(T) == typeof(ExpandoObject))
                return DbConverter.SingleString<T>(objVal, "xml");
            // .ToType<T>(objVal, this.map, this.properties);

            return SetValues(objVal);
        }

        public T SetValues(DbDataReader reader, object[] objVal) { return SetValues(objVal); }

        public virtual T SetValues(object[] objVal)
        {
            // object[] : {"No parameterless constructor defined for this object."}
            if (Type.IsArray) {
                return DbConverter.Clone<T>(objVal);
            } else if (Type.Equals(typeof(ExpandoObject))) {
                return (T)(object)DbConverter.ToExpando(objVal, fieldsDict: this.fields);
            }

            if (this.properties == null) {
                this.properties = new PropertyInfo[0];
                this.map = new int[0];
                this.iLen = 0;
                this.GetProperties(this.Reader, null, this.Type);
            }

            T val = DbConverter.ToType<T>(objVal, this.map, this.properties);
            return val;
        }

        public object[] DbRecordArray()
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public object[] DbRecordArray(int iLen)
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public virtual Dictionary<string, SqlFieldInfo> Fields { get { return fields; } }

        public virtual SqlField[] GetFields(DbDataReader dataReader)
        {
            //if (dataReader == null && fields != null)
            //    return fields;

            if (this.iLen == 0 || this.properties.Length == 0)
                this.GetProperties(dataReader, onError: null, type: typeof(T));

            return SqlFieldArray.GetArray(dataReader);
        }

        #endregion

        public static IFirstRecord<T> IterateDbObject(IDbObject dbObj)
        {
            var trans = new DbDataMapHelper<T>(dbObj);
            trans.dbObj = dbObj;
            trans.GetProperties(dbObj.Reader);
            return trans;
        }

        public int RecordNumber { get; private set; }

        #region IFirst

        private T firstRec;

        public T First {
            get { return firstRec; }
        }

        public bool Any()
        {
            if (numerator == null && !Prepare())
                return false;
            if (numerator.Current == null && !MoveNext())
                return false;
            return numerator.Current != null;
        }

        public ISqlProc SqlProc { get; set; }

        public virtual bool Prepare()
        {
            Reset();
            var proc = this.SqlProc;

            if (numerator == null && proc != null && proc is SqlProc) {
                proc.LastError = null;
                numerator = DbObject.Exec(proc as SqlProc);
                if (proc?.LastError != null
                    && numerator == null || numerator.Current == null) {
                    return false;
                }

                this.dbObj = numerator as IDbObject;
            }

            if (firstRec == null
                && numerator is IDbObject && (numerator as IDbObject).First != null) {
                var rec = dbObj.First;
                if (map == null)
                    GetProperties(dbObj.Reader);
                firstRec = SetValues(rec);
            }
            return numerator != null;
        }

        public virtual T Current {
            get {
                if (numerator == null)
                    return default(T);
                object[] rec = numerator.Current;
                return SetValues(rec);
            }
        }

        public override void Dispose()
        {
            numerator = null;
            base.Dispose();
        }

        object IEnumerator.Current {
            get { return numerator == null ? null : numerator.Current; }
        }

        public virtual bool MoveNext()
        {
            if (numerator == null || dbObj == null)
                return false;
            if (!numerator.MoveNext()) {
                if (RecordNumber == -1 && dbObj.RecordNumber == -1) {
                    numerator = dbObj.GetEnumerator();
                    if (numerator != null && !numerator.MoveNext())
                        return false;
                } else
                    return false;

                RecordNumber = dbObj.RecordNumber - 1;
            }

            if (map == null)
                GetProperties(dbObj.Reader);

            RecordNumber++;
            if (RecordNumber == 0)
                firstRec = Current;
            return true;
        }

        public virtual void Reset()
        {
            LastError = null;
            RecordNumber = -1;
            if (dbObj == null) {
                numerator = null;
                return;
            }

            if (dbObj.Reader != null) {
                if (dbObj.Reader.IsClosed)
                    dbObj.Prepare();

                if (dbObj.RecordNumber >= 0)
                    dbObj.Reset();

                numerator = dbObj;
                return;
            }
            numerator = dbObj.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            return this;
        }

        #endregion
    }

}
