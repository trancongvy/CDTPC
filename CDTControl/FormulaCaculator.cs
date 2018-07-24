using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTControl;
using Formula;

namespace CDTControl
{
    public enum DataType { Single, Detail, MasterDetail, Report };
    public enum DataAction { Insert, Update, Delete, IUD};
    public struct CurrentRowDt
    {
        public string TableName;
        public DataRow RowDetail;
    }
    public class FormulaCaculator
    {
        private bool _active;
        private DataType _dataType;
        private DataRow _drCurrentMaster;
        private DataSet _dsData;
        private DataSet _dsStruct;
        private DataSet _dsStructDt;
        private List<CurrentRowDt> _lstCurrentRowDt;
        private List<DataRow> _lstDrCurrentDetails;
        private Hashtable _variables;
        private Hashtable _variablesMaster;

        // Methods
        public FormulaCaculator(DataType dataType, DataSet dsStruct)
        {
            this._dsStruct = new DataSet();
            this._lstDrCurrentDetails = new List<DataRow>();
            this._variablesMaster = new Hashtable();
            this._variables = new Hashtable();
            this._active = true;
            this._dsStructDt = new DataSet();
            this._dataType = dataType;
            this._dsStruct = dsStruct;
            this.GetVariables();
        }

        public FormulaCaculator(DataType dataType, DataSet dsStruct, DataSet dsStructDt)
        {
            this._dsStruct = new DataSet();
            this._lstDrCurrentDetails = new List<DataRow>();
            this._variablesMaster = new Hashtable();
            this._variables = new Hashtable();
            this._active = true;
            this._dsStructDt = new DataSet();
            this._dataType = dataType;
            this._dsStruct = dsStruct;
            this._dsStructDt = dsStructDt;
            this.GetVariables();
        }

        private void DataTable_i_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (this._active)
            {
                string fieldName;
                string strFormula;
                string[] var;
                if (this._dsStructDt.Tables[(sender as DataTable).TableName].Columns.Contains("CalIndex") && (this._dsStruct.Tables[1].DefaultView.Sort == ""))
                {
                    this._dsStructDt.Tables[(sender as DataTable).TableName].DefaultView.Sort = "CalIndex";
                }
                if (this._variables.ContainsKey(e.Column.ColumnName.ToUpper()))
                {
                    foreach (DataRowView vdrField in this._dsStructDt.Tables[(sender as DataTable).TableName].DefaultView)
                    {
                        DataRow drField = vdrField.Row;
                        fieldName = drField["FieldName"].ToString();
                        strFormula = drField["Formula"].ToString();
                        if (strFormula != string.Empty)
                        {
                            BieuThuc bt;
                            if (strFormula.ToUpper().Contains("ROUND("))
                            {
                                var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                                if (var.Length != 2)
                                {
                                    continue;
                                }
                                string strFormulaTmp = var[0];
                                bt = new BieuThuc(strFormulaTmp);
                            }
                            else
                            {
                                bt = new BieuThuc(strFormula);
                            }
                            if (bt.variables.Contains(e.Column.ColumnName.ToUpper()))
                            {
                                bool isValid = true;
                                Hashtable h = new Hashtable();
                                foreach (string s in bt.VariablesOriginal)
                                {
                                    if (e.Row.Table.Columns.Contains(s))
                                    {
                                        h.Add(s, e.Row[s]);
                                    }
                                    else if (this._drCurrentMaster.Table.Columns.Contains(s))
                                    {
                                        h.Add(s, this._drCurrentMaster[s]);
                                    }
                                    else
                                    {
                                        isValid = false;
                                        break;
                                    }
                                }
                                if (isValid)
                                {
                                    double value;
                                    if (strFormula.ToUpper().Contains("ROUND("))
                                    {
                                        var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                                        if (var.Length != 2)
                                        {
                                            continue;
                                        }
                                        string r = var[1];
                                        value = Math.Round(bt.Evaluate(h), int.Parse(r),MidpointRounding.AwayFromZero);
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), int.Parse(r), MidpointRounding.AwayFromZero) != value))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    else
                                    {
                                        value = bt.Evaluate(h);
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    e.Row.EndEdit();
                                }
                            }
                        }
                    }
                }
                if (this._variablesMaster.ContainsKey(e.Column.ColumnName.ToUpper()))
                {
                    foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
                    {
                        fieldName = drField["FieldName"].ToString();
                        strFormula = drField["Formula"].ToString();
                        if (strFormula != string.Empty)
                        {
                            int i = strFormula.IndexOf("@") + 1;
                            string variable = strFormula.Substring(i, (strFormula.Length - i) - 1);
                            List<DataRow> _lstdrCrrDetails = new List<DataRow>();
                            foreach (CurrentRowDt CrDt in this._lstCurrentRowDt)
                            {
                                if (CrDt.TableName == (sender as DataTable).TableName)
                                {
                                    _lstdrCrrDetails.Add(CrDt.RowDetail);
                                }
                            }
                            if (_lstdrCrrDetails.Count >= 1)
                            {
                                decimal sum;
                                string value;
                                if (strFormula.ToUpper().Contains("SUMIF"))
                                {
                                    var = variable.Split(",".ToCharArray());
                                    if ((var.Length == 2) && (var[0].ToUpper() == e.Column.ColumnName.ToUpper()))
                                    {
                                        variable = var[0];
                                        DataRow[] drCurrentdetail = _lstdrCrrDetails.ToArray();
                                        DataTable detaildTmp = drCurrentdetail[0].Table.Clone();
                                        detaildTmp.Clear();
                                        foreach (DataRow drDetail in drCurrentdetail)
                                        {
                                            detaildTmp.ImportRow(drDetail);
                                        }
                                        drCurrentdetail = detaildTmp.Select(var[1]);
                                        sum = 0M;
                                        foreach (DataRow drDetail in drCurrentdetail)
                                        {
                                            try
                                            {
                                                value = drDetail[e.Column.ColumnName].ToString();
                                                if (value != string.Empty)
                                                {
                                                    sum += decimal.Parse(value);
                                                }
                                            }
                                            catch   
                                            {
                                            }
                                        }
                                        if (e.Row.RowState == DataRowState.Detached)
                                        {
                                            this._dsData.Tables[(sender as DataTable).TableName].Rows.Add(e.Row);
                                        }
                                        this._drCurrentMaster[fieldName] = sum;
                                        this._drCurrentMaster.EndEdit();
                                    }
                                }
                                else if ((variable.ToUpper() == e.Column.ColumnName.ToUpper()) && strFormula.ToUpper().Contains("SUM"))
                                {
                                    sum = 0M;
                                    foreach (DataRow drDetail in _lstdrCrrDetails)
                                    {
                                        try
                                        {
                                            value = drDetail[e.Column.ColumnName].ToString();
                                            if (value != string.Empty)
                                            {
                                                sum += decimal.Parse(value);
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    if (e.Row.RowState == DataRowState.Detached)
                                    {
                                        this._dsData.Tables[(sender as DataTable).TableName].Rows.Add(e.Row);
                                    }
                                    this._drCurrentMaster[fieldName] = sum;
                                    this._drCurrentMaster.EndEdit();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DataTable_i_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            if (this._active)
            {
                List<DataRow> _lstdrCrrDetails = new List<DataRow>();
                foreach (CurrentRowDt CrDt in this._lstCurrentRowDt)
                {
                    if (CrDt.TableName == (sender as DataTable).TableName)
                    {
                        _lstdrCrrDetails.Add(CrDt.RowDetail);
                    }
                }
                foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
                {
                    string fieldName = drField["FieldName"].ToString();
                    string strFormula = drField["Formula"].ToString();
                    if (strFormula != string.Empty)
                    {
                        int i = strFormula.IndexOf("@") + 1;
                        if (this._drCurrentMaster == null)
                        {
                            break;
                        }
                        foreach (DataColumn col in this._dsData.Tables[(sender as DataTable).TableName].Columns)
                        {
                            if ((strFormula.Substring(i, (strFormula.Length - i) - 1).ToUpper() == col.ColumnName.ToUpper()) && strFormula.ToUpper().Contains("SUM"))
                            {
                                decimal sum = 0M;
                                foreach (DataRow drDetail in _lstdrCrrDetails)
                                {
                                    try
                                    {
                                        string value = drDetail[col.ColumnName].ToString();
                                        if (value != string.Empty)
                                        {
                                            sum += decimal.Parse(value);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                this._drCurrentMaster[fieldName] = sum;
                            }
                        }
                        this._drCurrentMaster.EndEdit();
                    }
                }
            }
        }

        private void DataTable0_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (!_active || e.Row.RowState == DataRowState.Deleted)
                return;
            string[] var;
            foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
            {
                string fieldName1 = drField["FieldName"].ToString();
                string strFormula1 = drField["Formula"].ToString();
                string strFormnulaCT = drField["FormulaDetail"].ToString();
                if (strFormnulaCT.ToUpper() == e.Column.ColumnName.ToUpper())
                {
                    e.Row[fieldName1] = e.Row[e.Column.ColumnName];
                    e.Row.EndEdit();
                }
                if (strFormula1.ToUpper().Contains(e.Column.ColumnName.ToUpper())) 
                {
                    string[] fieldName;
                    DateTime d1;
                    TimeSpan d;

                    if (strFormula1.Contains("DAY"))
                    {
                        fieldName = strFormula1.Substring(4, strFormula1.Length - 5).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = d.Days;
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                    if (strFormula1.Contains("HOUR"))
                    {
                        fieldName = strFormula1.Substring(5, strFormula1.Length - 6).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = d.Hours;
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                    if (strFormula1.Contains("MINUTE"))
                    {
                        fieldName = strFormula1.Substring(7, strFormula1.Length - 8).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = d.Minutes;
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                    if (strFormula1.Contains("TDAY"))
                    {
                        fieldName = strFormula1.Substring(5, strFormula1.Length - 6).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = d.Days;
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                    if (strFormula1.Contains("RDAY"))
                    {
                        fieldName = strFormula1.Substring(5, strFormula1.Length - 6).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = Math.Round(d.TotalDays, 0);
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                    if (strFormula1.Contains("THOUR"))
                    {
                        fieldName = strFormula1.Substring(6, strFormula1.Length - 7).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = d.Hours;
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                    if (strFormula1.Contains("TMINUTE"))
                    {
                        fieldName = strFormula1.Substring(7, strFormula1.Length - 9).Split(",".ToCharArray());
                        try
                        {
                            d1 = DateTime.Parse(e.Row[fieldName[0].ToString()].ToString());
                            d = (TimeSpan)(DateTime.Parse(e.Row[fieldName[1].ToString()].ToString()) - d1);
                            e.Row[fieldName1] = d.Minutes;
                            e.Row.EndEdit();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (this._variablesMaster.ContainsKey(e.Column.ColumnName.ToUpper()))
            {
                string fieldName;
                string strFormula;
                BieuThuc bt;
                bool isValid;
                Hashtable h;


                foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
                {
                    fieldName = drField["FieldName"].ToString();
                    if (drField["DisplayMember"].ToString() != string.Empty)
                    {
                       e.Row.EndEdit();
                    }
                    strFormula = drField["Formula"].ToString();
                    if (strFormula != string.Empty)
                    {
                        if (strFormula.ToUpper().Contains("ROUND("))
                        {
                            var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                            if (var.Length == 2)
                            {
                                string strFormulaTmp = var[0];
                                bt = new BieuThuc(strFormulaTmp);
                            }
                            else
                            {
                                bt = new BieuThuc(strFormula);
                            }
                        }
                        else if (strFormula.ToUpper().Contains("INT("))
                        {
                            string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                            bt = new BieuThuc(strFormulaTmp);
                        }
                        else
                        {
                            bt = new BieuThuc(strFormula);
                        }
                        if (bt.variables.Contains(e.Column.ColumnName.ToUpper()))
                        {
                            isValid = true;
                            h = new Hashtable();
                            foreach (string s in bt.VariablesOriginal)
                            {
                                if (e.Row.Table.Columns.Contains(s))
                                {
                                    h.Add(s, e.Row[s]);
                                }
                                else
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                            if (isValid)
                            {
                                double value;
                                if (strFormula.ToUpper().Contains("ROUND("))
                                {
                                    var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                                    if (var.Length != 2)
                                    {
                                        continue;
                                    }
                                    string r = var[1];
                                    value = Math.Round(bt.Evaluate(h), int.Parse(r), MidpointRounding.AwayFromZero);
                                    if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), int.Parse(r), MidpointRounding.AwayFromZero) != value))
                                    {
                                        e.Row[fieldName] = value;
                                        e.Row.EndEdit();
                                    }
                                }
                                else  if (strFormula.ToUpper().Contains("INT("))
                                {
                                    string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                    value = bt.Evaluate(h);
                                    if (value < 0) value = 0;

                                    e.Row[fieldName] = value;
                                    e.Row.EndEdit();
                                }
                                else
                                {
                                    double v = bt.Evaluate(h);
                                    if (e.Row[fieldName] == DBNull.Value || double.Parse(e.Row[fieldName].ToString()) != v)
                                        e.Row[fieldName] = bt.Evaluate(h);
                                    e.Row.EndEdit();
                                }
                            }
                        }
                    }
                }
            }
            if ((this._dataType == DataType.MasterDetail) && this._variables.ContainsKey(e.Column.ColumnName.ToUpper()))
            {
                string fieldName;
                string strFormula;
                BieuThuc bt;
                bool isValid;
                Hashtable h;
                foreach (DataRow drField in this._dsStruct.Tables[1].Rows)
                {
                    fieldName = drField["FieldName"].ToString();
                    strFormula = drField["Formula"].ToString();
                    if (strFormula != string.Empty)
                    {
                         bt = new BieuThuc("0");
                        List<string> ConditionVariable = new List<string>(); ;
                        if (strFormula.ToUpper().Contains("ROUND("))
                        {
                            var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                            if (var.Length != 2)
                            {
                                continue;
                            }
                            string strFormulaTmp = var[0];
                            bt = new BieuThuc(strFormulaTmp);
                        }
                        else if (strFormula.ToUpper().Contains("INT("))
                        {
                            string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                            bt = new BieuThuc(strFormulaTmp);
                        }
                        else if (strFormula.ToUpper().Contains("ABS("))
                        {
                            string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                            bt = new BieuThuc(strFormulaTmp);
                        }
                        else if (strFormula.ToUpper().Contains("CASE("))
                        {
                            var = strFormula.Substring(5, strFormula.Length - 6).Split(",".ToCharArray());
                            if (var.Length != 3)
                            {
                                strFormula = var[0];
                                bt = new BieuThuc(strFormula);
                                continue;
                            }
                            string condition = var[0];
                            string phepss = "";
                            string[] condition1;

                            if (condition.Contains("<="))
                            {
                                condition1 = condition.Split("<=".ToCharArray());
                                phepss = "<=";
                            }
                            else if (condition.Contains(">="))
                            {
                                condition1 = condition.Split(">=".ToCharArray());
                                phepss = ">=";
                            }
                            else if (condition.Contains("="))
                            {
                                condition1 = condition.Split("=".ToCharArray());
                                phepss = "=";
                            }
                            else if (condition.Contains(">"))
                            {
                                condition1 = condition.Split(">".ToCharArray());
                                phepss = ">";
                            }
                            else if (condition.Contains("<"))
                            {
                                condition1 = condition.Split("<".ToCharArray());
                                phepss = "<";
                            }
                            else
                            {
                                condition1 = condition.Split("=".ToCharArray());
                            }


                            BieuThuc bttmp = new BieuThuc(condition1[0]);
                            Hashtable htmp = new Hashtable();
                            ConditionVariable.AddRange(bttmp.variables);
                            foreach (string s in bttmp.VariablesOriginal)
                            {
                                if (e.Row.Table.Columns.Contains(s))
                                {
                                    htmp.Add(s, e.Row[s]);
                                }
                                else if (this._drCurrentMaster.Table.Columns.Contains(s))
                                {
                                    htmp.Add(s, this._drCurrentMaster[s]);
                                }

                            }
                            string strFormulaTmp;
                            if (condition1.Length == 1)
                            {
                                if (bool.Parse(htmp[bttmp.VariablesOriginal[0]].ToString()))
                                {
                                    strFormulaTmp = var[1];
                                    bt = new BieuThuc(strFormulaTmp);
                                }
                                else
                                {
                                    strFormulaTmp = var[2];
                                    bt = new BieuThuc(strFormulaTmp);
                                }
                            }
                            else if (condition1.Length == 2)
                            {
                                double v1 = bttmp.Evaluate(htmp);
                                BieuThuc bttmp1 = new BieuThuc(condition1[1]);
                                Hashtable htmp1 = new Hashtable();
                                ConditionVariable.AddRange(bttmp1.variables);
                                foreach (string s in bttmp1.VariablesOriginal)
                                {
                                    if (e.Row.Table.Columns.Contains(s))
                                    {
                                        htmp1.Add(s, e.Row[s]);
                                    }
                                    else if (this._drCurrentMaster.Table.Columns.Contains(s))
                                    {
                                        htmp1.Add(s, this._drCurrentMaster[s]);
                                    }

                                }
                                double v2 = bttmp1.Evaluate(htmp1);
                                bool ConValue = false;
                                switch (phepss)
                                {
                                    case "=":
                                        ConValue = v1 == v2;
                                        break;
                                    case ">=":
                                        ConValue = v1 >= v2;
                                        break;
                                    case "<=":
                                        ConValue = v1 <= v2;
                                        break;
                                    case ">":
                                        ConValue = v1 > v2;
                                        break;
                                    case "<":
                                        ConValue = v1 < v2;
                                        break;
                                }
                                if (ConValue)
                                {
                                    strFormulaTmp = var[1];
                                    bt = new BieuThuc(strFormulaTmp);
                                }
                                else
                                {
                                    strFormulaTmp = var[2];
                                    bt = new BieuThuc(strFormulaTmp);
                                }
                            }
                            else
                            {
                                strFormulaTmp = var[0];
                                bt = new BieuThuc(strFormulaTmp);
                            }


                        }
                        else
                        {
                            bt = new BieuThuc(strFormula);
                        }
                        if (bt.VariablesOriginal.Contains(e.Column.ColumnName))
                        {
                            foreach (DataRow drDetail in this._lstDrCurrentDetails)
                            {
                                isValid = true;
                                h = new Hashtable();
                                foreach (string s in bt.VariablesOriginal)
                                {
                                    if (e.Row.Table.Columns.Contains(s))
                                    {
                                        h.Add(s, e.Row[s]);
                                    }
                                    else if ((drDetail.Table.Columns.Contains(s) && (drDetail.RowState != DataRowState.Detached)) && (drDetail.RowState != DataRowState.Deleted))
                                    {
                                        h.Add(s, drDetail[s]);
                                    }
                                    else
                                    {
                                        isValid = false;
                                        break;
                                    }
                                }
                                
                                    if (isValid)
                                    {
                                        double value;
                                        if (strFormula.ToUpper().Contains("ROUND("))
                                        {
                                            var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                                            if (var.Length != 2)
                                            {
                                                continue;
                                            }
                                            string r = var[1];

                                            value = Math.Round(bt.Evaluate(h), int.Parse(r), MidpointRounding.AwayFromZero);
                                            if ((drDetail[fieldName] != DBNull.Value) && (Math.Round(double.Parse(drDetail[fieldName].ToString()), int.Parse(r), MidpointRounding.AwayFromZero) != value))
                                            {
                                                drDetail[fieldName] = value;
                                            }
                                        }
                                        else if (strFormula.ToUpper().Contains("INT("))
                                        {
                                            // string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                            value = bt.Evaluate(h);
                                            if (value < 0) value = 0;
                                            if ((drDetail[fieldName] != DBNull.Value) && (Math.Round(double.Parse(drDetail[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                            {
                                                drDetail[fieldName] = value;
                                            }
                                        }
                                        else if (strFormula.ToUpper().Contains("ABS("))
                                        {
                                            // string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                            value = bt.Evaluate(h);
                                            value = Math.Abs(value);
                                            if ((drDetail[fieldName] != DBNull.Value) && (Math.Round(double.Parse(drDetail[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                            {
                                                drDetail[fieldName] = value;
                                            }
                                        }
                                        else if (strFormula.ToUpper().Contains("CASE("))
                                        {
                                            //string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                            value = bt.Evaluate(h);

                                            if ((drDetail[fieldName] != DBNull.Value) && (Math.Round(double.Parse(drDetail[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                            {
                                                drDetail[fieldName] = value;
                                            }
                                        }
                                        else
                                        {
                                            value = bt.Evaluate(h);
                                            if ((drDetail[fieldName] != DBNull.Value) && (Math.Round(double.Parse(drDetail[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                            {
                                                drDetail[fieldName] = value;
                                            }
                                        }
                                        drDetail.EndEdit();
                                    }
                                //{
                                //    double v = bt.Evaluate(h);
                                //    if (drDetail[fieldName] == DBNull.Value || double.Parse(drDetail[fieldName].ToString()) != v)
                                //        drDetail[fieldName] = v;
                                //    drDetail.EndEdit();
                                //}
                            }
                        }
                    }
                }
                for (int i = 0; i < this._dsStructDt.Tables.Count; i++)
                {
                    DataTable tbStruct_i = this._dsStructDt.Tables[i];
                    foreach (DataRow drField in tbStruct_i.Rows)
                    {
                        fieldName = drField["FieldName"].ToString();
                        strFormula = drField["Formula"].ToString();
                        if (strFormula != string.Empty && strFormula.ToUpper() == e.Column.ColumnName.ToUpper())
                        {
                            if (int.Parse(drField["Type"].ToString()) != 8)
                            {
                                List<DataRow> _lstdrCrrDetails = new List<DataRow>();
                                foreach (CurrentRowDt CrDt in this._lstCurrentRowDt)
                                {
                                    if (CrDt.TableName == tbStruct_i.TableName)
                                    {
                                        _lstdrCrrDetails.Add(CrDt.RowDetail);
                                        CrDt.RowDetail[fieldName] = e.Row[e.Column.ColumnName];
                                        CrDt.RowDetail.EndEdit();
                                    }
                                }

                            }
                            else
                            {
                            }
                        }
                    }

                }
            }
            
        }

        private void DataTable1_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (this._active)
            {
                string fieldName;
                string strFormula;
                string[] var;
                foreach (DataRow drField in this._dsStruct.Tables[1].Rows)
                {
                    fieldName = drField["FieldName"].ToString();
                    string strFormnulaCT = drField["FormulaDetail"].ToString();
                    if (strFormnulaCT.ToUpper() == e.Column.ColumnName.ToUpper())
                    {
                        if (e.Row[fieldName].ToString() != e.Row[e.Column.ColumnName].ToString())
                        {
                            e.Row[fieldName] = e.Row[e.Column.ColumnName];
                            e.Row.EndEdit();
                        }
                    }
                }
                if (this._dsStruct.Tables[1].Columns.Contains("CalIndex") && (this._dsStruct.Tables[1].DefaultView.Sort == ""))
                {
                    this._dsStruct.Tables[1].DefaultView.Sort = "CalIndex";
                }
                if (this._variables.ContainsKey(e.Column.ColumnName.ToUpper()))
                {
                    foreach (DataRowView vdrField in this._dsStruct.Tables[1].DefaultView)
                    {
                        DataRow drField = vdrField.Row;
                        fieldName = drField["FieldName"].ToString();
                        strFormula = drField["Formula"].ToString();
                        if (strFormula != string.Empty)
                        {
                            BieuThuc bt=new BieuThuc("0");
                            List<string> ConditionVariable = new List<string>(); ;
                            if (strFormula.ToUpper().Contains("ROUND("))
                            {
                                var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                                if (var.Length != 2)
                                {
                                    continue;
                                }
                                string strFormulaTmp = var[0];
                                bt = new BieuThuc(strFormulaTmp);
                            }
                            
                            else if (strFormula.ToUpper().Contains("INT("))
                            {
                                string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);                                
                                bt = new BieuThuc(strFormulaTmp);                                
                            }
                            else if (strFormula.ToUpper().Contains("ABS("))
                            {
                                string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                bt = new BieuThuc(strFormulaTmp);
                            }
                            else if (strFormula.ToUpper().Contains("CASE("))
                            {
                                var = strFormula.Substring(5, strFormula.Length - 6).Split(",".ToCharArray());
                                if (var.Length != 3)
                                {
                                    strFormula = var[0];
                                    bt = new BieuThuc(strFormula);
                                    continue;
                                }
                                string condition = var[0];
                                string phepss="";
                                string[] condition1 ;
                                
                                 if(condition.Contains("<=")) {
                                    condition1 = condition.Split("<=".ToCharArray());
                                    phepss="<=";
                                }else if(condition.Contains(">=")) {
                                    condition1 = condition.Split(">=".ToCharArray());
                                    phepss=">=";
                                }else if(condition.Contains("=")) {
                                    condition1 = condition.Split("=".ToCharArray());
                                    phepss="=";
                                }else if (condition.Contains(">")) {
                                    condition1 = condition.Split(">".ToCharArray());
                                    phepss=">";
                                }else if (condition.Contains("<")) {
                                    condition1 = condition.Split("<".ToCharArray());
                                    phepss="<";
                                }else 
                                 {
                                    condition1 = condition.Split("=".ToCharArray());
                                }


                                BieuThuc bttmp = new BieuThuc(condition1[0]);
                                Hashtable htmp = new Hashtable();
                                ConditionVariable.AddRange(bttmp.variables);
                                foreach (string s in bttmp.VariablesOriginal)
                                {
                                    if (e.Row.Table.Columns.Contains(s))
                                    {
                                        htmp.Add(s, e.Row[s]);
                                    }
                                    else if (this._drCurrentMaster.Table.Columns.Contains(s))
                                    {
                                        htmp.Add(s, this._drCurrentMaster[s]);
                                    }

                                }
                                string strFormulaTmp;
                                if (condition1.Length == 1)
                                {
                                    if (bool.Parse(htmp[bttmp.VariablesOriginal[0]].ToString()))
                                    {
                                        strFormulaTmp = var[1];
                                        bt = new BieuThuc(strFormulaTmp);
                                    }
                                    else
                                    {
                                        strFormulaTmp = var[2];
                                        bt = new BieuThuc(strFormulaTmp);
                                    }
                                }
                                else if (condition1.Length == 2)
                                {
                                    double v1 = bttmp.Evaluate(htmp);
                                    BieuThuc bttmp1 = new BieuThuc(condition1[1]);
                                    Hashtable htmp1 = new Hashtable();
                                    ConditionVariable.AddRange(bttmp1.variables);
                                    foreach (string s in bttmp1.VariablesOriginal)
                                    {
                                        if (e.Row.Table.Columns.Contains(s))
                                        {
                                            htmp1.Add(s, e.Row[s]);
                                        }
                                        else if (this._drCurrentMaster.Table.Columns.Contains(s))
                                        {
                                            htmp1.Add(s, this._drCurrentMaster[s]);
                                        }

                                    }
                                    double v2 = bttmp1.Evaluate(htmp1);
                                    bool ConValue = false;
                                    switch (phepss){
                                        case "=":
                                            ConValue = v1 == v2;
                                            break;
                                        case ">=":
                                            ConValue = v1 >= v2;
                                            break;
                                        case "<=":
                                            ConValue = v1 <= v2;
                                            break;
                                        case ">":
                                            ConValue = v1 > v2;
                                            break;
                                        case "<":
                                            ConValue = v1 < v2;
                                            break;
                                    }
                                    if (ConValue)
                                    {
                                        strFormulaTmp = var[1];
                                        bt = new BieuThuc(strFormulaTmp);
                                    }
                                    else
                                    {
                                        strFormulaTmp = var[2];
                                        bt = new BieuThuc(strFormulaTmp);
                                    }
                                }
                                else
                                {
                                    strFormulaTmp = var[0];
                                    bt = new BieuThuc(strFormulaTmp);
                                }


                            }
                            else
                            {
                                bt = new BieuThuc(strFormula);
                            }
                            if (bt.variables.Contains(e.Column.ColumnName.ToUpper()) || ConditionVariable.Contains(e.Column.ColumnName.ToUpper()))
                            {
                                bool isValid = true;
                                Hashtable h = new Hashtable();
                                foreach (string s in bt.VariablesOriginal)
                                {
                                    if (e.Row.Table.Columns.Contains(s))
                                    {
                                        h.Add(s, e.Row[s]);
                                    }
                                    else if (this._drCurrentMaster.Table.Columns.Contains(s))
                                    {
                                        h.Add(s, this._drCurrentMaster[s]);
                                    }
                                    else
                                    {
                                        isValid = false;
                                        break;
                                    }
                                }
                                if (isValid)
                                {
                                    double value;
                                    if (strFormula.ToUpper().Contains("ROUND("))
                                    {
                                        var = strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray());
                                        if (var.Length != 2)
                                        {
                                            continue;
                                        }
                                        string r = var[1];

                                        value = Math.Round(bt.Evaluate(h), int.Parse(r), MidpointRounding.AwayFromZero);
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), int.Parse(r), MidpointRounding.AwayFromZero) != value))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    else if (strFormula.ToUpper().Contains("INT("))
                                    {
                                       // string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                        value = bt.Evaluate(h);
                                        if (value < 0) value = 0;
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    else if (strFormula.ToUpper().Contains("ABS("))
                                    {
                                        // string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                        value = bt.Evaluate(h);
                                        value = Math.Abs(value);
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    else if (strFormula.ToUpper().Contains("CASE("))
                                    {
                                        //string strFormulaTmp = strFormula.Substring(4, strFormula.Length - 5);
                                        value = bt.Evaluate(h);
                                       
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    else
                                    {
                                        value = bt.Evaluate(h);
                                        if ((e.Row[fieldName] != DBNull.Value) && (Math.Round(double.Parse(e.Row[fieldName].ToString()), 6, MidpointRounding.AwayFromZero) != Math.Round(value, 6, MidpointRounding.AwayFromZero)))
                                        {
                                            e.Row[fieldName] = value;
                                        }
                                    }
                                    e.Row.EndEdit();
                                }
                            }
                        }
                    }
                }
               
            }
        }
        private void DataTable1_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            string fieldName;
            string strFormula;
            string[] var;
            if (this._drCurrentMaster.RowState == DataRowState.Deleted || this._drCurrentMaster.RowState == DataRowState.Detached) return;
            foreach (DataColumn col in this._dsData.Tables[1].Columns)
            {
                if (this._variablesMaster.ContainsKey(col.ColumnName.ToUpper()))
                {
                    foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
                    {
                        fieldName = drField["FieldName"].ToString();
                        strFormula = drField["Formula"].ToString();
                        if (strFormula != string.Empty)
                        {
                            int i = strFormula.IndexOf("@") + 1;
                            string variable = strFormula.Substring(i, (strFormula.Length - i) - 1);
                            if (this._lstDrCurrentDetails.Count >= 1)
                            {
                                decimal sum;
                                string value;
                                if (strFormula.ToUpper().Contains("SUMIF"))
                                {
                                    var = variable.Split(",".ToCharArray());
                                    if ((var.Length == 2) && (var[0].ToUpper() == col.ColumnName.ToUpper()))
                                    {
                                        variable = var[0];
                                        DataRow[] drCurrentdetail = this._lstDrCurrentDetails.ToArray();
                                        DataTable detaildTmp = drCurrentdetail[0].Table.Clone();
                                        detaildTmp.Clear();
                                        foreach (DataRow drDetail in drCurrentdetail)
                                        {
                                            detaildTmp.ImportRow(drDetail);
                                        }
                                        drCurrentdetail = detaildTmp.Select(var[1]);
                                        sum = 0M;
                                        foreach (DataRow drDetail in drCurrentdetail)
                                        {
                                            try
                                            {
                                                value = drDetail[col.ColumnName].ToString();
                                                if (value != string.Empty)
                                                {
                                                    sum += decimal.Parse(value);
                                                }
                                            }
                                            catch
                                            {
                                            }
                                        }
                                        if (e.Row.RowState == DataRowState.Detached)
                                        {
                                            this._dsData.Tables[1].Rows.Add(e.Row);
                                        }
                                        if (this._drCurrentMaster[fieldName] == DBNull.Value || decimal.Parse(this._drCurrentMaster[fieldName].ToString()) != sum)
                                            this._drCurrentMaster[fieldName] = sum;
                                        this._drCurrentMaster.EndEdit();
                                    }
                                }
                                else if ((variable.ToUpper() == col.ColumnName.ToUpper()) && strFormula.ToUpper().Contains("SUM"))
                                {
                                    sum = 0M;
                                    foreach (DataRow drDetail in this._lstDrCurrentDetails)
                                    {
                                        try
                                        {
                                            value = drDetail[col.ColumnName].ToString();
                                            if (value != string.Empty)
                                            {
                                                sum += decimal.Parse(value);
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    //if (e.Row.RowState == DataRowState.Detached)
                                    //{
                                    //this._dsData.Tables[1].Rows.Add(e.Row);
                                    //}
                                    if (this._drCurrentMaster[fieldName]==DBNull.Value || decimal.Parse(this._drCurrentMaster[fieldName].ToString()) != sum)
                                        this._drCurrentMaster[fieldName] = sum;
                                    this._drCurrentMaster.EndEdit();
                                }
                            }
                        }
                    }
                }
            }
        }
        public void DataTable1_Rowdeleted(object sender, DataRowChangeEventArgs e)
        {
            if (this._active)
            {
                foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
                {
                    string fieldName = drField["FieldName"].ToString();
                    string strFormula = drField["Formula"].ToString();
                    if (strFormula != string.Empty)
                    {
                        int i = strFormula.IndexOf("@") + 1;
                        if (this._drCurrentMaster == null)
                        {
                            break;
                        }
                        foreach (DataColumn col in this._dsData.Tables[1].Columns)
                        {
                            if ((strFormula.Substring(i, (strFormula.Length - i) - 1).ToUpper() == col.ColumnName.ToUpper()) && strFormula.ToUpper().Contains("SUM"))
                            {
                                decimal sum = 0M;
                                foreach (DataRow drDetail in this._lstDrCurrentDetails)
                                {
                                    try
                                    {
                                        string value = drDetail[col.ColumnName].ToString();
                                        if (value != string.Empty)
                                        {
                                            sum += decimal.Parse(value);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                this._drCurrentMaster[fieldName] = sum;
                            }
                        }
                        this._drCurrentMaster.EndEdit();
                    }
                }
            }
        }

        private void GetVariables()
        {
            string strFormula;
            BieuThuc bt;
            foreach (DataRow drField in this._dsStruct.Tables[0].Rows)
            {
                string fieldName = drField["FieldName"].ToString().ToUpper();
                strFormula = drField["Formula"].ToString();
                string displayMember = drField["DisplayMember"].ToString();
                int fType = int.Parse(drField["Type"].ToString());
                if ((strFormula != string.Empty) || ((displayMember != string.Empty) && (fType == 1)))
                {
                    if ((displayMember != string.Empty) && (fType == 1) && !_variablesMaster.Contains(fieldName))
                    {
                        this._variablesMaster.Add(fieldName, null);
                    }
                    else
                    {
   
                        bt = new BieuThuc(strFormula);
                        foreach (string variable in bt.variables)
                            if (!_variablesMaster.ContainsKey(variable))
                                _variablesMaster.Add(variable, null);

                        if (strFormula.ToUpper().Contains("SUMIF"))
                        {
                            int i = strFormula.IndexOf("(") + 1;
                            string subFormula = strFormula.Substring(i, strFormula.Length - i - 1);
                            subFormula = subFormula.Split(",".ToCharArray())[0].ToUpper();
                            bt = new BieuThuc(subFormula);

                            foreach (string variable in bt.variables)
                                if (!_variablesMaster.ContainsKey(variable))
                                    _variablesMaster.Add(variable, null);
                        }
                        if (strFormula.ToUpper().Contains("ROUND"))
                        {
                            int i = strFormula.IndexOf("(") + 1;
                            string subFormula = strFormula.Substring(i, strFormula.Length - i - 1);
                            subFormula = subFormula.Split(",".ToCharArray())[0].ToUpper();
                            bt = new BieuThuc(subFormula);

                            foreach (string variable in bt.variables)
                                if (!_variablesMaster.ContainsKey(variable))
                                    _variablesMaster.Add(variable, null);
                        }
                        if (strFormula.ToUpper().Contains("CASE"))
                        {
                            
                            string subFormula = strFormula.Substring(5, strFormula.Length - 5 - 1);
                            subFormula = subFormula.Replace(",", "+");
                        }
                    }
                }
            }
            if (this._dataType == DataType.MasterDetail)
            {
                foreach (DataRow drField in this._dsStruct.Tables[1].Rows)
                {
                    strFormula = drField["Formula"].ToString();
                    if (strFormula != string.Empty)
                    {
                        if (strFormula.ToUpper().Contains("ROUND"))
                        {
                            bt = new BieuThuc(strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray())[0]);
                        }
                        else if (strFormula.ToUpper().Contains("INT("))
                        {
                            bt = new BieuThuc(strFormula.Substring(4, strFormula.Length - 5));
                        }
                        else if (strFormula.ToUpper().Contains("ABS("))
                        {
                            bt = new BieuThuc(strFormula.Substring(4, strFormula.Length - 5));
                        }
                        else if (strFormula.ToUpper().Contains("CASE"))
                        {

                            string subFormula = strFormula.Substring(5, strFormula.Length - 5 - 1);
                            subFormula = subFormula.Replace(",", "+");
                            bt = new BieuThuc(subFormula);
                        }else
                        {
                            bt = new BieuThuc(strFormula);
                        }
                        foreach (string variable in bt.variables)
                        {
                            if (!this._variables.ContainsKey(variable))
                            {
                                this._variables.Add(variable, null);
                            }
                        }
                        if (int.Parse(drField["Type"].ToString()) != 8)
                        {
                            if (!this._variables.ContainsKey(strFormula.ToUpper()))
                            {
                                this._variables.Add(strFormula.ToUpper(), null);
                            }
                        }
                    }
                }
                foreach (DataTable tb in this._dsStructDt.Tables)
                {
                    foreach (DataRow drField in tb.Rows)
                    {
                        strFormula = drField["Formula"].ToString();
                        if (strFormula != string.Empty)
                        {
                            if (strFormula.ToUpper().Contains("ROUND"))
                            {
                                bt = new BieuThuc(strFormula.Substring(6, strFormula.Length - 7).Split(",".ToCharArray())[0]);
                            }
                            else if (strFormula.ToUpper().Contains("INT("))
                            {
                                bt = new BieuThuc(strFormula.Substring(4, strFormula.Length - 5).Split(",".ToCharArray())[0]);
                            }
                            else if (strFormula.ToUpper().Contains("ABS("))
                            {
                                bt = new BieuThuc(strFormula.Substring(4, strFormula.Length - 5).Split(",".ToCharArray())[0]);
                            }
                            else if (strFormula.ToUpper().Contains("CASE"))
                            {

                                string subFormula = strFormula.Substring(5, strFormula.Length - 5 - 1);
                                subFormula = subFormula.Replace(",", "+");
                                bt = new BieuThuc(subFormula);
                            }
                            else
                            {
                                bt = new BieuThuc(strFormula);
                            }
                            foreach (string variable in bt.variables)
                            {
                                if (!this._variables.ContainsKey(variable))
                                {
                                    this._variables.Add(variable, null);
                                }
                            }
                            if ( int.Parse(drField["Type"].ToString()) != 8)
                            {
                                if (!this._variables.ContainsKey(strFormula.ToUpper()))
                                {
                                    this._variables.Add(strFormula.ToUpper(), null);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Properties
        public bool Active
        {
            get
            {
                return this._active;
            }
            set
            {
                this._active = value;
            }
        }

        public DataRow DrCurrentMaster
        {
            get
            {
                return this._drCurrentMaster;
            }
            set
            {
                this._drCurrentMaster = value;
            }
        }

        public DataSet DsData
        {
            set
            {
                this._dsData = value;
                if (this._dsData != null)
                {
                    this._dsData.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(this.DataTable0_ColumnChanged);
                    if ((this._dataType == DataType.MasterDetail) && (this._dsData.Tables.Count > 1))
                    {
                        this._dsData.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(this.DataTable1_ColumnChanged);
                        this._dsData.Tables[1].RowChanged += new DataRowChangeEventHandler(this.DataTable1_RowChanged);
                        this._dsData.Tables[1].RowDeleted += new DataRowChangeEventHandler(this.DataTable1_Rowdeleted);
                    }
                    if ((this._dataType == DataType.MasterDetail) && (this._dsData.Tables.Count > 2))
                    {
                        for (int i = 2; i < this._dsData.Tables.Count; i++)
                        {
                            this._dsData.Tables[i].ColumnChanged += new DataColumnChangeEventHandler(this.DataTable_i_ColumnChanged);
                            this._dsData.Tables[i].RowDeleted += new DataRowChangeEventHandler(this.DataTable_i_RowDeleted);
                        }
                    }
                }
            }
        }

        public List<CurrentRowDt> LstCurrentRowDt
        {
            get
            {
                return this._lstCurrentRowDt;
            }
            set
            {
                this._lstCurrentRowDt = value;
            }
        }

        public List<DataRow> LstDrCurrentDetails
        {
            get
            {
                return this._lstDrCurrentDetails;
            }
            set
            {
                this._lstDrCurrentDetails = value;
            }

            //void DataTable1_TableNewRow(object sender, DataTableNewRowEventArgs e)
            //{
            //    foreach (DataRow drField in _dsStruct.Tables[1].Rows)
            //    {
            //        string fieldName = drField["FieldName"].ToString();
            //        string strFormula = drField["Formula"].ToString();
            //        if (strFormula == string.Empty)
            //            continue;

            //        BieuThuc bt = new BieuThuc(strFormula);
            //        if(strFormula.Contains("."))
            //        {
            //            string[] f = strFormula.Split(".".ToCharArray());
            //            if (f.Length < 2) continue;
            //            DataColumnChangeEventArgs ec = new DataColumnChangeEventArgs(e.Row, e.Row.Table.Columns[f[0]], e.Row[e.Row.Table.Columns[f[0]]]);
            //            DataTable1_ColumnChanged(sender, ec);

            //        }
            //    }
            //}
        }
    }
}
