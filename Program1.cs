using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using CDTLib;
using CDTSystem;
using CDTControl.CDTControl;

namespace CDT
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.UserSkins.OfficeSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();

            //tuy theo moi soft co productName khac nhau
            string productName = "CDT"; //giá trị mặc định
            if (args.Length > 0)
                productName = args[0];
            string H_KEY = "HKEY_CURRENT_USER\\Software\\Combosoft\\";
            Config.NewKeyValue("H_KEY", H_KEY);
            Config.NewKeyValue("ProductName", productName);
            
            //lay style mac dinh cho form
            string defaultStyle = Registry.GetValue(H_KEY, "Style", string.Empty).ToString();
            DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeelMain = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            if (defaultStyle != string.Empty)
                defaultLookAndFeelMain.LookAndFeel.SetSkinStyle(defaultStyle);

            //RegisterNumber
            string P_KEY = H_KEY + productName + "\\";
            string Company = Registry.GetValue(P_KEY, "CompanyName", "").ToString();
            CPUid Cpu = new CPUid(Company + productName);
            string RegisterNumber = Registry.GetValue(P_KEY, "RegisterNumber", "").ToString();
            if (RegisterNumber != Cpu.KeyString)
            {
                RegisterF rf = new RegisterF();
                rf.producName = productName;
                rf.ShowDialog();
                if (rf.DialogResult == DialogResult.Cancel)
                    return;
            }


            //kiem tra so lieu da duoc khoi tao chua, neu chua thuc hien khoi tao so lieu
            string created = Registry.GetValue(P_KEY, "Created", 0).ToString();
            if (created == "0")
            {
                CreateData frmCreateData = new CreateData();
                frmCreateData.ShowDialog();
                if (frmCreateData.DialogResult == DialogResult.Cancel)
                    return;
                Registry.SetValue(P_KEY, "Created", 1);
            }
            //da co so lieu, bat dau thuc hien dang nhap
            SetEnvironment();
            Login frmLogin = new Login();
            frmLogin.ShowDialog();

            //dang nhap thanh cong, bat dau su dung chuong trinh
            if (frmLogin.DialogResult != DialogResult.Cancel)
                Application.Run(new Main(frmLogin.drUser, frmLogin.drPackage));
        }

        private static void SetEnvironment()
        {
            System.Globalization.CultureInfo CultureInfo = System.Windows.Forms.Application.CurrentCulture.Clone() as System.Globalization.CultureInfo;
            CultureInfo = new CultureInfo("en-US");
            System.Windows.Forms.Application.CurrentCulture = CultureInfo;

            string H_KEY = Config.GetValue("H_KEY").ToString();
            //lay chuoi ket noi
            string StructConnection = Registry.GetValue(H_KEY, "StructDb", string.Empty).ToString();
            Config.NewKeyValue("StructConnection", StructConnection);

            //lay ten cong ty
            string TenCongTy = Registry.GetValue(H_KEY, "CompanyName", string.Empty).ToString();
            Config.NewKeyValue("TenCongTy", TenCongTy);
        }
    }
}