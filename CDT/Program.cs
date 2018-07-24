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
            if (args.Length==0)
                args = new string[] { "CBABPM" };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.UserSkins.OfficeSkins.Register();
           // DevExpress.Skins.SkinManager.EnableFormSkins();

            //tuy theo moi soft co productName khac nhau
                string productName = "CBASFLOW"; //giá trị mặc định
                if (args.Length > 0)
                    productName = args[0];
                string H_KEY = "HKEY_CURRENT_USER\\Software\\SGD\\";
                Config.NewKeyValue("H_KEY", H_KEY);
                Config.NewKeyValue("ProductName", productName);
                string subkey = @"Software\SGD\" + productName;
                string P_KEY = H_KEY + productName + "\\";
            //lay style mac dinh cho form


            //RegisterNumber

            Config.NewKeyValue("H_KEY", P_KEY);
            RegistryKey pKey = Registry.CurrentUser.OpenSubKey(subkey);
            if (pKey == null)
            {
                Registry.CurrentUser.CreateSubKey(subkey);
                Registry.SetValue(P_KEY, "CompanyName", "SGD", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "Created", "0", RegistryValueKind.DWord);
                Registry.SetValue(P_KEY, "isDemo", "0", RegistryValueKind.DWord);
                Registry.SetValue(P_KEY, "Language", "0", RegistryValueKind.DWord);
                Registry.SetValue(P_KEY, "Package", "7", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "Password", "20-2C-B9-62-AC-59-07-5B-96-4B-07-15-2D-23-4B-70", RegistryValueKind.ExpandString);
                Registry.SetValue(P_KEY, "RegisterNumber", "", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "SavePassword", "True", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "StructDb", "SGD", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "RemoteServer", "SGD", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "Style", "Money Twins", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "SupportOnline", "SGD", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "UserName", "Admin", RegistryValueKind.String);
                Registry.SetValue(P_KEY, "isRemote", "False", RegistryValueKind.String);
            }
            
            //lay style mac dinh cho form
            string defaultStyle = Registry.GetValue(H_KEY, "Style", string.Empty).ToString();
            DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeelMain = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            if (defaultStyle != string.Empty)
                defaultLookAndFeelMain.LookAndFeel.SetSkinStyle(defaultStyle);

            //RegisterNumber

            string Company = Registry.GetValue(P_KEY, "CompanyName", "").ToString();
            CPUid Cpu = new CPUid(Company + productName + "SGDEMTOnline");
            string RegisterNumber = Registry.GetValue(P_KEY, "RegisterNumber", "").ToString();
            if (RegisterNumber != Cpu.KeyString)
            {
                
                    Config.NewKeyValue("isDemo", 1);
                    if (MessageBox.Show("Bạn đang dùng phiên bản demo, bạn có muốn đăng ký lại không?", "Thông báo", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RegisterF rf = new RegisterF();
                        rf.producName = productName;
                        rf.ShowDialog();
                        Config.NewKeyValue("isDemo", 0);
                        if (rf.DialogResult == DialogResult.Cancel) return;
                    }              
                
            }
            else
            {
                Config.NewKeyValue("isDemo", 0);
                Config.NewKeyValue("CompanyName", Company) ;
            }


            //kiem tra so lieu da duoc khoi tao chua, neu chua thuc hien khoi tao so lieu
            string created = Registry.GetValue(P_KEY, "Created", 0).ToString();
            if (created == "0")
            {
                CreateData frmCreateData = new CreateData(productName);
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
            {
                Application.Run(new Main(frmLogin.drUser, frmLogin.drPackage));
                
            }
        }

        private static void SetEnvironment()
        {
            System.Globalization.CultureInfo CultureInfo = System.Windows.Forms.Application.CurrentCulture.Clone() as System.Globalization.CultureInfo;
            CultureInfo = new CultureInfo("en-US");
            DateTimeFormatInfo dtInfo = new DateTimeFormatInfo();
            dtInfo.LongDatePattern = "MM/dd/yyyy h:mm:ss tt";
            dtInfo.ShortDatePattern = "MM/dd/yyyy";
            CultureInfo.DateTimeFormat = dtInfo;
            System.Windows.Forms.Application.CurrentCulture = CultureInfo;

            string H_KEY = Config.GetValue("H_KEY").ToString();
            //lay chuoi ket noi
            
            string isRemote = "false";
            isRemote = Registry.GetValue(H_KEY, "isRemote", "false").ToString();
            Config.NewKeyValue("isRemote", isRemote);
            //if(Boolean.Parse(isRemote))
            //    Config.NewKeyValue("StructConnection", RemoteStructConnection);
            //else
            //    Config.NewKeyValue("StructConnection", StructConnection);
            Config.NewKeyValue("StartupPath", System.Windows.Forms.Application.StartupPath);
        }
    }
}