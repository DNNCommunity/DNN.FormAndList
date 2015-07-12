using System;

namespace DotNetNuke.Modules.UserDefinedTable.Interfaces
{
    public interface IFormEvents
    {
        event Action RecordUpdated ;
        event Action RecordDeleted ; 
    }
}