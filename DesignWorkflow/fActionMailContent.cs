using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace DesignWorkflow
{
    public partial class fActionMailContent : DevExpress.XtraEditors.XtraForm
    {
        public bool SendMail = false;
        public bool SendMailKH = false;
        public  string MailContent = "";
       public string MailContentKH = "";
        public fActionMailContent()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //if (tCommand.Text == "")
            //{
            //    MessageBox.Show("Lệnh rỗng");
            //}
            //else
            //{
            MailContentKH = tCustomMail.Text;
            MailContent = tStaffMail.Text;
            SendMail = ckStaff.Checked;
            SendMailKH = ckCustom.Checked;
            this.Dispose();
            //}
        }

        private void fActionCommand_Load(object sender, EventArgs e)
        {
            tCustomMail.Text = MailContentKH;
            tStaffMail.Text = MailContent;
            ckStaff.Checked = SendMail;
            ckCustom.Checked = SendMailKH;
        }
    }
}