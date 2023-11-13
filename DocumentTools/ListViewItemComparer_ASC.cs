using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace DocumentTools
{
    internal class ListViewItemComparer_ASC : IComparer
    {
        public int Compare(object x, object y)
        {
            System.Windows.Forms.ListViewItem listViewItemX = (System.Windows.Forms.ListViewItem)x;
            System.Windows.Forms.ListViewItem listViewItemY = (System.Windows.Forms.ListViewItem)y;

            string textX = Path.GetFileNameWithoutExtension(listViewItemX.Text);
            string textY = Path.GetFileNameWithoutExtension(listViewItemY.Text);

            // 使用正则表达式提取数字
            Match matchX = Regex.Match(textX, @"\d+");
            Match matchY = Regex.Match(textY, @"\d+");

            if (matchX.Success && matchY.Success)
            {
                int fileNumberX = int.Parse(matchX.Value);
                int fileNumberY = int.Parse(matchY.Value);

                return fileNumberX.CompareTo(fileNumberY);
            }
            else
            {
                // 处理文本中不包含数字的情况，例如按照文本的字母顺序排序
                return textX.CompareTo(textY);
            }
        }

    }
}
