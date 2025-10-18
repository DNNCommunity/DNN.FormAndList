using System;
using System.Web;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// <summary>
    /// Summary description for ShowXml
    /// </summary>
    public class ShowXml : IHttpHandler
    {
        private readonly INavigationManager navigationManager;
        private readonly IPortalAliasService portalAliasService;

        public ShowXml()
        {
            // We may be able to use construction injection here in DNN 10 instead of this reflection hack.
            var globalsType = typeof(Globals);
            var dependencyProviderProperty = globalsType.GetProperty("DependencyProvider", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var dependencyProvider = dependencyProviderProperty.GetValue(null) as IServiceProvider;

            this.navigationManager = dependencyProvider.GetRequiredService<INavigationManager>();
            this.portalAliasService = dependencyProvider.GetRequiredService<IPortalAliasService>();
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                PortalController.Instance.GetCurrentSettings();

                if ((context.Request.QueryString["tabid"] == null || context.Request.QueryString["mid"] == null) ||
                    !(context.Request.IsAuthenticated))
                {
                    return;
                }
                // get TabId
                var tabId = Convert.ToInt32(-1);
                if (context.Request.QueryString["tabid"] != null)
                {
                    tabId = int.Parse(context.Request.QueryString["tabid"]);
                }

                // get ModuleId
                var moduleId = Convert.ToInt32(-1);
                if (context.Request.QueryString["mid"] != null)
                {
                    moduleId = int.Parse(context.Request.QueryString["mid"]);
                }

                var userInfo = UserController.Instance.GetCurrentUserInfo();
                var mc = new ModuleController();
               
                var moduleInfo = new ModuleController().GetModule(moduleId, tabId);
                var settings = moduleInfo.ModuleSettings;

                if (ModulePermissionController.CanManageModule(moduleInfo))
                {
                    var udt = new UserDefinedTableController(moduleId, tabId, userInfo, this.navigationManager, this.portalAliasService);
                    var ds = udt.GetDataSet(true);

                    ds.Tables.Add(udt.Context(moduleInfo, userInfo,
                                                 context.Request[Definition.QueryStringParameter].AsString(),
                                                 settings[SettingName.SortField].AsString(),
                                                 settings[SettingName.SortOrder].AsString(),
                                                 settings[SettingName.Paging].AsString()));
                    context.Response.ContentType = "Text/Xml";
                    ds.WriteXml(context.Response.OutputStream);
                }
            }
            catch (Exception)
            {
                context.Response.Write("Not defined");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}