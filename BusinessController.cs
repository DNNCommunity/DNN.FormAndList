using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Search.Entities;
using Microsoft.VisualBasic;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable
{
    public class BusinessController : ModuleSearchBase, IPortable, IPortable2
    {
        public enum SettingsType
        {
            ModuleSettings,
            TabModuleSettings
        }

        static DataTable GetSettingsTable(Hashtable settings, SettingsType type)
        {


            DataTable returnValue = null;

            switch (type)
            {
                case SettingsType.ModuleSettings:

                    returnValue = new DataTable(DataSetTableName.Settings);
                    break;
                case SettingsType.TabModuleSettings:

                    returnValue = new DataTable(DataSetTableName.TabSettings);
                    break;
            }

            var sortedSettings = new SortedList<string, string>();
            if (settings != null)
                foreach (DictionaryEntry item in settings)
                {
                    sortedSettings.Add(item.Key.ToString(), item.Value.ToString());
                }


            using (var nameCol = new DataColumn(SettingsTableColumn.Setting, typeof(string)) { ColumnMapping = MappingType.Attribute })
            {
                if (returnValue != null)
                {
                    returnValue.Columns.Add(nameCol);
                    using (var valCol = new DataColumn(SettingsTableColumn.Value, typeof(string)) { ColumnMapping = MappingType.Attribute })
                    {
                        returnValue.Columns.Add(valCol);
                        foreach (var key in sortedSettings.Keys)
                        {
                            var row = returnValue.NewRow();
                            row[SettingsTableColumn.Setting] = key;
                            row[SettingsTableColumn.Value] = sortedSettings[key];
                            returnValue.Rows.Add(row);
                        }
                    }
                }
            }
            return returnValue;
        }

        DataTable GetStylesheetTable(Components.Settings settings, int portalId)
        {
            var returnValue = new DataTable(DataSetTableName.Stylesheets);

            returnValue.Columns.Add(new DataColumn(StylesheetTableColumn.NameOfSetting, typeof(string)));
            returnValue.Columns.Add(new DataColumn(StylesheetTableColumn.LocalFilePath, typeof(string)));
            returnValue.Columns.Add(new DataColumn(StylesheetTableColumn.Stylesheet, typeof(string)));

            var renderMethod = string.Format("UDT_{0}", settings.RenderingMethod);
            var listScript = renderMethod == SettingName.XslUserDefinedStyleSheet
                                 ? settings.ScriptByRenderingMethod(renderMethod)
                                 : string.Empty;
            if (listScript.Length > 0)
            {
                var row = returnValue.NewRow();
                row[StylesheetTableColumn.NameOfSetting] = SettingName.XslUserDefinedStyleSheet;
                row[StylesheetTableColumn.LocalFilePath] = listScript;
                row[StylesheetTableColumn.Stylesheet] = Utilities.ReadStringFromFile(listScript, portalId);
                returnValue.Rows.Add(row);
            }

            var trackingSkript = settings.TrackingScript;

            if (trackingSkript.Length > 0 && trackingSkript != "[AUTO]")
            {
                var row = returnValue.NewRow();
                row[StylesheetTableColumn.NameOfSetting] = SettingName.TrackingScript;
                row[StylesheetTableColumn.LocalFilePath] = trackingSkript;
                row[StylesheetTableColumn.Stylesheet] = Utilities.ReadStringFromFile(trackingSkript, portalId);
                returnValue.Rows.Add(row);
            }

            return returnValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Implements the search interface for DotNetNuke
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo modInfo, DateTime beginDateUtc)
        {
            var searchDocuments = new List<SearchDocument>();
            var udtController = new UserDefinedTableController(modInfo);

            try
            {
                var dsUserDefinedRows = udtController.GetDataSet(withPreRenderedValues: false);

                //Get names of ChangedBy and ChangedAt columns
                var colnameChangedBy = udtController.ColumnNameByDataType(dsUserDefinedRows,
                                                                          DataTypeNames.UDT_DataType_ChangedBy);
                var colnameChangedAt = udtController.ColumnNameByDataType(dsUserDefinedRows,
                                                                          DataTypeNames.UDT_DataType_ChangedAt);

                var settings = modInfo.ModuleSettings;
                var includeInSearch = !(settings[SettingName.ExcludeFromSearch].AsBoolean());

                if (includeInSearch)
                {
                    foreach (DataRow row in dsUserDefinedRows.Tables[DataSetTableName.Data].Rows)
                    {
                        var changedDate = DateTime.Today;
                        var changedByUserId = 0;

                        if (colnameChangedAt != string.Empty && !Information.IsDBNull(row[colnameChangedAt]))
                        {
                            changedDate = Convert.ToDateTime(row[colnameChangedAt]);
                        }
                        if (colnameChangedBy != string.Empty && !Information.IsDBNull(row[colnameChangedBy]))
                        {
                            changedByUserId = ModuleSecurity.UserId(row[colnameChangedBy].ToString(), modInfo.PortalID);
                        }

                        var desc = string.Empty;
                        foreach (DataRow col in dsUserDefinedRows.Tables[DataSetTableName.Fields].Rows)
                        {
                            var fieldType = col[FieldsTableColumn.Type].ToString();
                            var fieldTitle = col[FieldsTableColumn.Title].ToString();
                            var visible = Convert.ToBoolean(col[FieldsTableColumn.Visible]);
                            if (visible &&
                                (fieldType.StartsWith("Text") || fieldType == DataTypeNames.UDT_DataType_String))
                            {
                                desc += string.Format("{0} &bull; ", Convert.ToString(row[fieldTitle]));
                            }
                        }
                        if (desc.EndsWith("<br/>"))
                        {
                            desc = desc.Substring(0, Convert.ToInt32(desc.Length - 5));
                        }

                        var searchDoc = new SearchDocument
                        {
                            UniqueKey = row[DataTableColumn.RowId].ToString(),
                            PortalId = modInfo.PortalID,
                            Title = modInfo.ModuleTitle,
                            Description = desc,
                            Body = desc,
                            ModifiedTimeUtc = changedDate
                        };

                        searchDocuments.Add(searchDoc);

                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return searchDocuments;
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Implements the export interface for DotNetNuke
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int moduleId)
        {
            return ExportModule(moduleId, Null.NullInteger);
        }


        /// <summary>
        ///   Implements the enhanced export interface for DotNetNuke
        /// </summary>
        public string ExportModule(int moduleId, int tabId)
        {
            return ExportModule(moduleId, tabId, Null.NullInteger);
        }


        public string ExportModule(int moduleId, int tabId, int maxNumberOfItems)
        {
            var ds = ExportModuleDataSet(moduleId, tabId);
            if (maxNumberOfItems > Null.NullInteger)
            {
                //clear all but first row
                for (var i = ds.Tables[DataSetTableName.Data].Rows.Count - 1; i >= maxNumberOfItems; i--)
                {
                    ds.Tables[DataSetTableName.Data].Rows.RemoveAt(i);
                }
            }
            //dataset to xml
            return ds.GetXml();
        }

        public DataSet ExportModuleDataSet(int moduleId, int tabId)
        {
            DataSet ds;

            if (tabId == Null.NullInteger)
            {
                var udtController = new UserDefinedTableController(moduleId);
                ds = udtController.GetDataSet(false);
                var moduleInfo = new ModuleController().GetModule(moduleId);
                ds.Tables.Add(GetSettingsTable(moduleInfo.ModuleSettings, SettingsType.ModuleSettings));
            }
            else
            {
                var moduleInfo = new ModuleController().GetModule(moduleId, tabId);
                var udtController = new UserDefinedTableController(moduleInfo);
                ds = udtController.GetDataSet(false);
                ds.Tables.Add(GetSettingsTable(moduleInfo.ModuleSettings, SettingsType.ModuleSettings));
                ds.Tables.Add(GetSettingsTable(moduleInfo.TabModuleSettings, SettingsType.TabModuleSettings));
                ds.Tables.Add(GetStylesheetTable(udtController.Settings, moduleInfo.PortalID));
            }
            return (ds);
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Implements the import interface for DotNetNuke
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ImportModule(int moduleId, string content, string version, int userId)
        {
            ImportModule(moduleId, Null.NullInteger, content, version, userId, false);
        }

        /// <summary>
        ///   Implements the enhanced Import Interface for DotNetNuke
        /// </summary>
        public void ImportModule(int moduleId, int tabId, string content, string version, int userId, bool isInstance)
        {
            // save script timeout
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            try
            {
                // temporarily set script timeout to large value ( this value is only applicable when application is not running in Debug mode )
                HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

                var udtController = new UserDefinedTableController(moduleId);
                using (var ds = new DataSet())
                {
                    var xmlNode = Globals.GetContent(content, string.Empty);
                    ds.ReadXml(new XmlNodeReader(xmlNode));
                    var modules = new ModuleController();
                    var tabModuleId = Null.NullInteger;
                    if (tabId != Null.NullInteger)
                    {
                        var moduleInfo = modules.GetModule(moduleId, tabId);
                        tabModuleId = moduleInfo.TabModuleID;
                    }
                    if (tabModuleId != Null.NullInteger && ds.Tables[DataSetTableName.TabSettings] != null)
                    {
                        AddTabModuleSettings(modules, tabModuleId, ds);
                    }
                    if (!isInstance)
                    {
                        AddModuleSettings(moduleId, modules, ds);
                        //Fields - first delete old Fields
                        udtController.ResetModule();
                        AddFields(moduleId, ds);
                        AddData(udtController, ds);
                    }
                    if (ds.Tables.Contains(DataSetTableName.Stylesheets))
                    {
                        ImportStyleSheet(moduleId, isInstance, tabModuleId, modules, ds);
                    }
                }
            }
            finally
            {
                // reset script timeout
                HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
            }
        }

        static void AddTabModuleSettings(ModuleController modules, int tabModuleId, DataSet ds)
        {
            foreach (DataRow row in ds.Tables[DataSetTableName.TabSettings].Rows)
            {
                modules.UpdateTabModuleSetting(tabModuleId, row[SettingsTableColumn.Setting].ToString(),
                                               row[SettingsTableColumn.Value].ToString());
            }
        }

        static void AddModuleSettings(int moduleId, ModuleController modules, DataSet ds)
        {
            if (ds.Tables[DataSetTableName.Settings] != null)
            {
                foreach (DataRow row in ds.Tables[DataSetTableName.Settings].Rows)
                {
                    modules.UpdateModuleSetting(moduleId, row[SettingsTableColumn.Setting].ToString(),
                                                row[SettingsTableColumn.Value].ToString());
                }
            }
        }

        static void ImportStyleSheet(int moduleId, bool isInstance, int tabModuleId, ModuleController modules,
                                     DataSet ds)
        {
            var portalSettings = Globals.GetPortalSettings();
            foreach (DataRow row in ds.Tables[DataSetTableName.Stylesheets].Rows)
            {
                var settingName = row[StylesheetTableColumn.NameOfSetting].ToString();
                var localFilePath = row[StylesheetTableColumn.LocalFilePath].ToString();
                var stylesheet = row[StylesheetTableColumn.Stylesheet].ToString();
                //Check whether file exists
                if (File.Exists(((portalSettings.HomeDirectoryMapPath + localFilePath).Replace("/", "\\"))))
                {
                    //nothing to do, settings points to existing stylesheet
                }
                else
                {
                    var fileName =
                        localFilePath.Substring(
                            Convert.ToInt32(
                                localFilePath.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1));
                    var folder = Utilities.GetFolder(portalSettings, Definition.XSLFolderName);
                    Utilities.SaveScript(stylesheet, fileName, folder, false);
                    if (tabModuleId != Null.NullInteger)
                    {
                        modules.UpdateTabModuleSetting(tabModuleId, settingName,
                                                       string.Format("{0}/{1}", Definition.XSLFolderName, fileName));
                    }
                    else
                    {
                        if (!isInstance)
                        {
                            modules.UpdateModuleSetting(moduleId, settingName,
                                                        string.Format("{0}/{1}", Definition.XSLFolderName, fileName));
                        }
                    }
                }
            }
        }

        static void AddData(UserDefinedTableController udtController, DataSet ds)
        {
            if (ds.Tables[DataSetTableName.Data] != null)
            {
                for (var rowNr = 0; rowNr <= ds.Tables[DataSetTableName.Data].Rows.Count - 1; rowNr++)
                {
                    udtController.UpdateRow(ds, rowNr, isDataToImport: true);
                }
            }
        }

        static void AddFields(int moduleId, DataSet ds)
        {
            var fieldIndex = ds.Tables[DataSetTableName.Fields].Rows.Count;
            var fieldSettings = ds.Tables[DataSetTableName.FieldSettings];
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                var oldFieldId = row[FieldsTableColumn.Id].AsInt();
                var newFieldId =
                 FieldController.AddField(moduleId, row[FieldsTableColumn.Title].ToString(),
                                                                     row.AsString(FieldsTableColumn.Order).AsInt(fieldIndex),
                                                                     row.AsString((FieldsTableColumn.HelpText)),
                                                                     row.AsString(FieldsTableColumn.Required).AsBoolean(),
                                                                     row.AsString((FieldsTableColumn.Type)),
                                                                     row.AsString((FieldsTableColumn.Default)),
                                                                     row.AsString(FieldsTableColumn.Visible).AsBoolean(),
                                                                     row.AsString(FieldsTableColumn.ShowOnEdit).AsBoolean(true),
                                                                     row.AsString(FieldsTableColumn.Searchable).AsBoolean(),
                                                                     row.AsString(FieldsTableColumn.IsPrivate).AsBoolean(),
                                                                     row.AsString(FieldsTableColumn.MultipleValues).AsBoolean(),
                                                                     row.AsString((FieldsTableColumn.InputSettings)),
                                                                     row.AsString((FieldsTableColumn.OutputSettings)),
                                                                     row.AsString(FieldsTableColumn.NormalizeFlag).AsBoolean(),
                                                                     row.AsString((FieldsTableColumn.ValidationRule)),
                                                                     row.AsString((FieldsTableColumn.ValidationMessage)),
                                                                     row.AsString((FieldsTableColumn.EditStyle)));

                if (fieldSettings != null)
                {
                    foreach (DataRowView setting in fieldSettings.WithFieldId(oldFieldId))
                    {
                        FieldSettingsController.UpdateFieldSetting(
                            (string)setting["SettingName"],
                            (string)setting["SettingValue"],
                            newFieldId);
                    }
                }
                row[FieldsTableColumn.Id] = newFieldId;
                fieldIndex--;
            }
        }

        public bool ManagesModuleSettings
        {
            get { return true; }
        }

        public bool ManagesTabModuleSettings
        {
            get { return true; }
        }
    }
}