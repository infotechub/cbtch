using System;
using System.Linq;
using MrCMS.Entities.People;
using MrCMS.Services;

namespace MrCMS.Membership.Internals
{
    public class RoleProvider : IRoleProvider
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public RoleProvider(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public bool IsUserInRole(string username, string roleName)
        {
            User user = _userService.GetUserByEmail(username);
            if (user != null)
            {
                if (user.Roles.Any(x => x.Name == roleName))
                    return true;
            }
            return false;
        }

        public string[] GetRolesForUser(string username)
        {
            User user = _userService.GetUserByEmail(username);

            return user == null || !user.Roles.Any() ? new string[0] : user.Roles.Select(rolename => rolename.Name).ToArray();
        }

        public void CreateRole(string roleName)
        {
            UserRole role = _roleService.GetRoleByName(roleName);
            if (role != null)
            {
                _roleService.SaveRole(new UserRole { Name = roleName });
            }
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            UserRole role = _roleService.GetRoleByName(roleName);
            if (throwOnPopulatedRole && role.Users.Any())
                throw new Exception("Role " + roleName + " is still populated");

            foreach (User user in role.Users)
                user.Roles.Remove(role);

            _roleService.DeleteRole(role);

            return true;
        }

        public bool RoleExists(string roleName)
        {
            UserRole role = _roleService.GetRoleByName(roleName);
            return (role != null);
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (User user in usernames.Select(username => _userService.GetUserByEmail(username)))
            {
                User thisUser = user;
                foreach (UserRole role in roleNames.Select(roleName => _roleService.GetRoleByName(roleName)).Where(role => !thisUser.Roles.Contains(role)))
                {
                    user.Roles.Add(role);
                    role.Users.Add(user);
                }
                _userService.SaveUser(user);
            }
        }

        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (User user in usernames.Select(username => _userService.GetUserByEmail(username)))
            {
                User thisUser = user;
                foreach (UserRole role in roleNames.Select(roleName => _roleService.GetRoleByName(roleName)).Where(role => thisUser.Roles.Contains(role)))
                {
                    user.Roles.Remove(role);
                    role.Users.Remove(user);
                }
                _userService.SaveUser(user);
            }
        }

        public string[] GetUsersInRole(string roleName)
        {
            UserRole roleByName = _roleService.GetRoleByName(roleName);
            return roleByName == null
                       ? new string[0]
                       : roleByName.Users.Select(user => user.Email).ToArray();
        }

        public string[] GetAllRoles()
        {
            return _roleService.GetAllRoles().Select(role => role.Name).ToArray();
        }

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            UserRole roleByName = _roleService.GetRoleByName(roleName);
            return roleByName == null
                       ? new string[0]
                       : roleByName.Users.Where(user => user.Email.Contains(usernameToMatch))
                                   .Select(user => user.Email).ToArray();
        }
    }
}