using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools
{
    public partial class DonateMeFrm : Form
    {
        Point QR_Wechat_Button_Point;
        int MoveCount = 0;
        public DonateMeFrm()
        {
            InitializeComponent();
        }

        private void DonateMeFrm_Load(object sender, EventArgs e)
        {
            QR_Wechat_Button_Point = ShowWechatQR.Location;

            ShowWechatQR.MouseEnter += (s1, s2) =>
            {
                RandomButtonPoint(s1,s2);
            };
        }

        private void ShowWechatQR_Click(object sender, EventArgs e)
        {
            Bitmap TempBitMap = new Bitmap((Bitmap)Util.GetEmbedResourceObj("F6D781F44F41AAEE77BEA2A8E2F55510"));
            
            Util.PlayMp3WithStream("amagi");
            
            if(WechatDonatePicture.Image.Width!=TempBitMap.Width)
            {
                WechatDonatePicture.Image = TempBitMap;
            }
        }

        private void RandomButtonPoint(object sender,EventArgs e)
        {
            Random TempRandom = new Random();

            int ButtonWidth = (sender as Button).Width;
            int ButtonHeight = (sender as Button).Height;

            int RandomX = TempRandom.Next(0, Width- ButtonWidth);
            int RandomY = TempRandom.Next(0, Height- ButtonHeight);

            MoveCount++;

            (sender as Button).Location = new Point(RandomX, RandomY);

            if (MoveCount == 6)
            {
                (sender as Button).MouseEnter -= RandomButtonPoint;
                (sender as Button).Location = QR_Wechat_Button_Point;
                MessageBox.Show("不玩了，现在可点击加载赞赏二维码了，谢谢老板！","提示",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
                (sender as Button).MouseEnter += (s1, s2) =>
                {
                    (sender as Button).Location = QR_Wechat_Button_Point;
                };
            }

        }
    }
}
