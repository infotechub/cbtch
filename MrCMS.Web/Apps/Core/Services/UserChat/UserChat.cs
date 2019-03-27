using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Services;
namespace MrCMS.Web.Apps.Core.Services.UserChat
{
    public class UserChat : IUserChat
    {
        private readonly IUserService _userservice;
        public UserChat(IUserService service)
        {
            _userservice = service;
        }
        public IList<User> Getallusers()
        {
            return _userservice.GetAllUsers();
        }

        public bool UserOnline()
        {
            return true;
        }
    }
}