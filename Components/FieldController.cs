using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class FieldController
    {
        public static void ChangeFieldOrder(IEnumerable<int> fieldIds)
        {
            var i = 0;
            foreach (var fieldId in fieldIds)
            {
                DataProvider.Instance().SetFieldOrder(fieldId, i++);
            }
        }

        public static int GetMaxFieldSize()
        {
            return DataProvider.Instance().GetMaxFieldSize();
        }

        public static void DeleteField(int userDefinedFieldId)
        {
            DataProvider.Instance().DeleteField(userDefinedFieldId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   persists a new column setting in the database
        /// </summary>
        /// <param name = "moduleId">ID of the column</param>
        /// <param name = "fieldTitle">ID of the column</param>
        /// <param name = "before"></param>
        /// <param name = "helpText"></param>
        /// <param name = "required">is an entry in this column required, when adding a row?</param>
        /// <param name = "fieldType">type name of data field</param>
        /// <param name = "Default">default value, entered when a new row is added</param>
        /// <param name = "visible">is this column visible in list view?</param>
        /// <param name = "showOnEdit"></param>
        /// <param name = "isPrivateColumn"></param>
        /// <param name = "searchable">is this column available for search in list view?</param>
        /// <param name="multipleValues"></param>
        /// <param name = "inputSettings">additional settings stored in single string</param>
        /// <param name = "outputSettings">additional settings stored in single string</param>
        /// <param name = "normalizeFlag">display flag, usage dependant on data type</param>
        /// <param name = "validationRule">optional expresion that needs to be true, to successfully enter a record</param>
        /// <param name = "validationMessage">displayed, if validation rules is eveluated to "false"</param>
        /// <param name="editStyle"></param>
        /// <returns>The ID of the created Field</returns>
        /// -----------------------------------------------------------------------------
        public static int AddField(int moduleId, string fieldTitle, int before, string helpText, bool required,
                                   string fieldType, string Default,
                                   bool visible, bool showOnEdit, bool searchable, bool isPrivateColumn,
                                   bool multipleValues,
                                   string inputSettings, string outputSettings, bool normalizeFlag,
                                   string validationRule,
                                   string validationMessage, string editStyle)
        {
            return DataProvider.Instance().AddField(moduleId, fieldTitle, before, helpText, required, fieldType, Default,
                                                    visible, showOnEdit, searchable, isPrivateColumn, multipleValues,
                                                    inputSettings, outputSettings, normalizeFlag, validationRule,
                                                    validationMessage, editStyle);
        }

        public static int AddField(int moduleId, string fieldTitle)
        {
            return AddField(moduleId, fieldTitle, Null.NullInteger, string.Empty, false,
                            DataTypeNames.UDT_DataType_String,
                            string.Empty, true, true, true, false, false, string.Empty, string.Empty, false,
                            string.Empty, string.Empty, string.Empty);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   persists an altered column setting in the database
        /// </summary>
        /// <param name = "userDefinedFieldId">ID of the column field</param>
        /// <param name = "fieldTitle">ID of the column</param>
        /// <param name="helpText"></param>
        /// <param name = "required">is an entry in this column required, when adding a row?</param>
        /// <param name = "fieldType">type name of data field</param>
        /// <param name = "Default">default value, entered when a new row is added</param>
        /// <param name = "visible">is this column visible in list view?</param>
        /// <param name="showOnEdit"></param>
        /// <param name = "searchable">is this column available for search in list view?</param>
        /// <param name="multipleValues"></param>
        /// <param name = "inputSettings">additional settings stored in single string</param>
        /// <param name = "outputSettings">additional settings stored in single string</param>
        /// <param name = "normalizeFlag">display flag, usage dependant on data type</param>
        /// <param name = "validationRule">optional expresion that needs to be true, to successfully enter a record</param>
        /// <param name = "validationMessage">displayed, if validation rules is eveluated to "false"</param>
        /// <param name="isPrivateColumn"></param>
        /// <param name="editStyle"></param>
        /// -----------------------------------------------------------------------------
        public static void UpdateField(int userDefinedFieldId, string fieldTitle, string helpText, bool required,
                                string fieldType, string Default, bool visible, bool showOnEdit, bool searchable,
                                bool isPrivateColumn, bool multipleValues, string inputSettings, string outputSettings,
                                bool normalizeFlag, string validationRule, string validationMessage, string editStyle)
        {
            DataProvider.Instance().UpdateField(userDefinedFieldId, fieldTitle, helpText, required, fieldType, Default,
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
        public static DataTable GetFieldsTable(int moduleId)
        {
            return GetFieldsTable(moduleId, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all field definitions for one UDT table (module) from the database
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name = "addNewColumn">specifies, whether a new column shall be added</param>
        /// <returns>All field settings as DataTable</returns>
        /// -----------------------------------------------------------------------------
        public static  DataTable GetFieldsTable(int moduleId, bool addNewColumn)
        {
            return GetFieldsTable( moduleId, addNewColumn, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets all field definitions for one UDT table (module) from the database
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name = "addNewColumn">specifies, whether a new column shall be added</param>
        /// <param name = "addAuditColumns">specifies, whether colums for creation and update (user and timestamp) shall be added</param>
        /// <returns>All field settings as DataTable</returns>
        /// -----------------------------------------------------------------------------
        public static DataTable GetFieldsTable(int moduleId, bool addNewColumn, bool addAuditColumns)
        {
            DataTable fieldsTable;
            using (var dr = DataProvider.Instance().GetFields(moduleId))
            {
                fieldsTable = Globals.ConvertDataReaderToDataTable(dr);
            }


            //Insert CreatedBy & Co Fields
            if (addAuditColumns && fieldsTable.Rows.Count == 0)
            {
                const int order = 0;
                foreach (var localizedFieldType in DataType.SystemDataTypes().Keys)
                {
                    AddField(moduleId , localizedFieldType, order, string.Empty, true,
                             DataType.SystemDataTypes()[localizedFieldType],
                             string.Empty, false, false, false, false, false, string.Empty, string.Empty, false,
                             string.Empty, string.Empty, string.Empty);
                }
                return GetFieldsTable(moduleId, addNewColumn);
                //Reload fields again
            }

            if (addNewColumn)
            {
                //find position (insert before first trailing "insert/updated by/at" field):
                var pos = fieldsTable.Rows.Count;
                var intOrder = 0;
                while (pos >= 1)
                {
                    var fieldType = fieldsTable.Rows[pos - 1]["FieldType"].ToString();
                    if (DataType.ByName(fieldType).IsUserDefinedField)
                    {
                        break;
                    }
                    pos--;
                    intOrder = int.Parse((fieldsTable.Rows[pos]["FieldOrder"].ToString()));
                }
                var row = fieldsTable.NewRow();
                row[FieldsTableColumn.Id] = Null.NullInteger.ToString(CultureInfo.InvariantCulture);
                row[FieldsTableColumn.Title] = string.Empty;
                row[FieldsTableColumn.Required] = false;
                row[FieldsTableColumn.Type] = "String";
                row[FieldsTableColumn.Default] = string.Empty;
                row[FieldsTableColumn.Visible] = true;
                row[FieldsTableColumn.Searchable] = false;
                row[FieldsTableColumn.IsPrivate] = false;
                row[FieldsTableColumn.NormalizeFlag] = false;
                row[FieldsTableColumn.Order] = intOrder;
                row[FieldsTableColumn.ShowOnEdit] = true;
                fieldsTable.Rows.InsertAt(row, pos);
            }
            return fieldsTable;
        }




    }
}