using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace Mono.Entity.Schema
{
    using System.Collections.Generic;
    using System.Reflection;
    using IDataTable = IFirstRecord<KeyValuePair<string, SqlFieldInfo>>; // or IFirstEnumerable
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Data.Common;

    public static class Build
    {
        // IFirstRecord<SqlFieldInfo> 
        public static RecordDict<SqlFieldInfo> GetSchemaTable(this SqlDataReader reader)
        {
            if (!DbCheck.IsReaderAvailable(reader))
                return null;

            var schemaTable = new RecordDict<SqlFieldInfo>();

#if NET451 || WPF 
            DataTable  table = null;
            table = InternalBuildSchemaTable(reader);

            if (table == null || table.Rows.Count == 0)
                return schemaTable;    // empty schema

            try
            {
                foreach (DataRow prop in table.Rows)
                {
                    string Name = prop.Field<string>("ColumnName");
                    SqlFieldInfo item = FillField(null, prop);
                    schemaTable.Add(Name, item);
                }
            }
            catch (Exception ex) { schemaTable.LastError = ex; }
#endif
            return schemaTable;
        }

#if NET451 || WPF 
        // [obsolete] DataTable/DataRow
        public static SqlFieldInfo FillField(this SqlFieldInfo? field, DataRow prop)
        {
            // ColumnName, ColumnOrdinal ColumnSize, DataType, AllowDBNull DataTypeName, NumericPrecision, NumericScale

            var sqlTypeName = prop.Field<object>("DataTypeName");
            // System.RuntimeType
            var runtime = prop.Field<object>("DataType");
            Type PropertyType = runtime is string ? typeof(string)
                            : (runtime is int ? typeof(int) : runtime as Type);

            int ordinal = prop.Field<int>("ColumnOrdinal");

            SqlFieldInfo item = field.HasValue ? field.Value :
                item = new SqlFieldInfo
                {
                    SqlType = PropertyType,
                    Ordinal = ordinal,
                    MaxLength = prop.Field<int>("ColumnSize"),
                    IsNull = prop.Field<bool>("AllowDBNull"),
                    SqlTypeName = sqlTypeName as string
                };

            if (field != null)
            {
                item.SqlType = PropertyType;
                item.Ordinal = ordinal;
                item.MaxLength = prop.Field<int>("ColumnSize");
                item.IsNull = prop.Field<bool>("AllowDBNull");
                item.SqlTypeName = sqlTypeName as string;
            }

            // TODO:  if (TdsEnums.UNKNOWN_PRECISION_SCALE != col.scale)
            // schemaRow[Scale] = col.scale;
            return item;
        }

        // [obsolete] DataTable/DataRow
        public static void FillSchemaTable(this SqlDataReader reader, SqlField[] columns, ILastError errorReport = null)
        {
            if (!DbCheck.IsReaderAvailable(reader))
                return;

            var table = Build.InternalBuildSchemaTable(reader);
            if (table == null || table.Rows.Count == 0)
                return;    // empty schema

            Exception LastError = null;
            int ordinal = -1;
            try
            {

                foreach (DataRow prop in table.Rows)
                {
                    string Name = prop.Field<string>("ColumnName");
                    ordinal++;

                    SqlField item = columns[ordinal];
                    item = FillSqlField(item, prop);
                    if (!string.IsNullOrWhiteSpace(Name))
                    {
                        columns[ordinal].Name = Name;
                        columns[ordinal].Caption = Name;
                    }
                    columns[ordinal] = item;
                }
            }
            catch (Exception ex) { LastError = ex; }

            if (errorReport is ILastError)
                errorReport.LastError = LastError;

            return;
        }

        public static SqlField FillSqlField(this SqlField item, DataRow prop)
        {
            if (item.Equals(null))
                item = new SqlField();

            var sqlTypeName = prop.Field<object>("DataTypeName");
            // System.RuntimeType
            var runtime = prop.Field<object>("DataType");
            Type PropertyType = runtime is string ? typeof(string)
                            : (runtime is int ? typeof(int) : runtime as Type);

            int ordinal = prop.Field<int>("ColumnOrdinal");

            item.Type = PropertyType;
            item.Ordinal = ordinal;
            item.MaxLength = prop.Field<int>("ColumnSize");
            item.Nullable = prop.Field<bool>("AllowDBNull");
            item.SqlTypeName = sqlTypeName as string;

            item.NumericPrecision = prop.Field<short>("NumericPrecision");
            item.NumericScale = prop.Field<short>("NumericScale");

            return item;
        }

        internal static System.Data.DataTable InternalBuildSchemaTable(SqlDataReader reader)
        {
            return reader.GetSchemaTable();
        }
#endif

        #region Private routines

        private static IFirstRecord<KeyValuePair<string, SqlFieldInfo>> BuildSchemaTable(Object entry)
        {
            var schemaTable = new RecordDict<SqlFieldInfo>();
#if NET451
            var props = Enumerable.Cast<PropertyDescriptor>(
                        TypeDescriptor.GetProperties(entry));

            int ordinal = -1;
            foreach (var prop in props)
            {
                SqlFieldInfo item = new SqlFieldInfo { SqlType = prop.PropertyType, Ordinal = ++ordinal };
                schemaTable.Add(prop.Name, item);
            }
#endif
            return schemaTable;
        }

        // https://github.com/jusfr/Jusfr.Persistent/blob/master/src/Jusfr.Persistent/BulkCopyHelper.cs

//        private static DataTable BuildSchemaTable<TEntry>() where TEntry : class, new()
//        {
//            var schemaTable = new RecordDict<SqlFieldInfo>();

//#if NET451
//            var targetType = typeof(TEntry);
//            var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

//            int ordinal = -1;
//            foreach (var prop in props)
//            {
//                SqlFieldInfo item = new SqlFieldInfo { SqlType = prop.PropertyType, Ordinal = ++ordinal };
//                schemaTable.Add(prop.Name, item);
//            }
//#endif
//            return schemaTable;
//        }

        private static IDataTable BuildSchemaTable(IEnumerable<PropertyInfo> props)
        {
            var schemaTable = new RecordDict<SqlFieldInfo>();

            int ordinal = -1;
            foreach (var prop in props)
            {
                SqlFieldInfo item = new SqlFieldInfo { SqlType = prop.PropertyType, Ordinal = ++ordinal };
                schemaTable.Add(prop.Name, item);
            }
            return schemaTable;
        }

#endregion
    }
}