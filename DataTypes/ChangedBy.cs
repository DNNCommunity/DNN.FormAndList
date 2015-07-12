namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "ChangedBy"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditChangedBy : EditCreatedBy
    {
        public override string Value
        {
            get { return CurrentUserName(); }
            set { base.Value = value; }
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "ChangedBy"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeChangedBy : DataTypeCreatedBy
    {
        public override string Name
        {
            get { return "ChangedBy"; }
        }
    }

    #endregion
}