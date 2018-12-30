using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public static class SqlProcDataTableResult
    {
        public static DataTable Result(this SqlProc proc)
        {
            var cmd = proc.CreateCommand();

            var table = new DataTable();

            if (cmd.Connection.State != ConnectionState.Open)
            {
                Guard.Check(cmd.Connection.ConnectionString.Length > 0);
                cmd.Connection.Open();
                if (cmd.Connection.State != ConnectionState.Open)
                    return null;
            }

            var reader = cmd.ExecuteReader();

            // Load(IDataReader reader, LoadOption loadOption);
            table.Load(reader, LoadOption.OverwriteChanges);

            return table;
        }

    }
}
