using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public static class DnnExtensionMethods
    {
        public static string GetSafeUsername(this UserInfo userInfo)
        {
            if (userInfo.Username == null)
                return Definition.NameOfAnonymousUser;
            else
                return userInfo.Username;
        }
        public static string GetSafeDisplayname(this UserInfo userInfo)
        {
            if (userInfo.Username == null)
                return Definition.NameOfAnonymousUser;
            else
                return userInfo.DisplayName;
        }
    }
}