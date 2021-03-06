using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTLib;
using CDTDatabase;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
namespace CusAccounting
{
   public class ExportData2Dat
    {
        public ExportData2Dat(DateTime tuNgay, DateTime denNgay)
        {
            _Tungay = tuNgay;
            _Denngay = denNgay;
        }
        private DateTime _Tungay;
        private DateTime _Denngay;
        private Database _StructData = Database.NewStructDatabase();
        private Database _Data = Database.NewDataDatabase();
        private int PackageId;
        FileStream fstLog;
        string strFileName;
       StreamWriter swrLog;
        public bool Export()
        {
            PackageId = int.Parse(Config.GetValue("sysPackageID").ToString());
            string sql =  " select * from sysTable where sysPackageid=" + PackageId.ToString();
            DataTable SysTable = _StructData.GetDataTable(sql);
            //DataRow[] exitsNgayCt = SysTable.Select("TableName='dmkh'");
            Colected.Clear();
            sql = "select Package from syspackage where sysPackageid=" + PackageId.ToString();
            DataTable tbPack = _StructData.GetDataTable(sql);
            if (tbPack.Rows.Count == 0) return false;
            strFileName =Application.StartupPath+"\\BackUp\\" +  tbPack.Rows[0][0].ToString() + "_" + _Tungay.ToString("dd/MM/yy").Replace("/", "_") + "_" + _Denngay.ToString("dd/MM/yy").Replace("/", "_" ) ;
            fstLog = File.Create(strFileName);
            fstLog.Close();
            swrLog = File.AppendText(strFileName);
            try
            {
                
                _Data.BeginMultiTrans();

                foreach (DataRow drT in SysTable.Rows)
                {
                    ExecuteCollect(drT);
                    if (_Data.HasErrors)
                    {
                        _Data.RollbackMultiTrans();
                        fstLog.Close();
                        return false;
                    }
                }
                fstLog.Close();
                _Data.EndMultiTrans();
                swrLog.Dispose();
                ProcessStartInfo sf = new ProcessStartInfo("Winrar.exe");
                string fileName = tbPack.Rows[0][0].ToString() + "_" + _Tungay.ToString("dd/MM/yy").Replace("/", "_") + "_" + _Denngay.ToString("dd/MM/yy").Replace("/", "_");
                sf.Arguments = string.Format("a {0} {1} -r ", fileName + ".rar", fileName);
                sf.WorkingDirectory = Application.StartupPath + "\\BackUp";
                sf.WindowStyle = ProcessWindowStyle.Hidden;
               // Process.Start(sf);
                
                using (Process exeProcess = Process.Start(sf))
                {
                    exeProcess.WaitForExit();
                }
                File.Delete(strFileName);
            }
            catch(Exception ex)
            {
                fstLog.Close();
                _Data.RollbackMultiTrans();
                return false;
            }
            finally
            {
                if (_Data.Connection.State != ConnectionState.Closed)
                    _Data.Connection.Close();
            }
            return true;
        }

        List<int> Colected = new List<int>();
        private bool ExecuteCollect(DataRow drT)
        {
            if (Colected.Contains(int.Parse(drT["systableid"].ToString())))
                return true;
            Colected.Add(int.Parse(drT["systableid"].ToString()));
            DataTable Reftable = GetKeytable(int.Parse(drT["sysTableId"].ToString()));
            if (int.Parse(drT["CollectType"].ToString()) > 3) return true;
            if (Reftable.Rows.Count > 0)
            {

                foreach (DataRow dr in Reftable.Rows)
                {

                    ExecuteCollect(dr);
                }
            }
                string[] sql = GenInsertQuery(drT);
                
                DataTable tbData;
                if (sql.Length == 2)
                {
                    //AppendToFile(sql[1]);
                    tbData = _Data.GetDataTable(sql[0]);
                    
                }
                else
                {
                    return false;
                }
                if (tbData != null)
                {
                    if (tbData.Rows.Count > 0)
                    {
                        //AppendToFile(sql[1]);
                        swrLog.WriteLine(sql[1]);
                        swrLog.Flush();
                    }
                    string dataString;
                    foreach (DataRow dr in tbData.Rows)
                    {
                        dataString = GenDataQuery(dr);
                       // AppendToFile(dataString);
                        swrLog.WriteLine(dataString);
                        swrLog.Flush();
                    }
                }
            
            return true;
        }
        private DataTable GetKeytable(int sysTableID)
        {
            string sql = "";
            sql = " select * from systable where sysPackageid=" + PackageId.ToString() + " and TableName in ";
            sql += "(select RefTable from sysField where systableId=" + sysTableID + " and RefTable is not null)";
            return _StructData.GetDataTable(sql);
        }
        private string[] GenInsertQuery(DataRow drT)
        {
            //trừ kiểu số nguyên tự tăng
            string TableName = drT["TableName"].ToString().Trim();
            int collectType = int.Parse(drT["CollectType"].ToString());
            int TableID = int.Parse(drT["sysTableId"].ToString());
            string pk = drT["Pk"].ToString();
            string ListField = GetFieldString(TableID);
            string sql;
            string select="";
            string insert;
            if (collectType == 1 || collectType == 2)
            {

                DataTable listField = GetField(int.Parse(drT["sysTableId"].ToString()));
                DataRow[] exitsNgayCt = listField.Select("FieldName='NgayCt'");
                if (exitsNgayCt.Length > 0)
                {
                    select = "select " + ListField + " from " + TableName + " where ngayCt between '" + _Tungay.ToShortDateString() + "' and '" + _Denngay.ToShortDateString() + "'";
                }
                else
                {
                    string MasterTable = drT["MasterTable"].ToString().Trim();
                    //lấy trường khóa
                    
                    sql = "select pk from systable where TableName='" + MasterTable + "' and sysPackageid=" + PackageId;
                    
                    string MasterPk = _StructData.GetValue(sql).ToString();
                    DataRow[] lstFkField = listField.Select("refTable='" + MasterTable + "'");
                    string fkField=MasterPk;
                    if (lstFkField.Length > 0) fkField= lstFkField[0]["fieldName"].ToString(); 
                    select = "select " + ListField + " from " + TableName;
                    select += " where " + fkField+ " in (select " + MasterPk + " from " + MasterTable + " where  ngayCt between '" + _Tungay.ToShortDateString() + "' and '" + _Denngay.ToShortDateString() + "')";

                }
            }
            else if (collectType == 0)
            {
                select = "select " + ListField + " from " + TableName;
            }
            else
            {
                return new string[] { };
            }
            insert = "~!" + TableName + "~!" + ListField + "~!" + pk;
            return new string[] { select, insert };
        }
        private string GenDataQuery(DataRow dr)
        {
            string sData = "";
            foreach (DataColumn col in dr.Table.Columns)
            {
                if (dr[col] == DBNull.Value)
                {
                    sData += "NULL,~";
                    continue;
                }
                if (col.DataType == typeof(Guid))
                {
                    sData += "'" + dr[col].ToString() + "',~";
                }
                else if (col.DataType == typeof(DateTime))
                {
                    sData += "'" + dr[col].ToString() + "',~";
                }
                else if (col.DataType == typeof(string))
                {
                    sData += "N'" + dr[col].ToString().Replace("\n","@Enter") + "',~";
                }
                else if (col.DataType == typeof(bool))
                {
                    if (dr[col].ToString().ToLower() == "false")
                        sData += "0,~";
                    else
                        sData += "1,~";
                }
                else
                {
                    sData += dr[col].ToString().Replace(",", ".") + ",~";
                }
            }
            sData = sData.Substring(0, sData.Length - 2);
            return sData;
        }
        private string GetFieldString(int SystableID)
        {
            string sql = "";
            DataTable listField = GetField(SystableID);
            foreach (DataRow dr in listField.Rows)
            {
                sql += dr["FieldName"].ToString().Trim() + ",";
            }
           sql = sql.Substring(0, sql.Length - 1);
            return sql;
        }
        private DataTable GetField(int SystableID)
        {
            string sql = "select * from sysField where systableid=" + SystableID + " and type <>3";
            return _StructData.GetDataTable(sql);
        }

    }
}
