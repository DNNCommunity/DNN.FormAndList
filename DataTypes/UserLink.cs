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
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.UI.UserControls;
using Microsoft.VisualBasic;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit &amp; Validation Control for DataType "Url"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditUserLink : EditControl
    {
        protected UrlControl CtlUrl;
        protected ListControl CtlValueBox;

        void EditUserLink_Init(object sender, EventArgs e)
        {
            if (IsNotAListOfValues)
            {
                CtlUrl = (UrlControl) (Page.LoadControl("~/controls/URLControl.ascx"));
                CtlUrl.ID = CleanID(string.Format("{0}_url", FieldTitle));
                var container = new HtmlGenericControl("div");
                container.Attributes.Add("class", "dnnLeft");
                container.Controls.Add(CtlUrl);
                ValueControl = CtlUrl ;
                Controls.Add(container );
            }
            else
            {
                var ctlListControl = GetListControl();
                AddListItems(ctlListControl);
                foreach (var username in InputValueList)
                {
                    var user = UserController.GetUserByName(PortalId, username);
                    if (user == null && Information.IsNumeric(username)) //check for valid userID:
                    {
                        user = new UserController().GetUser(PortalId, int.Parse(username));
                    }
                    if (user != null)
                    {
                        var item = new ListItem
                            {
                                Text = NormalizeFlag ? user.Username : user.DisplayName,
                                Value = string.Format("UserId={0}", user.UserID)
                            };
                        ctlListControl.Items.Add(item);
                    }
                }
                CtlValueBox = ctlListControl;
                CtlValueBox.CssClass = "NormalTextBox";
                CtlValueBox.ID = CleanID(FieldTitle);
                Controls.Add(CtlValueBox);
            }
        }

        public EditUserLink()
        {
            Load += EditUserLink_Load;
            Init += EditUserLink_Init;
        }

        void EditUserLink_Load(object sender, EventArgs e)
        {
            //we need to initialize once only
            if (!Page.IsPostBack)
            {
                if (IsNotAListOfValues)
                {
                    var mc = new ModuleController();
                    var settings = mc.GetModule(ModuleId).ModuleSettings;
                    var showInNewWindow = settings[SettingName.URLNewWindow].AsBoolean();
                    //The following code must be executed during load, because the URLcontrol uses the viewstate
                    CtlUrl.UrlType = "M";
                    CtlUrl.ShowUsers = true;
                    CtlUrl.ShowFiles = false;
                    CtlUrl.ShowTabs = false;
                    CtlUrl.ShowUrls = false;
                    CtlUrl.ShowUpLoad = false;
                    CtlUrl.ShowLog = false;
                    CtlUrl.ShowTrack = false;
                    CtlUrl.Required = Required;
                    CtlUrl.ShowNewWindow = showInNewWindow;
                }

                if (!ValueIsSet && DefaultValue.Length > 0)
                {
                    var user = UserController.GetUserByName(PortalId, DefaultValue);
                    if (user == null && Information.IsNumeric(DefaultValue)) //check for valid userID:
                    {
                        user = new UserController().GetUser(PortalId, int.Parse(DefaultValue));
                    }
                    
                    Value = string.Format("UserId={0}", user.UserID);
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
                    var urls = new UrlController();
                    urls.UpdateUrl(PortalId, CtlUrl.Url, CtlUrl.UrlType, CtlUrl.Log, CtlUrl.Track, CtlUrl.ModuleID,
                                   CtlUrl.NewWindow);
                    var strLinkOptions = "";
                    if (CtlUrl.NewWindow)
                    {
                        strLinkOptions = "|options=W";
                    }
                    returnValue = CtlUrl.Url + strLinkOptions;
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
                    catch
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
        public class DataTypeUserLink : DataType, IUserSource, IEmailAdressSource
        {
            public override string Name
            {
                get { return "UserLink"; }
            }

            readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "ShowUserName", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "TokenText", Section = "List", SystemType = "String"},
                        new FieldSettingType {Key = "OpenInNewWindow", Section = "List", SystemType = "Boolean"}
                    };
            public override IEnumerable<FieldSettingType> FieldSettingTypes { get { return _fieldSettingTypes; }}

            struct FieldSetting
            {
                public string Title;
                public string TokenText;
                public bool ShowUserName;
                public bool OpenInNewWindow;
            }

            public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
            {
                if (ds != null)
                {
                    var fields = new ArrayList();
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
                                                TokenText = GetFieldSetting("TokenText", fieldId, ds).AsString(),
                                                ShowUserName = GetFieldSetting("ShowUserName", fieldId, ds).AsBoolean(),
                                                OpenInNewWindow = GetFieldSetting("OpenInNewWindow", fieldId, ds).AsBoolean()

                                            };
                            fields.Add(field);
                            tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Url,typeof (string)));
                            tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Original,typeof (string)));
                            tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Caption,typeof (string)));
                        }
                    }

                    if (fields.Count > 0)
                    {
                        PortalSettings portalSettings = null;
                        if (HttpContext.Current != null)
                        {
                            portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                        }
                        var mc = new ModuleController();
                        var settings = mc.GetModule(moduleId).ModuleSettings;
             
                        foreach (DataRow row in tableData.Rows)
                        {
                            foreach (FieldSetting field in fields)
                            {
                                var strFieldvalue = string.Empty;
                                //Link showed to the user
                                var link = row[field.Title].ToString();


                                //set caption:
                                var caption = field.TokenText;
                                var url = string.Empty;
                                //Link readable by browsers

                                link = UrlUtil.StripURL(link);
                                if (link != string.Empty) //valid link
                                {
                                    //var isLink = true;
                                    var intUser = Convert.ToInt32(-1);
                                    if (link.Like( "userid=*") && portalSettings != null)
                                    {
                                        try
                                        {
                                            intUser = int.Parse(link.Substring(7));
                                            tokenReplace.User = new UserController().GetUser(portalSettings.PortalId,
                                                                                             intUser);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    if (intUser == -1)
                                    {
                                        tokenReplace.User = new UserInfo {Username = "???"};
                                    }


                                    if (caption == string.Empty)
                                    {
                                        caption = field.ShowUserName ? "[User:DisplayName]" : "[User:UserName]";
                                    }

                                    caption = tokenReplace.ReplaceEnvironmentTokens(caption, row);
                                    if (caption == string.Empty) //DisplayName empty
                                    {
                                        caption = tokenReplace.ReplaceEnvironmentTokens("[User:username]");
                                    }

                                    url =
                                        HttpUtility.HtmlEncode(Globals.LinkClick(link, portalSettings.ActiveTab.TabID,
                                                                                 moduleId));

                                    strFieldvalue = string.Format("<!--{1}--><a href=\"{0}\"{2}>{1}</a>",
                                                                  url,
                                                                  caption,
                                                                  (field.OpenInNewWindow ? " target=\"_blank\"" : ""));

                                }
                                row[field.Title] = strFieldvalue;
                                row[field.Title + DataTableColumn.Appendix_Original] = link;
                                row[field.Title + DataTableColumn.Appendix_Url] = url;
                                row[field.Title + DataTableColumn.Appendix_Caption] = caption;
                            }
                        }
                    }
                }
            }

            public override string SupportedCasts
            {
                get { return string.Format("{0}|UserLink", base.SupportedCasts); }
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


            public override bool SupportsSearch
            {
                get { return true; }
            }

            public UserInfo GetUser(string fieldName, DataRow row)
            {
                var strUserid = (row.Table.Columns.Contains(fieldName + DataTableColumn.Appendix_Original) 
                                     ? row[fieldName + DataTableColumn.Appendix_Original] 
                                     : row[fieldName]).AsString();
                if (strUserid != string.Empty)
                {
                    var userInfo = new UserController().GetUser(Globals.GetPortalSettings().PortalId,
                                                                int.Parse(strUserid.Substring(7)));
                    return userInfo;
                }
                return null;
            }

            public string GetEmailAddress(string fieldName, DataRow row)
            {
                return GetAddress(fieldName, row);
            }

            public string GetAddress(string fieldName, DataRow row)
            {
                return GetUser(fieldName, row).Email;
            }
        }

        #endregion
    }
