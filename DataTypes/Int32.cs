using System;
using System.Globalization;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Int32"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditInt32 : EditString
    {
        protected override bool IsValidType()
        {
            int i;
            return Value == string.Empty || int.TryParse(Value, out i);
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
                        returnValue = (int.Parse(base.Value)).ToString(CultureInfo.CurrentCulture);
                    }
                    //normalize format
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
                    value = (int.Parse(value).ToString(CultureInfo.InvariantCulture));
                }
                base.Value = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e); 
            CtlValueBox.Attributes.Add("style", "text-align:right");
            CtlValueBox.Width = new Unit("10em");
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Int32"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeInt32 : DataType
    {
        public override void SetStylesAndFormats(BoundField column, string format)
        {
            column.DataFormatString = format == string.Empty ? "{0:#,###,##0}" : string.Format("{{0:{0}}}", format);
            column.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
            column.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
        }

        public override string Name
        {
            get { return "Int32"; }
        }

        public override string SystemTypeName
        {
            get { return "Int32"; }
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}|Decimal|Currency", base.SupportedCasts); }
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

        public override bool SupportsSearch
        {
            get { return true; }
        }

        public override bool SupportsEditStyle
        {
            get { return true; }
        }
    }

    #endregion
}