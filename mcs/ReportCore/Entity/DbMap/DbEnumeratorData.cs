using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

namespace Mono.Entity
{
    //public static class DbCheck
    //{
    //    public static bool IsReaderAvailable(this DbDataReader reader, ILastError onError = null)
    //    {
    //        bool? ok = null;
    //        if (reader != null && !reader.IsClosed) {
    //            int maxCnt = 10;
    //            try {
    //                ok = reader.FieldCount > 0;
    //                if (ok.HasValue && !ok.Value) {
    //                    while (maxCnt > 0 && !ok.Value) {
    //                        ok = reader.NextResult();
    //                        maxCnt--;
    //                    }
    //                }
    //            }
    //            catch (Exception ex) {
    //                // Invalid attempt to call FieldCount when closed
    //                // http://referencesource.microsoft.com/#System.Data/System/Data/SqlClient/SqlDataReader.cs,8dcd4c76d9376d85
    //                if (onError != null)
    //                    onError.LastError = ex;
    //            }
    //        }
    //        return ok ?? false;
    //    }
    //}

    public class DbEnumeratorData<T> : DbEnumeratorData, IEnumerator<T>, IEnumerable<T>, System.Collections.IEnumerator
    {
        public static new DbEnumeratorData<T> Empty { get { return (DbEnumeratorData<T>)(object)DbEnumeratorData.Empty; } }

        public IDataMapHelper Helper { get; set; }
        public Func<IDataMapHelper<T>> GetHelper { get; set; }

        public DbEnumeratorData(Func<Tuple<DbDataReader, IDbConnection>> getReader)
            : base(null)
        {
            if (GetHelper != null)
                Helper = GetHelper();
            if (getReader != null) {
                GetReader = getReader;
                Reset();
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (Current == null)
                Helper = GetHelper == null ? null : GetHelper();
        }

        public override bool NextResult()
        {
            Helper = null;
            if (!base.NextResult())
                return false;
            if (ReaderAvailable && Current != null) {
                Helper = null;
                Helper = GetHelper != null ? GetHelper() :
                    (IDataMapHelper)new DbMapperDyn(Reader);
            }
            return ReaderAvailable;
        }

        public new T Current
        {
            get
            {
                // IDataRecord 
                object[] obj = base.Current;
                if (obj == null || Helper == null)
                    return default(T);

                var helper = Helper;
                T res = ConvertType(obj, helper);
                return res;
            }
        }

        public new T First
        {
            get
            {
                object[] obj = base.first;
                if (obj == null || obj.Length <= 0 || Helper == null)
                    return default(T);

                var helper = Helper;
                T res = ConvertType(obj, helper);
                return res;
            }
        }

        public virtual T ConvertType(object[] obj, IDataMapHelper helper = null)
        {
            helper = helper ?? Helper;
            if (!(helper is DbMapperDyn) && helper is IDataMapHelper<T>) {
                var helperT = helper as IDataMapHelper<T>;
                return helperT.SetValues(Reader, obj);
            }

            ExpandoObject expando = null;
            if (helper is DbMapperDyn) // IDataMapHelper<ExpandoObject>)
            {
                var helperExp = helper as DbMapperDyn; // as IDataMapHelper<ExpandoObject>;
                var fields = helperExp.Fields;
                if (fields.Count == 1 || fields.Keys.First<string>() == "")
                    expando = helperExp.SetValuesXml(this.Reader, obj);
                else
                    expando = helperExp.SetValues(this.Reader, obj);
            }

            if (expando == null) {
                return default(T);
            }
            if (typeof(T) == typeof(ExpandoObject))
                return (T)(object)expando;

            T res = (T)Convert.ChangeType(expando, typeof(T));
            return res;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return base.Current; }
        }

        public override bool MoveNext()
        {
            if (!base.MoveNext()) {
                if (RecordNumber == -1 && ReaderAvailable
                    && !Reader.HasRows && Helper != null)
                    Helper.GetFields(Reader);
                return false;
            }
            if (Helper == null && ReaderAvailable)
                Helper = new DbMapperDyn(Reader);

            if (RecordNumber <= 0)
                Helper.GetFields(Reader);

            return true;
        }

        public new IEnumerator<T> GetEnumerator()
        {
            if (Current != null && resultNum > 0)  // || base.Base != null 
                Reset();
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public class DbEnumeratorData : IFirstRecord<object[]>, IReaderNextResult, ILastError   // IDataRecord
    {
        #region Static

        public static DbEnumeratorCollect<T> SqlFillEach<T>(ISqlProc procedure, Func<object[], T> each) where T : class
        {
            procedure.LastError = null;
            var numerator = DbGetHelper.ExecFillArray(procedure);
            var list = DbEnumeratorCollect<T>.FromEach(numerator, each);

            numerator.Dispose();
            return list;
        }

        static DbEnumeratorData()
        {
            Empty = new DbEnumeratorData(null);
        }

        public static DbEnumeratorData Empty { get; private set; }
        public static DbEnumeratorData
               GetEnumerator(Func<Tuple<DbDataReader, IDbConnection>> getReader, Action<Exception> onError = null)
        {
            var numerator = new DbEnumeratorData(getReader);
            numerator.iRecord = -1;
            if (onError != null && numerator.LastError != null)
                onError(numerator.LastError);
            return numerator;
        }

        public static // IEnumerator<ExpandoObject> 
            ResultDyn GetResultDyn(Func<Tuple<DbDataReader, IDbConnection>> getReader)
        {
            var numerator = GetEnumerator(getReader);
            if (numerator == null || numerator.Reader == null)
                return ResultDyn.Empty(numerator == null ? null : numerator.LastError);

            var dynNumerator = new ResultDyn
            {
                Reader = numerator.Reader as SqlDataReader,
                Connection = numerator.Connection as SqlConnection,
                Numerator = numerator
            };
            return dynNumerator;
        }

        #endregion

        #region Properties
        public object[] First { get { return first; } }
        public int RecordNumber { get { return iRecord; } }

        public bool Any()
        {
            if (Current == null && !Prepare()) return false;
            if (Current == null && !MoveNext()) return false;
            return (Current != null);
        }

        public virtual bool Prepare()
        {
            if (first == null && !MoveNext()) {
                if (!ReaderAvailable && Reader != null)
                    Reader = null; // GetReader();
                Reset();
                if (Current == null)
                    MoveNext();
            }

            return first != null;
        }

        public DbEnumeratorData() { iRecord = -1; resultNum = -1; }
        public DbEnumeratorData(Func<Tuple<DbDataReader, IDbConnection>> getReader)
        {
            first = null;
            iRecord = -1;
            resultNum = -1;
            GetReader = getReader;
            if (GetReader != null)
                Reset();
        }

        public Func<Tuple<DbDataReader, IDbConnection>> GetReader { get; set; }
        public DbDataReader Reader { get; private set; }
        public IDbConnection Connection { get; private set; }

        public Exception LastError { get; set; }
        public bool ReaderAvailable { get { return DbCheck.IsReaderAvailable(Reader); } }

        public object[] Current
        {
            get
            {
                if (iRecord < 0 || !ReaderAvailable || !Reader.HasRows) // .RecordsAffected < 0)
                    return null;
                if (first != null && iRecord == 0 && first.Length == Reader.FieldCount)
                    return first;

                var reader = Reader;
                object[] values = (object[])Array.CreateInstance(typeof(object), reader.FieldCount);
                reader.GetValues(values);
                return values;
            }
        }

        protected int iRecord;
        protected int resultNum;
        protected object[] first;   // IDataRecord

        #endregion

        public virtual void Reset()
        {
            if (Reader != null && iRecord < 0 // && Base != null
                || iRecord == 0 && first != null
                   && first.Length == Reader.FieldCount) {
                iRecord = -1;
                return;
            }

            // Base.Reset();   // exception
            if (Reader != null || resultNum <= 0) {
                try {
                    if (Reader != null) {
                        Reader.Dispose();           // Timeout
                        if (Connection != null)
                            Connection.CloseConn();
                    }
                }
                catch { }

                Reader = null;
                try {
                    var tuple = GetReader();
                    if (tuple != null) {
                        Reader = tuple.Item1;
                        Connection = tuple.Item2;
                    }
                }
                catch (SqlException sqlEx) {
                    LastError = new Exception(
                        String.Format("Sql error {0} \n {1} line: {2}", sqlEx.Message
                              , sqlEx.Procedure, sqlEx.LineNumber)
                        , sqlEx);
                    throw LastError;
                }
                catch (Exception ex) {
                    LastError = ex;
                    throw LastError;
                }
            }

            if (resultNum < 0)
                resultNum = 0;
            if (Reader != null || resultNum == 0) {
                iRecord = -1;
                first = null;
            }
        }

        public virtual bool MoveNext()
        {
            iRecord++;
            if (iRecord == 0 && first != null && first.Length == Reader.FieldCount && Reader.HasRows) {
                // after Reset
                // iRecord++;
                return true;    // already got first record
            }

            if (!ReaderAvailable) // Base == null || !Base.MoveNext())
            {
                iRecord = -1;
                return false;
            }
            bool success = false;
            try {
                success = Reader.Read();
            }
            catch (Exception ex) { this.LastError = new Exception("Sql data server error", ex); }

            if (!success)
            {
                iRecord = -1;    // Vidine SQL klaida : string value truncated pvz...

                if (!DbCheck.IsReaderAvailable(Reader)) // .IsClosed && !Reader.NextResult()) 
                {
                    Reader.Dispose();
                    Dispose();
                }
                return false;
            }

            if (iRecord == 0)
                first = Current;
            return true;
        }

        public void Dispose()
        {
            // cashe = null;
            if (Reader != null)
                Reader.Dispose(); // Close();
            if (Connection != null)
                Connection.CloseConn();
            Reader = null;
            first = null;
        }

        #region Implement
        object IEnumerator.Current { get { return Current; } }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void System.Collections.IEnumerator.Reset()
        {
            Reset();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            if (Current != null)
                Reset();
            return this;
        }

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return this;
        //}

        #endregion

        public virtual bool NextResult()
        {
            if (!ReaderAvailable)
                return false;
            var reader = Reader;
            if (reader == null || reader.IsClosed || !reader.NextResult()) {
                if (reader != null && !reader.IsClosed)
                    reader.Dispose(); //  Close();
                return false;
            }

            // Base = null;
            resultNum++;
            first = null;
            Reset();

            if (!ReaderAvailable)
                return false;
            if (First == null) {
                MoveNext();
                Reset();
            }
            return First != null;
        }
    }

}
