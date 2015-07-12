using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using Microsoft.VisualBasic.FileIO;
namespace DotNetNuke.Modules.UserDefinedTable
{
    public partial class ImportCsv : PortalModuleBase
    {
        #region Private Members

         int _moduleId = Convert.ToInt32(- 1);

        #endregion

        #region Event Handlers

         protected override void OnInit(EventArgs e)
         {
             base.OnInit(e);
             Load += Page_Load;
             cboFolders.SelectedIndexChanged  += cboFolders_SelectedIndexChanged;
             cmdCancel.Click += cmdCancel_Click;
             cmdImport.Click += cmdImport_Click;
         }

        void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["moduleid"] != null)
                {
                    _moduleId = int.Parse(Request.QueryString["moduleid"]);
                }

                if (! Page.IsPostBack)
                {
                    cboFolders.Items.Insert(0,
                                            new ListItem(
                                                string.Format("<{0}>", Localization.GetString("None_Specified")), "-"));
                    var folders = FolderManager.Instance.GetFolders(UserInfo, "READ, WRITE");
                  
                    foreach (var folder in folders)
                    {
                        var folderItem = new ListItem
                                             {
                                                 Text =
                                                     folder.FolderPath == Null.NullString
                                                         ? Localization.GetString("Root", LocalResourceFile)
                                                         : folder.FolderPath,
                                                 Value = folder.FolderPath
                                             };
                        cboFolders.Items.Add(folderItem);
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void cboFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboFiles.Items.Clear();
            if (cboFolders.SelectedIndex != 0)
            {
                var objModules = new ModuleController();
                var objModule = objModules.GetModule(_moduleId, TabId, false);

                if (objModule != null)
                {
                    var arrFiles = Globals.GetFileList(PortalId, "csv", false, cboFolders.SelectedItem.Value);


                    foreach (FileItem objFile in arrFiles)
                    {
                        cboFiles.Items.Add(new ListItem(objFile.Text, objFile.Text));
                    }
                }
            }
        }

        void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void cmdImport_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboFiles.SelectedItem != null)
                {
                    var objModules = new ModuleController();
                    var objModule = objModules.GetModule(_moduleId, TabId, false);
                    if (objModule != null)
                    {
                        var strMessage = ImportModule(_moduleId, cboFiles.SelectedItem.Value,
                                                      cboFolders.SelectedItem.Value, rblDelimiter.SelectedValue);
                        if (strMessage == "")
                        {
                            Response.Redirect(Globals.NavigateURL(), true);
                        }
                        else
                        {
                            UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                        }
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, "Please specify the file to import",
                                                   ModuleMessage.ModuleMessageType.RedError);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Private Methods

        string ImportModule(int moduleId, string fileName, string folder, string delimiter)
        {
            var strMessage = "";

            var mc = new ModuleController();
            var moduleInfo = mc.GetModule(moduleId, TabId, false);

            if (Path.GetExtension(fileName).ToUpper() == ".CSV" && moduleInfo != null)
            {
                var udtController = new UserDefinedTableController(ModuleContext );
              
                var file = FileManager.Instance.GetFile(PortalId, Path.Combine(folder, fileName));
                using (var stream = FileManager.Instance.GetFileContent(file))
                {
                    // Open the stream and read it back.
                    using (var reader = new TextFieldParser(stream))
                    {
                        reader.TextFieldType = FieldType.Delimited;
                        reader.SetDelimiters(delimiter);

                        DataSet ds = null;
                        DataTable dataTable = null;
                        string colChangedBy = null;
                        string colChangedAt = null;
                        string colCreatedBy = null;
                        string colCreatedAt = null;
                        var obligateDataTypesCount = DataType.SystemDataTypes().Count;

                        try
                        {
                            while (!reader.EndOfData)
                            {
                                var linecsv = reader.ReadFields();
                                if (dataTable == null)
                                {
                                    ds = CreateFields(linecsv, udtController);
                                    dataTable = ds.Tables[DataSetTableName.Data];
                                    colChangedBy = udtController.ColumnNameByDataType(ds, DataTypeNames.UDT_DataType_ChangedBy);
                                    colChangedAt = udtController.ColumnNameByDataType(ds, DataTypeNames.UDT_DataType_ChangedAt);
                                    colCreatedBy = udtController.ColumnNameByDataType(ds, DataTypeNames.UDT_DataType_CreatedBy);
                                    colCreatedAt = udtController.ColumnNameByDataType(ds, DataTypeNames.UDT_DataType_CreatedAt);
                                }
                                else
                                {
                                    var insertTime = DateTime.Now.ToString("s");
                                    var newRow = dataTable.NewRow();
                                    newRow[DataTableColumn.RowId] = -1;
                                    newRow[colChangedBy] = UserInfo.Username;
                                    newRow[colCreatedBy] = UserInfo.Username;
                                    newRow[colChangedAt] = insertTime;
                                    newRow[colCreatedAt] = insertTime;
                                    dataTable.Rows.Add(FillRow(linecsv, newRow, obligateDataTypesCount));
                                }
                            }

                            if (ds != null)
                            {
                                for (var rowNr = 0; rowNr <= dataTable.Rows.Count - 1; rowNr++)
                                {
                                    udtController.UpdateRow(ds, rowNr, isDataToImport: true);
                                }
                                mc.UpdateModuleSetting(moduleId, SettingName.ListOrForm, "List");
                            }
                        }
                        catch (Exception ex)
                        {
                            strMessage = string.Format(Localization.GetString("importError", LocalResourceFile), ex.Message);
                        }
                    }
                }

                
            }

            return strMessage;
        }

         DataSet CreateFields(IEnumerable<string> columns, UserDefinedTableController udtController)
        {
            udtController.ResetModule();
            FieldController.GetFieldsTable(ModuleId );
            foreach (var column in columns)
            {
                FieldController.AddField(ModuleId, column);
            }
            return udtController.GetRow(- 1, false);
        }

        DataRow FillRow(string[] data, DataRow row, int start)
        {
            for (var counter = 0; counter <= data.Length - 1; counter++)
            {
                row[counter + start + 1] = data[counter];
            }
            return row;
        }

        #endregion
    }
}