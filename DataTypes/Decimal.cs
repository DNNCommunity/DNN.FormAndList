using System;
using System.Globalization;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Decimal"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditDecimal : EditString
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
                                      if (base.Value != string.Empty)
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

        void EditDecimal_Init(object sender, EventArgs e)
        {
            CtlValueBox.Attributes.Add("style", "text-align:right");
            CtlValueBox.Width = new Unit("10em");
        }

        public EditDecimal()
        {
            Init += EditDecimal_Init;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Decimal"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeDecimal : DataType
    {
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            column.DataFormatString = format == string.Empty ? "{0:#,###,##0.0###}" : string.Format("{{0:{0}}}", format);
            column.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
            column.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
        }

        public override string Name
        {
            get { return "Decimal"; }
        }

        public override string SystemTypeName
        {
            get { return "Decimal"; }
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}|Currency", base.SupportedCasts); }
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

        public override bool SupportsEditStyle
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