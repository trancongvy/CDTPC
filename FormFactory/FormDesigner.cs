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
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using CBSControls;
using DataFactory;
using CDTLib;
using CDTControl;
using DevControls;
using DevExpress.XtraTab;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Utils;
using DevExpress.XtraLayout.Utils;
using ErrorManager;
using System.Threading;
using publicCDT;
using DevExpress.XtraGrid.Views.BandedGrid;
namespace FormFactory
{

   public class FormDesigner
{
    // Fields
    public List<BaseEdit> _BaseList;
    private BindingSource _bindingSource;
    private CDTData _data;
    private BaseEdit _firstControl;
    private FormAction _formAction;
    public List<GridControl> _gcDetail;
    private GridControl _gcMain;
    public List<CDTGridLookUpEdit> _glist;
    private List<LookUp_CDTData> _Glist;
    public List<LayoutControlItem> _LayoutList;
    public List<CDTData> _lstData;
    private List<CDTRepGridLookup> _lstRep;
    private List<CDTRepGridLookup> _rlist;
    private List<RLookUp_CDTData> _Rlist;
    public bool InsertedToDetail;
    private Hashtable RIOldValue;
    private Hashtable GOldValue;
    public XtraTabControl TabDetail;
    public List<fileContener> _lFileContener = new List<fileContener>();
    private System.Windows.Forms.ImageList imageList1;
       private LayoutControl lcMain;
    // Methods
    public FormDesigner(CDTData data)
    {
        this._lstData = new List<CDTData>();
        this.RIOldValue = new Hashtable();
        this.GOldValue = new Hashtable();
        this._lstRep = new List<CDTRepGridLookup>();
        this._Glist = new List<LookUp_CDTData>();
        this._glist = new List<CDTGridLookUpEdit>();
        this._Rlist = new List<RLookUp_CDTData>();
        this._rlist = new List<CDTRepGridLookup>();
        this._BaseList = new List<BaseEdit>();
        this._LayoutList = new List<LayoutControlItem>();
        this.TabDetail = new XtraTabControl();
        this.InsertedToDetail = true;
        this._data = data;
        if (this._data.dataType != DataType.Report)
        {
            this._lstData.Add(this._data);
        }
    }

    public FormDesigner(CDTData data, BindingSource bindingSource)
    {
        this._lstData = new List<CDTData>();
        this.RIOldValue = new Hashtable();
        this.GOldValue = new Hashtable();
        this._lstRep = new List<CDTRepGridLookup>();
        this._Glist = new List<LookUp_CDTData>();
        this._glist = new List<CDTGridLookUpEdit>();
        this._Rlist = new List<RLookUp_CDTData>();
        this._rlist = new List<CDTRepGridLookup>();
        this._BaseList = new List<BaseEdit>();
        this._LayoutList = new List<LayoutControlItem>();
        this.TabDetail = new XtraTabControl();
        this.InsertedToDetail = true;
        this._data = data;
        this._bindingSource = bindingSource;
        if (this._data.dataType != DataType.Report)
        {
            this._lstData.Add(this._data);
        }
    }

    private void FormDesigner_Spin1(object sender, SpinEventArgs e)
    {
        (sender as CalcEdit).ShowPopup();
    }

    private void gcMain_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F4)
        {
            GridControl gcMain = sender as GridControl;
            (gcMain.MainView as GridView).DeleteSelectedRows();
        }
        if (e.KeyCode == Keys.F5)
        {
            if (this.formAction == FormAction.New || this.formAction == FormAction.Edit )
            {
                GridControl gcMain = sender as GridControl;
                DataRow drCurrentTable ;
                drCurrentTable = _data.DrTable;
                string TableName = gcMain.DataMember;
                if (_data.DsData.Tables[1].TableName == TableName) drCurrentTable = _data.DrTable;
                else
                    foreach (DataRow dr in _data._drTableDt)
                    {
                        if (dr["TableName"].ToString() == TableName)
                        {
                            drCurrentTable = dr;
                            break;
                        }
                    }
                if ((gcMain.MainView as GridView).SelectedRowsCount > 0)
                {
                    int[] i = (gcMain.MainView as GridView).GetSelectedRows();
                    for (int j = 0; j < i.Length; j++)
                    {
                        DataRow dr = (gcMain.MainView as GridView).GetDataRow(i[j]);
                        DataRow drnew = dr.Table.NewRow();
                        drnew.ItemArray = dr.ItemArray;
                        if (dr.Table.Columns[drCurrentTable["Pk"].ToString()].DataType == typeof(Guid) || dr.Table.Columns[drCurrentTable["Pk"].ToString()].DataType == typeof(int))
                        {
                            drnew[drCurrentTable["Pk"].ToString()] = Guid.NewGuid();
                        }
                        try
                        {
                            dr.Table.Rows.Add(drnew);
                        }
                        catch { }
                    }
                }
            }
        }
    }

    public BaseEdit GenCBSControl(DataRow dr)
    {
        BaseEdit tmp;
        string dataMember = dr["FieldName"].ToString();
        int pType = int.Parse(dr["Type"].ToString());
        switch (pType)
        {
            case 0:
                tmp = new VTextEdit();
                (tmp as VTextEdit).Properties.MaxLength = 0x20;
                (tmp as VTextEdit).Properties.CharacterCasing = CharacterCasing.Upper;
                break;

            case 1:
            case 4:
            case 7:
            {
                tmp = this.GenGridLookupEdit(dr, true);
                CDTGridLookUpEdit tmpGrd = tmp as CDTGridLookUpEdit;
                tmpGrd.Properties.CloseUpKey = new KeyShortcut(Keys.Control | Keys.Down);
                tmpGrd.Properties.View.OptionsView.ShowAutoFilterRow = true;
                tmpGrd.Properties.View.OptionsView.ColumnAutoWidth = false;
                        tmpGrd.Properties.View.OptionsView.ShowGroupedColumns = false;
                break;
            }
            case 2:
                tmp = new VTextEdit();
                (tmp as VTextEdit).Properties.MaxLength = 0xff;
                if (dataMember.ToLower() == "soseri")
                {
                    (tmp as VTextEdit).Properties.CharacterCasing = CharacterCasing.Upper;
                }
                break;

            case 5:
                tmp = new VSpinEdit();
                break;

            case 8:
                tmp = new VCalcEdit();
                tmp.Tag = dr;
                (tmp as VCalcEdit).Spin += new SpinEventHandler(this.FormDesigner_Spin1);
                (tmp as VCalcEdit).KeyUp += new KeyEventHandler(this.VCalEdit_KeyUp);
                if (dr["EditMask"].ToString() != string.Empty)
                {

                        (tmp as VCalcEdit).Properties.EditMask = dr["EditMask"].ToString();
                        (tmp as VCalcEdit).Properties.Mask.UseMaskAsDisplayFormat = true;
  
                }
                tmp.EditValueChanged += new EventHandler(tmp_EditValueChanged);
                break;

            case 9:
                tmp = new VDateEdit();
                break;

            case 10:
                tmp = new VCheckEdit();

                tmp.Text = dr["Tip"].ToString();
                break;

            case 11:
                tmp = new TimeEdit();
                break;

            case 12:
                tmp = new PictureEdit();
                if (Config.GetValue("Language").ToString() == "0")
                {
                    tmp.Properties.NullText = "Click phải chuột chọn nạp h\x00ecnh";
                }
                tmp.DataBindings.Add("EditValue", this._bindingSource, dataMember, true, DataSourceUpdateMode.OnValidation);
                break;

            case 13:
                tmp = new MemoEdit();
                tmp.Height = 200;
                break;

            case 14:
                tmp = new VDateEdit();(tmp as VDateEdit).Properties.DisplayFormat.FormatType = FormatType.DateTime;
                if (dr["EditMask"].ToString() == string.Empty)
                {
                    (tmp as VDateEdit).Properties.EditMask = "dd/MM/yyyy HH:mm:ss tt";
                    (tmp as VDateEdit).Properties.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm:ss tt";
                }
                else
                {
                    (tmp as VDateEdit).Properties.EditMask = dr["EditMask"].ToString();
                    (tmp as VDateEdit).Properties.DisplayFormat.FormatString = dr["EditMask"].ToString();
                }
                tmp.EditValue = DateTime.Now;
                tmp.Text = tmp.EditValue.ToString();
                (tmp as VDateEdit).DateTime = DateTime.Now;
                break;
            case 16:

                fileContener fC = new fileContener(dr);
                fC.DataBindings.Add("Text", this._bindingSource, dataMember, false, DataSourceUpdateMode.OnValidation);
                fC.LoadnewData += new EventHandler(fC_LoadnewData);
                _lFileContener.Add(fC);
                tmp = fC as BaseEdit;
                break;
            default:
                tmp = null;
                break;
        }
        if (tmp != null)
        {
            tmp.Name = dr["FieldName"].ToString();
            tmp.ToolTip = dr["Tip"].ToString();
            if (pType != 12 && pType !=16)
            {
                tmp.DataBindings.Add("EditValue", this._bindingSource, dataMember, false, DataSourceUpdateMode.OnValidation);                
            }

            if (int.Parse(dr["TabIndex"].ToString()) == -1)
            { tmp.TabIndex = 100; }
            bool admin = bool.Parse(Config.GetValue("Admin").ToString());
            if (!admin)
            {
               
                string canEdit1 = dr["Editable1"].ToString();
                //string canEdit = dr["Editable"].ToString();
                if (canEdit1 != string.Empty)
                {                   
                    tmp.Properties.ReadOnly = !bool.Parse(canEdit1);
                }
               
            }
            
        }
       
        return tmp;
    }

    void fC_LoadnewData(object sender, EventArgs e)
       {
           fileContener fC=(sender as fileContener);
           for (int i = 0; i < Data._fileData.Count; i++)
           {
               CDTData.FileData fdata = Data._fileData[i];
               if (fdata.drField["sysFieldID"].ToString() == fC.drField["sysFieldID"].ToString())
               {
                   Data._fileData.Remove(fdata);
                   i--;
               }
           }
 
           Data._fileData.Add(new CDTData.FileData(fC.drField, fC.data, true));
       }

    void tmp_EditValueChanged(object sender, EventArgs e)
       {
           if (sender as VCalcEdit == null) return;
           VCalcEdit tmp = sender as VCalcEdit;
           DataRow dr = tmp.Tag as DataRow;
           //if (tmp.EditValue != DBNull.Value) 
           //    setDynFiter(tmp.Name, tmp.EditValue,typeof( decimal));


       }

    public BaseEdit GenControl(DataRow dr)
    {
        BaseEdit tmp;
        string dataMember = dr["FieldName"].ToString();
        int pType = int.Parse(dr["Type"].ToString());
        switch (pType)
        {
            case 0:
                tmp = new VTextEdit();
                (tmp as VTextEdit).EnterMoveNextControl = true;
                (tmp as VTextEdit).Properties.AllowNullInput = DefaultBoolean.True;
                (tmp as VTextEdit).Properties.MaxLength = 0x20;
                (tmp as VTextEdit).Properties.CharacterCasing = CharacterCasing.Upper;
                break;

            case 1:
            case 4:
            case 7:
                    {
                        tmp = this.GenGridLookupEdit(dr, false);
                        CDTGridLookUpEdit tmpGrd = tmp as CDTGridLookUpEdit;
                        tmpGrd.Properties.CloseUpKey = KeyShortcut.Empty;
                        tmpGrd.EnterMoveNextControl = true;
                        tmpGrd.Properties.NullText = string.Empty;
                        tmpGrd.Properties.ImmediatePopup = true;
                        tmpGrd.Properties.AllowNullInput = DefaultBoolean.True;
                        tmpGrd.Properties.View.OptionsView.ShowAutoFilterRow = true;
                        tmpGrd.Properties.View.OptionsView.ColumnAutoWidth = false;
                        tmpGrd.Properties.View.OptionsView.ShowGroupedColumns = false;
                        if ((tmpGrd.DymicCondition != "") && (this.formAction != FormAction.Filter))
                        {
                            this._lstRep.Add((CDTRepGridLookup)tmpGrd.Properties);
                        }
                        break;
                    }
            case 2:
                tmp = new VTextEdit();
                (tmp as VTextEdit).EnterMoveNextControl = true;
                (tmp as VTextEdit).Properties.MaxLength = 0xff;
                (tmp as VTextEdit).Properties.AllowNullInput = DefaultBoolean.True;
                break;

            case 5:
                tmp = new VSpinEdit();
                (tmp as VSpinEdit).EnterMoveNextControl = true;
                (tmp as VSpinEdit).Properties.AllowNullInput = DefaultBoolean.True;
                break;

            case 8:
                tmp = new VCalcEdit();
                (tmp as VCalcEdit).EnterMoveNextControl = true;
                (tmp as VCalcEdit).Properties.AllowNullInput = DefaultBoolean.True;
                (tmp as VCalcEdit).Spin += new SpinEventHandler(this.FormDesigner_Spin1);
                (tmp as VCalcEdit).KeyUp += new KeyEventHandler(this.VCalEdit_KeyUp);
                if (dr["EditMask"].ToString() != string.Empty)
                {
                    (tmp as VCalcEdit).Properties.EditMask = dr["EditMask"].ToString();
                    (tmp as VCalcEdit).Properties.Mask.UseMaskAsDisplayFormat = true;
                }
                break;

            case 9:
                tmp = new VDateEdit();
                break;

            case 10:
                tmp = new VCheckEdit();
                break;

            case 11:
                tmp = new TimeEdit();
                (tmp as TimeEdit).EnterMoveNextControl = true;
                (tmp as TimeEdit).Properties.AllowNullInput = DefaultBoolean.True;
                break;

            case 12:
                tmp = new PictureEdit();
                if (Config.GetValue("Language").ToString() == "0")
                {
                    tmp.Properties.NullText = "Click phải chuột chọn nạp h\x00ecnh";
                }
                tmp.DataBindings.Add("EditValue", this._bindingSource, dataMember, true, DataSourceUpdateMode.OnValidation);
                break;

            case 13:
                tmp = new MemoEdit();
                break;

            case 14:
                tmp = new VDateEdit();
                (tmp as VDateEdit).Properties.EditMask = "dd/MM/yyyy HH:mm:ss";
                break;
            case 15:
                tmp = new VTextEdit();
                (tmp as VTextEdit).EnterMoveNextControl = true;
                (tmp as VTextEdit).Properties.MaxLength = 0xff;
                (tmp as VTextEdit).Properties.AllowNullInput = DefaultBoolean.True;
                break;
            case 16:

                fileContener fC = new fileContener(dr);
                fC.DataBindings.Add("Text", this._bindingSource, dataMember, false, DataSourceUpdateMode.OnValidation);
                fC.LoadnewData += new EventHandler(fC_LoadnewData);
                _lFileContener.Add(fC);
                tmp = fC as BaseEdit;
                break;
            default:
                tmp = null;
                break;
        }
        if (tmp != null)
        {
            tmp.Name = dr["FieldName"].ToString();
            tmp.ToolTip = dr["Tip"].ToString();
            if ((pType != 12 && pType != 16) && !((this._formAction == FormAction.Filter) && bool.Parse(dr["IsBetween"].ToString())))
            {
                tmp.DataBindings.Add("EditValue", this._bindingSource, dataMember, false, DataSourceUpdateMode.OnValidation);
            }
            if(int.Parse(dr["TabIndex"].ToString())>=0)
                tmp.TabIndex = int.Parse(dr["TabIndex"].ToString());
            bool admin = bool.Parse(Config.GetValue("Admin").ToString());

            if (!admin)
            {
                string canEdit1 = dr["Editable1"].ToString();
               // string canEdit = dr["Editable"].ToString();
                if (canEdit1 != string.Empty)
                {
                    tmp.Properties.ReadOnly = !bool.Parse(canEdit1);
                }
            }
        }
        return tmp;
    }

        private CDTGridColumn GenGridColumn(DataRow dr, int exColNum, bool checkData)
        {
            CDTGridColumn gcl = new CDTGridColumn();
            if (dr["FieldName"].ToString().ToLower() == "makhthue")
            {
            }
            gcl.Name = "cl" + dr["FieldName"].ToString();
            gcl.FieldName = dr["FieldName"].ToString();
            if (dr["fieldName"].ToString().ToUpper() == "MAXVALUE")
            {
            }
            string caption = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
            int formType = int.Parse(this._data.DrTable["Type"].ToString());
            if (!(((formType != 1) && (formType != 4)) || dr["AllowNull"].ToString() == "0"))
            {
                caption = "*" + caption;
            }
            gcl.Caption = caption;
            gcl.ToolTip = dr["Tip"].ToString();

            if (int.Parse(dr["TabIndex"].ToString()) != -1) gcl.VisibleIndex = int.Parse(dr["TabIndex"].ToString()) + exColNum;
            else gcl.VisibleIndex = -1;
            gcl.MasterRow = dr;
            gcl.refFilter = dr["DynCriteria"].ToString();
            if (!checkData)
            {
                // gcl.Visible = dr["Visible"].ToString()=="1";
            }
            if (bool.Parse(dr["IsFixCol"].ToString()))
            {
                gcl.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            }
            if (bool.Parse(dr["IsGroupCol"].ToString()))
            {
                gcl.GroupIndex = 0;
            }
            int pType = int.Parse(dr["Type"].ToString());
            if (!(checkData || (pType != 3)))
            {
                gcl.Visible = false;
            }
            else gcl.Visible = true;
            if (pType == 1)
            {
                gcl.Width += 20;
            }
            if (pType == 2)
            {
                gcl.Width += 150;
            }
            if (pType == 3)
            {
                gcl.Width -= 40;
            }
            if (pType == 15)
            {
                gcl.Width += 150;
            }
            if (pType == 9)
            {
                gcl.DisplayFormat.FormatType = FormatType.DateTime;
                gcl.DisplayFormat.FormatString = "dd/MM/yyyy";
            }
            if (pType == 14)
            {
                gcl.DisplayFormat.FormatType = FormatType.DateTime;

                RepositoryItemDateEdit dEdit = new RepositoryItemDateEdit();
                gcl.ColumnEdit = dEdit;

                dEdit.EditFormat.FormatType = FormatType.DateTime;
                if (dr["EditMask"].ToString() == string.Empty)
                {
                    gcl.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm:ss";
                    dEdit.EditMask = "dd/MM/yyyy HH:mm:ss";
                    dEdit.EditFormat.FormatString = "dd/MM/yyyy HH:mm:ss";
                }
                else
                {
                    gcl.DisplayFormat.FormatString = dr["EditMask"].ToString();
                    dEdit.EditMask = dr["EditMask"].ToString();
                    dEdit.EditFormat.FormatString = dr["EditMask"].ToString();
                }
                gcl.Width += 50;
            }
            if (pType == 8)
            {
                string f;

                if (dr["EditMask"].ToString() == string.Empty)
                    f = "";
                else
                    f = ":" + dr["EditMask"].ToString();
                gcl.DisplayFormat.FormatType = FormatType.Numeric;
                gcl.DisplayFormat.FormatString = dr["EditMask"].ToString();
                gcl.SummaryItem.Assign(new GridSummaryItem(DevExpress.Data.SummaryItemType.Sum, dr["FieldName"].ToString(), "{0" + f + "}"));
                //gcl.Width += 30;
            }
            bool admin = bool.Parse(Config.GetValue("Admin").ToString());
            if (dr.Table.Columns.Contains("ColWidth") && dr["ColWidth"] != DBNull.Value)
            {
                gcl.Width = int.Parse(dr["ColWidth"].ToString());
            }
            if (dr["Visible"].ToString() == "0")
            {
                gcl.Visible = false;
            }
           
            if (!admin)
            {
                string canEdit1 = dr["Editable1"].ToString();
                //string canEdit = dr["Editable"].ToString();
                if (canEdit1 != string.Empty)
                {
                    gcl.OptionsColumn.AllowEdit = bool.Parse(canEdit1);
                }
            }
            return gcl;
        }
        private CDTBandGridColumn GenBandGridColumn(DataRow dr, int exColNum, bool checkData)
        {
            CDTBandGridColumn gcl = new CDTBandGridColumn();
            if (dr["FieldName"].ToString().ToLower() == "makhthue")
            {
            }
            gcl.Name = "cl" + dr["FieldName"].ToString();
            gcl.FieldName = dr["FieldName"].ToString();
            if (dr["fieldName"].ToString().ToUpper() == "MAXVALUE")
            {
            }
            string caption = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
            int formType = int.Parse(this._data.DrTable["Type"].ToString());
            if (!(((formType != 1) && (formType != 4)) || dr["AllowNull"].ToString() == "0"))
            {
                caption = "*" + caption;
            }
            gcl.Caption = caption;
            gcl.ToolTip = dr["Tip"].ToString();

            if (int.Parse(dr["TabIndex"].ToString()) != -1) gcl.VisibleIndex = int.Parse(dr["TabIndex"].ToString()) + exColNum;
            else gcl.VisibleIndex = -1;
            gcl.MasterRow = dr;
            gcl.refFilter = dr["DynCriteria"].ToString();
            if (!checkData)
            {
                // gcl.Visible = dr["Visible"].ToString()=="1";
            }
            if (bool.Parse(dr["IsFixCol"].ToString()))
            {
                gcl.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            }
            if (bool.Parse(dr["IsGroupCol"].ToString()))
            {
                gcl.GroupIndex = 0;
            }
            int pType = int.Parse(dr["Type"].ToString());
            if (!(checkData || (pType != 3)))
            {
                gcl.Visible = false;
            }
            else gcl.Visible = true;
            if (pType == 1)
            {
                gcl.Width += 20;
            }
            if (pType == 2)
            {
                gcl.Width += 150;
            }
            if (pType == 3)
            {
                gcl.Width -= 40;
            }
            if (pType == 15)
            {
                gcl.Width += 150;
            }
            if (pType == 9)
            {
                gcl.DisplayFormat.FormatType = FormatType.DateTime;
                gcl.DisplayFormat.FormatString = "dd/MM/yyyy";
            }
            if (pType == 13||pType==12)
            {
                gcl.RowCount = 3;
            }
                if (pType == 14)
            {
                gcl.DisplayFormat.FormatType = FormatType.DateTime;

                RepositoryItemDateEdit dEdit = new RepositoryItemDateEdit();
                gcl.ColumnEdit = dEdit;

                dEdit.EditFormat.FormatType = FormatType.DateTime;
                if (dr["EditMask"].ToString() == string.Empty)
                {
                    gcl.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm:ss";
                    dEdit.EditMask = "dd/MM/yyyy HH:mm:ss";
                    dEdit.EditFormat.FormatString = "dd/MM/yyyy HH:mm:ss";
                }
                else
                {
                    gcl.DisplayFormat.FormatString = dr["EditMask"].ToString();
                    dEdit.EditMask = dr["EditMask"].ToString();
                    dEdit.EditFormat.FormatString = dr["EditMask"].ToString();
                }
                gcl.Width += 50;
            }
            if (pType == 8)
            {
                string f;

                if (dr["EditMask"].ToString() == string.Empty)
                    f = "";
                else
                    f = ":" + dr["EditMask"].ToString();
                gcl.DisplayFormat.FormatType = FormatType.Numeric;
                gcl.DisplayFormat.FormatString = dr["EditMask"].ToString();
                gcl.SummaryItem.Assign(new GridSummaryItem(DevExpress.Data.SummaryItemType.Sum, dr["FieldName"].ToString(), "{0" + f + "}"));
                //gcl.Width += 30;
            }
            bool admin = bool.Parse(Config.GetValue("Admin").ToString());
            if (dr.Table.Columns.Contains("ColWidth") && dr["ColWidth"] != DBNull.Value)
            {
                gcl.Width = int.Parse(dr["ColWidth"].ToString());
            }
            if (dr["Visible"].ToString() == "0")
            {
                gcl.Visible = false;
            }

            if (!admin)
            {
                string canEdit1 = dr["Editable1"].ToString();
                //string canEdit = dr["Editable"].ToString();
                if (canEdit1 != string.Empty)
                {
                    gcl.OptionsColumn.AllowEdit = bool.Parse(canEdit1);
                }
            }
            return gcl;
        }
        public GridControl GenGridControl(DataTable dt, bool isEdit, DockStyle ds)
    {
        GridControl gcMain = new GridControl();
        GridView gvMain = new GridView();
        gcMain.BeginInit();
        gvMain.BeginInit();
        gcMain.Dock = ds;
        gcMain.SendToBack();
        gcMain.MainView = gvMain;
        gcMain.ViewCollection.AddRange(new BaseView[] { gvMain });
        gvMain.OptionsView.ShowFooter = true;
        gvMain.GridControl = gcMain;
            gvMain.Appearance.HideSelectionRow.ForeColor = Color.Blue;
            gvMain.Appearance.HideSelectionRow.Options.UseForeColor = true;
            gvMain.Appearance.FocusedRow.ForeColor = Color.Blue;
            gvMain.Appearance.FocusedRow.Options.UseForeColor = true;
            gvMain.Appearance.FocusedRow.BackColor = Color.Azure;
            gvMain.Appearance.FocusedRow.Options.UseBackColor = true;
            gvMain.Appearance.SelectedRow.Options.UseForeColor=true;
            gvMain.Appearance.SelectedRow.ForeColor=Color.Blue;
            gvMain.Appearance.SelectedRow.Options.UseBackColor = true;
            gvMain.Appearance.SelectedRow.BackColor = Color.AliceBlue;
            gvMain.OptionsDetail.AllowExpandEmptyDetails = true;
            if (Config.GetValue("Language").ToString() == "0")
        {
            gvMain.GroupPanelText = "Bảng nhóm: kéo thả một cột vào đây để nhóm số liệu";
        }
        gvMain.OptionsView.ColumnAutoWidth = false;
        gvMain.OptionsView.EnableAppearanceEvenRow = true;
        gvMain.OptionsSelection.MultiSelect = true;
        gvMain.OptionsBehavior.Editable = false;
        gvMain.OptionsView.ShowAutoFilterRow = true;
        gvMain.OptionsNavigation.EnterMoveNextColumn = true;
        gvMain.IndicatorWidth = 40;
        //gvMain.OptionsView.ShowDetailButtons = false;
        gvMain.OptionsBehavior.AutoExpandAllGroups = true;
        gvMain.OptionsNavigation.AutoFocusNewRow = true;
       // gvMain.BestFitColumns();

        gvMain.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(this.View_CustomDrawRowIndicator);
        gcMain.KeyUp += new KeyEventHandler(gcMain_KeyUp);
        gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gvMain_CellValueChanged);
        if (isEdit)
        {
            gvMain.FocusedRowChanged += new FocusedRowChangedEventHandler(this.gvMain_FocusedRowChanged);
            gcMain.KeyDown += new KeyEventHandler(this.gcMain_KeyDown);
            gvMain.OptionsBehavior.Editable = true;
            gvMain.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gvMain.OptionsView.ShowFooter = true;
        }

        gvMain.RowCountChanged += new EventHandler(gvMain_RowCountChanged);
        int exColNum = 0;
        bool admin = bool.Parse(Config.GetValue("Admin").ToString());
        DateTime t1 = DateTime.Now;
        DateTime tx = DateTime.Now;
        string s = "";
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            
            DataRow dr = dt.Rows[i];

            

            string viewable = dr["Viewable"].ToString();

            if ((admin || !(viewable != string.Empty)) || bool.Parse(viewable))
            {
                CDTGridColumn gcl = this.GenGridColumn(dr, exColNum, false);
                RepositoryItem ri = this.GenRepository(dr);
                    
                    if (ri != null)
                {
                    gcMain.RepositoryItems.Add(ri);
                    gcl.ColumnEdit = ri;
                    CDTRepGridLookup CDTRi = ri as CDTRepGridLookup;
                    if (CDTRi != null)
                    {
                        CDTRi.MainView = gvMain;
                        CDTRi.MainStruct = dt;
                    }
                }
                gvMain.Columns.Add(gcl);
                if (dr["Visible"].ToString() != "0")
                    gcl.VisibleIndex = int.Parse(dr["TabIndex"].ToString()) + exColNum;
                else
                    gcl.VisibleIndex = -1;
                int pType = int.Parse(dr["Type"].ToString());
                if (pType == 12)
                {
                    gvMain.OptionsView.RowAutoHeight = true;
                }
                if ((pType == 1) && (dr["DisplayMember"].ToString() != string.Empty))
                {
                    CDTGridColumn gcl1 = this.GenGridColumn(dr, exColNum, false);
                    gcl1.GroupIndex = -1;
                    gcl1.isExCol = true;
                    RepositoryItem ri1 = this.GenRepository(dr);
                    CDTRepGridLookup CDTRi1 = ri1 as CDTRepGridLookup;
                    if (CDTRi1 != null)
                    {
                        CDTRi1.MainView = gvMain;
                        CDTRi1.MainStruct = dt;
                    }
                    if (ri1 != null)
                    {
                        string caption;
                        string displayMember = dr["DisplayMember"].ToString();
                        ((CDTRepGridLookup) ri1).DisplayMember = displayMember;
                        gcMain.RepositoryItems.Add(ri1);
                        if (Config.GetValue("Language").ToString() == "0")
                        {
                            caption = "Tên " + dr["LabelName"].ToString().ToLower();
                        }
                        else
                        {
                            caption = dr["LabelName2"].ToString() + " name";
                        }
                        int formType = int.Parse(this._data.DrTable["Type"].ToString());
                        if (!(((formType != 1) && (formType != 4)) || (dr["AllowNull"].ToString()=="0")))
                        {
                            caption = "*" + caption;
                        }
                        gcl1.Caption = caption;
                        gcl1.VisibleIndex++;
                        gcl1.Width += 150;
                        gcl1.ColumnEdit = ri1;
                        gcl1.Visible = gcl.Visible;
                    }
                    gvMain.Columns.Add(gcl1);
                    exColNum++;
                }
                if (gcl.GroupIndex >= 0)
                {
                    gvMain.GroupSummary.Add(new GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Count, gcl.FieldName, null, "({0} mục)"));
                }
            }
            DateTime t2 = DateTime.Now;
            TimeSpan t = t2 - t1;
            s += "\n" + dr["fieldName"].ToString() + " " + t.TotalMilliseconds.ToString();
            t1 = t2;
        }
        LogFile.AppendToFile("log.txt",s);
        TimeSpan tt = DateTime.Now -tx;
        s = "\n" +  " Tong cong " + tt.TotalMilliseconds.ToString();
        LogFile.AppendToFile("log.txt", s);
        gcMain.EndInit();
        gvMain.EndInit();
        this._gcMain = gcMain;
        return gcMain;
    }
        public GridControl GenBandGridControl(DataTable dt,DataTable dtband, bool isEdit, DockStyle ds)
        {
            GridControl gcMain = new GridControl();
            AdvBandedGridView gvMain = new AdvBandedGridView();
            gcMain.BeginInit();
            gvMain.BeginInit();
            gcMain.Dock = ds;
            gcMain.SendToBack();
            gcMain.MainView = gvMain;
            gcMain.ViewCollection.AddRange(new BaseView[] { gvMain });
            gvMain.OptionsView.ShowFooter = true;
            gvMain.GridControl = gcMain;
            gvMain.Appearance.HideSelectionRow.ForeColor = Color.Blue;
            gvMain.Appearance.HideSelectionRow.Options.UseForeColor = true;
            gvMain.Appearance.FocusedRow.ForeColor = Color.Blue;
            gvMain.Appearance.FocusedRow.Options.UseForeColor = true;
            gvMain.Appearance.FocusedRow.BackColor = Color.Azure;
            gvMain.Appearance.FocusedRow.Options.UseBackColor = true;
            gvMain.Appearance.SelectedRow.Options.UseForeColor = true;
            gvMain.Appearance.SelectedRow.ForeColor = Color.Blue;
            gvMain.Appearance.SelectedRow.Options.UseBackColor = true;
            gvMain.Appearance.SelectedRow.BackColor = Color.AliceBlue;
            if (Config.GetValue("Language").ToString() == "0")
            {
                gvMain.GroupPanelText = "Bảng nhóm: kéo thả một cột vào đây để nhóm số liệu";
            }
            gvMain.OptionsView.ColumnAutoWidth = false;
            gvMain.OptionsView.EnableAppearanceEvenRow = true;
            gvMain.OptionsSelection.MultiSelect = true;
            gvMain.OptionsBehavior.Editable = false;
            gvMain.OptionsView.ShowAutoFilterRow = true;
            gvMain.OptionsNavigation.EnterMoveNextColumn = true;
            gvMain.IndicatorWidth = 40;
            //gvMain.OptionsView.ShowDetailButtons = false;
            gvMain.OptionsBehavior.AutoExpandAllGroups = true;
            gvMain.OptionsNavigation.AutoFocusNewRow = true;
            // gvMain.BestFitColumns();

            gvMain.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(this.View_CustomDrawRowIndicator);
            gcMain.KeyUp += new KeyEventHandler(gcMain_KeyUp);
            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gvMain_CellValueChanged);
            if (isEdit)
            {
                gvMain.FocusedRowChanged += new FocusedRowChangedEventHandler(this.gvMain_FocusedRowChanged);
                gcMain.KeyDown += new KeyEventHandler(this.gcMain_KeyDown);
                gvMain.OptionsBehavior.Editable = true;
                gvMain.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
                gvMain.OptionsView.ShowFooter = true;
            }

            gvMain.RowCountChanged += new EventHandler(gvMain_RowCountChanged);
            int exColNum = 0;
            bool admin = bool.Parse(Config.GetValue("Admin").ToString());
            DateTime t1 = DateTime.Now;
            DateTime tx = DateTime.Now;
            string s = "";
            foreach(DataRow dr in dtband.Rows)
            {
                GridBand gb = new GridBand();
                gb.Name = dr["sysBandID"].ToString();
                gb.Caption = dr["Caption"].ToString();
                gb.Width = int.Parse(dr["Width"].ToString());
                gvMain.Bands.Add(gb);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                DataRow dr = dt.Rows[i];
                string viewable = dr["Viewable"].ToString();

                if ((admin || !(viewable != string.Empty)) || bool.Parse(viewable))
                {
                    CDTBandGridColumn gcl = this.GenBandGridColumn(dr, exColNum, false);
                    if (dr["sysBandID"] != DBNull.Value)
                    {
                        gvMain.Bands[dr["sysBandID"].ToString()].Columns.Add(gcl);
                    }
                    RepositoryItem ri = this.GenRepository(dr);

                    if (ri != null)
                    {
                        gcMain.RepositoryItems.Add(ri);
                        gcl.ColumnEdit = ri;
                        CDTRepGridLookup CDTRi = ri as CDTRepGridLookup;
                        if (CDTRi != null)
                        {
                            CDTRi.MainView = gvMain;
                            CDTRi.MainStruct = dt;
                        }
                    }
                    gvMain.Columns.Add(gcl);
                    if (dr["Visible"].ToString() != "0")
                        gcl.VisibleIndex = int.Parse(dr["TabIndex"].ToString()) + exColNum;
                    else
                        gcl.VisibleIndex = -1;
                    int pType = int.Parse(dr["Type"].ToString());
                    if (pType == 12)
                    {
                        gvMain.OptionsView.RowAutoHeight = true;
                    }
                    if ((pType == 1) && (dr["DisplayMember"].ToString() != string.Empty))
                    {
                        CDTBandGridColumn gcl1 = this.GenBandGridColumn(dr, exColNum, false);
                        gcl1.GroupIndex = -1;
                        gcl1.isExCol = true;
                        RepositoryItem ri1 = this.GenRepository(dr);
                        CDTRepGridLookup CDTRi1 = ri1 as CDTRepGridLookup;
                        if (CDTRi1 != null)
                        {
                            CDTRi1.MainView = gvMain;
                            CDTRi1.MainStruct = dt;
                        }
                        if (ri1 != null)
                        {
                            string caption;
                            string displayMember = dr["DisplayMember"].ToString();
                            ((CDTRepGridLookup)ri1).DisplayMember = displayMember;
                            gcMain.RepositoryItems.Add(ri1);
                            if (Config.GetValue("Language").ToString() == "0")
                            {
                                caption = "Tên " + dr["LabelName"].ToString().ToLower();
                            }
                            else
                            {
                                caption = dr["LabelName2"].ToString() + " name";
                            }
                            int formType = int.Parse(this._data.DrTable["Type"].ToString());
                            if (!(((formType != 1) && (formType != 4)) || (dr["AllowNull"].ToString() == "0")))
                            {
                                caption = "*" + caption;
                            }
                            gcl1.Caption = caption;
                            gcl1.VisibleIndex++;
                            gcl1.Width += 150;
                            gcl1.ColumnEdit = ri1;
                            gcl1.Visible = gcl.Visible;
                        }
                        gvMain.Columns.Add(gcl1);
                        exColNum++;
                    }
                    if (gcl.GroupIndex >= 0)
                    {
                        gvMain.GroupSummary.Add(new GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Count, gcl.FieldName, null, "({0} mục)"));
                    }
                }
                DateTime t2 = DateTime.Now;
                TimeSpan t = t2 - t1;
                s += "\n" + dr["fieldName"].ToString() + " " + t.TotalMilliseconds.ToString();
                t1 = t2;
            }
            LogFile.AppendToFile("log.txt", s);
            TimeSpan tt = DateTime.Now - tx;
            s = "\n" + " Tong cong " + tt.TotalMilliseconds.ToString();
            LogFile.AppendToFile("log.txt", s);
            gcMain.EndInit();
            gvMain.EndInit();
            this._gcMain = gcMain;
            return gcMain;
        }
        void gvMain_RowCountChanged(object sender, EventArgs e)
   {
       
   }

    void gcMain_KeyUp(object sender, KeyEventArgs e)
   {
       
   }

    public GridControl GenGridControlDt(DataTable dt, string lstField, bool isEdit, DockStyle ds)
    {
        GridControl gcMain = new GridControl();
        GridView gvMain = new GridView();
        gcMain.BeginInit();
        gvMain.BeginInit();
        gcMain.Dock = ds;
        gcMain.SendToBack();
        gcMain.MainView = gvMain;
        gcMain.ViewCollection.AddRange(new BaseView[] { gvMain });
        gvMain.OptionsView.ShowFooter = true;
        gvMain.GridControl = gcMain;
        if (Config.GetValue("Language").ToString() == "0")
        {
            gvMain.GroupPanelText = "Bảng nhóm: kéo thả một cột vào đây để nhóm số liệu";
        }
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
        gcMain.KeyUp+=new KeyEventHandler(gcMain_KeyUp);

        gvMain.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(this.View_CustomDrawRowIndicator);
           
        gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gvMain_CellValueChanged);
        if (isEdit)
        {
            gvMain.FocusedRowChanged += new FocusedRowChangedEventHandler(this.gvMain_FocusedRowChanged);
            gcMain.KeyDown += new KeyEventHandler(this.gcMain_KeyDown);
            gvMain.OptionsBehavior.Editable = true;
            gvMain.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
        }
        int exColNum = 0;
        bool admin = bool.Parse(Config.GetValue("Admin").ToString());
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow dr = dt.Rows[i];
            string viewable = dr["Viewable"].ToString();
            if ((admin || !(viewable != string.Empty)) || bool.Parse(viewable))
            {
                CDTGridColumn gcl = this.GenGridColumn(dr, exColNum, false);

                RepositoryItem ri = this.GenRepository(dr);
               
                if (ri != null)
                {
                    CDTRepGridLookup CDTRi = ri as CDTRepGridLookup;
                    if (CDTRi != null)
                    {
                        CDTRi.MainView = gvMain;
                        CDTRi.MainStruct = dt;
                    }
                    gcMain.RepositoryItems.Add(ri);
                    gcl.ColumnEdit = ri;
                }
                gcl.Visible = gcl.Visible && lstField.ToLower().Contains(gcl.FieldName.ToLower() + ",");
                gvMain.Columns.Add(gcl);
                int pType = int.Parse(dr["Type"].ToString());
                if (pType == 12)
                {
                    gvMain.OptionsView.RowAutoHeight = true;
                }
                if ((pType == 1) && (dr["DisplayMember"].ToString() != string.Empty))
                {
                    CDTGridColumn gcl1 = this.GenGridColumn(dr, exColNum, false);
                    gcl1.GroupIndex = -1;
                    gcl1.Visible = gcl1.Visible && lstField.ToLower().Contains(gcl.FieldName.ToLower() + ",");
                    gcl1.isExCol = true;
                    RepositoryItem ri1 = this.GenRepository(dr);
                    CDTRepGridLookup CDTRi1 = ri1 as CDTRepGridLookup;
                    if (CDTRi1 != null)
                    {
                        CDTRi1.MainView = gvMain;
                        CDTRi1.MainStruct = dt;
                    }
                    if (ri1 != null)
                    {
                        string caption;
                        string displayMember = dr["DisplayMember"].ToString();
                        ((CDTRepGridLookup) ri1).DisplayMember = displayMember;
                        gcMain.RepositoryItems.Add(ri1);
                        if (Config.GetValue("Language").ToString() == "0")
                        {
                            caption = "Tên " + dr["LabelName"].ToString().ToLower();
                        }
                        else
                        {
                            caption = dr["LabelName2"].ToString() + " name";
                        }
                        int formType = int.Parse(this._data.DrTable["Type"].ToString());
                        if (!(((formType != 1) && (formType != 4)) || bool.Parse(dr["AllowNull"].ToString())))
                        {
                            caption = "*" + caption;
                        }
                        gcl1.Caption = caption;
                        gcl1.VisibleIndex++;
                        gcl1.Width += 200;
                        gcl1.ColumnEdit = ri1;
                        gcl1.Visible = gcl.Visible;
                    }
                    gvMain.Columns.Add(gcl1);
                    exColNum++;
                }
                if (gcl.GroupIndex >= 0)
                {
                    gvMain.GroupSummary.Add(new GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Count, gcl.FieldName, null, "({0} mục)"));
                }
            }
        }
        gcMain.EndInit();
        gvMain.EndInit();
        return gcMain;
    }
        public GridControl GenBandGridControlDt(DataTable dt, DataTable dtband, string lstField, bool isEdit, DockStyle ds)
        {
            GridControl gcMain = new GridControl();
            AdvBandedGridView gvMain = new AdvBandedGridView();
            gcMain.BeginInit();
            gvMain.BeginInit();
            gcMain.Dock = ds;
            gcMain.SendToBack();
            gcMain.MainView = gvMain;
            gcMain.ViewCollection.AddRange(new BaseView[] { gvMain });
            gvMain.OptionsView.ShowFooter = true;
            gvMain.GridControl = gcMain;
            if (Config.GetValue("Language").ToString() == "0")
            {
                gvMain.GroupPanelText = "Bảng nhóm: kéo thả một cột vào đây để nhóm số liệu";
            }
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
            gcMain.KeyUp += new KeyEventHandler(gcMain_KeyUp);
            gvMain.OptionsDetail.AllowExpandEmptyDetails = true;
            gvMain.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(this.View_CustomDrawRowIndicator);

            gvMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gvMain_CellValueChanged);
            if (isEdit)
            {
                gvMain.FocusedRowChanged += new FocusedRowChangedEventHandler(this.gvMain_FocusedRowChanged);
                gcMain.KeyDown += new KeyEventHandler(this.gcMain_KeyDown);
                gvMain.OptionsBehavior.Editable = true;
                gvMain.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            }
            foreach (DataRow dr in dtband.Rows)
            {
                GridBand gb = new GridBand();
                gb.Name = dr["sysBandID"].ToString();
                gb.Caption = dr["Caption"].ToString();
                gb.Width = int.Parse(dr["Width"].ToString());
                gvMain.Bands.Add(gb);
            }
            int exColNum = 0;
            bool admin = bool.Parse(Config.GetValue("Admin").ToString());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string viewable = dr["Viewable"].ToString();
                if ((admin || !(viewable != string.Empty)) || bool.Parse(viewable))
                {
                    CDTBandGridColumn gcl = this.GenBandGridColumn(dr, exColNum, false);
                    if (dr["sysBandID"] != DBNull.Value)
                    {
                        gvMain.Bands[dr["sysBandID"].ToString()].Columns.Add(gcl);
                    }
                    RepositoryItem ri = this.GenRepository(dr);

                    if (ri != null)
                    {
                        CDTRepGridLookup CDTRi = ri as CDTRepGridLookup;
                        if (CDTRi != null)
                        {
                            CDTRi.MainView = gvMain;
                            CDTRi.MainStruct = dt;
                        }
                        gcMain.RepositoryItems.Add(ri);
                        gcl.ColumnEdit = ri;
                    }
                    gcl.Visible = gcl.Visible && lstField.ToLower().Contains(gcl.FieldName.ToLower() + ",");
                    gvMain.Columns.Add(gcl);
                    int pType = int.Parse(dr["Type"].ToString());
                    if (pType == 12)
                    {
                        gvMain.OptionsView.RowAutoHeight = true;
                    }
                    if ((pType == 1) && (dr["DisplayMember"].ToString() != string.Empty))
                    {
                        CDTBandGridColumn gcl1 = this.GenBandGridColumn(dr, exColNum, false);
                        gcl1.GroupIndex = -1;
                        gcl1.Visible = gcl1.Visible && lstField.ToLower().Contains(gcl.FieldName.ToLower() + ",");
                        gcl1.isExCol = true;
                        RepositoryItem ri1 = this.GenRepository(dr);
                        CDTRepGridLookup CDTRi1 = ri1 as CDTRepGridLookup;
                        if (CDTRi1 != null)
                        {
                            CDTRi1.MainView = gvMain;
                            CDTRi1.MainStruct = dt;
                        }
                        if (ri1 != null)
                        {
                            string caption;
                            string displayMember = dr["DisplayMember"].ToString();
                            ((CDTRepGridLookup)ri1).DisplayMember = displayMember;
                            gcMain.RepositoryItems.Add(ri1);
                            if (Config.GetValue("Language").ToString() == "0")
                            {
                                caption = "Tên " + dr["LabelName"].ToString().ToLower();
                            }
                            else
                            {
                                caption = dr["LabelName2"].ToString() + " name";
                            }
                            int formType = int.Parse(this._data.DrTable["Type"].ToString());
                            if (!(((formType != 1) && (formType != 4)) || bool.Parse(dr["AllowNull"].ToString())))
                            {
                                caption = "*" + caption;
                            }
                            gcl1.Caption = caption;
                            gcl1.VisibleIndex++;
                            gcl1.Width += 200;
                            gcl1.ColumnEdit = ri1;
                            gcl1.Visible = gcl.Visible;
                        }
                        gvMain.Columns.Add(gcl1);
                        exColNum++;
                    }
                    if (gcl.GroupIndex >= 0)
                    {
                        gvMain.GroupSummary.Add(new GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Count, gcl.FieldName, null, "({0} mục)"));
                    }
                }
            }
            gcMain.EndInit();
            gvMain.EndInit();
            return gcMain;
        }
        public CDTGridLookUpEdit GenGridLookupEdit(DataRow drField, bool isCBSControl)
    {
        string condition;
        DataTable dt;
        CDTGridLookUpEdit tmp = isCBSControl ? new CDTGridLookUpEdit() : new CDTGridLookUpEdit();
        string refField = drField["RefField"].ToString();
        string refTable = drField["RefTable"].ToString();
        if (this._formAction != FormAction.Filter)
        {
            condition = drField["refCriteria"].ToString();
        }
        else
        {
            condition = drField["FilterCond"].ToString();
        }
        string Dyncondition = drField["DynCriteria"].ToString();
        bool isMaster = true;
        int n = 0;
        CDTData data = null;
        data = this.GetDataForLookup(refTable, condition, Dyncondition, ref isMaster, ref n);
        FormDesigner fd = new FormDesigner(data);
        string displayMember = drField["DisplayMember"].ToString();
        if (isMaster)
        {
            dt = data.DsStruct.Tables[0];
        }
        else
        {
            dt = data.DsStruct.Tables[1];
        }
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow dr1 = dt.Rows[i];
            CDTGridColumn gcl = fd.GenGridColumn(dr1, 0, false);
            if (dr1["EditMask"].ToString() != string.Empty)
            {
                gcl.DisplayFormat.FormatType = FormatType.Numeric;
                gcl.DisplayFormat.FormatString = dr1["EditMask"].ToString();
            }
                gcl.GroupIndex = -1;
            tmp.Properties.View.Columns.Add(gcl);
        }
        BindingSource bs = new BindingSource();
        if (isMaster)
        {
            bs.DataSource = data.DsData.Tables[0];
        }
        else
        {
            bs.DataSource = data.DsData.Tables[1];
        }
        tmp.fieldName = drField["FieldName"].ToString();
        tmp.Data = data;
        tmp.Properties.DataSource = bs;
        tmp.Properties.ValueMember = refField;
        tmp.refTable = refTable;
        tmp.Properties.Name = drField["FieldName"].ToString();
        tmp.Properties.ValueMember = refField;
        tmp.DymicCondition = Dyncondition;
        tmp.Properties.View.ViewCaption = condition;
        tmp.Allownull =drField["AllowNull"].ToString()=="1";
        tmp.Properties.Buttons[0].Tag = drField;
        tmp.DataIndex = n;
        this._Glist.Add(new LookUp_CDTData(tmp, n));
        this._glist.Add(tmp);
        this.GOldValue.Add(_glist.Count.ToString(), "");
        tmp.Properties.View.OptionsView.ShowFooter = true;
        if (int.Parse(drField["Type"].ToString()) == 1)
        {
            tmp.Properties.DisplayMember = refField;
        }
        else
        {
            tmp.Properties.DisplayMember = displayMember;
        }
        tmp.Properties.PopupFormMinSize = new Size(600, 100);
        tmp.Properties.View.IndicatorWidth = 40;
        tmp.Properties.View.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(this.View_CustomDrawRowIndicator);
        tmp.EditValueChanged += new EventHandler(this.GridLookupEdit_EditValueChanged);
        tmp.Validated += new EventHandler(this.GridLookupEdit_Validated);
        tmp.Popup += new EventHandler(this.GridLookupEdit_Popup);
        tmp.KeyDown += new KeyEventHandler(this.GridLookupEdit_KeyDown);
        tmp.Properties.View.KeyDown += new KeyEventHandler(this.View_KeyDown);
        if (this._formAction != FormAction.Filter)
        {
            EditorButton plusBtn = new EditorButton(data, ButtonPredefines.Plus);
            plusBtn.Shortcut = new KeyShortcut(Keys.F6);
            plusBtn.ToolTip = "F6 - New";
            tmp.Properties.Buttons.Add(plusBtn);

            tmp.Properties.ButtonClick += new ButtonPressedEventHandler(this.Plus_ButtonClick);
            EditorButton refreshData = new EditorButton(data, ButtonPredefines.Ellipsis);
            refreshData.Shortcut = new KeyShortcut(Keys.F3);
            refreshData.ToolTip = "F5 - Refresh";
            tmp.Properties.Buttons.Add(refreshData);
            
        }
        return tmp;
    }

    public LayoutControl GenLayout1(ref GridControl gcMain, bool isCBSControl)
    {
        int i;
        DataRow dr;
        BaseEdit ctrl;
        LayoutControlItem lci;
        DataTable dt = this._data.DsStruct.Tables[0];
        LayoutControl lcMain = new LayoutControl();
        LayoutControlGroup lcgMain = lcMain.Root;
        lcMain.BeginInit();
        lcMain.SuspendLayout();
        lcgMain.BeginInit();
        lcMain.Dock = DockStyle.Fill;
        lcMain.OptionsView.HighlightFocusedItem = true;
        lcgMain.TextVisible = false;
        dt.DefaultView.RowFilter = "Visible = 1";
        if (gcMain != null)
        {
            DataView defaultView = dt.DefaultView;
            defaultView.RowFilter = defaultView.RowFilter + " and IsBottom = 0";
        }
        bool admin = bool.Parse(Config.GetValue("Admin").ToString());
        if (!admin)
        {
            DataView view2 = dt.DefaultView;
            view2.RowFilter = view2.RowFilter + " and (Viewable is null or Viewable = 1)";
        }
        for (i = 0; i < dt.DefaultView.Count; i++)
        {
            dr = dt.DefaultView[i].Row;
            ctrl = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
            if (ctrl != null)
            {
                BaseEdit ctrl1;
                LayoutControlItem lci1;
                if (this._firstControl == null)
                {
                    this._firstControl = ctrl;
                }
                ctrl.StyleController = lcMain;
                int pType = int.Parse(dr["Type"].ToString());
                lci = new LayoutControlItem();
                string caption = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                if (dr["AllowNull"].ToString()=="0")
                {
                    caption = "*" + caption;
                }
                lci.Text = caption;
                lci.Control = ctrl;
                if (dr["Visible"].ToString()=="0")
                {
                    lci.Visibility = LayoutVisibility.Never;
                }
                lcMain.Controls.Add(ctrl);
                lcgMain.AddItem(lci);
                if (((this._formAction != FormAction.Filter) && (pType == 1)) && (dr["DisplayMember"].ToString() != string.Empty))
                {
                    ctrl1 = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                    ((CDTGridLookUpEdit) ctrl1).Properties.DisplayMember = dr["DisplayMember"].ToString();
                    ctrl1.StyleController = lcMain;
                    lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        caption = "Tên " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        caption = dr["LabelName2"].ToString() + " name";
                    }
                    if (dr["AllowNull"].ToString()=="1")
                    {
                        caption = "*" + caption;
                    }
                    lci1.Text = caption;
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    this._BaseList.Add(ctrl1);
                    lcgMain.AddItem(lci1);
                }
                if ((this._formAction == FormAction.Filter) && bool.Parse(dr["IsBetween"].ToString()))
                {
                    ctrl1 = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                    ctrl.Name = ctrl.Name + "1";
                    ctrl.DataBindings.Add("EditValue", this._bindingSource, dr["FieldName"].ToString() + "1");
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        lci.Text = "Từ " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        lci.Text = "From " + dr["LabelName2"].ToString().ToLower();
                    }
                    ctrl1.Name = ctrl1.Name + "2";
                    ctrl1.DataBindings.Add("EditValue", this._bindingSource, dr["FieldName"].ToString() + "2");
                    ctrl1.StyleController = lcMain;
                    lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        lci1.Text = "Đến " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        lci1.Text = "To " + dr["LabelName2"].ToString().ToLower();
                    }
                    if (dr["AllowNull"].ToString() == "0")
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
        }
        dt.DefaultView.RowFilter = "Visible = 1";
        if (!admin)
        {
            DataView view3 = dt.DefaultView;
            view3.RowFilter = view3.RowFilter + " and (Viewable is null or Viewable = 1)";
        }
        if (gcMain != null)
        {
            lcgMain.DefaultLayoutType = LayoutType.Vertical;
            LayoutControlGroup lcg3 = lcgMain.AddGroup();
            LayoutControlItem lcit = new LayoutControlItem();
            gcMain = this.GenGridControl(this._data.DsStruct.Tables[1], true, DockStyle.None);
            lcg3.TextVisible = false;
            lcg3.GroupBordersVisible = false;
            lcit.Name = "Detail";
            lcit.TextVisible = false;
            lcit.Control = gcMain;
            lcMain.Controls.Add(gcMain);
            lcg3.AddItem(lcit);
            DataView view4 = dt.DefaultView;
            view4.RowFilter = view4.RowFilter + " and IsBottom = 1";
            if (dt.DefaultView.Count > 0)
            {
                LayoutControlGroup lcg4 = lcgMain.AddGroup();
                lcg4.TextVisible = false;
                lcg4.GroupBordersVisible = false;
                for (i = 0; i < dt.DefaultView.Count; i++)
                {
                    dr = dt.DefaultView[i].Row;
                    ctrl = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                    if (ctrl != null)
                    {
                        ctrl.StyleController = lcMain;
                        lci = new LayoutControlItem ();
                        lci.Text = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                            lci.Control = ctrl;
                        
                        lcMain.Controls.Add(ctrl);
                        lcg4.AddItem(lci);
                    }
                }
            }
        }
        lcMain.EndInit();
        lcMain.ResumeLayout(false);
        lcgMain.EndInit();
        return lcMain;
    }

    public LayoutControl GenLayout2(ref GridControl gcMain, bool isCBSControl)
    {
        DataRow dr;
        BaseEdit ctrl;
        LayoutControlItem lci;
        DataTable dt = this._data.DsStruct.Tables[0];
             lcMain = new LayoutControl();
        LayoutControlGroup lcgMain = lcMain.Root;
        lcgMain.Name = "Root";
        lcMain.BeginInit();
        lcMain.SuspendLayout();
        lcgMain.BeginInit();
        lcMain.Dock = DockStyle.Fill;
        lcMain.OptionsView.HighlightFocusedItem = true;
        lcgMain.TextVisible = false;
        lcgMain.DefaultLayoutType = LayoutType.Horizontal;
        LayoutControlGroup lcg1 = lcgMain.AddGroup();
        lcg1.TextVisible = false;
        lcg1.GroupBordersVisible = false;
        lcg1.Name = "g1";
        LayoutControlGroup lcg2 = lcgMain.AddGroup();
        lcg2.TextVisible = false;
        lcg2.GroupBordersVisible = false;
        lcg1.Name = "g2";
        lcg1.Size = new Size((lcgMain.Size.Width / 2) + 20, 0);
        if (gcMain != null)
        {
            dt.DefaultView.RowFilter = " IsBottom = 0";
        }
        bool admin = bool.Parse(Config.GetValue("Admin").ToString());
        if (!admin)
        {
            if (dt.DefaultView.RowFilter == "")
            {
                DataView defaultView = dt.DefaultView;
                defaultView.RowFilter = defaultView.RowFilter + " (Viewable is null or Viewable = 1)";
            }
            else
            {
                DataView view2 = dt.DefaultView;
                view2.RowFilter = view2.RowFilter + " and (Viewable is null or Viewable = 1)";
            }
        }
        int i = 0;
        while (i < dt.DefaultView.Count)
        {
            dr = dt.DefaultView[i].Row;
            ctrl = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
            if (ctrl != null)
            {
                BaseEdit ctrl1;
                LayoutControlItem lci1;
                if (this._firstControl == null && dr["Visible"].ToString() == "1")
                {
                    this._firstControl = ctrl;
                }

                ctrl.StyleController = lcMain;
                int pType = int.Parse(dr["Type"].ToString());
                lci = new LayoutControlItem();
                string caption = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                if (dr["AllowNull"].ToString()=="0")
                {
                    caption = "*" + caption;
                }
                lci.Text = caption;
                lci.Control = ctrl;
                lcMain.Controls.Add(ctrl);
                if (dr["Visible"].ToString() == "0")
                {
                    lci.Visibility = LayoutVisibility.OnlyInCustomization;
                }
                this._LayoutList.Add(lci);
                this._BaseList.Add(ctrl);
                lci.Name = dr["FieldName"].ToString();
                if (i < (dt.DefaultView.Count / 2))
                {
                    lcg1.AddItem(lci);
                }
                else
                {
                    lcg2.AddItem(lci);
                }
                if ((((this._formAction != FormAction.Filter) && (pType == 1)) && (dr["DisplayMember"].ToString() != string.Empty)) && ((dr["Visible"].ToString() == "1")||(dr["Visible"].ToString() == "True")))
                {
                    ctrl1 = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                   // ((CDTGridLookUpEdit)ctrl1).Properties.DisplayMember = dr["DisplayMember"].ToString();
                    ctrl1.StyleController = lcMain;
                    lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        caption = "Tên " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        caption = dr["LabelName2"].ToString() + " name";
                    }
                    if (dr["AllowNull"].ToString()=="0")
                    {
                        caption = "*" + caption;
                    }
                    lci1.Text = caption;
                    ctrl1.Name = ctrl1.Name + "001";
                    lci1.Name = dr["fieldName"].ToString() + "001";
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    this._BaseList.Add(ctrl1);
                    lci.Name = dr["FieldName"].ToString();
                    if (i < (dt.DefaultView.Count / 2))
                    {
                        lcg1.AddItem(lci1);
                    }
                    else
                    {
                        lcg2.AddItem(lci1);
                    }
                }
                if (((this._formAction == FormAction.Filter) && bool.Parse(dr["IsBetween"].ToString())) && ((dr["Visible"].ToString() == "1")||(dr["Visible"].ToString() == "True")))
                {
                    ctrl1 = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                    ctrl.Name = ctrl.Name + "1";
                        ctrl.DataBindings.Clear();
                    ctrl.DataBindings.Add("EditValue", this._bindingSource, dr["FieldName"].ToString() + "1");
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        lci.Text = "Từ " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        lci.Text = "From " + dr["LabelName2"].ToString().ToLower();
                    }
                    ctrl1.Name = ctrl1.Name + "2";
                        ctrl1.DataBindings.Clear();
                        ctrl1.DataBindings.Add("EditValue", this._bindingSource, dr["FieldName"].ToString() + "2");
                    ctrl1.StyleController = lcMain;
                    lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        lci1.Text = "Đến " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        lci1.Text = "To " + dr["LabelName2"].ToString().ToLower();
                    }
                    if (dr["AllowNull"].ToString()=="0")
                    {
                        lci.Text = "*" + lci.Text;
                        lci1.Text = "*" + lci1.Text;
                    }
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    if (i < (dt.DefaultView.Count / 2))
                    {
                        lcg1.AddItem(lci1);
                    }
                    else
                    {
                        lcg2.AddItem(lci1);
                    }
                }
            }
            i++;
        }
        if (!admin)
        {
            dt.DefaultView.RowFilter = " (Viewable is null or Viewable = 1)";
        }
            if (gcMain != null)
            {
                lcgMain.DefaultLayoutType = LayoutType.Vertical;
                LayoutControlGroup lcgBt = lcgMain.AddGroup();
                LayoutControlItem lcit = new LayoutControlItem();
                lcgBt.Name = "lcgBt";
                this.TabDetail.SelectedPageChanging += new TabPageChangingEventHandler(this.TabDetail_SelectedPageChanging);
                this.TabDetail.SendToBack();
                XtraTabPage Tab1 = new XtraTabPage();
                Tab1.Text = "Chi tiết";

                this.TabDetail.TabPages.Add(Tab1);
                lcgBt.TextVisible = false;
                lcgBt.GroupBordersVisible = false;

                lcit.Name = "Detail";
                this.TabDetail.Name = "TabDetail";
                lcit.TextVisible = false;
                gcMain = this.GenGridControl(this._data.DsStruct.Tables[1], true, DockStyle.Fill);
                lcit.Control = this.TabDetail;
                this.TabDetail.GotFocus += new EventHandler(this.TabDetail_GotFocus);
                Tab1.Controls.Add(gcMain);
                Tab1.GotFocus += new EventHandler(this.Tab1_GotFocus);
                lcMain.Controls.Add(this.TabDetail);
                lcgBt.AddItem(lcit);
                this._gcDetail = new List<GridControl>();
                for (i = 0; i < this.Data._drTableDt.Count; i++)
                {
                    DataRow drTable = this.Data._drTableDt[i];
                    GridControl gcDt = this.GenGridControlDt(this.Data._dsStructDt.Tables[i], this.Data._dtDetail.Rows[i]["lstField"].ToString(), true, DockStyle.Fill);
                    this._gcDetail.Add(gcDt);
                    GridView gv = gcDt.ViewCollection[0] as GridView;
                    gv.OptionsView.ShowAutoFilterRow = false;
                    gv.OptionsView.ShowGroupPanel = false;
                    //gv.OptionsView.ShowFooter = false;
                    XtraTabPage t = new XtraTabPage();
                    t.GotFocus += new EventHandler(this.Tab1_GotFocus);
                    t.Controls.Add(gcDt);

                    t.Text = this.Data._dtDetail.Rows[i]["DetailName"].ToString();
                    if (this.Data.DsData.Tables[drTable["TableName"].ToString()].Columns.Contains("DTID"))
                    {
                        gcMain.ViewCollection.Add(gv);
                        GridLevelNode gridLevelNode1 = new GridLevelNode();
                        gridLevelNode1.LevelTemplate = gv;
                        gridLevelNode1.RelationName = drTable["TableName"].ToString() + "1";
                        gcMain.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] { gridLevelNode1 });
                        (gcMain.Views[0] as GridView).OptionsDetail.AllowExpandEmptyDetails = true;
                    }
                    else
                    {
                        this.TabDetail.TabPages.Add(t);
                    }
                }
            
            //lcgMain.DefaultLayoutType = LayoutType.Vertical;
            //LayoutControlGroup lcg3 = lcgMain.AddGroup();
            //LayoutControlItem lcit = new LayoutControlItem();
            //gcMain = this.GenGridControl(this._data.DsStruct.Tables[1], true, DockStyle.None);
            //lcg3.TextVisible = false;
            //lcg3.GroupBordersVisible = false;
            //lcit.Name = "Detail";
            //lcit.TextVisible = false;
            //lcit.Control = gcMain;
            //lcMain.Controls.Add(gcMain);
            //lcg3.AddItem(lcit);
            dt.DefaultView.RowFilter = " IsBottom = 1";
            if (dt.DefaultView.Count > 0)
            {
                LayoutControlGroup lcg4 = lcgMain.AddGroup();
                lcg4.TextVisible = false;
                lcg4.GroupBordersVisible = false;
                lcg4.Name = "lcg4";
                lcg4.DefaultLayoutType = LayoutType.Horizontal;
                LayoutControlGroup lcg5 = lcg4.AddGroup();
                lcg5.TextVisible = false;
                lcg5.GroupBordersVisible = false;
                lcg5.Name = "lcg5";
                lcg5.DefaultLayoutType = LayoutType.Vertical;
                LayoutControlGroup lcg6 = lcg4.AddGroup();
                lcg6.TextVisible = false;
                lcg6.GroupBordersVisible = false;
                lcg6.Name = "lcg6";
                lcg6.DefaultLayoutType = LayoutType.Vertical;
                for (i = 0; i < dt.DefaultView.Count; i++)
                {
                    dr = dt.DefaultView[i].Row;
                    ctrl = this.GenCBSControl(dr);
                    if (ctrl != null)
                    {
                        ctrl.StyleController = lcMain;
                        lci = new LayoutControlItem();
                        lci.Text = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                        lci.Control = ctrl;
                        lci.Name = dr["fieldName"].ToString();
                        if (dr["Visible"].ToString() == "0")
                        {
                            lci.Visibility = LayoutVisibility.OnlyInCustomization;
                        }
                        lcMain.Controls.Add(ctrl);
                        this._BaseList.Add(ctrl);
                        this._LayoutList.Add(lci);
                        if (i < (dt.DefaultView.Count / 2))
                        {
                            lcg5.AddItem(lci);
                        }
                        else
                        {
                            lcg6.AddItem(lci);
                        }
                    }
                }
            }
            dt.DefaultView.RowFilter = string.Empty;
        }
        lcMain.EndInit();
        lcMain.ResumeLayout(false);
        lcgMain.EndInit();
        return lcMain;
    }

    public LayoutControl GenLayout3(ref GridControl gcMain, bool isCBSControl)
    {
        int i;
        DataRow dr;
        BaseEdit ctrl;
        LayoutControlItem lci;
       
        DataTable dt = this._data.DsStruct.Tables[0];
        LayoutControl lcMain = new LayoutControl();
        
        LayoutControlGroup lcgMain = lcMain.Root;
        lcgMain.Name = "Root";
        lcMain.BeginInit();
        lcMain.SuspendLayout();
        lcgMain.BeginInit();
        lcMain.Dock = DockStyle.Fill;
        lcMain.OptionsView.HighlightFocusedItem = true;
        lcMain.BringToFront();
        lcgMain.TextVisible = false;
        lcgMain.DefaultLayoutType = LayoutType.Horizontal;
        LayoutControlGroup lcg1 = lcgMain.AddGroup();
        lcg1.TextVisible = false;
        lcg1.GroupBordersVisible = false;
        lcg1.Name = "g1";
        LayoutControlGroup lcg2 = lcgMain.AddGroup();
        lcg2.TextVisible = false;
        lcg2.GroupBordersVisible = false;
        lcg2.Name = "g2";
        LayoutControlGroup lcg3 = lcgMain.AddGroup();
        lcg3.TextVisible = false;
        lcg3.GroupBordersVisible = false;
        lcg3.Name = "g3";
        lcg1.Size = new Size((lcgMain.Size.Width / 3) + 20, 0);
        if (gcMain != null)
        {
            dt.DefaultView.RowFilter = " IsBottom = 0";
        }
        bool admin = bool.Parse(Config.GetValue("Admin").ToString());
        if (!admin)
        {
            if (dt.DefaultView.RowFilter == "")
            {
                DataView defaultView = dt.DefaultView;
                defaultView.RowFilter = defaultView.RowFilter + " (Viewable is null or Viewable = 1)";
            }
            else
            {
                DataView view2 = dt.DefaultView;
                view2.RowFilter = view2.RowFilter + " and (Viewable is null or Viewable = 1)";
            }
        }
        DataRow[] drCount = dt.Select("Visible <> '0' and IsBottom=0 and Type<>13");
        DataRow[] drCountDisplay = dt.Select("DisplayMember is not null and DisplayMember <>'' and IsBottom=0");
        DataRow[] drMemo = dt.Select("Type=13 and IsBottom=0 and visible=1");
        int ControlCount = drCount.Length + drCountDisplay.Length + drMemo.Length * 3;
        int j = 0;
        for (i = 0; i < dt.DefaultView.Count; i++)
        {
            dr = dt.DefaultView[i].Row;
            ctrl = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
            if (ctrl != null)
            {

                BaseEdit ctrl1;
                LayoutControlItem lci1;
                if (this._firstControl == null && dr["Visible"].ToString() == "1")
                {
                    this._firstControl = ctrl;
                }
                ctrl.StyleController = lcMain;
                int pType = int.Parse(dr["Type"].ToString());
                lci = new LayoutControlItem();
                string caption = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                if (dr["AllowNull"].ToString()=="0")
                {
                    caption = "*" + caption;
                }
                lci.Text = caption;
                lci.Control = ctrl;

                lcMain.Controls.Add(ctrl);
                
                if (dr["Visible"].ToString() == "0")
                {
                    lci.Visibility = LayoutVisibility.OnlyInCustomization;
                }

                if(lci.Visibility==LayoutVisibility.Always)
                {
                    if (ctrl.ToString().Contains("Memo"))
                        j += 2;
                    else
                        j++;

                }
                
                this._LayoutList.Add(lci);
                this._BaseList.Add(ctrl);
                lci.Name = dr["FieldName"].ToString();
                if (j <= (ControlCount / 3))
                {
                    lcg1.AddItem(lci);
                }
                else if (j <= ((ControlCount * 2) / 3))
                {
                    lcg2.AddItem(lci);
                }
                else
                {
                    lcg3.AddItem(lci);
                }
                if (dr["Visible"].ToString() != "0" && ctrl.ToString().Contains("Memo"))
                {
                    j += 1;
                }

                if ((((this._formAction != FormAction.Filter) && (pType == 1)) && (dr["DisplayMember"].ToString() != string.Empty)) && ((dr["Visible"].ToString() == "1")||(dr["Visible"].ToString() == "True")))
                {
                    ctrl1 = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                    ((CDTGridLookUpEdit)ctrl1).Properties.DisplayMember = dr["DisplayMember"].ToString();
                   // ((CDTGridLookUpEdit)ctrl1).Properties.Name = dr["DisplayMember"].ToString();
                    ((CDTGridLookUpEdit)ctrl1).Properties.Name = dr["FieldName"].ToString();
                    ctrl1.StyleController = lcMain;
                    lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        caption = "Tên " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        caption = dr["LabelName2"].ToString() + " name";
                    }
                    if (dr["AllowNull"].ToString()=="0")
                    {
                        caption = "*" + caption;
                    }
                    lci1.Text = caption;
                    ctrl1.Name = ctrl1.Name + "001";
                    lci1.Name = dr["FieldName"].ToString() + "001";
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    this._BaseList.Add(ctrl1);
                    j++;
                    if (j <= (ControlCount / 3))
                    {
                        lcg1.AddItem(lci1);
                    }
                    else if (j <= ((ControlCount * 2) / 3))
                    {
                        lcg2.AddItem(lci1);
                    }
                    else
                    {
                        lcg3.AddItem(lci1);
                    }
                }
                if (((this._formAction == FormAction.Filter) && bool.Parse(dr["IsBetween"].ToString())) && ((dr["Visible"].ToString() == "1")||(dr["Visible"].ToString() == "True")))
                {
                    ctrl1 = isCBSControl ? this.GenCBSControl(dr) : this.GenControl(dr);
                    ctrl.Name = ctrl.Name + "1";
                    ctrl.DataBindings.Add("EditValue", this._bindingSource, dr["FieldName"].ToString() + "1");
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        lci.Text = "Từ " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        lci.Text = "From " + dr["LabelName2"].ToString().ToLower();
                    }
                    ctrl1.Name = ctrl1.Name + "2";
                    ctrl1.DataBindings.Add("EditValue", this._bindingSource, dr["FieldName"].ToString() + "2");
                    ctrl1.StyleController = lcMain;
                    lci1 = new LayoutControlItem();
                    if (Config.GetValue("Language").ToString() == "0")
                    {
                        lci1.Text = "Đến " + dr["LabelName"].ToString().ToLower();
                    }
                    else
                    {
                        lci1.Text = "To " + dr["LabelName2"].ToString().ToLower();
                    }
                    if (dr["AllowNull"].ToString()=="0")
                    {
                        lci.Text = "*" + lci.Text;
                        lci1.Text = "*" + lci1.Text;
                    }
                    lci1.Control = ctrl1;
                    lci1.Visibility = lci.Visibility;
                    lcMain.Controls.Add(ctrl1);
                    
                    if (j <= (ControlCount / 3))
                    {
                        lcg1.AddItem(lci);
                    }
                    else if (j <= ((ControlCount * 2) / 3))
                    {
                        lcg2.AddItem(lci);
                    }
                    else
                    {
                        lcg3.AddItem(lci);
                    }
                }
            }
        }
        if (!admin)
        {
            dt.DefaultView.RowFilter = " (Viewable is null or Viewable = 1)";
        }
        if (gcMain != null)
        {
            lcgMain.DefaultLayoutType = LayoutType.Vertical;
            LayoutControlGroup lcgBt = lcgMain.AddGroup();
            LayoutControlItem lcit = new LayoutControlItem();
            lcgBt.Name = "lcgBt";
            this.TabDetail.SelectedPageChanging += new TabPageChangingEventHandler(this.TabDetail_SelectedPageChanging);
            this.TabDetail.SendToBack();
            XtraTabPage Tab1 = new XtraTabPage();
            Tab1.Text = "Chi tiết";

            this.TabDetail.TabPages.Add(Tab1);
            lcgBt.TextVisible = false;
            lcgBt.GroupBordersVisible = false;
            lcit.Name = "Detail";
            this.TabDetail.Name = "TabDetail";
            lcit.TextVisible = false;
            gcMain = this.GenGridControl(this._data.DsStruct.Tables[1], true, DockStyle.Fill);
            lcit.Control = this.TabDetail;
            this.TabDetail.GotFocus += new EventHandler(this.TabDetail_GotFocus);
            Tab1.Controls.Add(gcMain);
            Tab1.GotFocus += new EventHandler(this.Tab1_GotFocus);
            lcMain.Controls.Add(this.TabDetail);
                (gcMain.Views[0] as GridView).OptionsView.ShowDetailButtons = true;

                lcgBt.AddItem(lcit);
            this._gcDetail = new List<GridControl>();
                for (i = 0; i < this.Data._drTableDt.Count; i++)
                {

                    DataRow drTable = this.Data._drTableDt[i];
                    DataRow drDetail = this.Data._dtDetail.Rows[i];
                    GridControl gcDt;
                    if (drTable.Table.Columns.Contains("useband") &&  bool.Parse(drTable["useBand"].ToString()))
                    {
                        gcDt=this.GenBandGridControlDt(this.Data._dsStructDt.Tables[i],_data._dsBand.Tables[drTable["TableName"].ToString()], this.Data._dtDetail.Rows[i]["lstField"].ToString(), true, DockStyle.Fill);
                    }
                    else
                    {
                        gcDt = this.GenGridControlDt(this.Data._dsStructDt.Tables[i], this.Data._dtDetail.Rows[i]["lstField"].ToString(), true, DockStyle.Fill);
                        
                    }
                    this._gcDetail.Add(gcDt);
                    AdvBandedGridView gb=new AdvBandedGridView();
                    GridView gv=new GridView();
                    if (drTable.Table.Columns.Contains("DTID") && bool.Parse(drTable["useBand"].ToString()))
                    {
                        gb = gcDt.Views[0] as AdvBandedGridView;
                        gb.OptionsView.ShowAutoFilterRow = false;
                        gb.OptionsView.ShowGroupPanel = false;
                    }
                    else
                    {
                        gv = gcDt.Views[0] as GridView;
                        gv.OptionsView.ShowAutoFilterRow = false;
                        gv.OptionsView.ShowGroupPanel = false;
                    }
                    // gv.OptionsView.ShowFooter = false;
                    XtraTabPage t = new XtraTabPage();

                    t.GotFocus += new EventHandler(this.Tab1_GotFocus);
                    t.Controls.Add(gcDt);

                    t.Text = drDetail["DetailName"].ToString();
                    if (this.Data.DsData.Tables[drTable["TableName"].ToString()].Columns.Contains("DTID"))
                    {
                        GridLevelNode gridLevelNode1 = new GridLevelNode();
                        if (drTable.Table.Columns.Contains("DTID") && bool.Parse(drTable["useBand"].ToString()))
                        {                            
                            gcMain.ViewCollection.Add(gb);
                            gridLevelNode1.LevelTemplate = gb;
                        }
                        else
                        {                            
                            gcMain.ViewCollection.Add(gv);
                            gridLevelNode1.LevelTemplate = gv;
                        }
                        
                        
                        gridLevelNode1.RelationName = drTable["TableName"].ToString() + "1";
                        gcMain.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] { gridLevelNode1 });
                        if (drTable.Table.Columns.Contains("useband") && bool.Parse(drTable["useBand"].ToString()))
                        {
                            gb.OptionsDetail.AllowExpandEmptyDetails = true; gb.ViewCaption = drTable["DienGiai"].ToString();
                        }
                        else
                        {
                            gv.OptionsDetail.AllowExpandEmptyDetails = true; gv.ViewCaption = drTable["DienGiai"].ToString();
                        }
                        
                    }
                    else
                    {
                        if(drDetail["Outtag"]!=DBNull.Value && bool.Parse(drDetail["Outtag"].ToString()))
                        {
                            LayoutControlItem lcitdt = new LayoutControlItem();
                            lcitdt.Name = drDetail["DetailName"].ToString();
                            lcitdt.TextVisible = false;
                            lcitdt.Control = gcDt;
                            lcgBt.AddItem(lcitdt);
                        }
                        else
                        {
                            this.TabDetail.TabPages.Add(t);
                        }
                       
                    }
                }
            dt.DefaultView.RowFilter = " IsBottom = 1";
            if (dt.DefaultView.Count > 0)
            {
                LayoutControlGroup lcg4 = lcgMain.AddGroup();
                lcg4.TextVisible = false;
                lcg4.GroupBordersVisible = false;
                lcg4.Name = "lcg4";
                lcg4.DefaultLayoutType = LayoutType.Horizontal;
                LayoutControlGroup lcg5 = lcg4.AddGroup();
                lcg5.Name = "lcg5";
                lcg5.TextVisible = false;
                lcg5.GroupBordersVisible = false;
                lcg5.DefaultLayoutType = LayoutType.Vertical;
                LayoutControlGroup lcg6 = lcg4.AddGroup();
                lcg6.TextVisible = false;
                lcg6.GroupBordersVisible = false;
                lcg6.Name = "lcg6";
                lcg6.DefaultLayoutType = LayoutType.Vertical;
                for (i = 0; i < dt.DefaultView.Count; i++)
                {
                    dr = dt.DefaultView[i].Row;
                    ctrl = this.GenCBSControl(dr);
                    if (ctrl != null)
                    {
                        ctrl.StyleController = lcMain;
                        lci = new LayoutControlItem();
                        lci.Text = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
                        lci.Control = ctrl;
                        lci.Name = dr["fieldName"].ToString();
                        if (dr["Visible"].ToString() == "0")
                        {
                            lci.Visibility = LayoutVisibility.OnlyInCustomization;
                        }
                        
                        lcMain.Controls.Add(ctrl);
                        this._BaseList.Add(ctrl);
                        this._LayoutList.Add(lci);
                        if (i < (dt.DefaultView.Count / 2))
                        {
                            lcg5.AddItem(lci);
                        }
                        else
                        {
                            lcg6.AddItem(lci);
                        }
                    }
                }
            }
            dt.DefaultView.RowFilter = string.Empty;
        }
        lcMain.EndInit();
        lcMain.ResumeLayout(false);
        lcgMain.EndInit();
        return lcMain;
    }

    private RepositoryItem GenRepository(DataRow dr)
    {
        RepositoryItem tmp = null;
        int pType = int.Parse(dr["Type"].ToString());
        switch (pType)
        {
            case 1:
            case 2:
            case 4:
            case 7:
                if ((pType != 2) || !(dr["refTable"].ToString() == string.Empty))
                {

                    tmp = this.GenRIGridLookupEdit(dr);
                    // tmp = new CDTRepGridLookup();
                    CDTRepGridLookup riTmp = tmp as CDTRepGridLookup;

                    riTmp.CloseUpKey = KeyShortcut.Empty;
                    riTmp.AllowNullInput = DefaultBoolean.True;
                    riTmp.NullText = string.Empty;
                    riTmp.View.OptionsView.ShowAutoFilterRow = true;
                    riTmp.View.OptionsView.ColumnAutoWidth = false;
                    if (riTmp.DymicCondition != null)
                    {

                        this.RIOldValue.Add(_lstRep.Count.ToString(), "");
                        this._lstRep.Add(riTmp);
                    }
                }
                break;

            case 5:
                tmp = new RepositoryItemSpinEdit();
                (tmp as RepositoryItemSpinEdit).AllowNullInput = DefaultBoolean.True;
                break;

            case 8:
                tmp = new RepositoryItemCalcEdit();
                (tmp as RepositoryItemCalcEdit).AllowNullInput = DefaultBoolean.True;
                (tmp as RepositoryItemCalcEdit).Spin += new SpinEventHandler(this.FormDesigner_Spin1);
                (tmp as RepositoryItemCalcEdit).KeyUp += new KeyEventHandler(this.VCalEdit_KeyUp);
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

            case 11:
                tmp = new RepositoryItemTimeEdit();
                (tmp as RepositoryItemTimeEdit).AllowNullInput = DefaultBoolean.True;
                break;

            case 12:
                tmp = new RepositoryItemPictureEdit ();
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
        {
            tmp.Name = dr["FieldName"].ToString();
        }
        return tmp;
    }

    private CDTRepGridLookup GenRIGridLookupEdit(DataRow drField)
    {
        DataTable dt;
        CDTRepGridLookup tmp = new CDTRepGridLookup();
        string refField = drField["RefField"].ToString();
        string refTable = drField["RefTable"].ToString();
        string condition = drField["refCriteria"].ToString();
        string Dyncondition = drField["DynCriteria"].ToString();
        string displayMember = drField["DisplayMember"].ToString();
        bool isMaster = true;
        int n = 0;
        if (refTable == "DT28")
        {
        }
        CDTData data = this.GetDataForLookup(refTable, condition, Dyncondition, ref isMaster, ref n);
        FormDesigner fd = new FormDesigner(data);
        if (isMaster)
        {
            dt = data.DsStruct.Tables[0];
        }
        else
        {
            dt = data.DsStruct.Tables[1];
        }
        int k = 0;
        for (int i = 0; i < dt.Rows.Count; i++)
        {

            DataRow dr1 = dt.Rows[i];
            if (dr1["TabIndex"].ToString() == "-1" && !(dr1["Type"].ToString() == "7" || dr1["Type"].ToString() == "4" || dr1["Type"].ToString() == "0" || dr1["Type"].ToString() == "3" || dr1["Type"].ToString() == "6"))
            {
               
                continue;
            }
            k++;
            CDTGridColumn gcl = fd.GenGridColumn(dr1, 0, false);
            if (dr1["EditMask"].ToString() != string.Empty)
            {
                gcl.DisplayFormat.FormatType = FormatType.Numeric;
                gcl.DisplayFormat.FormatString = dr1["EditMask"].ToString();
            }
            tmp.View.Columns.Add(gcl);
            //if (k == 8)
            //{
            //    break;
            //}
        }
        BindingSource bs = new BindingSource();
        if (isMaster)
        {
            bs.DataSource = data.DsData.Tables[0];
        }
        else
        {
            bs.DataSource = data.DsData.Tables[1];
        }
        tmp.Data = data;
        tmp.DataSource = bs;
        tmp.refTable = refTable;
        tmp.ValueMember = refField;
        tmp.DymicCondition = Dyncondition;
        tmp.Name = drField["FieldName"].ToString();
        tmp.Condition = condition;
        tmp.DataIndex = n;
        this._Rlist.Add(new RLookUp_CDTData(tmp, n));
        this._rlist.Add(tmp);
        
        if (int.Parse(drField["Type"].ToString()) == 1)
        {
            tmp.DisplayMember = refField;
        }
        else
        {
            tmp.DisplayMember = displayMember;
        }
        tmp.PopupFormMinSize = new Size(600, 100);
        tmp.View.OptionsView.ShowFooter = true;
        tmp.View.IndicatorWidth = 40;
        tmp.ImmediatePopup = true;
            tmp.TextEditStyle = TextEditStyles.Standard;
            //tmp.ReadOnly = true;
        tmp.View.CustomDrawRowIndicator += new RowIndicatorCustomDrawEventHandler(this.View_CustomDrawRowIndicator);
        
        if (tmp.refTable.Substring(0, 1) != "w"  )
        {
            EditorButton plusBtn = new EditorButton(data, ButtonPredefines.Plus);
            plusBtn.Shortcut = new KeyShortcut(Keys.F2);
            plusBtn.ToolTip = "F2 - New";
            tmp.Buttons.Add(plusBtn);
           
        }

            tmp.Button_click += new ButtonPressedEventHandler(this.tmp_RIButton_click);
            EditorButton refreshData = new EditorButton(data, ButtonPredefines.Ellipsis);
        refreshData.Shortcut = new KeyShortcut(Keys.F5);
        refreshData.ToolTip = "F5 - Refresh";
        tmp.Buttons.Add(refreshData);

        tmp.Popup += new EventHandler(this.RIGridLookupEdit_Popup);
        if (drField["AllowNull"].ToString() == "0")
        {
            tmp.AllowNullInput = DefaultBoolean.False;
        }
            tmp.KeyDown += new KeyEventHandler(this.RiGridLookupEdit_KeyDown);
           
       
        tmp.DymicCondition = Dyncondition;
        tmp.CloseUp +=  new CloseUpEventHandler(RItmp_CloseUp);

        tmp.EditValueChanged += new EventHandler(this.RIGridLookupEdit_EditValueChanged);
        tmp.Validating += new CancelEventHandler(this.RIGridLookupEdit_Validating);
        tmp.View.KeyDown += new KeyEventHandler(this.View_KeyDown);
        return tmp;
    }

    private TreeListColumn GenTreeListColumn(DataRow dr, int exColNum, bool checkData)
    {
        TreeListColumn tlcl = new TreeListColumn();
            tlcl.Name = "cl" + dr["FieldName"].ToString();
            tlcl.FieldName = dr["FieldName"].ToString();
            tlcl.Caption = (Config.GetValue("Language").ToString() == "0") ? dr["LabelName"].ToString() : dr["LabelName2"].ToString();
            tlcl.VisibleIndex = int.Parse(dr["TabIndex"].ToString()) + exColNum;
        
        if (!checkData)
        {
            tlcl.Visible = dr["Visible"].ToString() == "1";
        }
        return tlcl;
    }

    internal TreeList GenTreeListControl(DataRow drTable, DataTable dt)
    {

        TreeList tlMain = new TreeList();
        int bType = int.Parse(drTable["Type"].ToString());
        tlMain.BeginInit();
        tlMain.Dock = DockStyle.Fill;
        tlMain.KeyFieldName = drTable["Pk"].ToString();
        tlMain.ParentFieldName = drTable["ParentPk"].ToString();
        tlMain.OptionsView.AutoWidth = false;
        tlMain.OptionsView.EnableAppearanceEvenRow = true;
        tlMain.Visible = false;
        tlMain.OptionsBehavior.Editable = false;
        switch (bType)
        {
            case 1:
            case 4:
                tlMain.OptionsBehavior.Editable = true;
                break;
        }
        int reCol = 0;
        int deCol = 0;
        foreach (DataRow dr in dt.Rows)
        {
            if (Int32.Parse(dr["Type"].ToString()) == 1 && tlMain.ParentFieldName.ToUpper() != dr["FieldName"].ToString().ToUpper())
                reCol++;
            if (tlMain.ParentFieldName.ToUpper() == dr["FieldName"].ToString().ToUpper())
                deCol++;
        }
        TreeListColumn[] tlcls = new TreeListColumn[(dt.Rows.Count + reCol) - deCol];
        int exColNum = 0;
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow dr =dt.Rows[i];
            if (tlMain.ParentFieldName.ToUpper() == dr["FieldName"].ToString().ToUpper())
            {
                exColNum--;
            }
            else
            {
                TreeListColumn tlcl = this.GenTreeListColumn(dr, exColNum, false);
                RepositoryItem ri = this.GenRepository(dr);
                if (ri != null)
                {
                    tlMain.RepositoryItems.Add(ri);
                    tlcl.ColumnEdit = ri;
                }
                tlcls[i + exColNum] = tlcl;
                if ((int.Parse(dr["Type"].ToString()) == 1) && (dr["DisplayMember"].ToString() != string.Empty))
                {
                    TreeListColumn tlcl1 = this.GenTreeListColumn(dr, exColNum, false);
                    RepositoryItem ri1 = this.GenRepository(dr);
                    if (ri1 != null)
                    {
                        string caption;
                        string displayMember = dr["DisplayMember"].ToString();
                        ((CDTRepGridLookup) ri1).DisplayMember = displayMember;
                        tlMain.RepositoryItems.Add(ri1);
                        if (Config.GetValue("Language").ToString() == "0")
                        {
                            caption = "T\x00ean " + dr["LabelName"].ToString().ToLower();
                        }
                        else
                        {
                            caption = dr["LabelName2"].ToString() + " name";
                        }
                        if (!(dr["AllowNull"].ToString()=="1"))
                        {
                            caption = "*" + caption;
                        }
                        tlcl1.Caption = caption;
                        tlcl1.VisibleIndex++;
                        tlcl1.Width += 100;
                        tlcl1.ColumnEdit = ri1;
                        tlcl1.Visible = tlcl.Visible;
                    }
                    exColNum++;
                    tlcls[i + exColNum] = tlcl1;
                }
            }
        }
        tlMain.Columns.AddRange(tlcls);
        tlMain.EndInit();
        return tlMain;
    }

    private CDTData GetDataForLookup(string tableName, string condition, string DynCondition, ref bool isMaster, ref int n)
    {
        CDTData data = null;
        foreach (CDTData d in this._lstData)
        {
            if ((((d.dataType != DataType.MasterDetail) && (d.DrTable["TableName"].ToString().ToUpper() == tableName.ToUpper())) && (d.DrTable["TableName"].ToString().ToUpper() != this._data.DrTable["TableName"].ToString().ToUpper())) && (d.Condition.Trim() == condition.Trim()))
            {
                n = this._lstData.IndexOf(d);
                data = d;
                if (d.dataType == DataType.MasterDetail)
                {
                    isMaster = false;
                }
                else if (d.dataType == DataType.Detail)
                {
                    isMaster = true;
                }
                break;
            }
            if (((d.DrTableMaster != null) && (d.DrTableMaster["TableName"].ToString().ToUpper() == tableName.ToUpper())) && (d.ConditionMaster.Trim() == condition.Trim()))
            {
                n = this._lstData.IndexOf(d);
                data = d;
                if (d.dataType == DataType.MasterDetail)
                {
                    isMaster = true;
                }
                else if (d.dataType == DataType.Detail)
                {
                    isMaster = false;
                }
                break;
            }
        }
        if (data == null)
        {
            CDTData dataInPulibc = publicCDTData.findCDTData(tableName, condition, DynCondition);
            if (dataInPulibc != null)
            {
                data = dataInPulibc;
                if (!data.FullData) data.GetData();
                if (this._lstData.Exists(c => c._tableName == data._tableName && c.Condition == data.Condition && c.DynCondition == data.DynCondition))
                {
                   
                }
                else
                {
                    this._lstData.Add(data);
                    n = this._lstData.Count - 1;
                }
            }
            else
            {
                string sysPackageID = Config.GetValue("sysPackageID").ToString();
                data = DataFactory.DataFactory.Create(DataType.Single, tableName, sysPackageID);
                data.Condition = condition;
                data.DynCondition = DynCondition;
                if (((tableName == "sysTable") || (tableName == "sysField")) || (tableName == "sysDataConfig") || (tableName == "sysPackage"))
                {
                    data.GetData();
                    data.FullData = true;
                }
                else
                {
                    data.GetDataForLookup(this._data);
                }


                this._lstData.Add(data);
                publicCDTData.AddCDTData(data);
                n = this._lstData.Count - 1;
            }
            return data;
        }
        if (data.DsData == null)
        {
            data.GetData();
        }
        return data;
    }

    private void GridLookupEdit_EditValueChanged(object sender, EventArgs e)
    {
        try
        {
            CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
            tmp.Refresh();
            string value = tmp.EditValue.ToString();
            BindingSource bs = tmp.Properties.DataSource as BindingSource;
            if (_data.DrCurrentMaster != null && _data.DrCurrentMaster.RowState != DataRowState.Unchanged)
            {

                if (((this._formAction != FormAction.View) && (this._formAction != FormAction.Delete)) && (tmp.EditValue != null))
                {


                    if (!tmp.Data.FullData)
                    {

                        this.RefreshLookup(tmp.DataIndex);
                    }
                }
            }
            int index = tmp.Properties.GetIndexByKeyValue(value);
            if ((index >= 0) && (value != string.Empty))
            {
                DataTable dt = bs.DataSource as DataTable;
                DataRow drData;
                DataRowView drDataView = tmp.Properties.GetRowByKeyValue(value) as DataRowView;
                if (drDataView != null)
                {
                    drData = drDataView.Row;
                    if ((drData != null) && (this._formAction == FormAction.Filter))
                    {
                        for (int i = 0; i < drData.Table.Columns.Count; i++)
                        {
                            (this._data as DataReport).reConfig.NewKeyValue("@" + drData.Table.Columns[i].ColumnName, drData[i]);
                        }
                    }
                    else if (drData != null)
                    {
                        if (_data.DrCurrentMaster != null && _data.DrCurrentMaster.RowState != DataRowState.Unchanged)
                        {
                            this._data.SetValuesFromList(tmp.Properties.Name, value, drData, false);
                            this.RefreshDataForLookup(tmp.Name, false);
                        }
                    }
                }


            }

            //this.setDynFiter(tmp);
        }
        catch
        {
        }
        CDTGridLookUpEdit tm1 = sender as CDTGridLookUpEdit;
    }

    private void GridLookupEdit_KeyDown(object sender, KeyEventArgs e)
    {
        CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
        if (!tmp.Allownull)
        {
            if ((!tmp.IsPopupOpen && ((tmp.EditValue == null) || (tmp.EditValue.ToString() == string.Empty))) && (e.KeyCode == Keys.Return))
            {
                tmp.ShowPopup();
                e.Handled = true;
            }
            if (((!tmp.IsPopupOpen && (tmp.EditValue != null)) && (tmp.EditValue.ToString() != string.Empty)) && (e.KeyCode == Keys.Delete))
            {
                tmp.EditValue = null;
            }
        }
    }

    private void GridLookupEdit_Popup(object sender, EventArgs e)
    {
        CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
        //int n = -1;
        //int i = 0;
        //for (i = 0; i < this._Glist.Count; i++)
        //{
        //    if (this._Glist[i].glk == tmp)
        //    {
        //        n = this._Glist[i].dataIndex;
        //        break;
        //    }
        //}
        
        this.RefreshLookup(tmp.DataIndex);
        setDynFilter(tmp);
    }

    private void GridLookupEdit_Validated(object sender, EventArgs e)
    {
        CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
       // this.setDynFiter(tmp);
    }

    private void gvMain_CellValueChanged(object sender,DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
    {
        CDTRepGridLookup tmp = e.Column.ColumnEdit as CDTRepGridLookup;
        if (tmp != null)
        {
            //this.setRepFilter(tmp);
        }
    }

    private void gvMain_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
    {
        if (e.FocusedRowHandle < 0)
        {
            GridView gvMain = sender as GridView;
            try
            {
                gvMain.FocusedColumn = gvMain.VisibleColumns[0];
            }
            catch
            {
            }
        }
    }

    private void Plus_ButtonClick(object sender, ButtonPressedEventArgs e)
       {
           if (e.Button.Tag != null)
           {

               CDTGridLookUpEdit tmp = sender as CDTGridLookUpEdit;
               if (e.Button.ToolTip == "F5 - Refresh")
               {
                   int n = -1;
                   int i = 0;
                   for (i = 0; i < this._Glist.Count; i++)
                   {
                       if (this._Glist[i].glk == tmp)
                       {
                           n = this._Glist[i].dataIndex;
                           break;
                       }
                   }
                   this.RefreshLookupAllowFull(n);
                   return;
               }
               if (!tmp.Data.FullData)
               {
                   //int n = -1;
                   //int i = 0;
                   //for (i = 0; i < this._Glist.Count; i++)
                   //{
                   //    if (this._Glist[i].glk == tmp)
                   //    {
                   //        n = this._Glist[i].dataIndex;
                   //        break;
                   //    }
                   //}
                   this.RefreshLookup(tmp.DataIndex);
               }
               CDTData d = e.Button.Tag as CDTData;
               bool ok = false;
               BindingSource bs =null;
               FrmSingleDt frm =null;
               FormAction fAction =FormAction.View;
               if (e.Button.ToolTip == "F3 - Edit" && d != null)
               {

                  bs = tmp.Properties.DataSource as BindingSource;
                   //bs.DataSource = d.DsData.Tables[0];  
                 frm = new FrmSingleDt(d, bs, true);

                   if (frm.ShowDialog() == DialogResult.OK)
                   {
                       ok = true;
                       fAction = FormAction.Edit;
                   }
               }
               else if (e.Button.ToolTip == "F6 - New" && d != null)
               {
                   // BindingSource bs = tmp.Properties.DataSource as BindingSource;

                    bs = new BindingSource();
                   bs.DataSource = d.DsData.Tables[0];
                   frm = new FrmSingleDt(d, bs);
                   if (frm.ShowDialog() == DialogResult.OK)
                   {
                       ok = true;
                       fAction = FormAction.New;
                   }
               }
              

               if (ok && bs!=null)
               {
                   tmp.Properties.DataSource = bs;
                   string tableName;
                   if (tmp.refTable != null)
                   {
                       tableName = tmp.refTable;
                   }
                   else
                   {
                       tableName = tmp.refTable;
                   }
                   List<string> _GlistNametmp = new List<string>();
                   foreach (LookUp_CDTData ldtmp in frm._frmDesigner._Glist)
                   {
                       _GlistNametmp.Add(ldtmp.glk.refTable);
                   }
                   foreach (RLookUp_CDTData rgc in this._Rlist)
                   {

                       CDTRepGridLookup rg = rgc.rglk;
                       if (rg.refTable == "DMKH")
                       {
                       }
                       if ((rg.refTable.ToUpper() == tableName.ToUpper()) && (rg.View.ViewCaption.ToUpper() == d.Condition.ToUpper()))
                       {
                           rg.DataSource = null;
                           bs.DataSource = d.DsData.Tables[0];
                           rg.DataSource = bs;
                       }
                       if (_GlistNametmp.Contains(rg.refTable))
                       {
                           this.RefreshLookupAllowFull(rgc.dataIndex);
                       }
                   }
                   foreach (LookUp_CDTData rgc in this._Glist)
                   {
                       CDTGridLookUpEdit rg = rgc.glk;
                       if ((rg.refTable.ToUpper() == tableName.ToUpper()) && (rg.Condition == d.Condition))
                       {

                           rg.Properties.DataSource = null;
                           rg.Properties.DataSource = bs;
                       }
                       if (_GlistNametmp.Contains(rg.refTable))
                       {
                           this.RefreshLookupAllowFull(rgc.dataIndex);
                       }
                   }
                   if ((tmp.GetType() == typeof(CDTGridLookUpEdit)) || (this._gcMain == null))
                   {
                       int index = bs.Count - 1;
                       if (fAction == FormAction.Edit)
                       {
                           Object value = tmp.EditValue;
                           index = bs.Find(tmp.Properties.ValueMember, value);
                       }
                       tmp.EditValue = (bs.List[index] as DataRowView)[tmp.Properties.ValueMember];
                   }
                   else
                   {
                       object t = (bs.List[bs.Count - 1] as DataRowView)[tmp.Properties.ValueMember];
                       (this._gcMain.MainView as GridView).SetFocusedRowCellValue((this._gcMain.MainView as GridView).FocusedColumn, t);
                       (this._gcMain.MainView as GridView).UpdateCurrentRow();
                       tmp.EditValue = t;
                       this.RIGridLookupEdit_EditValueChanged(tmp, new EventArgs());
                   }

               }
           }
       }

    public void RefreshDataForLookup()
    {
        int i;
        BindingSource bs;
        for (i = 1; i < this._lstData.Count; i++)
        {
            if (!this._lstData[i].FullData)
            {
                this._lstData[i].GetDataForLookup(this._data);
            }
        }
        for (i = 0; i < this._Glist.Count; i++)
        {
            bs = this._Glist[i].glk.Properties.DataSource as BindingSource;
            bs.DataSource = this._lstData[this._Glist[i].dataIndex].DsData.Tables[0];
        }
        for (i = 0; i < this._Rlist.Count; i++)
        {
            bs = this._Rlist[i].rglk.DataSource as BindingSource;
            bs.DataSource = this._lstData[this._Rlist[i].dataIndex].DsData.Tables[0];
        }
    }

    private void RefreshDataForLookup(string controlFrom, bool isDetail)
    {
        string formulaDetail;
        string[] str;
        int i;
        int n;
        List<string> lstStr = new List<string>();
        if (isDetail)
        {
            foreach (DataRow drField in this._data.DsStruct.Tables[1].Rows)
            {
                formulaDetail = drField["FormulaDetail"].ToString();
                if (!(formulaDetail == string.Empty))
                {
                    str = formulaDetail.Split(".".ToCharArray());
                    if (!(controlFrom.ToUpper() != str[0].ToUpper()) || lstStr.Contains(str[0].ToUpper()))
                    {
                        lstStr.Add(drField["FieldName"].ToString().ToUpper());
                    }
                }
            }
            foreach (string s in lstStr)
            {
                i = 0;
                while (i < this._Rlist.Count)
                {
                    if (this._Rlist[i].rglk.Name.ToUpper() == s)
                    {
                        n = this._Rlist[i].dataIndex;
                        this.RefreshLookup(n);
                    }
                    i++;
                }
            }
        }
        lstStr.Clear();
        foreach (DataRow drField in this._data.DsStruct.Tables[0].Rows)
        {
            formulaDetail = drField["FormulaDetail"].ToString();
            if (!(formulaDetail == string.Empty))
            {
                str = formulaDetail.Split(".".ToCharArray());
                if (!(controlFrom.ToUpper() != str[0].ToUpper()) || lstStr.Contains(str[0].ToUpper()))
                {
                    lstStr.Add(drField["FieldName"].ToString().ToUpper());
                }
            }
        }
        foreach (string s in lstStr)
        {
            for (i = 0; i < this._Glist.Count; i++)
            {
                if (this._Glist[i].glk.Name.ToUpper() == s)
                {
                    n = this._Glist[i].dataIndex;
                    this.RefreshLookup(n);
                }
            }
        }
    }

    public void RefreshDataLookupForColChanged()
    {
        int i;
        BindingSource bs;
        for (i = 1; i < this._lstData.Count; i++)
        {
            if (!this._lstData[i].FullData)
            {
                this._lstData[i].GetData();
            }
        }
        for (i = 0; i < this._Glist.Count; i++)
        {
            bs = this._Glist[i].glk.Properties.DataSource as BindingSource;
            bs.DataSource = this._lstData[this._Glist[i].dataIndex].DsData.Tables[0];
        }
        for (i = 0; i < this._Rlist.Count; i++)
        {
            bs = this._Rlist[i].rglk.DataSource as BindingSource;
            bs.DataSource = this._lstData[this._Rlist[i].dataIndex].DsData.Tables[0];
        }
    }

    public void RefreshFormulaDetail()
    {
        for (int i = 0; i < this._Glist.Count; i++)
        {
            CDTGridLookUpEdit tmp = this._Glist[i].glk;
            if (tmp.EditValue != null)
            {
                string value = tmp.EditValue.ToString();
                BindingSource bs = tmp.Properties.DataSource as BindingSource;
                int index = tmp.Properties.GetIndexByKeyValue(value);
                if ((index >= 0) && !(value == string.Empty))
                {
                    DataTable dt = bs.DataSource as DataTable;
                    DataRow drData = dt.Rows[index];
                    if (drData != null)
                    {
                        this._data.SetValuesFromList(tmp.Name, value, drData, true);
                    }
                }
            }
        }
    }

    public void RefreshLookup(int dataIndex)
    {
        if (dataIndex >= 0)
        {
            CDTData data = this._lstData[dataIndex];
           // if (data.FullData) return;
            if ((((data.dataType != DataType.MasterDetail) || !data.DrTableMaster.Table.Columns.Contains("TableName")) || (data.DrTableMaster["TableName"].ToString() != "sysTable")))
            {
                int i;
                BindingSource bs;
                if (!data.FullData)
                    data.GetData();
                for (i = 0; i < this._Glist.Count; i++)
                {
                    if (this._Glist[i].dataIndex == dataIndex)
                    {
                        bs = this._Glist[i].glk.Properties.DataSource as BindingSource;
                        if ((bs.DataSource as DataTable).TableName == data.DsData.Tables[0].TableName)
                        {
                            bs.DataSource = data.DsData.Tables[0];
                        }
                        else
                        {
                            bs.DataSource = data.DsData.Tables[1];
                        }
                    }
                }
                for (i = 0; i < this._Rlist.Count; i++)
                {
                    if (this._Rlist[i].dataIndex == dataIndex)
                    {
                        
                        bs = this._Rlist[i].rglk.DataSource as BindingSource;
                        
                        if (bs.DataSource ==null ||(bs.DataSource as DataTable).TableName == data.DsData.Tables[0].TableName)
                        {
                            bs.DataSource = data.DsData.Tables[0];
                        }
                        else
                        {
                            bs.DataSource = data.DsData.Tables[1];
                        }
                    }
                }
            }
        }
    }
       private void RefreshLookupAllowFull(int dataIndex)
       {
           if (dataIndex >= 0)
           {
               CDTData data = this._lstData[dataIndex];
               if ((((data.dataType != DataType.MasterDetail) || !data.DrTableMaster.Table.Columns.Contains("TableName")) || (data.DrTableMaster["TableName"].ToString() != "sysTable")))//(data.dataType != DataType.MasterDetail) ||
                {
                   int i;
                   BindingSource bs;
                   data.GetData();
                   for (i = 0; i < this._Glist.Count; i++)
                   {
                       if (this._Glist[i].dataIndex == dataIndex)
                       {
                           bs = this._Glist[i].glk.Properties.DataSource as BindingSource;
                           if ((bs.DataSource as DataTable).TableName == data.DsData.Tables[0].TableName)
                           {
                               try
                               {
                                   bs.DataSource = data.DsData.Tables[0];
                               }
                               catch { }
                           }
                           else
                           {
                               bs.DataSource = data.DsData.Tables[1];
                           }
                       }
                   }
                   for (i = 0; i < this._Rlist.Count; i++)
                   {
                       if (this._Rlist[i].dataIndex == dataIndex)
                       {
                           bs = this._Rlist[i].rglk.DataSource as BindingSource;
                           if ((bs.DataSource as DataTable).TableName == data.DsData.Tables[0].TableName)
                           {
                               try
                               {
                                   bs.DataSource = data.DsData.Tables[0];
                               }
                               catch { }
                           }
                           else
                           {
                               bs.DataSource = data.DsData.Tables[1];
                           }
                       }
                   }
               }
           }
       }
    public void RefreshViewForLookup()
    {
        for (int i = 1; i < this._lstData.Count; i++)
        {
            if ((this._lstData[i].DrTable["CollectType"].ToString() == "-1") && this._lstData[i].FullData)
            {
                BindingSource bs;
                this._lstData[i].GetData();
                for (int j = 0; j < this._Glist.Count; j++)
                {
                    if (this._Glist[j].dataIndex == i)
                    {
                        bs = this._Glist[j].glk.Properties.DataSource as BindingSource;
                        bs.DataSource = this._lstData[this._Glist[j].dataIndex].DsData.Tables[0];
                    }
                }
                for (int k = 0; k < this._Rlist.Count; k++)
                {
                    if (this._Rlist[k].dataIndex == i)
                    {
                        bs = this._Rlist[k].rglk.DataSource as BindingSource;
                        bs.DataSource = this._lstData[this._Rlist[k].dataIndex].DsData.Tables[0];
                    }
                }
            }
        }
        
    }
    public void RefreshGridLookupEdit()
    {
        if (this.formAction == FormAction.New || this.formAction == FormAction.Copy)
        {
            for (int j = 0; j < this._Glist.Count; j++)
            {
                if (this._Glist[j].glk.EditValue != null && this._Glist[j].glk.EditValue != DBNull.Value)
                    GridLookupEdit_EditValueChanged(this._Glist[j].glk, new EventArgs());

            }
        }
    }
    private void RItmp_CloseUp(object sender, CloseUpEventArgs e)
       {
           GridLookUpEdit tmp = sender as GridLookUpEdit;
           tmp.EditValue = e.Value;
           //RIGridLookupEdit_EditValueChanged(sender, new EventArgs());
           
           if (tmp.EditValue != null)
               RIGridLookupEdit_Validating(sender, new EventArgs());
           ClearFilter();
           //CDTRepGridLookup ri = tmp.Tag as CDTRepGridLookup;
           //if(ri==null) return;
           //if (ri.isFiltered)
           //{
           //    BindingSource bs = ri.DataSource as BindingSource;
           //    bs.DataSource = ri.Data.DsData.Tables[0];
           //    ri.DataSource = bs;
           //}
       }
    private void RIGridLookupEdit_EditValueChanged(object sender, EventArgs e)
    {
        if (((this._formAction != FormAction.View) && (this._formAction != FormAction.Delete)) && (this._formAction != FormAction.Filter))
        {
            GridLookUpEdit tmp = sender as GridLookUpEdit;
            GridView gv = this._gcMain.Views[0] as GridView;
            if (tmp.EditValue != null)
            {
                string value = tmp.EditValue.ToString();
                if (value != string.Empty)
                {
                    BindingSource bs = tmp.Properties.DataSource as BindingSource;

                    //int index = bs.Position;// 
                    int index = tmp.Properties.GetIndexByKeyValue(value);
                    if (index < 0)
                    {
                        index = bs.Count - 1;
                    }
                    if (tmp.Tag != null)
                    {
                        (tmp.Tag as CDTRepGridLookup).bsCur = index;
                    }
                    DataTable dt = bs.DataSource as DataTable;
                    if (index < 0) return;
                    DataRow drData = dt.Rows[index];
                    DataRowView drDataView = tmp.Properties.GetRowByKeyValue(value) as DataRowView;
                    
                    if (drDataView != null)
                    {
                        drData = drDataView.Row;
                        DataRow drDetail = gv.GetDataRow(gv.FocusedRowHandle);
                        if (this._data.dataType != DataType.MasterDetail)
                        {
                            this._data.SetValuesFromList(tmp.Properties.Name, value, drData, false);
                        }
                        this.RefreshDataForLookup(tmp.Properties.Name, this._data.dataType == DataType.MasterDetail);
                    }
                }
            }
        }
    }

    private void RiGridLookupEdit_KeyDown(object sender, KeyEventArgs e)
    {
        GridLookUpEdit tmp = sender as GridLookUpEdit;
        if ((!tmp.IsPopupOpen && ((tmp.EditValue == null) || (tmp.EditValue.ToString() == string.Empty))) && (e.KeyCode == Keys.Return))
        {
            if (tmp.Properties.AllowNullInput == DefaultBoolean.False)
            {
                tmp.ShowPopup();
                e.Handled = true;
            }
        }
        if (((!tmp.IsPopupOpen && (tmp.EditValue != null)) && (tmp.EditValue.ToString() != string.Empty)) && (e.KeyCode == Keys.Delete))
        {
            tmp.EditValue = null;
        }
    }

    private void RIGridLookupEdit_Popup(object sender, EventArgs e)
    {
        GridLookUpEdit tmp = (GridLookUpEdit) sender;
        CDTRepGridLookup ri=tmp.Tag as CDTRepGridLookup;
        int k = _lstRep.IndexOf(ri) + 1;
       // ri.View.ActiveFilter
        //int n = -1;
        //int i = 0;
        //for (i = 0; i < this._Rlist.Count; i++)
        //{
        //    if (this._Rlist[i].rglk.Name == tmp.Properties.Name)
        //    {
        //        n = this._Rlist[i].dataIndex;
        //        break;
        //    }
        //}
        if (!ri.Data.FullData)
            this.RefreshLookup(ri.DataIndex);
        BindingSource bs = tmp.Properties.DataSource as BindingSource;

        if (tmp.Tag != null)
        {
            bs.Position = int.Parse((tmp.Tag as CDTRepGridLookup).bsCur.ToString());
        }
        setDynFiter(ri);
        
    }

    private void RIGridLookupEdit_Validating(object sender, EventArgs e)
    {
        if (((this._formAction != FormAction.View) && (this._formAction != FormAction.Delete)) && (this._formAction != FormAction.Filter))
        {
            GridLookUpEdit tmp = sender as GridLookUpEdit;
            if (tmp.EditValue != null)
            {
                string value = tmp.EditValue.ToString();
                if (value != string.Empty)
                {
                    BindingSource bs = tmp.Properties.DataSource as BindingSource;
                    //int index = bs.Position;
                    int index = tmp.Properties.GetIndexByKeyValue(value);
                    if (index < 0)
                    {
                        index = bs.Count - 1;
                    }
                    DataTable dt = bs.DataSource as DataTable;
                    if (index < 0) return;
                    DataRow drData ;//= dt.Rows[index];
                    DataRowView drDataView = tmp.Properties.GetRowByKeyValue(value) as DataRowView;
                    
                    if (drDataView != null)
                    {
                        drData = drDataView.Row;
                        GridView gv;
                        CDTRepGridLookup CDTri = tmp.Tag as CDTRepGridLookup;
                        DataTable tbStruct;
                        if (CDTri != null)
                        {
                            gv = CDTri.MainView;
                            tbStruct = CDTri.MainStruct;
                        }
                        else
                        {
                            gv = this._gcMain.Views[0] as GridView;
                            tbStruct = this._data.DsStruct.Tables[1];
                        }
                        DataRow drDetail = gv.GetDataRow(gv.FocusedRowHandle);
                        if (gv.Columns[tmp.Properties.Name] != null)
                        {
                            if (this._data.dataType == DataType.MasterDetail)
                            {
                                this._data.SetValuesFromListDt(drDetail, tmp.Properties.Name, value, drData,tbStruct);
                            }
                        }
                        else
                        {
                            if (_gcDetail != null)
                            {
                                foreach (GridControl gc in this._gcDetail)
                                {
                                    gv = gc.Views[0] as GridView;
                                    drDetail = gv.GetDataRow(gv.FocusedRowHandle);
                                    if (gv.Columns[tmp.Properties.Name] != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (this._data.dataType == DataType.MasterDetail)
                            {
                                this._data.SetValuesFromListDetail(drDetail, tmp.Properties.Name, value, drData);
                            }
                        }
                        if (gv.Columns[tmp.Properties.Name] != null)
                        {
                        }
                    }
                }
            }
        }
    }
    public void setStaticFilter()
       {
           string refFilter;
           foreach (CDTRepGridLookup Ri in _lstRep)
           {
               refFilter = Ri.DymicCondition;
               if (refFilter == null || refFilter == string.Empty || refFilter.Contains("@")) continue;
               Ri.View.OptionsFilter.BeginUpdate();
               try
               {

                   Ri.View.ActiveFilterString = refFilter;
               }
               catch (Exception e) { }
               Ri.ActiveFilter = refFilter;

               Ri.View.ActiveFilterEnabled = true;
               Ri.View.OptionsFilter.EndUpdate();
               Ri.View.RefreshEditor(true);
           }

       }
    public void ClearFilter()
       {
           foreach (CDTGridLookUpEdit Ri in this._glist)
           {
               if (!Ri.isFiltered) continue;
               Ri.Properties.BeginUpdate();
               BindingSource bs =  Ri.Properties.DataSource as BindingSource;
               bs.DataSource = Ri.Data.DsData.Tables[0];
               Ri.Properties.DataSource = bs;
               Ri.Properties.EndUpdate();

           }
           foreach (CDTRepGridLookup Ri in this._rlist)
           {
               if (!Ri.isFiltered) continue;
               Ri.BeginUpdate();
               BindingSource bs = Ri.DataSource as BindingSource;
               bs.DataSource = Ri.Data.DsData.Tables[0];
               Ri.DataSource = bs;
               Ri.EndUpdate();

           }

       }
    private void setDynFilter(CDTGridLookUpEdit Gl)
    {
        string filter = string.Empty;
        if (Gl.DymicCondition == null || Gl.DymicCondition == string.Empty) return;
        filter = Gl.DymicCondition;
        if (_data.DrCurrentMaster == null) return;
        foreach (DataColumn dcMater in _data.DrCurrentMaster.Table.Columns)
        {
            string fieldName = dcMater.ColumnName;
            if (!filter.ToUpper().Contains("@" + fieldName.ToUpper())) continue;
            string value = _data.DrCurrentMaster[fieldName].ToString();
            if (value == null || value == string.Empty) continue;
            if (!(dcMater.DataType == typeof(decimal) || dcMater.DataType == typeof(int)) )
                value = "'" + value + "'";
            if (dcMater.DataType == typeof(bool))
            {
                if (value == "'True'") value = "1";
                else if (value == "'False'") value = "0";
                else value = "null ";
            }
            filter = filter.ToUpper().Replace("@" + fieldName.ToUpper(), value);
        }
        if (filter != string.Empty)
        {
            if (!Gl.Data.FullData) Gl.Data.GetData();
            DataTable dttmp = Gl.Data.DsData.Tables[0].Clone();

            DataRow[] drtmp;
            try
            {
                drtmp = Gl.Data.DsData.Tables[0].Select(filter);
            }
            catch
            {
                drtmp = Gl.Data.DsData.Tables[0].Select("1=0");
            }

            foreach (DataRow dr in drtmp)
                dttmp.ImportRow(dr);
            BindingSource bs = Gl.Properties.DataSource as BindingSource;
            bs.DataSource = dttmp;
            Gl.isFiltered = true;


        }
    }
    private void setDynFiter(CDTRepGridLookup Ri)
       {
           string refFilter;
           string strReplaced;
           string filter=string.Empty;
           if (Ri.DymicCondition == null || Ri.DymicCondition == string.Empty) return;
           filter = Ri.DymicCondition;
           if (_data.DrCurrentMaster == null) return;
           foreach (DataColumn dcMater in _data.DrCurrentMaster.Table.Columns)
           {
               string fieldName = dcMater.ColumnName;
               if (!filter.ToUpper().Contains("@" + fieldName.ToUpper())) continue;
               string value = _data.DrCurrentMaster[fieldName].ToString();
               if (value == null || value == string.Empty) continue;
               if (!(dcMater.DataType == typeof(decimal) || dcMater.DataType == typeof(int)))
                   value = "'" + value + "'";
               filter = filter.ToUpper().Replace("@" + fieldName.ToUpper(), value);
           }
           DataRow drc = (this._gcMain.DefaultView as GridView).GetFocusedDataRow();
           if (drc != null)
           {
               foreach (DataColumn dcDetail in drc.Table.Columns)
               {
                   string fieldName = dcDetail.ColumnName;
                   if (!filter.ToUpper().Contains("@" + fieldName.ToUpper())) continue;
                   string value = drc[fieldName].ToString();
                   if (value == null || value == string.Empty) continue;
                   if (!(dcDetail.DataType == typeof(decimal) || dcDetail.DataType == typeof(int)))
                       value = "'" + value + "'";
                   filter = filter.ToUpper().Replace("@" + fieldName.ToUpper(), value);
               }
           }
           if (filter != string.Empty)
           {
               if (!Ri.Data.FullData) Ri.Data.GetData();
               DataTable dttmp = Ri.Data.DsData.Tables[0].Clone();

               DataRow[] drtmp ;
               try
               {
                   drtmp = Ri.Data.DsData.Tables[0].Select(filter);
               }catch {
                   drtmp = Ri.Data.DsData.Tables[0].Select("1=0");
               }

                   foreach (DataRow dr in drtmp)
                       dttmp.ImportRow(dr);
                   BindingSource bs = Ri.DataSource as BindingSource;
                   bs.DataSource = dttmp;
                   //Ri.DataSource = bs;
                   Ri.isFiltered = true;
               
               
           }
       }

    private void Tab1_GotFocus(object sender, EventArgs e)
    {
        try
        {
            (sender as XtraTabPage).Controls[0].Focus();
        }
        catch
        {
        }
    }

    private void TabDetail_GotFocus(object sender, EventArgs e)
    {
        try
            {
            (sender as XtraTabControl).TabPages[0].Focus();
        }
        catch
        {
        }
    }

    private void TabDetail_SelectedPageChanging(object sender, TabPageChangingEventArgs e)
    {
        if ((e.PrevPage != null) && ((e.PrevPage.TabIndex == 0) && !this.InsertedToDetail))
        {
            DataSet DsTmp = (this.Data as DataMasterDetail).UpdateDetailFromMTDT();
            for (int i = 0; i < this._gcDetail.Count; i++)
            {
                GridControl gr = this._gcDetail[i];
                GridView gv = gr.MainView as GridView;
                foreach (DataRow dr in DsTmp.Tables[i].Rows)
                {
                    gv.AddNewRow();
                    CurrentRowDt drDt = this.Data._lstCurRowDetail[this.Data._lstCurRowDetail.Count - 1];
                    foreach (DataColumn col in DsTmp.Tables[i].Columns)
                    {
                        drDt.RowDetail[col.ColumnName] = dr[col.ColumnName];
                    }
                }
            }
            this.InsertedToDetail = true;
        }
    }
    public void InsertDetailFromMTDT()
       {
           DataSet DsTmp = (this.Data as DataMasterDetail).UpdateDetailFromMTDT();
           this.Data._lstCurRowDetail.Clear();
           for (int i = 0; i < this._gcDetail.Count; i++)
           {
               GridControl gr = this._gcDetail[i];
               GridView gv = gr.MainView as GridView;
               while (gv.RowCount > 1)
               {
                   gv.DeleteRow(0);
               }

               foreach (DataRow dr in DsTmp.Tables[i].Rows)
               {
                   gv.AddNewRow();
                   CurrentRowDt drDt = this.Data._lstCurRowDetail[this.Data._lstCurRowDetail.Count - 1];
                   foreach (DataColumn col in DsTmp.Tables[i].Columns)
                   {
                       drDt.RowDetail[col.ColumnName] = dr[col.ColumnName];
                   }
               }
           }
           this.InsertedToDetail = true;
       }
    private void tmp_RIButton_click(object sender, ButtonPressedEventArgs e)
    {
        if (e.Button.Tag != null)
        {
            CDTRepGridLookup tmp = sender as CDTRepGridLookup;
            CDTData d = e.Button.Tag as CDTData;

            if (e.Button.ToolTip == "F5 - Refresh")
            {
                int n = -1;
                int i = 0;
                for (i = 0; i < this._rlist.Count; i++)
                {
                    if (this._Rlist[i].rglk == tmp)
                    {
                        n = this._Rlist[i].dataIndex;
                        break;
                    }
                }
                this.RefreshLookupAllowFull(n);
                return;
            }
           // if (!d.FullData)
           // {
                //int n = -1;
                //int i = 0;
                //for (i = 0; i < this._Rlist.Count; i++)
                //{
                //    if (this._Rlist[i].rglk == tmp)
                //    {
                //        n = this._Rlist[i].dataIndex;
                //        break;
                //    }
                //}
                this.RefreshLookup(tmp.DataIndex);
           // }
            if (d != null)
            {
                BindingSource bs = tmp.DataSource as BindingSource;
                FrmSingleDt frm = new FrmSingleDt(d, bs);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string tableName;
                    if (tmp.refTable != null)
                    {
                        tableName = tmp.refTable;
                    }
                    else
                    {
                        tableName = tmp.refTable;
                    }
                    foreach (RLookUp_CDTData rgc in this._Rlist)
                    {
                        CDTRepGridLookup rg = rgc.rglk;
                        if ((rg.refTable.ToUpper() == tableName.ToUpper()) && (rg.View.ViewCaption.ToUpper() == d.Condition.ToUpper()))
                        {
                            rg.DataSource = null;
                            rg.DataSource = bs;
                        }
                    }
                    foreach (LookUp_CDTData rgc in this._Glist)
                    {
                        CDTGridLookUpEdit rg = rgc.glk;
                        if ((rg.refTable.ToUpper() == tableName.ToUpper()) && (rg.Condition == d.Condition))
                        {
                            rg.Properties.DataSource = null;
                            rg.Properties.DataSource = bs;
                        }
                    }


                    object t = (bs.List[bs.Count - 1] as DataRowView)[tmp.ValueMember];
                    (this._gcMain.MainView as GridView).SetFocusedRowCellValue((this._gcMain.MainView as GridView).FocusedColumn, t);
                    (this._gcMain.MainView as GridView).UpdateCurrentRow();
                    tmp.GridLookup.EditValue = t;
                    this.RIGridLookupEdit_EditValueChanged(tmp.GridLookup, new EventArgs());
                }
                else
                {
                    tmp.DataSource = null;
                    tmp.DataSource = bs;
                }
            }
        }
    }

    private void VCalEdit_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Return)
        {
            (sender as CalcEdit).ClosePopup();
        }
    }

    private void View_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
    {
        if (e.Info.IsRowIndicator && (e.RowHandle >= 0))
        {
            e.Info.DisplayText = (e.RowHandle + 1).ToString();
        }
    }

    private void View_KeyDown(object sender, KeyEventArgs e)
    {
        GridView tmp = sender as GridView;
        if (tmp.OptionsView.ShowAutoFilterRow && (e.KeyCode == Keys.F5))
        {
            tmp.FocusedRowHandle = -999997;
            tmp.FocusedColumn = tmp.VisibleColumns[1];
            tmp.ShowEditor();
        }
    }

    // Properties
    public BindingSource bindingSource
    {
        get
        {
            return this._bindingSource;
        }
        set
        {
            this._bindingSource = value;
        }
    }

    public CDTData Data
    {
        get
        {
            return this._data;
        }
    }

    public BaseEdit FirstControl
    {
        get
        {
            return this._firstControl;
        }
        set
        {
            this._firstControl = value;
        }
    }

    public FormAction formAction
    {
        get
        {
            return this._formAction;
        }
        set
        {
            this._formAction = value;
        }
    }

    public List<CDTRepGridLookup> rlist
    {
        get
        {
            return this._rlist;
        }
        set
        {
            this._rlist = value;
            this.RefreshDataForLookup();
        }
    }

    // Nested Types

    private struct LookUp_CDTData
    {
        public CDTGridLookUpEdit glk;
        public int dataIndex;
        public LookUp_CDTData(CDTGridLookUpEdit g, int i)
        {
            this.glk = g;
            this.dataIndex = i;
        }
    }

    private struct RLookUp_CDTData
    {
        public CDTRepGridLookup rglk;
        public int dataIndex;
        public RLookUp_CDTData(CDTRepGridLookup r, int i)
        {
            this.rglk = r;
            this.dataIndex = i;
        }
    }
}


}
