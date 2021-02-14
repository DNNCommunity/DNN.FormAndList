using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   EditControls acts as a Factory and Collection for concrete "EditControl"s
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditControls : Dictionary<string, EditControl>
    {
        readonly ModuleInstanceContext _moduleContext;
        readonly bool _inputFilterScript;
        readonly bool _inputFilterTags;

        public EditControls(ModuleInstanceContext moduleContext)
        {
            _moduleContext = moduleContext;
            var isAdmin = ModuleSecurity.IsAdministrator();
            var isAnonymous = Convert.ToBoolean(moduleContext.PortalSettings.UserId == - 1);
            _inputFilterScript =
                Convert.ToBoolean(
                    !(isAdmin && ! moduleContext.Settings[SettingName.ForceInputFiltering].AsBoolean()));
            _inputFilterTags =
                Convert.ToBoolean(! isAdmin &&
                                  (isAnonymous ||
                                   moduleContext.Settings[SettingName.ForceInputFiltering].AsBoolean()));
        }

        public EditControl Add(string fieldTitle, string fieldType, int fieldId, string controlHelpText,
                               string defaultValue, bool required, string validationRule, string validationMsg,
                               string editStyle, string inputSettings, string outputSettings, bool normalizeFlag,
                               bool multipleValuesFlag, DataTable fieldSettingsTable, IFormEvents formEvents)
        {
            var editor = DataType.ByName(fieldType).EditControl;

            var tr = new TokenReplace {ModuleInfo = _moduleContext.Configuration};
            var strDefaultValue = System.Web.HttpUtility.HtmlDecode(tr.ReplaceEnvironmentTokens(defaultValue));

            var listInputType = EditControl.InputType.DropdownList;
            if (inputSettings.EndsWith(Definition.verticalRadioButtonEnabledToken))
            {
                inputSettings =
                    inputSettings.Remove(inputSettings.Length - Definition.verticalRadioButtonEnabledToken.Length);
                listInputType = EditControl.InputType.verticalRadioButtons;
            }
            else if (inputSettings.EndsWith(Definition.horizontalRadioButtonEnabledToken))
            {
                inputSettings =
                    inputSettings.Remove(inputSettings.Length - Definition.horizontalRadioButtonEnabledToken.Length);
                listInputType = EditControl.InputType.horizontalRadioButtons;
            }

            editor.Initialise(fieldTitle, fieldType, fieldId, _moduleContext.ModuleId, controlHelpText, strDefaultValue,
                              required, validationRule, validationMsg, editStyle, inputSettings, outputSettings,
                              normalizeFlag, multipleValuesFlag, _inputFilterTags, _inputFilterScript, listInputType,
                              _moduleContext,fieldSettingsTable , formEvents );

            editor.ID = string.Format("Edit{0}", fieldId);
            Add(fieldTitle, editor);
            return editor;
        }
    }
}