using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Web.Apps.Core.Services.Widgets;
using System.Web;
namespace MrCMS.Web.Apps.Core.Services
{
    public class PageMessageSvc : IPageMessageSvc
    {
        private readonly HttpSessionStateBase _websession;
        public PageMessageSvc(HttpSessionStateBase session)
        {
            _websession = session;
        }
        public bool SetErrormessage(string msg)
        {

            _websession["PageErrorMessage"] = msg;
            return true;
        }

        public bool SetSuccessMessage(string msg)
        {
            _websession["PageSuccessMessage"] = msg;
            return true;
        }

        public bool ClearErrormessage()
        {
            _websession["PageErrorMessage"] = string.Empty;
            return true;
        }

        public bool ClearSuccessmessage()
        {
            _websession["PageSuccessMessage"] = string.Empty;
            return true;
        }
    }
}