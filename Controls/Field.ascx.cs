using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.UserDefinedTable.Controls
{
    public partial class Field : UserControl
    {

        protected LabelControl DefaultLabel;
        protected LabelControl InputSettingsLabel;
        protected LabelControl OutputSettingsLabel;
        protected LabelControl NormalizeFlagLabel;
        protected LabelControl MultipleValuesLabel;
        protected LabelControl ShowOnEditLabel;
        public class FieldTitelException:Exception
        {
            public FieldTitelException(string message) : base(message){}
        }
      
        public Func<string, string> LocalizeString { get; set; }

        protected override void OnInit(EventArgs e)
        {
            cmdCancel.Click += cmdCancel_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            cboFieldType.SelectedIndexChanged += cboFieldType_SelectedIndexChanged;
            Load += Page_Load;
            SetHelpLinks();
        }

        void Page_Load(object sender, EventArgs e)
        {
            divError.Visible = false;
            divWarning.Visible = false;
            if (IsPostBack)
            {
                var selectedType = DataType.ByName(cboFieldType.SelectedValue);
                FormFieldSettings.Show(selectedType);
                ListFieldSettings.Show(selectedType);
            }
        }

        void cboFieldType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var newType = DataType.ByName(cboFieldType.SelectedValue);
            CheckCast(newType);
            ShowOrHideSettingsByType(newType);
            FormFieldSettings.Show(newType);
            ListFieldSettings.Show(newType);
        }

        void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    UpdateField();  
                    if (HideField != null) HideField();
                }
            }
        
            catch (FieldTitelException ea)
            {
                ShowErrorMessage(ea.Message );
            }
        }

        void ShowErrorMessage(string message)
        {
            divError.Controls.Add(new LiteralControl(message));
            divError.Visible = true;
        }

        void ShowWarning(string message)
        {
            divWarning.Controls.Add(new LiteralControl(message));
            divWarning.Visible = true;
        }


        void cmdCancel_Click(object sender, EventArgs e)
        {
            if (HideField != null) HideField();
        }

        public event Action HideField;

        public DataRowView DataSource { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        protected IDictionary<string, string> ContentDataTypes
        {
            get { return DataType.ContentDataTypes(); }
        }

        protected IDictionary<string, string> SystemDataTypes
        {
            get { return DataType.SystemDataTypes(); }
        }

        public DataTable Settings { get; set; }
        
        void UpdateField()
        {
            var fieldOrder = (int)DataSource[FieldsTableColumn.Order];
            
            var newFieldType = cboFieldType.SelectedItem.Value;
            var newHelpText = txtHelpText.Text.Trim();
            var fieldTitle = GetFieldTitle();
            var inputSettings = GetInputSettings();
            var formerTitle = DataSource[FieldsTableColumn.Title].AsString();
            var formerType = DataType.ByName(DataSource[FieldsTableColumn.Type].AsString());
            var id = DataSource[FieldsTableColumn.Id].AsInt();
            if (IsAllowedFieldTitle(fieldTitle) && IsUniqueFieldTitle(fieldTitle, formerTitle))
            {
                if (formerType.IsUserDefinedField)
                {
                    if (IsNewField(id))
                    {
                        id = FieldController.AddField(ModuleContext.ModuleId, fieldTitle,
                                                               fieldOrder, newHelpText,
                                                               chkRequired.Checked, newFieldType, txtDefault.Text,
                                                               chkDisplayOnList.Checked , chkShowOnEdit.Checked,
                                                               chkSearchable.Checked, chkRestrictedFormField.Checked,
                                                               chkMultipleValues.Checked, inputSettings,
                                                               txtOutputSettings.Text, chkNormalizeFlag.Checked,
                                                               txtValidationRule.Text, txtValidationMessage.Text,
                                                               txtEditStyle.Text);
                        if (txtDefault.Text != "")
                        {
                            new UserDefinedTableController(ModuleContext).FillDefaultData(id, txtDefault.Text);
                        }
                    }
                    else
                    {
                        FieldController.UpdateField(id, fieldTitle, newHelpText, chkRequired.Checked,
                                                    newFieldType, txtDefault.Text, chkDisplayOnList.Checked,
                                                    chkShowOnEdit.Checked, chkSearchable.Checked,
                                                    chkRestrictedFormField.Checked, chkMultipleValues.Checked,
                                                    inputSettings,
                                                    txtOutputSettings.Text, chkNormalizeFlag.Checked,
                                                    txtValidationRule.Text, txtValidationMessage.Text,
                                                    txtEditStyle.Text);
                    }
                }
                else
                {
                    FieldController.UpdateField(id,
                                                fieldTitle, newHelpText, true, formerType.Name, string.Empty,
                                                chkDisplayOnList.Checked, chkShowOnEdit.Checked, chkSearchable.Checked,
                                                chkRestrictedFormField.Checked, false,
                                                string.Empty,
                                                 txtOutputSettings.Text, chkNormalizeFlag.Checked,
                                                string.Empty, string.Empty, string.Empty );
                }
            }
            ListFieldSettings.Update( id, DataType.ByName(newFieldType));
            FormFieldSettings.Update( id, DataType.ByName(newFieldType));
            
        }

        string GetFieldTitle()
        {
            var fieldTitle = txtFieldTitle.Text.Trim();
            if (!ModuleSecurity.IsAdministrator())
            {
                fieldTitle = new PortalSecurity().InputFilter(fieldTitle, PortalSecurity.FilterFlag.NoScripting);
            }
            return fieldTitle;
        }

        static bool IsNewField(int id)
        {
            return id == Null.NullInteger;
        }

        string GetInputSettings()
        {
            var inputSettings = txtInputSettings.Text;
            if (cboInputSettings.Visible)
            {
                inputSettings = cboInputSettings.SelectedValue;
            }
            if (rblListType.SelectedValue == "RadioButtons")
            {
                inputSettings += Definition.verticalRadioButtonEnabledToken;
            }
            if (rblListType.SelectedValue == "RadioButtonsHorizontal")
            {
                inputSettings += Definition.horizontalRadioButtonEnabledToken;
            }
            return inputSettings;
        }

        bool IsAllowedFieldTitle(string title)
        {
            if (title.IndexOfAny(":,*?".ToCharArray()) > -1)
            {
                throw new FieldTitelException( LocalizeString("SpecialCharactersInFieldTitle.ErrorMessage") );
            }
            if (title.Contains(  DataTableColumn.Appendix_Prefix) )
            {
                throw new FieldTitelException(LocalizeString("UDT_InFieldTitle.ErrorMessage"));
            }
            return title != string.Empty;
        }

        bool IsUniqueFieldTitle(string fieldTitle, string oldfieldtitle)
        {
            oldfieldtitle = oldfieldtitle.ToLowerInvariant();
            var fields = FieldController.GetFieldsTable(ModuleContext.ModuleId);
            var isUnique = !fields.Rows.Cast<DataRow>()
                                .Any(
                                    field =>
                                    field[FieldsTableColumn.Title].ToString().ToLowerInvariant() ==
                                    fieldTitle.ToLowerInvariant()
                                    && field[FieldsTableColumn.Title].ToString().ToLowerInvariant() != oldfieldtitle);
            if (!isUnique)
            {
                throw new FieldTitelException(LocalizeString("UniqueFieldName.ErrorMessage"));
            }
            return true;
        }

        void CheckCast(DataType newType)
        {
            var formerType = DataType.ByName(DataSource[FieldsTableColumn.Type].AsString("String"));
           
                var allowedCasts = formerType.SupportedCasts.Split("|".ToCharArray());
                if (allowedCasts.Any(n => newType.Name  == n))
                {
                    return;
                }
            // Cast is not allowed, now we need to check whether data already exists for that column
            var fieldId = DataSource[FieldsTableColumn.Id].AsInt();
            if (new UserDefinedTableController(ModuleContext).FieldHasData(fieldId))
                {
                    var message = LocalizeString("UnsupportedCast.ErrorMessage")
                            .AsString("You have changed the fieldtype for {2} from {0} to {1}. Note that this may cause an error");

                    var title = new PortalSecurity().InputFilter(txtFieldTitle.Text.Trim(), PortalSecurity.FilterFlag.NoScripting);
                    message = string.Format(message, formerType.GetLocalization(),
                                           newType.GetLocalization(), title);
                    ShowWarning(message);
                }
        }

        void SetHelpLinks()
        {

            var helpUrL = string.Format("javascript:OpenHelpWindow(\'{0}\')",
                                           ResolveUrl("~/desktopmodules/userdefinedtable/HelpPopup.aspx?resourcekey=Help_Tokens_6"));
            hlpToken.NavigateUrl = helpUrL;

            helpUrL = string.Format("javascript:OpenHelpWindow(\'{0}\')",
                                    ResolveUrl("~/desktopmodules/userdefinedtable/HelpPopup.aspx?resourcekey=Help_HiddenColumns"));
            hlpColumns.NavigateUrl = helpUrL;
        }

        public void Bind()
        {
            txtFieldTitle.Text = DataSource[FieldsTableColumn.Title].AsString();

            var type = DataType.ByName(DataSource[FieldsTableColumn.Type].AsString("String"));
            cboFieldType.DataSource = type.IsUserDefinedField ? DataType.ContentDataTypes() : DataType.SystemDataTypes();
            cboFieldType.DataBind();
            cboFieldType.SelectedValue = type.Name;
            lblType.Text = type.GetLocalization();

            txtHelpText.Text = DataSource[FieldsTableColumn.HelpText].AsString();
            txtDefault.Text = DataSource[FieldsTableColumn.Default].AsString();
            var inputSettings = DataSource[FieldsTableColumn.InputSettings].AsString();

            chkRequired.Checked = DataSource[FieldsTableColumn.Required].AsBoolean();
            chkDisplayOnList.Checked = DataSource[FieldsTableColumn.Visible].AsBoolean();
            chkRestrictedFormField.Checked = DataSource[FieldsTableColumn.IsPrivate].AsBoolean();
            chkSearchable.Checked = DataSource[FieldsTableColumn.Searchable].AsBoolean();

            if (inputSettings.EndsWith(Definition.verticalRadioButtonEnabledToken))
            {
                rblListType.SelectedValue = "RadioButtons";
            }
            else if (inputSettings.EndsWith(Definition.horizontalRadioButtonEnabledToken))
            {
                rblListType.SelectedValue = "RadioButtonsHorizontal";
            }

            txtInputSettings.Text = Regex.Replace(inputSettings, "-\\[\\[(?:h|v)RBL]]$", "");
            txtOutputSettings.Text = DataSource[FieldsTableColumn.OutputSettings].AsString();
            chkNormalizeFlag.Checked = DataSource[FieldsTableColumn.NormalizeFlag].AsBoolean();
            chkMultipleValues.Checked = DataSource[FieldsTableColumn.MultipleValues].AsBoolean();
            chkShowOnEdit.Checked = DataSource[FieldsTableColumn.ShowOnEdit].AsBoolean();
            txtValidationRule.Text = DataSource[FieldsTableColumn.ValidationRule].AsString();
            txtValidationMessage.Text = DataSource[FieldsTableColumn.ValidationMessage].AsString();
            txtEditStyle.Text = DataSource[FieldsTableColumn.EditStyle].AsString();
            if (cboInputSettings.Visible)
            {
                cboInputSettings.SelectedValue = inputSettings.AsString("String");
            }
            ShowOrHideSettingsByType(type);
           
            var id = DataSource[FieldsTableColumn.Id].AsInt();
            DataBind();
            if (!IsPostBack)
            {
                FormFieldSettings.Show(type);
                FormFieldSettings.BindData(id, Settings, type);
                ListFieldSettings.Show(type);
                ListFieldSettings.BindData(id, Settings, type);
            }

        }

      

        void ShowOrHideSettingsByType(DataType selectedType)
        {
            cboFieldType.Visible = selectedType.IsUserDefinedField;
            lblType.Visible = !selectedType.IsUserDefinedField;
            chkRequired.Visible = selectedType.IsUserDefinedField;
            imgRequired.Visible = !selectedType.IsUserDefinedField;
            chkSearchable.Visible = selectedType.SupportsSearch;
            lblSearchable.Visible = selectedType.SupportsSearch;
           

            if (selectedType.SupportsDefaultValue)
            {
                panDefault.Visible = true;
                DefaultLabel.Text = selectedType.GetLocalization("DefaultValue").AsString(DefaultLabel.Text);
                DefaultLabel.HelpText = selectedType.GetLocalization("DefaultValue.Help").AsString(DefaultLabel.HelpText);
            }
            else
            {
                panDefault.Visible = false;
            }

            if (selectedType.SupportsInputSettings)
            {
                panInputSettings.Visible = true;
                var selectionSource = selectedType.InputSettingsList;
                if (selectionSource == null)
                {
                    txtInputSettings.Visible = true;
                    cboInputSettings.Visible = false;
                }
                else
                {
                    cboInputSettings.DataSource = selectionSource;
                    var currValue = cboInputSettings.SelectedValue;
                    cboInputSettings.DataBind();
                    if (currValue != "" && cboInputSettings.Items.FindByValue(currValue) != null)
                    {
                        cboInputSettings.SelectedValue = currValue;
                    }
                    else
                    {
                        cboInputSettings.SelectedValue = selectedType.InputSettingDefault;
                    }
                    txtInputSettings.Visible = false;
                    cboInputSettings.Visible = true;
                }
                InputSettingsLabel.Text = selectedType.GetLocalization("InputSetting").AsString(InputSettingsLabel.Text);
                InputSettingsLabel.HelpText =
                    selectedType.GetLocalization("InputSetting.Help").AsString(InputSettingsLabel.HelpText);
                rblListType.Visible = selectedType.InputSettingsIsValueList;
            }
            else
            {
                panInputSettings.Visible = false;
            }

            if (selectedType.SupportsOutputSettings)
            {
                panOutputSettings.Visible = true;
                OutputSettingsLabel.Text = selectedType.GetLocalization("OutputSetting").AsString(OutputSettingsLabel.Text);
                OutputSettingsLabel.HelpText =
                    selectedType.GetLocalization("OutputSetting.Help").AsString(OutputSettingsLabel.HelpText);
            }
            else
            {
                panOutputSettings.Visible = false;
            }

            if (selectedType.SupportsNormalizeFlag)
            {
                panNormalizeFlag.Visible = true;
                NormalizeFlagLabel.Text = selectedType.GetLocalization("NormalizeFlag").AsString(NormalizeFlagLabel.Text);
                NormalizeFlagLabel.HelpText =
                    selectedType.GetLocalization("NormalizeFlag.Help").AsString(NormalizeFlagLabel.HelpText);
            }
            else
            {
                panNormalizeFlag.Visible = false;
            }

            if (selectedType.SupportsMultipleValues)
            {
                panMultipleValues.Visible = true;
                MultipleValuesLabel.Text = selectedType.GetLocalization("MultipleValues").AsString(MultipleValuesLabel.Text);
                MultipleValuesLabel.HelpText =
                    selectedType.GetLocalization("MultipleValues.Help").AsString(MultipleValuesLabel.HelpText);
            }
            else
            {
                panMultipleValues.Visible = false;
            }
            panShowOnEdit.Visible = selectedType.SupportsHideOnEdit;
            if (selectedType.SupportsHideOnEdit )
            {
                ShowOnEditLabel.Text = selectedType.GetLocalization("HideOnEdit").AsString(ShowOnEditLabel.Text);
                ShowOnEditLabel.HelpText  = selectedType.GetLocalization("HideOnEdit.Help").AsString(ShowOnEditLabel.HelpText);
            }
            panEditStyle.Visible = selectedType.SupportsEditStyle;
            panValidationRule.Visible = selectedType.SupportsValidation;
            panValidationMessage.Visible = selectedType.SupportsValidation;
            panHelpText.Visible = selectedType.SupportsEditing;
        }
    }
}