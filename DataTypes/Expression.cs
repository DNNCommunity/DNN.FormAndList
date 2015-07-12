using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Expression" - a calculated column
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditExpression : EditControl
    {
        public override string Value
        {
            get { return string.Empty; }
            set
            {
                //do nothing
            }
        }

        string CalculateCurrentExpression()
        {
            var udt = new UserDefinedTableController(ModuleContext);
            int rowId;
            int.TryParse(HttpContext.Current.Request.QueryString[DataTableColumn.RowId], out rowId);
            var ds = udt.GetRow(rowId, true);
            DataRow currentrow;
            if (ds.Tables[DataSetTableName.Data].Rows.Count == 1)
            {
                currentrow = ds.Tables[DataSetTableName.Data].Rows[0];
            }
            else
            {
                currentrow = ds.Tables[DataSetTableName.Data].NewRow();
                ds.Tables[DataSetTableName.Data].Rows.Add(currentrow);
            }
            return currentrow[FieldTitle].ToString();
        }


        void EditExpression_Init(object sender, EventArgs e)
        {
            Control ctl;
            if (GetFieldSetting( "RenderInForm").AsBoolean( ))
            {
                //HtmlEncode is not called, as we are expecting that CalculateCurrentExpression usually returns markup.
                ctl = new LiteralControl(CalculateCurrentExpression());
            }
            else
            {
                ctl =
                    new LiteralControl(string.Format(
                        "<span class=\"Normal\" style=\"font-style: italic;\">({0})</span>",
                        HttpUtility.HtmlEncode(DefaultValue)));
            }
            Controls.Add(ctl);
        }

        public EditExpression()
        {
            Init += EditExpression_Init;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Expression"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeExpression : DataType
    {
        readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "RenderInForm", Section = "Form", SystemType = "Boolean"}
                    };

        public override IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get
            {
                return _fieldSettingTypes;
            }
        }

        public override string Name
        {
            get { return "Expression"; }
        }


        public override void SetStylesAndFormats(BoundField column, string format)
        {
            base.SetStylesAndFormats(column, format);
            if (format != string.Empty)
            {
                column.DataFormatString = string.Format("{{0:{0}}}", format);
            }
        }

        static string WarningMessage(string message, int moduleId)
        {
            message = new PortalSecurity().InputFilter(message.Replace("\'", "\'\'"), PortalSecurity.FilterFlag.NoMarkup);
            var tabId = Convert.ToInt32(- 1);
            if (HttpContext.Current != null && HttpContext.Current.Request.QueryString["tabid"] != null)
            {
                tabId = int.Parse(HttpContext.Current.Request.QueryString["tabid"]);
            }
            if (tabId > - 1 && new ModuleSecurity(moduleId, tabId).IsAllowedToAdministrateModule())
            {
                return string.Format("\'<span style=\"COLOR: red\">{0}</span>\'", message);
            }
            return string.Format("\'<span style=\"COLOR: red\">{0}</span>\'", "Expression failed");
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var tokenReplace = new TokenReplace {ModuleId = moduleId};
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int)row[FieldsTableColumn.Id];
                    var columnName = row[FieldsTableColumn.Title].ToString();
                    try
                    {
                        var typestring = row[FieldsTableColumn.InputSettings].AsString();
                        var expression = tokenReplace.ReplaceEnvironmentTokens(
                             row[FieldsTableColumn.Default].AsString("String"));

                        ds.Tables[DataSetTableName.Data].Columns.Remove(columnName);
                        var dc = new DataColumn(columnName, GetType(typestring));
                        ds.Tables[DataSetTableName.Data].Columns.Add(dc);
                        dc.Expression = expression;
                    }
                    catch (SyntaxErrorException ex)
                    {
                        ds.Tables[DataSetTableName.Data].Columns.Remove(columnName);
                        var dc = new DataColumn(columnName, typeof (string));
                        ds.Tables[DataSetTableName.Data].Columns.Add(dc);
                        ds.Tables[DataSetTableName.Data].Columns[columnName].Expression =
                            WarningMessage(ex.Message, moduleId);
                    }
                    catch (EvaluateException ex)
                    {
                        ds.Tables[DataSetTableName.Data].Columns.Remove(columnName);
                        var dc = new DataColumn(columnName, typeof (string));
                        ds.Tables[DataSetTableName.Data].Columns.Add(dc);
                        ds.Tables[DataSetTableName.Data].Columns[columnName].Expression =
                            WarningMessage(ex.Message, moduleId);
                    }
                    catch (ArgumentNullException ex)
                    {
                        var dc = new DataColumn(columnName, typeof (string));
                        ds.Tables[DataSetTableName.Data].Columns.Add(dc);
                        ds.Tables[DataSetTableName.Data].Columns[columnName].Expression =
                            WarningMessage(ex.Message, moduleId);
                    }
                }
            }
        }

        static Type GetType(string typestring)
        {
            return Type.GetType(("System." + typestring));
        }

        public override IDictionary InputSettingsList
        {
            get { return ListOfCommonDataTypes; }
        }

        public override bool SupportsInputSettings
        {
            get { return true; }
        }

        public override bool InputSettingsIsValueList
        {
            get { return false; }
        }

        public override string InputSettingDefault
        {
            get { return "String"; }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }

        public override bool SupportsHideOnEdit
        {
            get { return true; }
        }

        public override bool SupportsSearch
        {
            get { return true; }
        }
    }

    #endregion
}