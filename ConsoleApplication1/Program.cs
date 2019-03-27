using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication1.ServiceReference1;
using GetShortCode;
namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var stri = "nha buy   6096";
            var itemm = stri.Split(' ');

            var test = new GetShortCode.Class1();
            test.Collect();

            test.Read(4);




            //var svc = new ConsoleApplication1.ServiceReference1.SMSSiteAdminProxySoapClient();
            //var login = svc.Login("numagtech@yahoo.com:Admin", "numag1234");
            //var token = login.ExtraMessage;

            //var dest= new ArrayOfString();
            //dest.Add("08088170914");

            //var message = new MessageInfo();
            //message.CallBack = "novohub";
            //message.Destination = dest;
            //message.Message = "hello guy";
            //message.MessageType= SMSTypeEnum.TEXT;



            //var sms = svc.SendSMS(token, message);
        }
      
    }
}
