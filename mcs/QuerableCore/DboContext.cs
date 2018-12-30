
using System;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using IQToolkit.Data.SqlClient;
using System.Data.SqlClient;
using IQToolkit;

namespace Mono.Entity
{
    /// <summary>
    /// Simple querable
    /// </summary>
    public class DboContext : ILastError, IDisposable, IQuery, IEnumerable
    {
        public static DboContext OpenDbo(SqlConnection mssql, string dbname) {

            mssql.ChangeDatabase(dbname);

            var dbo = new DboContext(new SqlQueryProvider(mssql, null));
            return dbo;
        }

        public DboContext(SqlQueryProvider provider) {
            Provider = provider;
        }

        public SqlQueryProvider Provider { [DebuggerStepThrough] get; set; }
        IQueryProvider IQuery.Provider { get => Provider; }

        public Exception LastError { [DebuggerStepThrough] get; set; }
        public Expression Expression { [DebuggerStepThrough] get; protected set; }
        public object[] ParamValues { [DebuggerStepThrough] get; set; }

        public void Dispose() {
            Provider?.Connection?.Dispose();
            Provider = null;
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Provider.Execute(this.Expression)).GetEnumerator();

        public IEntityTable<T> Table<T>(string from) => Provider.GetTable<T>(from ?? typeof(T).Name);
    }

    /// <summary>
    /// Querable Element with Type
    /// </summary>
    public class DboContext<T> : DboContext, IQueryable, IQuery
    {
        public DboContext(SqlQueryProvider provider) : base(provider) { }
        public DboContext(SqlConnection conn) : base(new SqlQueryProvider(conn, typeof(T))) { }

        public Type ElementType { get => typeof(T); }
        IQueryProvider IQueryable.Provider { get => Provider; }

        public IEntityTable<T> GetTable(string from) => Table<T>(from);

        public override string ToString() => Provider?.GetQueryText(this.Expression) ?? this.Expression.ToString();
    }
  
}