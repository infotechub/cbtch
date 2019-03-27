using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.People;
namespace MrCMS.Web.Apps.Core.Services.UserChat
{
    public interface IUserChat
    {
        IList<User> Getallusers();
        bool UserOnline();

    }
}