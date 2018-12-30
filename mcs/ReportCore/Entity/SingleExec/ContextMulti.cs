using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Mono.Entity
{

    public class MultiResult<T> : IFirstRecord<T>, IDisposable where T : class
    {
        #region Properties, Data

        public int Count { get; private set; }
        public int Depth { get { return dataReader == null ? -1 : dataReader.Depth; } }
        public SqlDataReader Reader { get { return dataReader; } }
        public ISqlProc Proc { get { return proc; } }
        public Exception LastError { get; set; }

        public Dictionary<string, SqlFieldInfo> Fields { get; protected set; }

        public object[] FirstRecord { get; protected set; }

        public MultiResult(ISqlProc proc = null)
        {
            Count = 1;
            // mapper = null;
            this.proc = proc;
            dataReader = null;
            mapHelper = null;
            RecordNumber = -1;
        }

        ~MultiResult()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (dataReader != null && !dataReader.IsClosed
                && dataReader.RecordsAffected > 0) // or Handle is not initialized error
                dataReader.Dispose();

            dataReader = null;
            if (proc != null && proc.CloseOnDispose && proc.Connection != null)
                proc.Connection.CloseConn(true);
            proc = null;
            mapHelper = null;
            numerator = null;
        }

        protected ISqlProc proc;
        protected IDataMapHelper<object[]> mapHelper;     // DbObject mapper;
        protected SqlDataReader dataReader;
        // protected DbEnumeratorData numerator;
        protected IEnumerator<object[]> numerator;

        #endregion

        #region IFirstRecord

        public T First { get; protected set; }
        public int RecordNumber { get; protected set; }
        public T Current { get { return helper == null || RecordNumber < 0 ? null : helper.SetValues(dataReader, numerator.Current); } }

        // public DbEnumeratorData Numerator { get { return numerator as DbEnumeratorData; } }
        private IFirstRecord<object[]> Worker { get { return numerator as IFirstRecord<object[]>; } }
        object IEnumerator.Current { get { return numerator == null ? null : numerator.Current; } }

        public virtual bool Any() { return numerator != null && RecordNumber >= 0; } //  && Numerator.RecordIndex >= 0; }
        public bool Prepare()
        {
            if (proc.Connection != null && proc.Connection.State == ConnectionState.Closed) {
                proc.LastError = null;
                proc.Connection = proc.PreparedSqlConnection();
                if (proc.Connection == null || proc.Connection.State == ConnectionState.Closed)
                    proc.LastError = new InvalidOperationException("Connection closed");
                else
                    if (proc.Connection is SqlConnection
                        && SqlProcConnect.UpdateSpidIfError(proc.Connection as SqlConnection, proc) != null)
                    proc.LastError = null;
            }

            // There is already an open DataReader associated with this Command which must be closed first.
            // ContextMulti.cs:line 87
            if (proc.LastError != null)
                throw proc.LastError;
            proc.LastError = null;
            if (numerator == null && Prepare(proc as SqlProc) == null)
                return false;
            var lastErr = proc.LastError;
            if (lastErr is SqlException)
                return false;

            Reset();
            return numerator != null;
        }

        public virtual void Reset()
        {
            if (numerator == null) {
                Dispose();
                if (!Prepare())
                    return;
            }
            numerator.Reset();
        }
        public virtual bool MoveNext()
        {
            bool success = numerator != null && numerator.MoveNext();
            this.RecordNumber = numerator == null ? -1 : Worker.RecordNumber; // Numerator.RecordIndex;
            return success;
        }

        protected IDataMapHelper<T> helper;
        public IEnumerator<T> GetEnumerator() { return Result<T>().GetEnumerator(); }
        //IEnumerator IEnumerable.GetEnumerator() { return Result<T>().GetEnumerator(); }

        #endregion

        public virtual MultiResult<T> Prepare(ISqlProc proc
                , Action<SqlCommand> setup = null
                , Action<SqlField[]> onReadFields = null
                , bool noMoveFirst = false
                , Action<Exception> onFieldError = null
                )
            => PrepareData(Worker);

        public virtual MultiResult<T> PrepareData(IFirstRecord<object[]> numerator)
        {
            if (numerator == null || numerator.Current == null)
                return null;

            var rec = numerator.Current as object[]; // DbDataRecord;
            this.RecordNumber = numerator.RecordNumber;

            if (mapHelper == null) {
                var dbObj = new DbObjectSimple();
                mapHelper = dbObj.GetProperties(dataReader);
            }
            Fields = dataReader.GetFields();    // SqlFieldArray
            FirstRecord = rec;
            if (helper == null) {
                helper = new MapHelperSimpleProperties<T>(); // new DbDataMapHelper<T>();
            }

            First = Current;
            return this;
        }

        public virtual Task<MultiResult<T>> PrepareAsync(ISqlProc proc
                , Action<SqlCommand> setup = null
                , Action<SqlField[]> onReadFields = null
                , bool noMoveFirst = false
                , Action<Exception> onFieldError = null
                )
        {
            if (numerator == null) {
                var cmd = proc.CreateCommand();
                var dbObj = new DbObjectSimpleNext() { DbCommand = cmd, Reader = dataReader };
                numerator = dbObj;
            }

            if (numerator == null || numerator.Current == null)
                return null;

            var rec = numerator.Current as object[]; // DbDataRecord;
            this.RecordNumber = (numerator as IFirstRecord<object[]>)?.RecordNumber ?? -1;

            if (mapHelper == null) {
                var dbObj = new DbObjectSimple();
                mapHelper = dbObj.GetProperties(dataReader);
            }
            Fields = dataReader.GetFields();    // SqlFieldArray
            FirstRecord = rec;
            if (helper == null) {
                helper = new MapHelperSimpleProperties<T>(); // new DbDataMapHelper<T>();
            }

            First = Current;
            return Task.FromResult(this);
        }

        public static object[] DbRecord(int iLen)
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public IEnumerable<object[]> ResultObj()
        {
            var cycle = numerator;
            do {
                var rec = numerator.Current as object[]; // DbDataRecord;
                if (rec == null)
                    yield break;    // first error
                yield return rec;
            } while (cycle.MoveNext());

            cycle.Dispose();
        }

        // -> DataReaderExpando
        //public IEnumerable<ExpandoObject> ResultDyn()
        //{
        //    var cycle = numerator;
        //    var helper = new DbMapperDyn(reader);
        //    do
        //    {
        //        object[] rec = numerator.Current; // as DbDataRecord;
        //        if (rec == null)
        //            yield break;    // first error
        //        dynamic obj = helper.Get(rec);
        //        yield return obj;
        //    } while (cycle.MoveNext());
        //    cycle.Dispose();
        //}

        public IEnumerable<TRes> Result<TRes>() where TRes : class
        {
            Mono.Guard.Check(proc != null && proc.Connection != null, "MultiResult proc error");
            if (numerator == null)
                yield break;
            if (typeof(TRes).Equals(typeof(object[]))) {
                do {
                    var values = numerator.Current as object[]; // DbDataRecord;
                    yield return values as TRes;
                } while (numerator.MoveNext());
            }

            var helper = new DbDataMapHelper<TRes>();

            var cycle = numerator;
            do {
                object[] rec = numerator.Current as object[]; // as DbDataRecord;
                if (rec == null) {
                    yield break;    // first error
                }

                var objArray = DbRecord(dataReader.FieldCount);
                TRes obj = helper.SetValues(objArray);
                yield return obj;
            }
            while (cycle.MoveNext());
            cycle.Dispose();
        }

        public bool NextResult() //  Next<T>(DbDataMapHelper<T> helper)
        {
            if (Depth < 0 || numerator == null)
                return false;
            var next = numerator as IReaderNextResult;
            if (next == null) {
                next = new DbObjectSimpleNext { Reader = this.Reader, DbConnection = this.Proc.Connection };
            }

            if (!next.NextResult()) {
                return false;
            }

            return true;
        }

    }

    public static class ContextMulti
    {
        public static MultiResult<T> MultiExec<T>(this Context db, SqlCommand cmd) where T : class
        {
            var proc = new SqlProc {
                Connection = ConnectionPool.NewConn(db.ConnectionString()),
                CmdText = cmd.CommandText,
                Param = SqlProc.CloneParam(cmd.Parameters)
            };

            return MultiExec<T>(proc);
        }


        public static MultiResult<T> MultiExec<T>(this Context db, object sqlProcNamed) where T : class
        {
            var proc = SqlProcExt.ProcNamed(sqlProcNamed);
            proc.Context = db;
            return MultiExec<T>(proc);
        }

        public static DataReaderEnum<T> ExecFirst<T>(this Context db, object sqlProcNamed,
                Action<Exception> onError = null
                ) where T : class
        {
            var proc = SqlProcResult.ProcNamed(sqlProcNamed, db);
            return new DataReaderEnum<T>(proc);
        }

        public static Task<DataReaderEnum<T>> ExecFirstAsync<T>(this Context db, object sqlProcNamed,
                Action<Exception> onError = null
                ) where T : class
        {
            var proc = SqlProcResult.ProcNamed(sqlProcNamed, db);
            return DataReaderEnum<T>.FromTask(proc);
        }

        public static MultiResult<T> MultiExec<T>(this SqlProc proc) where T : class
        {
            MultiResult<T> result = new MultiResult<T>();
            result.Prepare(proc);
            return result;
        }

        public static DataReaderArray MultiObjCommand(this Context db, string proc, Action<SqlCommand> setup = null)
        {
            var procMap = new SqlProcResult { Context = db, Connection = db.SqlConnection, CmdText = proc, Param = null };
            return procMap.DataReaderArray();
        }
        // #endif
    }
}
