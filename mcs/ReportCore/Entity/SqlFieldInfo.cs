using System;

namespace Mono.Entity
{

#if XLSX && !WEB && !SNEX
    using Mono.Internal;
#endif

    public struct SqlFieldInfo
    {
        public int Ordinal;
        public Type SqlType;
        public int? MaxLength;
        public bool? IsNull;
        public string SqlTypeName;
        // TODO: NumericPrecision, NumericScale

        public override string ToString()
        {
            return String.Format("{0}{1}{2}", SqlTypeName ?? SqlType.ToString().Replace("System.", ""),
                SqlType == typeof(int) && MaxLength == 4 ? null : "(" + MaxLength.ToStringIfNull(null) + ")",
                (IsNull ?? false) ? " NULL" : string.Empty);
        }
    }

}