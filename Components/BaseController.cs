using System.Collections;
using System.Globalization;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public abstract class BaseController
    {
        private readonly INavigationManager navigationManager;
        private readonly IPortalAliasService portalAliasService;

        protected BaseController(INavigationManager navigationManager, IPortalAliasService portalAliasService)
        {
            this.navigationManager = navigationManager;
            this.portalAliasService = portalAliasService;

            TabId = Null.NullInteger;
            TabModuleId = Null.NullInteger;
        }

        Hashtable _moduleSettings;

        ModuleInfo _configuration;
        PortalInfo _portalInfo;
        Components.Settings _settings;

        protected INavigationManager NavigationManager => navigationManager;

        protected IPortalAliasService PortalAliasService => portalAliasService;

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

        /// <summary>
        /// Sanitizes a URL by ensuring it has only one question mark separator between the base URL and query parameters.
        /// This prevents duplicate question marks when formatting URL templates with parameters.
        /// </summary>
        /// <param name="url">The URL to sanitize</param>
        /// <returns>A sanitized URL with proper query string formatting</returns>
        public static string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // Find the first occurrence of '?'
            var firstQuestionMarkIndex = url.IndexOf('?');
            
            if (firstQuestionMarkIndex == -1)
                return url; // No query parameters, return as-is
            
            // Split into base URL and query parameters
            var baseUrl = url.Substring(0, firstQuestionMarkIndex);
            var queryPart = url.Substring(firstQuestionMarkIndex + 1);
            
            // Remove any additional '?' characters from the query part and replace with '&'
            queryPart = queryPart.Replace('?', '&');
            
            // Reconstruct the URL
            return baseUrl + "?" + queryPart;
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