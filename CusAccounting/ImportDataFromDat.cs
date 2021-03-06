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
    public class ImportDataFromDat
    {
        public ImportDataFromDat(DateTime tuNgay, DateTime denNgay)
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
        private void AppendToFile(string strContent)
        {
            if (File.Exists(strFileName) == false)
            {
                FileStream fstLog = File.Create(strFileName);
                fstLog.Close();
            }
            StreamWriter swrLog = File.AppendText(strFileName);

            swrLog.WriteLine(strContent);
            swrLog.Flush();
            swrLog.Close();
        }
        public bool Import()
        {
            string sqltmp;
            PackageId = int.Parse( Config.GetValue("sysPackageID").ToString());

            sqltmp = "select Package from syspackage where sysPackageid=" + PackageId.ToString();
            DataTable tbPack = _StructData.GetDataTable(sqltmp);
            strFileName = Application.StartupPath + "\\BackUp\\" + tbPack.Rows[0][0].ToString() + "_" + _Tungay.ToString("dd/MM/yy").Replace("/", "_") + "_" + _Denngay.ToString("dd/MM/yy").Replace("/", "_") ;
            if (!File.Exists(strFileName+".rar")) return false;
            try
            {
                ProcessStartInfo sf = new ProcessStartInfo("Winrar.exe");
                string fileName = tbPack.Rows[0][0].ToString() + "_" + _Tungay.ToString("dd/MM/yy").Replace("/", "_") + "_" + _Denngay.ToString("dd/MM/yy").Replace("/", "_");
                sf.Arguments = string.Format("x {0} {1} -y ", fileName + ".rar", fileName);
                sf.WorkingDirectory = Application.StartupPath + "\\BackUp";
                sf.WindowStyle = ProcessWindowStyle.Hidden;
                using (Process exeProcess = Process.Start(sf))
                {
                    exeProcess.WaitForExit();
                }
                string[] query = File.ReadAllLines(strFileName);
                string sql = "";
                string pk = "";
                string TableName = "";
                string FieldList = "";
                _Data.BeginMultiTrans();
                deleteData();
                _Data.HasErrors = false;
                List<string> lstQueryError=new List<string>();
                for (int i = 0; i < query.Length; i++)
                {

                    string sInsert="";
                    if (query[i].Substring(0, 2) == "~!")
                    {
                        //Insert dữ liệu trước đó chưa insert được, do khóa cha bị insert sau khóa chính
                        if (sql != "" && lstQueryError.Count > 0)
                        {
                            for (int j = lstQueryError.Count - 1; j >= 0; j--)
                            {
                                _Data.UpdateByNonQuery(lstQueryError[j], false);
                                if (_Data.HasErrors)
                                {
                                    _Data.HasErrors = false;
                                }
                                else
                                {
                                    lstQueryError.RemoveAt(j);
                                }
                            }
                        }
                        //Tạo query mới
                        query[i] = query[i].Remove(0, 2);
                        string[] tmp = query[i].Split(new string[] { "~!" }, StringSplitOptions.RemoveEmptyEntries);

                        if (tmp.Length < 3)
                        {
                            sql = "";
                            continue;
                        }
                        sql = "insert into " + tmp[0] + "(" + tmp[1] + ") values(@@values)";
                        pk = tmp[2];
                        TableName = tmp[0];
                        FieldList = tmp[1];
                    }
                    else
                    {
                        if (sql == "") continue;
                        if (!checkExit(TableName, FieldList, query[i], pk))
                        {
                            query[i] = query[i].Replace( ",~", ",");
                            sInsert = sql.Replace("@@values", query[i]);
                            _Data.UpdateByNonQuery(sInsert,false);

                        }

                    }
                    if (_Data.HasErrors)
                    {
                        _Data.HasErrors = false;
                        lstQueryError.Add(sInsert);

                    }
                }
                //insert lần cuối những item không import được
                for (int j = lstQueryError.Count - 1; j >= 0; j--)
                {
                    _Data.UpdateByNonQuery(lstQueryError[j], false);
                    if (_Data.HasErrors)
                    {
                        _Data.HasErrors = false;
                    }
                    else
                    {
                        lstQueryError.RemoveAt(j);
                    }
                }
                //Nếu vẫn còn những câu import ko được, báo lỗi
                if (lstQueryError.Count > 0)
                {

                    _Data.RollbackMultiTrans();
                    return false;
                }
                _Data.EndMultiTrans();
                File.Delete(strFileName);
                return true;

            }
            catch (Exception ex)
            {
                _Data.RollbackMultiTrans();
                return false;
            }
            finally
            {
                if (_Data.Connection.State != ConnectionState.Closed)
                    _Data.Connection.Close();
            }
        }

        private void deleteData()
        {
            PackageId = int.Parse(Config.GetValue("sysPackageID").ToString());
            string sqltmp = " select * from sysTable where sysPackageid=" + PackageId.ToString();
            
            DataTable SysTable = _StructData.GetDataTable(sqltmp);
            foreach (DataRow drT in SysTable.Rows)
            {
                ExecuteDelete(drT);
            }
        }

        private bool checkExit(string tableName, string fieldList, string valueList, string pk)
        {

            string[] fL = fieldList.Split(",".ToCharArray());
            fieldList = fieldList + ",";
            if (!fieldList.Contains(pk + ",")) return false;
            string[] vL = valueList.Split(new string[] { ",~" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < fL.Length; i++)
            {
                if (fL[i] == pk)
                {
                    string sql;
                    if (vL[i] == "NULL")
                        sql = "select " + pk + " from " + tableName + " where " + pk + " is " + vL[i];
                    else
                        sql = "select " + pk + " from " + tableName + " where " + pk + " = " + vL[i];
                    DataTable tb = _Data.GetDataTable(sql);
                    if (tb.Rows.Count > 0) return true;
                    else return false;
                }
            }
            return false;
        }
        List<int> Deleted = new List<int>();
        private bool ExecuteDelete(DataRow drT)
        {
            if (Deleted.Contains(int.Parse(drT["systableid"].ToString()))) return true;
            Deleted.Add(int.Parse(drT["systableid"].ToString()));
            int collectType = int.Parse(drT["CollectType"].ToString());
            if (collectType != 1 && collectType != 2 ) return true;

            DataTable Reftable = GetReftable(drT["TableName"].ToString());

            if (Reftable.Rows.Count > 0)
            {
                foreach (DataRow dr in Reftable.Rows)
                {
                    ExecuteDelete(dr);
                }
            }
            try
            {
                string sql = CreateDeleteSql(drT);
                _Data.UpdateByNonQuery(sql);
            }
            catch
            {

            }
            finally
            {
                
            }
            return true;
        }
        private DataTable GetReftable(string TableName)
        {
            string sql = "";
            sql = " select * from systable where sysPackageid=" + PackageId.ToString() + " and sysTableID in ";
            sql += "(select SysTableID from sysField where refTable='" + TableName + "')";
            return _StructData.GetDataTable(sql);
        }
        private string CreateDeleteSql(DataRow drT)
        {
            string TableName = drT["TableName"].ToString().Trim();
            string sql = "";
            DataTable listField = GetField(int.Parse(drT["sysTableId"].ToString()));
            DataRow[] exitsNgayCt = listField.Select("FieldName='ngayct'");
            int collectType = int.Parse(drT["CollectType"].ToString());
            if (collectType == 1 || collectType == 2)
            {
                if (exitsNgayCt.Length > 0)
                {
                    sql = "delete " + TableName + " where ngayct between '" + _Tungay.ToShortDateString() + "' and '" + _Denngay.ToShortDateString() + "'";
                }
                else
                {
                    string MarterTable = drT["MasterTable"].ToString().Trim();
                    sql = "select pk from systable where TableName='" + MarterTable + "' and sysPackageid=" + PackageId;
                    string MarterPk = _StructData.GetValue(sql).ToString();
                    sql = "delete " + TableName + " where " + MarterPk + " in (select " + MarterPk + " from " + MarterTable + " where ngayct between '" + _Tungay.ToShortDateString() + "' and '" + _Denngay.ToShortDateString() + "')";
                }
            }
            else if (collectType == 3)
            {
                sql = "delete " + TableName;
            }
            return sql;
        }
        private DataTable GetField(int SystableID)
        {
            string sql = "select * from sysField where systableid=" + SystableID + " and type <>3";
            return _StructData.GetDataTable(sql);
        }
    }
}
