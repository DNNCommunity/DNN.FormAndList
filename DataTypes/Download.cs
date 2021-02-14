using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.FileSystem;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Download"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditDownload : EditURL
    {
        internal override void EditURL_Load(object sender, EventArgs e)
        {
            if (! Page.IsPostBack && IsNotAListOfValues)
            {
                CtlUrl.ShowUrls = false;
                CtlUrl.ShowTabs = false;
                CtlUrl.ShowTrack = true;
                CtlUrl.ShowLog = false;
            }
        }

        public override string Value
        {
            get
            {
                string returnValue;
                if (IsNotAListOfValues)
                {
                    var urlController = new UrlController();
                    urlController.UpdateUrl(PortalId, CtlUrl.Url, CtlUrl.UrlType, CtlUrl.Log, CtlUrl.Track, CtlUrl.ModuleID,
                                      CtlUrl.NewWindow);
                    returnValue = CtlUrl.Url + (CtlUrl.NewWindow ? "|options=W" : "");
                }
                else
                {
                    var fi =FileManager.Instance.GetFile(PortalId, CtlValueBox.SelectedValue);
                    returnValue = fi != null ? string.Format("FileID={0}", fi.FileId) : "";
                }
                return returnValue;
            }
            set
            {
                if (IsNotAListOfValues)
                {
                    CtlUrl.Url = UrlUtil.StripURL(value);
                    ValueIsSet = true;
                }
                else //If CStrN(Value) <> "" Then
                {
                    try
                    {
                        CtlValueBox.SelectedValue = value;
                        ValueIsSet = true;
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
// ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                }
            }
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Download"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeDownload : DataType
    {
        readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "Abbreviate", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "ShowOpenInNewWindow", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "EnforceDownload", Section = "List", SystemType = "Boolean"}
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
            get { return "Download"; }
        }

        struct FieldSettings
        {
            public string Title;
            public string OutputFormat;
            public bool Abbreviate;
            public bool ShowOpenInNewWindow;
            public bool EnforceDownload;
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            if (ds != null)
            {
                var fields = new ArrayList();
                //List of columns that contains URLs
                var tableData = ds.Tables[DataSetTableName.Data];
                var tokenReplace = new TokenReplace {ModuleId = moduleId};
                foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
                {
                    if (row[FieldsTableColumn.Type].ToString() == Name)
                    {
                        var fieldId = (int)row[FieldsTableColumn.Id];
                        var field = new FieldSettings
                                        {
                                            Title = row[FieldsTableColumn.Title].ToString(),
                                            OutputFormat = row[FieldsTableColumn.OutputSettings].AsString(),
                                            Abbreviate = GetFieldSetting("Abbreviate", fieldId, ds).AsBoolean(),
                                            ShowOpenInNewWindow = GetFieldSetting("ShowOpenInNewWindow", fieldId, ds).AsBoolean(),
                                            EnforceDownload = GetFieldSetting("EnforceDownload", fieldId, ds).AsBoolean()
                                        };
                        fields.Add(field);
                        tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Url, typeof (string)));
                        tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Caption,
                                                             typeof (string)));
                        tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Original,
                                                             typeof (string)));
                    }
                }
                if (fields.Count > 0)
                {
                    var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                 

                    foreach (DataRow row in tableData.Rows)
                    {
                        foreach (FieldSettings field in fields)
                        {
                            var strFieldvalue = string.Empty;
                            var strFileId = row[field.Title].ToString();
                            var openInNewWindow = !field.ShowOpenInNewWindow || UrlUtil.OpenUrlInNewWindow(strFileId);
                            strFileId = UrlUtil.StripURL(strFileId);
                            var strUrl = "";
                            //Link readable by browsers

                            var strCaption = string.Empty;
                            if (strFileId != string.Empty)
                            {
                                strUrl =
                                    HttpUtility.HtmlEncode(Globals.LinkClick(strFileId, portalSettings.ActiveTab.TabID,
                                                                             moduleId));
                                var fName = "";
                                var strDisplayName = "";
                                if (strFileId.Like("FileID=*")) 
                                {
                                    var f =FileManager.Instance.GetFile(int.Parse(UrlUtils.GetParameterValue(strFileId)));
                                    if (f != null)
                                    {
                                        fName = f.FileName;
                                        if (field.Abbreviate)
                                        {
                                            strDisplayName = (f.Folder + fName);
                                        }
                                        else
                                        {
                                            strDisplayName = fName;
                                        }
                                    }
                                }
                                else
                                {
                                    fName = Globals.ResolveUrl(strUrl);
                                    strDisplayName = field.Abbreviate 
                                        ? fName.Substring(Convert.ToInt32(fName.LastIndexOf("/", StringComparison.Ordinal) + 1)) 
                                        : fName;
                                }
                                strCaption = field.OutputFormat;
                                strCaption = string.IsNullOrEmpty(strCaption) 
                                    ? fName 
                                    : tokenReplace.ReplaceEnvironmentTokens(strCaption, row);
                                if (field.EnforceDownload)
                                {
                                    strUrl += "&amp;forcedownload=true";
                                }
                                strFieldvalue = string.Format("<!--{0}--><a href=\"{1}\" {2}>{3}</a>", strDisplayName,
                                                              strUrl, (openInNewWindow ? " target=\"_blank\"" : ""),
                                                              strCaption);
                            }
                            row[field.Title] = strFieldvalue;
                            row[field.Title + DataTableColumn.Appendix_Caption] = strCaption;
                            row[field.Title + DataTableColumn.Appendix_Original] = strFileId;
                            row[field.Title + DataTableColumn.Appendix_Url] = strUrl;
                        }
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

        public override bool SupportsInputSettings
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }

        public override bool SupportsLateRendering
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