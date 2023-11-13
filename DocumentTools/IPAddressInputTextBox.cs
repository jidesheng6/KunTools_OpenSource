using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools
{
    public partial class IPAddressInputTextBox : TextBox
    {
        public IPAddressInputTextBox()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams { 
            get { 
                CreateParams cp = base.CreateParams;
                cp.ClassName = "SysIPAddress32";
                return cp; 
            }
        }
    }
}
