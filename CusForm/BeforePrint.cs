using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
using CusData;
using CDTLib;

namespace CusForm
{
    public partial class BeforePrint : DevExpress.XtraEditors.XtraForm
    {
        string _reportFile = string.Empty;
        string _title = string.Empty;
        int[] _arrIndex;
        CDTData _data;
        DataTable tbMau=null;
        public BeforePrint(CDTData data, int[] arrIndex)
        {
            InitializeComponent();
            _data = data;
            _reportFile = _data.DrTable["Report"].ToString();
            _title = data.dataType == CDTControl.DataType.MasterDetail ? _data.DrTableMaster["DienGiai"].ToString(): _data.DrTable["DienGiai"].ToString();
            _arrIndex = arrIndex;
            if (Config.GetValue("Language").ToString() == "1")
                DevLocalizer.Translate(this);
            if (_data.dataType == CDTControl.DataType.MasterDetail)
            {
                tbMau = (_data as CusData.DataMasterDetail).GetReportFile(_data.DrTable["sysTableID"].ToString());
            }
            if (tbMau == null||tbMau.Rows.Count==0)
            {
                layoutControlItem8.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
            else
            {
                gridLookUpEdit1.Properties.DataSource = tbMau;
                gridLookUpEdit1.EditValue = tbMau.Rows[0]["RFile"].ToString();
                layoutControlItem1.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                 
            }
        }




       
        private void simpleButtonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButtonChapNhan_Click(object sender, EventArgs e)
        {
            PrintOrPreview(false);
        }

        private void SetVariables(DevExpress.XtraReports.UI.XtraReport rptTmp)
        {
            foreach (DictionaryEntry de in Config.Variables)
            {
                string key = de.Key.ToString();
                if (key.Contains("@"))
                    key = key.Remove(0, 1);
                XRControl xrc = rptTmp.FindControl(key, true);
                if (xrc != null)
                {
                    string value = de.Value.ToString();
                    try
                    {
                        if (value.Contains("/"))
                            xrc.Text = DateTime.Parse(value).ToShortDateString();
                        else
                            xrc.Text = value;
                    }
                    catch
                    {
                        xrc.Text = value;
                    }
                    xrc = null;
                }
            }
        }

        private void simpleButtonSuaMau_Click(object sender, EventArgs e)
        {
            DataTable dtReport = _data.GetDataForPrint(_arrIndex[0]);
            if (dtReport == null)
                return;
            DevExpress.XtraReports.UI.XtraReport rptTmp = null;
            string path;
            if (Config.GetValue("DuongDanBaoCao") != null)
                path = Config.GetValue("DuongDanBaoCao").ToString() + "\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";
            else
                path = Application.StartupPath + "\\Reports\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";
            string pathTmp;
            if (Config.GetValue("DuongDanBaoCao") != null)
                pathTmp = Config.GetValue("DuongDanBaoCao").ToString() + "\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";
            else
                pathTmp = Application.StartupPath + "\\" + Config.GetValue("Package").ToString() + "\\Reports\\template.repx";
            if (System.IO.File.Exists(path))
                rptTmp = DevExpress.XtraReports.UI.XtraReport.FromFile(path, true);
            else if (System.IO.File.Exists(pathTmp))
                rptTmp = DevExpress.XtraReports.UI.XtraReport.FromFile(pathTmp, true);
            else
                rptTmp = new DevExpress.XtraReports.UI.XtraReport();
            if (rptTmp != null)
            {
                rptTmp.DataSource = dtReport;
                XRDesignFormEx designForm = new XRDesignFormEx();
                designForm.OpenReport(rptTmp);
                if (System.IO.File.Exists(path))
                    designForm.FileName = path;
                designForm.KeyPreview = true;
                designForm.KeyDown += new KeyEventHandler(designForm_KeyDown);
                designForm.Show();
            }
        }

        void designForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.X)
                (sender as XRDesignFormEx).Close();
        }

        private void BeforePrint_Load(object sender, EventArgs e)
        {
            textEditTitle.Text = _title;
            textEditSoCTGoc.Text = "0";
            textEditSoLien.Text = "1";
        }

        private void PrintOrPreview(bool isPrint)
        {
            DevExpress.XtraReports.UI.XtraReport rptTmp = null;
            string path;
            if (Config.GetValue("DuongDanBaoCao") != null)
                path = Config.GetValue("DuongDanBaoCao").ToString() + "\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";
            else
                path = Application.StartupPath + "\\Reports\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";
            if (System.IO.File.Exists(path))
            {
                for (int i = 0; i < _arrIndex.Length; i++)
                {
                    int index = _arrIndex[i];
                    DataTable dtReport = _data.GetDataForPrint(index);
                    if (_data.DsData.Tables[0].Columns.Contains("PrintIndex"))
                    {

                        _reportFile = tbMau.Rows[int.Parse(_data.DsData.Tables[0].Rows[index]["PrintIndex"].ToString())]["RFile"].ToString();
                    }
                    if (Config.GetValue("DuongDanBaoCao") != null)
                        path = Config.GetValue("DuongDanBaoCao").ToString() + "\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";
                    else
                        path = Application.StartupPath + "\\Reports\\" + Config.GetValue("Package").ToString() + "\\" + _reportFile + ".repx";

                    rptTmp = DevExpress.XtraReports.UI.XtraReport.FromFile(path, true);
                    DevExpress.XtraReports.UI.XRControl xrcTitle = rptTmp.FindControl("title", true);
                    if (xrcTitle != null)
                        xrcTitle.Text = textEditTitle.Text.ToUpper();
                    DevExpress.XtraReports.UI.XRControl xrcSoCTGoc = rptTmp.FindControl("SoCTGoc", true);
                    if (xrcSoCTGoc != null)
                        xrcSoCTGoc.Text = textEditSoCTGoc.Text;
                    SetVariables(rptTmp);
                    
                    
                    if (dtReport == null)
                        continue;
                    rptTmp.DataSource = dtReport;
                    rptTmp.ScriptReferences = new string[] {Application.StartupPath + "\\CDTLib.dll"};
                    if (Config.GetValue("Language").ToString() == "1")
                        Translate(rptTmp);
                    for (int j = 0; j < Int32.Parse(textEditSoLien.Text); j++)
                    {
                        if (isPrint)
                            rptTmp.Print();
                        else
                            rptTmp.ShowPreview();
                    }
                }
            }
            else
                XtraMessageBox.Show("Không tìm thấy file báo cáo " + _reportFile);
        }

        private void simpleButtonIn_Click(object sender, EventArgs e)
        {
            PrintOrPreview(true);
        }

        private void BeforePrint_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.F7:
                    simpleButtonIn_Click(simpleButtonIn, new EventArgs());
                    break;
            }
        }

        private void Translate(XRControl xrc)
        {
            if (xrc.HasChildren)
                foreach (XRControl xrChild in xrc.Controls)
                    Translate(xrChild);
            xrc.Text = UIDictionary.Translate(xrc.Text);
        }

        private void gridLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            
            _reportFile = gridLookUpEdit1.EditValue.ToString();// (gridLookUpEdit1.EditValue as DataRowView)[gridLookUpEdit1.Properties.ValueMember].ToString();
            _title = gridLookUpEdit1.Text;
            this.textEditTitle.Text = _title;
        }
    }
}