using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using CDTDatabase;

namespace ScriptGeneration
{
    class PackageAdded
    {
        string _package = string.Empty;
        string _sysPackageID = string.Empty;
        Database _dbSource;

        public PackageAdded(string cnnSource, string package)
        {
            _dbSource = Database.NewCustomDatabase(cnnSource);
            _package = package;
        }

        public string GenScript()
        {
            string sql = "use CDT \r\n";
            sql += PackageScript();
            sql += TableScript();
            sql += FieldScript();
            sql += DataConfigScript();
            sql += DataConfigDtScript();
            sql += ReportScript();
            sql += MenuScript();
            sql += ReportFilterScript();
            sql += FormReportScript();
            sql += ConfigScript();
            return sql;
        }

        private string GenInsert(string tableName, DataTable dtData)
        {
            string tmp = "SET IDENTITY_INSERT " + tableName + " ON \r\n";
            foreach (DataRow dr in dtData.Rows)
            {
                string tmp1 = "insert into " + tableName + "(";
                string tmp2 = " values(";
                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    if (dtData.Columns[i].DataType == typeof(System.Byte[]) || dr[i].ToString() == string.Empty)
                    {
                        if (i == dtData.Columns.Count - 1)
                        {
                            if (tmp1[tmp1.Length - 1] == ',')
                                tmp1 = tmp1.Substring(0, tmp1.Length - 1);
                            if (tmp2[tmp2.Length - 1] == ',')
                                tmp2 = tmp2.Substring(0, tmp2.Length - 1);
                            tmp1 += ")";
                            tmp2 += ")";
                        }
                        if (dtData.Columns[i].DataType == typeof(System.Byte[]) && dr[i].ToString() != string.Empty)
                        {
                            string path = _package + "_" + tableName + "_" + dtData.Columns[i].ColumnName + "_" + dtData.Columns[0].ColumnName;
                            if (!System.IO.Directory.Exists(path))
                                System.IO.Directory.CreateDirectory(path);
                            System.IO.File.WriteAllBytes(path + "\\" + dr[0].ToString() + ".png", dr[i] as byte[]);
                        }
                        continue;
                    }
                    tmp1 += dtData.Columns[i].ColumnName;
                    if (dtData.Columns[i].DataType != typeof(System.Boolean))
                    {
                        if (dtData.Columns[i].DataType == typeof(System.String))
                            tmp2 += "N";
                        tmp2 += "'" + dr[i].ToString().Replace("'", "''") + "'";
                    }
                    else
                        tmp2 += "'" + (Boolean.Parse(dr[i].ToString()) ? "1" : "0") + "'";
                    if (i == dtData.Columns.Count - 1)
                    {
                        tmp1 += ")";
                        tmp2 += ")";
                    }
                    else
                    {
                        tmp1 += ",";
                        tmp2 += ",";
                    }
                }
                tmp += tmp1 + tmp2 + "\r\n";
            }
            tmp += "SET IDENTITY_INSERT " + tableName + " OFF \r\n";
            return tmp;
        }

        private string PackageScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysPackage" +
                " where Package = '" + _package + "'";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                tmp = GenInsert("sysPackage", dt);
                _sysPackageID = dt.Rows[0][0].ToString();
            }
            return tmp;
        }

        private string TableScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysTable " +
                " where sysPackageID = " + _sysPackageID;
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysTable", dt);
            return tmp;
        }

        private string FieldScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysField " +
                " where systableid in (select systableid from systable where syspackageid = " + _sysPackageID + ")";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysField", dt);
            return tmp;
        }

        private string DataConfigScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysDataConfig " +
                " where systableid in (select systableid from systable where syspackageid = " + _sysPackageID + ")";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysDataConfig", dt);
            return tmp;
        }

        private string DataConfigDtScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysDataConfigDt " +
                " where BlConfigID in (select BlConfigID from sysDataConfig where systableid in (select systableid from systable where syspackageid = " + _sysPackageID + "))";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysDataConfigDt", dt);
            return tmp;
        }

        private string ReportScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysReport " +
                " where syspackageid = " + _sysPackageID + " order by sysReportParentID";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysReport", dt);
            return tmp;
        }

        private string MenuScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysMenu " +
                " where syspackageid = " + _sysPackageID + " order by sysTableID, sysReportID, sysMenuParent";
            if (_package == "CDT")
                sql = "select * from sysMenu " +
                " where syspackageID is null or syspackageid = " + _sysPackageID + " order by sysTableID, sysReportID, sysMenuParent";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysMenu", dt);
            return tmp;
        }

        private string ReportFilterScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysReportFilter " +
                " where sysReportID in (select sysReportID from sysReport where sysPackageid = " + _sysPackageID + ")";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysReportFilter", dt);
            return tmp;
        }

        private string FormReportScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysFormReport " +
                " where sysReportID in (select sysReportID from sysReport where sysPackageid = " + _sysPackageID + ")";
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysFormReport", dt);
            return tmp;
        }

        private string ConfigScript()
        {
            string tmp = string.Empty;
            string sql = "select * from sysConfig " +
                " where sysPackageid = " + _sysPackageID;
            DataTable dt = _dbSource.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
                tmp = GenInsert("sysConfig", dt);
            return tmp;
        }
    }
}
