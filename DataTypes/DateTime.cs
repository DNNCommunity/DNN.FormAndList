using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.Localization;
using Microsoft.VisualBasic;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "DateTime"
    ///   A composition from the Controls Date and Time
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditDateTime : EditControl
    {
        EditTime _ctlTime;
        EditDate _ctlDate;
        bool _convertTimezone;

        public override string Value
        {
            get
            {
                if (_ctlDate.Value == "" && _ctlTime.Value == "")
                {
                    return "";
                }
                if (Information.IsDate(_ctlDate.Value) && Information.IsDate(_ctlTime.Value))
                {
                    var d = DateTime.Parse(_ctlDate.Value);
                    var T = DateTime.Parse(_ctlTime.Value);
                    d = new DateTime(d.Year, d.Month, d.Day, T.Hour, T.Minute, T.Second);
                    var timeZone = _convertTimezone
                                       ? UserController.Instance.GetCurrentUserInfo().Profile.PreferredTimeZone
                                       : ModuleContext.PortalSettings.TimeZone;
                    d = TimeZoneInfo.ConvertTimeToUtc( d, timeZone);
                    return d.ToString("s");
                }
                return string.Format("{0} {1}", _ctlDate.Value, _ctlTime.Value);
                //invalid Value
            }

            set
            {
                if (Information.IsDate(value))
                {
                    var d = DateTime.Parse(value);
                    if (value.Contains("+")) d = TimeZoneInfo.ConvertTimeToUtc(d);
                    var timeZone = _convertTimezone
                                      ? UserController.Instance.GetCurrentUserInfo().Profile.PreferredTimeZone
                                      : ModuleContext.PortalSettings.TimeZone;
                    value = TimeZoneInfo.ConvertTimeFromUtc(d,timeZone ).ToString( "s"); 
                }
                _ctlDate.Value = value;
                _ctlTime.Value = value;
            }
        }

        protected override bool IsValidType()
        {
            if (_ctlTime.Value == string.Empty && _ctlDate.Value == string.Empty)
            {
                return true;
            }
            try
            {
               // ReSharper disable ReturnValueOfPureMethodIsNotUsed
               DateTime.ParseExact(_ctlDate.Value, new[] {"d", "D", "s"}, Thread.CurrentThread.CurrentCulture, DateTimeStyles.None);
               DateTime.ParseExact(_ctlTime.Value, new[] {"t", "T", "s"}, Thread.CurrentThread.CurrentCulture, DateTimeStyles.None);
               // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        void EditDateTime_Init(object sender, EventArgs e)
        {
            _ctlDate = new EditDate();
            _ctlDate.Initialise(FieldTitle, FieldType, FieldId, ModuleId, HelpText, DefaultValue, Required,
                               ValidationRule, ValidationMessage, Style, InputSettings, OutputSettings, NormalizeFlag,
                               MultipleValuesFlag, FilterTags, FilterScript, ListInputType, ModuleContext, FieldSettingsTable, FormEvents );
            Controls.Add(_ctlDate);
            Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
            var ctlTimeLbl = new Label
                                 {
                                     Text = string.Format("{0}: ", Localization.GetString("Time", LocalResourceFile)),
                                     CssClass = "SubHead"
                                 };
            Controls.Add(ctlTimeLbl);
            _ctlTime = new EditTime();
            _ctlTime.Initialise(FieldTitle, FieldType, FieldId, ModuleId, HelpText, DefaultValue, Required,
                               ValidationRule, ValidationMessage, Style, InputSettings, OutputSettings, NormalizeFlag,
                               MultipleValuesFlag, FilterTags, FilterScript, ListInputType, ModuleContext,  FieldSettingsTable, FormEvents );
            Controls.Add(_ctlTime);
            _convertTimezone = GetFieldSetting("ConvertToUserDateTime").AsBoolean();
            Value = DefaultValue;
        }

        public EditDateTime()
        {
            Init += EditDateTime_Init;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "DateTime"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeDateTime : DataTypeDate
    {
        readonly FieldSettingType[] _fieldSettingTypes = new[]
                    {
                        new FieldSettingType {Key = "ConvertToUserDateTime", Section = "List", SystemType = "Boolean"}
                    };

        public override IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get
            {
                return _fieldSettingTypes;
            }
        }
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            base.SetStylesAndFormats(column, format);
            column.DataFormatString = format != string.Empty ? string.Format("{{0:{0}}}", format) : "{0:g}";
        }

        public override string Name
        {
            get { return "DateTime"; }
        }

        struct FieldSetting
        {
            public string Title;
            public bool ConvertToUserTime;
            public string FormatString;
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var fields = new ArrayList();
            //List of columns that contains DateTime values
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int)row[FieldsTableColumn.Id];
                    var field = new FieldSetting
                                    {
                                        ConvertToUserTime = GetFieldSetting("ConvertToUserDateTime", fieldId, ds).AsBoolean(),
                                        Title = row[FieldsTableColumn.Title].ToString(),
                                        FormatString = row[FieldsTableColumn.OutputSettings].AsString("g")
                                    };
                    fields.Add(field);
                    var renderedValueColumnName = field.Title + DataTableColumn.Appendix_LocalizedValue;
                    ds.Tables[DataSetTableName.Data].Columns.Add(renderedValueColumnName, typeof (string));
                    ds.Tables[DataSetTableName.Data].Columns.Add(field.Title + DataTableColumn.Appendix_Ticks, typeof (long));
                    row[FieldsTableColumn.ValueColumn] = XmlConvert.EncodeName(renderedValueColumnName);
                }
            }
            
         
            if (HttpContext.Current != null)
            {
                var serverTimeZone = PortalController.Instance.GetCurrentPortalSettings().TimeZone;
                var userTimeZone = UserController.Instance.GetCurrentUserInfo().Profile.PreferredTimeZone;
               
                foreach (DataRow row in ds.Tables[DataSetTableName.Data].Rows)
                {
                    foreach (FieldSetting field in fields)
                    {
                        if (Information.IsDate(row[field.Title]))
                        {
                            var d = Convert.ToDateTime(row[field.Title]);
                            var timeZone = field.ConvertToUserTime ? userTimeZone : serverTimeZone;
                            d = TimeZoneInfo.ConvertTimeFromUtc(d, timeZone);
                            row[field.Title] = d;
                            var format = "<!--{0:000000000000}-->{1:" + field.FormatString + "}";
                            row[field.Title + DataTableColumn.Appendix_LocalizedValue] = string.Format(format,d.Ticks/10000000,d);
                            row[field.Title + DataTableColumn.Appendix_Ticks] = d.Ticks;
                        }
                    }
                }
            }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsInputSettings
        {
            get { return false; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }

        public override bool SupportsValidation
        {
            get { return true; }
        }
    }

    #endregion
}