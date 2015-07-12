using System.Collections.Generic;
using System.Data;
using DotNetNuke.Modules.UserDefinedTable.Components;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   DataType "Separator"
    /// </summary>
    /// <remarks>
    ///   No EditSeparator required
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DataTypeSeparator : DataType
    {
        public override string Name
        {
            get { return "Separator"; }
        }

        readonly FieldSettingType[] _fieldSettingTypes = new[]
        {
            new FieldSettingType {Key = "IsCollapsible", Section = "List", SystemType = "Boolean"}
        };
        public override IEnumerable<FieldSettingType> FieldSettingTypes { get { return _fieldSettingTypes; }}
        

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                //it never shows up
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    row[FieldsTableColumn.Visible] = false;
                }
            }
        }

        public override bool IsSeparator
        {
            get { return (true); }
        }
    }
}