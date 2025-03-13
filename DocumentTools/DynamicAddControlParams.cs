using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DocumentTools
{
    internal class DynamicAddControlParams
    {
        #region WORD内容批量替换部分动态添加的控件参数值
        /// <summary>
        /// 查找区域的控件名称前缀
        /// </summary>
        public static string WORDFINDREPLACE_FIND_NAME_PREFIX = "WordReplaceFindTextBox_";
        /// <summary>
        /// 替换区域的控件名称前缀
        /// </summary>
        public static string WORDFINDREPLACE_REPLACE_NAME_PREFIX = "WordReplaceTextBox_";
        /// <summary>
        /// 查找区域的控件名称前缀索引初始值
        /// </summary>
        public static int WORDFINDREPLACE_FIND_NAME_INDEX = 0;

        /// <summary>
        /// 动态添加的查找按钮控件
        /// </summary>
        public static List<Control> WordFindReplace_FindControl_Lists = new List<Control>();

        /// <summary>
        /// 动态添加的替换按钮控件
        /// </summary>
        public static List<Control> WordFindReplace_ReplaceControl_Lists = new List<Control>();


        #endregion

        #region EXCEL内容批量替换部分动态添加的控件参数值
        public static string EXCELFINDREPLACE_FIND_NAME_PREFIX = "ExcelReplaceFindTextBox_";

        public static string EXCELFINDREPLACE_REPLACE_NAME_PREFIX = "ExcelReplaceTextBox_";

        public static int EXCELFINDREPLACE_NAME_INDEX = 0;

        /// <summary>
        /// 动态添加的查找按钮控件
        /// </summary>
        public static List<Control> ExcelFindReplace_FindControl_Lists = new List<Control>();

        /// <summary>
        /// 动态添加的替换按钮控件
        /// </summary>
        public static List<Control> ExcelFindReplace_ReplaceControl_Lists = new List<Control>();
        #endregion

        #region 插入考评表项目和选项部分动态添加的控件参数值
        /// <summary>
        /// 插入项目名称的控件名称前缀
        /// </summary>
        public static string InsertExam_ExamName_Prefix = "InsertExamNameBox_";
        /// <summary>
        /// 插入项目得分的控件名称前缀
        /// </summary>
        public static string InsertExam_ExamScore_Prefix = "InsertExamScoreBox_";
        /// <summary>
        /// 插入项目和得分的控件名称前缀索引初始值
        /// </summary>
        public static int InsertExam_Control_Index = 0;
        /// <summary>
        /// 插入项目和得分的控件Y轴高度起始值（公用）
        /// </summary>
        public static int InsertExam_Control_LocationY = 0;

        public static int TempBaseInsertItemLocationY = 0;

        public static List<Control> InsertExam_ExamNameControl_Lists = new List<Control>();

        public static List<Control> InsertExam_ExamScoreControl_Lists = new List<Control>();

        /// <summary>
        /// 插入项目选项的控件名称前缀
        /// </summary>
        public static string InsertExam_ExamSelector_Prefix = "InsertExamSelectorBox_";

        /// <summary>
        /// 插入项目选项得分比重的控件名称前缀
        /// </summary>
        public static string InsertExam_ExamSelectorPer_Prefix = "InsertExamSelectorPerCentBox_";

        /// <summary>
        /// 插入项目选项是否计入得票率的控件名称前缀
        /// </summary>
        public static string InsertExam_ExamSelectorAvg_Prefix = "InsertExamSelectorAvgBox_";

        /// <summary>
        /// 插入项目和选项的控件名称前缀索引初始值
        /// </summary>
        public static int InsertExamSelector_Control_Index = 0;

        /// <summary>
        /// 选项名称的控件Name集合
        /// </summary>
        public static List<Control> InsertExam_ExamSelectorControl_Lists = new List<Control>();

        /// <summary>
        /// 选项得分比重的控件Name集合
        /// </summary>
        public static List<Control> InsertExam_ExamSelectorPerControl_Lists = new List<Control>();

        /// <summary>
        /// 选项计入得票率的控件Name集合
        /// </summary>
        public static List<Control> InsertExam_ExamSelectorAvgControl_Lists = new List<Control>();

        public static int TempBaseInsertItemSelectorLocationY = 0;

        #endregion
    }
}
