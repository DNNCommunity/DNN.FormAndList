using System;
using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Modules.UserDefinedTable.Templates;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Editor for Module templates (aka Module Applications)
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Template : ModuleUserControlBase
    {
        private readonly INavigationManager navigationManager;

        public Template()
        {
            // In DNN 10 we should be able to use constructor injection here instead of this reflection hack.
            var globalstype = typeof(Globals);
            var dependencyProviderProperty = globalstype.GetProperty("DependencyProvider",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var serviceProvider = dependencyProviderProperty.GetValue(null) as IServiceProvider;

            this.navigationManager = serviceProvider.GetRequiredService<INavigationManager>();
        }

        protected void cmdSaveFile_Click(object sender, EventArgs e)
        {
            if (TemplateController.SaveTemplate(txtTitle.Text, txtDescription.Text, ModuleContext, false,
                                                MaxNumberOfRecords()))
            {
                Response.Redirect(this.navigationManager.NavigateURL(ModuleContext.TabId), true);
            }
            else
            {
                panConfirm.Visible = true;
                panSave.Visible = false;
                cmdSaveFile.Enabled = false;
                txtDescription.Enabled = false;
                txtTitle.Enabled = false;
            }
        }

        protected void cmdConfirmOverwriteFile_Click(object sender, EventArgs e)
        {
            TemplateController.SaveTemplate(txtTitle.Text, txtDescription.Text, ModuleContext, true,
                                            MaxNumberOfRecords());
            Response.Redirect(this.navigationManager.NavigateURL(ModuleContext.TabId), true);
        }

        protected void cmdDenyOverwriteFile_Click(object sender, EventArgs e)
        {
            cmdSaveFile.Enabled = true;
            panSave.Visible = true;
            txtDescription.Enabled = true;
            txtTitle.Enabled = true;
            txtTitle.Text = "";
            panConfirm.Visible = false;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cmdConfirmOverwriteFile.Click += cmdConfirmOverwriteFile_Click;
            cmdDenyOverwriteFile.Click += cmdDenyOverwriteFile_Click;
            cmdSaveFile.Click  += cmdSaveFile_Click;
        }

        int MaxNumberOfRecords()
        {
            int returnValue;
            if (! int.TryParse(txtNumbers.Text, out returnValue))
            {
                returnValue = 1;
            }
            return returnValue;
        }
    }
}