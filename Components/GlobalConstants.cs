using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class RenderingMethod
    {
        public const string GridRendering = "DataGrid";
        public const string UserdefinedXSL = "XslUserDefinedStyleSheet";
        public const string PredefinedXSL = "XslPreDefinedStyleSheet";
    }


    public class Definition
    {
        public const string NameOfAnonymousUser = "Anonymous";
        public const string PathOfCustomConfig = "/Portals/_Default/UserDefinedTable/";
        public const string PathOfModule = "/DesktopModules/UserDefinedTable/";
        public const string ModuleDefinitionFriendlyName = "Form and List";
        public const string ModuleName = "DNN_UserDefinedTable";
        public const string QueryStringParameter = "udt_{0}_param";
        public const string XSLFolderName = "XslStyleSheets";
        public const string TemplateFolderName = "templates";
        public const string horizontalRadioButtonEnabledToken = "-[[hRBL]]";
        public const string verticalRadioButtonEnabledToken = "-[[vRBL]]";

        public static readonly string SharedRessources = string.Format("~{0}{1}/SharedRescources.resx", PathOfModule,
                                                                       Localization.LocalResourceDirectory);
    }

    public class PermissionName
    {
        public const string HasEditRowPermission = "ROWEDIT";
        public const string HasDeleteRowPermission = "ROWDELETE";
        public const string HasAddRowPermission = "ROWADD";
        public const string ShowAllUserDefinedColumnsPermission = "COLUMNVISIBLE";
        public const string EditRestricedFieldsPermission = "PRIVATECOLUMNS";
        public const string ShowListPermission = "SHOWLIST";
        public const string Code = "UDTEDIT";
    }

    public class DataSetTableName
    {
        public const string Fields = "Fields";
        public const string FieldSettings = "FieldSettings";
        public const string Data = "Data";
        public const string Settings = "Settings";
        public const string TabSettings = "TabSettings";
        public const string Stylesheets = "XSL";
    }

    public class FieldsTableColumn
    {
        public const string Id = "UserDefinedFieldId";
        public const string Title = "FieldTitle";
        public const string Required = "Required";
        public const string Type = "FieldType";
        public const string HelpText = "HelpText";
        public const string Default = "Default";
        public const string Visible = "Visible";
        public const string ShowOnEdit = "ShowOnEdit";
        public const string ValueColumn = "ValueColumn";
        public const string SortColumn = "SortColumn";
        public const string Order = "FieldOrder";
        public const string Searchable = "Searchable";
        public const string IsPrivate = "PrivateField";
        public const string MultipleValues = "MultipleValues";
        public const string InputSettings = "InputSettings";
        public const string OutputSettings = "OutputSettings";
        public const string NormalizeFlag = "NormalizeFlag";
        public const string ValidationMessage = "ValidationMessage";
        public const string ValidationRule = "ValidationRule";
        public const string EditStyle = "EditStyle";
    }

    public class DataTypeNames
    {
        public const string UDT_DataType_CreatedBy = "CreatedBy";
        public const string UDT_DataType_CreatedAt = "CreatedAt";
        public const string UDT_DataType_ChangedBy = "ChangedBy";
        public const string UDT_DataType_ChangedAt = "ChangedAt";
        public const string UDT_DataType_String = "String";
    }

    public class DataTableColumn
    {
        public const string EditLink = "EditLink";
        public const string Value = "FieldValue";
        public const string RowId = "UserDefinedRowId";
        public const string Appendix_Prefix = "_UDT_";
        public const string Appendix_Url = Appendix_Prefix + "Url";
        public const string Appendix_Ticks = Appendix_Prefix + "Ticks";
        public const string Appendix_Original = Appendix_Prefix + "Original";
        public const string Appendix_LocalizedValue = Appendix_Prefix + "Value";
        public const string Appendix_Caption = Appendix_Prefix + "Caption";
    }

    public class SettingsTableColumn
    {
        public const string Value = "Value";
        public const string Setting = "Setting";
    }

    public class StylesheetTableColumn
    {
        public const string NameOfSetting = "Type";
        public const string LocalFilePath = "LocalFilePath";
        public const string Stylesheet = "Stylesheet";
    }

    public class SettingName
    {
    //    public const string UsedCssClasses = "UDT_UsedCssClasses";
        public const string ImageWidth = "UDT_ImageWidth";
        public const string ImageHeight = "UDT_ImageHeight";
        public const string Paging = "UDT_Paging";
        public const string RenderingMethod = "UDT_RenderingMethod";
        public const string SortField = "UDT_SortField";
        public const string SortOrder = "UDT_SortOrder";
        public const string CalculatedColumnsRenderExpressionInForm = "CalculatedColumnsRenderExpressionInForm";
        public const string URLNewWindow = "UDT_URLNewWindow";
        public const string UserLinkNewWindow = "UDT_UserLinkNewWindow";
        public const string UserLinkCaption = "UDT_UserLinkUserName";
        public const string CnCLink = "UDT_CnCLink";
        public const string XslPreDefinedStyleSheet = "UDT_XslPreDefinedStyleSheet";
        public const string XslUserDefinedStyleSheet = "UDT_XslUserDefinedStyleSheet";
        public const string ShowSearchTextBox = "UDT_ShowSearchTextBox";
        public const string ShowNoRecordsUntilSearch = "UDT_ShowNoRecordsUntilSearch";
        public const string SimpleSearch = "UDT_SimpleSearch";
        public const string URLSearch = "UDT_URLSearch";
 //       public const string UseButtons = "UDT_UseButtons";
        public const string ForceCaptchaForAnonymous = "UDT_ForceCaptchaForAnonymous";
        public const string PreferReCaptcha = "UDT_PreferReCaptcha";
        public const string ReCaptchaSiteKey = "UDT_ReCaptchaSiteKey";
        public const string ReCaptchaSecretKey = "UDT_ReCaptchaPrivateKey";
        public const string ForceInputFiltering = "UDT_ForceFiltering";
        public const string EditOnlyOwnItems = "UDT_EditOnlyOwnItems";
        public const string ExcludeFromSearch = "UDT_ExcludeFromSearch";
        public const string UserRecordQuota = "UDT_UserRecordQuota";
        public const string Filter = "UDT_Filter";
        public const string Search = "UDT_Search";
        public const string ListOrForm = "UDT_ListNotFormsMode";
        //public const string ShowListRequiresPermission = "UDT_ShowListRequiresPermission";
        public const string SubmissionText = "UDT_Submission_succeeded";
      //  public const string TrackingEnabled = "UDT_EnableTracking";
        public const string TrackingEmailFrom = "UDT_TokenTrackingFrom";
        public const string TrackingEmailReplyTo = "UDT_TokenTrackingReplyTo";
        public const string TrackingEmailTo = "UDT_TokenTracking";
        public const string TrackingEmailCc = "UDT_TokenTrackingCC";
        public const string TrackingEmailBcc = "UDT_TokenTrackingBCC";
        public const string TrackingScript = "UDT_XslTracking";
        public const string TrackingSubject = "UDT_TrackingSubject";
        public const string TrackingTriggerOnNew = "UDT_TriggerOnNew";
        public const string TrackingTriggerOnUpdate = "UDT_TriggerOnUpdate";
        public const string TrackingTriggerOnDelete = "UDT_TriggerOnDelete";
        public const string TrackingTextOnNew = "UDT_TrackingTextOnNew";
        public const string TrackingTextOnUpdate = "UDT_TrackingTextOnUpdate";
        public const string TrackingTextOnDelete = "UDT_TrackingTextOnDelete";
        public const string TrackingMessage = "UDT_TrackingMessage";
        public const string ShowSystemColumns = "UDT_ShowSystemColumns";
        public const string EditPrivateColumnsForAdmins = "UDT_EditPrivateColumnsForAdmins";
        public const string ShowAllColumnsForAdmins = "UDT_ShowAllColumnsForAdmins";
     //   public const string TableFreeEditForm = "UDT_TableFreeEditForm";
        public const string TopCount = "UDT_TopCount";
        //public const string ControlFullWidth = "UDT_ControlFullWidth";
        public const string ForceDownload = "UDT_ForceDownload";
        public const string UponSubmitAction = "UDT_UponSubmitAction";
        public const string UponSubmitRedirect = "UDT_UponSubmitRedirect";
        public const string UseButtonsInForm = "UseButtonsInForm";
        public const string FormTemplate = "FormTemplate";
        public const string EnableFormTemplate = "EnableFormTemplate";
    }
}