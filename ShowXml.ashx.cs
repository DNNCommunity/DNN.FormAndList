using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// <summary>
    /// Summary description for ShowXml
    /// </summary>
    public class ShowXml : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                PortalController.GetCurrentPortalSettings();

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

                var userInfo = UserController.GetCurrentUserInfo();
                var mc = new ModuleController();
                var settings = mc.GetModuleSettings(moduleId);
                var moduleInfo = new ModuleController().GetModule(moduleId, tabId);

                if (ModulePermissionController.CanManageModule(moduleInfo))
                {
                    var udt = new UserDefinedTableController(moduleId, tabId, userInfo);
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