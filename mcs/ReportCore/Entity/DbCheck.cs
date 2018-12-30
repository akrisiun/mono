using System;
using System.Data.Common;

namespace Mono.Entity
{
    public static class DbCheck
    {
        public static bool IsReaderAvailable(this DbDataReader reader, ILastError onError = null)
        {
            bool? ok = null;
            if (reader != null && !reader.IsClosed) {
                int maxCnt = 10;
                try {
                    ok = reader.FieldCount > 0;
                    if (ok.HasValue && !ok.Value) {
                        while (maxCnt > 0 && !ok.Value) {
                            ok = reader.NextResult();
                            maxCnt--;
                        }
                    }
                }
                catch (Exception ex) {
                    // Invalid attempt to call FieldCount when closed
                    // http://referencesource.microsoft.com/#System.Data/System/Data/SqlClient/SqlDataReader.cs,8dcd4c76d9376d85
                    if (onError != null)
                        onError.LastError = ex;
                }
            }
            return ok ?? false;
        }
    }

}

