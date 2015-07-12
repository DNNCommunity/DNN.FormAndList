using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.UserDefinedTable.Components;


namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Module Settings
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : ModuleSettingsBase
    {
        #region Private Methods

        public override void LoadSettings()
        {
            txtUserRecordQuota.Text = Settings[SettingName.UserRecordQuota].AsString();
            chkEditOwnData.Checked = Convert.ToBoolean(Settings[SettingName.EditOnlyOwnItems]);
            chkCaptcha.Checked = Convert.ToBoolean(Settings[SettingName.ForceCaptchaForAnonymous]);
            chkInputFiltering.Checked = Convert.ToBoolean(Settings[SettingName.ForceInputFiltering]);
            chkDisplayColumns.Checked =
                Convert.ToBoolean(! Settings[SettingName.ShowAllColumnsForAdmins].AsBoolean(true));
            chkPrivateColumns.Checked =
                Convert.ToBoolean(! Settings[SettingName.EditPrivateColumnsForAdmins].AsBoolean(true));
            chkHideSystemColumns.Checked = Convert.ToBoolean(! Settings[SettingName.ShowSystemColumns].AsBoolean());
        }

        public override void UpdateSettings()
        {
            var mc = new ModuleController();
            mc.UpdateModuleSetting(ModuleId, SettingName.UserRecordQuota, txtUserRecordQuota.Text);
            mc.UpdateModuleSetting(ModuleId, SettingName.EditOnlyOwnItems, chkEditOwnData.Checked.ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ForceCaptchaForAnonymous, chkCaptcha.Checked.ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ForceInputFiltering, chkInputFiltering.Checked.ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ShowAllColumnsForAdmins,
                                   (!(chkDisplayColumns.Checked)).ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.EditPrivateColumnsForAdmins,
                                   (!(chkPrivateColumns.Checked)).ToString());
            mc.UpdateModuleSetting(ModuleId, SettingName.ShowSystemColumns, (!(chkHideSystemColumns.Checked)).ToString());
        }

        #endregion
    }
}