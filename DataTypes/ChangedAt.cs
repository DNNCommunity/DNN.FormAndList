using DotNetNuke.Services.SystemDateTime;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "ChangedAt"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditChangedAt : EditCreatedAt
    {
        public override string Value
        {
            get { return Common.Utilities.DateUtils.GetDatabaseTime().ToString("s"); }
            
            set { base.Value = value; }
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "ChangedAt"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeChangedAt : DataTypeDateTime
    {
        public override string Name
        {
            get { return "ChangedAt"; }
        }

        public override bool SupportsEditStyle
        {
            get { return false; }
        }

        public override bool IsUserDefinedField
        {
            get { return false; }
        }

        public override bool SupportsDefaultValue
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

        public override bool SupportsHideOnEdit
        {
            get { return true; }
        }

        public override bool SupportsValidation
        {
            get { return false; }
        }
    }

    #endregion
}