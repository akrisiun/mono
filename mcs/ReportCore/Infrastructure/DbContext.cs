using Mono.Entity;
using System.Data;
using System.Data.SqlClient;

namespace Mono.Infrastructure
{
    public class DbContext : Context
    {
        public const string DefaultCatalog = "master";

        public static DbContext AsDbContext(ISqlContext db, string connKey = null)
        {
            return db as DbContext ?? new DbContext(db.Connection, connKey ?? Context.LastConnKey);
        }

#if WEB || NET45
        public static DbContext OpenWithConnKeyDb(string connKey, string initialCatalog = null)
        {
            Context.LastConnKey = connKey;

            var str = System.Configuration.ConfigurationManager
                .ConnectionStrings[connKey].ConnectionString;
            return OpenWithConnStrCatalog(connKey, str, initialCatalog);
        }
#endif

        public static DbContext OpenWithConnStrCatalog(string connKey, string connStr, string initialCatalog = null)
        {
            DbContext.LastConnKey = connKey;

            var conn = new SqlConnection(connStr);
            if (!string.IsNullOrWhiteSpace(initialCatalog) || InitialCatalog.Equals(conn.Database))
                conn.ChangeDatabase(initialCatalog);

            var db = new DbContext(conn, connKey);
            return db;
        }

        public static object ParseName(object nameOrConnection, string connKey)
        {
            if (!string.IsNullOrWhiteSpace(connKey))
                return nameOrConnection ?? new SqlConnection();

            if (nameOrConnection != null && nameOrConnection is string)
            {
                var str = nameOrConnection as string;
                if (str.StartsWith("name="))
                    return str.Substring(5) as object;
            }
            return nameOrConnection;
        }

        public DbContext(object nameOrConnection, string connKey)
            : base(ParseName(nameOrConnection, connKey) as string, connKey)
        { }

        public DbContext(IDbConnection connection, string connKey)
            : base(connection, connKey)
        { }

        protected virtual void OnModelCreating(IDbModelBuilder modelBuilder) { }
    }
}

/*
namespace System.Data.Linq.Mapping
{
    //     Represents a source for mapping information.
    public abstract class MappingSource
    {
        //     The meta-model created to match the current mapping scheme.
        protected abstract MetaModel CreateModel(Type dataContextType);

        //   dataContextType:
        //     The type of System.Data.Linq.DataContext of the model to be returned.
        //     The mapping model associated with this mapping source.
        public MetaModel GetModel(Type dataContextType);

*/
