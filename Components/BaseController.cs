using System.Collections;
using System.Globalization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public abstract class BaseController
    {
        protected BaseController()
        {
            TabId = Null.NullInteger;
            TabModuleId = Null.NullInteger;
        }

        Hashtable _moduleSettings;

        ModuleInfo _configuration;
        PortalInfo _portalInfo;
        Components.Settings _settings;

        public Components.Settings  Settings
        {
            get
            {
                if (_settings == null)
                {
                    if (_moduleSettings == null)
                    {
                        var mc = new ModuleController();
                        _configuration = mc.GetModule(ModuleId, TabId);
                        _moduleSettings = new Hashtable(_configuration.ModuleSettings);
                        if (TabModuleId != Null.NullInteger)
                        {
                            var tabModuleSettings = _configuration.TabModuleSettings;
                            foreach (string strKey in tabModuleSettings.Keys)
                            {
                                _moduleSettings[strKey] = tabModuleSettings[strKey];
                            }
                        }
                    }
                    _settings = new Settings(_moduleSettings);
                }
                return _settings;
            }
        }

        public ModuleInfo Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
                ModuleId = value.ModuleID;
                TabModuleId = value.TabModuleID;
                TabId = value.TabID;
                PortalId = value.PortalID;
            }
        }

        public int TabModuleId { get; set; }

        public UserInfo User { get; set; }

        public int TabId { get; set; }

        public PortalInfo PortalInfo
        {
            get { return _portalInfo ?? (_portalInfo = new PortalController().GetPortal(PortalId)); }
        }

        public int ModuleId { get; set; }


        protected int PortalId { get; set; }


        public string EditUrlPattern { get; set; }

        public void Initialise(ModuleInstanceContext context)
        {
            _moduleSettings  = context.Settings;
            Configuration = context.Configuration;
            User = context.PortalSettings.UserInfo;
         
            EditUrlPattern = context.EditUrl(  DataTableColumn.RowId.ToString(CultureInfo.InvariantCulture), "{0}","edit");
        }

        public void Initialise(int moduleId, int tabId, UserInfo user)
        {
            ModuleId = moduleId;
            TabId = tabId;
            User = user;
        }

        public void Initialise(int moduleId)
        {
            ModuleId = moduleId;
        }

        public void Initialise(ModuleInfo moduleInfo)
        {
            Configuration = moduleInfo;
        }
    }
}