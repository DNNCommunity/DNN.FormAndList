 using System;
using System.Web.UI;
 using DotNetNuke.Services.SystemDateTime;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "CreatedAt"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditCreatedAt : EditControl
    {
        const string TimeStamp = "TimeStamp";

        public EditCreatedAt()
        {
            Load += EditCreatedAt_Load;
        }

        public override string Value
        {
            get
            {
                if (ViewState[TimeStamp] == null)
                {
                    return Common.Utilities.DateUtils.GetDatabaseTime().ToString("s");
                }
                return DateTime.Parse((string)ViewState[TimeStamp]).ToString("s");
            }
            set
            {
                if (value != string.Empty)
                {
                    ViewState[TimeStamp] = value;
                    Controls.Add(
                        new LiteralControl(string.Format("<span class=\"Normal\">{0:g}</span>", ServerTime(value))));
                }
            }
        }


        void EditCreatedAt_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                if (ViewState[TimeStamp]!=null)
                {
                    Controls.Add(
                        new LiteralControl(string.Format("<span class=\"Normal\">{0}</span>", ServerTime((string)ViewState[TimeStamp]))));
                }
            }
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "CreatedAt"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeCreatedAt : DataTypeDateTime
    {
        public override string Name
        {
            get { return "CreatedAt"; }
        }

        public override bool IsUserDefinedField
        {
            get { return false; }
        }

        public override bool SupportsDefaultValue
        {
            get { return false; }
        }
        public override bool SupportsEditStyle
        {
            get { return false; }
        }

        public override bool SupportsInputSettings
        {
            get { return false; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }

        public override bool SupportsValidation
        {
            get { return false; }
        }

        public override bool SupportsHideOnEdit
        {
            get { return true; }
        }
    }

    #endregion
}