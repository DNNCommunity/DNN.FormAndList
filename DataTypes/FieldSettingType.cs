using System;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{
    public class FieldSettingType
    {
        public String Key { get; set; }
        public Boolean Localizeable { get; set; }
        public String SystemType { get; set; }
        public String Section { get; set; }
        Func<string, bool> _verifySetting;
        public Func<string, bool> VerifySetting
        {
            get { return _verifySetting ?? (input => true); }
            set { _verifySetting = value; }
        }
    }
}