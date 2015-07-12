using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.UserDefinedTable.Controls
{
    public partial class FieldSettings : System.Web.UI.UserControl
    {
      
        public string Section { get; set; }
        public string TypeName { get; set; }
        public DataType SelectedType { get; set; }

        public void Show(DataType selectedType)
        {
            TypeName = selectedType.Name;
            SelectedType = selectedType;
            Repeater1.DataSource = selectedType.FieldSettingTypes.Where(t=>t.Section==Section);
            Repeater1.DataBind();
        }

        public void BindData(int fieldId,DataTable  settingsTable, DataType type)
        {
            
            if (!IsPostBack)
            {
                var types = type.FieldSettingTypes.Where(t => t.Section == Section).ToArray();
                for (var i = 0; i < types.Count(); i++)
                {
                    var key = types[i].Key;
                    var value = settingsTable.GetFieldSetting(key, fieldId);
                    if (!string.IsNullOrEmpty(value))
                    {
                        var t = type.FieldSettingTypes.ElementAt(i).SystemType;
                        if (t == "Boolean")
                        {
                            var cb = Repeater1.Controls[i].FindControl("Checkbox") as CheckBox;
                            if (cb != null) cb.Checked = value.AsBoolean();
                        }
                        else
                        {
                            var tb = Repeater1.Controls[i].FindControl("Textbox") as TextBox;
                            if (tb != null) tb.Text = value;
                        }
                    }
                   
                }
            }
        }

       public  void Update(int fieldId, DataType type)
        {
            var types = type.FieldSettingTypes.Where(t => t.Section == Section).ToArray(); 
           for (var i = 0; i < types.Count(); i++)
            {
                var value = string.Empty;
                var t = types[i].SystemType;
                if (t == "Boolean")
                {
                    var cb = Repeater1.Controls[i].FindControl("Checkbox") as CheckBox;
                    if (cb != null) value = cb.Checked.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    var tb = Repeater1.Controls[i].FindControl("Textbox") as TextBox;
                    if (tb != null) value = tb.Text;
                }

                if (t == "Int")
                {
                    value = value.AsInt().ToString(CultureInfo.InvariantCulture);
                }

                var key = types[i].Key;
                if (types[i].VerifySetting (value))
                FieldSettingsController.UpdateFieldSetting(key, value, fieldId);
            }

        }
    }
}