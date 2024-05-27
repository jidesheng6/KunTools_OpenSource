using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// 查找和替换区域的控件Y轴高度起始值（公用）
        /// </summary>
        public static int WORDFINDREPLACE_FINDREPLACE_LOCATION_Y = 0;
        /// <summary>
        /// 查找按钮动态生成的名称集合
        /// </summary>
        public static Dictionary<string, string> WORDFINDREPLACE_ADD_FIND_CONTROL_LIST = new Dictionary<string, string>();

        #endregion

        #region EXCEL内容批量替换部分动态添加的控件参数值
        public static string EXCELFINDREPLACE_FIND_NAME_PREFIX = "ExcelReplaceFindTextBox_";

        public static string EXCELFINDREPLACE_REPLACE_NAME_PREFIX = "ExcelReplaceTextBox_";

        public static int EXCELFINDREPLACE_NAME_INDEX = 0;

        public static int EXCELFINDREPLACE_LOCATION_Y = 0;

        public static Dictionary<string, string> EXCELFINDREPLACE_ADD_CONTROL_LIST = new Dictionary<string, string>();

        #endregion
    }
}
