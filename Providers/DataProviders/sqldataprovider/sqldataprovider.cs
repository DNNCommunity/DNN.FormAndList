using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Modules.UserDefinedTable.Components;
using Microsoft.ApplicationBlocks.Data;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.UserDefinedTable
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The SqlDataProvider Class is an SQL Server implementation of the DataProvider Abstract
    ///   class that provides the DataLayer for the UserDefinedTables Module.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class SqlDataProvider : DataProvider
    {
        #region Private Members

        const string ProviderType = "data";
        const string SPPrefix = "UserDefinedTable_";

        readonly ProviderConfiguration _providerConfiguration =ProviderConfiguration.GetProviderConfiguration (ProviderType);

        readonly string _connectionString;
        readonly string _providerPath;
        readonly string _objectQualifier;
        readonly string _databaseOwner;

        #endregion

        #region Constructors

        public SqlDataProvider()
        {
            // Read the configuration specific information for this provider
            var objProvider = (Provider) (_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

            //Get Connection string from web.config
            _connectionString = Config.GetConnectionString();

            if (_connectionString == "")
            {
                // Use connection string specified in provider
                _connectionString = objProvider.Attributes["connectionString"];
            }

            _providerPath = objProvider.Attributes["providerPath"];

            _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (_objectQualifier != "" && ! _objectQualifier.EndsWith("_"))
            {
                _objectQualifier += "_";
            }

            _databaseOwner = objProvider.Attributes["databaseOwner"];
            if (_databaseOwner != "" && ! _databaseOwner.EndsWith("."))
            {
                _databaseOwner += ".";
            }
        }

        #endregion

        #region Properties

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string ProviderPath
        {
            get { return _providerPath; }
        }

        public string ObjectQualifier
        {
            get { return _objectQualifier; }
        }

        public string DatabaseOwner
        {
            get { return _databaseOwner; }
        }

        #endregion

        #region Public Methods

        object GetNull(object Field)
        {
            return Null.GetNull(Field, DBNull.Value);
        }

        public override IDataReader GetFields(int ModuleId)
        {
            return
                ((SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "GetFields",
                                          ModuleId)));
        }

        public override IDataReader GetField(int UserDefinedFieldId)
        {
            return
                ((SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "GetField",
                                          UserDefinedFieldId)));
        }

        public override IDataReader GetRow(int UserDefinedRowId, int ModuleId)
        {
            return
                ((SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "GetRow",
                                          UserDefinedRowId, ModuleId)));
        }

        public override void DeleteField(int UserDefinedFieldId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "DeleteField",
                                      UserDefinedFieldId);
        }

        public override int AddField(int ModuleId, string FieldTitle, int BeforePos, string HelpText, bool Required,
                                     string FieldType, string Default, bool Visible, bool ShowOnEdit, bool Searchable,
                                     bool IsPrivateColumn, bool MultipleValues, string InputSettings,
                                     string OutputSettings, bool NormalizeFlag, string validationRule,
                                     string validationMessage, string EditStyle)
        {
            return
                Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString,
                                                        DatabaseOwner + ObjectQualifier + SPPrefix + "AddField",
                                                        ModuleId, FieldTitle, GetNull(BeforePos), GetNull(HelpText),
                                                        Required, FieldType, GetNull(Default), Visible, ShowOnEdit,
                                                        Searchable, IsPrivateColumn, MultipleValues,
                                                        GetNull(InputSettings), GetNull(OutputSettings), NormalizeFlag,
                                                        GetNull(validationRule), GetNull(validationMessage),
                                                        GetNull(EditStyle)));
        }

        public override void FillDefaultData(int ModuleId, int FieldId, string DefaultValue)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "FillDefaultData",
                                      ModuleId, GetNull(FieldId), DefaultValue);
        }

       

        public override void UpdateField(int UserDefinedFieldId, string FieldTitle, string HelpText, bool Required,
                                         string FieldType, string Default, bool Visible, bool ShowOnEdit,
                                         bool Searchable, bool IsPrivateColumn, bool MultipleValues,
                                         string InputSettings, string OutputSettings, bool NormalizeFlag,
                                         string validationRule, string validationMessage, string EditStyle)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "UpdateField",
                                      UserDefinedFieldId, FieldTitle, GetNull(HelpText), Required, FieldType,
                                      GetNull(Default), Visible, ShowOnEdit, Searchable, IsPrivateColumn, MultipleValues,
                                      GetNull(InputSettings), GetNull(OutputSettings), NormalizeFlag,
                                      GetNull(validationRule), GetNull(validationMessage), GetNull(EditStyle));
            
        }

        public override IDataReader GetRows(int ModuleId)
        {
            return
                ((SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "GetRows",
                                          ModuleId)));
        }

        public override IDataReader GetFieldSettings(int moduleId)
        {
            return ((SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "GetFieldSettings",
                                       moduleId)));
        }

        public override void UpdateFieldSetting(int fieldid, string key, string value)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, 
                DatabaseOwner + ObjectQualifier + SPPrefix + "UpdateFieldSetting", 
                fieldid,key,value);
        }

        public override void Reset(int ModuleId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "Reset", ModuleId);
        }

        public override void DeleteRow(int UserDefinedRowId, int ModuleId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "DeleteRow",
                                      UserDefinedRowId, ModuleId);
        }

        public override void DeleteRows(int ModuleId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "DeleteRows",
                                      ModuleId);
        }

        public override int AddRow(int ModuleId)
        {
            return
                Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString,
                                                        DatabaseOwner + ObjectQualifier + SPPrefix + "AddRow", ModuleId));
        }

        public override IDataReader GetData(int UserDefinedRowId, int UserDefinedFieldId)
        {
            return
                ((SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "GetData",
                                          UserDefinedRowId, UserDefinedFieldId)));
        }

        public override void AddData(int UserDefinedRowId, int UserDefinedFieldId, string FieldValue)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "AddData",
                                      UserDefinedRowId, UserDefinedFieldId, FieldValue);
        }

        public override void UpdateData(int UserDefinedRowId, int UserDefinedFieldId, string FieldValue)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "UpdateData",
                                      UserDefinedRowId, UserDefinedFieldId, FieldValue);
        }

        public override void UpdateData(int rowId, IDictionary<int, string> values)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var trx = con.BeginTransaction();
                try
                {
                    foreach (var kvp in values)
                    {
                        SqlHelper.ExecuteNonQuery(trx, DatabaseOwner + ObjectQualifier + SPPrefix + "UpdateData",
                                                  rowId, kvp.Key, kvp.Value);
                    }
                    trx.Commit();
                }
                catch 
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public override void SwapFieldOrder(int FirstUserDefinedFieldId, int SecondUserDefinedFieldId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "SwapFieldOrder",
                                      FirstUserDefinedFieldId, SecondUserDefinedFieldId);
        }

        public override void DeleteData(int UserDefinedRowId, int UserDefinedFieldId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "DeleteData",
                                      UserDefinedRowId, UserDefinedFieldId);
        }

        public override int GetMaxFieldSize()
        {
            return Null.NullInteger;
            // FieldValue is no longer stored in side a NVARCHAR(2000) column
        }

        public override int GetFieldDataCount(int UserDefinedFieldID)
        {
            return
                Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString,
                                                        DatabaseOwner + ObjectQualifier + SPPrefix + "GetFieldDataCount",
                                                        UserDefinedFieldID));
        }

        public override void SetFieldOrder(int UserDefinedFieldID, int FieldOrder)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + SPPrefix + "SetFieldOrder",
                                      UserDefinedFieldID, FieldOrder);
        }

        #endregion
    }
}