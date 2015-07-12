using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable.Controls
{
    public partial class Fields : System.Web.UI.UserControl
    {  
        public ModuleInstanceContext ModuleContext { get; set; }
        public Func<string, string> LocalizeString { get; set; }

     
        public string LocalResourceFile { get; set; }

        protected Field Field;

        protected override void OnInit(EventArgs e)
        {
            Load += Page_Load;
            grdFields.DeleteCommand += grdFields_DeleteCommand;
            grdFields.ItemCreated += grdFields_ItemCreated;
            grdFields.ItemDataBound += grdFields_ItemDataBound;
           
        }
    
        void SetFieldOrder()
        {
            var fidString = FieldOrder.Value.Replace(",,", ",");
            var fids = fidString.Split(',');
            var ids = fids.Select(fid => int.Parse(fid.Split('_')[1]));
            FieldController.ChangeFieldOrder(ids);
            BindFields();
            FieldOrder.Value = string.Empty;
        }
        protected string EditUrl(int id)
        {
           var url = Globals.NavigateURL(ModuleContext.TabId, "EditField", new[]{"mid=" + ModuleContext.ModuleId , "fieldId=" + id});
           if (ModuleContext.PortalSettings.EnablePopUps) 
                url = UrlUtils.PopUpUrl(url, this, ModuleContext.PortalSettings, false, false, 760, 950);
           return url;
        }

        protected string Shorten(string input)
        {
            if (input.Length > 50)
                return input.Substring(0, 47) + "...";
            return input;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
         if (!Page.IsPostBack)
            {
                //Localize Grid
                Localization.LocalizeDataGrid(ref grdFields, LocalResourceFile);
                BindFields( );
                
            }
            else
            {
                if (Request["__EVENTARGUMENT"] == "FieldOrder") SetFieldOrder();
            }
        }

       void grdFields_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            try
            {
                var data = (DataRowView)e.Item.DataItem;
                if (data != null)
                {
                    var id = string.Format("fid_{0}", data[FieldsTableColumn.Id]);
                    e.Item.Attributes.Add("id", id);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void grdFields_DeleteCommand(object source, DataGridCommandEventArgs e)
        {
            try
            {
                var fields = FieldController.GetFieldsTable(ModuleContext.ModuleId, false);
                var fieldId = int.Parse(Convert.ToString(grdFields.DataKeys[e.Item.ItemIndex]));
                var fieldType =
                    fields.Select(string.Format("UserDefinedFieldId={0}", fieldId))[0][FieldsTableColumn.Type].AsString();

                if (DataType.ByName(fieldType).IsUserDefinedField)
                {
                    FieldController.DeleteField(fieldId);
                }

                grdFields.EditItemIndex = Convert.ToInt32(-1);
                BindFields();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void grdFields_ItemCreated(object sender, DataGridItemEventArgs e)
        {
            try
            {
                var cmdDeleteUserDefinedField = e.Item.FindControl("cmdDeleteUserDefinedField");

                if (cmdDeleteUserDefinedField != null)
                {
                    ClientAPI.AddButtonConfirm((WebControl)cmdDeleteUserDefinedField,
                                               LocalizeString("DeleteField"));
                }

                if (e.Item.ItemType == ListItemType.Header)
                {
                    e.Item.Cells[1].Attributes.Add("scope", "col");
                    e.Item.Cells[2].Attributes.Add("scope", "col");
                    e.Item.Cells[3].Attributes.Add("scope", "col");
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected DataType DataTypeByName(string name)
        {
            return DataType.ByName(name);
        }
        protected string GetTypeName(string name)
        {
            var type = DataType.ByName(name);
            if (type.Name == name)
            {
                return type.GetLocalization();
            }

            name = new PortalSecurity().InputFilter(name, PortalSecurity.FilterFlag.NoMarkup);
            UI.Skins.Skin.AddModuleMessage(this,
                                           string.Format(
                                               Localization.GetString("DataTypeNotInstalled", LocalResourceFile),
                                               name), ModuleMessage.ModuleMessageType.RedError);
            return string.Format("<img src=\"{1}/images/deny.gif\" alt=\"{0} not installed\" /> {0}", name,
                                 Globals.ApplicationPath);
        }
        void BindFields()
        {
            var fields = FieldController.GetFieldsTable(ModuleContext.ModuleId);
            var rowCount = fields.Rows.Count;
          
            grdFields.DataSource = fields.DefaultView;
            grdFields.DataBind();
            grdFields.Visible = Convert.ToBoolean(rowCount != 0);

            cmdAddField.NavigateUrl = EditUrl(-1);
        }
    }
}