using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Mono.Reflection;

namespace Mono.Entity
{
    public static class SqlProcExt
    {
        public static SqlProcResult CmdText(string cmdText, Context context = null, Action<SqlProc> setup = null)
        {
            var proc = new SqlProcResult()
            {
                CmdText = cmdText,
                Context = context,
                Connection = context == null ? null : context.SqlConnection,
                Param = null
            };
            if (proc.Connection == null)
                proc.CloseOnDispose = false;

            setup?.Invoke(proc);
            return proc;
        }

        public static SqlProcResult WithParam(this SqlProcResult proc, string name, object value)
        {
            if (proc.Param == null)
                proc.Param = new List<SqlParameter>() { SqlProc.AddWithValue(name, value) };
            else
                if (value != null)
                proc.Param.Add(SqlProc.AddWithValue(name, value));

            return proc;
        }

        public static SqlProcResult WithParam(this SqlProcResult proc, object namedParam)
        {
            if (namedParam != null)
                proc.Param = NameProperties.Parse(namedParam);

            return proc;
        }

        public static SqlProc ProcParamNamed(string procName, object sqlNamedParam = null, Context db = null)
        {
            string cmdText = procName;
            Mono.Guard.Check(cmdText.Length > 4);
            var proc = SqlProcExt.CmdText(cmdText);

            if (sqlNamedParam != null)
            {
                var properties = new NameProperties(sqlNamedParam);
                if (properties.List.Count > 0)
                {
                    var listParam = new List<SqlParameter>();
                    foreach (string itemName in properties.Names(0))
                    {
                        var val = Utils.GetPropertyValue(sqlNamedParam, itemName);
                        if (val != null)
                            listParam.Add(SqlProc.AddWithValue("@" + itemName, val));
                    }
                    proc.Param = listParam;
                }
            }

            proc.Context = db;
            return proc;
        }

        public static SqlProc ProcNamed(object namedParam, Context db = null)
        {
            var properties = new NameProperties(namedParam);
            if (properties.List.Count == 0)
                return null;
            string cmdText = properties.GetValue(namedParam, properties.FirstName()) as string;
            Mono.Guard.Check(cmdText.Length > 4);

            var proc = SqlProcExt.CmdText(cmdText);
            proc.Param = SqlProcExt.ListParam(proc, namedParam, properties);
            proc.Context = db;

            return proc;
        }

        public static SqlParameter AddWithType(this SqlCommand cmd, SqlDbType type, string name, object value)
        {
            Guard.Check(name.StartsWith("@"));
            var tParam = cmd.Parameters.Add(name, type, 0);
            tParam.Value = value;
            return tParam;
        }

        internal static List<SqlParameter> ListParam(ISqlProc proc, object namedParam, NameProperties properties)
        {
            var listParam = new List<SqlParameter>();
            foreach (string itemName in properties.Names(1))
            {
                var val = Utils.GetPropertyValue(namedParam, itemName);
                if (val != null)
                    listParam.Add(SqlProc.AddWithValue("@" + itemName, val));
            }

            return listParam;
        }

        public static IEnumerable<string> Extract(this IEnumerable<SqlParameter> paramList)
        {
            if (paramList == null)
                yield break;

            var numer = paramList.GetEnumerator();
            int iNum = 0;
            while (numer.MoveNext())
                yield return 
                    ((++iNum) > 1 ? ", " : string.Empty)
                    + numer.Current.ParameterName + " = \'" + (numer.Current.Value ?? "") + "\'";
        }

    }
}