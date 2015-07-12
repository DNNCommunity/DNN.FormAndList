using System.Data;

namespace DotNetNuke.Modules.UserDefinedTable.Interfaces
{
    public interface IEmailAdressSource
    {
        string GetEmailAddress(string fieldName, DataRow row);
    }
}