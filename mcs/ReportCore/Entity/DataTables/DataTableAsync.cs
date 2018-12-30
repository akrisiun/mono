using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Entity;

namespace Mono.Entity.DataTables
{
    //public static class DataTableAsync
    //{
    //    public static async Task<DataTable> ExecuteAsync(this Context context
    //            , SqlProc procedure, bool parseError = true)
    //    {
    //        if (procedure.Connection == null)
    //        {
    //            procedure.Connection = context.SqlConnection;
    //            if (procedure.Connection == null)
    //                return null;
    //        }

    //        Trace.WriteLine("SqlProc: " + procedure.CmdText);

    //        if (procedure.Connection.State != ConnectionState.Open)
    //        {
    //            Task<object> task = ContextAsync.OpenAsync(context, procedure.Connection); // , CancellationToken.None);
    //            await task;
    //            if (task.Exception != null)
    //            {
    //                var tcs = new TaskCompletionSource<DataTable>();
    //                tcs.SetException(task.Exception);
    //                context.LastError = task.Exception;
    //                if (!parseError)
    //                    throw task.Exception;

    //                return await tcs.Task;
    //            }
    //        }

    //        if (!parseError)
    //            return await Task.Factory.StartNew(() =>
    //            {
    //                return procedure.Result();
    //            });

    //        try
    //        {
    //            return await Task.Factory.StartNew(() =>
    //            {
    //                return procedure.Result();
    //            });
    //        }
    //        catch (Exception ex)
    //        { context.LastError = ex; }
    //        return null;
    //    }

    //}
 
}
