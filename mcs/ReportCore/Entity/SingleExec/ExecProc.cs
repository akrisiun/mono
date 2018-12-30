using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public static class ExecProc
    {

#if false
        public static IEnumerator<T> MultiExec<T>(SqlProc proc,
                 Action<SqlDataReader, SqlField[]> onReadFields = null,
                 Action<Exception> onError = null)
                 where T : class
        {
            var cmd = proc.CreateCommand();
            var mapper = new DbObject();

            SqlDataReader reader = SqlMultiDyn.ExecMultiReader(proc, onError: onError);
            if (reader == null || !reader.HasRows)
                yield break;

            IEnumerable<object[]> worker = ExecMulti(proc, reader, mapper, onReadFields, progress: null);
            var numerator = worker.GetEnumerator();

            Guard.Check(numerator.Current == null);
            Type type = typeof(T);
            if (type.IsArray)
            {
                while (numerator.MoveNext())
                {
                    object[] record = numerator.Current;
                    T result = (object)record as T;
                    yield return result;
                }
            }
            else
            {
                var helper = new DbDataMapHelper<T>();
                Guard.Check(reader.FieldCount > 0 && reader.HasRows);
                helper.GetProperties(reader);

                while (numerator.MoveNext())
                {
                    object[] record = numerator.Current;
                    // var objArray = DbRecord(reader.FieldCount);
                    T obj = helper.SetValues(record);
                    if (obj != null)
                        yield return obj;
                }
            }

            numerator.Dispose();
            if (!reader.IsClosed)
            {
                reader.Close();
                if (proc.Connection != null)
                    proc.Connection.CloseConn(true);
            }
        }

        public static IEnumerable<object[]> ExecMulti(ISqlProc proc, SqlDataReader dataReader,
                    IDataMapHelper<object[]> mapper,
                    Action<SqlDataReader, SqlField[]> onReadFields = null,
                    Action<double> progress = null)
        {
            var helper = mapper;
            helper.GetProperties(dataReader);

            if (onReadFields != null)
                onReadFields(dataReader, helper.GetFields(dataReader));

            if (dataReader.HasRows) // .RecordsAffected > 0)
            {
                while (dataReader.Read())
                {
                    object[] objVal = helper.DbRecordArray();
                    dataReader.GetValues(objVal);

                    object[] val = helper.SetValues(dataReader, objVal);
                    yield return val;
                }
            }

            if (!dataReader.IsClosed)
                dataReader.Close();
            if (proc.Connection != null)
                proc.Connection.CloseConn(true);

            if (progress != null)
                progress(1.0);
        }

        public static IEnumerable<object[]> ExecArray(this ISqlProc proc,
                Action<SqlDataReader, SqlField[]> onReadFields = null)
        {
            SqlDataReader dataReader = SqlMultiDyn.ExecMultiReader(proc);
            if (dataReader == null || dataReader.IsClosed)
                yield break;
            if (dataReader.FieldCount > 0 && !dataReader.HasRows)  // && !dataReader.Read())
                yield break;

            var mapper = new DbObject { Reader = dataReader };
            SqlField[] fields = mapper.GetFields(dataReader);
            if (onReadFields != null)
                onReadFields(dataReader, fields);

            if (dataReader.HasRows)  // .RecordsAffected > 0)
            {
                while (dataReader.Read())
                {
                    object[] objVal = mapper.DbRecordArray();
                    dataReader.GetValues(objVal);
                    yield return objVal;
                }
            }

            if (!dataReader.IsClosed)
            {
                dataReader.Close();
                if (proc.Connection != null)
                    proc.Connection.CloseConn(true);
            }
        }

        // todo: IFirstRecord<object[]>
        public static IEnumerable<object[]> ExecFirstArray(this Context db, object sqlProcNamed,
                Action<Exception> onError = null)
        {
            var proc = SqlProcExt.ProcNamed(sqlProcNamed);
            proc.Context = db;
            SqlDataReader reader = null;
            var numerator = ExecProc.ExecArray(proc,
                               onReadFields: (r, fields) =>
                               {
                                   reader = r;
                               });

            return numerator;
        }

#endif

    }
}
