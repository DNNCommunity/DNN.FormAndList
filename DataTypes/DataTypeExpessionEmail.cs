using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.Services.Mail;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{
    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "DataTypeExpessionEmail"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeExpessionEmail : DataTypeExpression, IEmailAdressSource
    {
        public string GetEmailAddress(string fieldName, DataRow row)
        {
            return row[fieldName].AsString();
        }
    }

    #endregion
}