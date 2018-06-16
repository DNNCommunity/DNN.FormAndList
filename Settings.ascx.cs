using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.UserDefinedTable.Components;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Module Settings
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : ModuleSettingsBase
    {
        public override void LoadSettings()
        {
            txtUserRecordQuota.Text = Settings[SettingName.UserRecordQuota].AsString();
            chkEditOwnData.Checked = Convert.ToBoolean(Settings[SettingName.EditOnlyOwnItems]);           
            chkInputFiltering.Checked = Convert.ToBoolean(Settings[SettingName.ForceInputFiltering]);
            chkDisplayColumns.Checked =
                Convert.ToBoolean(!Settings[SettingName.ShowAllColumnsForAdmins].AsBoolean(true));
            chkPrivateColumns.Checked =
                Convert.ToBoolean(!Settings[SettingName.EditPrivateColumnsForAdmins].AsBoolean(true));
            chkHideSystemColumns.Checked = Convert.ToBoolean(!Settings[SettingName.ShowSystemColumns].AsBoolean());
            ddlCaptcha.Items.Add(new ListItem(LocalizeString("No"), "No"));
            ddlCaptcha.Items.Add(new ListItem(LocalizeString("DnnCaptcha"), "DnnCaptcha"));
            ddlCaptcha.Items.Add(new ListItem(LocalizeString("ReCaptcha"), "ReCaptcha"));
            if (!Convert.ToBoolean(Settings[SettingName.ForceCaptchaForAnonymous]))
            {
                ddlCaptcha.SelectedIndex = 0;
            }
            else
            {
                if (Convert.ToBoolean(Settings[SettingName.PreferReCaptcha]))
                {
                    ddlCaptcha.SelectedIndex = 2;
                }
                else
                {
                    ddlCaptcha.SelectedIndex = 1;
                }
            }
            txtSiteKey.Text = Settings[SettingName.ReCaptchaSiteKey].AsString();
            txtSecretKey.Text = Settings[SettingName.ReCaptchaSecretKey].AsString();
        }        

        public override void UpdateSettings()
        {
            var mc = new ModuleController();
            mc.UpdateModuleSetting(ModuleId, SettingName.UserRecordQuota, txtUserRecordQuota.Text);
            mc.UpdateModuleSetting(ModuleId, SettingName.EditOnlyOwnItems, chkEditOwnData.Checked.ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ForceInputFiltering, chkInputFiltering.Checked.ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ShowAllColumnsForAdmins,
                                   (!(chkDisplayColumns.Checked)).ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.EditPrivateColumnsForAdmins,
                                   (!(chkPrivateColumns.Checked)).ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ShowSystemColumns, (!(chkHideSystemColumns.Checked)).ToString());
            switch (ddlCaptcha.SelectedIndex)
            {
                case 0:
                    // No
                    mc.UpdateModuleSetting(ModuleId, SettingName.ForceCaptchaForAnonymous, false.ToString());
                    break;
                case 1:
                    // DnnCaptcha
                    mc.UpdateModuleSetting(ModuleId, SettingName.ForceCaptchaForAnonymous, true.ToString());
                    mc.UpdateModuleSetting(ModuleId, SettingName.PreferReCaptcha, false.ToString());
                    break;
                case 2:
                    // ReCaptcha
                    mc.UpdateModuleSetting(ModuleId, SettingName.ForceCaptchaForAnonymous, true.ToString());
                    mc.UpdateModuleSetting(ModuleId, SettingName.PreferReCaptcha, true.ToString());
                    break;
                default:
                    break;
            }
            mc.UpdateModuleSetting(ModuleId, SettingName.ReCaptchaSiteKey, txtSiteKey.Text);
            mc.UpdateModuleSetting(ModuleId, SettingName.ReCaptchaSecretKey, txtSecretKey.Text);
        }        
    }
}