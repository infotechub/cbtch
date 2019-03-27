using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsEngine.ServiceReference1;
using System.IO.Compression;
using System.Net.Http;
using Newtonsoft.Json;

namespace SmsEngine
{
    public class SmsProxy
    {
     

        private  static string _username;
        private static  string _password;
        private static string _token;
        public static ServiceReference1.SMSSiteAdminProxySoapClient Svc;
       public SmsProxy(string username,string password)
       {
           //_username = username;
           //_password = password;
           //var svc = new ServiceReference1.SMSSiteAdminProxySoapClient();
      
           //Svc = svc;
           //var resp = Svc.Login(username, password);
           //_token = resp.ExtraMessage;
      
       }
        public static ResponseInfo SendSmsold(string fromId, List<string> destphoneNumbers,string message ,DateTime deliveryDate )
        {
            
            var dest = new ArrayOfString();
            dest.AddRange(destphoneNumbers);
            var messageinfo = new MessageInfo();
            messageinfo.CallBack = fromId ;
            messageinfo.Destination = dest;
            messageinfo.Message = message;
            messageinfo.MessageType = SMSTypeEnum.TEXT;
            //messageinfo.DeliveryEmail = deliveryDate.AddMonths(-1).ToString("DD MMM yyyy hh:mm tt");
            var sms = Svc.SendSMS(_token, messageinfo);
        
            return sms;
        }

        //private static void Main(string[] args)
        //{
        //    var tony = new List<string>() { "08088170914" };
          
        //    var resp = SendSms("novohealth", tony, "Hello",DateTime.Now);


        //}
        public static string  Login(string ownerEmail,string subacc,string password)
        {
            string resultContent = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://www.smslive247.com");
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("cmd", "login"),
                 new KeyValuePair<string, string>("owneremail", ownerEmail),
                  new KeyValuePair<string, string>("subacct", subacc),
                   new KeyValuePair<string, string>("subacctpwd", password)
            });
                var result = client.PostAsync("/http/index.aspx", content).Result;
              resultContent  = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(resultContent);
            }

            return resultContent;
        }
      
        public static ResponseInfo SendSms(string fromId, List<string> destphoneNumbers, string message, DateTime deliveryDate)
        {
            string resultContent = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://www.smslive247.com");

                
               
                var content = new[]
                {
                new KeyValuePair<string, string>("cmd", "sendmsg"),
                 new KeyValuePair<string, string>("sessionid", "ea9efd09-8be7-424c-829d-0adde2df7d1e"),
                  new KeyValuePair<string, string>("message",message),
                   new KeyValuePair<string, string>("sender", fromId),
                    new KeyValuePair<string, string>("sendto", destphoneNumbers[0] ),
                     new KeyValuePair<string, string>("msgtype", "0")
            };
                StringBuilder stringBuilder = new StringBuilder();
                foreach (KeyValuePair<string, string> current in content)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append('&');
                    }

                    stringBuilder.Append(current.Key);
                    stringBuilder.Append('=');
                    stringBuilder.Append(current.Value);
                }


                var jsonString = JsonConvert.SerializeObject(content);
                var contentmin = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = client.PostAsync("/http/index.aspx", contentmin).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(resultContent);
            }
            return new ResponseInfo()
            {
                ExtraMessage = resultContent
            };
            
        }
    }
}
