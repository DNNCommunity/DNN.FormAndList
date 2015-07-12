using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Serialization;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable.Templates
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class TemplateController
    {
        static string CacheKey
        {
            get { return string.Format("UDT_Tempaltes{0}", Globals.GetPortalSettings().PortalId); }
        }

        static readonly object TemplateLock = new object();

        public static IDictionary<string, TemplateInfo> Templates
        {
            get
            {
                var templates = (IDictionary<string, TemplateInfo>) (DataCache.GetCache(CacheKey));

                if (templates == null)
                {
                    lock (TemplateLock)
                    {
                        templates = new Dictionary<string, TemplateInfo>();
                        var folderNames = new[]
                                              {
                                                  Globals.GetPortalSettings().HomeDirectoryMapPath +
                                                  Definition.TemplateFolderName,
                                                  Globals.HostMapPath + Definition.TemplateFolderName
                                              };
                        foreach (var folderName in folderNames)
                        {
                            if (Directory.Exists(folderName))
                            {
                                var folder = new DirectoryInfo(folderName);
                                foreach (var file in folder.GetFiles("*.dnn_userdefinedtable.module.template"))
                                {
                                    try
                                    {
                                        var doc = new XmlDocument();
                                        doc.Load(file.FullName);
                                        var templateNode = doc.SelectSingleNode("/moduletemplate");
                                        // ReSharper disable PossibleNullReferenceException
                                        var template = new TemplateInfo
                                                           {
                                                               Name = templateNode.Attributes["title"].Value,
                                                               Description = templateNode.Attributes["description"].Value,
                                                               ExportContent = templateNode.InnerXml
                                                           };
                                        // ReSharper restore PossibleNullReferenceException
                                        templates[template.Name] = template;
                                    }
                                    catch (Exception exp)
                                    {
                                        var e =
                                            new Exception(
                                                string.Format("UDT Template: {0} caused an exception", file.FullName),
                                                exp);
                                        Exceptions.LogException(e);
                                    }
                                }
                            }
                            DataCache.SetCache(CacheKey, templates);
                        }
                    }
                }
                return templates;
            }
        }

        /// <summary>
        ///   Returns a list of all Templates, needed for ObjectDatasource
        /// </summary>
        public ICollection<TemplateInfo> TemplateList()
        {
            return Templates.Values;
        }

        public static void LoadTemplate(string name, int portalId, int tabid)
        {
            var doc = new XmlDocument();
            doc.LoadXml((Templates[name].ExportContent));
            ModuleSerializationController.DeserializeModule(doc.DocumentElement, doc.DocumentElement, portalId, tabid,
                                                            PortalTemplateModuleAction.Ignore, new Hashtable());
        }

        public static void LoadTemplate(XmlDocument content, int portalId, int tabid)
        {
            ModuleSerializationController.DeserializeModule(content.DocumentElement, content.DocumentElement, portalId,
                                                            tabid, PortalTemplateModuleAction.Ignore, new Hashtable());
        }

        public static void ClearCache()
        {
            DataCache.RemoveCache(CacheKey);
        }

        public static bool SaveTemplate(string name, string description, ModuleInstanceContext context,
                                        bool forceOverwrite, int maxNumberOfRecords)
        {
            var doc = new XmlDocument();
            var moduleInfo = new ModuleController().GetModule(context.Configuration.ModuleID, context.Configuration.TabID);
            var node = ModuleSerializationController.SerializeModule(doc, moduleInfo, true, maxNumberOfRecords);
            // add PaneName as element "name"
            var paneNode = doc.CreateElement("name");
            paneNode.InnerXml = context.Configuration.PaneName;
            node.AppendChild(paneNode);
            var template = new TemplateInfo
                               {
                                   Name = name,
                                   Description = description,
                                   ExportContent = XslTemplatingUtilities.PrettyPrint(node.OuterXml)
                               };

            var fileName = string.Format("{0}.{1}.module.template", Globals.CleanFileName(name),
                                         moduleInfo.DesktopModule.ModuleName.ToLowerInvariant());

            var portalSettings = context.PortalSettings;
            var folder = Utilities.GetFolder(portalSettings, Definition.TemplateFolderName);

            if (Utilities.SaveScript( template.GetXml(), fileName, folder, forceOverwrite))
            {
                ClearCache();
                return true;
            }
            return false;
        }
    }
}