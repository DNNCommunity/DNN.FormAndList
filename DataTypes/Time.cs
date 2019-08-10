using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using Microsoft.VisualBasic;
using DotNetNuke.Web.UI.WebControls;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl
    public class EditTime : EditControl
    {
        TextBox _ctlTime;


        public override string Value
        {
            get
            {
                var selectedDate = _ctlTime.Text;

                DateTime d = DateTime.MinValue;
                return DateTime.TryParse(selectedDate, out d)
                       ? d.ToString("s")
                       : ""; ;

            }
            set
            {
                if (Information.IsDate(value))
                {
                    var d = DateTime.Parse(value);
                    _ctlTime.Text = d.ToShortTimeString();
                }
            }
        }

        void EditTime_Init(object sender, EventArgs e)
        {
            //Time-Textbox
            _ctlTime = new TextBox
            {
                ID = CleanID(string.Format("{0}_time", FieldTitle))
            };

            if (!string.IsNullOrEmpty(Style))
            {
                _ctlTime.Style.Value = Style;
            }
            _ctlTime.CssClass = "fnl-timepicker";
            if (Required) _ctlTime.CssClass += " dnnFormRequired";

            Controls.Add(_ctlTime);
            Value = DefaultValue;
            ValueControl = _ctlTime;
        }

        public EditTime()
        {
            Init += EditTime_Init;
        }
    }


    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Date"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeTime : DataTypeDate
    {
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            base.SetStylesAndFormats(column, format.AsString("t"));
        }

        public override string Name
        {
            get { return "Time"; }
        }

        struct FieldSetting
        {
            public string Title;
            public string FormatString;
        }

        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var fields = new ArrayList();
            //List of columns that contains URLs
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int) row[FieldsTableColumn.Id];
                    var field = new FieldSetting
                                    {
                                        Title = row[FieldsTableColumn.Title].ToString(),
                                        FormatString = row[FieldsTableColumn.OutputSettings].AsString("t")
                                    };
                    fields.Add(field);
                    var renderedValueColumnName = field.Title + DataTableColumn.Appendix_LocalizedValue;
                    ds.Tables[DataSetTableName.Data].Columns.Add(renderedValueColumnName, typeof (string));
                    ds.Tables[DataSetTableName.Data].Columns.Add(field.Title + DataTableColumn.Appendix_Ticks,
                                                                 typeof (long));
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

                            var format = "<!--{0:000000000000}-->{1:" + field.FormatString + "}";
                            row[field.Title + DataTableColumn.Appendix_LocalizedValue] = string.Format(format, d.Ticks/10000000,d);
                            row[field.Title + DataTableColumn.Appendix_Ticks] = d.Ticks;
                        }
                    }
                }
            }
        }

        public override bool SupportsEditStyle
        {
            get { return true; }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }

        public override bool SupportsInputSettings
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