using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Areas.Admin.Hubs;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Web.Apps.Core.Services;
using INotificationHubService = MrCMS.Web.Areas.Admin.Services.INotificationHubService;

namespace MrCMS.Web.Apps.Core.Hub
{
    public class ClientHub :Microsoft.AspNet.SignalR.Hub
    {

        public override System.Threading.Tasks.Task OnConnected()
        {
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }
}