using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
namespace CustomClass
{
    public partial class fPhieuGiaohang : DevExpress.XtraEditors.XtraForm
    {
        public fPhieuGiaohang()
        {
            InitializeComponent();
        }
        BindingSource bs = new BindingSource();
        dPhieuGiaoHang _data = new dPhieuGiaoHang();
        private void fPhieuGiaohang_Load(object sender, EventArgs e)
        {
            bs.DataSource = _data.ds;
            this.bs.CurrentChanged += new EventHandler(bs_CurrentChanged);
            this.bs.DataMember = _data.mt.TableName;
            
            this.gcMt.DataSource = bs;
            gcDt.DataSource = bs;
            gcDt.DataMember =_data.ds.Relations[0].RelationName; 
            bs_CurrentChanged(bs, new EventArgs());
            if (bs.Count > 0) bs.MoveFirst();
            this.KeyDown += new KeyEventHandler(fPhieuGiaohang_KeyDown);
        }

        void fPhieuGiaohang_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    btNew_Click(sender,new EventArgs());
                    break;
                case Keys.F3:
                    btEdit_Click(sender, new EventArgs());
                    break;
                case Keys.F4:
                    btDelete_Click(sender, new EventArgs());
                    break;
                case Keys.F6:
                    btFind_Click(sender, new EventArgs());
                    break;
                case Keys.F7:
                    btPrint_Click(sender, new EventArgs());
                    break;
                case Keys.Escape:
                    this.Dispose();
                    break;

            }  
        }

        void bs_CurrentChanged(object sender, EventArgs e)
        {
            if ((bs.Current as DataRowView) == null)
            {
                _data.mtCur = null;
                return;
            }
            _data.mtCur = (bs.Current as DataRowView).Row;
        }
        fPhieuGiaohangdt fdt;
        private void btNew_Click(object sender, EventArgs e)
        {
           string s = gvMt.ActiveFilterString;
            gvMt.ClearColumnsFilter();
            _data.FAction = FormAction.New;

            bs.AddNew();             
            bs.EndEdit();
            if(fdt == null)
            fdt = new fPhieuGiaohangdt( bs,_data);
            fdt.ShowDialog();
            
            gvMt.ActiveFilterString = s;
            gvMt.ApplyColumnsFilter();
            this.gvMt.OptionsSelection.MultiSelect = true;
            gvMt.OptionsView.ShowGroupPanel = false;
            this.gvMt.SelectRow(_data.mt.Rows.Count - 1);
        }

        private void btEdit_Click(object sender, EventArgs e)
        {
            if (_data.mtCur == null) return;
            _data.FAction = FormAction.Edit;
            int i = bs.Position;
            if (fdt == null)
            {
                fdt = new fPhieuGiaohangdt( bs,_data);
                bs.Position = i;
            }
            fdt.ShowDialog();


        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (_data.mtCur == null) return;
            if (bs.Position < 0) return;
            if (bs.Current == null) return;
            if (MessageBox.Show("Bạn có thật sự muốn xóa phiếu không?", "Xác nhận!", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if(_data.DeleteData(_data.mtCur))                
                    bs.RemoveCurrent();
                bs.EndEdit();

            }

        }

        private void btPrint_Click(object sender, EventArgs e)
        {
            Config.NewKeyValue("Operation", (sender as SimpleButton).Text);
            if (gvMt.SelectedRowsCount == 0)
                return;
            if (_data.dsStr.Tables[1].Rows[0]["Report"].ToString() == string.Empty)
                gcMt.ShowPrintPreview();
            else
            {
                int[] oldIndex = gvMt.GetSelectedRows();
                int[] newIndex = oldIndex;
                if (gvMt.SortedColumns.Count > 0)
                    for (int i = 0; i < oldIndex.Length; i++)
                        newIndex[i] = _data.mt.Rows.IndexOf(gvMt.GetDataRow(oldIndex[i]));

                CustomBeforePrint bp = new CustomBeforePrint(_data, newIndex);
                bp.ShowDialog();
            }
        }
        private void DisplayData()
        {
            if (_data.ds == null)
                return;
                        bs.DataSource = _data.ds;
            this.bs.CurrentChanged += new EventHandler(bs_CurrentChanged);
            this.bs.DataMember = _data.mt.TableName;

            this.gcMt.DataSource = bs;
            gcDt.DataSource = bs;
            gcDt.DataMember = _data.ds.Relations[0].RelationName;
            bs_CurrentChanged(bs, new EventArgs());
            if (bs.Count > 0) bs.MoveFirst();


        }
        private void btFind_Click(object sender, EventArgs e)
        {
            Config.NewKeyValue("Operation", "F6-Tìm kiếm");
                gvMt.ShowFilterEditor(gvMt.Columns[0]);
                if (gvMt.RowFilter != string.Empty)
                {
                    SqlSearching sSearch = new SqlSearching();
                    string sql = sSearch.GenSqlFromGridFilter(gvMt.RowFilter);
                    _data._Condition = sql;
                    _data.getdata();
                    this.DisplayData();
                    gvMt.ClearColumnsFilter();
                    XtraMessageBox.Show("Kết quả tìm kiếm: " + gvMt.DataRowCount.ToString() + " mục số liệu");
                }
            

        }
    }
}