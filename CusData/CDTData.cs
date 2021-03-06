using System;
using System.Data;
using System.Collections.Generic;
using CDTDatabase;
using CDTControl;
using CDTLib;
using System.Windows.Forms;
namespace CusData
{
    public abstract class CDTData
    {
        protected DataType _dataType;
        protected Database _dbStruct = Database.NewStructDatabase();
        private Database _dbData = Database.NewDataDatabase();
        protected DataRow _drTable;// Thông tin systable
        protected DataRow _drTableMaster;//Thông tin systable --Dòng master
        private DataSet _dsData; //Data hiện tại
        protected DataSet _dsDataTmp;//Data cũ
        protected DataSet _dsStruct = new DataSet();
        protected string _condition = string.Empty;
        protected string _DynCondition = string.Empty;
        protected string _conditionMaster = string.Empty;
        protected DataRow _drCurrentMaster; //Dòng mater hiện tại
        protected List<DataRow> _lstDrCurrentDetails = new List<DataRow>(); //Các dòng detail tương ứng
        protected bool _dataChanged = false;
        internal FormulaCaculator _formulaCaculator;
        internal DataMasterDetailPrint _printData;
        internal AutoIncrementValues _autoIncreValues;
        internal DataTransfer _dataTransfer;
        internal Customize _customize;
        internal bool fullData = false;
        protected string _sInsert = string.Empty;
        protected string _sUpdate = string.Empty;
        protected string _sDelete = string.Empty;
        protected string _sUpdateImage = string.Empty;
        protected string _sInsertDetail = string.Empty;
        protected string _sUpdateDetail = string.Empty;
        protected string _sDeleteDetail = string.Empty;
        protected string _sUpdateWs = string.Empty;
        public event EventHandler SetDetailValue;
        protected struct SqlField
        {
            public string FieldName;
            public SqlDbType DbType;
            public SqlField(string fieldName, SqlDbType dbType)
            {
                FieldName = fieldName;
                DbType = dbType;
            }
        }
        protected SqlField PkMaster ;
        protected List<SqlField> _vInsert = new List<SqlField>();
        protected List<SqlField> _vUpdate = new List<SqlField>();
        protected List<SqlField> _vUpdateImage = new List<SqlField>();
        protected List<SqlField> _vDelete = new List<SqlField>();
        protected List<SqlField> _vInsertDetail = new List<SqlField>();
        protected List<SqlField> _vUpdateDetail = new List<SqlField>();
        protected List<SqlField> _vDeleteDetail = new List<SqlField>();
        protected bool _identityPk = false;
        protected bool _identityPkDt = false;

        public bool FullData
        {
            get { return fullData; }
            set { fullData = value; }
        }

        public DataType dataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public bool DataChanged
        {
            get { return _dataChanged; }
            set { _dataChanged = value; }
        }

        public Database DbData
        {
            get { return _dbData; }
            set { _dbData = value; }
        }
        public Database dbStruct
        {
            get { return _dbStruct; }
            set { _dbStruct = value; }
        }
        public DataRow DrCurrentMaster
        {
            get { return _drCurrentMaster; }
            set 
            {
                _drCurrentMaster = value;
                if (_formulaCaculator != null)
                    _formulaCaculator.DrCurrentMaster = _drCurrentMaster;
            }
        }

        public List<DataRow> LstDrCurrentDetails
        {
            get { return _lstDrCurrentDetails; }
            set 
            {
                _lstDrCurrentDetails = value;
                if (_formulaCaculator != null)
                    _formulaCaculator.LstDrCurrentDetails = _lstDrCurrentDetails;
            }
        }

        public string ConditionMaster
        {
            get { return _conditionMaster; }
            set
            {
                _conditionMaster = value; 
            }
        }

        public string Condition
        {
            get { return _condition; }
            set 
            {
                _condition = value;
            }
        }
        public string DynCondition
        {
            get { return _DynCondition; }
            set
            {
                _DynCondition = value;
            }
        }
        public DataSet DsStruct
        {
            get { return _dsStruct; }
            set
            {
                _dsStruct = value;
                GetPkMaster();
            }
        }

        public DataSet DsData
        {
            get { return _dsData; }
            set 
            { 
                _dsData = value;
                if (_dsData != null)
                {
                    _dsData.Tables[0].TableNewRow += new DataTableNewRowEventHandler(DataTable0_TableNewRow);
                    _dsData.Tables[0].RowDeleted += new DataRowChangeEventHandler(DataTable0_RowDeleted);
                    _dsData.Tables[0].RowChanged += new DataRowChangeEventHandler(DataTable0_RowChanged);
                    _dsData.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(DataTable0_ColChanged);
                    if (_dataType != DataType.Single && _dsData.Tables.Count > 1)
                    {
                        _dsData.Tables[1].TableNewRow += new DataTableNewRowEventHandler(DataTable1_TableNewRow);
                        _dsData.Tables[1].RowDeleted += new DataRowChangeEventHandler(DataTable1_RowDeleted);
                        _dsData.Tables[1].RowChanged += new DataRowChangeEventHandler(DataTable1_RowChanged);
                        
                    }
                    if (_dataType != DataType.Report)
                        _formulaCaculator.DsData = _dsData;
                }
            }
        }


        public virtual void DataTable0_ColChanged(object sender, DataColumnChangeEventArgs e)
        {
            _dataChanged = true;
        }
        public virtual void DataTable1_ColChanged(object sender, DataColumnChangeEventArgs e)
        {
            _dataChanged = true;
        }
        void DataTable0_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            _dataChanged = true;
        }

        void DataTable1_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            _dataChanged = true;
        }

        void DataTable1_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            if (!_lstDrCurrentDetails.Contains(e.Row))
                _lstDrCurrentDetails.Add(e.Row);
            _dataChanged = true;
        }

        void DataTable0_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            _drCurrentMaster = e.Row;
            _dataChanged = true;
        }

        void DataTable0_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            if (_dataType != DataType.Report)
            {
                string sysTableID = _drTable["SysTableID"].ToString();
                if (_autoIncreValues == null)
                    _autoIncreValues = new AutoIncrementValues(sysTableID, _dsStruct.Tables[0]);
                _autoIncreValues._dbStruct = this._dbStruct;
                _autoIncreValues.MakeNewStruct();
                if (_drTable["Type"].ToString() == "1" || _drTable["Type"].ToString() == "4") //nhap lieu tren luoi
                    _drCurrentMaster = e.Row;
            }
            SetDefaultValues(_dsStruct.Tables[0], e.Row);
            _dataChanged = true;
        }

        void DataTable1_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            
           // if (e.Row.RowState == DataRowState.Detached) (sender as DataTable).Rows.Add(e.Row);
            //Không thể add row được mà phải để detached vì lúc này ForcusRow của Gridview <0. (khi lấy row có forcus<0 thì nó sẽ lấy rowstase=Detached)
            SetDefaultValues(_dsStruct.Tables[1], e.Row);
            _dataChanged = true;
           
            this._lstDrCurrentDetails.Add(e.Row);
        }

        public DataRow DrTable
        {
            get { return _drTable; }
            set { _drTable = value; }
        }

        public DataRow DrTableMaster
        {
            get { return _drTableMaster; }
            set { _drTableMaster = value; }
        }

        public virtual void GetInfor(string sysTableID)
        {
            DataTable dt = _dbStruct.GetDataTable("select * from sysTable t, sysUserTable ut where t.sysTableID *= ut.sysTableID and t.sysTableID = " + sysTableID);
            if (dt != null && dt.Rows.Count > 0)
            {
                _drTable = dt.Rows[0];
                InsertHistory();
            }
            else
            {   //trường hợp dữ liệu nằm ở CDT
                dt = _dbStruct.GetDataTable("select * from sysTable t, sysUserTable ut where t.sysTableID *= ut.sysTableID and t.sysTableID = '" + sysTableID + "' and t.sysPackageID = 5");
                if (dt != null && dt.Rows.Count > 0)
                {
                    _drTable = dt.Rows[0];
                    _dbData = _dbStruct;
                    InsertHistory();
                }
            }
        }

        public virtual void GetInfor(string TableName, string sysPackageID)
        {
            DataTable dt = _dbStruct.GetDataTable("select * from sysTable t, sysUserTable ut where t.sysTableID *= ut.sysTableID and t.TableName = '" + TableName + "' and t.sysPackageID = " + sysPackageID);
            if (dt != null && dt.Rows.Count > 0)
            {
                _drTable = dt.Rows[0];
                InsertHistory();
            }
            else
            {   //trường hợp dữ liệu nằm ở CDT
                dt = _dbStruct.GetDataTable("select * from sysTable t, sysUserTable ut where t.sysTableID *= ut.sysTableID and t.TableName = '" + TableName + "' and t.sysPackageID = 5");
                if (dt != null && dt.Rows.Count > 0)
                {
                    _drTable = dt.Rows[0];
                    _dbData = _dbStruct;
                    InsertHistory();
                }
            }
            
        }

        public virtual void GetInfor(DataRow drTable)
        {
            _drTable = drTable;
            //trường hợp dữ liệu nằm ở CDT
            if (_drTable.Table.Columns.Contains("sysPackageID2") && _drTable["sysPackageID2"].ToString() == string.Empty)
                _dbData = _dbStruct;
            InsertHistory();
        }

        private void GetPkMaster()
        {
            foreach (DataRow drField in DsStruct.Tables[0].Rows)
            {
                string fieldName = drField["FieldName"].ToString();
                int type = Int32.Parse(drField["Type"].ToString());
                if (type == 0 || type == 3 || type == 6)
                {
                    PkMaster = new SqlField(fieldName,GetDbType(type));
                    break;
                }
            }
        }

        private SqlDbType GetDbType(int fType)
        {
            SqlDbType tmp = SqlDbType.VarChar;
            switch (fType)
            {
                case 0:
                case 1:
                    tmp = SqlDbType.VarChar;
                    break;
                case 2:
                    tmp = SqlDbType.NVarChar;
                    break;
                case 3:
                case 4:
                case 5:
                    tmp = SqlDbType.Int;
                    break;
                case 6:
                case 7:
                    tmp = SqlDbType.UniqueIdentifier;
                    break;
                case 8:
                    tmp = SqlDbType.Decimal;
                    break;
                case 9:
                case 11:
                case 14:
                    tmp = SqlDbType.DateTime;
                    break;
                case 10:
                    tmp = SqlDbType.Bit;
                    break;
                case 12:
                    tmp = SqlDbType.Image;
                    break;
                case 13:
                    tmp = SqlDbType.NText;
                    break;
            }
            return tmp;
        }
       
        protected void GenSqlString()
        {
            string tableName = _dataType == DataType.MasterDetail ? _drTableMaster["TableName"].ToString() : _drTable["TableName"].ToString();
            _sInsert = "insert into " + tableName + "(";
            _sUpdate = "update " + tableName + " set ";
            _sDelete = "delete from " + tableName;
            _sUpdateImage = "update " + tableName + " set ";
            _sUpdateWs = "update " + tableName + " set ws='" + Config.GetValue("sysUserID").ToString() + "'";
            string condition = string.Empty;
            string tmp = " values(";
            foreach (DataRow drField in _dsStruct.Tables[0].Rows)
            {
                string fieldName = drField["FieldName"].ToString();
                int type = Int32.Parse(drField["Type"].ToString());
                if (type == 0  || type == 6)
                {
                    condition = " where " + fieldName + " = @" + fieldName;
                    _vUpdate.Add(new SqlField(fieldName, GetDbType(type)));
                    _vDelete.Add(new SqlField(fieldName, GetDbType(type)));
                   
                }
                
                if (type == 3)
                {
                    _identityPk = true;
                    condition = " where " + fieldName + " = @" + fieldName;
                    _vUpdate.Add(new SqlField(fieldName, GetDbType(type)));
                    _vDelete.Add(new SqlField(fieldName, GetDbType(type)));
                    continue;
                }
                if (type == 12)//Image
                {
                    _vUpdateImage.Add(new SqlField(fieldName, GetDbType(type)));
                    _sUpdateImage += fieldName + "=@" + fieldName + ",";
                    continue;
                }
                else
                {
                    if (drField["Editable"].ToString() == "True" && !(type == 0 || type == 6|| type==3))
                    {
                        _vUpdate.Add(new SqlField(fieldName, GetDbType(type)));  
                        _sUpdate += fieldName + " = @" + fieldName + ",";
                    }
                    _sInsert += fieldName + ",";
                    tmp += "@" + fieldName + ",";
                    _vInsert.Add(new SqlField(fieldName, GetDbType(type)));
                    
                }
            }
            _sInsert = _sInsert.Remove(_sInsert.Length - 1) + ")" + tmp.Remove(tmp.Length - 1) + ")";
            _sUpdate = _sUpdate.Remove(_sUpdate.Length - 1) + condition;
            _sUpdateImage = _sUpdateImage.Remove(_sUpdateImage.Length - 1);
            _sDelete = _sDelete + condition;
            if (_dataType == DataType.MasterDetail)
            {
                _sInsertDetail = "insert into " + _drTable["TableName"].ToString() + "(";
                _sUpdateDetail = "update " + _drTable["TableName"].ToString() + " set ";
                _sDeleteDetail = "delete from " + _drTable["TableName"].ToString();
                condition = string.Empty;
                tmp = " values(";
                foreach (DataRow drField in _dsStruct.Tables[1].Rows)
                {
                    string fieldName = drField["FieldName"].ToString();
                    int type = Int32.Parse(drField["Type"].ToString());
                    if (type == 0 || type == 3 || type == 6)
                    {
                        condition = " where " + fieldName + " = @" + fieldName;
                        _vDeleteDetail.Add(new SqlField(fieldName, GetDbType(type)));
                    }
                    _vUpdateDetail.Add(new SqlField(fieldName, GetDbType(type)));
                    if (type == 3)
                    {
                        _identityPkDt = true;
                        continue;
                    }
                    _sInsertDetail += fieldName + ",";
                    tmp += "@" + fieldName + ",";
                    _vInsertDetail.Add(new SqlField(fieldName, GetDbType(type)));
                    _sUpdateDetail += fieldName + " = @" + fieldName + ",";
                }
                _sInsertDetail = _sInsertDetail.Remove(_sInsertDetail.Length - 1) + ")" + tmp.Remove(tmp.Length - 1) + ")";
                _sUpdateDetail = _sUpdateDetail.Remove(_sUpdateDetail.Length - 1) + condition;
                _sDeleteDetail = _sDeleteDetail + condition;
            }
        }

        protected bool Update(DataRow drData)
        {
            if (_sInsert == string.Empty)
                GenSqlString();
            List<SqlField> tmp = new List<SqlField>();
            List<string> paraNames = new List<string>();
            List<object> paraValues = new List<object>();
            List<SqlDbType> paraTypes = new List<SqlDbType>();
            string sql = string.Empty;
            bool updateIdentity = false, isDelete = false;
            switch (drData.RowState)
            {
                case DataRowState.Added:
                    if (_identityPk)
                        updateIdentity = true;
                    tmp = _vInsert;
                    sql = _sInsert;
                    break;
                case DataRowState.Modified:
                    tmp = _vUpdate;
                    sql = _sUpdate;
                    break;
                case DataRowState.Deleted:
                    tmp = _vDelete;
                    sql = _sDelete;
                    drData.RejectChanges();
                    isDelete = true;
                    break;
            }
            foreach (SqlField sqlField in tmp)
            {
                string fieldName = sqlField.FieldName;
                paraNames.Add(fieldName);
                if (drData[fieldName].ToString() != string.Empty)
                    paraValues.Add(drData[fieldName]);
                else
                    paraValues.Add(DBNull.Value);
                paraTypes.Add(sqlField.DbType);
            }
            //thêm vào phần ws
            bool updateWsCompleted = true;
            
            if (isDelete)
                drData.Delete();
            if (sql == string.Empty)
                return true;
            bool result = _dbData.UpdateData(sql, paraNames.ToArray(), paraValues.ToArray(), paraTypes.ToArray());

            object o;
            string pk=string.Empty;
            pk = _dataType == DataType.MasterDetail ? _drTableMaster["Pk"].ToString() : _drTable["Pk"].ToString();
            if (result && updateIdentity)
            {
                o = _dbData.GetValue("select @@identity");
                if (o != null)
                    drData[pk] = o;
            }
            //thêm vào phần update ID 
            if (_drCurrentMaster != null)
            {
                if (_dataType == DataType.MasterDetail && _drCurrentMaster.Table.Columns.Contains("ws") && (drData.RowState == DataRowState.Added || drData.RowState == DataRowState.Modified))
                {
                    sql = _sUpdateWs + " where " + pk + "='" + drData[pk].ToString() + "'";
                    updateWsCompleted = _dbData.UpdateByNonQuery(sql);
                }
                if (_dataType == DataType.Single && _drCurrentMaster.Table.Columns.Contains("ws") && drData.RowState == DataRowState.Added)
                {
                    sql = "update " + _drTable["TableName"].ToString() + " set ws='" + Config.GetValue("sysUserID").ToString() + "_' where " + pk + "='" + drData[pk].ToString() + "'";
                    updateWsCompleted = _dbData.UpdateByNonQuery(sql);
                }
            }
            result = result && updateWsCompleted;
            //Them vao phan update image
            if (result && _vUpdateImage.Count>0  && !isDelete)
            {
                string exsql=string.Empty;
                if (drData[pk].GetType() == typeof(int))
                {
                    exsql = "";
                }
                else
                {
                    exsql = "'";
                }
                if (drData.RowState == DataRowState.Added || drData.RowState == DataRowState.Modified)
                {
                    sql = _sUpdateImage + " where " + pk + "=" + exsql + drData[pk].ToString() + exsql;                    
                }
                
                List<object> pImValue=new List<object>();
                List<SqlDbType> pImType = new List<SqlDbType>();
                List<string> pImName = new List<string>();
                foreach (SqlField sqlField in _vUpdateImage)
                {
                    string fieldName = sqlField.FieldName;
                    pImName.Add(fieldName);
                    if (drData[fieldName].ToString() != string.Empty)
                        pImValue.Add(drData[fieldName]);
                    else
                        pImValue.Add(DBNull.Value);
                    pImType.Add(sqlField.DbType);
                }
                result = _dbData.UpdateData(sql, pImName.ToArray(), pImValue.ToArray(), pImType.ToArray());
                
            }
            return result;
        }

        private void InsertHistory()
        {
            if (!_drTable.Table.Columns.Contains("sysMenuID"))
                return;
            string sysMenuID = _drTable["sysMenuID"].ToString();
            string action = "Xem";
            SysHistory sh = new SysHistory();
            sh.InsertHistory(sysMenuID, action, string.Empty, string.Empty);
        }

        protected virtual string GetContentForHistory(DataSet dsDataCopy, ref string pkValue)
        {
            string s = string.Empty;
            DataView dvData = new DataView(dsDataCopy.Tables[0]);
            dvData.RowStateFilter = DataViewRowState.ModifiedOriginal | DataViewRowState.Deleted;
            if (dvData.Count > 0)
            {
                string pk = _dataType == DataType.MasterDetail ? _drTableMaster["Pk"].ToString() : _drTable["Pk"].ToString();
                pkValue = dvData[0][pk].ToString();
                foreach (DataRow drField in _dsStruct.Tables[0].Rows)
                {
                    int fType = Int32.Parse(drField["Type"].ToString());
                    if (fType == 3 || fType == 4 || fType == 6 || fType == 7 || fType == 12 || fType == 13)
                        continue;
                    string fieldName = drField["FieldName"].ToString();
                    string fieldValue = dvData[0][fieldName].ToString();
                    if (dvData[0].Row.RowState == DataRowState.Modified && dvData[0].Row[fieldName].ToString() == fieldValue)
                        continue;
                    string labelName = drField["LabelName"].ToString();
                    s += labelName + ":" + fieldValue + "; ";
                }
            }
            if (_dataType == DataType.Single)
                return s;
            dvData = new DataView(dsDataCopy.Tables[1]);
            dvData.RowStateFilter = DataViewRowState.ModifiedOriginal;
            foreach (DataRowView drDetail in dvData)
            {
                s += "\n";
                foreach (DataRow drField in _dsStruct.Tables[1].Rows)
                {
                    int fType = Int32.Parse(drField["Type"].ToString());
                    if (fType == 3 || fType == 4 || fType == 6 || fType == 7)
                        continue;
                    string fieldName = drField["FieldName"].ToString();
                    string fieldValue = drDetail[fieldName].ToString();
                    if (drDetail.Row.RowState == DataRowState.Modified && drDetail.Row[fieldName].ToString() == fieldValue)
                        continue;
                    string labelName = drField["LabelName"].ToString();
                    s += labelName + ":" + fieldValue + "; ";
                }
            }
            return s;
        }

        protected void InsertHistory(DataAction dataAction, DataSet dsDataCopy)
        {
            if (!_drTable.Table.Columns.Contains("sysMenuID"))
                return;
            string sysMenuID = _drTable["sysMenuID"].ToString();
            string action = string.Empty, content = string.Empty, pkValue = string.Empty;
            switch (dataAction)
            {
                case DataAction.Insert:
                    action = "Mới";
                    string pk = _dataType == DataType.MasterDetail ? _drTableMaster["Pk"].ToString() : _drTable["Pk"].ToString();
                    pkValue = _drCurrentMaster != null ? _drCurrentMaster[pk].ToString() : string.Empty;
                    break;
                case DataAction.Update:
                    action = "Sửa";
                    content = GetContentForHistory(dsDataCopy, ref pkValue);
                    break;
                case DataAction.Delete:
                    action = "Xóa";
                    content = GetContentForHistory(dsDataCopy, ref pkValue);
                    break;
                case DataAction.IUD:
                    action = "Xem";
                    content = GetContentForHistory(dsDataCopy, ref pkValue);
                    break;
            }
            SysHistory sh = new SysHistory();
            sh.InsertHistory(sysMenuID, action, pkValue, content);
        } 

        protected void ConditionForPackage()
        {
            object o = Config.GetValue("curPackageID");
            string curPackageID;
            if (o == null || o.ToString().Trim() == string.Empty)
                return;
            else
                curPackageID = o.ToString();
            if (_dataType == DataType.MasterDetail)
            {
                string and = _conditionMaster == string.Empty ? string.Empty : " and ";
                string tableName = _drTableMaster["TableName"].ToString().ToUpper();
                if (tableName == "SYSTABLE")
                    _conditionMaster += and + "sysPackageID = " + curPackageID;
            }
            else
            {
                string and = _condition == string.Empty ? string.Empty : " and ";
                string tableName = _drTable["TableName"].ToString().ToUpper();
                if (tableName == "SYSTABLE" || tableName == "SYSMENU" || tableName == "SYSREPORT")
                    _condition += and + "sysPackageID = " + curPackageID;

                if (tableName == "SYSFIELD")
                    _condition += and + "sysTableID in (select sysTableID from sysTable where sysPackageID = " + curPackageID + ")";
                if (_dataType == DataType.Detail)
                {
                    and = _conditionMaster == string.Empty ? string.Empty : " and ";
                    tableName = _drTableMaster["TableName"].ToString().ToUpper();
                    if (tableName == "SYSTABLE" || tableName == "SYSMENU" || tableName == "SYSREPORT")
                        _conditionMaster += and + "sysPackageID = " + curPackageID;

                    if (tableName == "SYSDATACONFIG")
                        _condition += and + "sysTableID in (select sysTableID from sysTable where sysPackageID = " + curPackageID + ")";
                }
            }
        }

        public virtual void GetStruct()
        {
            string sysTableID = _drTable["SysTableID"].ToString();
            string queryString = "select * from sysField f, sysUserField uf where f.sysFieldID *= uf.sysFieldID " +
                " and f.sysTableID = " + sysTableID + " order by TabIndex";
            DataTable dtStruct = _dbStruct.GetDataTable(queryString);
            if (dtStruct != null)
            {
                _dsStruct.Tables.Add(dtStruct);
                GetPkMaster();
            }
        }

        protected void TransferData(DataAction dataAction, int index)
        {
            if (_drTable["TableName"].ToString() == "sysDataConfig")
                return;
            List<DataRow> drDetails = new List<DataRow>();
            string mtTableID, pk;
            if (_dataType == DataType.MasterDetail)
            {
                mtTableID = _drTableMaster["sysTableID"].ToString();
                pk = _drTableMaster["Pk"].ToString();
            }
            else
            {
                mtTableID = _drTable["sysTableID"].ToString();
                pk = _drTable["Pk"].ToString();
            }
            if (_dataTransfer == null)
                _dataTransfer = new DataTransfer(_dbData, mtTableID, pk);
            
            string PkValue;
            if (dataAction == DataAction.Delete)
                PkValue = _dsDataTmp.Tables[0].Rows[index][pk].ToString();
            else
                PkValue = dataType == DataType.MasterDetail ? _drCurrentMaster[pk].ToString() : _dsData.Tables[0].Rows[index][pk].ToString();
            bool masterEdit = false;
            if (_dataType == DataType.MasterDetail)
            {
                DataView dvMaster = new DataView(_dsData.Tables[0]);
                dvMaster.RowStateFilter = DataViewRowState.ModifiedCurrent;
                masterEdit = dvMaster.Count > 0;
                DataView dv = new DataView(_dsData.Tables[1]);
                dv.RowFilter = pk + " = '" + PkValue + "'";
                foreach (DataRowView dr in dv)
                    if (dr.Row.RowState != DataRowState.Unchanged)
                        drDetails.Add(dr.Row);
                    else
                        if (dvMaster.Count > 0)
                            drDetails.Add(dr.Row);
                //dong xoa lay rieng
                dv.RowFilter = string.Empty;
                dv.RowStateFilter = DataViewRowState.Deleted;
                foreach (DataRowView dr in dv)
                    if (dr.Row.RowState != DataRowState.Unchanged)
                        drDetails.Add(dr.Row);
            }
            _dataTransfer.Transfer(dataAction, PkValue, drDetails, masterEdit);
        }

        public abstract void GetData();
        public abstract bool UpdateData(DataAction dataAction);
        public abstract DataTable GetDataForPrint(int index);

        public bool UpdateData()
        {
            if (!_dataChanged)
                return true;
            _dbData.BeginMultiTrans();
            DataAction da = DataAction.IUD;
            if (!_customize.BeforeUpdate(-1, _dsData))
            {
                CancelUpdate();
                _dbData.RollbackMultiTrans();
                return false;
            }
            DataSet dsDataCopy = _dsData.Copy();
            bool success = false;

            DataView dv ;

                dv = _dsData.Tables[0].DefaultView;

            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent | DataViewRowState.Deleted|DataViewRowState.Unchanged;
            foreach (DataRowView drvData in dv)
            {
                if (drvData.Row.RowState == DataRowState.Unchanged) continue;
                success = Update(drvData.Row);
                if (success)
                {
                    switch (drvData.Row.RowState)
                    {
                        case DataRowState.Added:
                            da = DataAction.Insert;
                            break;
                        case DataRowState.Deleted:
                            da = DataAction.Delete;
                            break;
                        case DataRowState.Modified:
                            da = DataAction.Update;
                            break;
                    }
                    int i = _dsData.Tables[0].Rows.IndexOf(drvData.Row);
                    TransferData(da, i);
                }
            }
            dv.RowStateFilter = DataViewRowState.CurrentRows;
            bool isError = _dbData.HasErrors;
            if (!isError)
            {
                
                _dsData.AcceptChanges();
                _dsDataTmp = _dsData.Copy();
                _customize.AfterUpdate();
                _dataChanged = false;
            }
            else
                CancelUpdate();
            if (_dbData.HasErrors)
                _dbData.RollbackMultiTrans();
            else
                _dbData.EndMultiTrans();
            if (!isError)
            {
                InsertHistory(da, dsDataCopy);
            }
            return (!isError);
        }

        public void CancelUpdate()
        {
            if (!_dataChanged)
                return;
            DsData = _dsDataTmp.Copy();
        }

        public void CloneData()
        {
            DataRow drMasterDes = _dsData.Tables[0].NewRow();
            _formulaCaculator.Active = false;
            drMasterDes.ItemArray = (object[])_drCurrentMaster.ItemArray.Clone();
            string pkMaster = _dataType == DataType.MasterDetail ? _drTableMaster["Pk"].ToString() : _drTable["Pk"].ToString();
            Guid id = Guid.NewGuid();
            if (drMasterDes[pkMaster].GetType() == typeof(Guid))
                drMasterDes[pkMaster] = id;
            //else
            //    drMasterDes[pkMaster] = DBNull.Value;
            _dsData.Tables[0].Rows.Add(drMasterDes);
            DrCurrentMaster = drMasterDes;

            if (_dataType != DataType.MasterDetail)
            {
                _formulaCaculator.Active = true;
                return;
            }
            DataRow[] arrDrCurrentDetails = new DataRow[_lstDrCurrentDetails.Count];
            _lstDrCurrentDetails.CopyTo(arrDrCurrentDetails);
            _lstDrCurrentDetails.Clear();
            for (int i = 0; i < arrDrCurrentDetails.Length; i++)
            {
                DataRow drDetailSource = arrDrCurrentDetails[i];
                DataRow drDetailDes = _dsData.Tables[1].NewRow();
                _formulaCaculator.Active = false;
                drDetailDes.ItemArray = (object[])drDetailSource.ItemArray.Clone();
                string pkDetail = _drTable["Pk"].ToString();
                if (drDetailDes[pkDetail].GetType() == typeof(Guid))
                    drDetailDes[pkDetail] = Guid.NewGuid();
                else
                    drDetailDes[pkDetail] = DBNull.Value;

                if (drMasterDes[pkMaster].GetType() == typeof(Guid))
                    drDetailDes[pkMaster] = id;
                else
                    drDetailDes[pkMaster] = DBNull.Value;
                if (drDetailDes.RowState == DataRowState.Detached)
                    _dsData.Tables[1].Rows.Add(drDetailDes);
            }
            LstDrCurrentDetails = _lstDrCurrentDetails;
            _formulaCaculator.Active = true;

        }

        public DataTable GetRelativeFunction()
        {
            string sysPackageID = Config.GetValue("sysPackageID").ToString();
            string s = "select * from sysTable where sysPackageID = " + sysPackageID + " and MasterTable like '" + DrTable["TableName"].ToString() + "' order by DienGiai";
            return (_dbStruct.GetDataTable(s));
        }

        public DataTable GetRelativeData()
        {
            string sysPackageID = Config.GetValue("sysPackageID").ToString();
            string s = "select * from sysTable t, sysField f where t.sysTableID = f.sysTableID and t.sysPackageID = " + sysPackageID + " and f.refTable like '" + DrTable["TableName"].ToString() + "' order by DienGiai";
            return (_dbStruct.GetDataTable(s));
        }

        protected bool IsUnique(DataAction dataAction, string value, string fieldName, string tableName, string pk, string pkValue)
        {
            string sql = "select " + fieldName + " from " + tableName + " where " + fieldName + " = '" + value + "'";
            if (dataAction == DataAction.Update)
                sql += " and " + pk + " <> '" + pkValue + "'";
            DataTable dtData = _dbData.GetDataTable(sql);
            if (dtData == null)
                return true;
            return (dtData.Rows.Count == 0);
        }

        /// <summary>
        /// Hàm kiểm tra ràng buộc dữ liệu trước khi lưu
        /// </summary>
        public virtual void CheckRules(DataAction dataAction)
        {
            if (_drCurrentMaster.RowState == DataRowState.Deleted)
                return;
            foreach (DataRow drField in _dsStruct.Tables[0].Rows)
            {
                if (!Boolean.Parse(drField["Visible"].ToString()))
                    continue;
                string fieldName = drField["FieldName"].ToString();
                int pType = Int32.Parse(drField["Type"].ToString());
                if (pType == 3 || pType == 6)
                    continue;
                string fieldValue = _drCurrentMaster[fieldName].ToString();
                if (!Boolean.Parse(drField["AllowNull"].ToString()))
                {
                    if (fieldValue == string.Empty)
                        _drCurrentMaster.SetColumnError(fieldName, "Phải nhập");
                    else
                        _drCurrentMaster.SetColumnError(fieldName, string.Empty);
                }
                if (fieldValue == string.Empty)
                    continue;
                if (Boolean.Parse(drField["IsUnique"].ToString()))
                {
                    string tableName = _dataType == DataType.MasterDetail ? _drTableMaster["TableName"].ToString() : _drTable["TableName"].ToString();
                    string pk = _dataType == DataType.MasterDetail ? _drTableMaster["Pk"].ToString() : _drTable["Pk"].ToString();
                    string pkValue = _drCurrentMaster[pk].ToString();
                    if (IsUnique(dataAction, fieldValue, fieldName, tableName, pk, pkValue))
                    {
                        _drCurrentMaster.SetColumnError(fieldName, string.Empty);
                    }
                    else
                    {
                        string editMask = drField["EditMask"].ToString();
                        if ((pType == 0 || pType == 2)&& dataAction==DataAction.Insert && editMask != string.Empty)
                        {
                            if (drField.Table.Columns.Contains("AutoCreate"))
                            {
                                if (Boolean.Parse(drField["AutoCreate"].ToString()))
                                {
                                    _autoIncreValues.MakeNewStruct();
                                    _drCurrentMaster[fieldName] = drField["DefaultValue"].ToString();
                                }
                                else
                                {
                                    _drCurrentMaster.SetColumnError(fieldName, "Đã có số liệu trùng");
                                }
                            }
                            else
                            {
                                _drCurrentMaster.SetColumnError(fieldName, "Đã có số liệu trùng");
                            }
                        }
                        else
                        {
                            _drCurrentMaster.SetColumnError(fieldName, "Đã có số liệu trùng");
                        }
                    }
                }
                int value = 0;
                if (!Int32.TryParse(_drCurrentMaster[fieldName].ToString(), out value))
                    continue;
                if (drField["MinValue"].ToString() != string.Empty)
                {
                    int minValue = Int32.Parse(drField["MinValue"].ToString());
                    if (minValue > value)
                    {
                        _drCurrentMaster.SetColumnError(fieldName, "Phải lớn hơn hoặc bằng " + minValue.ToString());
                        continue;
                    }
                    else
                        _drCurrentMaster.SetColumnError(fieldName, string.Empty);
                }
                if (drField["MaxValue"].ToString() != string.Empty)
                {
                    int maxValue = Int32.Parse(drField["MaxValue"].ToString());
                    if (maxValue < value)
                        _drCurrentMaster.SetColumnError(fieldName, "Phải nhỏ hơn hoặc bằng " + maxValue.ToString());
                    else
                        _drCurrentMaster.SetColumnError(fieldName, string.Empty);
                }
            }
        }

        /// <summary>
        /// Hàm thiết lập giá trị mặc định trước khi bắt đầu nhập liệu
        /// </summary>
        protected virtual void SetDefaultValues(DataTable dtStruct, DataRow drData)
        {
            //riêng cho sửa biểu mẫu báo cáo
            if (Config.GetValue("sysReportID") != null && drData.Table.Columns.Contains("sysReportID"))
                drData["sysReportID"] = Config.GetValue("sysReportID");

            if (_formulaCaculator != null)
                _formulaCaculator.Active = false;
            foreach (DataRow drField in dtStruct.Rows)
            {                
                if (_drCurrentMaster != null)
                {   //truong hop gia tri trong detail se nhan tu master, dung defaultvalue cua detail lam trung gian
                    string formulaDetail = drField["FormulaDetail"].ToString();
                    if (formulaDetail != string.Empty && !formulaDetail.Contains(".")&&_drCurrentMaster.Table.Columns.Contains(formulaDetail) )
                    {
                            
                        drField["DefaultValue"] = _drCurrentMaster[formulaDetail];

                    }
                }

                string fieldName = drField["FieldName"].ToString();
                if (_drTable.Table.Columns.Contains("TableName") && _drTable["TableName"].ToString().ToUpper() != "SYSPACKAGE" && fieldName.ToUpper() == "SYSPACKAGEID")
                {
                    if (_drTable["TableName"].ToString().ToUpper() == "SYSCONFIG")
                        drData[fieldName] = Config.GetValue("sysPackageID").ToString();
                    else if (Config.GetValue("curPackageID") != null && Config.GetValue("curPackageID").ToString().Trim() != string.Empty)
                        drData[fieldName] = Config.GetValue("curPackageID").ToString();
                }
                string defaultValue = drField["DefaultValue"].ToString();
                int pType = Int32.Parse(drField["Type"].ToString());

                if ((pType==9 || pType == 14) && drField["EditMask"].ToString() == "n" && this.dataType!=DataType.Report)
                    drData[fieldName] = DateTime.Now;
                if (defaultValue == string.Empty && pType != 6)
                    continue;
                if (pType == 6) //cap nhat gia tri khoa guid cho khoa chinh
                    drData[fieldName] = Guid.NewGuid();
                else
                {
                    if (pType == 10)
                        drData[fieldName] = (defaultValue == "1") ? true : false;
                    else
                    {
                        drData[fieldName] = defaultValue;
                    }
                }
            }
            if (_formulaCaculator != null)
                _formulaCaculator.Active = true;
        }

        public void SetValuesFromList(string controlFrom, string value, DataRow drDataFrom, bool Refresh)
        {   if (_drCurrentMaster == null)
                return;
            if (!Refresh)
            {
                _drCurrentMaster[controlFrom] = value;
                foreach (DataRow drField in _dsStruct.Tables[0].Rows)
                {
                    string formulaDetail = drField["FormulaDetail"].ToString();
                    if (formulaDetail == string.Empty)
                        continue;
                    string[] str = formulaDetail.Split(".".ToCharArray());
                    if (controlFrom.ToUpper() != str[0].ToUpper())
                        continue;
                    string fieldName = drField["FieldName"].ToString();
                    _drCurrentMaster[fieldName] = drDataFrom[str[1]];
                    _drCurrentMaster.EndEdit();
                }
            }
            List<string> lstStr = new List<string>();
            if (_dataType == DataType.MasterDetail)
                foreach (DataRow drField in _dsStruct.Tables[1].Rows)
                {   //truong hop gia tri trong detail se nhan tu danh muc cua master, dung defaultvalue cua detail lam trung gian
                    string formulaDetail = drField["FormulaDetail"].ToString();
                    if (formulaDetail == string.Empty)
                        continue;
                    string[] str = formulaDetail.Split(".".ToCharArray());
                    if (controlFrom.ToUpper() != str[0].ToUpper() && !lstStr.Contains(str[0].ToUpper()))
                        continue;
                    lstStr.Add(drField["FieldName"].ToString().ToUpper());
                    drField["DefaultValue"] = drDataFrom[str[1]];
                    List<object> ob = new List<object>();
                    ob.Add(drField["FieldName"]);
                    ob.Add(drField["DefaultValue"]);
                    this.SetDetailValue(ob, new EventArgs());
                }
        }

        public void SetValuesFromListDt(DataRow drDetail, string controlFrom, string value, DataRow drDataFrom)
        {
            if (_lstDrCurrentDetails.Count == 0)
                return;
            drDetail[controlFrom] = value;
            foreach (DataRow drField in _dsStruct.Tables[1].Rows)
            {
                string formulaDetail = drField["FormulaDetail"].ToString();
                if (formulaDetail == string.Empty)
                    continue;
                string[] str = formulaDetail.Split(".".ToCharArray());
                if (controlFrom.ToUpper() != str[0].ToUpper())
                    continue;
                string fieldName = drField["FieldName"].ToString();
                
                drDetail[fieldName] = drDataFrom[str[1]];
                drDetail.EndEdit();
            }
        }

        public void GetDataForLookup(CDTData ParentData)
        {
            (this as DataSingle).GetData(ParentData);
        }

        public void Reset()
        {
            _lstDrCurrentDetails = new List<DataRow>();
            _drCurrentMaster = null;
            _dataChanged = false;
        }
        public bool KiemtraDemo()
        {
            string sql;
            if (Config.GetValue("isDemo").ToString() == "1")
            {
                sql = "select thang from (select cast(month(ngayct)as nvarchar(2)) + cast(year(ngayct)as nvarchar(4))    as thang from bltk ) x group by thang";
                DataTable TKtra = _dbData.GetDataTable(sql);
                if (TKtra.Rows.Count >= 2)
                {
                    MessageBox.Show("Bản Demo chỉ sử dụng trong 1 tháng số liệu");
                    return false;
                }
            }
            return true;
        }
    }
}
