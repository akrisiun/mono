using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity
{
    public class SqlProcNumerable
    {
        static SqlProcNumerable()
        {
            CommandTimeout = 5;
        }

        [Obsolete]
        public static IEnumerable<T> Exec<T>(SqlConnection conn, string sql, Action<SqlCommand> onSetup = null) where T : class
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using (var command = CreateCommand(conn, sql, onSetup))
            {
                using (SqlDataReader dataReader = command.ExecuteReader(behavior: CommandBehavior.Default))
                {
                    var results = Enumerable.Empty<T>();
                    if (dataReader.Read())
                    {
                        int iLen = dataReader.FieldCount;
                        int[] map = (int[])Array.CreateInstance(typeof(int), iLen);

                        Type type = typeof(T);
                        System.Reflection.PropertyInfo[] properties = type.GetProperties(
                               BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
                        for (int i = 0; i < properties.Length; i++)
                        {
                            for (int j = 0; j < iLen; j++)
                                if (dataReader.GetName(j).Equals(properties[i].Name))
                                    map[j] = i;
                        }

                        do
                        {
                            // PropertyInfo, typeof(T).GetConstructor()
                            T val = Activator.CreateInstance<T>();

                            object[] objVal = (object[])Array.CreateInstance(typeof(object), iLen);
                            int ret = dataReader.GetValues(objVal);
                            for (int i = 0; i < map.Length; i++)
                                if (i == 0 || map[i] > 0)
                                {
                                    PropertyInfo info = properties[map[i]];
                                    if (!objVal[i].Equals(DBNull.Value))
                                        info.SetValue(val, objVal[i], null);
                                }

                            results = results.Concat(new[] { val });
                        }
                        while (dataReader.Read());
                    }


                    return results;
                }
            }
        }

        public static int CommandTimeout { get; set; }

        private static SqlCommand CreateCommand(SqlConnection conn, string commandText, Action<SqlCommand> onSetup = null)
        {
            SqlCommand command = conn.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            if (onSetup != null)
                onSetup(command);
            
            return command;
        }

    }
}
