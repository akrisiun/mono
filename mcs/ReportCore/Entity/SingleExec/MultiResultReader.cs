using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Mono.Entity
{

    public class MultiResultReaderArray : MultiResult<object[]>, IFirstRecord<object[]>
    {
#if NET451|| NET40 || WPF || WEB
        public override MultiResult<object[]> Prepare(ISqlProc proc
                , Action<SqlCommand> setup = null
                , Action<SqlField[]> onReadFields = null
                , bool noMoveFirst = false
                , Action<Exception> onFieldError = null
                )
        {
            this.proc = proc ?? this.proc;
            var procResult = proc as SqlProcResult;
            if (this.dataReader == null)
                this.dataReader = procResult.ExecuteReader(
                        proc.CreateCommand() as SqlCommand,
                        proc.CloseOnDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default // Multi result
                        );
            if (base.numerator == null) {
                var dbObj = new DbObjectSimple();
                numerator = dbObj;
                Reset();
                if (numerator.Current == null)
                    MoveNext();

                base.mapHelper = dbObj.GetProperties(dataReader);
                this.Fields = dataReader.GetFields();    // SqlFieldArray
                this.FirstRecord = dbObj.Current;

                // no: if (helper == null)
                First = numerator.Current;
            }
            return this;
        }
#endif

        public MultiResult<object[]> PrepareNext(ISqlProc proc)
        {
            if (!(this.numerator is DbObjectSimple dbObj) || dataReader == null) {
                return null;
            }

            base.mapHelper = dbObj.GetProperties(dataReader);
            this.Fields = dataReader.GetFields();    // SqlFieldArray
            this.FirstRecord = dbObj.Current;

            // no: if (helper == null)
            First = numerator.Current;
            return this;
        }

        public override bool MoveNext()
        {
            if (!IsReaderAvailable(dataReader) || !dataReader.Read()) {
                if (!dataReader.IsClosed)
                    dataReader.Close();
                return false;
            }

            var dbObj = numerator as DbObjectSimple;
            dbObj.RecordNumber++;
            // First record
            dbObj.Current = dbObj.DbRecordArray(dataReader, dataReader.FieldCount);
            if (dbObj.Current == null) {
                this.LastError = dbObj.LastError;
                return false;
            }
            RecordNumber = dbObj.RecordNumber;
            return true;
        }

        public new object[] Current { get { return numerator == null || RecordNumber < 0 ? null : numerator.Current; } }

        public override void Reset() { (numerator as DbObjectSimple).RecordNumber = -1; }

        public static bool IsReaderAvailable(SqlDataReader reader)
        {
            return reader != null && !reader.IsClosed && reader.FieldCount > 0;
        }
    }

    public class MultiResultReader<T> : MultiResult<T>, IFirstEnumerable<T> where T : class
    {
        protected Func<SqlDataReader, IDataMapHelper<T>> lazyHelper;

        public MultiResultReader(ISqlProc proc, Func<SqlDataReader, IDataMapHelper<T>> lazyHelper = null)
            : base(proc)
        {
            this.lazyHelper = lazyHelper;
        }

        public override async Task<MultiResult<T>> PrepareAsync(ISqlProc proc
                , Action<SqlCommand> setup = null
                , Action<SqlField[]> onReadFields = null
                , bool noMoveFirst = false
                , Action<Exception> onFieldError = null
                )
        {
            this.proc = proc ?? this.proc;
            // var procResult = proc as SqlProcResult;
            this.LastError = null;
            this.numerator = null;
            this.Reset();
            this.dataReader = null;

            var cmd = this.proc.CreateCommand() as SqlCommand;
            setup?.Invoke(cmd);

            SqlDataReader task = await ExecuteReaderAsync(cmd, null);
            if (task == null || task.IsClosed) {
                return null;
            }

            if (numerator == null) {
                var dbObj = new DbObjectSimpleNext() { DbCommand = cmd, Reader = dataReader };
                numerator = dbObj;
                Reset();
            }
            if (numerator.Current == null) {
                await MoveNextAsync();
            }
            if (First == null && numerator is DbObjectSimpleNext) {
                SetValues(numerator as DbObjectSimpleNext, cmd, onFieldError);
            }

            return this;
        }

        public override MultiResult<T> Prepare(ISqlProc proc
                , Action<SqlCommand> setup = null
                , Action<SqlField[]> onReadFields = null
                , bool noMoveFirst = false
                , Action<Exception> onFieldError = null
                )
        {
            this.proc = proc ?? this.proc;
            var procResult = proc as SqlProcResult;
            this.LastError = null;

            var cmd = this.proc.CreateCommand() as SqlCommand;
            setup?.Invoke(cmd);

            if (ExecuteReader(cmd, procResult) == null)
                return null;

            if (numerator == null) {
                var dbObj = new DbObjectSimpleNext() { DbCommand = cmd, Reader = dataReader };
                numerator = dbObj;
                Reset();
                if (numerator.Current == null)
                    MoveNext();

                SetValues(dbObj, cmd, onFieldError);
            }

            return this;
        }

        public MultiResult<T> SetValues(DbObjectSimpleNext dbObj, SqlCommand cmd, Action<Exception> onFieldError = null)
        {
            dbObj.DbConnection = cmd.Connection;
            base.mapHelper = dbObj.GetProperties(dataReader);
            this.Fields = dataReader.GetFields(onDublicateField: onFieldError);    // SqlFieldArray
            this.FirstRecord = dbObj.Current;

            if (lazyHelper != null)
                helper = lazyHelper(dataReader);
            if (helper == null) {
                helper = new MapHelperSimpleProperties<T>();
                helper.GetProperties(dataReader, onDublicateField: onFieldError);
            }
            First = helper.SetValues(dataReader, numerator.Current);
            return this;
        }

        public async Task<SqlDataReader> ExecuteReaderAsync(SqlCommand cmd, SqlProcResult procResult)
        {
            if (this.dataReader == null) {
                /* if (this.proc is global::Mono.Entity.ISqlProcReader) {
                    this.dataReader = (this.proc as ISqlProcReader).Reader as SqlDataReader;
                    if (!this.dataReader.IsReaderAvailable(null)) {
                        this.dataReader = null;
                    }
                    if (this.dataReader != null)
                       return this.dataReader;
                } */

                this.dataReader = await this.proc.ExecuteReaderAsync(cmd) as SqlDataReader;
            }
            return this.dataReader;
        }

        public SqlDataReader ExecuteReader(SqlCommand cmd, SqlProcResult procResult)
        {
            try {
                if (procResult == null && this.dataReader == null) {
                    if (this.proc is global::Mono.Entity.ISqlProcReader)
                        this.dataReader = (this.proc as ISqlProcReader).Reader as SqlDataReader;

                    if (this.dataReader == null && procResult == null)
                        this.dataReader = this.proc.ExecuteReader(cmd) as SqlDataReader;
                } else if (this.dataReader == null) {
                    this.dataReader = procResult.ExecuteReader(
                        cmd,
                        proc.CloseOnDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                }
                if (this.dataReader == null)
                    this.dataReader = cmd.ExecuteReader(
                        CommandBehavior.Default);  // is already open data reader
            }
            catch (Exception ex) {
                this.LastError = ex;
                if (proc != null)
                    proc.LastError = ex;
            }
            return this.dataReader;
        }

        public MultiResult<T> PrepareNext(ISqlProc proc)
        {
            var dbObj = numerator as DbObjectSimpleNext
                ?? new DbObjectSimpleNext { DbCommand = proc.LastCommand as SqlCommand };

            dbObj.Current = null;
            helper = null;
            numerator = dbObj;
            Reset();
            if (numerator.Current == null)
                MoveNext();

            this.SetValues(dbObj);
            return this;
        }

        public virtual void SetValuesNext(DbObjectSimpleNext dbObjNext) {
            SqlDataReader reader = this.dataReader;

            base.mapHelper = dbObjNext.GetProperties(reader);
            this.SetValues(dbObjNext as DbObjectSimple);
        }

        public virtual void SetValues(DbObjectSimple dbObj)
        {
            SqlDataReader reader = this.dataReader;

            base.mapHelper = dbObj.GetProperties(reader);
            this.Fields = reader.GetFields();    // SqlFieldArray
            this.FirstRecord = dbObj.Current;

            if (lazyHelper != null)
                helper = lazyHelper(reader);
            if (helper == null) {
                helper = new MapHelperSimpleProperties<T>();
                helper.GetProperties(reader, null);
            }
            First = helper.SetValues(reader, numerator.Current);
        }

        public override void Reset()
        {
            if (numerator == null)
                return;
            if (numerator is DbObjectSimple) {
                var numSimple = numerator as DbObjectSimple;
                if (numSimple.RecordNumber == 0 && this.RecordNumber < 0) {
                    this.RecordNumber = numSimple.RecordNumber;
                    if (numSimple.Current != null) {
                        SetValues(numSimple);
                    }

                    return;
                }
                numSimple.Current = null;
                numSimple.RecordNumber = -1;
            }
            if (numerator is IFirstRecord<object[]>)
                (numerator as IFirstRecord<object[]>).Prepare();

            this.RecordNumber = -1;
        }

        public virtual async Task<bool> MoveNextAsync()
        {
            bool success = false;
            if (!IsReaderAvailable(dataReader)) {
                return success;
            }
            var task = dataReader.ReadAsync();
            var dbObj = numerator as DbObjectSimple;
            try {
                success = await task;
                dbObj.RecordNumber++;
            }
            catch (NullReferenceException ex1) { this.LastError = ex1; success = false; }
            catch (SqlException ex) {
                success = false;
                this.LastError = ex;
                task = TaskEx.FromException<bool>(ex.InnerException ?? ex);
            }
            if (!success || dbObj == null) {
                return false;
            }

            // First record
            dbObj.Current = dbObj.DbRecordArray(dataReader, dataReader.FieldCount);
            if (dbObj.Current == null) {
                this.LastError = dbObj.LastError;
                return false;
            }
            RecordNumber = dbObj.RecordNumber;
            return true;
        }

        public override bool MoveNext()
        {
            if (!IsReaderAvailable(dataReader)) {
                return false;
            }
            bool success = false;
            var dbObj = numerator as DbObjectSimple;

            try {
                success = dataReader.Read();
                //    dataReader.Close();     // NO!!!, expecting NextResult
            }
            catch (Exception ex) {           // divide by zero or other errors ..
                this.LastError = ex;
            }

            dbObj.RecordNumber++;
            if (!success) {
                return false;
            }

            // First record
            dbObj.Current = dbObj.DbRecordArray(dataReader, dataReader.FieldCount);
            if (dbObj.Current == null) {
                this.LastError = dbObj.LastError;
                return false;
            }
            RecordNumber = dbObj.RecordNumber;
            return true;
        }

        public static bool IsReaderAvailable(SqlDataReader reader)
        {
            return reader != null && !reader.IsClosed && reader.FieldCount > 0;
        }


        public virtual ICollection<T> IntoCollection()
        {
            var list = new Collection<T>();
            Reset();
            while (MoveNext())
                list.Add(this.Current);

            return list;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Prepare();
            return this;
        }

    }

}
