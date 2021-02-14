using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using Globals = DotNetNuke.Common.Globals;
using TabInfo = DotNetNuke.Entities.Tabs.TabInfo;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Url"
    /// </summary>
    /// -----------------------------------------------------------------------------
// ReSharper disable InconsistentNaming
    public class EditURL : EditControl
// ReSharper restore InconsistentNaming
    {
        protected UrlControl CtlUrl;
        protected ListControl CtlValueBox;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Load += EditURL_Load;
            InitEditControl();
        }

        void InitEditControl()
        {
            if (IsNotAListOfValues)
            {
                CtlUrl = (UrlControl) (Page.LoadControl("~/controls/URLControl.ascx"));
                CtlUrl.ID = CleanID(string.Format("{0}_url", FieldTitle));
                var container = new HtmlGenericControl("div");
                container.Attributes.Add("class", "dnnLeft");
                container.Controls.Add(CtlUrl );
                Controls.Add(container );
                ValueControl = CtlValueBox;
            }
            else
            {
                ListControl ctlListControl;
                switch (ListInputType)
                {
                    case InputType.horizontalRadioButtons:
                        ctlListControl = new RadioButtonList
                        {
                            RepeatDirection = RepeatDirection.Horizontal,
                            RepeatLayout = RepeatLayout.Flow
                        };
                        break;
                    case InputType.verticalRadioButtons:
                        ctlListControl = new RadioButtonList { RepeatLayout = RepeatLayout.Flow };
                        break;
                    default:
                        ctlListControl = new DropDownList();
                        break;
                }
                AddListItems(ctlListControl);
                CtlValueBox = ctlListControl;
                CtlValueBox.CssClass = "NormalTextBox";
                CtlValueBox.ID = CleanID(FieldTitle);
                ValueControl = CtlValueBox;
                var container = new HtmlGenericControl("div");
                container.Attributes.Add("class", "dnnLeft");
                container.Controls.Add(CtlValueBox);
                Controls.Add(container) ;
                ValueControl = CtlValueBox;
            }
        }


        internal virtual void EditURL_Load(object sender, EventArgs e)
        {
            //we need to intilialize only once
            if (! Page.IsPostBack)
            {
                if (IsNotAListOfValues)
                {
                    CtlUrl.Required = Required;
                    CtlUrl.ShowLog = false;
                    CtlUrl.ShowTrack = false;
                    CtlUrl.ShowNewWindow = GetFieldSetting("ShowOpenInNewWindow").AsBoolean();
                }
                if (! ValueIsSet)
                {
                    Value = DefaultValue;
                }
            }
        }

        protected bool ValueIsSet
        {
            get
            {
                if (ViewState["ValueIsSet"] != null)
                {
                    return Convert.ToBoolean(ViewState["ValueIsSet"]);
                }
                return false;
            }
            set { ViewState["ValueIsSet"] = value; }
        }


        public override string Value
        {
            get
            {
                string returnValue;
                if (IsNotAListOfValues)
                {
                    var mc = new ModuleController();
                    var settings = mc.GetModule(ModuleId).ModuleSettings;

                    var urlController = new UrlController();
                    var trackDownloads = GetFieldSetting("TrackDownloads").AsBoolean();
                    urlController.UpdateUrl(PortalId, CtlUrl.Url, CtlUrl.UrlType, CtlUrl.Log,
                                      trackDownloads, CtlUrl.ModuleID,
                                      CtlUrl.NewWindow);
                    returnValue = CtlUrl.Url + (CtlUrl.NewWindow ? "|options=W" : "");
                }
                else
                {
                    returnValue = CtlValueBox.SelectedValue;
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
                else
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
    ///   MetaData and Formating for DataType "Url"
    /// </summary>
    /// -----------------------------------------------------------------------------
// ReSharper disable InconsistentNaming
    public class DataTypeURL : DataType
// ReSharper restore InconsistentNaming
    {
        public override string Name
        {
            get { return "URL"; }
        }

        readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "Abbreviate", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "ShowOpenInNewWindow", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "EnforceDownload", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "TrackDownloads", Section = "List", SystemType = "Boolean"}
                    };

        public override IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get { return _fieldSettingTypes; }
        }

        struct FieldSetting
        {
            public string Title;
            public string OutputFormat;
            public bool Abbreviate;
            public bool ShowOpenInNewWindow;
            public bool EnforceDownload;
            public bool TrackDownloads;
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            if (ds != null)
            {
                var fields = new ArrayList();
                var tableData = ds.Tables[DataSetTableName.Data];
                var tokenReplace = new TokenReplace {ModuleId = moduleId};
                PrepareHiddenColumns(ds, fields, tableData);

                if (fields.Count <= 0)
                {
                    return;
                }

                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                
                var tabCtrl = new TabController();
                foreach (DataRow row in tableData.Rows)
                {
                    foreach (FieldSetting field in fields)
                    {
                        FillTypeColumns(moduleId, tokenReplace, portalSettings, tabCtrl, row, field);
                    }
                }
            }
        }

        void FillTypeColumns(int moduleId, TokenReplace objTokenReplace, PortalSettings portalSettings,
                                TabController tabCtrl,
                               DataRow row, FieldSetting field)
        {
            var link = row[field.Title].ToString();
            //Link showed to the user
            bool openInNewWindow;
            if (field.ShowOpenInNewWindow ) //mit URL gepeicherten Wert lesen
            {
                openInNewWindow = UrlUtil.OpenUrlInNewWindow(link);
            }
            else
            {
                switch (Globals.GetURLType(UrlUtil.StripURL(link)))
                {
                    case TabType.File:
                        openInNewWindow = true;
                        break;
                    case TabType.Tab: //link to internal tab
                        openInNewWindow = false;
                        break;
                    default:
                        openInNewWindow = link.Like(  Globals.ApplicationMapPath + "*");
                        break;
                }
            }

            //set caption:
            var caption = field.OutputFormat;
            if (! string.IsNullOrEmpty(caption))
            {
                caption = objTokenReplace.ReplaceEnvironmentTokens(caption, row);
            }
            var isLink = true;
            //Link readable by browsers
            link = UrlUtil.StripURL(link);
            var url  = Globals.LinkClick(link, portalSettings.ActiveTab.TabID, moduleId, field.TrackDownloads, field.EnforceDownload);
            if (link != string.Empty)
            {
                switch (Globals.GetURLType(link))
                {
                    case TabType.Tab:
                        var tab = tabCtrl.GetTab(int.Parse(link), portalSettings.PortalId, false);
                        if (tab != null)
                        {
                            if (caption == string.Empty)
                            {
                                if (! field.Abbreviate)
                                {
                                    var strPath = string.Empty;
                                    if (tab.BreadCrumbs != null && tab.BreadCrumbs.Count > 0)
                                    {
                                        foreach (TabInfo b in tab.BreadCrumbs)
                                        {
                                            var strLabel = b.TabName;
                                            if (strPath != string.Empty)
                                            {
                                                strPath +=
                                                    string.Format(
                                                        "<img src=\"{0}/images/breadcrumb.gif\" border=\"0\" alt=\"Spacer\"/>",
                                                        Globals.ApplicationPath);
                                            }
                                            strPath += strLabel;
                                        }
                                    }
                                    caption = tab.TabPath.Replace("//",
                                                                     string.Format(
                                                                         "<img src=\"{0}/images/breadcrumb.gif\" border=\"0\" alt=\"Spacer\"/>",
                                                                         Globals.ApplicationPath));
                                }
                                else
                                {
                                    caption = tab.TabName;
                                }
                            }
                            url = field.EnforceDownload ? url : Globals.NavigateURL(int.Parse(link));
                        }
                        else
                        {
                            caption = Localization.GetString("PageNotFound.ErrorMessage",
                                                                Globals.ResolveUrl(
                                                                    string.Format("~{0}{1}/SharedResources.resx",
                                                                                  Definition.PathOfModule,
                                                                                  Localization.LocalResourceDirectory)));
                            url = string.Empty;
                            isLink = false;
                        }
                        break;
                    case TabType.File:
                        if (caption == string.Empty)
                        {
                            if (link.ToLowerInvariant().StartsWith("fileid="))
                            {
                                var file = FileManager.Instance.GetFile(int.Parse(link.Substring(7)));
                                if (file != null)
                                {
                                    if (! field.Abbreviate)
                                    {
                                        caption = file.Folder + file.FileName;
                                    }
                                    else
                                    {
                                        caption = file.FileName;
                                    }
                                }
                            }
                            else if (field.Abbreviate && link.IndexOf("/", StringComparison.Ordinal) > - 1)
                            {
                                caption = link.Substring(Convert.ToInt32(link.LastIndexOf("/", StringComparison.Ordinal) + 1));
                            }
                            else
                            {
                                caption = link;
                            }
                        }
                        break;

                    case TabType.Url:
                    case TabType.Normal:
                        if (caption == string.Empty)
                        {
                            if (field.Abbreviate && link.IndexOf("/", StringComparison.Ordinal) > - 1)
                            {
                                caption = link.Substring(Convert.ToInt32(link.LastIndexOf("/", StringComparison.Ordinal) + 1));
                            }
                            else
                            {
                                caption = link;
                            }
                        }
                        if (!field.TrackDownloads) url = link;
                        break;
                }


                if (field.EnforceDownload) url += "&amp;forcedownload=true";

                string strFieldvalue;
                if (isLink)
                {
                    strFieldvalue = string.Format("<!--{1}--><a href=\"{0}\"{2}>{1}</a>", url,
                                                  caption, (openInNewWindow ? " target=\"_blank\"" : ""));
                }
                else
                {
                    strFieldvalue = caption;
                }
                row[field.Title] = strFieldvalue;
                row[field.Title + DataTableColumn.Appendix_Caption] = caption;
                row[field.Title + DataTableColumn.Appendix_Original] = link;
                row[field.Title + DataTableColumn.Appendix_Url] = url;
            }
        }

        void PrepareHiddenColumns(DataSet ds, ArrayList fields, DataTable tableData)
        {
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int)row[FieldsTableColumn.Id];
                    var field = new FieldSetting
                                    {
                                        Title = row[FieldsTableColumn.Title].ToString(),
                                        OutputFormat = row[FieldsTableColumn.OutputSettings].AsString(),
                                        Abbreviate = GetFieldSetting("Abbreviate", fieldId, ds).AsBoolean(),
                                        ShowOpenInNewWindow = GetFieldSetting("ShowOpenInNewWindow", fieldId, ds).AsBoolean(),
                                        TrackDownloads = GetFieldSetting("TrackDownloads", fieldId, ds).AsBoolean(),
                                        EnforceDownload = GetFieldSetting("EnforceDownload", fieldId, ds).AsBoolean()
                                    };
                    fields.Add(field);
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Url, typeof (string)));
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Caption, typeof (string)));
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Original,
                                                         typeof (string)));
                }
            }
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}|Image", base.SupportedCasts); }
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

        public override bool SupportsSearch
        {
            get { return true; }
        }
    }

    #endregion

    #region URL Utilities

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Utilities for DataType "Url" with enclosed options.
    ///   StripURL returns URL string with options stripped off.
    ///   Options are appended delimited by "|" and preceeded by "options=".
    ///   Supported options:
    ///   W   open in new window
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class UrlUtil
    {
        public static string StripURL(string strUrl)
        {
            var i = Convert.ToInt32(strUrl.AsString().IndexOf("|", StringComparison.Ordinal));
            if (i > 0)
            {
                return strUrl.Substring(0, i);
            }
            return strUrl;
        }

        public static bool OpenUrlInNewWindow(string strUrl)
        {
            var strLinkOptions = "";
            //link
            var i = Convert.ToInt32(strUrl.AsString().IndexOf("|options=", StringComparison.Ordinal));
            if (i >= 0 && i <= strUrl.Length - 9)
            {
                strLinkOptions = strUrl.Substring(i + 9);
                i = strLinkOptions.IndexOf("|", StringComparison.Ordinal);
                if (i >= 0)
                {
                    strLinkOptions = strLinkOptions.Substring(0, i - 1);
                }
            }
            return strLinkOptions.IndexOf("W", StringComparison.Ordinal) >= 0;
        }
    }

    #endregion
}