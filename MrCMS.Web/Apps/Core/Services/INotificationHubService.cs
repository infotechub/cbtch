using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using MrCMS.Entities.People;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Core;
using MrCMS.Web.Apps.Core.Entities;
namespace MrCMS.Web.Apps.Core.Services
{
    public interface INotificationHubService
    {
        //get the connection id of the connected user if user exist
        string GetUserConnectionId(User user);


        string GetUserConnectionId(string userId);
        bool AddUsertoConnectionList(User user, string connectionId);


        bool RemoveUserfromConnectionList(User user);


        bool PushUserNotification(string destinationuser, string message, NotificationTarget target, UserRole role,
                                        NotificationType notificationType, string clickaction);
        IList<OfflineMessage> GetAllofflineMessages(string userId);
        bool MarkOfflinemessageasread(OfflineMessage msg);
        bool AddOfflineMessage(string fromId, string toId, string message, string msgDate);
        IList<UserNotification> GetNotifications();
        int GetNotificationCount();
        void MarkAllAsRead(User user);
        User GetUser(IPrincipal user);
    }
}