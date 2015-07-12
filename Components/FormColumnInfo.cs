using System.Web.UI;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class FormColumnInfo
    {
        bool _isUserDefinedField = true;


        public string Title { get; set; }
        public string Help { get; set; }
        public Control EditControl { get; set; }
        public bool Visible { get; set; }
        public bool Required { get; set; }
        public bool IsCollapsible { get; set; }
        public bool IsSeparator { get; set; }

        public bool IsUserDefinedField
        {
            get { return _isUserDefinedField; }
            set { _isUserDefinedField = value; }
        }

        public Control ValueControl
        {
            get
            {
                if ((EditControl) is EditControl)
                {
                    return ((EditControl) EditControl).ValueControl;
                }
                else
                {
                    return (null);
                }
            }
        }


        public string FieldType
        {
            get
            {
                if ((EditControl) is EditControl)
                {
                    return ((EditControl) EditControl).FieldType;
                }
                else if (IsSeparator)
                {
                    return "Separator";
                }
                else if (IsCollapsible)
                {
                    return "FieldSet";
                }
                else
                {
                    return "Captcha";
                }
            }
        }
    }
}