using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;
namespace CusPOS
{
    public partial class fThanhtoan : DevExpress.XtraEditors.XtraForm
    {
        Database _db = Database.NewDataDatabase();
        DataTable dmtk;
        public int returnValue = -1;
        public string tk = "";
        public string makh="";
        public bool layHD = false;
        public fThanhtoan()
        {
            InitializeComponent();
            string sql;
            sql = "select TK,TenTK from dmTK where KT like '112%'";
            dmtk = _db.GetDataTable(sql);
            gridLookUpEdit1.Properties.DataSource = dmtk;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (radioGroup1.SelectedIndex == 0)
            {
                returnValue = 0;
                this.Dispose();
            }
            else
            {
                if (gridLookUpEdit1.EditValue != null)
                {
                    tk = gridLookUpEdit1.EditValue.ToString();
                    this.returnValue = 1;
                    this.Dispose();
                }
                else
                {
                    MessageBox.Show("Chưa chọn phòng");
                }
            }
        }

        private void fThanhtoan_Load(object sender, EventArgs e)
        {

        }
    }
}