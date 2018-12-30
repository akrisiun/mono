using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Mono.Entity
{
    using ReaderEnumExpando = KeyValuePair<SqlDataReader, IEnumerator<ExpandoObject>>;

    public static class ContextXElem
    {
        public static XElement Convert(this ExpandoObject obj, string rootName)
        {
            XElement root = new XElement(rootName);
            var numerator = obj.Keys().GetEnumerator();
            while (numerator.MoveNext())
            {
                string key = numerator.Current;
                object value = (obj as IDictionary<string, object>)[key];
                root.Add(new XElement(key, value));
            }
            return root;
        }

        public static XElement ExecMergeXElem(this Context db, object namedParam
                , string[] elemNames
                , Action<SqlCommand> setupCmd = null, Action<Exception> onError = null)
        {
            KeyValuePair<SqlDataReader, DbEnumeratorData<ExpandoObject>>
                    firstSet = ExecDyn(db, namedParam, setupCmd);

            var reader = firstSet.Key;
            var numerator = firstSet.Value;

            if (reader == null)
            {
                if (db.LastError != null && onError != null)
                    onError(db.LastError);
                return null;
            }

            Guard.Check(elemNames.Length >= 2);
            var retElement = new XElement(elemNames[0]);
            var rootName = elemNames[1];

            var depth = reader.Depth;
            int index = 1;

            numerator.Reset();
            while (numerator.MoveNext())
            {
                var expando = numerator.Current as ExpandoObject;
                retElement.Add(expando.Convert(rootName));
            }

            while (!reader.IsClosed)
            {
                if (!reader.NextResult())
                    break;

                index++;
                rootName = elemNames.Length <= index ? "Node" + index.ToString()
                         : elemNames[index];

                var result2 = DbEnumeratorData.GetResultDyn(() =>
                {
                    return new Tuple<DbDataReader, IDbConnection>(reader, numerator.Connection);
                });

                while (result2.MoveNext())
                {
                    XElement elem = result2.Current.Convert(rootName);
                    retElement.Add(elem);
                }
            }

            // now is safe to dispose
            reader.Dispose();
            numerator.Dispose();

            return retElement;
        }

#if NET451 || NETCORE
        internal static IEnumerator<ExpandoObject>
                 ExecMergeDyn(this Context db, object namedParam
                       , Action<SqlCommand> setupCmd = null)
        {
            var numerator = new DataReaderExpando(SqlProcResult.ProcNamed(namedParam, db));
            return numerator.Worker;
        }
#endif

        // -> DataReaderExpando
        static internal IEnumerable<ExpandoObject> ResultDyn(IEnumerator<object[]> numerator, SqlDataReader reader)
        {
            var cycle = numerator;
            var helper = new DbMapperDyn(reader);
            do
            {
                object[] rec = numerator.Current; // as DbDataRecord;
                if (rec == null)
                    yield break;    // first error
                dynamic obj = helper.Get(rec);
                yield return obj;
            }
            while (cycle.MoveNext());

            cycle.Dispose();
        }

        internal static KeyValuePair<SqlDataReader, DbEnumeratorData<ExpandoObject>>
                 ExecDyn(this Context db, object namedParam
                        , Action<SqlCommand> setupCmd = null)
        {
            var proc = SqlProcExt.ProcNamed(namedParam);
            proc.Context = db;
            if (proc.Connection == null)
                proc.Connection = proc.PreparedSqlConnection();

            SqlDataReader readerGet = null;
            var numerator
                = SqlMultiDyn.ResultDyn(proc, (reader) => readerGet = reader, setupCmd);
            if (numerator.Current == null || readerGet == null)
                numerator.MoveNext();

            return new KeyValuePair<SqlDataReader, DbEnumeratorData<ExpandoObject>>(readerGet, numerator);
        }
    }
}
