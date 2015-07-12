using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The DataProvider Class Is an abstract class that provides the DataLayer
    ///   for the UserDefinedTable Module.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class DataProvider
    {
        #region Shared/Static Methods

        // singleton reference to the instantiated object
        static DataProvider _provider;

        // constructor
        static DataProvider()
        {
            CreateProvider();
        }

        // dynamically create provider
        static void CreateProvider()
        {
            _provider = (DataProvider) (Reflection.CreateObject("data", "DotNetNuke.Modules.UserDefinedTable", ""));
        }

        // return the provider
        public static DataProvider Instance()
        {
            return _provider;
        }

        #endregion

        #region Abstract methods

        public abstract void AddData(int UserDefinedRowID, int UserDefinedFieldID, string FieldValue);

        public abstract int AddField(int ModuleID, string FieldTitle, int before, string HelpText, bool Required,
                                     string FieldType, string Default, bool Visible, bool ShowOnEdit, bool Searchable,
                                     bool isPrivateColumn, bool MultipleValues, string inputSettings,
                                     string outputSettings, bool NormalizeFlag, string validationRule,
                                     string validationMessage, string EditStyle);

        public abstract int AddRow(int ModuleId);

        public abstract void FillDefaultData(int ModuleId, int FieldId, string DefaultValue);

        public abstract void DeleteData(int UserDefinedRowID, int UserDefinedFieldID);

        public abstract void DeleteField(int UserDefinedFieldID);

        public abstract void DeleteRow(int UserDefinedRowID, int ModuleId);

        public abstract void DeleteRows(int ModuleId);

        public abstract void Reset(int ModuleId);

        public abstract IDataReader GetData(int UserDefinedRowID, int UserDefinedFieldID);

        public abstract IDataReader GetField(int UserDefinedFieldId);

        public abstract IDataReader GetFields(int ModuleId);

        public abstract IDataReader GetRow(int UserDefinedRowId, int ModuleId);

        public abstract IDataReader GetRows(int ModuleId);
        public abstract IDataReader GetFieldSettings(int moduleId);
        public abstract void UpdateFieldSetting(int fieldid, string key, string value);

 
        public abstract void UpdateData(int UserDefinedRowID, int UserDefinedFieldID, string FieldValue);
        public abstract void UpdateData(int rowId, IDictionary<int,string > values );


        public abstract void UpdateField(int UserDefinedFieldID, string FieldTitle, string HelpText, bool Required,
                                         string FieldType, string Default, bool Visible, bool ShowOnEdit,
                                         bool Searchable, bool isPrivateColumn, bool MultipleValues,
                                         string inputSettings, string outputSettings, bool NormalizeFlag,
                                         string validationRule, string validationMessage, string EditStyle);

        public abstract void SwapFieldOrder(int FirstUserDefinedFieldId, int SecondUserDefinedFieldId);

        public abstract int GetMaxFieldSize();

        public abstract int GetFieldDataCount(int UserDefinedFieldID);
        public abstract void SetFieldOrder(int UserDefinedFieldID, int FieldOrder);
        

        #endregion


    }
}