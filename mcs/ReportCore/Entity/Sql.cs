using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace Mono.Entity
{
    public static class SqlEnum
    {
        // static SqlConnection Connect()

        [Obsolete]
        public static IEnumerable<T> ExecExtract<T>(string proc, Action<SqlCommand> setParam, Func<SqlDataReader, T> extract)
        {
            using (var cmd = new SqlCommand()
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = proc
            })
            {
                setParam(cmd);
                if (cmd.Connection == null)
                    yield break;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        yield return extract(reader);

                    reader.Close();
                }   
            }
        }
    }
}