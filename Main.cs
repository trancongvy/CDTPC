using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using DevExpress.XtraBars;
using System.Configuration;
using DevExpress.XtraEditors;
using DevExpress.XtraNavBar;
using FormFactory;
using ReportFactory;
using CDTControl;
using CDTLib;
using DataFactory;
using CDTSystem;
using Plugins;

namespace CDT
{
    public partial class Main : DevExpress.XtraEditors.XtraForm
    {
        private string H_KEY = Config.GetValue("H_KEY").ToString();
        string sysPackageID, dbName;
        DataTable dtMenu;
        SysMenu _sysMenu = new SysMenu();
        PluginManager pm = new PluginManager();

        public Main(DataRow drUser, DataRow drPackage)
        {
            
            InitializeComponent();
            sysPackageID = drPackage["sysPackageID"].ToString();
            dbName = drPackage["DbName"].ToString();
            _sysMenu.SynchronizeMenuWithPlugins(pm);
            
            InitializeMenu();
            barManagerMain.Items.Add(barSubItemHelp);
            barMainMenu.LinksPersistInfo.Add(new DevExpress.XtraBars.LinkPersistInfo(barSubItemHelp));
            barManagerMain.ItemClick += new ItemClickEventHandler(barManagerMain_ItemClick);
            
            if (Config.GetValue("Language").ToString() == "1")
            {
                DevLocalizer.Translate(this);
                TranslateForMenu();
            }
            
            InitializeForm(drUser, drPackage);
            bool supported = false;
            string RegSupport = Registry.GetValue(H_KEY, "SupportOnline", "false").ToString();
            if (RegSupport != string.Empty)
                supported = Boolean.Parse(RegSupport);
            
            if (DateTime.Today.DayOfWeek == DayOfWeek.Monday && !supported)
            {
                Startup frm = new Startup();
                frm.MdiParent = this;
                frm.Show();
                Registry.SetValue(H_KEY, "SupportOnline", true);
            }
            else
                if (DateTime.Today.DayOfWeek != DayOfWeek.Monday && supported)
                    Registry.SetValue(H_KEY, "SupportOnline", false);
            
        }

        private void TranslateForMenu()
        {
            for (int i = 0; i < barManagerMain.Items.Count; i++)
                barManagerMain.Items[i].Caption = UIDictionary.Translate(barManagerMain.Items[i].Caption);
        }

        private void InitializeForm(DataRow drUser, DataRow drPackage)
        {
            if (drUser["FullName"].ToString() != string.Empty)
                bsiUserName.Caption = bsiUserName.Caption + ": " + drUser["FullName"].ToString();
            else
                bsiUserName.Caption = bsiUserName.Caption + ": " + drUser["UserName"].ToString();
            if (!Boolean.Parse(drUser["CoreAdmin"].ToString()))
                iCheckData.Visibility = BarItemVisibility.Never;
            if (!Boolean.Parse(Config.GetValue("Admin").ToString()))
            {
                iViewHistory.Visibility = BarItemVisibility.Never;
                iUserTrace.Visibility = BarItemVisibility.Never;
                iCollectData.Visibility = BarItemVisibility.Never;
            }
            bsiStyle.Caption = bsiStyle.Caption + ": " + Config.GetValue("Style").ToString();
            bsiToday.Caption = bsiToday.Caption + ": " + DateTime.Today.ToString("dd/MM/yyyy");
            this.Text = Config.GetValue("Language").ToString() == "0" ? drPackage["PackageName"].ToString() : drPackage["PackageName2"].ToString();
            if (drPackage["Background"].ToString() != string.Empty)
                this.BackgroundImage = GetImage(drPackage["Background"] as byte[]);
        }

        private void SystemMenuClick(object sender, ItemClickEventArgs e)
        {
            switch (e.Item.Name)
            {
                case "iRestart":
                    Application.Restart();
                    break;
                case "iExit":
                    if (XtraMessageBox.Show("Vui lòng xác nhận thoát khỏi ứng dụng?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        Application.Exit();
                    break;
                case "iUserConfig":
                    UserConfig frmUserConfig = new UserConfig();
                    if (frmUserConfig.IsShow)
                        frmUserConfig.ShowDialog();
                    break;
                case "iCheckData":
                    CheckData frmCheckData = new CheckData(true);
                    frmCheckData.ShowDialog();
                    break;
                case "iViewHistory":
                    CheckData frmViewHistory = new CheckData(false);
                    frmViewHistory.ShowDialog();
                    break;
                case "iChangePassword":
                    ChangePassword frmChangePwd = new ChangePassword();
                    frmChangePwd.ShowDialog();
                    break;
                case "iAbout":
                    About frmAbout = new About();
                    frmAbout.ShowDialog();
                    break;
                case "iHelpOnline":
                    System.Diagnostics.Process.Start("http://www.combosoft.com.vn/?.p=21");
                    break;
                case "iHelp":
                    string fileHelp = Config.GetValue("Package").ToString() + ".chm";
                    if (System.IO.File.Exists(fileHelp))
                        System.Diagnostics.Process.Start(fileHelp);
                    break;
                case "iBackup":
                    DataMaintain dmBk = new DataMaintain();
                    dmBk.BackupData(Application.StartupPath);
                    break;
                case "iRestore":
                    DataMaintain dmRt = new DataMaintain();
                    dmRt.RestoreData(Application.StartupPath);
                    break;
                case "iCollectData":
                    FrmDataCollection frmDc = new FrmDataCollection();
                    frmDc.ShowDialog();
                    break;
            }
        }

        void barManagerMain_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.Item.GetType() != typeof(BarSubItem))
            {
                DataRow dr = e.Item.Tag as DataRow;
                if (dr == null)
                    SystemMenuClick(sender, e);
                else
                    ExecuteCommand(dr);
            }
        }

        private void navBarControlMain_LinkClicked(object sender, NavBarLinkEventArgs e)
        {
            DataRow dr = e.Link.Item.Tag as DataRow;

            ExecuteCommand(dr);
        }

        private void treeListMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!treeListMain.FocusedNode.HasChildren)
            {
                DataRow dr = dtMenu.Rows[treeListMain.FocusedNode.Id];

                ExecuteCommand(dr);
            }
        }

        private void treeListMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                if (!treeListMain.FocusedNode.HasChildren)
                {
                    DataRow dr = dtMenu.Rows[treeListMain.FocusedNode.Id];

                    ExecuteCommand(dr);
                }
        }

        private void ExecuteCommand(DataRow dr)
        {
            if (dr == null)
                return;
            if (dr["sysTableID"].ToString() != string.Empty)
                ShowTable(dr);
            else
            {
                if (dr["sysReportID"].ToString() != string.Empty)
                    ShowReport(dr);
                else
                    ExecutePlugin(dr);
            }
        }

        private void InitializeMenu()
        {

            dtMenu = _sysMenu.GetMenu();
            if (dtMenu == null)
                return;

            treeListMain.OptionsView.EnableAppearanceEvenRow = true;

            //treeListMain.DataSource = dtMenu;
            //treeListMain.KeyFieldName = "sysMenuID";
            //treeListMain.ParentFieldName = "sysMenuParent";
            tlcMenuName.FieldName = Config.GetValue("Language").ToString() == "0" ? "MenuName" : "MenuName2";
            if (dtMenu.Rows.Count < 20)
                treeListMain.ExpandAll();

            foreach (DataRow dr in dtMenu.Rows)
            {
                string sysMenuParent = dr["sysMenuParent"].ToString();
                if (sysMenuParent == string.Empty)  //menu cha
                {
                    string menuName = Config.GetValue("Language").ToString() == "0" ? dr["MenuName"].ToString() : dr["MenuName2"].ToString();
                    BarSubItem bsi = new BarSubItem(barManagerMain, menuName);
                    barMainMenu.LinksPersistInfo.Add(new LinkPersistInfo(bsi));
                    LoopMenu(dtMenu, dr, bsi);

                    if (dr["sysPackageID2"].ToString() != string.Empty)
                    {
                        NavBarGroup nvb = new NavBarGroup(menuName);
                        if (GetImage(dr))
                            nvb.SmallImageIndex = imageCollection1.Images.Count - 1;
                        navBarControlMain.Groups.Add(nvb);
                        LoopNavBar(dr, nvb);
                    }
                }

                _sysMenu.ModifyMenu(dr);
            }

        }

        private void LoopNavBar(DataRow dr, NavBarGroup nvb)
        {
            foreach (DataRow drChild in dtMenu.Rows)
            {
                if (drChild["sysMenuParent"].ToString() == dr["sysMenuID"].ToString())
                {
                    string menuName = Config.GetValue("Language").ToString() == "0" ? drChild["MenuName"].ToString() : drChild["MenuName2"].ToString();
                    if (HasChild(dtMenu, drChild["sysMenuID"].ToString()))
                        LoopNavBar(drChild, nvb);
                    else
                    {
                        NavBarItem nbi = new NavBarItem(menuName);
                        nbi.Tag = drChild;
                        if (GetImage(drChild))
                            nbi.SmallImageIndex = imageCollection1.Images.Count - 1;
                        navBarControlMain.Items.Add(nbi);
                        nvb.ItemLinks.Add(nbi);
                    }
                }
            }
        }

        private Shortcut GetShortcut(string strShortcut)
        {
            Array arrShortcut = Enum.GetValues(typeof(Shortcut));
            foreach (Shortcut sctmp in arrShortcut)
                if (sctmp.ToString() == strShortcut)
                    return sctmp;
            return Shortcut.None;
        }

        private void LoopMenu(DataTable dtMenu, DataRow dr, BarSubItem bsi)
        {
            foreach (DataRow drChild in dtMenu.Rows)
            {
                if (drChild["sysMenuParent"].ToString() == dr["sysMenuID"].ToString())
                {
                    string menuName = Config.GetValue("Language").ToString() == "0" ? drChild["MenuName"].ToString() : drChild["MenuName2"].ToString();
                    if (HasChild(dtMenu, drChild["sysMenuID"].ToString()))  //vua cha vua con
                    {
                        BarSubItem bsiChild = new BarSubItem(barManagerMain, menuName);
                        //if (drChild["sysPackageID2"].ToString() == string.Empty)
                        //    barSubItemSystem.LinksPersistInfo.Add(new LinkPersistInfo(bsiChild));
                        //else
                            bsi.LinksPersistInfo.Add(new LinkPersistInfo(bsiChild));
                        LoopMenu(dtMenu, drChild, bsiChild);
                    }   
                    else
                    {   //menu con
                        BarLargeButtonItem bbi = new BarLargeButtonItem(barManagerMain, menuName);
                        bbi.Hint = menuName;
                        bbi.Tag = drChild;
                        bbi.CaptionAlignment = BarItemCaptionAlignment.Bottom;
                        string strShortcut = drChild["ShortKey"].ToString();
                        if (strShortcut != string.Empty)
                            bbi.ItemShortcut = new BarShortcut(GetShortcut(strShortcut));
                        if (GetImage(drChild))
                            bbi.ImageIndex = imageCollectionMain.Images.Count - 1;
                        //if (drChild["sysPackageID2"].ToString() == string.Empty)
                        //    barSubItemSystem.LinksPersistInfo.Add(new LinkPersistInfo(bbi));
                        //else
                            bsi.LinksPersistInfo.Add(new LinkPersistInfo(bbi));
                        if (Boolean.Parse(drChild["isToolbar"].ToString()))
                            barToolbars.LinksPersistInfo.Add(new LinkPersistInfo(BarLinkUserDefines.PaintStyle, bbi, BarItemPaintStyle.CaptionGlyph));
                    }
                }
            }
        }

        private bool HasChild(DataTable dtMenu, string sysMenuID)
        {
            foreach (DataRow dr in dtMenu.Rows)
                if (dr["sysMenuParent"].ToString() == sysMenuID)
                    return true;
            return false;
        }

        private bool GetImage(DataRow dr)
        {
            if (dr["Image"].ToString() == string.Empty)
                return false;
            Image im = GetImage(dr["Image"] as byte[]);
            if (im == null)
                return false;
            imageCollection1.AddImage(im);
            imageCollectionMain.AddImage(im);
            return true;
        }

        private Image GetImage(byte[] b)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(b);
            if (ms == null)
                return null;
            Image im = Image.FromStream(ms);
            return (im);
        }

        private void ShowTable(DataRow drTable)
        {
            if (drTable == null)
                return;
            int bType = Int32.Parse(drTable["Type"].ToString());
            FormType formType;
            switch (bType)
            {
                case 1:
                case 2:
                    formType = FormType.Single;
                    break;
                case 3:
                    formType = FormType.MasterDetail;
                    break;
                case 4:
                case 5:
                    formType = FormType.Detail;
                    break;
                default:
                    formType = FormType.Single;
                    break;
            }
            Form frm = MdiExists(drTable["MenuName"].ToString());
            if (frm != null)
                frm.Activate();
            else
            {
                frm = FormFactory.FormFactory.Create(formType, drTable);
                frm.MdiParent = this;
                frm.Show();
            }
        }

        private void ShowReport(DataRow drReport)
        {
            Form frm = MdiExists(drReport["MenuName"].ToString());
            if (frm != null)
                frm.Activate();
            else
            {
                frm = ReportFactory.ReportFactory.Create(drReport);
                frm.MdiParent = this;
                frm.Show();
            }
        }

        private Form MdiExists(string caption)
        {
            foreach (Form frm in this.MdiChildren)
                if (frm.Text == caption)
                    return frm;
            return null;
        }

        private void ExecutePlugin(DataRow drData)
        {
            int menuID = Int32.Parse(drData["MenuPluginID"].ToString());
            string pluginName = drData["PluginName"].ToString();
            pm.Execute(menuID, pluginName);
        }

        private void iRestart_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}