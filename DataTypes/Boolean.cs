using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.Localization;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Boolean"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditBoolean : EditControl
    {
        CheckBox _checkBox;

        public EditBoolean()
        {
            Init += EditBoolean_Init;
        }

        public override string Value
        {
            get
            {
                return _checkBox.Checked.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                value = value.ToLowerInvariant();
                _checkBox.Checked =
                    Convert.ToBoolean((LikeOperator.LikeString(value, "true", CompareMethod.Binary)) ||
                                      (LikeOperator.LikeString(value, "on", CompareMethod.Binary)) ||
                                      (LikeOperator.LikeString(value, "yes", CompareMethod.Binary)));
            }
        }

        protected override bool IsNull()
        {
            return (bool.FalseString == Value);
        }

        void EditBoolean_Init(object sender, EventArgs e)
        {
            _checkBox = new CheckBox();
            _checkBox.Style.Value = Style;
            _checkBox.ID = CleanID(FieldTitle);
            ValueControl = _checkBox;
            Controls.Add(_checkBox);
            if (! string.IsNullOrEmpty(OutputSettings))
            {
                Controls.Add(new LiteralControl(string.Format(" {0}", OutputSettings)));
            }

            Value = DefaultValue;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Boolean"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeBoolean : DataType
    {
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            column.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
            column.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        public override string Name
        {
            get { return "Boolean"; }
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var colBoolean = new ArrayList();
            var tableData = ds.Tables[DataSetTableName.Data];
            foreach (DataRow row in ds.Tables["Fields"].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == "Boolean")
                {
                    var title = row[FieldsTableColumn.Title].ToString();
                    colBoolean.Add(title);
                    tableData.Columns.Add(new DataColumn(title + DataTableColumn.Appendix_Original, typeof (string)));
                    tableData.Columns.Add(new DataColumn(title + DataTableColumn.Appendix_LocalizedValue,
                                                         typeof (string)));
                    tableData.Columns.Add(new DataColumn(title + DataTableColumn.Appendix_Caption, typeof (string)));
                }
            }
            if (colBoolean.Count > 0)
            {
                foreach (DataRow row in ds.Tables["Data"].Rows)
                {
                    foreach (string fieldName in colBoolean)
                    {
                        var strBoolean = (row[fieldName].ToString().ToLowerInvariant());
                        var alt = Localization.GetString(((strBoolean.AsBoolean()) ? "Yes" : "No"));
                        string strFieldvalue;
                        switch (strBoolean)
                        {
                            case "true":
                                strFieldvalue = string.Format("<img src=\"{0}/images/checked.gif\" alt=\"{1}\"/>",
                                                              Globals.ApplicationPath, alt);
                                break;
                            case "false":
                                strFieldvalue = string.Format("<img src=\"{0}/images/unchecked.gif\" alt=\"{1}\"/>",
                                                              Globals.ApplicationPath, alt);
                                break;
                            default:
                                strFieldvalue = "";
                                break;
                        }
                        row[fieldName] = strFieldvalue;
                        row[fieldName + DataTableColumn.Appendix_Original] = strBoolean;
                        row[fieldName + DataTableColumn.Appendix_Caption] = strBoolean;
                    }
                }
            }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsEditing
        {
            get { return true; }
        }

        public override bool SupportsEditStyle
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }
    }

    #endregion
}