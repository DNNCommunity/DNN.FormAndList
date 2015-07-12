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

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "EMail"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditEMail : EditString
    {
        protected override bool IsValidType()
        {
            if (Value == string.Empty)
            {
                return true;
            }
            if (StrValRule == string.Empty)
            {
                return Mail.IsValidEmailAddress(Value, PortalId);
            }
            return true;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Email"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeEMail : DataType, IEmailAdressSource
    {
        readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "NoLink", Section = "List", SystemType = "Boolean"}
                    };

        public override IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get
            {
                return _fieldSettingTypes;
            }
        }
        
        struct FieldSetting
        {
            public string Title;
            public string OutputFormat;
            public bool AsLink;
        }


        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var fields = new ArrayList();
            //List of columns that contains eMail addresses
            var tableData = ds.Tables[DataSetTableName.Data];
            var tokenReplace = new TokenReplace {ModuleId = moduleId};
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int)row[FieldsTableColumn.Id];
                    var field = new FieldSetting
                                    {
                                        Title = row[FieldsTableColumn.Title].ToString(),
                                        OutputFormat = row[FieldsTableColumn.OutputSettings].AsString(),
                                        AsLink = !GetFieldSetting("NoLink", fieldId, ds).AsBoolean()
                                    };
                    fields.Add(field);
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Original,
                                                         typeof (string)));
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Caption, typeof (string)));
                }
            }
            if (fields.Count > 0)
            {
                foreach (DataRow row in tableData.Rows)
                {
                    foreach (FieldSetting field in fields)
                    {
                        //Link shown to the user
                        //Link readable by browsers
                        var strUrl = row[field.Title].ToString().Trim();
                        //strip optional parameter like subject or body for display:
                        var strLink = strUrl.IndexOf("?", System.StringComparison.Ordinal) != - 1 ? strUrl.Substring(0, strUrl.IndexOf("?", System.StringComparison.Ordinal)) : strUrl;

                        if (strLink != string.Empty)
                        {
                            var strCaption = field.OutputFormat;
                            if (! string.IsNullOrEmpty(strCaption))
                            {
                                strCaption = string.Format(tokenReplace.ReplaceEnvironmentTokens(strCaption, row),
                                                           strLink);
                            }
                            else
                            {
                                strCaption = strLink;
                            }

                            string strFieldvalue;
                            if (strCaption != string.Empty && field.AsLink )
                            {
                                strFieldvalue = string.Format("<a href=\"mailto:{0}\">{1}</a>",
                                                              HttpUtility.UrlEncode(strUrl), strCaption);
                            }
                            else
                            {
                                strFieldvalue = strLink;
                            }

                            row[field.Title] = noScript ? strFieldvalue : (Globals.CloakText(strFieldvalue));
                            row[field.Title + DataTableColumn.Appendix_Caption] = strCaption;
                            row[field.Title + DataTableColumn.Appendix_Original] = strUrl;
                        }
                    }
                }
            }
        }

        public override string Name
        {
            get { return "EMail"; }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsEditing
        {
            get { return true; }
        }

        public override bool SupportsInputSettings
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

        public override bool SupportsValidation
        {
            get { return true; }
        }

        public override bool SupportsSearch
        {
            get { return true; }
        }

        public string GetEmailAddress(string fieldName, DataRow row)
        {
            return GetAddress(fieldName, row);
        }

        public string GetAddress(string fieldName, DataRow row)
        {
            if (row.Table.Columns.Contains(fieldName + DataTableColumn.Appendix_Original))
            {
                return row[fieldName + DataTableColumn.Appendix_Original].AsString();
            }
            return row[fieldName].AsString();
        }
    }

    #endregion
}