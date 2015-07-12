using System.Data;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.UserDefinedTable.Interfaces
{
    public interface IUserSource
    {
        UserInfo GetUser(string fieldName, DataRow row);
    }
}