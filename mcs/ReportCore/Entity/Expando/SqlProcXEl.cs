using System;
using System.Data.SqlClient;
using System.Dynamic;
using System.Xml.Linq;
using Mono.XHtml;

namespace Mono.Entity
{
    public static class SqlProcXEl
    {
        public static SqlProcText ProcDb(this Context db, string cmdText)
        {
            if (db == null)
                return null;

            db.LastError = null;
            db.AssureOpen(true);
            return new SqlProcText { CmdText = cmdText, Context = db, Connection = db.SqlConnection };
        }

        public static ExpandoObject ExecDynFirst(this ISqlProcContext proc)
        {
            ExpandoObject first = null;
            SqlDataReader reader = null;
            DbEnumeratorData<ExpandoObject> numerator = 
                SqlMultiDyn.ResultDyn(proc, (r) => reader = r);

            numerator.Prepare();
            first = numerator.First;

            if (proc.Context != null)
                proc.Context.LastError = numerator.LastError;

            return first;
        }

        public static XElement ExecDynFirstXml(this ISqlProcContext proc, string field = "xml")
        {
            var first = ExecDynFirst(proc);
            if (first == null || !ExpandoUtils.ContainsKey(first, field))
                return EmptyXEl;

            string xml = ExpandoUtils.ValObj<string>(first, field);
            proc.LastError = null;
            XElement result = XEl.AsElement(xml, proc as ILastError);
            if (proc.LastError != null && proc.Context != null) 
                proc.Context.LastError = proc.LastError;

            return result ?? EmptyXEl;
        }

        public static readonly XElement EmptyXEl = XElement.Parse("<Empty />");
    }
}
