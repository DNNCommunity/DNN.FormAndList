using System;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.UserDefinedTable
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Abstraced (MustInherit) EditControl
    ///   Defines the Interface and provides some Default Settings
    ///   Concrete Classes provides the UI to edit and validate data
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class  EditControl : Control
    {
        public enum InputType
        {
            DropdownList,
// ReSharper disable InconsistentNaming
            horizontalRadioButtons,
            verticalRadioButtons
        }
        // ReSharper restore InconsistentNaming
        #region Private Members

        string _customValidationMessage;
        public DataTable FieldSettingsTable { get; private set; }

        #endregion

        #region Public Properties
        public virtual void Initialise(string fieldTitle, string fieldType, int fieldId, int moduleId,
                                       string controlHelpText, string defaultValue, bool required, string validationRule,
                                       string validationMsg, string editStyle, string inputSettings,
                                       string outputSettings, bool normalizeFlag, bool multipleValuesFlag,
                                       bool inputFilterTags, bool inputFilterScript, InputType inputSettingsListType,
                                       ModuleInstanceContext moduleContext)
        {
            Initialise(fieldTitle, fieldType, fieldId, moduleId,
                       controlHelpText, defaultValue, required, validationRule,
                       validationMsg, editStyle, inputSettings,
                       outputSettings, normalizeFlag, multipleValuesFlag,
                       inputFilterTags, inputFilterScript, inputSettingsListType, moduleContext, null, null);
        }

        public virtual void Initialise(string fieldTitle, string fieldType, int fieldId, int moduleId,
                                       string controlHelpText, string defaultValue, bool required, string validationRule,
                                       string validationMsg, string editStyle, string inputSettings,
                                       string outputSettings, bool normalizeFlag, bool multipleValuesFlag,
                                       bool inputFilterTags, bool inputFilterScript, InputType inputSettingsListType,
                                       ModuleInstanceContext moduleContext, DataTable fieldSettingsTable,
                                       IFormEvents formEvents)
        {
            FieldTitle = fieldTitle;
            FieldType = fieldType;
            FieldId = fieldId;
            ModuleId = moduleId;
            HelpText = controlHelpText;
            DefaultValue = defaultValue;
            Required = required;
            ValidationRule = validationRule;
            _customValidationMessage = validationMsg;
            InputSettings = inputSettings;
            OutputSettings = outputSettings;
            Style = editStyle;
            NormalizeFlag = normalizeFlag;
            MultipleValuesFlag = multipleValuesFlag;
            FilterScript = inputFilterScript;
            FilterTags = inputFilterTags;
            ListInputType = inputSettingsListType;
            ModuleContext = moduleContext;
            FieldSettingsTable = fieldSettingsTable;
            FormEvents = formEvents;
        }

        protected IFormEvents FormEvents { get; set; }
       
        public string GetFieldSetting(string key)
        {
            return FieldSettingsTable.GetFieldSetting(key, FieldId);
        }

       
        public ModuleInstanceContext ModuleContext { get; private set; }

        public string DefaultValue { get; private set; }

        public bool FilterScript { get; private set; }

        public bool FilterTags { get; private set; }

        public string InputSettings { get; private set; }

        public InputType ListInputType { get; private set; }

        public int ModuleId { get; private set; }

        public bool NormalizeFlag { get; private set; }

        public bool MultipleValuesFlag { get; private set; }

        public string OutputSettings { get; private set; }

        public string Style { get; private set; }

        public abstract string Value { get; set; }

        protected static string LocalResourceFile
        {
            get
            {
                return string.Format("~/{0}{1}/SharedResources.resx", Definition.PathOfModule,
                                     Localization.LocalResourceDirectory);
            }
        }

        protected static int PortalId
        {
            get { return PortalController.Instance.GetCurrentPortalSettings().PortalId; }
        }

        protected bool IsNotAListOfValues
        {
            get { return !InputSettings.Contains(";"); }
        }

        protected string[] InputValueList
        {
            get { return InputSettings.Split(new []{';'}); }
        }

        public string FieldTitle { get; private set; }

        public string FieldType { get; private set; }

        public string HelpText { get; private set; }

        public Control ValueControl { get; set; }


        public int FieldId { get; private set; }

        protected bool Required { get; private set; }

        public string ValidationRule { get; private set; }

        public string ValidationMessage { get; private set; }

        #endregion

        #region Validation & Helper functions

        protected virtual bool IsNull()
        {
            return Null.IsNull(Value);
        }

        public bool IsValid()
        {
            //Is required and empty
            if (Required && IsNull())
            {
                ValidationMessage = Localization.GetString("Required2.ErrorMessage", LocalResourceFile);
           
            }
         
            //TypeValidation
            else if (! IsValidType())
            {
                ValidationMessage = Localization.GetString(string.Format("{0}.ErrorMessage", FieldType),
                                                            LocalResourceFile);
             
            }
            //CustomValidation
            else if (DataType.ByName(FieldType).SupportsValidation)
            {
                if (! IsValidCustom())
                {
                    ValidationMessage = _customValidationMessage;
                }
            }

            
            var isValid = string.IsNullOrEmpty( ValidationMessage );
            if (!isValid) Controls.Add(new LiteralControl(string.Format("<span class=\"dnnFormMessage dnnFormError\">{0}</span>", ValidationMessage)));
            return isValid;
        }

        bool IsValidCustom()
        {
            if (ValidationRule != string.Empty && Value != string.Empty)
            {
                return Value.ValidateRegEx(ValidationRule);
            }
            return true;
        }

        protected virtual bool IsValidType()
        {
            return true;
        }

        protected void AddListItems(ListControl control)
        {
            foreach (var v in InputSettings.Split(';'))
            {
                var item = new ListItem();

                if (v.Contains("|"))
                {
                    var pair = v.Split('|');

                    item.Text = pair[0].Trim();
                    item.Value = pair[1].Trim();
                }
                else
                {
                    item.Value = v.Trim();
                }
                item.Attributes["class"] = "dnnFormRadioButtons";
                control.Items.Add(item);
            }
        }

        protected static string ApplyScriptFilter(string input)
        {
            //portalsecurity tries to detect encoding, if the input contains "&[g,l]t;", it decodes the input first
            //than it cleans up the input and encodes it again

            //workaraound
            //1) encode helperstring + input (Enforces decoding inside inputfilter!)
            input = HttpUtility.HtmlEncode(string.Concat("<>", input));
            //2) apply filter
            input = new PortalSecurity().InputFilter(input, PortalSecurity.FilterFlag.NoScripting);
            //3) return decoded String minus helperstring
// ReSharper disable PossibleNullReferenceException
            return HttpUtility.HtmlDecode(input).Substring(2);
// ReSharper restore PossibleNullReferenceException
        }

        protected static string CleanID(string name)
        {
            return XmlConvert.EncodeName(name.Replace(" ", "_"));
        }

        #endregion

        protected DateTime ServerTime(string value)
        {
            var dateTime = DateTime.Parse(value);
            return value.Contains("+") 
                ? dateTime 
                : TimeZoneInfo.ConvertTimeFromUtc( dateTime, ModuleContext.PortalSettings.TimeZone );
        }

        protected ListControl GetListControl()
        {
            ListControl ctlListControl;
            switch (ListInputType)
            {
                case InputType.horizontalRadioButtons:
                    if (MultipleValuesFlag)
                        ctlListControl = new CheckBoxList
                                             {
                                                 RepeatDirection = RepeatDirection.Horizontal,
                                                 RepeatLayout = RepeatLayout.Flow
                                             };
                    else
                        ctlListControl = new RadioButtonList
                                             {
                                                 RepeatDirection = RepeatDirection.Horizontal,
                                                 RepeatLayout = RepeatLayout.Flow
                                             };
                    break;
                case InputType.verticalRadioButtons:
                    if (MultipleValuesFlag)
                        ctlListControl = new CheckBoxList();
                    else
                    {
                        ctlListControl = new RadioButtonList {RepeatLayout = RepeatLayout.Table };
                    }
                    break;
                default:
                    if (MultipleValuesFlag)
                        ctlListControl = new ListBox{SelectionMode=ListSelectionMode.Multiple};
                    else
                        ctlListControl = new DropDownList();
                    break;
            }
            return ctlListControl;
        }
    }
}
