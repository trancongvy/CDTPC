using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid;
using DevExpress.XtraTreeList;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using CBSControls;
using CusData;
using CDTLib;
using CDTControl;
using DevControls;
namespace CusForm
{

    public class FormDesigner
    {
        private GridControl _gcMain;
        private CDTData _data;
        private BindingSource _bindingSource;
        private FormAction _formAction;
        private List<CDTData> _lstData = new List<CDTData>();
        private BaseEdit _firstControl;
        private Hashtable RIOldValue=new Hashtable();
        private List<DevControls.CDTRepGridLookup> _lstRep = new List<DevControls.CDTRepGridLookup>();
        private List<LookUp_CDTData> _Glist = new List<LookUp_CDTData>();
        public List<CDTGridLookUpEdit> _glist = new List<CDTGridLookUpEdit>();
        private List<RLookUp_CDTData> _Rlist = new List<RLookUp_CDTData>();
        private List<DevControls.CDTRepGridLookup> _rlist = new List<DevControls.CDTRepGridLookup>();
        public List<BaseEdit> _BaseList = new List<BaseEdit>();
        public List<LayoutControlItem> _LayoutList = new List<LayoutControlItem>();
        private struct LookUp_CDTData
        {
            public CDTGridLookUpEdit glk;
            public int dataIndex;
            public LookUp_CDTData(CDTGridLookUpEdit g, int i)
            {
                glk = g;
                dataIndex = i;
            }
        }
        private struct RLookUp_CDTData
        {
            public DevControls.CDTRepGridLookup rglk;
            public int dataIndex;
            public RLookUp_CDTData(DevControls.CDTRepGridLookup r, int i)
            {
                rglk = r;
                dataIndex = i;
            }
        }
        public List<DevControls.CDTRepGridLookup> rlist
        {
            get { return _rlist; }
            set
            {
                _rlist = value;
                RefreshDataForLookup();
            }
        }
        public BaseEdit FirstControl
        {
            get { return _firstControl; }
            set { _firstControl = value; }
        }

        public CDTData Data
        {
            get { return _data; }
        }

        public FormAction formAction
        {
            get { return _formAction; }
            set { _formAction = value; }
        }

        public BindingSource bindingSource
        {
            get { return _bindingSource; }
            set { _bindingSource = value; }
        }

        public FormDesigner(CDTData data)
        {
            this._data = data;
            if (_data.dataType != DataType.Report)
                _lstData.Add(_data);
        }

        public FormDesigner(CDTData data, BindingSource bindingSource)
        {
            this._data = data;
            this._bindingSource = bindingSource;
            if (_data.dataType != DataType.Report)
                _lstData.Add(_data);

        }

       

        /// <summary>
        /// Khởi tạo lưới từ cấu trúc của bảng đang mở
        /// </summary>
        public GridControl GenGridControl(DataTable dt, bool isEdit, DockStyle ds)
        {
            GridControl gcMain = new GridControl();
            DevExpress.XtraGrid.Views.Grid.GridView gvMain = new DevExpress.XtraGrid.Views.Grid.GridView();

            ((ISupportInitialize)gcMain).BeginInit();
            ((ISupportInitialize)gvMain).BeginInit();

            //grid control
            gcMain.Dock = ds;
            gcMain.SendToBack();
            gcMain.MainView = gvMain;
            gcMain.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gvMain });
            
            //grid view
            gvMain.OptionsView.ShowFooter = true;
            gvMain.GridControl = gcMain;
            if (Config.GetValue("Language").ToString() == "0")
                gvMain.GroupPanelText = "Bảng nhóm: kéo thả một cột vào đây để nhóm số liệu";
            gvMain.OptionsView.ColumnAutoWidth = false;
            gvMain.OptionsView.EnableAppearanceEvenRow = true;
            gvMain.OptionsSelection.MultiSelect = true;
            gvMain.OptionsBehavior.Editable = false;
            gvMain.OptionsView.ShowAutoFilterRow = true;
            gvMain.OptionsNavigation.EnterMoveNextColumn = true;
            gvMain.IndicatorWidth = 40;
            gvMain.OptionsView.ShowDetailButtons = false;
            gvMain.OptionsBehavior.AutoExpandAllGroups = true;
            gvMain.OptionsNavigation.AutoFocusNewRow = true;
            gvMain.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(View_CustomDrawRowIndicator);
            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvMain_CellValueChanged);
            if (isEdit)
            {
                gvMain.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(gvMain_FocusedRowChanged);
                gcMain.KeyDown += new KeyEventHandler(gcMain_KeyDown);
                gvMain.OptionsBehavior.Editable = true;
                gvMain.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
            }

            //grid column
            int exColNum = 0;
            bool admin = Boolean.Parse(Config.GetValue("Admin").ToString());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string viewable = dr["Viewable"].ToString();
                if (!admin && viewable != string.Empty && !Boolean.Parse(viewable))
                    continue;
                DevControls.CDTGridColumn gcl = GenGridColumn(dr, exColNum, false);
                RepositoryItem ri = GenRepository(dr);
                //get repository control is gen
                if (ri != null)
                {
                    gcMain.RepositoryItems.Add(ri);
                    gcl.ColumnEdit = ri;
                }
                gvMain.Columns.Add(gcl);

               
                //pType = 1: them cot Dien giai cho ma (fieldname cua cot la gia tri DisplayMember trong sysField)
                int pType = Int32.Parse(dr["Type"].ToString());
                if (pType == 12)
                    gvMain.OptionsView.RowAutoHeight = true;
                if (pType == 1 && dr["DisplayMember"].ToString() != string.Empty)
                {
                    DevControls.CDTGridColumn gcl1 = GenGridColumn(dr, exColNum, false);
                    RepositoryItem ri1 = GenRepository(dr);
                    //get repository control is gen
                    if (ri1 != null)
                    {   //hieu chinh lai cac thuoc tinh cua cot Dien giai nay
                        string displayMember = dr["DisplayMember"].ToString();
                        ((DevControls.CDTRepGridLookup)ri1).DisplayMember = displayMember;
                        gcMain.RepositoryItems.Add(ri1);
                        string caption;
                        if (Config.GetValue("Language").ToString() == "0")
                            caption = "Tên " + dr["LabelName"].ToString().ToLower();
                        else
                            caption = dr["LabelName2"].ToString() + " name";
                        int formType = Int32.Parse(_data.DrTable["Type"].ToString());
                        if ((formType == 1 || formType == 4) && !Boolean.Parse(dr["AllowNull"].ToString()))
                            caption = "*" + caption;
                        gcl1.Caption = caption;
                        gcl1.VisibleIndex = gcl1.VisibleIndex + 1;
                        
                        gcl1.Width = gcl1.Width + 200;
                        gcl1.ColumnEdit = ri1;
                        gcl1.Visible = gcl.Visible;
                    }
                    gvMain.Columns.Add(gcl1);
                    exColNum++;
                }
                if (gcl.GroupIndex >= 0)
                    gvMain.GroupSummary.Add(new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Count, gcl.FieldName, null, "({0} mục)"));
            }
            ((ISupportInitialize)gcMain).EndInit();
            ((ISupportInitialize)gvMain).EndInit();

            _gcMain = gcMain;

            return gcMain;
        }



        void gvMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                DevExpress.XtraGrid.Views.Grid.GridView gvMain = sender as DevExpress.XtraGrid.Views.Grid.GridView;
                gvMain.FocusedColumn = gvMain.VisibleColumns[0];
            }
        }

        private void gcMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4)
            {
                GridControl gcMain = sender as GridControl;
                DevExpress.XtraGrid.Views.Grid.GridView gvMain = gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
                gvMain.DeleteSelectedRows();
            }
        }

        /// <summary>
        /// Hàm tạo ra GridColumn cho lưới
        /// </summary>
        /// <param name="dr">Thông tin về field hiện tại của bảng (1 dòng trong sysField)</param>
        /// <param name="exColNum">Số lượng cột mở rộng cho trường hợp có bảng danh mục tham chiếu</param>
        private DevControls.CDTGridColumn GenGridColumn(DataRow dr, int exColNum, bool checkData)
        {
            DevControls.CDTGridColumn gcl = new DevControls.CDTGridColumn();
            gcl.Name = "cl" + dr["FieldName"].ToString();
            gcl.FieldName = dr["FieldName"].ToString();
            string caption = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
            int formType = Int32.Parse(_data.DrTable["Type"].ToString());
            if ((formType == 1 || formType == 4) && !Boolean.Parse(dr["AllowNull"].ToString()))
                caption = "*" + caption;
            gcl.Caption = caption;
            gcl.ToolTip = dr["Tip"].ToString();
            gcl.VisibleIndex = Int32.Parse(dr["TabIndex"].ToString()) + exColNum;
            gcl.MasterRow = dr;//dung vao viec fresh look up
            gcl.refFilter = dr["DynCriteria"].ToString();
            if (!checkData)
                gcl.Visible = Boolean.Parse(dr["Visible"].ToString());
            if (Boolean.Parse(dr["IsFixCol"].ToString()))
                gcl.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            if (Boolean.Parse(dr["IsGroupCol"].ToString()))
                gcl.GroupIndex = 0;
            int pType = Int32.Parse(dr["Type"].ToString());
            if (!checkData && pType == 3)
                gcl.Visible = false;
            
            if (pType == 2)
                gcl.Width = gcl.Width + 80;
            if (pType ==3)
                gcl.Width = gcl.Width -40;
            if (pType == 9)
            {
                gcl.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gcl.DisplayFormat.FormatString = "dd/MM/yyyy";
                
            }
            if (pType == 14)
            {
                gcl.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gcl.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm:ss";
                RepositoryItemDateEdit dEdit = new RepositoryItemDateEdit();
                gcl.ColumnEdit = dEdit;
                dEdit.EditMask = "dd/MM/yyyy HH:mm:ss";
                dEdit.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                dEdit.EditFormat.FormatString = "dd/MM/yyyy HH:mm:ss"; ;
                gcl.Width = gcl.Width + 100;
            }
            if (pType == 8)
            {
                string f = dr["EditMask"].ToString() != string.Empty ? ":" + dr["EditMask"].ToString() : "";
                gcl.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcl.DisplayFormat.FormatString = dr["EditMask"].ToString();
                gcl.SummaryItem.Assign(new GridSummaryItem(DevExpress.Data.SummaryItemType.Sum, dr["FieldName"].ToString(),"{0" + f + "}"));
                gcl.Width = gcl.Width + 50;
                
            }
            
            return gcl;
        }

        /// <summary>
        /// Hàm tạo ra Control nhúng cho GridColumn
        /// </summary>
        /// <param name="dr">Thông tin về field hiện tại của bảng (1 dòng trong sysField)</param>
        private RepositoryItem GenRepository(DataRow dr)
        {
            RepositoryItem tmp = null;
            int pType = Int32.Parse(dr["Type"].ToString());

            //0: text(pk); 1: text(fk); 2: text; 3: int(pk); 4: int(fk); 5: int; 6: unique identifier; 
            //7: unique identifier(fk); 8: decimal; 9: date; 10: boolean; 11: time; 12: image;
            switch (pType)
            {
                case 5:
                    tmp = new RepositoryItemSpinEdit();
                    (tmp as RepositoryItemSpinEdit).AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    break;
                case 8:
                    tmp = new RepositoryItemCalcEdit();
                    (tmp as RepositoryItemCalcEdit).AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    (tmp as RepositoryItemCalcEdit).Spin += new DevExpress.XtraEditors.Controls.SpinEventHandler(FormDesigner_Spin1);
                    (tmp as RepositoryItemCalcEdit).KeyUp += new KeyEventHandler(VCalEdit_KeyUp);
                    if (dr["EditMask"].ToString() != string.Empty)
                    {
                        (tmp as RepositoryItemCalcEdit).EditMask = dr["EditMask"].ToString();
                        (tmp as RepositoryItemCalcEdit).Mask.UseMaskAsDisplayFormat = true;

                    }
                    break;
                case 9:
                    tmp = new RDateEdit();
                    (tmp as RDateEdit).EditMask = "dd/MM/yyyy";
                    break;
                case 10:
                    tmp = new RepositoryItemCheckEdit();
                    break;
                case 1:
                case 4:
                case 7:
                case 2:
                    if (pType == 2 && dr["refTable"].ToString() == string.Empty)
                        break;
                    tmp = GenRIGridLookupEdit(dr);
                    DevControls.CDTRepGridLookup riTmp = (tmp as DevControls.CDTRepGridLookup);
                    riTmp.CloseUpKey = DevExpress.Utils.KeyShortcut.Empty;
                    riTmp.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    riTmp.NullText = string.Empty;
                    //riTmp.View.BestFitColumns();
                    riTmp.View.OptionsView.ShowAutoFilterRow = true;
                    riTmp.View.OptionsView.ColumnAutoWidth = false;
                    if (!(riTmp.DymicCondition ==null))
                    {
                        this._lstRep.Add(riTmp);
                    }
                    break;
                case 11:
                    tmp = new RepositoryItemTimeEdit();
                    (tmp as RepositoryItemTimeEdit).AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    break;
                case 12:
                    tmp = new RepositoryItemPictureEdit();
                    tmp.NullText = " ";
                    break;
                case 13:
                    tmp = new RepositoryItemMemoEdit();
                    break;
                default:
                    tmp = null;
                    break;
            }
            if (tmp != null)
                tmp.Name = dr["FieldName"].ToString();
            return tmp;
        }

       

        void FormDesigner_Spin1(object sender, DevExpress.XtraEditors.Controls.SpinEventArgs e)
        {
            CalcEdit tmp = (sender as CalcEdit);
            
                tmp.ShowPopup();
            
        }

        

        /// <summary>
        /// Sự kiện nhấn nút tạo mới 1 đối tượng danh mục trong GridLookUpEdit control
        /// </summary>
        private void Plus_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            
           

            if (e.Button.Tag != null)
            {
                CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
                CDTData d = e.Button.Tag as CDTData;
                if (d == null) return;
                BindingSource bs = tmp.Properties.DataSource as BindingSource;
                //if (d.DrTable["Type"].ToString() == "1")
                //{
                //    FrmSingle frm = new FrmSingle(d, bs);
                //    frm.ShowDialog();
                //}
                //else
                //{
                FrmSingleDt frm = new FrmSingleDt(d, bs);
                
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    //BindingSource bs = new BindingSource();
                    //bs.DataSource = d.DsData.Tables[0];
                    string tableName;
                    if (!(tmp.refTable== null))
                    {
                        tableName = tmp.refTable;
                    }
                    else
                    {
                        tableName = tmp.refTable;
                    }
                    foreach (RLookUp_CDTData rgc in _Rlist)
                    {
                        DevControls.CDTRepGridLookup rg = rgc.rglk;
                        if (rg.refTable.ToUpper() == tableName.ToUpper() && rg.View.ViewCaption.ToUpper() == d.Condition.ToUpper())
                        {
                            rg.DataSource = null;
                            rg.DataSource = bs;
                        }

                    }
                    foreach (LookUp_CDTData rgc in _Glist)
                    {
                        CDTGridLookUpEdit rg = rgc.glk;
                        if (rg.refTable.ToUpper() == tableName.ToUpper() && rg.Condition == d.Condition)
                        {
                            rg.Properties.DataSource = null;
                            rg.Properties.DataSource = bs;

                        }
                    }
                    if (tmp.GetType() == typeof(CDTGridLookUpEdit) || _gcMain == null)
                        tmp.EditValue = (bs.List[bs.Count - 1] as System.Data.DataRowView)[tmp.Properties.ValueMember];
                    else //trường hợp repository nằm trong grid
                    {
                        object t = (bs.List[bs.Count - 1] as System.Data.DataRowView)[tmp.Properties.ValueMember];
                        (_gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView).SetFocusedRowCellValue((_gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView).FocusedColumn, t);
                        (_gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView).UpdateCurrentRow();
                        tmp.EditValue = t;//do sự kiện không tự chạy nên phải gọi
                        RIGridLookupEdit_EditValueChanged(tmp, new EventArgs());
                    }
                }
                //}
            }
        }

        void View_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle >= 0)
                e.Info.DisplayText = (e.RowHandle + 1).ToString();                       

        }

        /// <summary>
        /// Khởi tạo cây từ cấu trúc của bảng đang mở
        /// </summary>
        internal TreeList GenTreeListControl(DataRow drTable, DataTable dt)
        {
            TreeList tlMain = new TreeList();
            int bType = Int32.Parse(drTable["Type"].ToString());
            ((ISupportInitialize)tlMain).BeginInit();
            //treelist control
            tlMain.Dock = DockStyle.Fill;
            tlMain.KeyFieldName = drTable["Pk"].ToString();
            tlMain.ParentFieldName = drTable["ParentPk"].ToString();
            tlMain.OptionsView.AutoWidth = false;
            tlMain.OptionsView.EnableAppearanceEvenRow = true;
            tlMain.Visible = false;
            tlMain.OptionsBehavior.Editable = false;
            if (bType == 1 || bType == 4)
                tlMain.OptionsBehavior.Editable = true;

            //tao mang luu cac cot treelist
            int reCol = 0, deCol = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if (Int32.Parse(dr["Type"].ToString()) == 1 && tlMain.ParentFieldName.ToUpper() != dr["FieldName"].ToString().ToUpper())
                    reCol++;
                if (tlMain.ParentFieldName.ToUpper() == dr["FieldName"].ToString().ToUpper())
                    deCol++;
            }
            DevExpress.XtraTreeList.Columns.TreeListColumn[] tlcls = new DevExpress.XtraTreeList.Columns.TreeListColumn[dt.Rows.Count + reCol - deCol];
            //treelist column
            int exColNum = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                if (tlMain.ParentFieldName.ToUpper() == dr["FieldName"].ToString().ToUpper())
                {
                    exColNum--;
                    continue;
                }
                DevExpress.XtraTreeList.Columns.TreeListColumn tlcl = GenTreeListColumn(dr, exColNum, false);
                RepositoryItem ri = GenRepository(dr);
                //get repository control is gen
                if (ri != null)
                {
                    tlMain.RepositoryItems.Add(ri);
                    tlcl.ColumnEdit = ri;
                }
                tlcls[i + exColNum] = tlcl;
                //pType = 1: them cot Dien giai cho ma (fieldname cua cot la gia tri DisplayMember trong sysField)
                int pType = Int32.Parse(dr["Type"].ToString());
                if (pType == 1 && dr["DisplayMember"].ToString() != string.Empty)
                {
                    DevExpress.XtraTreeList.Columns.TreeListColumn tlcl1 = GenTreeListColumn(dr, exColNum, false);
                    RepositoryItem ri1 = GenRepository(dr);
                    //get repository control is gen
                    if (ri1 != null)
                    {   //hieu chinh lai cac thuoc tinh cua cot Dien giai nay
                        string displayMember = dr["DisplayMember"].ToString();
                        ((DevControls.CDTRepGridLookup)ri1).DisplayMember = displayMember;
                        tlMain.RepositoryItems.Add(ri1);
                        string caption;
                        if (Config.GetValue("Language").ToString() == "0")
                            caption = "Tên " + dr["LabelName"].ToString().ToLower();
                        else
                            caption = dr["LabelName2"].ToString() + " name";
                        if (!Boolean.Parse(dr["AllowNull"].ToString()))
                            caption = "*" + caption;
                        tlcl1.Caption = caption;
                        tlcl1.VisibleIndex = tlcl1.VisibleIndex + 1;
                        tlcl1.Width = tlcl1.Width + 100;
                        tlcl1.ColumnEdit = ri1;
                        tlcl1.Visible = tlcl.Visible;
                    }
                    exColNum++;
                    tlcls[i + exColNum] = tlcl1;
                }
            }
            tlMain.Columns.AddRange(tlcls);
            ((ISupportInitialize)tlMain).EndInit();
            return tlMain;
        }

        /// <summary>
        /// Hàm tạo ra TreeListColumn cho TreeView
        /// </summary>
        /// <param name="dr">Thông tin về field hiện tại của bảng (1 dòng trong sysField)</param>
        /// <param name="exColNum">Số lượng cột mở rộng cho trường hợp có bảng danh mục tham chiếu</param>
        private DevExpress.XtraTreeList.Columns.TreeListColumn GenTreeListColumn(DataRow dr, int exColNum, bool checkData)
        {
            DevExpress.XtraTreeList.Columns.TreeListColumn tlcl = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            tlcl.Name = "cl" + dr["FieldName"].ToString();
            tlcl.FieldName = dr["FieldName"].ToString();
            tlcl.Caption = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
            tlcl.VisibleIndex = Int32.Parse(dr["TabIndex"].ToString()) + exColNum;
            if (!checkData)
                tlcl.Visible = Boolean.Parse(dr["Visible"].ToString());
            return tlcl;
        }

        public LayoutControl GenLayout1(ref GridControl gcMain, bool isCBSControl)
        {
            DataTable dt = _data.DsStruct.Tables[0];
            LayoutControl lcMain = new LayoutControl();
            LayoutControlGroup lcgMain = lcMain.Root;
            ((ISupportInitialize)lcMain).BeginInit();
            lcMain.SuspendLayout();
            ((ISupportInitialize)lcgMain).BeginInit();

            //layout control
            lcMain.Dock = DockStyle.Fill;
            lcMain.OptionsView.HighlightFocusedItem = true;

            //layout control group
            lcgMain.TextVisible = false;

            //layout control item
            dt.DefaultView.RowFilter = "Visible = 1";
            if (gcMain != null)
                dt.DefaultView.RowFilter += " and IsBottom = 0";
            bool admin = Boolean.Parse(Config.GetValue("Admin").ToString());
            if (!admin)
                dt.DefaultView.RowFilter += " and (Viewable is null or Viewable = 1)";
            for (int i = 0; i < dt.DefaultView.Count; i++)
            {
                DataRow dr = dt.DefaultView[i].Row;
                //get control is gen
                BaseEdit ctrl = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                if (ctrl == null)
                    continue;
                if (_firstControl == null)
                    _firstControl = ctrl;
                ctrl.StyleController = lcMain;

                int pType = Int32.Parse(dr["Type"].ToString());

                LayoutControlItem lci = new LayoutControlItem();
                string caption = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                if (!Boolean.Parse(dr["AllowNull"].ToString()))
                    caption = "*" + caption;
                lci.Text = caption;
                lci.Control = ctrl;
                if (!Boolean.Parse(dr["Visible"].ToString()))
                    lci.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lcMain.Controls.Add(ctrl);
                lcgMain.AddItem(lci);
                //pType = 1: them control Dien giai cho ma (binding cua control la gia tri DisplayMember trong sysField)
                if (_formAction != FormAction.Filter && pType == 1 && dr["DisplayMember"].ToString() != string.Empty)
                {
                    BaseEdit ctrl1 = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                    ((CDTGridLookUpEdit)ctrl1).Properties.DisplayMember = dr["DisplayMember"].ToString();
                    ctrl1.StyleController = lcMain;
                    LayoutControlItem lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                        caption = "Tên " + dr["LabelName"].ToString().ToLower();
                    else
                        caption = dr["LabelName2"].ToString() + " name";
                    if (!Boolean.Parse(dr["AllowNull"].ToString()))
                        caption = "*" + caption;
                    lci1.Text = caption;
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    lcgMain.AddItem(lci1);
                }
                //isBetween = true: tao ra 2 control from va tu
                if (_formAction == FormAction.Filter && Boolean.Parse(dr["IsBetween"].ToString()))
                {
                    BaseEdit ctrl1 = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                    ctrl.Name = ctrl.Name + "1";
                    ctrl.DataBindings.Add("EditValue", _bindingSource, dr["FieldName"].ToString() + "1");
                    if (Config.GetValue("Language").ToString() == "0")
                        lci.Text = "Từ " + dr["LabelName"].ToString().ToLower();
                    else
                        lci.Text = "From " + dr["LabelName2"].ToString().ToLower();
                    ctrl1.Name = ctrl1.Name + "2";
                    ctrl1.DataBindings.Add("EditValue", _bindingSource, dr["FieldName"].ToString() + "2");
                    ctrl1.StyleController = lcMain;
                    LayoutControlItem lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                        lci1.Text = "Đến " + dr["LabelName"].ToString().ToLower();
                    else
                        lci1.Text = "To " + dr["LabelName2"].ToString().ToLower();
                    if (!Boolean.Parse(dr["AllowNull"].ToString()))
                    {
                        lci.Text = "*" + lci.Text;
                        lci1.Text = "*" + lci1.Text;
                    }
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    lcgMain.AddItem(lci1);
                }
            }
            dt.DefaultView.RowFilter = "Visible = 1";
            if (!admin)
                dt.DefaultView.RowFilter += " and (Viewable is null or Viewable = 1)";
            if (gcMain != null)
            {
                //grid control
                lcgMain.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                LayoutControlGroup lcg3 = lcgMain.AddGroup();
                LayoutControlItem lcit = new LayoutControlItem();
                gcMain = GenGridControl(_data.DsStruct.Tables[1], true, DockStyle.None);
                lcg3.TextVisible = false;
                lcg3.GroupBordersVisible = false;
                lcit.Name = "Detail";
                lcit.TextVisible = false;
                lcit.Control = gcMain;
                lcMain.Controls.Add(gcMain);
                lcg3.AddItem(lcit);

                //bottom layout control group
                dt.DefaultView.RowFilter += " and IsBottom = 1";
                if (dt.DefaultView.Count > 0)
                {
                    LayoutControlGroup lcg4 = lcgMain.AddGroup();
                    lcg4.TextVisible = false;
                    lcg4.GroupBordersVisible = false;
                    for (int i = 0; i < dt.DefaultView.Count; i++)
                    {
                        DataRow dr = dt.DefaultView[i].Row;
                        //get control is gen
                        BaseEdit ctrl = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                        if (ctrl == null)
                            continue;
                        ctrl.StyleController = lcMain;
                        LayoutControlItem lci = new LayoutControlItem();
                        lci.Text = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                        lci.Control = ctrl;
                        lcMain.Controls.Add(ctrl);
                        lcg4.AddItem(lci);
                    }
                }
            }
            ((ISupportInitialize)lcMain).EndInit();
            lcMain.ResumeLayout(false);
            ((ISupportInitialize)lcgMain).EndInit();
            return lcMain;
        }

        public LayoutControl GenLayout2(ref GridControl gcMain, bool isCBSControl)
        {
            DataTable dt = _data.DsStruct.Tables[0];
            LayoutControl lcMain = new LayoutControl();
            LayoutControlGroup lcgMain = lcMain.Root;
            ((ISupportInitialize)lcMain).BeginInit();
            lcMain.SuspendLayout();
            ((ISupportInitialize)lcgMain).BeginInit();

            //layout control
            lcMain.Dock = DockStyle.Fill;
            lcMain.OptionsView.HighlightFocusedItem = true;

            //layout control group
            lcgMain.TextVisible = false;
            lcgMain.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Horizontal;
            LayoutControlGroup lcg1 = lcgMain.AddGroup();
            lcg1.TextVisible = false;
            lcg1.GroupBordersVisible = false;
            LayoutControlGroup lcg2 = lcgMain.AddGroup();
            lcg2.TextVisible = false;
            lcg2.GroupBordersVisible = false;
            lcg1.Size = new Size(lcgMain.Size.Width / 2 + 20, 0);

            //layout control item
            //dt.DefaultView.RowFilter = "Visible = 1";
            if (gcMain != null)
                dt.DefaultView.RowFilter = "  IsBottom = 0";
            bool admin = Boolean.Parse(Config.GetValue("Admin").ToString());
            if (!admin)
                if (dt.DefaultView.RowFilter == "")
                    dt.DefaultView.RowFilter += " (Viewable is null or Viewable = 1)";
                else 
                dt.DefaultView.RowFilter += " and (Viewable is null or Viewable = 1)";
            for (int i = 0; i < dt.DefaultView.Count; i++)
            {
                DataRow dr = dt.DefaultView[i].Row;
                //get control is gen
                BaseEdit ctrl = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                if (ctrl == null)
                    continue;
                if (_firstControl == null)
                    _firstControl = ctrl;
                ctrl.StyleController = lcMain;

                int pType = Int32.Parse(dr["Type"].ToString());

                LayoutControlItem lci = new LayoutControlItem();
                string caption = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                if (!Boolean.Parse(dr["AllowNull"].ToString()))
                    caption = "*" + caption;
                lci.Text = caption;
                lci.Control = ctrl;
                lcMain.Controls.Add(ctrl);
                if (dr["Visible"].ToString() == "False")
                    lci.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                _LayoutList.Add(lci);
                _BaseList.Add(ctrl);
                if (i < dt.DefaultView.Count / 2)
                    lcg1.AddItem(lci);
                else
                    lcg2.AddItem(lci);
                //pType = 1: them control Dien giai cho ma (binding cua control la gia tri DisplayMember trong sysField)
                if (_formAction != FormAction.Filter && pType == 1 && dr["DisplayMember"].ToString() != string.Empty && dr["Visible"].ToString() == "True")
                {
                    BaseEdit ctrl1 = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                    ((CDTGridLookUpEdit)ctrl1).Properties.DisplayMember = dr["DisplayMember"].ToString();
                    ctrl1.StyleController = lcMain;
                    LayoutControlItem lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                        caption = "Tên " + dr["LabelName"].ToString().ToLower();
                    else
                        caption = dr["LabelName2"].ToString() + " name";
                    if (!Boolean.Parse(dr["AllowNull"].ToString()))
                        caption = "*" + caption;
                    lci1.Text = caption;
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    if (i < dt.DefaultView.Count / 2)
                        lcg1.AddItem(lci1);
                    else
                        lcg2.AddItem(lci1);
                }
                //isBetween = true: tao ra 2 control from va tu
                if (_formAction == FormAction.Filter && Boolean.Parse(dr["IsBetween"].ToString()) && dr["Visible"].ToString() == "True")
                {
                    BaseEdit ctrl1 = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                    ctrl.Name = ctrl.Name + "1";
                    ctrl.DataBindings.Add("EditValue", _bindingSource, dr["FieldName"].ToString() + "1");
                    if (Config.GetValue("Language").ToString() == "0")
                        lci.Text = "Từ " + dr["LabelName"].ToString().ToLower();
                    else
                        lci.Text = "From " + dr["LabelName2"].ToString().ToLower();
                    ctrl1.Name = ctrl1.Name + "2";
                    ctrl1.DataBindings.Add("EditValue", _bindingSource, dr["FieldName"].ToString() + "2");
                    ctrl1.StyleController = lcMain;
                    LayoutControlItem lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                        lci1.Text = "Đến " + dr["LabelName"].ToString().ToLower();
                    else
                        lci1.Text = "To " + dr["LabelName2"].ToString().ToLower();

                    if (!Boolean.Parse(dr["AllowNull"].ToString()))
                    {
                        lci.Text = "*" + lci.Text;
                        lci1.Text = "*" + lci1.Text;
                    }
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    if (i < dt.DefaultView.Count / 2)
                        lcg1.AddItem(lci1);
                    else
                        lcg2.AddItem(lci1);
                }
            }
            //dt.DefaultView.RowFilter = "Visible = 1";
            if (!admin)
                dt.DefaultView.RowFilter = "  (Viewable is null or Viewable = 1)";
            if (gcMain != null)
            {
                //grid control
                lcgMain.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                LayoutControlGroup lcg3 = lcgMain.AddGroup();
                LayoutControlItem lcit = new LayoutControlItem();
                gcMain = GenGridControl(_data.DsStruct.Tables[1], true, DockStyle.None);
                lcg3.TextVisible = false;
                lcg3.GroupBordersVisible = false;
                lcit.Name = "Detail";
                lcit.TextVisible = false;
                lcit.Control = gcMain;
                lcMain.Controls.Add(gcMain);
                lcg3.AddItem(lcit);

                //bottom layout control group
                dt.DefaultView.RowFilter = "  IsBottom = 1";
                if (dt.DefaultView.Count > 0)
                {
                    LayoutControlGroup lcg4 = lcgMain.AddGroup();
                    lcg4.TextVisible = false;
                    lcg4.GroupBordersVisible = false;
                    lcg4.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Horizontal;
                    LayoutControlGroup lcg5 = lcg4.AddGroup();
                    lcg5.TextVisible = false;
                    lcg5.GroupBordersVisible = false;
                    lcg5.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                    LayoutControlGroup lcg6 = lcg4.AddGroup();
                    lcg6.TextVisible = false;
                    lcg6.GroupBordersVisible = false;
                    lcg6.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                    for (int i = 0; i < dt.DefaultView.Count; i++)
                    {
                        DataRow dr = dt.DefaultView[i].Row;
                        //get control is gen
                        BaseEdit ctrl = GenCBSControl(dr);
                        if (ctrl == null)
                            continue;
                        ctrl.StyleController = lcMain;
                        LayoutControlItem lci = new LayoutControlItem();
                        lci.Text = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                        lci.Control = ctrl;
                        if (dr["Visible"].ToString() == "False")
                            lci.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                        lcMain.Controls.Add(ctrl);
                        _BaseList.Add(ctrl);
                        _LayoutList.Add(lci);
                        if (i < dt.DefaultView.Count / 2)
                            lcg5.AddItem(lci);
                        else
                            lcg6.AddItem(lci);
                    }
                }
                dt.DefaultView.RowFilter = string.Empty;
            }
            ((ISupportInitialize)lcMain).EndInit();
            lcMain.ResumeLayout(false);
            ((ISupportInitialize)lcgMain).EndInit();
            return lcMain;
        }
        public LayoutControl GenLayout3(ref GridControl gcMain, bool isCBSControl)
        {
            DataTable dt = _data.DsStruct.Tables[0];
            LayoutControl lcMain = new LayoutControl();
            LayoutControlGroup lcgMain = lcMain.Root;
            ((ISupportInitialize)lcMain).BeginInit();
            lcMain.SuspendLayout();
            ((ISupportInitialize)lcgMain).BeginInit();

            //layout control
            lcMain.Dock = DockStyle.Fill;
            
            lcMain.OptionsView.HighlightFocusedItem = true;

            //layout control group
            lcgMain.TextVisible = false;
            lcgMain.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Horizontal;


            LayoutControlGroup lcg1 = lcgMain.AddGroup();

            lcg1.TextVisible = false;
            lcg1.GroupBordersVisible = false;

            LayoutControlGroup lcg2 = lcgMain.AddGroup();
            lcg2.TextVisible = false;
            lcg2.GroupBordersVisible = false;

            LayoutControlGroup lcg3 =  lcgMain.AddGroup();          
            lcg3.TextVisible = false;
            lcg3.GroupBordersVisible = false;

            lcg1.Size = new Size(lcgMain.Size.Width / 3+20, 0);

            //layout control item
            //dt.DefaultView.RowFilter = "Visible = 1";
            if (gcMain != null)
                dt.DefaultView.RowFilter = "  IsBottom = 0";
            bool admin = Boolean.Parse(Config.GetValue("Admin").ToString());
            if (!admin)
                if (dt.DefaultView.RowFilter == "")
                    dt.DefaultView.RowFilter += " (Viewable is null or Viewable = 1)";
                else
                    dt.DefaultView.RowFilter += " and (Viewable is null or Viewable = 1)";
            DataRow[] drCount=dt.Select("Visible = 1 and IsBottom=0");
            DataRow[] drCountDisplay = dt.Select("DisplayMember is not null and DisplayMember <>'' and IsBottom=0");
            int ControlCount = drCount.Length + drCountDisplay.Length;
            int j = 0;
            for (int i = 0; i < dt.DefaultView.Count; i++)
            {
                DataRow dr = dt.DefaultView[i].Row;
                //get control is gen
                BaseEdit ctrl = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                if (ctrl == null)
                    continue;
                if (_firstControl == null)
                    _firstControl = ctrl;
                ctrl.StyleController = lcMain;

                int pType = Int32.Parse(dr["Type"].ToString());

                LayoutControlItem lci = new LayoutControlItem();
                string caption = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                if (!Boolean.Parse(dr["AllowNull"].ToString()))
                    caption = "*" + caption;
                lci.Text = caption;
                lci.Control = ctrl;
                lcMain.Controls.Add(ctrl);
                if (dr["Visible"].ToString() == "False")
                {
                    lci.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                }
                else
                {
                    j++;
                }
                _LayoutList.Add(lci);
                _BaseList.Add(ctrl);

                if (j <= ControlCount / 3)
                    lcg1.AddItem(lci);
                else if (j <= ControlCount * 2 / 3)
                    lcg2.AddItem(lci);
                else
                    lcg3.AddItem(lci);
                //pType = 1: them control Dien giai cho ma (binding cua control la gia tri DisplayMember trong sysField)
                if (_formAction != FormAction.Filter && pType == 1 && dr["DisplayMember"].ToString() != string.Empty && dr["Visible"].ToString() == "True")
                {
                    BaseEdit ctrl1 = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                    ((CDTGridLookUpEdit)ctrl1).Properties.DisplayMember = dr["DisplayMember"].ToString();
                    ctrl1.StyleController = lcMain;
                    LayoutControlItem lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                        caption = "Tên " + dr["LabelName"].ToString().ToLower();
                    else
                        caption = dr["LabelName2"].ToString() + " name";
                    if (!Boolean.Parse(dr["AllowNull"].ToString()))
                        caption = "*" + caption;
                    lci1.Text = caption;
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    j++;
                    if (j <= ControlCount / 3)
                        lcg1.AddItem(lci1);
                    else if (j <= ControlCount * 2 / 3)
                        lcg2.AddItem(lci1);
                    else
                        lcg3.AddItem(lci1);
                }
                //isBetween = true: tao ra 2 control from va tu
                if (_formAction == FormAction.Filter && Boolean.Parse(dr["IsBetween"].ToString()) && dr["Visible"].ToString() == "True")
                {
                    BaseEdit ctrl1 = isCBSControl ? GenCBSControl(dr) : GenControl(dr);
                    ctrl.Name = ctrl.Name + "1";
                    ctrl.DataBindings.Add("EditValue", _bindingSource, dr["FieldName"].ToString() + "1");
                    if (Config.GetValue("Language").ToString() == "0")
                        lci.Text = "Từ " + dr["LabelName"].ToString().ToLower();
                    else
                        lci.Text = "From " + dr["LabelName2"].ToString().ToLower();
                    ctrl1.Name = ctrl1.Name + "2";
                    ctrl1.DataBindings.Add("EditValue", _bindingSource, dr["FieldName"].ToString() + "2");
                    ctrl1.StyleController = lcMain;
                    LayoutControlItem lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                        lci1.Text = "Đến " + dr["LabelName"].ToString().ToLower();
                    else
                        lci1.Text = "To " + dr["LabelName2"].ToString().ToLower();

                    if (!Boolean.Parse(dr["AllowNull"].ToString()))
                    {
                        lci.Text = "*" + lci.Text;
                        lci1.Text = "*" + lci1.Text;
                    }
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    if (j<= ControlCount / 3)
                        lcg1.AddItem(lci);
                    else if (j <= ControlCount * 2 / 3)
                        lcg2.AddItem(lci);
                    else
                        lcg3.AddItem(lci);
                }
            }
            //dt.DefaultView.RowFilter = "Visible = 1";
            if (!admin)
                dt.DefaultView.RowFilter = "  (Viewable is null or Viewable = 1)";
            if (gcMain != null)
            {
                //grid control
                lcgMain.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                LayoutControlGroup lcgBt = lcgMain.AddGroup();
                LayoutControlItem lcit = new LayoutControlItem();
                gcMain = GenGridControl(_data.DsStruct.Tables[1], true, DockStyle.None);
                lcgBt.TextVisible = false;
                lcgBt.GroupBordersVisible = false;
                lcit.Name = "Detail";
                lcit.TextVisible = false;
                lcit.Control = gcMain;
                lcMain.Controls.Add(gcMain);
                lcgBt.AddItem(lcit);

                //bottom layout control group
                dt.DefaultView.RowFilter = "  IsBottom = 1";
                if (dt.DefaultView.Count > 0)
                {
                    LayoutControlGroup lcg4 = lcgMain.AddGroup();
                    lcg4.TextVisible = false;
                    lcg4.GroupBordersVisible = false;
                    lcg4.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Horizontal;
                    LayoutControlGroup lcg5 = lcg4.AddGroup();
                    lcg5.TextVisible = false;
                    lcg5.GroupBordersVisible = false;
                    lcg5.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                    LayoutControlGroup lcg6 = lcg4.AddGroup();
                    lcg6.TextVisible = false;
                    lcg6.GroupBordersVisible = false;
                    lcg6.DefaultLayoutType = DevExpress.XtraLayout.Utils.LayoutType.Vertical;
                    for (int i = 0; i < dt.DefaultView.Count; i++)
                    {
                        DataRow dr = dt.DefaultView[i].Row;
                        //get control is gen
                        BaseEdit ctrl = GenCBSControl(dr);
                        if (ctrl == null)
                            continue;
                        ctrl.StyleController = lcMain;
                        LayoutControlItem lci = new LayoutControlItem();
                        lci.Text = Config.GetValue("Language").ToString() == "0" ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                        lci.Control = ctrl;
                        if (dr["Visible"].ToString() == "False")
                            lci.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                        lcMain.Controls.Add(ctrl);
                        _BaseList.Add(ctrl);
                        _LayoutList.Add(lci);
                        if (i < dt.DefaultView.Count / 2)
                            lcg5.AddItem(lci);
                        else
                            lcg6.AddItem(lci);
                    }
                }
                dt.DefaultView.RowFilter = string.Empty;
            }
            ((ISupportInitialize)lcMain).EndInit();
            lcMain.ResumeLayout(false);
            ((ISupportInitialize)lcgMain).EndInit();
            return lcMain;
        }

        public BaseEdit GenControl(DataRow dr)
        {
            BaseEdit tmp;
            string dataMember = dr["FieldName"].ToString();
            int pType = Int32.Parse(dr["Type"].ToString());
            //0: text(pk); 1: text(fk); 2: text; 3: int(pk); 4: int(fk); 5: int; 6: unique identifier; 
            //7: unique identifier(fk); 8: decimal; 9: date; 10: boolean; 11: time; 12: image;
            switch (pType)
            {
                case 0:
                    tmp = new CBSControls.VTextEdit();
                    (tmp as CBSControls.VTextEdit).EnterMoveNextControl = true;
                    (tmp as CBSControls.VTextEdit).Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    (tmp as CBSControls.VTextEdit).Properties.MaxLength = 32;
                    (tmp as CBSControls.VTextEdit).Properties.CharacterCasing = CharacterCasing.Upper;
                    break;
                case 2:
                    tmp = new CBSControls.VTextEdit();
                    (tmp as CBSControls.VTextEdit).EnterMoveNextControl = true;
                    (tmp as CBSControls.VTextEdit).Properties.MaxLength = 255;
                    (tmp as CBSControls.VTextEdit).Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    break;
                case 5:
                    tmp = new CBSControls.VSpinEdit();
                    (tmp as VSpinEdit).EnterMoveNextControl = true;
                    (tmp as VSpinEdit).Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    break;
                case 8:
                    tmp = new VCalcEdit();
                    (tmp as VCalcEdit).EnterMoveNextControl = true;
                    (tmp as VCalcEdit).Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    (tmp as VCalcEdit).Spin+=new DevExpress.XtraEditors.Controls.SpinEventHandler(FormDesigner_Spin1);
                    (tmp as VCalcEdit).KeyUp += new KeyEventHandler(VCalEdit_KeyUp);
                    if (dr["EditMask"].ToString() != string.Empty)
                    {
                        (tmp as VCalcEdit).Properties.EditMask = dr["EditMask"].ToString();
                        (tmp as VCalcEdit).Properties.Mask.UseMaskAsDisplayFormat = true;
                    }
                    break;
                case 9:
                    tmp = new VDateEdit();
                    break;
                case 14:
                    tmp = new VDateEdit();
                    (tmp as VDateEdit).Properties.EditMask = "dd/MM/yyyy HH:mm:ss";
                    break;
                case 10:
                    tmp = new VCheckEdit();
                    break;
                case 1:
                case 4:
                case 7:
                    tmp = GenGridLookupEdit(dr, false);
                    CDTGridLookUpEdit tmpGrd = tmp as CDTGridLookUpEdit;
                    tmpGrd.Properties.CloseUpKey = DevExpress.Utils.KeyShortcut.Empty;
                    tmpGrd.EnterMoveNextControl = true;
                    tmpGrd.Properties.NullText = string.Empty;
                    tmpGrd.Properties.ImmediatePopup = true;
                    tmpGrd.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    tmpGrd.Properties.View.OptionsView.ShowAutoFilterRow = true;
                    tmpGrd.Properties.View.OptionsView.ColumnAutoWidth = false;
                    if ((tmpGrd.DymicCondition != "" && this.formAction !=FormAction.Filter))
                    {
                        // this._lstRep.Add(tmpGrd.Properties);
                        this._lstRep.Add((CDTRepGridLookup)(tmpGrd.Properties));
                    }
                    break;
                case 11:
                    tmp = new TimeEdit();
                    (tmp as TimeEdit).EnterMoveNextControl = true;
                    (tmp as TimeEdit).Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                    break;
                case 12:
                    tmp = new PictureEdit();
                    if (Config.GetValue("Language").ToString() == "0")
                        tmp.Properties.NullText = "Click phải chuột chọn nạp hình";
                    tmp.DataBindings.Add("EditValue", _bindingSource, dataMember, true, DataSourceUpdateMode.OnValidation);
                    break;
                case 13:
                    tmp = new MemoEdit();
                    break;
                default:
                    tmp = null;
                    break;
            }
            if (tmp != null)
            {
                tmp.Name = dr["FieldName"].ToString();
                tmp.ToolTip = dr["Tip"].ToString();
                if (pType != 12)
                    if (_formAction != FormAction.Filter || !Boolean.Parse(dr["IsBetween"].ToString()))
                        tmp.DataBindings.Add("EditValue", _bindingSource, dataMember, false, DataSourceUpdateMode.OnValidation);
                tmp.TabIndex = Int32.Parse(dr["TabIndex"].ToString());
                bool admin = Boolean.Parse(Config.GetValue("Admin").ToString());
                if (_formAction == FormAction.Edit && !admin)
                {
                    string canEdit = dr["Editable1"].ToString();
                    tmp.Properties.ReadOnly = canEdit != string.Empty && !Boolean.Parse(canEdit);
                }
                //if (_formAction == FormAction.Edit && !tmp.Properties.ReadOnly)
                //    tmp.Properties.ReadOnly = !Boolean.Parse(dr["Editable"].ToString());

            }

            return tmp;
        }

        void VCalEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                (sender as CalcEdit).ClosePopup();
            }
        }

        public void RefreshViewForLookup()
        {
            //bo qua CDTData dau tien vi no chinh la ParentData
            for (int i = 1; i < _lstData.Count; i++)
                if (_lstData[i].DrTable["CollectType"].ToString() == "-1" && _lstData[i].FullData)
                {
                    _lstData[i].GetData();
                    //phai gan lai datasouce cho cac CDTGridLookUpEdit
                    for (int j = 0; j < _Glist.Count; j++)
                    {
                        if (_Glist[j].dataIndex == i)
                        {
                            BindingSource bs = _Glist[j].glk.Properties.DataSource as BindingSource;
                            bs.DataSource = _lstData[_Glist[j].dataIndex].DsData.Tables[0];
                        }
                    }
                    for (int k = 0; k < _Rlist.Count; k++)
                    {
                        if (_Rlist[k].dataIndex == i)
                        {
                            BindingSource bs = _Rlist[k].rglk.DataSource as BindingSource;
                            bs.DataSource = _lstData[_Rlist[k].dataIndex].DsData.Tables[0];
                        }
                    }
                }
        }
        public void RefreshDataLookupForColChanged()
        {
            for (int i = 1; i < _lstData.Count; i++)
                if (!_lstData[i].FullData)
                    _lstData[i].GetData();
            //phai gan lai datasouce cho cac CDTGridLookUpEdit
            for (int i = 0; i < _Glist.Count; i++)
            {
                BindingSource bs = _Glist[i].glk.Properties.DataSource as BindingSource;
                bs.DataSource = _lstData[_Glist[i].dataIndex].DsData.Tables[0];
            }
            for (int i = 0; i < _Rlist.Count; i++)
            {
                BindingSource bs = _Rlist[i].rglk.DataSource as BindingSource;
                bs.DataSource = _lstData[_Rlist[i].dataIndex].DsData.Tables[0];
            }
        }
        public void RefreshDataForLookup()
        {
            //bo qua CDTData dau tien vi no chinh la ParentData
            for (int i = 1; i < _lstData.Count; i++)
                if (!_lstData[i].FullData)
                    _lstData[i].GetDataForLookup(_data);
            //phai gan lai datasouce cho cac CDTGridLookUpEdit
            for (int i = 0; i < _Glist.Count; i++)
            {
                BindingSource bs = _Glist[i].glk.Properties.DataSource as BindingSource;
                bs.DataSource = _lstData[_Glist[i].dataIndex].DsData.Tables[0];
            }
            for (int i = 0; i < _Rlist.Count; i++)
            {
                BindingSource bs = _Rlist[i].rglk.DataSource as BindingSource;
                bs.DataSource = _lstData[_Rlist[i].dataIndex].DsData.Tables[0];
            }
        }

        private CDTData GetDataForLookup(string tableName, string condition, string DynCondition, ref bool isMaster, ref int n)
        {
            CDTData data = null;
            foreach (CDTData d in _lstData)
            {
                if (d.dataType != DataType.MasterDetail 
                    && d.DrTable["TableName"].ToString().ToUpper() == tableName.ToUpper()
                    && d.DrTable["TableName"].ToString().ToUpper() != _data.DrTable["TableName"].ToString().ToUpper()
                    && d.Condition.Trim() == condition.Trim())
                {
                    n = _lstData.IndexOf(d);
                    data = d;
                    if (d.dataType == DataType.MasterDetail)
                        isMaster = false;
                    else
                        if (d.dataType == DataType.Detail)
                            isMaster = true;
                    break;
                }
                if (d.DrTableMaster != null && d.DrTableMaster["TableName"].ToString().ToUpper() == tableName.ToUpper()
                    && d.ConditionMaster.Trim() == condition.Trim())
                {
                    n = _lstData.IndexOf(d);
                    data = d;
                    if (d.dataType == DataType.MasterDetail)
                        isMaster = true;
                    else
                        if (d.dataType == DataType.Detail)
                            isMaster = false;
                    break;
                }
            }
            if (data == null)
            {
                string sysPackageID = Config.GetValue("sysPackageID").ToString();
                data = CusForm.FormFactory.Create(DataType.Single, tableName, sysPackageID);
                data.Condition = condition;
                data.DynCondition = DynCondition;
                if (tableName == "sysTable" || tableName == "sysField")
                {
                    data.GetData();
                    data.FullData = true;
                }
                else
                    data.GetDataForLookup(_data);
                _lstData.Add(data);
                n = _lstData.Count - 1;
            }
            else
                if (data.DsData == null)
                    data.GetData();

            return data;
        }

        public CDTGridLookUpEdit GenGridLookupEdit(DataRow drField, bool isCBSControl)
        {
             CDTGridLookUpEdit tmp = isCBSControl ? new CDTGridLookUpEdit() : new CDTGridLookUpEdit();
            string refField = drField["RefField"].ToString();
            string refTable = drField["RefTable"].ToString();
            
            string condition;
            if (this._formAction != FormAction.Filter)
            {
                condition = drField["refCriteria"].ToString();
            }
            else
            {
                condition = drField["FilterCond"].ToString();
            }
            string Dyncondition = drField["DynCriteria"].ToString();
            bool isMaster = true; int n = 0;
            CDTData data = null;
            data = GetDataForLookup(refTable, condition, Dyncondition, ref isMaster, ref n);
            
            FormDesigner fd = new FormDesigner(data);
            string displayMember = drField["DisplayMember"].ToString();
            DataTable dt;
            if (isMaster)
                dt = data.DsStruct.Tables[0];
            else
                dt = data.DsStruct.Tables[1];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr1 = dt.Rows[i];
                DevControls.CDTGridColumn gcl = fd.GenGridColumn(dr1, 0, false);
                if (dr1["EditMask"].ToString() != string.Empty)
                {
                    gcl.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gcl.DisplayFormat.FormatString = dr1["EditMask"].ToString();
                }
                tmp.Properties.View.Columns.Add(gcl);
            }
            
            BindingSource bs = new BindingSource();
            if (isMaster)
                bs.DataSource = data.DsData.Tables[0];
            else
                bs.DataSource = data.DsData.Tables[1];
            tmp.Data = data;
            tmp.Properties.DataSource = bs;
            tmp.Properties.ValueMember = refField;

            tmp.refTable= refTable;
            tmp.Properties.Name = drField["FieldName"].ToString();
            tmp.Properties.ValueMember = refField;
            tmp.DymicCondition = data.DynCondition;
            tmp.Properties.View.ViewCaption = condition;
            tmp.Allownull = Boolean.Parse(drField["AllowNull"].ToString());
            tmp.Properties.Buttons[0].Tag = drField;//Dùng để gắn với nút +
            _Glist.Add(new LookUp_CDTData(tmp, n));
            _glist.Add(tmp);
            if (!RIOldValue.ContainsKey(tmp.Properties.Name))
                RIOldValue.Add(tmp.Properties.Name, "");

            tmp.Properties.View.OptionsView.ShowFooter = true;
            int fType = Int32.Parse(drField["Type"].ToString());
            if (fType == 1)
                tmp.Properties.DisplayMember = refField;
            else
                tmp.Properties.DisplayMember = displayMember;
            tmp.Properties.PopupFormMinSize = new Size(600, 100);
            //tmp.Properties.View.BestFitColumns();
            tmp.Properties.View.IndicatorWidth = 40;
            tmp.Properties.View.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(View_CustomDrawRowIndicator);
            tmp.EditValueChanged += new EventHandler(GridLookupEdit_EditValueChanged);
            tmp.Validated += new EventHandler(GridLookupEdit_Validated);
            tmp.Popup += new EventHandler(GridLookupEdit_Popup);
            tmp.KeyDown += new KeyEventHandler(GridLookupEdit_KeyDown);
            tmp.Properties.View.KeyDown += new KeyEventHandler(View_KeyDown);
            //if (!)
            //    tmp.KeyDown += new KeyEventHandler(GridLookupEdit_KeyDown);
            if (_formAction != FormAction.Filter)
            {
                DevExpress.XtraEditors.Controls.EditorButton plusBtn = new DevExpress.XtraEditors.Controls.EditorButton(data, DevExpress.XtraEditors.Controls.ButtonPredefines.Plus);
                plusBtn.Shortcut = new DevExpress.Utils.KeyShortcut(Keys.F2);
                tmp.Properties.Buttons.Add(plusBtn);
                tmp.Properties.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(Plus_ButtonClick);
            }
            return tmp;
        }

        void GridLookupEdit_Validated(object sender, EventArgs e)
        {
            CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
            setDynFiter(tmp);
        }



        void View_KeyDown(object sender, KeyEventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView tmp = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (tmp.OptionsView.ShowAutoFilterRow && e.KeyCode == Keys.F5)
            {
                tmp.FocusedRowHandle = -999997;
                tmp.FocusedColumn = tmp.VisibleColumns[1];
                tmp.ShowEditor();
            }

        }

        void GridLookupEdit_KeyDown(object sender, KeyEventArgs e)
        {

            CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
            if (!tmp.Allownull)
            {
                if (!tmp.IsPopupOpen && (tmp.EditValue == null || tmp.EditValue.ToString() == string.Empty)
                    && e.KeyCode == Keys.Enter)
                {
                    tmp.ShowPopup();
                    e.Handled = true;
                }
                if (!tmp.IsPopupOpen && tmp.EditValue != null && tmp.EditValue.ToString() != string.Empty
                    && e.KeyCode == Keys.Delete)
                    tmp.EditValue = null;
            }

            
        }
        void RiGridLookupEdit_KeyDown(object sender, KeyEventArgs e)
        {

            GridLookUpEdit tmp = sender as GridLookUpEdit;

                if (!tmp.IsPopupOpen && (tmp.EditValue == null || tmp.EditValue.ToString() == string.Empty)
                    && e.KeyCode == Keys.Enter)
                {
                    tmp.ShowPopup();
                    e.Handled = true;
                }
                if (!tmp.IsPopupOpen && tmp.EditValue != null && tmp.EditValue.ToString() != string.Empty
                    && e.KeyCode == Keys.Delete)
                    tmp.EditValue = null;

            
        }
        
        private void GridLookupEdit_Popup(object sender, EventArgs e)
        {
            CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
            int n = -1, i = 0;
            for (i = 0; i < _Glist.Count; i++)
                if (_Glist[i].glk == tmp)
                {
                    n = _Glist[i].dataIndex;
                    break;
                }
            RefreshLookup(n);
        }

        private DevControls.CDTRepGridLookup GenRIGridLookupEdit(DataRow drField)
        {
            DevControls.CDTRepGridLookup tmp = new DevControls.CDTRepGridLookup();
            string refField = drField["RefField"].ToString();
            string refTable = drField["RefTable"].ToString();
            string condition = drField["refCriteria"].ToString();
            string Dyncondition = drField["DynCriteria"].ToString();
            string displayMember = drField["DisplayMember"].ToString();
            bool isMaster = true; int n = 0;
            CDTData data = GetDataForLookup(refTable, condition,Dyncondition, ref isMaster, ref n);
            FormDesigner fd = new FormDesigner(data);
            DataTable dt;
            if (isMaster)
                dt = data.DsStruct.Tables[0];
            else
                dt = data.DsStruct.Tables[1];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr1 = dt.Rows[i];
                DevControls.CDTGridColumn gcl = fd.GenGridColumn(dr1, 0, false); 
                if (dr1["EditMask"].ToString() != string.Empty)
                {
                    gcl.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gcl.DisplayFormat.FormatString = dr1["EditMask"].ToString();
                }
                
                tmp.View.Columns.Add(gcl);
                if (i == 6) break;
            }

            BindingSource bs = new BindingSource();
            if (isMaster)
                bs.DataSource = data.DsData.Tables[0];
            else
                bs.DataSource = data.DsData.Tables[1];
            
            tmp.DataSource = bs;
            tmp.refTable = refTable;
            tmp.ValueMember = refField;
            tmp.DymicCondition = data.DynCondition;
            tmp.Name = drField["FieldName"].ToString();
            tmp.Condition = condition;
            _Rlist.Add(new RLookUp_CDTData(tmp, n));
            _rlist.Add(tmp);
            //tmp.View.Name = _Rlist.Count.ToString();
            if (!RIOldValue.ContainsKey(tmp.Name))
                RIOldValue.Add(tmp.Name, "");
            int fType = Int32.Parse(drField["Type"].ToString());
            if (fType == 1)                              
                tmp.DisplayMember = refField;
            else
                tmp.DisplayMember = displayMember;
            tmp.PopupFormMinSize = new Size(600, 100);
            //tmp.View.BestFitColumns();
            tmp.View.OptionsView.ShowFooter = true;
            tmp.View.IndicatorWidth = 40;
            tmp.ImmediatePopup = true;
            tmp.View.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(View_CustomDrawRowIndicator);
            DevExpress.XtraEditors.Controls.EditorButton plusBtn = new DevExpress.XtraEditors.Controls.EditorButton(data, DevExpress.XtraEditors.Controls.ButtonPredefines.Plus);

            plusBtn.Shortcut = new DevExpress.Utils.KeyShortcut(Keys.F2);
            tmp.Buttons.Add(plusBtn);

            //tmp.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(Plus_ButtonClick);
            tmp.Button_click +=new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(tmp_RIButton_click);
            tmp.Popup += new EventHandler(RIGridLookupEdit_Popup);
            if (!Boolean.Parse(drField["AllowNull"].ToString()))
                tmp.KeyDown += new KeyEventHandler(RiGridLookupEdit_KeyDown);
            tmp.DymicCondition = Dyncondition;
            //tmp.View.ActiveFilterEnabled=false;
            tmp.EditValueChanged += new EventHandler(RIGridLookupEdit_EditValueChanged);
            tmp.Validating += new CancelEventHandler(RIGridLookupEdit_Validating);
            tmp.View.KeyDown += new KeyEventHandler(View_KeyDown);
           // tmp.View.BestFitColumns();
            return tmp;

        }

        void tmp_RIButton_click(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Tag != null)
            {
                CDTRepGridLookup tmp = sender as CDTRepGridLookup;
                CDTData d = e.Button.Tag as CDTData;
                if (d == null) return;
                BindingSource bs = tmp.DataSource as BindingSource;
                //if (d.DrTable["Type"].ToString() == "1")
                //{
                //    FrmSingle frm = new FrmSingle(d, bs);
                //    frm.ShowDialog();
                //}
                //else
                //{
                FrmSingleDt frm = new FrmSingleDt(d, bs);

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    //BindingSource bs = new BindingSource();
                    //bs.DataSource = d.DsData.Tables[0];
                    string tableName;
                    if (!(tmp.refTable == null))
                    {
                        tableName = tmp.refTable;
                    }
                    else
                    {
                        tableName = tmp.refTable;
                    }
                    foreach (RLookUp_CDTData rgc in _Rlist)
                    {
                        DevControls.CDTRepGridLookup rg = rgc.rglk;
                        if (rg.refTable.ToUpper() == tableName.ToUpper() && rg.View.ViewCaption.ToUpper() == d.Condition.ToUpper())
                        {
                            rg.DataSource = null;
                            rg.DataSource = bs;
                        }

                    }
                    foreach (LookUp_CDTData rgc in _Glist)
                    {
                        CDTGridLookUpEdit rg = rgc.glk;
                        if (rg.refTable.ToUpper() == tableName.ToUpper() && rg.Condition == d.Condition)
                        {
                            rg.Properties.DataSource = null;
                            rg.Properties.DataSource = bs;

                        }
                    }

                        object t = (bs.List[bs.Count - 1] as System.Data.DataRowView)[tmp.ValueMember];
                        (_gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView).SetFocusedRowCellValue((_gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView).FocusedColumn, t);
                        (_gcMain.MainView as DevExpress.XtraGrid.Views.Grid.GridView).UpdateCurrentRow();
                        (tmp.GridLookup).EditValue = t;//do sự kiện không tự chạy nên phải gọi
                        RIGridLookupEdit_EditValueChanged(tmp.GridLookup, new EventArgs());

                }
                //}
            }
        }
        
        private void RIGridLookupEdit_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit tmp = (GridLookUpEdit)sender;

            int n = -1, i = 0;
            for (i = 0; i < _Rlist.Count; i++)
                if (_Rlist[i].rglk.Name == tmp.Properties.Name)
                {
                    n = _Rlist[i].dataIndex;
                    break;
                }
                       
            RefreshLookup(n);
            BindingSource bs = tmp.Properties.DataSource as BindingSource;
            if (tmp.Tag != null)
            {
                bs.Position = int.Parse((tmp.Tag as CDTRepGridLookup).bsCur.ToString());
            }
        }

        public void RefreshFormulaDetail()
        {
            for (int i = 0; i < _Glist.Count; i++)
            {
                CDTGridLookUpEdit tmp = _Glist[i].glk;
                if (tmp.EditValue == null) continue;
                string value = tmp.EditValue.ToString();
                BindingSource bs = tmp.Properties.DataSource as BindingSource;
                int index = tmp.Properties.GetIndexByKeyValue(value);
                if (index < 0 || value == string.Empty)
                    continue;
                DataTable dt = bs.DataSource as DataTable;
                DataRow drData = dt.Rows[index];
                if (drData != null)
                    _data.SetValuesFromList(tmp.Name, value, drData, true);
            }
        }

        void GridLookupEdit_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
                tmp.Refresh();
                if (_formAction == FormAction.View || _formAction == FormAction.Delete)
                    return;
                if (tmp.EditValue == null)
                    return;
                string value = tmp.EditValue.ToString();
                BindingSource bs = tmp.Properties.DataSource as BindingSource;
                int index = tmp.Properties.GetIndexByKeyValue(value);
                if (index < 0 || value == string.Empty)
                    return;
                DataTable dt = bs.DataSource as DataTable;
                DataRow drData = dt.Rows[index];
                if (drData != null && _formAction == FormAction.Filter)
                {
                    
                    for (int i = 0; i < drData.Table.Columns.Count; i++)
                        (this._data as DataReport).reConfig.NewKeyValue("@" + drData.Table.Columns[i].ColumnName, drData[i]);
                    return;
                }
                if (drData != null)
                {
                    _data.SetValuesFromList(tmp.Name, value, drData, false);
                    RefreshDataForLookup(tmp.Name, false);
                }
            }
            catch { }
        }
        
        void RIGridLookupEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_formAction == FormAction.View || _formAction == FormAction.Delete || _formAction == FormAction.Filter)
                return;
            GridLookUpEdit tmp = sender as GridLookUpEdit;
            
            if (tmp.EditValue == null)
                return;
            string value = tmp.EditValue.ToString();
            if (value == string.Empty)
                return;
            int index = tmp.Properties.GetIndexByKeyValue(value);
            BindingSource bs = tmp.Properties.DataSource as BindingSource;
            
            if (index < 0) //trường hợp thêm mới một mục (chưa có trong datarow của tmp)
                index = bs.Count - 1;
            (tmp.Tag as CDTRepGridLookup).bsCur = index;
            DataTable dt = bs.DataSource as DataTable;
            DataRow drData = dt.Rows[index];
            DevExpress.XtraGrid.Views.Grid.GridView gv = _gcMain.Views[0] as DevExpress.XtraGrid.Views.Grid.GridView;
            if (drData != null)
            {
                DataRow drDetail = gv.GetDataRow(gv.FocusedRowHandle);
                if (_data.dataType == DataType.MasterDetail)
                {
                    //_data.SetValuesFromListDt(drDetail, tmp.Properties.Name, value, drData);
                }
                else //truong hop nhap lieu tren luoi
                    _data.SetValuesFromList(tmp.Properties.Name, value, drData, false);
                RefreshDataForLookup(tmp.Properties.Name, _data.dataType == DataType.MasterDetail);
            }
           // setRepFilter(tmp);
            
        }

        private void RefreshDataForLookup(string controlFrom, bool isDetail)
        {
            List<string> lstStr = new List<string>();
            if (isDetail)
            {
                foreach (DataRow drField in _data.DsStruct.Tables[1].Rows)
                {
                    string formulaDetail = drField["FormulaDetail"].ToString();
                    if (formulaDetail == string.Empty)
                        continue;
                    string[] str = formulaDetail.Split(".".ToCharArray());
                    if (controlFrom.ToUpper() != str[0].ToUpper() && !lstStr.Contains(str[0].ToUpper()))
                        continue;
                    lstStr.Add(drField["FieldName"].ToString().ToUpper());
                }
                foreach (string s in lstStr)
                    for (int i = 0; i < _Rlist.Count; i++)
                    {
                        if (_Rlist[i].rglk.Name.ToUpper() == s)
                        {
                            int n = _Rlist[i].dataIndex;
                            RefreshLookup(n);
                        }
                    }
            }
            lstStr.Clear();
            foreach (DataRow drField in _data.DsStruct.Tables[0].Rows)
            {
                string formulaDetail = drField["FormulaDetail"].ToString();
                if (formulaDetail == string.Empty)
                    continue;
                string[] str = formulaDetail.Split(".".ToCharArray());
                if (controlFrom.ToUpper() != str[0].ToUpper() && !lstStr.Contains(str[0].ToUpper()))
                    continue;
                lstStr.Add(drField["FieldName"].ToString().ToUpper());
            }
            foreach (string s in lstStr)
                for (int i = 0; i < _Glist.Count; i++)
                {
                    if (_Glist[i].glk.Name.ToUpper() == s)
                    {
                        int n = _Glist[i].dataIndex;
                        RefreshLookup(n);
                    }
                }
            }

        private void RefreshLookup(int dataIndex)
        {
            if (dataIndex < 0)
                return;
            CDTData data = _lstData[dataIndex];
            if (data.FullData) return;
            if (data.dataType == DataType.MasterDetail)
                if (data.DrTableMaster.Table.Columns.Contains("TableName"))
                    if (data.DrTableMaster["TableName"].ToString() == "sysTable") return;
            data.GetData();
            for (int i = 0; i < _Glist.Count; i++)
                if (_Glist[i].dataIndex == dataIndex)
                {
                    BindingSource bs = _Glist[i].glk.Properties.DataSource as BindingSource;
                    if ((bs.DataSource as DataTable).TableName == data.DsData.Tables[0].TableName)
                        bs.DataSource = data.DsData.Tables[0];
                    else
                        bs.DataSource = data.DsData.Tables[1];
                }
            for (int i = 0; i < _Rlist.Count; i++)
                if (_Rlist[i].dataIndex == dataIndex)
                {
                    BindingSource bs = _Rlist[i].rglk.DataSource as BindingSource;
                    if ((bs.DataSource as DataTable).TableName == data.DsData.Tables[0].TableName)
                        bs.DataSource = data.DsData.Tables[0];
                    else
                        bs.DataSource = data.DsData.Tables[1];
                }

        }

        void RIGridLookupEdit_Validating(object sender, EventArgs e)
        {
            if (_formAction == FormAction.View || _formAction == FormAction.Delete || _formAction == FormAction.Filter)
                return;
            GridLookUpEdit tmp = sender as GridLookUpEdit;
            if (tmp.EditValue == null)
                return;
            string value = tmp.EditValue.ToString();
            if (value == string.Empty)
                return;
            int index = tmp.Properties.GetIndexByKeyValue(value);
            BindingSource bs = tmp.Properties.DataSource as BindingSource;
            if (index < 0) //trường hợp thêm mới một mục (chưa có trong datarow của tmp)
                index = bs.Count - 1;
            DataTable dt = bs.DataSource as DataTable;
            DataRow drData = dt.Rows[index];
            DevExpress.XtraGrid.Views.Grid.GridView gv = _gcMain.Views[0] as DevExpress.XtraGrid.Views.Grid.GridView;
            if (drData != null)
            {
                DataRow drDetail = gv.GetDataRow(gv.FocusedRowHandle);
                if (_data.dataType == DataType.MasterDetail)
                {
                    _data.SetValuesFromListDt(drDetail, tmp.Properties.Name, value, drData);
                }
                //else //truong hop nhap lieu tren luoi
                //    _data.SetValuesFromList(tmp.Properties.Name, value, drData, false);
                //RefreshDataForLookup(tmp.Properties.Name, _data.dataType == DataType.MasterDetail);
            }
        }

        void gvMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            
            CDTRepGridLookup tmp = e.Column.ColumnEdit as CDTRepGridLookup;

            if (tmp == null) return;
            setRepFilter(tmp);
        }
        private void setDynFiter(CDTGridLookUpEdit tmp)
        {
            string  fieldName = "@" + tmp.Name;
            if (tmp.Properties.View.FocusedRowHandle < 0) return;
            string value = tmp.Properties.View.GetDataRow(tmp.Properties.View.FocusedRowHandle)[tmp.Properties.ValueMember].ToString();
            foreach (DevControls.CDTGridLookUpEdit Ri in _glist)
            {
                if (Ri.DymicCondition.ToString().Contains(fieldName))
                {
                    string refFilter = Ri.DymicCondition;
                    Ri.Properties.View.OptionsFilter.BeginUpdate();
                    if (refFilter.Contains("."))
                    {
                        string strReplaced = refFilter.Substring(refFilter.IndexOf("@"));
                        fieldName = refFilter.Substring(refFilter.IndexOf(".") + 1);
                        value = tmp.Properties.View.GetDataRow(tmp.Properties.View.FocusedRowHandle)[fieldName].ToString();
                        string filter = Ri.DymicCondition.Replace(strReplaced, "'" + value + "'");
                        Ri.Properties.View.ActiveFilterString = filter;
                    }
                    else
                    {
                        //fieldName = tmp.Properties.ValueMember;

                        string ActiveFilter = "";
                        if (Ri.DymicCondition != null)
                        { ActiveFilter = Ri.ActiveFilter; }

                        string filter;
                        if (ActiveFilter == "" || ActiveFilter == null)
                        {
                            filter = Ri.DymicCondition.Replace( fieldName, "'" + value + "'");
                        }
                        else
                        {
                            filter = ActiveFilter.Replace("@" + fieldName, "'" + value + "'");
                            //trường hợp chọn rồi chọn lại , giá mà biết giá trị cũ
                            string OldValue = "";
                            if (RIOldValue[tmp.Name].ToString() != "")
                            {
                                OldValue = RIOldValue[tmp.Name].ToString();
                            }
                            if (OldValue != "")
                            {
                                filter = filter.Replace("'" + OldValue + "'", "'" + value + "'");
                            }
                        }
                        try
                        {
                            Ri.Properties.View.ActiveFilterString = filter;
                        }
                        catch { }
                        Ri.ActiveFilter = filter;
                        
                    }
                    Ri.Properties.View.ActiveFilterEnabled = true;
                    Ri.Properties.View.OptionsFilter.EndUpdate();
                    Ri.Properties.View.RefreshEditor(true);
                }
            }
            foreach (DevControls.CDTRepGridLookup Ri in _lstRep)
            {
                if (Ri.DymicCondition.ToString().Contains(fieldName))
                {
                    string refFilter = Ri.DymicCondition;
                    Ri.View.OptionsFilter.BeginUpdate();
                    if (refFilter.Contains("."))
                    {
                        string strReplaced = refFilter.Substring(refFilter.IndexOf("@"));
                        fieldName = refFilter.Substring(refFilter.IndexOf(".") + 1);
                        value = tmp.Properties.View.GetDataRow(tmp.Properties.View.FocusedRowHandle)[fieldName].ToString();
                        string filter = Ri.DymicCondition.Replace(strReplaced, "'" + value + "'");
                        Ri.View.ActiveFilterString = filter;
                    }
                    else
                    {
                        //fieldName = tmp.Properties.ValueMember;

                        string ActiveFilter = "";
                        if (Ri.DymicCondition != null)
                        { ActiveFilter = Ri.ActiveFilter; }

                        string filter;
                        if (ActiveFilter == "" || ActiveFilter == null)
                        {
                            filter = Ri.DymicCondition.Replace(fieldName, "'" + value + "'");
                        }
                        else
                        {
                            filter = ActiveFilter.Replace("@" + fieldName, "'" + value + "'");
                            //trường hợp chọn rồi chọn lại , giá mà biết giá trị cũ
                            string OldValue = "";
                            if (RIOldValue[tmp.Name].ToString() != "")
                            {
                                OldValue = RIOldValue[tmp.Name].ToString();
                            }
                            if (OldValue != "")
                            {
                                filter = filter.Replace("'" + OldValue + "'", "'" + value + "'");
                            }
                        }
                        try
                        {
                            Ri.View.ActiveFilterString = filter;
                        }
                        catch { }
                        Ri.ActiveFilter = filter;

                    }
                    Ri.View.ActiveFilterEnabled = true;
                    Ri.View.OptionsFilter.EndUpdate();
                    Ri.View.RefreshEditor(true);
                }
            }
            if (tmp.Name != null)
            {
                RIOldValue[tmp.Name] = value;//Giá trị chọn lần này
            }
        }
        public void setRepFilter(CDTRepGridLookup tmp)
        {           
            string fieldName = "@" + tmp.Name;
            string value = tmp.View.GetDataRow(tmp.View.FocusedRowHandle)[tmp.ValueMember].ToString();
               // if(tmp. !=null)
               // value = tmp.EditValue.ToString();
            foreach(DevControls.CDTRepGridLookup Ri in _lstRep)
            {
                if (Ri.DymicCondition.ToString().Contains(fieldName))
                {
                    string refFilter = Ri.DymicCondition;
                    
                    if (refFilter.Contains("."))
                    {
                        string strReplaced = refFilter.Substring(refFilter.IndexOf("@"));
                        fieldName = refFilter.Substring(refFilter.IndexOf(".") + 1);
                        value = tmp.View.GetDataRow(tmp.View.FocusedRowHandle)[fieldName].ToString();
                        string filter = Ri.DymicCondition.Replace(strReplaced, "'" + value + "'");
                        Ri.View.OptionsFilter.BeginUpdate();
                        Ri.View.ActiveFilterString = filter;
                        Ri.View.ActiveFilterEnabled = true;
                        Ri.View.OptionsFilter.EndUpdate();
                        Ri.View.RefreshEditor(true);
                    }
                    else
                    {
                       // fieldName = tmp.ValueMember;
                        string ActiveFilter ="";
                        if (Ri.DymicCondition!= null)
                        { ActiveFilter = Ri.ActiveFilter; }
                        
                        string filter;
                        if (ActiveFilter=="" ||ActiveFilter==null)
                        {
                            filter = Ri.DymicCondition.Replace(fieldName, "'" + value + "'");
                        }else
                        {
                            filter = ActiveFilter.Replace("@" + fieldName, "'" + value + "'");
                            filter = ActiveFilter.Replace( fieldName, "'" + value + "'");
                            //trường hợp chọn rồi chọn lại , giá mà biết giá trị cũ
                            string OldValue = "";
                            if (RIOldValue[tmp.Name].ToString()!="")
                            {
                                OldValue = RIOldValue[tmp.Name].ToString();
                            }
                            if (OldValue != "")
                            {
                                filter = filter.Replace("'" + OldValue + "'", "'" + value + "'");
                            }
                        }
                        if (ActiveFilter != filter)
                        {
                            Ri.View.OptionsFilter.BeginUpdate();
                            try
                            { Ri.View.ActiveFilterString = filter; }
                            catch { } 
                            Ri.ActiveFilter = filter; 
                            Ri.View.ActiveFilterEnabled = true;
                            Ri.View.OptionsFilter.EndUpdate();
                            Ri.View.RefreshEditor(true);
                        }    
                        
                    }
                    
                    
                   // Ri.View.BestFitColumns();
                }
            }
            if (tmp.Name != null)
            {
                RIOldValue[tmp.Name] = value;//Giá trị chọn lần này
            }

        }
        public void setRepFilter(CDTRepGridLookup tmp,string _value)
        {
            string fieldName = "@" + tmp.Name;
            string value = _value;//tmp.View.GetDataRow(tmp.View.FocusedRowHandle)[tmp.ValueMember].ToString();
            // if(tmp. !=null)
            // value = tmp.EditValue.ToString();
            foreach (DevControls.CDTRepGridLookup Ri in _lstRep)
            {
                if (Ri.DymicCondition.ToString().Contains(fieldName))
                {
                    string refFilter = Ri.DymicCondition;
                    Ri.View.OptionsFilter.BeginUpdate();
                    if (refFilter.Contains("."))
                    {
                        string strReplaced = refFilter.Substring(refFilter.IndexOf("@"));
                        fieldName = refFilter.Substring(refFilter.IndexOf(".") + 1);
                        value = tmp.View.GetDataRow(tmp.View.FocusedRowHandle)[fieldName].ToString();
                        string filter = Ri.DymicCondition.Replace(strReplaced, "'" + value + "'");
                        Ri.View.ActiveFilterString = filter;
                    }
                    else
                    {
                        // fieldName = tmp.ValueMember;

                        string ActiveFilter = "";
                        if (Ri.DymicCondition != null)
                        { ActiveFilter = Ri.ActiveFilter; }

                        string filter;
                        if (ActiveFilter == "" || ActiveFilter == null)
                        {
                            filter = Ri.DymicCondition.Replace(fieldName, "'" + value + "'");
                        }
                        else
                        {
                            filter = ActiveFilter.Replace("@" + fieldName, "'" + value + "'");
                            filter = ActiveFilter.Replace(fieldName, "'" + value + "'");
                            //trường hợp chọn rồi chọn lại , giá mà biết giá trị cũ
                            string OldValue = "";
                            if (RIOldValue[tmp.Name].ToString() != "")
                            {
                                OldValue = RIOldValue[tmp.Name].ToString();
                            }
                            if (OldValue != "")
                            {
                                filter = filter.Replace("'" + OldValue + "'", "'" + value + "'");
                            }
                        }
                        try
                        {
                            Ri.View.ActiveFilterString = filter;
                        }
                        catch { }
                        Ri.ActiveFilter = filter;
                    }
                    Ri.View.ActiveFilterEnabled = true;
                    Ri.View.OptionsFilter.EndUpdate();
                    Ri.View.RefreshEditor(true);
                   // Ri.View.BestFitColumns();
                }
            }
            if (tmp.Name != null)
            {
                RIOldValue[tmp.Name] = value;//Giá trị chọn lần này
            }

        }
        public BaseEdit GenCBSControl(DataRow dr)
        {
            BaseEdit tmp;
            string dataMember = dr["FieldName"].ToString();
            int pType = Int32.Parse(dr["Type"].ToString());
            //0: text(pk); 1: text(fk); 2: text; 3: int(pk); 4: int(fk); 5: int; 6: unique identifier; 
            //7: unique identifier(fk); 8: decimal; 9: date; 10: boolean; 11: time; 12: image; 13: ntext

            switch (pType)
            {
                case 0:
                    tmp = new VTextEdit();
                    (tmp as VTextEdit).Properties.MaxLength = 32;
                    (tmp as VTextEdit).Properties.CharacterCasing = CharacterCasing.Upper;
                    break;
                case 2:
                    tmp = new VTextEdit();                    
                    (tmp as VTextEdit).Properties.MaxLength = 255;
                    break;
                case 5:
                    tmp = new VSpinEdit();
                    break;
                case 8:
                    tmp = new VCalcEdit();
                    (tmp as VCalcEdit).Spin += new DevExpress.XtraEditors.Controls.SpinEventHandler(FormDesigner_Spin1);
                    (tmp as VCalcEdit).KeyUp += new KeyEventHandler(VCalEdit_KeyUp);
                    if (dr["EditMask"].ToString() != string.Empty)
                    {
                        (tmp as VCalcEdit).Properties.EditMask = dr["EditMask"].ToString();
                        (tmp as VCalcEdit).Properties.Mask.UseMaskAsDisplayFormat = true;
                    }
                    break;
                case 9:
                    tmp = new VDateEdit();
                    break;
                case 14:
                    tmp = new VDateEdit();
                    (tmp as VDateEdit).Properties.EditMask = "dd/MM/yyyy HH:mm:ss";

                    (tmp as VDateEdit).Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    (tmp as VDateEdit).Properties.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm:ss";   
                    break;
                case 10:
                    tmp = new VCheckEdit();
                    break;
                case 1:
                case 4:
                case 7:
                    tmp = GenGridLookupEdit(dr, true);
                    CDTGridLookUpEdit tmpGrd = tmp as CDTGridLookUpEdit;
                    tmpGrd.Properties.CloseUpKey = new DevExpress.Utils.KeyShortcut(Keys.Control | Keys.Down);
                    tmpGrd.Properties.View.OptionsView.ShowAutoFilterRow = true;
                    tmpGrd.Properties.View.OptionsView.ColumnAutoWidth = false;
                    break;
                case 11:
                    tmp = new TimeEdit();
                    break;
                case 12:
                    tmp = new PictureEdit();
                    if (Config.GetValue("Language").ToString() == "0")
                        tmp.Properties.NullText = "Click phải chuột chọn nạp hình";
                    tmp.DataBindings.Add("EditValue", _bindingSource, dataMember, true, DataSourceUpdateMode.OnValidation);
                    break;
                case 13:
                    tmp = new MemoEdit();
                    break;
                default:
                    tmp = null;
                    break;
            }
            if (tmp != null)
            {
                tmp.Name = dr["FieldName"].ToString();
                tmp.ToolTip = dr["Tip"].ToString();
                if (pType != 12)
                    tmp.DataBindings.Add("EditValue", _bindingSource, dataMember, false, DataSourceUpdateMode.OnValidation);
                tmp.TabIndex = Int32.Parse(dr["TabIndex"].ToString());
                bool admin = Boolean.Parse(Config.GetValue("Admin").ToString());
                if (_formAction == FormAction.Edit && !admin)
                {
                    string canEdit = dr["Editable1"].ToString();
                    tmp.Properties.ReadOnly = canEdit != string.Empty && !Boolean.Parse(canEdit);
                }
                //if (_formAction == FormAction.Edit && !tmp.Properties.ReadOnly)
                //    tmp.Properties.ReadOnly = !Boolean.Parse(dr["Editable"].ToString());
            }
            return tmp;
        }

    }
}
