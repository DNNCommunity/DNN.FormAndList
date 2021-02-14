using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using Microsoft.VisualBasic;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class Settings
    {
        readonly Hashtable _settings;

        public Settings(Hashtable settings)
        {
            _settings = settings;
        }

        public bool EditOnlyOwnItems
        {
            get { return _settings[SettingName.EditOnlyOwnItems].AsBoolean(); }
        }

        public bool EditPrivateColumnsForAdmins
        {
            get { return _settings[SettingName.EditPrivateColumnsForAdmins].AsBoolean(); }
        }

        public string Filter
        {
            get { return _settings[SettingName.Filter].AsString(); }
        }

        public bool ForceCaptchaForAnonymous
        {
            get { return _settings[SettingName.ForceCaptchaForAnonymous].AsBoolean(); }
        }

        public bool PreferReCaptcha
        {
            get { return _settings[SettingName.PreferReCaptcha].AsBoolean(); }
        }

        public string ReCaptchaSiteKey
        {
            get { return _settings[SettingName.ReCaptchaSiteKey].AsString(); }
        }

        public string ReCaptchaSecretKey
        {
            get { return _settings[SettingName.ReCaptchaSecretKey].AsString(); }
        }

        public string ListOrForm
        {
            get { return _settings[SettingName.ListOrForm].AsString("List"); }
        }

        public bool OnlyFormIsShown
        {
            get { return ListOrForm == "Form"; }
        }

        public bool OnlyListIsShown
        {
            get { return ListOrForm == "List"; }
        }

        public int Paging
        {
            get { return _settings[SettingName.Paging].AsInt(Null.NullInteger); }
        }

        public string RenderingMethod
        {
            get { return _settings[SettingName.RenderingMethod].AsString(); }
        }

        public bool ShowAllColumnsForAdmins
        {
            get { return _settings[SettingName.ShowAllColumnsForAdmins].AsBoolean(); }
        }

        public bool ShowNoRecordsUntilSearch
        {
            get { return _settings[SettingName.ShowNoRecordsUntilSearch].AsBoolean(); }
        }

        public bool ShowSearchTextBox
        {
            get { return _settings[SettingName.ShowSearchTextBox].AsBoolean(); }
        }

        public bool ShowSystemColumns
        {
            get { return _settings[SettingName.ShowSystemColumns].AsBoolean(); }
        }

        public bool SimpleSearch
        {
            get { return _settings[SettingName.SimpleSearch].AsBoolean(); }
        }

        public int SortFieldId
        {
            get { return _settings[SettingName.SortField].AsInt(Null.NullInteger); }
        }

        public string ScriptByRenderingMethod(string renderingMethod)
        {
            return _settings[renderingMethod].AsString();
        }

        public string SortOrder
        {
            get { return _settings[SettingName.SortOrder].AsString(); }
        }

        public string SubmissionText
        {
            get { return _settings[SettingName.SubmissionText].ToString(); }
        }

        public int TopCount
        {
            get { return _settings[SettingName.TopCount].AsInt(0); }
        }

        public string TrackingEmailFrom
        {
            get { return _settings[SettingName.TrackingEmailFrom].AsString(); }
        }

        public string TrackingEmailTo
        {
            get { return _settings[SettingName.TrackingEmailTo].AsString(); }
        }

        public string TrackingEmailCc
        {
            get { return _settings[SettingName.TrackingEmailCc].AsString(); }
        }

        public string TrackingEmailBcc
        {
            get { return _settings[SettingName.TrackingEmailBcc].AsString(); }
        }

        public string TrackingMessage
        {
            get { return _settings[SettingName.TrackingMessage].AsString(); }
        }

        public string TrackingEmailReplyTo
        {
            get { return _settings[SettingName.TrackingEmailReplyTo].AsString(); }
        }



        public string TrackingSubject
        {
            get { return _settings[SettingName.TrackingSubject].AsString(); }
        }

        public string TrackingScript
        {
            get { return _settings[SettingName.TrackingScript].AsString(); }
        }

        public string TrackingTextOnNew
        {
            get { return _settings[SettingName.TrackingTextOnNew].AsString(); }
        }

        public string TrackingTextOnUpdate
        {
            get { return _settings[SettingName.TrackingTextOnUpdate].AsString(); }
        }

        public string TrackingTextOnDelete
        {
            get { return _settings[SettingName.TrackingTextOnDelete].AsString(); }
        }

        public bool TrackingTriggerOnNew
        {
            get { return _settings[SettingName.TrackingTriggerOnNew].AsBoolean(); }
        }
        public bool TrackingTriggerOnUpdate
        {
            get { return _settings[SettingName.TrackingTriggerOnUpdate].AsBoolean(); }
        }
        public bool TrackingTriggerOnDelete
        {
            get { return _settings[SettingName.TrackingTriggerOnDelete].AsBoolean(); }
        }


        public string UponSubmitAction
        {
            get { return _settings[SettingName.UponSubmitAction].AsString("Text"); }
        }

        public string UponSubmitRedirect
        {
            get
            {
                var redirect = _settings[SettingName.UponSubmitRedirect].AsString();
                return redirect == string.Empty ? null : redirect;
            }
        }

        public bool UrlSearch
        {
            get { return _settings[SettingName.URLSearch].AsBoolean(); }
        }

        public bool UseButtonsInForm
        {
            get { return _settings[SettingName.UseButtonsInForm].AsBoolean(); }
        }

        public int UserRecordQuota
        {
            get { return _settings[SettingName.UserRecordQuota].AsInt(); }
        }

        public bool EnableFormTemplate
        {
            get { return _settings[SettingName.EnableFormTemplate].AsBoolean(); }
        }

        public string FormTemplate
        {
            get { return _settings[SettingName.FormTemplate].AsString(); }
        }
    }
}