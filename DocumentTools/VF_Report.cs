using System.Text.RegularExpressions;

namespace DocumentTools
{
    class VF_Report
    {
        internal static string QueryContent(string ReportId) => @$"SELECT Content FROM Report_Detail WHERE Kind = 'GridFormatData' AND Content LIKE 'Bool:%vftab%' And ReportID={ReportId}";

        internal static string QueryNumberExamContent(string ReportId) => @$"SELECT Content FROM Report_Detail WHERE Kind = 'GridFormatText' AND Content LIKE '999%' And ReportID={ReportId}";

        //项目名称
        internal static Regex ExamItemFormat_Regex { get; set; } = new Regex(@"Item=([^&]*)");//Item=总体评价

        //数据类型
        internal static Regex KindFormat_Regex { get; set; } = new Regex(@"Kind=([^&]*)");//Kind=该人得票数

        //小数点位数
        internal static Regex NumberPoint_Regex { get; set; } = new Regex(@"(?<=ReserveDec=)([^&]+)");//ReserveDec=0

        //小数点的显示文本
        internal static Regex NumberPointFormat_Regex { get; set; } = new Regex(@"999(?:\.\d+)?%?");//

        //测评主体
        internal static Regex RaidFormat_Regex { get; set; } = new Regex(@"Raid=([^&]*)");//Raid=A，B，C
    }
}
