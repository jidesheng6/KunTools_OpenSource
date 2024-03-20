using System.Windows.Forms;

namespace DocumentTools
{
    public partial class IPAddressInputTextBox : TextBox
    {
        public IPAddressInputTextBox()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassName = "SysIPAddress32";
                return cp;
            }
        }
    }
}
