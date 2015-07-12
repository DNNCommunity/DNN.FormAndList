using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable
{
    public partial class FieldEditor :  ModuleUserControlBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Field.HideField += Field_HideField;
            var fieldId = Request.QueryString["fieldId"];
            var fields = FieldController.GetFieldsTable(ModuleContext.ModuleId, fieldId=="-1" );
            var filter = string.Format("{0}={1}", FieldsTableColumn.Id, fieldId);
            Field.DataSource = new DataView(fields, filter, "", DataViewRowState.CurrentRows)[0];
            var fieldSettings = FieldSettingsController.GetFieldSettingsTable(ModuleContext.ModuleId);
            Field.Settings = fieldSettings;
            Field.Bind();
            Field.Visible = true;
            Field.LocalizeString = LocalizeString;
            Field.ModuleContext = ModuleContext;
       
        }


        void Field_HideField()
        {
            Response.Redirect(ModuleContext.EditUrl("Manage"));
        }
    }
}