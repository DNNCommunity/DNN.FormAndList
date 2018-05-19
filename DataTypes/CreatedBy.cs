using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "CreatedBy"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditCreatedBy : EditControl
    {
        protected string Editor="Editor";

        public EditCreatedBy()
        {
            Load += Page_Load;
        }



        public override string Value
        {
            get {
                return   (ViewState[Editor]!=null) ? (string) ViewState[Editor] : CurrentUserName();
            }
            set
            {
                if (value != string.Empty)
                {
                    ViewState[Editor] = value;
                    Controls.Add(
                        new LiteralControl("<span class=\"Normal\">" + ModuleSecurity.BestUserName(value, PortalId) +
                                           "</span>"));
                }
            }
        }

        protected static string CurrentUserName()
        {
            var result = UserController.Instance.GetCurrentUserInfo().Username;
            if (string.IsNullOrEmpty(result))
            {
                result = Definition.NameOfAnonymousUser;
            }
            return result;
        }

        void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                if (ViewState[Editor]!=null)
                {
                    Controls.Add(
                        new LiteralControl("<span class=\"Normal\">" +
                                           ModuleSecurity.BestUserName(ViewState[Editor].ToString(), PortalId) +
                                           "</span>"));
                }
            }
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Createdby"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeCreatedBy : DataType, IEmailAdressSource, IUserSource
    {
        readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "AsLink", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "OpenInNewWindow", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "PreferDisplayName", Section = "List", SystemType = "Boolean"}
                    };
        public override IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get
            {
                return _fieldSettingTypes;
            }
        }
        public override bool IsUserDefinedField
        {
            get { return false; }
        }

        public override string Name
        {
            get { return "CreatedBy"; }
        }

        public override bool SupportsHideOnEdit
        {
            get { return true; }
        }

       
        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var colCreatedBy = new ArrayList();
            var tableData = ds.Tables[DataSetTableName.Data];
            var asLink=false;
            var openInNewWindow=false;
            var preferDisplayName=false;
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int) row[FieldsTableColumn.Id];
                    asLink = GetFieldSetting("AsLink", fieldId, ds).AsBoolean( );
                    openInNewWindow = GetFieldSetting("OpenInNewWindow", fieldId, ds).AsBoolean();
                    preferDisplayName = GetFieldSetting("PreferDisplayName", fieldId, ds).AsBoolean();
                    var title = row[FieldsTableColumn.Title].ToString();
                    colCreatedBy.Add(title);
                    tableData.Columns.Add(new DataColumn(title + DataTableColumn.Appendix_Original, typeof (string)));
                    tableData.Columns.Add(new DataColumn(title + DataTableColumn.Appendix_Caption, typeof (string)));
                    tableData.Columns.Add(new DataColumn(title + DataTableColumn.Appendix_Url, typeof (string)));
                }
            }
            if (colCreatedBy.Count > 0)
            {
                var portalId = Null.NullInteger;
                var tabId = Null.NullInteger; 
                if (HttpContext.Current != null)
                {
                    var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    portalId = portalSettings.PortalId;
                    tabId = portalSettings.ActiveTab.TabID;
                }
               
                foreach (DataRow row in tableData.Rows)
                {
                    foreach (string fieldName in colCreatedBy)
                    {
                        var strCreatedBy = row[fieldName].ToString();
                        var strCaption = strCreatedBy;
                        var strUrl = string.Empty;
                  
                            if (!preferDisplayName)
                            {
                                strCaption = ModuleSecurity.BestUserName(strCreatedBy, portalId);
                            }
                            if (asLink)
                            {
                                var userId = ModuleSecurity.UserId(strCreatedBy, portalId);
                                if (userId > 0)
                                {
                                    strUrl = HttpUtility.HtmlEncode(Globals.LinkClick(("userid=" + userId), tabId,moduleId));
                                }
                            }
                   
                        string strFieldvalue;
                        if (asLink && strUrl != string.Empty)
                        {
                            strFieldvalue = string.Format("<!--{1}--><a href=\"{0}\"{2}>{1}</a>", strUrl, strCaption,
                                                          (openInNewWindow  ? " target=\"_blank\"" : ""));
                        }
                        else
                        {
                            strFieldvalue = strCaption;
                        }
                        row[fieldName] = strFieldvalue;
                        row[fieldName + DataTableColumn.Appendix_Original] = strCreatedBy;
                        row[fieldName + DataTableColumn.Appendix_Caption] = strCaption;
                        if (strUrl != string.Empty)
                        {
                            row[fieldName + DataTableColumn.Appendix_Url] = strUrl;
                        }
                    }
                }
            }
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
            return GetUser(fieldName, row).Email;
        }

        public UserInfo GetUser(string fieldName, DataRow row)
        {
            var username = (row.Table.Columns.Contains(fieldName + DataTableColumn.Appendix_Original) 
                                   ? row[fieldName + DataTableColumn.Appendix_Original] 
                                   : row[fieldName]).AsString();
            if (username != string.Empty)
            {
                var userInfo = UserController.GetUserByName(Globals.GetPortalSettings().PortalId, username);
                return userInfo;
            }
            return new UserInfo();
        }
    }

    #endregion
}