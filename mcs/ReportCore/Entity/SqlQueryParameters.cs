using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Mono.Entity
{

    public struct SqlQueryParameters : IDisposable
    {
        public ISqlProc proc { get; private set; }
        public IList<SqlParameter> Param { get; set; }

        public SqlDataReader DataReader { get { return NextDataReader ?? (IsPrepared() || Prepare() ? MapperReader.DataReader : null); } }

        public IDataMapHelper<object[]> Mapper { [DebuggerStepThrough] get; private set; }
        public SqlObjTableReader MapperReader { [DebuggerStepThrough]  get { return Mapper as SqlObjTableReader; } }

        public static SqlQueryParameters WithParser(ISqlProc proc, Action<SqlTableMapper, DbDataReader> propertiesParser = null)
        {
            return new SqlQueryParameters() { proc = proc, Mapper = new SqlObjTableReader(propertiesParser) };
        }

        public bool IsEmpty() { return string.IsNullOrWhiteSpace(proc.CmdText) || Mapper == null; }
        public bool IsPrepared()
        {
            return Mapper != null && Mapper.RecordNumber >= 0
                || !IsEmpty();
            // && MapperReader.DataReader != null; 
        }

        public bool Prepare(ISqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            this.proc = proc;
            if (IsEmpty()) return false;

            SqlObjTableReader mapper = Mapper as SqlObjTableReader;

            if (proc is SqlProcResult
                && (proc as SqlProcResult).Param?.Count > 0)
            {
                IList<SqlParameter> parm = (proc as SqlProcResult).Param;
                mapper.Parameters = parm; 

                if (mapper.PrepareWithParm(proc, parm, parser, commandTimeout))
                    return true;
            }
            else {
                if (mapper.Prepare(proc, parser, commandTimeout))
                    return true;
            }

            return false;
        }

        public bool Prepare()
        {
            return IsEmpty() || MapperReader != null && MapperReader.Prepare(this.proc);
        }

        public void Dispose()
        {
            if (proc == null && NextDataReader != null)
                NextDataReader.Dispose();
            if (proc != null)
                proc.Dispose();
            
            proc = null;
            Mapper = null;
            
            NextDataReader = null;
        }

        private SqlDataReader NextDataReader;
        public bool PrepareNextResult(ISqlProc proc, IDataMapHelper<object[]> nextMapper, SqlDataReader reader, Action<SqlTableMapper, DbDataReader> parser = null)
        {
            this.Mapper = nextMapper;

            this.NextDataReader = reader;
            return this.DataReader != null;
        }

        // public IEnumerable<object[]> Query()
        public IEnumerator<object[]> Query()
        {
            if (!IsPrepared())
                yield break;
            else if (MapperReader != null)
            {
                var querySource = MapperReader;
                foreach (var item in querySource.Query())
                    yield return item;

                Mapper.Dispose();
            }
            else if (Mapper is SqlObjTable)
            {
                var numerator = Mapper as SqlObjTable;

                while (numerator.MoveNext())
                {
                    yield return numerator.Current;
                }
            }
        }
    }
}
