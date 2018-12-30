using Mono.Report;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Mono.Entity
{
    // [Obsolete]
#if WEB || NET451
    public class SqlObjTableData : SqlObjTable, IData, IListSource
#else
    public class SqlObjTableData : SqlObjTable, IData
#endif
    {
        public SqlObjTableData()
        {
            Data = new Collection<object[]>();
        }

        public ICollection<object[]> Data;

        public void Add(object[] item)
        {
            Data.Add(item);
        }

        public new IEnumerable<object[]> Array { get { return Data; } }
        public override bool RowsAny { get { return Data.Count > 0; } }
        public override IEnumerator<object[]> GetEnumerator()
        {
            if (Data.Count > 0)
                return Data.GetEnumerator();

            return base.GetEnumerator();
        }

        public override void Dispose()
        {
            if (Data != null && Data is IDisposable)
                (Data as IDisposable).Dispose();
            Data = null;
            base.Dispose();
        }

    }

    public class SqlObjTable : IEnumerable<object[]>, IFirstRecord<object[]>, IDataMapHelper<object[]>, IData, System.ComponentModel.IListSource, IDisposable
    {
        public static SqlObjTable ExecCmd(SqlCommand cmd)
        {
            var table = new SqlObjTable();
            table.RecordNumber = -1;

            var proc = new SqlProc() { CmdText = cmd.CommandText, Context = null, Connection = cmd.Connection };
            proc.Param = SqlProc.CloneParam(cmd.Parameters);

            table.queryParam = SqlQueryParameters.WithParser(proc, propertiesParser: null);
            if (!table.ExecProc(proc))
                return null;    // error
            return table;
        }

        public SqlField[] GetFields(DbDataReader dataReader) { return SqlFieldArray.GetArray(dataReader); }

        public bool ExecNamed(Context context, object paramNamed, int iFrom = 0, int iTake = 0)
        {
            NameProperties prop = new NameProperties(paramNamed);

            ISqlProcReader proc = null;

            string execName = prop.GetValue(paramNamed, prop.FirstName()) as string;
            proc = SqlProcExt.CmdText(execName, context);
            if (prop.List.Count > 1)
                proc.Param = NameProperties.Parse(paramNamed, 1);

            return ExecProc(proc, iFrom, iTake);
        }

        public Func<SqlDataReader> GetReader { get; set; }

        public bool GetNextResult()
        {
            SqlDataReader reader = Reader;
            //if (Proc is ISqlProcReader)
            //    reader = (Proc as ISqlProcReader).Reader as SqlDataReader;

            if (reader == null)
                reader = GetReader == null ? null : GetReader();
            if (reader == null || !reader.NextResult())
            {
                if (!reader.IsClosed)
#if NETCORE
                        reader.Close(); // .Close();
#else
                        reader.Dispose();
#endif

                // #endif
                return false;
            }

            this.Fields = null;
            this.queryParam.PrepareNextResult(this.Proc, null, this.Reader);
            // check Mapper == null
            Reset();

            SqlObjTableReader.GetFirstRecord(this, reader);

            this.queryParam.PrepareNextResult(this.Proc, this, reader);
            querySource = null;

            return true;
        }

        #region ctor

        public SqlDataReader Reader {
            get {
                return queryParam.DataReader != null ? queryParam.DataReader
                    : (Proc != null && Proc is ISqlProcReader) ? (Proc as ISqlProcReader).Reader as SqlDataReader
                    : null;
            }
        }

        public ISqlProc Proc { [DebuggerStepThrough] get; set; }
        public ISqlProcReader ProcResult { [DebuggerStepThrough] get { return Proc as ISqlProcReader; } }

        public bool IsDisposeProc { get; set; }

        public SqlField[] Fields { [DebuggerStepThrough] get; set; }

        // private IQueryable<object[]> 
        private IDisposable querySource;
        private Mono.Entity.SqlQueryParameters queryParam;

        public virtual bool RowsAny {
            get {
                if (queryParam.IsEmpty()
                    || queryParam.DataReader == null)
                    return false;
                if (!queryParam.DataReader.HasRows)
                    return false;

                return true;
            }
        }

        public virtual IEnumerable<object[]> Rows {
            get {
                if (queryParam.IsEmpty())
                    return Enumerable.Empty<object[]>();        // error
                return this as IEnumerable<object[]>;
            }
        }

        // IList<object[]> Rows

        public SqlObjTable()
        {
            Fields = new SqlField[] { };
            RecordNumber = -1;

            queryParam = SqlQueryParameters.WithParser(
                new SqlProc() { CmdText = String.Empty, Context = null }, null);

            // IList<SqlParameter> Param { get; set; }
            queryParam.Param = new List<SqlParameter>();
            // TODO: Param

            // querySource = Enumerable.Empty<object[]>().AsQueryable();
        }

        public virtual IEnumerable<SqlField> FieldsVisible {
            get {
                int index = 0;
                foreach (var field in Fields)
                {
                    if (!field.Hide)
                    {
                        if (field.OrdinalVisible != index)
                        {
                            SqlField fieldModi = field;
                            // modify struct data
                            fieldModi.OrdinalVisible = index;
                            index++;
                            yield return fieldModi;
                        }
                        else
                        {
                            index++;
                            yield return field;
                        }
                    }
                }
            }
        }



        public Exception LastError { get; set; }
        public virtual void Dispose()
        {
            if (querySource is IDisposable)
                (querySource as IDisposable).Dispose();

            querySource = null;
            Fields = null;
            queryParam.Dispose();
        }

        #endregion

        #region IEnumerable

        public virtual IEnumerator<object[]> GetEnumerator()
        {
            var reader = queryParam.DataReader;
            if (reader == null)
                Prepare();

            var query = this.Query; // this.queryParam.Query();
            this.querySource = query;

            //foreach (object[] rowItem in query)
            //    yield return rowItem;

            while (query.MoveNext())
                yield return query.Current;

            querySource = null;
            (query as IDisposable).Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public IEnumerable<object[]> Array // IData.Array
        {
            get {
                var query = this.queryParam.Query();
                // foreach (object[] rowItem in query)

                query.Reset();

                while (query.MoveNext())
                    yield return query.Current;

                (query as IDisposable).Dispose();
            }
        }

        // public IQueryable<object[]> Query

        public Mono.Entity.SqlQueryParameters QueryParam {
            get => queryParam;
            set { queryParam = value; } 
        }

        public IEnumerator<object[]> Query {
            get {
                if (!queryParam.IsPrepared())
                    queryParam.Prepare(queryParam.proc);

                var numerator = queryParam.Query();

                return numerator;
            }
        }

        public int Timeout { get; set; }

#region Sql

        public bool IsClosed()
        {
            return Fields.Length == 0 || queryParam.proc.Connection == null;
        }

        public bool Reset()
        {
            var proc = this.Proc;
            var queryParam = this.queryParam;
            this.RecordNumber = -1;

            if (Fields == null || this.Fields.Length == 0)
                return false;

            this.querySource = this.Query;
            return this.querySource != null;
        }

#if NET451
        public bool Exec(Context context, string[] param, int iFrom = 0, int iTake = 0)
        {
            string execName = param[0];

            var proc = SqlProcExt.CmdText(execName, context);

            return ExecProc(proc, iFrom, iTake);
        }

        public bool ExecNamed(Context context, object namedParam)
        {
            // NameProperties prop = new NameProperties(namedParam);
            var proc = SqlProcExt.ProcNamed(namedParam);
            proc.Context = context;

            return ExecProc(proc);
        }
#endif

        private bool ExecProc(ISqlProc proc, int iFrom = 0, int iTake = 0)
        {
            Mono.Guard.Check(proc.CmdText.Length > 0, "SqlTable proc error");
            Mono.Guard.Check(Fields != null, "SqlTable Fields error");

            var fld = this.Fields;

#pragma warning disable IDE0039  // Use local function

            Action<SqlTableMapper, DbDataReader> parser = (map, dbReader) =>
            {
                fld = SqlFieldArray.GetArray(dbReader);
                this.Fields = fld;
            };

            this.Proc = proc;

            bool init = this.queryParam.Prepare(proc, parser, this.Timeout);

            if (queryParam.MapperReader != null)
            {
                this.RecordNumber = queryParam.Mapper.RecordNumber;
                this.First = queryParam.MapperReader.LastRow;
            }

            // SqlObjTableReader.GetFirstRecord(this, dataReader);
            // Guard.Check(fld.Length > 0, "fld.Length error");

            return init;
        }

#endregion

        public int RecordNumber { get; protected set; }
        public int? FieldCount;

        public IDataMapHelper<object[]> GetProperties(DbDataReader dataReader, Action<Exception> onDublicateField)
        {
            if (RecordNumber <= -1)
                RecordNumber = 0;

            FieldCount = dataReader.FieldCount;
            return this;
        }

        public Type Type { get { return typeof(object[]); } }
        DbDataReader IDataMapHelper<object[]>.Reader { get { return this.Reader; } }

        public readonly static object[] EmptyArray = new object[] { };

        public object[] SetValues(DbDataReader dataReader, object[] objVal)
        {
            if (dataReader.FieldCount == 0)
                return EmptyArray; // objVal;

            if (objVal.Length < dataReader.FieldCount)
                global::System.Array.Resize(ref objVal, dataReader.FieldCount);

            dataReader.GetValues(objVal);

            return objVal;
        }

        public SqlField[] GetFieldsSql(SqlDataReader dataReader) { return this.GetFields(dataReader as IDataReader); }

        public virtual SqlField[] GetFields(IDataReader dataReader)
        {
            if (dataReader is DbDataReader)
                return SqlFieldArray.GetArray(dataReader as DbDataReader); // no: onDublicateField : null);    

            return new SqlField[] { }; // empty array
        }

        public object[] DbRecordArray() { return DbRecordArray(FieldCount ?? this.Reader.FieldCount); }
        public object[] DbRecordArray(int len)
        {
            return (object[])global::System.Array.CreateInstance(typeof(object), len);
        }


        public object[] First { get; set; }

        public IListSource PreparedSource() { return Prepare() ? this : null; }
        public bool Prepare()
        {
            if (RecordNumber >= 0 && First != null) return true;

            if (!Reset() && this.queryParam.proc == null) return false;

            var reader = Reader;
            if (reader == null || reader.IsClosed)
                return false;

            SqlObjTableReader.GetFirstRecord(this, reader);
            if (First != null)
                Fields = GetFields(reader);

            return Any();
        }

        public bool Any() { return RecordNumber >= 0; }

        public object[] Current { get; protected set; }

        object IEnumerator.Current { get { return this.Current; } }

        public bool MoveNext()
        {
            if (First != null && RecordNumber == 0 && Current == null)
            {
                Current = First;
                return true;
            }

            var dataReader = this.Reader;
            if (dataReader != null && dataReader.IsClosed)
            {
                return false;
            }

            if (!dataReader.Read())
            {
                if (!dataReader.IsClosed)
                    dataReader.Dispose(); // Close();
                return false;
            }

            object[] objVal = DbRecordArray();

            dataReader.GetValues(objVal);
            RecordNumber++;
            Current = objVal;

            if (RecordNumber == 0)
                First = Current;

            return true;
        }

        void IEnumerator.Reset() { Reset(); }

#if WEB || NET451 || NET40CL
        bool IListSource.ContainsListCollection { get { return this.RowsAny; } }
        IList IListSource.GetList() { return System.Linq.Enumerable.ToList<object[]>(this); }
#endif

        public bool ContainsListCollection { get { return this.RowsAny; } }
        public IList GetList() { return System.Linq.Enumerable.ToList<object[]>(this); }

    }

}

namespace System.ComponentModel
{

//#if NET40CL // NET451
//    public interface IListSource
//    {
//        bool ContainsListCollection { get; }

//        IList GetList();
//    }
//#endif

}