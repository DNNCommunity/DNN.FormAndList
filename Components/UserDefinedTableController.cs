using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.SystemDateTime;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// <summary>
    ///   The UserDefinedTableController class provides Business Layer methods of
    ///   UDT for managing, editing and diplaying User Defined Table
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class UserDefinedTableController : BaseController
    {
        #region Constructors

        public UserDefinedTableController(int moduleId, int tabId, UserInfo userInfo)
        {
            Initialise(moduleId, tabId, userInfo);
        }

        public UserDefinedTableController(int moduleid)
        {
            ModuleId = moduleid;
        }

        public UserDefinedTableController()
        {
        }

        public UserDefinedTableController(ModuleInstanceContext moduleContext)
        {
            Initialise(moduleContext);
        }

        public UserDefinedTableController(ModuleInfo moduleinfo)
        {
            Initialise(moduleinfo);
        }

        #endregion

        #region Private Functions

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   add a new data row in the database
        /// </summary>
        /// <returns>ID of the created row (or 0, if creation failed)</returns>
        int AddRow()
        {
            return DataProvider.Instance().AddRow(ModuleId);
        }


        void SetEditLinksAndVisibilityBasedOnPermissions(DataSet ds)
        {
            if (User != null && TabId != Null.NullInteger)
            {
                //Provide a permission aware EditLink as an additional column to the dataset
                var security = new ModuleSecurity(ModuleId, TabId,   Settings);
                var createdByColumnName = ColumnNameByDataType(ds, DataTypeNames.UDT_DataType_CreatedBy);

                ds.Tables[DataSetTableName.Data].Columns.Add(DataTableColumn.EditLink, typeof (string));

                var urlPattern = EditUrlPattern ?? Globals.NavigateURL(TabId, "edit", "mid=" + ModuleId, DataTableColumn.RowId + "={0}");

                foreach (DataRow row in ds.Tables[DataSetTableName.Data].Rows)
                {
                    var rowCreatorUserName = row[createdByColumnName].ToString();
                    var isRowOwner =
                        Convert.ToBoolean((rowCreatorUserName == User.Username) &&
                                          rowCreatorUserName != Definition.NameOfAnonymousUser);
                    if (security.IsAllowedToEditRow(isRowOwner))
                    {
                        row[DataTableColumn.EditLink] = string.Format(urlPattern, row[DataTableColumn.RowId]);
                    }
                }
                //Adjust visibility to actual permissions
                foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
                {
                    row[FieldsTableColumn.Visible] = Convert.ToBoolean(row[FieldsTableColumn.Visible]) ||
                                                     (security.IsAllowedToSeeAllUserDefinedColumns() &&
                                                      (DataType.ByName(row[FieldsTableColumn.Type].ToString()).
                                                           IsUserDefinedField ||
                                                       Settings.ShowSystemColumns));
                }
            }
        }

        DataSet BuildMainDataSet(DataTable fieldsTable, IDataReader dr, bool rowMode)
        {
            fieldsTable.Columns.Add(FieldsTableColumn.ValueColumn, typeof (string));
            fieldsTable.Columns.Add(FieldsTableColumn.SortColumn, typeof (string));
            var strFields = "";
            foreach (DataRow row in fieldsTable.Rows)
            {
                strFields += string.Format("{0}{1}|", (strFields != string.Empty ? "," : string.Empty),
                                           row[FieldsTableColumn.Title]);
                if (rowMode)
                {
                    strFields += "String";
                }
                else
                {
                    //DataSet expects the FieldType to be in the namespace "System."
                    //so replace all UDT-specific field types by their .net-equivalent
                    strFields += DataType.ByName(row[FieldsTableColumn.Type].ToString()).SystemTypeName;
                }

                //needed for generic Xsl Transformations - Data Names is XmlEncoded too
                var xmlEncodedTitle = XmlConvert.EncodeName(row[FieldsTableColumn.Title].ToString());
                row[FieldsTableColumn.ValueColumn] = xmlEncodedTitle;
                // gets altered in RenderValuesToHtmlInsideDataSet depending on Datatype
                row[FieldsTableColumn.SortColumn] = xmlEncodedTitle;
            }


            var ds = Globals.BuildCrossTabDataSet("UserDefinedTable", dr, DataTableColumn.RowId + "|Int32", strFields,
                                                      DataTableColumn.RowId, FieldsTableColumn.Title, string.Empty,
                                                      DataTableColumn.Value, string.Empty, CultureInfo.InvariantCulture);
            dr.Close();

            ds.Tables[0].TableName = DataSetTableName.Data;
            fieldsTable.TableName = DataSetTableName.Fields;
            ds.Tables.Add(fieldsTable);
            return ds;
        }


        static void UpdateData(int rowId, IDictionary<int,string> values )
        {
            DataProvider.Instance().UpdateData(rowId, values);
        }

        #endregion

        #region Public Functions

      


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   fills all missing values for a field in all rows with default values.
        ///   If not default is provided or all fields are already initialized, nothing is
        ///   changed.
        /// </summary>
        /// <param name = "fieldId">ID of the Field.
        ///   If this param is omitted, all fields will be applied.</param>
        /// <param name = "defaultExpression">Expression to be filled in, either constant or
        ///   containing tokens.</param>
        /// -----------------------------------------------------------------------------
        public void FillDefaultData(int fieldId, string defaultExpression)
        {
            var tr = new Services.Tokens.TokenReplace {ModuleId = ModuleId, ModuleInfo = Configuration};
            if (tr.ContainsTokens(defaultExpression))
            {
                var ds = GetDataSet(false);
                var createdByColumnName = ColumnNameByDataType(ds, DataTypeNames.UDT_DataType_CreatedBy);

                foreach (DataRow dr in ds.Tables[DataSetTableName.Data].Rows)
                {
                    var user = (dr[createdByColumnName].ToString());
                    if (user == Definition.NameOfAnonymousUser)
                    {
                        user = string.Empty;
                    }
                    tr.User = UserController.GetUserByName(PortalId, user);
                    var newValue = tr.ReplaceEnvironmentTokens(defaultExpression, dr);
                    DataProvider.Instance().UpdateData(Convert.ToInt32(dr["UserDefinedRowID"]), fieldId, newValue);
                }
            }
            else
            {
                DataProvider.Instance().FillDefaultData(ModuleId, fieldId, defaultExpression);
            }
            DataProvider.Instance().FillDefaultData(ModuleId, fieldId, defaultExpression);
        }

        public void RenderValuesToHtmlInsideDataSet(DataSet ds, bool filterScript = false)
        {
            foreach (var fieldType in DataType.AllDataTypes)
            {
                DataType.ByName(fieldType).RenderValuesToHtmlInsideDataSet(ds, ModuleId, filterScript );
            }
        }

     

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all Data values of an UDT table (module) from the Database as DataSet
        /// </summary>
        /// <returns>All field values as DataSet (prerendered)</returns>
        /// -----------------------------------------------------------------------------
        public DataSet GetDataSet()
        {
            return GetDataSet(true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all Data values of an UDT table (module) from the Database as DataSet
        /// </summary>
        /// <returns>All field values as DataSet (prerendered)</returns>
        /// -----------------------------------------------------------------------------
        public DataSet GetDataSet(int moduleId)
        {
            Initialise(moduleId);
            return GetDataSet(true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all Data values of an UDT table (module) from the Database as DataSet
        /// </summary>
        /// <param name = "withPreRenderedValues">specifies, whether links, dates etc. shall be prerendered for XML output</param>
        /// <returns>All field values as DataSet</returns>
        /// -----------------------------------------------------------------------------
        public DataSet GetDataSet(bool withPreRenderedValues)
        {
            var fieldsTable = FieldController.GetFieldsTable(ModuleId, addNewColumn: false, addAuditColumns: false);
            DataSet ds;
            using (var dr = DataProvider.Instance().GetRows(ModuleId))
            {
                ds = BuildMainDataSet(fieldsTable, dr, !withPreRenderedValues);
            }
            var fieldSettingsTable = FieldSettingsController.GetFieldSettingsTable(ModuleId);
            ds.Tables.Add(fieldSettingsTable);

            SetEditLinksAndVisibilityBasedOnPermissions(ds);

            if (withPreRenderedValues)
            {
                RenderValuesToHtmlInsideDataSet(ds);
            }
            ds.Namespace = "DotNetNuke/UserDefinedTable";
            return ds;
        }

        public DataSet GetSchemaDataset()
        {
            var ds = GetRow(-1);
            RenderValuesToHtmlInsideDataSet(ds);
            ds.Namespace = "DotNetNuke/UserDefinedTable";
            return ds;
        }


        public DataTable Context()
        {
            return Context("", "", "", "");
        }

        public DataTable Context(string searchInput, string orderBy, string orderDirection, string paging)
        {
            return Context(Configuration, User, searchInput, orderBy, orderDirection, paging);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Provides a list of context variables for XML output
        /// </summary>
        /// <returns>DataTable with all context variables</returns>
        /// -----------------------------------------------------------------------------
        public DataTable Context(ModuleInfo moduleInfo, UserInfo userInfo, string searchInput, string orderBy,
                                 string orderDirection, string paging)
        {
            var request = HttpContext.Current.Request;
            orderBy = orderBy.AsString("UserDefinedRowId");
            orderDirection = orderDirection.AsString("ascending");

            var contextTable = new DataTable("Context");
            contextTable.Columns.Add("ModuleId", typeof (int));
            contextTable.Columns.Add("TabId", typeof (int));
            contextTable.Columns.Add("TabName", typeof(string));
            contextTable.Columns.Add("PortalId", typeof (int));
            contextTable.Columns.Add("UserName", typeof (string));
            contextTable.Columns.Add("BestUserName", typeof (string));
            //obsolete, please use DisplayName
            contextTable.Columns.Add("DisplayName", typeof (string));
            contextTable.Columns.Add("ApplicationPath", typeof (string));
            contextTable.Columns.Add("HomePath", typeof (string));
            contextTable.Columns.Add("UserRoles", typeof (string));
            contextTable.Columns.Add("IsAdministratorRole", typeof (bool));
            contextTable.Columns.Add("Parameter", typeof (string));
            contextTable.Columns.Add("OrderBy", typeof (string));
            contextTable.Columns.Add("OrderDirection", typeof (string));
            contextTable.Columns.Add("CurrentCulture", typeof (string));
            contextTable.Columns.Add("LocalizedString_Search", typeof (string));
            contextTable.Columns.Add("LocalizedString_Page", typeof (string));
            contextTable.Columns.Add("LocalizedString_Of", typeof (string));
            contextTable.Columns.Add("LocalizedString_First", typeof (string));
            contextTable.Columns.Add("LocalizedString_Previous", typeof (string));
            contextTable.Columns.Add("LocalizedString_Next", typeof (string));
            contextTable.Columns.Add("LocalizedString_Last", typeof (string));
            contextTable.Columns.Add("NowInTicks", typeof (long));
            contextTable.Columns.Add("TodayInTicks", typeof (long));
            contextTable.Columns.Add("TicksPerDay", typeof (long));
            contextTable.Columns.Add("LocalizedDate", typeof (string));
            contextTable.Columns.Add("Now", typeof (DateTime));

            if (paging != string.Empty)
            {
                contextTable.Columns.Add("Paging", typeof (int));
            }
            var row = contextTable.NewRow();
            row["ModuleId"] = moduleInfo.ModuleID;
            row["TabId"] = moduleInfo.TabID;
            row["TabName"] = moduleInfo.ParentTab.TabName;
            row["PortalId"] = moduleInfo.PortalID;

            // null username handled by extension method
            row["DisplayName"] = userInfo.GetSafeDisplayname();
            row["UserName"] = userInfo.GetSafeUsername();

            row["BestUserName"] = row["DisplayName"];
            var portalSettings = Globals.GetPortalSettings();
            row["HomePath"] = portalSettings.HomeDirectory;
            row["ApplicationPath"] = request.ApplicationPath == "/" ? "" : request.ApplicationPath;
            row["UserRoles"] = ModuleSecurity.RoleNames(userInfo);
            if (ModuleSecurity.IsAdministrator())
            {
                row["IsAdministratorRole"] = true;
            }
            row["Parameter"] = searchInput;
            row["OrderBy"] = orderBy;
            row["OrderDirection"] = orderDirection == "DESC" ? "descending" : "ascending";
            row["CurrentCulture"] = new Localization().CurrentCulture;
            if (paging != string.Empty)
            {
                row["Paging"] = int.Parse(paging);
            }
            row["LocalizedString_Search"] = Localization.GetString("Search.Text", Definition.SharedRessources);
            row["LocalizedString_Page"] = Localization.GetString("PagingPage.Text", Definition.SharedRessources);
            row["LocalizedString_of"] = Localization.GetString("PagingOf.Text", Definition.SharedRessources);
            row["LocalizedString_First"] = Localization.GetString("PagingFirst.Text", Definition.SharedRessources);
            row["LocalizedString_Previous"] = Localization.GetString("PagingPrevious.Text", Definition.SharedRessources);
            row["LocalizedString_Next"] = Localization.GetString("PagingNext.Text", Definition.SharedRessources);
            row["LocalizedString_Last"] = Localization.GetString("PagingLast.Text", Definition.SharedRessources);
            var d = DateUtils.GetDatabaseTime();
            var timeZone = userInfo.Username != null
                                  ? userInfo.Profile.PreferredTimeZone
                                  : portalSettings.TimeZone;
            d = TimeZoneInfo.ConvertTimeFromUtc( d, timeZone);
            row["Now"] = d;
            row["LocalizedDate"] = d.ToString("g", Thread.CurrentThread.CurrentCulture);
            row["NowInTicks"] = d.Ticks;
            row["TodayInTicks"] = d.Date.Ticks ;
            row["TicksPerDay"] = TimeSpan.TicksPerDay;
            contextTable.Rows.Add(row);

            return contextTable;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the name of the first column with a given column type (valuable for Track Columns)
        /// </summary>
        /// <param name = "ds">DataSet with column definitions</param>
        /// <param name = "dataType">type to be searched</param>
        /// <returns>name of the column</returns>
        /// -----------------------------------------------------------------------------
        public string ColumnNameByDataType(DataSet ds, string dataType)
        {
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == dataType)
                {
                    return row[FieldsTableColumn.Title].ToString();
                }
            }
            return string.Empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   deletes all data rows of a module in the database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void DeleteRows()
        {
            DataProvider.Instance().DeleteRows(ModuleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   deletes a data row in the database
        /// </summary>
        /// <param name = "userDefinedRowId">ID of the row to be deleted</param>
        /// -----------------------------------------------------------------------------
        public void DeleteRow(int userDefinedRowId)
        {
            TrackingController.OnAction(TrackingController.Trigger.Delete, userDefinedRowId, this);
            DataProvider.Instance().DeleteRow(userDefinedRowId, ModuleId);
        }

        public bool FieldHasData(int fieldId)
        {
            return DataProvider.Instance().GetFieldDataCount(fieldId) > 0;
        }

        public void UpdateRow(DataSet ds)
        {
            UpdateRow(ds, 0, false);
        }

        public void UpdateRow(DataSet ds, int rowNr, bool isDataToImport)
        {
            var currentRow = ds.Tables[DataSetTableName.Data].Rows[rowNr];

            var rowHasContent = false;
            foreach (DataRow field in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (DataType.ByName(field[FieldsTableColumn.Type].ToString()).IsUserDefinedField)
                {
                    var strColumnName = field[FieldsTableColumn.Title].ToString();
                    var strValueColumn = ((isDataToImport &&
                                           ds.Tables[DataSetTableName.Data].Columns.Contains(strColumnName +
                                                                                             DataTableColumn.
                                                                                                 Appendix_Original))
                                              ? strColumnName + DataTableColumn.Appendix_Original
                                              : strColumnName);
                    rowHasContent = Convert.ToBoolean(currentRow[strValueColumn].AsString() != string.Empty);
                    if (rowHasContent)
                    {
                        break;
                    }
                }
            }

            var userDefinedRowId = Convert.ToInt32(currentRow[DataTableColumn.RowId]);
            var isNew = isDataToImport || (userDefinedRowId == -1);
            if (isNew && rowHasContent)
            {
                //New entries need AddRow first
                userDefinedRowId = AddRow();
                currentRow[DataTableColumn.RowId] = userDefinedRowId;
            }

            if (rowHasContent)
            {
                var values = new Dictionary<int, string>();
                foreach (DataRow field in ds.Tables[DataSetTableName.Fields].Rows)
                {
                    var strColumnName = field[FieldsTableColumn.Title].ToString();
                    var strValueColumn = ((! isDataToImport &&
                                           ds.Tables[DataSetTableName.Data].Columns.Contains(strColumnName +
                                                                                             DataTableColumn.
                                                                                                 Appendix_Original))
                                              ? strColumnName + DataTableColumn.Appendix_Original
                                              : strColumnName);
                    if (ds.Tables[DataSetTableName.Data].Columns.Contains(strValueColumn))
                    {
                        values.Add(field[FieldsTableColumn.Id].AsInt(), currentRow[strValueColumn].AsString());
                    }
                }
                UpdateData(userDefinedRowId, values);
                if (! isDataToImport)
                {
                    TrackingController.OnAction(
                        isNew ? TrackingController.Trigger.New : TrackingController.Trigger.Update, userDefinedRowId, this);
                }
            }
            else
            {
                DeleteRow(userDefinedRowId);
            }
        }

        public DataSet GetRow(int userDefinedRowId, bool withPreRenderedValues, bool filterScript=false)
        {
            var fieldsTable = FieldController.GetFieldsTable(ModuleId, false, false);
            DataSet ds;
            using (var dr = DataProvider.Instance().GetRow(userDefinedRowId, ModuleId))
            {
                ds = BuildMainDataSet(fieldsTable, dr, !withPreRenderedValues );
                var fieldTablesettings = FieldSettingsController.GetFieldSettingsTable(ModuleId);
                ds.Tables.Add(fieldTablesettings);
                if (withPreRenderedValues)
                {
                    RenderValuesToHtmlInsideDataSet(ds, filterScript );
                }
                ds.Namespace = "DotNetNuke/UserDefinedTable";
            }

            return ds;
        }

        public DataSet GetRow(int userDefinedRowId)
        {
            return GetRow(userDefinedRowId, false);
        }

       

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   delete a whole table of a module.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ResetModule()
        {
            DataProvider.Instance().Reset(ModuleId);
        }

        #endregion

        #region "Obsolete Methods"
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   get value for maximal field size
        /// </summary>
        /// <returns>the maximal Fieldsize (number of characters), that can be stored in data field</returns>
        /// -----------------------------------------------------------------------------
        /// 
        [Obsolete("Please use FieldController")]
        public static int GetMaxFieldSize()
        {
            return FieldController.GetMaxFieldSize();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   persists deletion of a field into the database
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Please use FieldController")]
        public void DeleteField(int userDefinedFieldId)
        {
            FieldController.DeleteField(userDefinedFieldId);
        }

        /// <summary>
        ///   Persists a new column with Datatype string and Default Settings
        /// </summary>
        /// <param name = "fieldtitle"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        [Obsolete("Please use FieldController")]
        public int AddField(string fieldtitle)
        {
            return FieldController.AddField(ModuleId, fieldtitle);
        }

        [Obsolete("Please use FieldController")]
        public int AddField(string fieldTitle, int before, string helpText, bool required, string fieldType,
                            string Default, bool visible, bool showOnEdit, bool searchable, bool isPrivateColumn,
                            bool multipleValues, string inputSettings, string outputSettings, bool normalizeFlag,
                            string validationRule, string validationMessage, string editStyle)
        {
            return FieldController.AddField(ModuleId, fieldTitle, before, helpText, required, fieldType, Default, visible, showOnEdit, searchable, isPrivateColumn, multipleValues, inputSettings, outputSettings, normalizeFlag, validationRule, validationMessage, editStyle);
        }
        /// -----------------------------------------------------------------------------
        [Obsolete("Please use FieldController")]
        public void UpdateField(int userDefinedFieldId, string fieldTitle, string helpText, bool required,
                                string fieldType, string Default, bool visible, bool showOnEdit, bool searchable,
                                bool isPrivateColumn, bool multipleValues, string inputSettings, string outputSettings,
                                bool normalizeFlag, string validationRule, string validationMessage, string editStyle)
        {
            FieldController.UpdateField(userDefinedFieldId, fieldTitle, helpText, required, fieldType, Default,
                                                  visible, showOnEdit, searchable, isPrivateColumn, multipleValues,
                                                  inputSettings, outputSettings, normalizeFlag, validationRule,
                                                  validationMessage, editStyle);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all field definitions for one UDT table (module) from the database
        /// </summary>
        /// <returns>All field settings as DataTable</returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Please use FieldController")]
        public DataTable GetFieldsTable()
        {
            return FieldController.GetFieldsTable(ModuleId, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all field definitions for one UDT table (module) from the database
        /// </summary>
        /// <param name = "addNewColumn">specifies, whether a new column shall be added</param>
        /// <returns>All field settings as DataTable</returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Please use FieldController")]
        public DataTable GetFieldsTable(bool addNewColumn)
        {
            return FieldController.GetFieldsTable(ModuleId, addNewColumn, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all field definitions for one UDT table (module) from the database
        /// </summary>
        /// <param name = "addNewColumn">specifies, whether a new column shall be added</param>
        /// <param name = "addAuditColumns">specifies, whether colums for creation and update (user and timestamp) shall be added</param>
        /// <returns>All field settings as DataTable</returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Please use FieldController")]
        public DataTable GetFieldsTable(bool addNewColumn, bool addAuditColumns)
        {
            return FieldController.GetFieldsTable(ModuleId, addNewColumn, addAuditColumns);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   swap the ordinal position of two columns in a table definition.
        /// </summary>
        /// <param name = "firstUserDefinedFieldId">ID of the first column</param>
        /// <param name = "secondUserDefinedFieldId">ID of the second column</param>
        /// -----------------------------------------------------------------------------
        [Obsolete("Please use FieldController.SetFieldOrder")]
        public void SwapFieldOrder(int firstUserDefinedFieldId, int secondUserDefinedFieldId)
        {
            if (firstUserDefinedFieldId != secondUserDefinedFieldId)
            {
                DataProvider.Instance().SwapFieldOrder(firstUserDefinedFieldId, secondUserDefinedFieldId);
            }
        }
#endregion
    }
}