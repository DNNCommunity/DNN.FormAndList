using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetNuke.Common;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public static class FieldSettingsController
    {
        public static DataTable GetFieldSettingsTable(int moduleId)
        {
            DataTable fieldSettingsTable;
            using (var dr = DataProvider.Instance().GetFieldSettings(moduleId))
            {
                fieldSettingsTable = Globals.ConvertDataReaderToDataTable(dr);
            }
            fieldSettingsTable.TableName = DataSetTableName.FieldSettings; 
            return fieldSettingsTable;
        }

        public static string GetFieldSetting(this DataTable table, string key, int fieldId)
        {
            var r = table.Select(String.Format("FieldId={0} and SettingName='{1}'", fieldId, key)).FirstOrDefault();
            return (string)(r == null ? null : r["SettingValue"]);
        }

        public static void  UpdateFieldSetting( string key, string value, int fieldId)
        {
            DataProvider.Instance().UpdateFieldSetting(fieldId, key, value);
        }

        public static DataView WithFieldId(this DataTable table, int fieldId)
        {
            var filter = (String.Format("FieldId={0}", fieldId));
            return new DataView(table, filter, string.Empty, DataViewRowState.CurrentRows);
        }

       
    }
}