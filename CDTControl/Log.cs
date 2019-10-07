using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CDTControl
{
    public class Log
    {
        public string log(string companyName, string Product, string code, string _user, string _pass)
        {
            try
            {
                com.sgdsoft.Service a = new com.sgdsoft.Service();
                string k = a.GetKeyDirect(companyName, Product, code, _user, _pass);
                return k;
            }
            catch (Exception ex)
            {
                return "Lỗi";
            }
        }
        public bool Check(string user, string pass)
        {
            try
            {
                //Comboserver.Service a = new Comboserver.Service();
                com.sgdsoft.Service a = new com.sgdsoft.Service();
                bool k = a.CheckUserLogin(user, pass);
                return k;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string logFb(string ob)
        {
            string url = @"https://www.phanmemsgd.com/api/UserKeys";

            string sContentType = "application/json";

            HttpContent s = new StringContent(ob,Encoding.UTF8, sContentType);

            HttpClient oHttpClient = new HttpClient();
            try
            {
                var oTaskPostAsync = oHttpClient.PostAsync(url, s);

                if (oTaskPostAsync.Result.StatusCode == HttpStatusCode.BadRequest) return "";
                else if (oTaskPostAsync.Result.StatusCode == HttpStatusCode.OK)
                {
                    string s1 = oTaskPostAsync.Result.Content.ReadAsStringAsync().ConfigureAwait(true).GetAwaiter().GetResult() ;
                    return s1;
                }
                else
                { return ""; }
            }
            catch(Exception ex)
            {
                return "";
            }
            return "";
            


        }
    }
}
