using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Areas.Admin.Hubs;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Web.Apps.Core.Services;
using INotificationHubService = MrCMS.Web.Areas.Admin.Services.INotificationHubService;
using Newtonsoft.Json;

namespace MrCMS.Web.Apps.Core.Hub
{
    [HubName("clientNotificationHub")]

    public class ClientNotificationHub : Microsoft.AspNet.SignalR.Hub
    {
        //private readonly INotificationHubService _notificationHubService;
        private readonly Services.INotificationHubService _notificationHubUSerService;

        public ClientNotificationHub(Services.INotificationHubService notificationHubUSer)
        {
            //_notificationHubService = notificationHubService;
            _notificationHubUSerService = notificationHubUSer;
        }

        public void Send(string userid, string toUserId, string message)
        {

            //check if the recepient is online if not store as offline message

            string connectionid = _notificationHubUSerService.GetUserConnectionId(toUserId);
            if (!string.IsNullOrEmpty(connectionid))
            {
                // Call the client to update the recepient
                Clients.All.receiveMsg(userid, toUserId, message, DateTime.Now.ToString("dd MMM yy hh:mm"));
            }
            else
            {
                //message logged offline till the user connects again.
                _notificationHubUSerService.AddOfflineMessage(userid, toUserId, message,
                                                              DateTime.Now.ToString("dd MMM yy hh:mm"));


            }

        }

        public void ReturnUserOnlineStatus(string userid)
        {
            string connectionid = _notificationHubUSerService.GetUserConnectionId(userid);
            if (!string.IsNullOrEmpty(connectionid))
            {
                //Tell the client that the use is online
                GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.showuserOnline(userid,
                                                                                                               "1");
            }
            else
            {
                //Tell the client that the use is online
                GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.showuserOnline(userid,
                                                                                                               "0");
            }
        }



        public override System.Threading.Tasks.Task OnConnected()
        {
            string connectionid = Context.ConnectionId;
            MrCMS.Entities.People.User user = _notificationHubUSerService.GetUser(Context.User);

            if (user != null)
            {
                //add user to 
                _notificationHubUSerService.AddUsertoConnectionList(user, connectionid);
                //_notificationHubUSerService.PushUserNotification(user.Guid.ToString(), "hello world ,tony was here", user.Roles.Single(),
                //Enums.NotificationType.Single);

                //check if the user has offline messages 
                IList<Entities.OfflineMessage> msges = _notificationHubUSerService.GetAllofflineMessages(user.Guid.ToString());
                //send the client the username
                GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.Client(connectionid).
                    setUsername(user.Guid.ToString());
                //Tell the client that the use is online
                GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.showuserOnline(
                    user.Guid.ToString(), "1");
                if (msges.Any())
                {
                    //send the messages
                    foreach (Entities.OfflineMessage msg in msges)
                    {
                        // Call the client to update the recepient
                        Clients.All.receiveMsg(msg.FromId, msg.ToId, msg.Message, msg.Message);
                        //mark the messages as read 
                        _notificationHubUSerService.MarkOfflinemessageasread(msg);

                    }


                }



            }
            else
            {
                //probably providers
            }

            return base.OnConnected();
        }


        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string connectionid = Context.ConnectionId;
            MrCMS.Entities.People.User user = _notificationHubUSerService.GetUser(Context.User);

            if (user != null)
            {
                _notificationHubUSerService.RemoveUserfromConnectionList(user);

            }

            return base.OnDisconnected(stopCalled);
        }

    }

    public class NewNotificationAlert : INewNotificationEvent
    {
        //This is  the event listner it tells the guy that the have been an insert
        private readonly Services.INotificationHubService _notificationHubUSerService;

        public NewNotificationAlert(Services.INotificationHubService service)
        {
            _notificationHubUSerService = service;
        }

        public void Execute(NewNotificationArgs args)
        {
            //notify the system of new notification
            //Was sending to just the user client before but now am sending to all in the list 
            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.sendTransientNotification(args.Notification.UserId, "Notification", args.Notification.Message, 3);
            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.sendPersistentNotification(args.Notification.UserId);

        }

        public void Execute(VerificationCodeChangeArgs args)
        {
            //braodcast to all client to refresh the page
            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.refreshVerificationPageList();
        }

        public void Execute(AuthenticationCodeCreatedArgs args)
        {
            //braodcast to all client to refresh the page
            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.sendAuthCodeToProvider(args.AuthorizationCode.provider, args.AuthorizationCode.authorizationCode, args.AuthorizationCode.enrolleeName.ToUpper() + "(" + args.AuthorizationCode.policyNumber + ")", args.AuthorizationCode.treatmentAuthorized);
        }
        public void Execute(AuthRequestCodeCreatedArgs args)
        {
            //braodcast to all client to refresh the page
            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.sendAuthCodeToCallCenter(args.AuthRequest.providerid, args.AuthRequest.providerName, args.AuthRequest.policynumber, args.AuthRequest.fullname, args.AuthRequest.diagnosis, args.AuthRequest.reasonforcode);
        }
        public void Execute(ProviderClaimBKArgs args)
        {
            //braodcast to all client to refresh the page
            claimbkresp tonnyy = new claimbkresp();
            tonnyy.key = args.ClaimBK.clientkey;
            object test = JsonConvert.DeserializeObject(args.ClaimBK.data);
            tonnyy.data = test;


            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.pushupdatedbillToProvider(args.ClaimBK.provider.Id, tonnyy);
        }

        public void Execute(DeleteBillProviderPortalArgs args)
        {
            //braodcast to all client to refresh the page



            GlobalHost.ConnectionManager.GetHubContext<ClientNotificationHub>().Clients.All.deletebillProvider(args.providerid, args.key);
        }


    }



}