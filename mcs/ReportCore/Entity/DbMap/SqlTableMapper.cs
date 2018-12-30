using System;
using System.Data.Common;

namespace Mono.Entity
{
    public class SqlTableMapper : IDataMapHelper<object[]>
    {
        public SqlTableMapper(Action<SqlTableMapper, DbDataReader> propertiesParser)
        {
            this.propertiesParser = propertiesParser;
        }

        public virtual void Dispose() { FieldNames = null; iLen = 0; }

        protected Action<SqlTableMapper, DbDataReader> propertiesParser;

        public Type Type { get { return typeof(object[]); } }
        public int FieldCount { get { return iLen; } }
        public int RecordNumber { get; protected set; }

        #region GetProperties part

        int iLen;
        string[] FieldNames;
        DbDataReader IDataMapHelper<object[]>.Reader { get { return null; } }

        public IDataMapHelper<object[]> // IDataMapHelper<object[]>.
               GetProperties(DbDataReader dataReader, Action<Exception> onError = null)
        {
            RecordNumber = 0;

            iLen = dataReader.IsClosed ? 0 : dataReader.FieldCount;
            FieldNames = new string[iLen];
            for (int i = 0; i < iLen; i++)
                FieldNames[i] = dataReader.GetName(i);

            if (!dataReader.IsClosed && propertiesParser != null)
                propertiesParser(this, dataReader);
            return this;
        }

        #endregion

        #region Array values

        public virtual object[] SetValues(DbDataReader dataReader, object[] objVal)
        {
            // (dataReader as SqlDataReader)
            dataReader.GetValues(objVal);
            return objVal;
        }

        public object[] DbRecordArray()
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public object[] DbRecordArray(int len)
        {
            return (object[])Array.CreateInstance(typeof(object), len);
        }

        #endregion

        #region Fields

        public virtual int? GetOrdinal(string columnName)
        {
            for (int i = 0; i < FieldNames.Length; i++)
                if (FieldNames[i] == columnName)
                    return i;
            return null;
        }

        public virtual object GetField(string columnName, object[] arrayItem)
        {
            int? find = GetOrdinal(columnName);
            return find.HasValue ? arrayItem[find.Value] != DBNull.Value ? arrayItem[find.Value] : null
                                 : null;
        }

        public virtual SqlField[] GetFields(DbDataReader dataReader)
        {
            return null;
        }

        #endregion

    }

}
