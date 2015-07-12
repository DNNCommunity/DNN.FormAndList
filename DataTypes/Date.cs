using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Web.UI.WebControls;
using Microsoft.VisualBasic;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit &amp; Validation Control for DataType "Date"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditDate : EditControl
    {
        
        protected WebControl CtlValueBox;

        void EditDate_Init(object sender, EventArgs e)
        {
            if (IsNotAListOfValues)
            {
                var ctlDate = new DnnDatePicker {MinDate = DateTime.MinValue,MaxDate=DateTime.MaxValue };
                if (! string.IsNullOrEmpty(Style))
                {
                    ctlDate.Style.Value = Style;
                }
                ctlDate.ID = CleanID(string.Format("{0}_date", FieldTitle));
                if (Required) ctlDate.DateInput.CssClass = "dnnFormRequired";
              
                Controls.Add(ctlDate);
                CtlValueBox = ctlDate;
                ValueControl = ctlDate;
            }
            else
            {
                var ctlListControl = GetListControl();
                
                foreach (var v in InputValueList)
                {
                    if (Information.IsDate(v))
                    {
                        var d = DateTime.Parse(v.Trim());
                        ctlListControl.Items.Add(new ListItem(d.ToString("d"), d.ToString("s")));
                    }
                }
                if (! Required)
                {
                    ctlListControl.Items.Add(new ListItem("", ""));
                }
                ctlListControl.CssClass = "NormalTextBox";
                if (!String.IsNullOrEmpty(Style)) CtlValueBox.Style.Value = Style;
                ctlListControl.ID = CleanID(string.Format("{0}_date", FieldTitle));
                Controls.Add(ctlListControl);
                CtlValueBox = ctlListControl;
            }
            Value = DefaultValue;
        }

     

        public override string Value
        {
            get
            {
                string returnValue;
                if (CtlValueBox is DnnDatePicker)
                {
                    var dnnDatePicker = (DnnDatePicker)CtlValueBox;
                    returnValue = dnnDatePicker.SelectedDate.HasValue ? dnnDatePicker.SelectedDate.Value.ToString("s"):"" ;
                }
                else
                {
                    returnValue = ((DropDownList) CtlValueBox).SelectedValue;
                }
                if (returnValue != string.Empty)
                {
                    if (Information.IsDate(returnValue))
                    {
                        returnValue = DateTime.Parse(returnValue).ToString("s");
                    }
                }
                else
                {
                    returnValue = Null.NullString;
                }
                return returnValue;
            }
            set
            {
                if (Information.IsDate(value))
                {
                    var d = DateTime.Parse(value);
                    if (CtlValueBox is DnnDatePicker )
                    {
                        ((DnnDatePicker)CtlValueBox).SelectedDate= DateTime.Parse(value);
                    }
                    else
                    {
                        var ctlComboBox = (ListControl) CtlValueBox;
                        if (ctlComboBox.Items.FindByValue(d.ToString("s")) != null)
                        {
                            ctlComboBox.SelectedValue = d.ToString("s");
                        }
                    }
                }
                else
                {
                    if (CtlValueBox is DnnDatePicker)
                    {
                        //((DnnDatePicker)CtlValueBox).Text = string.Empty;
                    }
                    else
                    {
                        var ctlComboBox = (ListControl) CtlValueBox;
                        if (Required)
                        {
                            ctlComboBox.SelectedIndex = 0;
                        }
                        else
                        {
                            ctlComboBox.SelectedValue = "";
                        }
                    }
                }
            }
        }


        public EditDate()
        {
            Init += EditDate_Init;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Date"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeDate : DataType
    {
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            column.DataFormatString = format != "" ? string.Format("{{0:{0}}}", format) : "{0:d}";
            column.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
            column.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
        }

        public override string Name
        {
            get { return "Date"; }
        }

        public override string SystemTypeName
        {
            get { return "DateTime"; }
        }

        struct FieldSetting
        {
            public string FormatString;
            public string Title;
        }


        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var fields = new ArrayList();
            //List of columns that contains URLs
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldTitle = row[FieldsTableColumn.Title].ToString();
                    var fieldSetting = new FieldSetting
                                           {
                                               FormatString =
                                                   row[FieldsTableColumn.OutputSettings].AsString("d"),
                                               Title = fieldTitle
                                           };
                    fields.Add(fieldSetting);
                    var localizedValueColumnName = fieldTitle + DataTableColumn.Appendix_LocalizedValue;
                    ds.Tables[DataSetTableName.Data].Columns.Add(localizedValueColumnName, typeof (string));
                    ds.Tables[DataSetTableName.Data].Columns.Add(fieldTitle + DataTableColumn.Appendix_Ticks,
                                                                 typeof (long));
                    row[FieldsTableColumn.ValueColumn] = XmlConvert.EncodeName(localizedValueColumnName);
                }
            }
            foreach (DataRow row in ds.Tables[DataSetTableName.Data].Rows)
            {
                foreach (FieldSetting field in fields)
                {
                    if (Information.IsDate(row[field.Title]))
                    {
                        var d = Convert.ToDateTime(row[field.Title]);
                        var format = "<!--{0:000000000000}-->{1:" + field.FormatString + "}";
                        row[field.Title + DataTableColumn.Appendix_LocalizedValue] = string.Format(format, d.Ticks/10000000, d);
                        row[field.Title + DataTableColumn.Appendix_Ticks] = d.Ticks;
                    }
                }
            }
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}|Date|DateTime|Time", base.SupportedCasts); }
        }

        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsEditing
        {
            get { return true; }
        }

        public override bool SupportsInputSettings
        {
            get { return true; }
        }

        public override bool SupportsEditStyle
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return true; }
        }

        public override bool SupportsValidation
        {
            get { return true; }
        }

        public override bool SupportsSearch
        {
            get { return true; }
        }
    }

    #endregion
}