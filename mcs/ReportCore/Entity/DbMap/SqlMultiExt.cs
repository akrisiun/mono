using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public static class SqlMultiExt
    {
        // public static Func<SqlDataReader>
        public static Func<Tuple<DbDataReader, IDbConnection>> 
                LazyReader(ISqlProc proc
                , Action<SqlDataReader> readerGet
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            return SqlMultiDyn.LazyReader(proc, readerGet, setupCmd, onError);
        }

        public static ResultDyn ExecNamedResultDyn(this Context db, object named
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            var proc = SqlProcExt.ProcNamed(named, db);
            SqlDataReader reader = null;
            Exception lastError = null;

            if (proc.Connection == null)
                proc.Connection = proc.PreparedSqlConnection();

            var res = DbEnumeratorData.GetResultDyn(
                LazyReader(proc, (r) => reader = r, setupCmd
                          , onError: (err) =>
                          {
                              lastError = err;
                              if (onError != null)
                                  onError(err);
                          })
            );

            if (lastError != null)
                res.LastError = lastError;
            return res;
        }

        public static ResultDyn ExecProcResultDyn(this ISqlProc proc
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            SqlDataReader reader = null;
            return DbEnumeratorData.GetResultDyn(
                SqlMultiDyn.LazyReader(proc, (r) => reader = r, setupCmd, onError));
        }

        /*
        public static KeyValuePair<ResultDyn, IEnumerable<T>> ResultObj<T>(this Context db, object named
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null) where T : class
        {
            var proc = SqlProcExt.ProcNamed(named, db);
            Tuple<DbDataReader, IDbConnection> readContext;
            ResultDyn dyn = DbEnumeratorData.GetResultDyn(
                LazyReader(proc, null, setupCmd, onError));

            readContext = new Tuple<DbDataReader, IDbConnection>(dyn.Reader, dyn.Connection);
            if (dyn != null && db.LastError != null && dyn.LastError == null && dyn.Reader == null)
                dyn.LastError = db.LastError;

            var res = CastResult<T>(dyn, onError);
            if (res.Key != null)
                res.Key.Prepare();
            return res;
        }

        public static KeyValuePair<ResultDyn, IEnumerable<T>> CastResult<T>(this ResultDyn dyn
                , Action<Exception> onError = null) where T : class
        {
            return new KeyValuePair<ResultDyn, IEnumerable<T>>(dyn, dyn.CastIterator<T>());
        }

        public static IEnumerable<T> ExpandoCastIterator<T>(this ResultDyn dyn) where T : class
        {
            return dyn.CastIterator<T>();
        }
        */
    }
}
