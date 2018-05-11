using System;
using System.Globalization;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit &amp; Validation Control for DataType "Currency"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditCurrency : EditString
    {
        protected override bool IsValidType()
        {
            decimal d;
            return Value == string.Empty || decimal.TryParse(Value, out d);
        }

        public override string Value
        {
            get
            {
                string returnValue;
                try
                {
                    returnValue = string.Empty;
                    if (base.Value != "")
                    {
                        returnValue = (decimal.Parse(base.Value)).ToString(CultureInfo.CurrentCulture);
                    }
                }
                catch
                {
                    returnValue = base.Value;
                }
                return returnValue;
            }
            set
            {
                
                if (value != "")
                {
                    value = (decimal.Parse(value).ToString(CultureInfo.InvariantCulture));
                }
                base.Value = value;
            }
        }

        void EditCurrency_Init(object sender, EventArgs e)
        {
            CtlValueBox.Attributes.Add("style", "text-align:right");
            CtlValueBox.Width = new Unit("10em");
            var ctlSym = new Label
                             {
                                 CssClass = "Normal",
                                 Text =
                                     string.Format("&nbsp;{0}",
                                                   OutputSettings == string.Empty
                                                       ? PortalController.Instance.GetCurrentPortalSettings().Currency
                                                       : OutputSettings)
                             };
            Controls.Add(ctlSym);
        }

        public EditCurrency()
        {
            Init += EditCurrency_Init;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Currency"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeCurrency : DataTypeDecimal
    {
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            base.SetStylesAndFormats(column, format);
            column.DataFormatString = string.Format("{{0:#,###,##0.00 \'{0}\'}}", 
                format == string.Empty ? PortalController.Instance.GetCurrentPortalSettings().Currency : format);
        }

        public override string Name
        {
            get { return "Currency"; }
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}Decimal", base.SupportedCasts); }
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