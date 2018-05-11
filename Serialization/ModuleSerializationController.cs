using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.EventQueue;

namespace DotNetNuke.Modules.UserDefinedTable.Serialization
{
    public class ModuleSerializationController
    {
        #region Private Shared Methods

        static void AddContent(XmlNode nodeModule, ModuleInfo module, int maxNumberOfRecords)
        {
            if (module.DesktopModule.BusinessControllerClass != "" && module.DesktopModule.IsPortable)
            {
                try
                {
                    var businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass,
                                                                     module.DesktopModule.BusinessControllerClass);

                    var content = string.Empty;
                    if (businessController is IPortable2)
                    {
                        content =
                            Convert.ToString(((IPortable2) businessController).ExportModule(module.ModuleID,
                                                                                            module.TabID,
                                                                                            maxNumberOfRecords));
                    }
                    else if (businessController is IPortable)
                    {
                        content = Convert.ToString(((IPortable) businessController).ExportModule(module.ModuleID));
                    }

                    if (content != "")
                    {
                        // add attributes to XML document
// ReSharper disable PossibleNullReferenceException
                        XmlNode newnode = nodeModule.OwnerDocument.CreateElement("content");

                        var xmlattr = nodeModule.OwnerDocument.CreateAttribute("type");
                        xmlattr.Value = Globals.CleanName(module.DesktopModule.ModuleName);
                        newnode.Attributes.Append(xmlattr);
                        xmlattr = nodeModule.OwnerDocument.CreateAttribute("version");
                        xmlattr.Value = module.DesktopModule.Version;
                        newnode.Attributes.Append(xmlattr);

                        try
                        {
                            var doc = new XmlDocument();
                            doc.LoadXml(content);
// ReSharper disable AssignNullToNotNullAttribute
                            newnode.AppendChild(newnode.OwnerDocument.ImportNode(doc.DocumentElement, true));
// ReSharper restore AssignNullToNotNullAttribute
                        }
                            // ReSharper restore PossibleNullReferenceException
                        catch (Exception)
                        {
                            //only for invalid xhtml
                            content = HttpContext.Current.Server.HtmlEncode(content);
                            newnode.InnerXml = XmlUtils.XMLEncode(content);
                        }
                        nodeModule.AppendChild(newnode);
                    }
                }
                catch
                {
                    //ignore errors
                }
            }
        }

        static void AddSettings(XmlNode nodeModule, ModuleInfo module)
        {
            var moduleSettings = module.ModuleSettings;
            var tabModuleSettings = module.TabModuleSettings;

            var handleModuleSettings = true;
            var handleTabModuleSettings = true;

            if (module.DesktopModule.BusinessControllerClass != "" && module.DesktopModule.IsPortable)
            {
                try
                {
                    var businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass,
                                                                     module.DesktopModule.BusinessControllerClass);
                    if (businessController is IPortable2)
                    {
                        handleModuleSettings =
                            Convert.ToBoolean(!(((IPortable2) businessController).ManagesModuleSettings));
                        handleTabModuleSettings =
                            Convert.ToBoolean(!(((IPortable2) businessController).ManagesTabModuleSettings));
                    }
                }
                catch
                {
                }
            }

            XmlAttribute xmlattr;

            if (moduleSettings.Count > 0 && handleModuleSettings)
            {
// ReSharper disable PossibleNullReferenceException
                XmlNode settingsNode = nodeModule.OwnerDocument.CreateElement("modulesettings");

                foreach (string key in moduleSettings.Keys)
                {
                    XmlNode settingNode = nodeModule.OwnerDocument.CreateElement("setting");
                    xmlattr = nodeModule.OwnerDocument.CreateAttribute("name");
                    xmlattr.Value = key;
                    settingNode.Attributes.Append(xmlattr);
                    // ReSharper restore PossibleNullReferenceException
                    xmlattr = nodeModule.OwnerDocument.CreateAttribute("value");
                    xmlattr.Value = moduleSettings[key].ToString();
                    settingNode.Attributes.Append(xmlattr);
                    settingsNode.AppendChild(settingNode);
                }
                nodeModule.AppendChild(settingsNode);
            }

            if (tabModuleSettings.Count > 0 && handleTabModuleSettings)
            {
// ReSharper disable PossibleNullReferenceException
                XmlNode settingsNode = nodeModule.OwnerDocument.CreateElement("tabmodulesettings");
// ReSharper restore PossibleNullReferenceException
                foreach (string key in tabModuleSettings.Keys)
                {
                    XmlNode settingNode = nodeModule.OwnerDocument.CreateElement("setting");
                    xmlattr = nodeModule.OwnerDocument.CreateAttribute("name");
                    xmlattr.Value = key;
// ReSharper disable PossibleNullReferenceException
                    settingNode.Attributes.Append(xmlattr);
// ReSharper restore PossibleNullReferenceException
                    xmlattr = nodeModule.OwnerDocument.CreateAttribute("value");
                    xmlattr.Value = tabModuleSettings[key].ToString();
                    settingNode.Attributes.Append(xmlattr);
                    settingsNode.AppendChild(settingNode);
                }
                nodeModule.AppendChild(settingsNode);
            }
        }

        static bool CheckIsInstance(int templateModuleId, Hashtable hModules)
        {
            // will be instance or module
            var isInstance = false;
            if (templateModuleId > 0)
            {
                if (hModules[templateModuleId] != null)
                {
                    // this module has already been processed -> process as instance
                    isInstance = true;
                }
            }

            return isInstance;
        }

        static void CreateEventQueueMessage(ModuleInfo module, string content, string version, int userId)
        {
            var oAppStartMessage = new EventMessage
                                       {
                                           Priority = MessagePriority.High,
                                           ExpirationDate = DateTime.Now.AddYears(Convert.ToInt32(- 1)),
                                           SentDate = DateTime.Now,
                                           Body = "",
                                           ProcessorType =
                                               "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                                           ProcessorCommand = "ImportModule"
                                       };

            //Add custom Attributes for this message
            oAppStartMessage.Attributes.Add("BusinessControllerClass", module.DesktopModule.BusinessControllerClass);
            oAppStartMessage.Attributes.Add("ModuleId", module.ModuleID.ToString(CultureInfo.InvariantCulture));
            oAppStartMessage.Attributes.Add("Content", content);
            oAppStartMessage.Attributes.Add("Version", version);
            oAppStartMessage.Attributes.Add("UserID", userId.ToString(CultureInfo.InvariantCulture));

            //send it to occur on next App_Start Event
            EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
        }

        static ModuleInfo DeserializeModule(XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId,
                                            int moduleDefId)
        {
            //Create New Module
            var objModule = new ModuleInfo
                                {
                                    PortalID = portalId,
                                    TabID = tabId,
                                    ModuleOrder = - 1,
                                    ModuleTitle = XmlUtils.GetNodeValue(nodeModule, "title", ""),
                                    PaneName = XmlUtils.GetNodeValue(nodePane, "name", ""),
                                    ModuleDefID = moduleDefId,
                                    CacheTime = XmlUtils.GetNodeValueInt(nodeModule, "cachetime"),
                                    Alignment = XmlUtils.GetNodeValue(nodeModule, "alignment", ""),
                                    IconFile =
                                        Globals.ImportFile(portalId, XmlUtils.GetNodeValue(nodeModule, "iconfile", "")),
                                    AllTabs = XmlUtils.GetNodeValueBoolean(nodeModule, "alltabs")
                                };
            switch (XmlUtils.GetNodeValue(nodeModule, "visibility", ""))
            {
                case "Maximized":
                    objModule.Visibility = VisibilityState.Maximized;
                    break;
                case "Minimized":
                    objModule.Visibility = VisibilityState.Minimized;
                    break;
                case "None":
                    objModule.Visibility = VisibilityState.None;
                    break;
            }
            objModule.Color = XmlUtils.GetNodeValue(nodeModule, "color", "");
            objModule.Border = XmlUtils.GetNodeValue(nodeModule, "border", "");
            objModule.Header = XmlUtils.GetNodeValue(nodeModule, "header", "");
            objModule.Footer = XmlUtils.GetNodeValue(nodeModule, "footer", "");
            objModule.InheritViewPermissions = XmlUtils.GetNodeValueBoolean(nodeModule, "inheritviewpermissions", false);

            objModule.StartDate = XmlUtils.GetNodeValueDate(nodeModule, "startdate", Null.NullDate);
            objModule.EndDate = XmlUtils.GetNodeValueDate(nodeModule, "enddate", Null.NullDate);

            if (XmlUtils.GetNodeValue(nodeModule, "containersrc", "") != "")
            {
                objModule.ContainerSrc = XmlUtils.GetNodeValue(nodeModule, "containersrc", "");
            }
            objModule.DisplayTitle = XmlUtils.GetNodeValueBoolean(nodeModule, "displaytitle", true);
            objModule.DisplayPrint = XmlUtils.GetNodeValueBoolean(nodeModule, "displayprint", true);
            objModule.DisplaySyndicate = XmlUtils.GetNodeValueBoolean(nodeModule, "displaysyndicate", false);
            objModule.IsWebSlice = XmlUtils.GetNodeValueBoolean(nodeModule, "iswebslice", false);
            if (objModule.IsWebSlice)
            {
                objModule.WebSliceTitle = XmlUtils.GetNodeValue(nodeModule, "webslicetitle", objModule.ModuleTitle);
                objModule.WebSliceExpiryDate = XmlUtils.GetNodeValueDate(nodeModule, "websliceexpirydate",
                                                                         objModule.EndDate);
                objModule.WebSliceTTL = XmlUtils.GetNodeValueInt(nodeModule, "webslicettl",
                                                                 Convert.ToInt32(objModule.CacheTime/60));
            }

            return objModule;
        }

        static void DeserializeModulePermissions(XmlNodeList nodeModulePermissions, int portalId, ModuleInfo module)
        {
            var objRoleController = new RoleController();
            var objPermissionController = new PermissionController();

            foreach (XmlNode node in nodeModulePermissions)
            {
                var permissionKey = XmlUtils.GetNodeValue(node, "permissionkey", "");
                var permissionCode = XmlUtils.GetNodeValue(node, "permissioncode", "");
                var roleName = XmlUtils.GetNodeValue(node, "rolename", "");
                var allowAccess = XmlUtils.GetNodeValueBoolean(node, "allowaccess");

                var roleId = int.MinValue;
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser);
                        break;
                    default:
                        var objRole = objRoleController.GetRoleByName(portalId, roleName);
                        if (objRole != null)
                        {
                            roleId = objRole.RoleID;
                        }
                        break;
                }
                if (roleId != int.MinValue)
                {
                    var permissionId = Convert.ToInt32(- 1);
                    var arrPermissions = objPermissionController.GetPermissionByCodeAndKey(permissionCode, permissionKey);

                    int i;
                    for (i = 0; i <= arrPermissions.Count - 1; i++)
                    {
                        var permission = (PermissionInfo) (arrPermissions[i]);
                        permissionId = permission.PermissionID;
                    }

                    // if role was found add, otherwise ignore
                    if (permissionId != - 1)
                    {
                        var modulePermission = new ModulePermissionInfo
                                                   {
                                                       ModuleID = module.ModuleID,
                                                       PermissionID = permissionId,
                                                       RoleID = roleId,
                                                       AllowAccess = allowAccess
                                                   };
                        module.ModulePermissions.Add(modulePermission);
                    }
                }
            }
        }


        static void DeserializeModuleSettings(XmlNodeList nodeModuleSettings, int moduleId)
        {
            var objModules = new ModuleController();
            var applySettings = Convert.ToBoolean(objModules.GetModule(moduleId).ModuleSettings.Count == 0);

            foreach (XmlElement nodeSetting in nodeModuleSettings)
            {
                var name = nodeSetting.GetAttribute("name");
                var value = nodeSetting.GetAttribute("value");
                if (applySettings || nodeSetting.GetAttribute("installmode").ToLowerInvariant() == "force")
                {
                    objModules.UpdateModuleSetting(moduleId, name, value);
                }
            }
        }

        static void DeserializeTabModuleSettings(XmlNodeList nodeTabModuleSettings, int moduleId, int tabId)
        {
            var objModules = new ModuleController();
            var module = objModules.GetModule(moduleId, tabId);
            var tabModuleId = module.TabModuleID;

            var applySettings = Convert.ToBoolean(module.TabModuleSettings.Count == 0);
            foreach (XmlElement nodeSetting in nodeTabModuleSettings)
            {
                var name = nodeSetting.Attributes["name"].Value;
                var value = nodeSetting.Attributes["value"].Value;
                if (applySettings || nodeSetting.GetAttribute("installmode").ToLowerInvariant() == "force")
                {
                    objModules.UpdateTabModuleSetting(tabModuleId, name, value);
                }
            }
        }


        static bool FindModule(XmlNode nodeModule, int tabId, PortalTemplateModuleAction mergeTabs)
        {
            var modules = new ModuleController();
            var tabModules = modules.GetTabModules(tabId);

            var moduleFound = false;
            var modTitle = XmlUtils.GetNodeValue(nodeModule, "title", "");
            if (mergeTabs == PortalTemplateModuleAction.Merge)
            {
                if (tabModules.Select(kvp => kvp.Value).Any(module => modTitle == module.ModuleTitle))
                {
                    moduleFound = true;
                }
            }

            return moduleFound;
        }

        static void GetModuleContent(XmlNode nodeModule, int moduleId, int tabId, int portalId, bool isInstance)
        {
            var moduleController = new ModuleController();
            var module = moduleController.GetModule(moduleId, tabId, true);
            var contentNode = nodeModule.SelectSingleNode("content[@type]");
            var strVersion = contentNode.Attributes["version"].Value;
            var strcontent = contentNode.InnerXml;
            if (strcontent.StartsWith("<![CDATA["))
            {
                strcontent = strcontent.Substring(9, strcontent.Length - 12);
                strcontent = HttpContext.Current.Server.HtmlDecode(strcontent);
            }


            if (module.DesktopModule.BusinessControllerClass != "" && strcontent != "")
            {
                var portalController = new PortalController();
                var portal = portalController.GetPortal(portalId);

                //Determine if the Module is copmpletely installed
                //(ie are we running in the same request that installed the module).
                if (module.DesktopModule.SupportedFeatures == Null.NullInteger)
                {
                    // save content in eventqueue for processing after an app restart,
                    // as modules Supported Features are not updated yet so we
                    // cannot determine if the module supports IsPortable
                    strcontent = HttpContext.Current.Server.HtmlEncode(strcontent);
                    CreateEventQueueMessage(module, strcontent, strVersion, portal.AdministratorId);
                }
                else
                {
                    if (module.DesktopModule.IsPortable)
                    {
                        try
                        {
                            var objObject = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass,
                                                                    module.DesktopModule.BusinessControllerClass);
                            if (objObject is IPortable2)
                            {
                                ((IPortable2) objObject).ImportModule(moduleId, tabId, strcontent, strVersion,
                                                                      portal.AdministratorId, isInstance);
                            }
                            else if (objObject is IPortable && ! isInstance)
                            {
                                ((IPortable) objObject).ImportModule(moduleId, strcontent, strVersion,
                                                                     portal.AdministratorId);
                            }
                        }
                        catch
                        {
                            //ignore errors
                        }
                    }
                }
            }
        }

        static ModuleDefinitionInfo GetModuleDefinition(XmlNode nodeModule)
        {
            ModuleDefinitionInfo objModuleDefinition = null;

            // Templates prior to v4.3.5 only have the <definition> node to define the Module Type
            // This <definition> node was populated with the DesktopModuleInfo.ModuleName property
            // Thus there is no mechanism to determine to which module definition the module belongs.
            //
            // Template from v4.3.5 on also have the <moduledefinition> element that is populated
            // with the ModuleDefinitionInfo.FriendlyName.  Therefore the module Instance identifies
            // which Module Definition it belongs to.

            //Get the DesktopModule defined by the <definition> element
            var objDesktopModule =
                DesktopModuleController.GetDesktopModuleByModuleName(
                    XmlUtils.GetNodeValue(nodeModule, "definition", ""), Null.NullInteger);
            if (objDesktopModule != null)
            {
                //Get the moduleDefinition from the <moduledefinition> element
                var friendlyName = XmlUtils.GetNodeValue(nodeModule, "moduledefinition", "");

                if (string.IsNullOrEmpty(friendlyName))
                {
                    //Module is pre 4.3.5 so get the first Module Definition (at least it won't throw an error then)
                    var moduleDefinitions =
                        ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(
                            objDesktopModule.DesktopModuleID).Values;
                    foreach (
                        var moduleDefinition in
                            moduleDefinitions)
                    {
                        objModuleDefinition = moduleDefinition;
                        break;
                    }
                }
                else
                {
                    //Module is 4.3.5 or later so get the Module Definition by its friendly name
                    objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName,
                                                                                                       objDesktopModule.
                                                                                                           DesktopModuleID);
                }
            }

            return objModuleDefinition;
        }

        #endregion

        #region Public Shared Methods

        public static void DeserializeModule(XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId,
                                             PortalTemplateModuleAction mergeTabs, Hashtable hModules)
        {
            var moduleController = new ModuleController();
            var objModuleDefinition = GetModuleDefinition(nodeModule);

            // will be instance or module
            var templateModuleId = XmlUtils.GetNodeValueInt(nodeModule, "moduleID");
            var isInstance = CheckIsInstance(templateModuleId, hModules);

            //remove containersrc node if container is missing
            var containerNode = nodeModule.SelectSingleNode("containersrc");
            if (containerNode != null)
            {
                var container = containerNode.Value;
                if (! File.Exists(HttpContext.Current.Server.MapPath(container)))
                {
                    nodeModule.RemoveChild(containerNode);
                }
            }

            if (objModuleDefinition != null)
            {
                //If Mode is Merge Check if Module exists
                if (! FindModule(nodeModule, tabId, mergeTabs))
                {
                    var module = DeserializeModule(nodeModule, nodePane, portalId, tabId,
                                                   objModuleDefinition.ModuleDefID);

                    int intModuleId;
                    if (! isInstance)
                    {
                        //Add new module
                        intModuleId = moduleController.AddModule(module);
                        if (templateModuleId > 0)
                        {
                            hModules.Add(templateModuleId, intModuleId);
                        }
                    }
                    else
                    {
                        //Add instance
                        module.ModuleID = Convert.ToInt32(hModules[templateModuleId]);
                        intModuleId = moduleController.AddModule(module);
                    }

                    if (XmlUtils.GetNodeValue(nodeModule, "content", "") != "")
                    {
                        GetModuleContent(nodeModule, intModuleId, tabId, portalId, isInstance);
                    }

                    // Process permissions and moduleSettings only once
                    if (! isInstance)
                    {
                        var nodeModulePermissions = nodeModule.SelectNodes("modulepermissions/permission");
                        DeserializeModulePermissions(nodeModulePermissions, portalId, module);

                        //Persist the permissions to the Data base
                        ModulePermissionController.SaveModulePermissions(module);

                        var nodeModuleSettings = nodeModule.SelectNodes("modulesettings/setting");
                        DeserializeModuleSettings(nodeModuleSettings, intModuleId);
                    }

                    //apply TabModuleSettings
                    var nodeTabModuleSettings = nodeModule.SelectNodes("tabmodulesettings/setting");
                    DeserializeTabModuleSettings(nodeTabModuleSettings, intModuleId, tabId);
                }
            }
        }

        /// <summary>
        ///   SerializeModule
        /// </summary>
        /// <param name="xmlModule"> The Xml Document to use for the Module </param>
        /// <param name="objModule"> The ModuleInfo object to serialize </param>
        /// <param name="includeContent"> A flak that determines whether the content of the module is serialised. </param>
        public static XmlNode SerializeModule(XmlDocument xmlModule, ModuleInfo objModule, bool includeContent)
        {
            return SerializeModule(xmlModule, objModule, includeContent, Null.NullInteger);
        }

        /// <summary>
        ///   SerializeModule
        /// </summary>
        /// <param name="xmlModule"> The Xml Document to use for the Module </param>
        /// <param name="objModule"> The ModuleInfo object to serialize </param>
        /// <param name="includeContent"> A flak that determines whether the content of the module is serialised. </param>
        /// <param name="maxNumberofRecords"> Numer of reords. Choose Null.NullInteger (-1) to include all records </param>
        public static XmlNode SerializeModule(XmlDocument xmlModule, ModuleInfo objModule, bool includeContent,
                                              int maxNumberofRecords)
        {
            var xserModules = new XmlSerializer(typeof (ModuleInfo));
            using (var sw = new StringWriter())
            {
                xserModules.Serialize(sw, objModule);
                xmlModule.LoadXml((sw.GetStringBuilder().ToString()));
            }

            var nodeModule = xmlModule.SelectSingleNode("module");
// ReSharper disable PossibleNullReferenceException
            nodeModule.Attributes.Remove(nodeModule.Attributes["xmlns:xsd"]);
            nodeModule.Attributes.Remove(nodeModule.Attributes["xmlns:xsi"]);

            //remove unwanted elements
// ReSharper disable AssignNullToNotNullAttribute
            nodeModule.RemoveChild(nodeModule.SelectSingleNode("portalid"));

            nodeModule.RemoveChild(nodeModule.SelectSingleNode("tabid"));
            nodeModule.RemoveChild(nodeModule.SelectSingleNode("tabmoduleid"));
            nodeModule.RemoveChild(nodeModule.SelectSingleNode("moduleorder"));
            nodeModule.RemoveChild(nodeModule.SelectSingleNode("panename"));
            nodeModule.RemoveChild(nodeModule.SelectSingleNode("isdeleted"));

            foreach (XmlNode nodePermission in nodeModule.SelectNodes("modulepermissions/permission"))
            {
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("modulepermissionid"));
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("permissionid"));
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("moduleid"));
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("roleid"));
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("userid"));
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("username"));
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("displayname"));
            }
            // ReSharper restore AssignNullToNotNullAttribute
            // ReSharper restore PossibleNullReferenceException
            if (includeContent)
            {
                AddContent(nodeModule, objModule, maxNumberofRecords);
                AddSettings(nodeModule, objModule);
            }

            XmlNode newnode = xmlModule.CreateElement("definition");

            var objModuleDef = ModuleDefinitionController.GetModuleDefinitionByID(objModule.ModuleDefID);
            newnode.InnerText =
                DesktopModuleController.GetDesktopModule(objModuleDef.DesktopModuleID, objModule.PortalID).ModuleName;
            nodeModule.AppendChild(newnode);

            //Add Module Definition Info
            XmlNode nodeDefinition = xmlModule.CreateElement("moduledefinition");
            nodeDefinition.InnerText = objModuleDef.FriendlyName;
            nodeModule.AppendChild(nodeDefinition);

            return nodeModule;
        }

        #endregion
    }
}