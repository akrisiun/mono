using Mono.Entity;
using System;

namespace Mono.Report.Excel
{
#if NPOI
    using NPOI.SS.UserModel;
#endif

    public interface IExcelSettings
    {
        bool AutoFilter { get; set; }
        bool AligmentTop { get; set; }

#if NPOI
        Action<SqlField, NPOI.SS.UserModel.ICell> onHeaderCell { get; set; }
        Action<SqlField, NPOI.SS.UserModel.ICell, object> onDataCell { get; set; }

        Action<NPOI.SS.UserModel.ISheet> onAfterHeader { get; set; }
        Action<NPOI.SS.UserModel.ISheet> onFinishSheet { get; set; }

        void OnAfterHeader(ISheet npoiSheet);
        void OnFinishSheet(ISheet npoiSheet);
#endif
    }
}
