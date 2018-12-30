using Mono.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public class ScalarObj : Tuple<object[], SqlCommand, string[]>, IDisposable
    {
        static ScalarObj() { Empty = new object[] { }; }
        public static object[] Empty { [DebuggerStepThrough] get; private set; }

        public ScalarObj() : base(Empty, null, null) { }
        public ScalarObj(object[] item1, SqlCommand item2, string[] item3) : base(item1 ?? Empty, item2, item3) { }

        public void Dispose() { if (Item2 != null) this.Item2.Dispose(); }

        public object[] Data { [DebuggerStepThrough] get { return this.Item1; } }
        public SqlCommand Cmd { [DebuggerStepThrough] get { return this.Item2; } }
        public string[] FieldNames { [DebuggerStepThrough] get { return this.Item3; } }

#if NET451 || WPF || WEB
        public T Fill<T>(T item) where T : class
        {
            var prop = ReflectionCache.GetProperties(item);
            for (int i = 0; i < FieldNames.Length; i++)
            {
                var field = FieldNames[i];
                object propertyValue = Data[i];
                if (propertyValue == null)
                    continue;

                foreach (var pi in prop.Where((e) => e.Name == field))
                {
                    try
                    {
                        pi.SetValue(item,
                            System.Convert.ChangeType(propertyValue, pi.PropertyType));
                    }
                    catch { }
                }
            }

            return item;
        }
#endif

        //  T ConvertDataRecord<T>(IDataRecord dbDataRecord, IEnumerable<PropertyInfo> properties)
        //    where T : new() {
        //    var entity = new T();
        //    foreach (var propertyInfo in properties)
        //    {
        //        object value = null;
        //        try
        //        {
        //            value = dbDataRecord[propertyInfo.Name];
        //        }
        //        catch { }
        //        if (value == null)
        //            continue;
        //        var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        //        var newValue = value is DBNull || value == null ? null : Convert.ChangeType(value, propertyType);

        //        propertyInfo.SetValue(entity, newValue, new object[] { });
        //    }
        //    return entity;

    }

    public static class ScalarObjExec
    {
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result
        ///     Additional columns or rows are ignored. 
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static ScalarObj ExecuteScalarObj(this SqlProc proc, Action<Exception> onError = null)
        {
            Tuple<SqlConnection, SqlCommand> tuple = DbGetHelper.ConnCmd(proc);
            SqlCommand cmd = tuple.Item2;

            ScalarObj result = new ScalarObj(ScalarObj.Empty, cmd, null);
            if (cmd == null)
                return result;

            try
            {
                SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                if (reader.Read() && reader.FieldCount > 0)
                {
                    var dbObj = new DbObjectSimple();
                    var mapHelper = dbObj.GetProperties(reader,
                            (err) => { proc.LastError = err; });

                    var FieldNames = dbObj.GetFields(reader);
                    //this.Fields = reader.GetFields(onDublicateField: onFieldError);    // SqlFieldArray

                    //    helper = lazyHelper(reader);
                    //    helper = new MapHelperSimpleProperties<T>();
                    //    helper.GetProperties(reader, onDublicateField: onFieldError); 

                    object[] First = dbObj.DbRecordArray(reader, dbObj.iLen);

                    result = new ScalarObj(First, cmd, dbObj.FieldNames);

                    dbObj.Dispose();
                }
                reader.Dispose();
            }
            catch (Exception ex) {
                if (onError != null)
                    onError(ex);
                if (proc != null) proc.Context.LastError = ex;
            }

            if (proc.Connection != null)
                proc.Connection.Dispose();
            return result;
        }
    }

}
