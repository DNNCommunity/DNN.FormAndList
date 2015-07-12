using System;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using System.Linq;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "String"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditString : EditControl
    {
        protected WebControl CtlValueBox;
        protected string StrValRule = string.Empty;
        protected string StrValMsg = string.Empty;

        public EditString()
        {
            Init += EditString_Init;
        }

        void EditString_Init(object sender, EventArgs e)
        {
            if (IsNotAListOfValues)
            {
                var ctlTextBox = new TextBox {TextMode = TextBoxMode.SingleLine, Rows = 1};
                CtlValueBox = ctlTextBox;
                if (ValidationRule != "")
                {
                    StrValRule = ValidationRule;
                    StrValMsg = ValidationMessage;
                }
            }
            else
            {
                var ctlListControl = GetListControl();
                AddListItems(ctlListControl);
                CtlValueBox = ctlListControl;
            }
            Value = DefaultValue;
            if (Required) CtlValueBox.CssClass = "dnnFormRequired";
            if (!String.IsNullOrEmpty( Style)) CtlValueBox.Style.Value = Style;
            CtlValueBox.ID = CleanID(FieldTitle);
            ValueControl = CtlValueBox;
            Controls.Add(CtlValueBox);
        }


        public override string Value
        {
            get
            {
                string returnValue;
                if (CtlValueBox is TextBox)
                {
                    returnValue = FilterScript
                                      ? ApplyScriptFilter(((TextBox) CtlValueBox).Text)
                                      : ((TextBox) CtlValueBox).Text;
                    if (FilterTags)
                    {
                        returnValue = HtmlUtils.StripTags(returnValue, true);
                    }
                }
                else
                {
                    if (MultipleValuesFlag)
                        returnValue = ((ListControl) CtlValueBox).Items
                            .Cast<ListItem >()
                            .Where(i => i.Selected)
                            .Aggregate( "",(c,i)=>c + i.Value  +";")
                            .TrimEnd(';');
                    else
                        returnValue = ((ListControl)CtlValueBox).SelectedValue;
                }
                return returnValue;
            }
            set
            {
                if (CtlValueBox is TextBox)
                {
                    ((TextBox) CtlValueBox).Text = value;
                }
                else
                {
                    var ctlListControl = (ListControl) CtlValueBox;
                    if (MultipleValuesFlag)
                    {
                        var existingItems = value.Split(';')
                            .Select(entry => ctlListControl.Items.FindByValue(entry))
                            .Where(entry=>entry!=null);
                        foreach (var item in existingItems)
                        {
                            item.Selected = true;
                        }
                    }

                    else
                    if (ctlListControl.Items.FindByValue(value) != null)
                    {
                        ctlListControl.SelectedValue = value;
                    }
                }
            }
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "String"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeString : DataType
    {
        public override string Name
        {
            get { return "String"; }
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}|TextHtml", base.SupportedCasts); }
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

        public override bool SupportsValidation
        {
            get { return true; }
        }

        public override bool SupportsSearch
        {
            get { return true; }
        }

        public override bool SupportsEditStyle
        {
            get { return true; }
        }

        public override bool SupportsMultipleValues
        {
            get { return true; }
        }
    }

    #endregion
}