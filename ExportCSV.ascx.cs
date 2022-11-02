using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.UserDefinedTable.CSV;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using System.Globalization;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.UserDefinedTable
{
    public partial class ExportCsv : PortalModuleBase
    {
        #region Private Members

        int _moduleId = Convert.ToInt32(-1);

        #endregion

        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cmdCancel.Click += cmdCancel_Click;
            cmdExport.Click += cmdExport_Click;
            Load += Page_Load;
        }
        void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["moduleid"] != null)
                {
                    _moduleId = int.Parse(Request.QueryString["moduleid"]);
                }

                if (!Page.IsPostBack)
                {
                    cboFolders.Items.Insert(0,
                                            new ListItem(
                                                string.Format("<{0}>", Localization.GetString("None_Specified")), "-"));
                    var folders = FolderManager.Instance.GetFolders(UserInfo, "READ, WRITE");
                    foreach (FolderInfo folder in folders)
                    {
                        var folderItem = new ListItem
                        {
                            Text = folder.FolderPath == Null.NullString
                                                            ? Localization.GetString("Root", LocalResourceFile)
                                                            : folder.FolderPath,
                            Value = folder.FolderPath
                        };
                        cboFolders.Items.Add(folderItem);
                    }

                    var moduleController = new ModuleController();
                    var objModule = moduleController.GetModule(_moduleId, TabId, false);
                    if (objModule != null)
                    {
                        txtFile.Text = CleanName(objModule.ModuleTitle);
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
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

        void cmdExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboFolders.SelectedIndex != 0 && txtFile.Text != "")
                {
                    var strFile = CleanName(string.Format("{0}.csv", txtFile.Text));
                    var strMessage = ExportModule(_moduleId, strFile, cboFolders.SelectedItem.Value,
                                                  rblDelimiter.SelectedValue, txtInitialDate.Text, txtFinalDate.Text);
                    if (strMessage == "")
                    {
                        Response.Redirect(Globals.NavigateURL(), true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Validation", LocalResourceFile),
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

        string ExportModule(int moduleId, string fileName, string folder, string delimiter, string initialDate, string finalDate)
        {
            var strMessage = "";

            var moduleController = new ModuleController();
            var moduleInfo = moduleController.GetModule(moduleId, TabId, false);
            var extension = Path.GetExtension(fileName);
            if (extension != null && (extension.ToUpper() == ".CSV" && moduleInfo != null))
            {


                try
                {
                    ExportData(folder, fileName, delimiter, initialDate, finalDate);
                }
                catch (Exception ex)
                {
                    strMessage = string.Format("Error occurred: {0}", ex.Message);
                }
            }

            return strMessage;
        }

        static string CleanName(string name)
        {
            var strName = name;

            var badChars = Path.GetInvalidPathChars();

            return badChars.Aggregate(strName, (current, badChar) => current.Replace(badChar.ToString(), ""));
        }

        void ExportData(string folder, string strFileName, string delimiter, string initialDate, string finalDate)
        {
            DataSet ds;

            if(initialDate.Length>0 && finalDate.Length > 0)
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                string format = "yyyy/MM/dd";

                var serverTimeZone = PortalController.Instance.GetCurrentPortalSettings().TimeZone;
                var timeZone = serverTimeZone;

                DateTime startDate = DateTime.ParseExact(initialDate, format, provider);
                startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
                startDate = TimeZoneInfo.ConvertTimeToUtc(startDate, timeZone);

                DateTime endDate = DateTime.ParseExact(finalDate, format, provider);
                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);
                endDate = TimeZoneInfo.ConvertTimeToUtc(endDate, timeZone);

                ds = new UserDefinedTableController(_moduleId, TabId, UserInfo).GetDataSetWithDates(true,startDate, endDate);
            }
            else
            {
                ds = new UserDefinedTableController(_moduleId, TabId, UserInfo).GetDataSet(true);
            }
            
            var data = ds.Tables[0];
            var fields = ds.Tables[1];

            WriteData(data, fields, folder, strFileName, delimiter);
        }

        void WriteData(DataTable data, DataTable fields, string folder, string fileName, string delimiter)
        {

            using (var sw = new StringWriter())
            {
                var columns = new List<string>();
                //Writing top line with column names
                foreach (DataRow row in fields.Rows)
                {
                    var typeName = (row[FieldsTableColumn.Type].ToString());
                    //ignore system fields
                    if (DataType.ByName(typeName).IsUserDefinedField || cbSystemFields.Checked)
                    {
                        columns.Add(row[FieldsTableColumn.Title].ToString());
                    }
                }

                //write colums name as first line
                CSVWriter.WriteCSV(columns.ToArray(), sw, delimiter);

                //writing data
                foreach (DataRow row in data.Rows)
                {
                    var values = new List<string>();
                    //getting values for all colums
                    foreach (var fieldTitle in columns)
                    {
                        var valueName = ((data.Columns.Contains(fieldTitle + DataTableColumn.Appendix_Original))
                                             ? fieldTitle + DataTableColumn.Appendix_Original
                                             : fieldTitle);
                        var value = row[valueName].AsString();
                        if (fieldTitle == "Created at" || fieldTitle == "Changed at")
                        {
                            DateTime valueDate = DateTime.Parse(value);
                            value = valueDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                        values.Add(value);
                    }
                    CSVWriter.WriteCSV(values.ToArray(), sw, delimiter);

                }
                WriteFile(folder, fileName, sw);
            }
        }

        void WriteFile(string folder, string fileName, StringWriter sw)
        {
            using (var memStream = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(sw.ToString());
                memStream.Write(bytes, 0, bytes.Length);
                var f = FolderManager.Instance.GetFolder(PortalId, folder);
                FileManager.Instance.AddFile(f, fileName, memStream, true);
            }
        }

        #endregion
    }
}
