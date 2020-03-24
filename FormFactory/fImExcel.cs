using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTControl;
using CDTDatabase;
using CDTLib;
namespace FormFactory
{
    public partial class fImExcel : DevExpress.XtraEditors.XtraForm
    {
        public fImExcel(string systableid)
        {
            InitializeComponent();
            dmField = _dbStruct.GetDataTable("select * from sysfield where  Type<>3 and Visible=1 and sysTableid=" + systableid + " order by TabIndex");
            if (dmField == null) return;
            MapStruct = new DataTable();
            MapStruct.Columns.Add("FieldName", typeof(string));
            MapStruct.Columns.Add("Type", typeof(int));
            MapStruct.Columns.Add("ColName", typeof(string));
            MapStruct.Columns.Add("DefaultValue", typeof(string));
            MapStruct.Columns.Add("AllowNull", typeof(bool));
            foreach (DataRow rF in dmField.Rows)
            {
                DataRow dr = MapStruct.NewRow();
                dr["FieldName"] = rF["FieldName"];
                dr["Type"] = rF["Type"];
                dr["DefaultValue"] = rF["DefaultValue"];
                dr["AllowNull"] = rF["AllowNull"].ToString() == "1";
                MapStruct.Rows.Add(dr);
            }

            gridControl1.DataSource = MapStruct;
            gridControl1.DataMember = MapStruct.TableName;
        }
        public DataTable dbEx = null;
        ImportExcel IEx;
        public DataTable MapStruct;
        Database _db = Database.NewDataDatabase();
        Database _dbStruct = Database.NewStructDatabase();        
        DataTable dataType;
        private void fImExcel_Load(object sender, EventArgs e)
        {
        
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Excel|*.xls;*.xlsx";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tFileName.EditValue = dialog.FileName;
                IEx = new ImportExcel(dialog.FileName);
                List<string> sheets = IEx.GetSheets();
                lSheet.Properties.Items.Clear();
                lSheet.Properties.Items.AddRange(sheets.ToArray());
            }

        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string sql;
            if (IEx != null && IEx.Db != null)
            {
                dbEx = IEx.Db;
                this.Dispose();
            }
            else
            {
                MessageBox.Show("Không nhận được dữ liệu");
            }

            
        }

        private void lSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lSheet.EditValue == null) return;
            List<string> cols= IEx.GetCol(lSheet.EditValue.ToString());
            if (cols == null) return;
            RiCom.Items.AddRange(cols.ToArray());
            foreach (DataRow dr in MapStruct.Rows)
            {
                if (cols.Exists(x => x.ToString().ToUpper() == dr["FieldName"].ToString().ToUpper()))
                    dr["ColName"] = dr["FieldName"].ToString();

            }

        }
        DataTable dmField;


        private void tFileName_EditValueChanged(object sender, EventArgs e)
        {

        }


    }
}