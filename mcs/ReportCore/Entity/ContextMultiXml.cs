using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Mono.Entity
{
    public static class ContextMultiXml
    {
#if NET451 || NETCORE // 
        public static MultiResult<XElement> MultiXElem(this Context db, object sqlProcNamed)
        {
            var proc = SqlProcExt.ProcNamed(sqlProcNamed);
            proc.Context = db;
            return MultiXElem(proc);
        }

        // Sql data set to multiple XML
        public static MultiResult<XElement> MultiXElem(this SqlProc proc)
        {
            var result = new MultiResult<XElement>();
            return result.Prepare(proc);
        }
#endif

    }
}
