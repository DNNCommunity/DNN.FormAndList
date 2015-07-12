using System;
using System.Collections;
using System.Data;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class ModuleSecurity
    {
        readonly bool _hasEditRowPermission;
        readonly bool _hasDeleteRowPermission;
        readonly bool _hasAddRowPermission;
        readonly bool _hasShowAllUserDefinedColumnsPermission;
        readonly bool _hasEditPrivateColumnsPermission;
        readonly bool _hasViewListPermission;
        readonly bool _isOnlyAllowedToManipulateHisOwnData;
        readonly bool _canEditModuleContent;
        readonly bool _canManageModule;

        Components.Settings  Settings { get; set; }

        //public ModuleSecurity(int moduleId, int tabId, Hashtable settings)
        //{
        //    var moduleController = new ModuleController();
        //    var s = settings ?? moduleController.GetModuleSettings(moduleId);
        //    Settings = new Settings(s);
        //    return this.ModuleSecurity(moduleId, tabId, Settings);
        //}

        public ModuleSecurity(int moduleId, int tabId, Components.Settings settings)
        {
            var moduleController = new ModuleController();
            Settings = settings ?? new Settings(moduleController.GetModuleSettings(moduleId));
            var moduleInfo = moduleController.GetModule(moduleId, tabId);
            if (moduleInfo == null) return;
            var mp = moduleInfo.ModulePermissions;
            _hasEditRowPermission = ModulePermissionController.HasModulePermission(mp, PermissionName.HasEditRowPermission);
            _hasDeleteRowPermission = ModulePermissionController.HasModulePermission(mp, PermissionName.HasDeleteRowPermission);
            _hasAddRowPermission = ModulePermissionController.HasModulePermission(mp,PermissionName.HasAddRowPermission);
            _hasEditPrivateColumnsPermission = ModulePermissionController.HasModulePermission(mp,PermissionName.EditRestricedFieldsPermission);
            _hasShowAllUserDefinedColumnsPermission = ModulePermissionController.HasModulePermission(mp,PermissionName.ShowAllUserDefinedColumnsPermission);
            _canEditModuleContent = ModulePermissionController.CanEditModuleContent(moduleInfo);
            _canManageModule = ModulePermissionController.CanManageModule(moduleInfo);

            _hasViewListPermission = ModulePermissionController.HasModulePermission(mp,
                                                                                    PermissionName.ShowListPermission);
            _isOnlyAllowedToManipulateHisOwnData = Settings.EditOnlyOwnItems;
        }

        public ModuleSecurity(int moduleId, int tabId) : this(moduleId, tabId, null)
        {
        }

        public ModuleSecurity(ModuleInstanceContext context) : this(context.ModuleId, context.TabId, new Settings( context.Settings ))
        {
        }

        public bool IsAllowedToEditRow(bool isUsersOwnRow = true)
        {
            return _canEditModuleContent ||
                   (_hasEditRowPermission && (! _isOnlyAllowedToManipulateHisOwnData || isUsersOwnRow));
        }

        public bool IsAllowedToDeleteRow(bool isUsersOwnRow = true)
        {
            return _canEditModuleContent ||
                   (_hasDeleteRowPermission && (! _isOnlyAllowedToManipulateHisOwnData || isUsersOwnRow));
        }

        public bool IsAllowedToAddRow()
        {
            return _canEditModuleContent || _hasAddRowPermission;
        }

        public bool IsAllowedToEditAllColumns()
        {
            return _hasEditPrivateColumnsPermission &&
                   Settings.EditPrivateColumnsForAdmins  ||
                   _canEditModuleContent && Settings.EditPrivateColumnsForAdmins ;
        }

        public bool IsAllowedToSeeAllUserDefinedColumns()
        {
            return _hasShowAllUserDefinedColumnsPermission && Settings.ShowAllColumnsForAdmins;
        }

        public bool IsAllowedToAdministrateModule()
        {
            return _canManageModule;
        }

        public bool IsAllowedToViewList()
        {
            switch (Settings.ListOrForm )
            {
                case "Form": //List is normaly hidden
                    return _canEditModuleContent || _hasViewListPermission;
                case "FormAndList":
                case "ListAndForm":
                    return _hasViewListPermission;
                default:
                    return false;
            }
        }

        public static bool IsAdministrator()
        {
            var administratorRoleName = Globals.GetPortalSettings().AdministratorRoleName;
            return PortalSecurity.IsInRole(administratorRoleName);
        }

        public static string RoleNames(UserInfo user)
        {
            string roles = user.Roles != null ? string.Format("|{0}", string.Join("|", user.Roles)) : "";
            roles += string.Format("|{0}", Globals.glbRoleAllUsersName);
            if (IsAdministrator())
            {
                roles += string.Format("|{0}", Globals.GetPortalSettings().AdministratorRoleName);
            }
            return string.Format("{0}|", roles);
        }


        public static string BestUserName(string userName, int portalId)
        {
            var cacheKey = string.Format("UDT_BestUserNameFor{0}", userName);

            var bestUserName = DataCache.GetCache(cacheKey).AsString();

            if (bestUserName == string.Empty)
            {
                var ui = UserController.GetUserByName(portalId, userName);
                bestUserName = ui == null ? userName.AsString("Unknown Account") : ui.DisplayName;

                // cache data
                var intCacheTimeout = Convert.ToInt32(20*Convert.ToInt32(Host.PerformanceSetting));
                DataCache.SetCache(cacheKey, bestUserName, TimeSpan.FromMinutes(intCacheTimeout));
            }

            return bestUserName;
        }

        public static int UserId(string username, int portalId)
        {
            var strCacheKey = string.Format("UDT_UserIDFor{0}", username);

            var id = DataCache.GetCache(strCacheKey).AsInt();

            if (id == Null.NullInteger)
            {
                var ui = UserController.GetUserByName(portalId, username);
                id = ui == null ? 0 : Convert.ToInt32(ui.UserID);

                // cache data
                var intCacheTimeout = Convert.ToInt32(20*Convert.ToInt32(Host.PerformanceSetting));
                DataCache.SetCache(strCacheKey, id, TimeSpan.FromMinutes(intCacheTimeout));
            }
            return id;
        }


        public static bool HasAddPermissonByQuota(DataTable fieldsTable, DataTable dataTable, int userRecordQuota,
                                                  string username)
        {
            if (userRecordQuota > 0)
            {
                string titleOfCreatedByColumn;
                using (
                    var schemaOfCreatedByColumn = new DataView(fieldsTable,
                                                               string.Format("[{0}]=\'{1}\'", FieldsTableColumn.Type,
                                                                             DataTypeNames.UDT_DataType_CreatedBy), "",
                                                               DataViewRowState.CurrentRows))
                {
                    titleOfCreatedByColumn = schemaOfCreatedByColumn[0][FieldsTableColumn.Title].AsString();
                }


                var isDataTableWithPreRenderedValues =
                    dataTable.Columns.Contains(titleOfCreatedByColumn + DataTableColumn.Appendix_Original);
                if (isDataTableWithPreRenderedValues)
                {
                    titleOfCreatedByColumn = titleOfCreatedByColumn + DataTableColumn.Appendix_Original;
                }

                using (
                    var dv = new DataView(dataTable,
                                          string.Format("[{0}] = \'{1}\'", titleOfCreatedByColumn,
                                                        username.Replace("\'", "\'\'")), "",
                                          DataViewRowState.CurrentRows))
                {
                    return dv.Count < userRecordQuota;
                }
            }
            return true;
        }
    }
}