using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Web.Apps.Core;
using MrCMS.Website;
using NHibernate;
using NHibernate.Transform;

namespace MrCMS.Web.Apps.Core.Services
{
    public class NotificationHubService : INotificationHubService
    {
        public static Dictionary<string, string> UserConnectionList = new Dictionary<string, string>();
        private readonly ISession _session;

        //private readonly IUserService _userService;

        public NotificationHubService(ISession session)
        {
            //i dont need this yet but testing it out
            _session = session;
            //_userservice = userService;
            //_userService = userService;  using this service causes error in the hub
        }

        public string GetUserConnectionId(User user)
        {
            if (user != null)
            {
                if (UserConnectionList.ContainsKey(user.Guid.ToString()))
                {
                    return UserConnectionList.Single(x => x.Key == user.Guid.ToString()).Value;

                }
                else
                {
                    return string.Empty;
                }

            }
            else
            {
                return string.Empty;
            }
        }

        public string GetUserConnectionId(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                if (UserConnectionList.ContainsKey(userId))
                {
                    return UserConnectionList.Single(x => x.Key == userId).Value;

                }
                else
                {
                    return string.Empty;
                }

            }
            else
            {
                return string.Empty;
            }
        }

        public bool AddUsertoConnectionList(User user, string connectionId)
        {
            if (user != null)
            {
                if (!UserConnectionList.ContainsKey(user.Guid.ToString()))
                {

                    UserConnectionList.Add(user.Guid.ToString(), connectionId);
                    return true;

                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public bool RemoveUserfromConnectionList(User user)
        {
            if (user != null)
            {
                if (UserConnectionList.ContainsKey(user.Guid.ToString()))
                {

                    UserConnectionList.Remove(user.Guid.ToString());
                    return true;


                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }


        public bool PushUserNotification(string destinationuser, string message, NotificationTarget target, UserRole role,
                                        NotificationType notificationType, string clickaction)
        {
            //This method was discontinued.

            ////Check if its role or user 

            ////do the role maths here 
            //var usersinrole = _userservice.GetAllUsers().Where(x => x.Roles.Contains(role));

            //foreach(var user in usersinrole )
            //{
            //    //push to each of theme

            //    var notify = new UserNotification
            //    {
            //        UserId = user.Guid.ToString(),
            //        Message = message,
            //        Role = role,
            //        Target = Convert.ToInt32(target),
            //        ClickAction = clickaction,
            //        Type = Convert.ToInt32(notificationType)
            //    };

            //    _session.Transact(session => session.Save(notify));

            //    //Notify the theres a new notification
            //    var args = new NewNotificationArgs
            //    {
            //        Notification = notify
            //    };
            //    //Notify the Hub of the new Input
            // EventContext.Instance.Publish(typeof(INewNotificationEvent), args);

            //}

            return true;
        }

        public IList<OfflineMessage> GetAllofflineMessages(string userId)
        {
            IQueryOver<OfflineMessage, OfflineMessage> queryOver = _session.QueryOver<OfflineMessage>();
            return queryOver.Where(x => x.ToId == userId && x.Read == false).List<OfflineMessage>();

        }

        public bool MarkOfflinemessageasread(OfflineMessage msg)
        {
            if (msg != null)
            {
                msg.Read = true;
                _session.Transact(session => session.Update(msg));
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool AddOfflineMessage(string fromId, string toId, string message, string msgDate)
        {
            OfflineMessage offlinemsg = new OfflineMessage
            {
                FromId = fromId,
                ToId = toId,
                Message = message,
                MsgDate = msgDate,
                Read = false
            };

            _session.Transact(session => session.Save(offlinemsg));
            return true;
        }

        public IList<UserNotification> GetNotifications()
        {
            User user = CurrentRequestData.CurrentUser;

            IQueryOver<UserNotification, UserNotification> queryOver = _session.QueryOver<UserNotification>();


            if (user == null)
            {
                return new List<UserNotification>();

            }

            if (user.LastNotificationReadDate.HasValue)
                queryOver =
                    queryOver.Where(
                        notification =>
                        notification.CreatedOn >= user.LastNotificationReadDate &&
                        notification.UserId == user.Guid.ToString());

            UserNotification notificationModelAlias = null;
            IList<UserNotification> response = queryOver.SelectList(
                builder =>
                builder.Select(notification => notification.Message)
                    .WithAlias(() => notificationModelAlias.Message)
                    .Select(notification => notification.UserId)
                    .WithAlias(() => notificationModelAlias.UserId)
                      .Select(notification => notification.ClickAction)
                    .WithAlias(() => notificationModelAlias.ClickAction)
                     .Select(notification => notification.Type)
                    .WithAlias(() => notificationModelAlias.Type)
                    .Select(notification => notification.Target)
                    .WithAlias(() => notificationModelAlias.Target)
                    .Select(notification => notification.CreatedOn)
                    .WithAlias(() => notificationModelAlias.CreatedOn)).Where(notification => notification.UserId == user.Guid.ToString())
                .OrderBy(notification => notification.CreatedOn).Desc
                .TransformUsing(Transformers.AliasToBean<UserNotification>())

                .Take(15)
                .List<UserNotification>();


            return response;
        }

        public int GetNotificationCount()
        {

            User user = CurrentRequestData.CurrentUser;
            if (user != null)
            {

                IQueryOver<UserNotification, UserNotification> queryOver = _session.QueryOver<UserNotification>().Where(notification => notification.UserId == user.Guid.ToString());

                if (user.LastNotificationReadDate.HasValue)
                    queryOver = queryOver.Where(notification => notification.CreatedOn >= user.LastNotificationReadDate && notification.UserId == user.Guid.ToString());

                int count = queryOver.RowCount();
                return count;
            }
            else
            {

                return 0;
            }



        }

        public void MarkAllAsRead(User useR)
        {
            User user = GetUser(useR.Email);
            user.LastNotificationReadDate = CurrentRequestData.Now;
            _session.Transact(session => session.Update(user));
        }
        public User GetUser(IPrincipal user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Identity.Name))
                return null;
            return _session.QueryOver<User>()
                .Where(u => u.Email == user.Identity.Name)
                .Take(1)
                .Cacheable()
                .SingleOrDefault();
        }
        public User GetUser(string user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user))
                return null;
            return _session.QueryOver<User>()
                .Where(u => u.Email == user)
                .Take(1)
                .Cacheable()
                .SingleOrDefault();
        }
    }
}