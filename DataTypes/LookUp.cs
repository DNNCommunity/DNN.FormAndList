using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit &amp; Validation Control for DataType "Info"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditLookUp : EditExpression
    {
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Expression"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeLookUp : DataType
    {
        class TokenReplace : BaseCustomTokenReplace
        {
            readonly int _moduleId;

            public TokenReplace(int moduleId)
            {
                if (HttpContext.Current != null)
                {
                    AccessingUser = (UserInfo) (HttpContext.Current.Items["UserInfo"]);
                }
                else
                {
                    AccessingUser = new UserInfo();
                }
                CurrentAccessLevel = Scope.DefaultSettings;
                _moduleId = moduleId;
            }

            public string GetValue(string tokenText, DataRow row, string sourceColumn, string sourceType)
            {
                PropertySource.Clear();
                switch (sourceType.ToLowerInvariant())
                {
                    case "createdby":
                    case "changedby":
                    case "userlink":
                        var userInfo = ((IUserSource) (ByName(sourceType))).GetUser(sourceColumn, row);
                        if (userInfo == null)
                        {
                            return "";
                        }
                        PropertySource["user"] = userInfo;
                        PropertySource["profile"] = new ProfilePropertyAccess(userInfo);
                        break;
                    case "download":
                    case "url":
                    case "image":
                        var strFileId = row[sourceColumn + DataTableColumn.Appendix_Original].AsString();
                        if (strFileId != string.Empty)
                        {
                            PropertySource["file"] = new DownloadPropertyAccess(strFileId,
                                                                                Globals.GetPortalSettings().PortalId,
                                                                                _moduleId);
                        }
                        break;
                    default:
                        if ((ByName(sourceType)) is IEmailAdressSource)
                        {
                            var email = ((IEmailAdressSource) (ByName(sourceType))).GetEmailAddress(sourceColumn, row);
                            if (! string.IsNullOrEmpty(email))
                            {
                                PropertySource["gravatar"] = new GravatarPropertyAccess(email);
                            }
                        }
                        else
                        {
                            return "";
                        }
                        break;
                }

                return ReplaceTokens(tokenText);
            }
        }


        public override string Name
        {
            get { return "LookUp"; }
        }


        public override void SetStylesAndFormats(BoundField column, string format)
        {
            base.SetStylesAndFormats(column, format);
            if (format != string.Empty)
            {
                column.DataFormatString = string.Format("{{0:{0}}}", format);
            }
        }


        struct FieldSetting
        {
            public string Title;
            public string SourceColumn;
            public string SourceType;
            public string TokenText;
        }

        string GetTypeName(DataTable fieldsTable, string columnName)
        {
            using (
                var dv = new DataView(fieldsTable, string.Format("{0}=\'{1}\'", FieldsTableColumn.Title, columnName), "",
                                      DataViewRowState.CurrentRows))
            {
                return dv[0][FieldsTableColumn.Type].AsString();
            }
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var fields = new ArrayList();
            //List of columns that contains URLs
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldSetting = new FieldSetting
                                           {
                                               Title = row[FieldsTableColumn.Title].ToString(),
                                               SourceColumn = row[FieldsTableColumn.InputSettings].AsString()
                                           };
                    if (! string.IsNullOrEmpty(fieldSetting.SourceColumn))
                    {
                        fieldSetting.TokenText = row[FieldsTableColumn.Default].AsString();
                        fieldSetting.SourceType = GetTypeName(ds.Tables[DataSetTableName.Fields],
                                                              fieldSetting.SourceColumn);
                        fields.Add(fieldSetting);
                        row[FieldsTableColumn.ValueColumn] = XmlConvert.EncodeName(fieldSetting.Title);
                    }
                }
            }
            foreach (DataRow row in ds.Tables[DataSetTableName.Data].Rows)
            {
                foreach (FieldSetting field in fields)
                {
                    row[field.Title] = new TokenReplace(moduleId).GetValue(field.TokenText, row, field.SourceColumn,
                                                                           field.SourceType);
                }
            }
        }

        public override bool SupportsInputSettings
        {
            get { return true; }
        }

        public override bool InputSettingsIsValueList
        {
            get { return false; }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return false; }
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