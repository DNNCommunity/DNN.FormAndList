using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Framework;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.DataTypes;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Abstraced (MustInherit) DataType Class
    ///   Defines the Interface and provides some Default Settings
    ///   Used for Definition and Rendering of Fields in the UserDefinedTable
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class DataType
    {
        #region Internal Utilities (Protected)

        protected enum DbTypes
        {
            @Int32,
            @Decimal,
            @DateTime,
            @String,
            @Boolean
        }

        protected static IDictionary ListOfCommonDataTypes
        {
            get
            {
                var dic = new SortedDictionary<string, string>();
                foreach (var typeName in Enum.GetNames(typeof (DbTypes)))
                {
                    dic.Add(ByName(typeName).GetLocalization(), typeName);
                }
                return dic;
            }
        }

        #endregion

        #region Static Interface

        static IDictionary<string, DataType> _colDataTypes;
        static readonly object DataTypeLock = new object();

        static IDictionary<string, DataType> DataTypes
        {
            get
            {
                if (_colDataTypes == null)
                {
                    lock (DataTypeLock)
                    {
                        _colDataTypes = new Dictionary<string, DataType>();

                        var doc = new XmlDocument();
// ReSharper disable AssignNullToNotNullAttribute
                        doc.Load(GetDataTypesConfigPath());
// ReSharper restore AssignNullToNotNullAttribute
// ReSharper disable PossibleNullReferenceException
                        foreach (XmlElement elem in doc.SelectNodes("dataTypes/dataType"))
// ReSharper restore PossibleNullReferenceException
                        {
                            string systemTypeName;
                            if (elem.HasAttribute("typeName"))
                            {
                                systemTypeName = elem.GetAttribute("typeName");
                            }
                            else
                            {
                                systemTypeName = string.Format("DotNetNuke.Modules.UserDefinedTable.DataTypes.DataType{0}",
                                                               elem.GetAttribute("name"));
                            }
                            try
                            {
                                var type = (DataType)(Reflection.CreateObject(systemTypeName, systemTypeName));
                                _colDataTypes.Add(type.Name, type);
                            }
                            catch (Exception)
                            {
                                if (!systemTypeName.StartsWith("DotNetNuke.Modules.UserDefinedTable.DataTypes."))
                                {
                                    throw;
                                }
                                //Supporting old namespace
                                systemTypeName = systemTypeName.Replace(
                                    "DotNetNuke.Modules.UserDefinedTable.DataTypes.",
                                    "DotNetNuke.Modules.UserDefinedTable.");
                                var type = (DataType) (Reflection.CreateObject(systemTypeName, systemTypeName));
                                _colDataTypes.Add(type.Name, type);
                            }
                      
                        }
                    }
                }
                return _colDataTypes;
            }
        }

        public static String GetDataTypesConfigPath()
        {
            var retval = "";

            // if a file exists in Portals/_Default/ UserDefinedTable then use that one, otherwise use default file
            var defaultConfig =
                HostingEnvironment.MapPath(string.Format("~{0}/DataTypes.config", Definition.PathOfModule));
            var customConfig =
                HostingEnvironment.MapPath(string.Format("~{0}/DataTypes.config", Definition.PathOfCustomConfig));

            if (File.Exists(customConfig))
                retval = customConfig;
            else
                retval = defaultConfig;

            return retval;
        }

        public static ICollection<string> AllDataTypes
        {
            get { return DataTypes.Keys; }
        }

        public static IDictionary<string, string> ContentDataTypes()
        {
            IDictionary<string, string> returnValue = new SortedDictionary<string, string>();
            foreach (var type in DataTypes.Values)
            {
                if (type.IsUserDefinedField)
                {
                    returnValue.Add(type.GetLocalization(), type.Name);
                }
            }
            return returnValue;
        }

        public static IDictionary<string, string> SystemDataTypes()
        {
            IDictionary<string, string> returnValue = new SortedDictionary<string, string>();
            foreach (var type in DataTypes.Values)
            {
                if (! type.IsUserDefinedField)
                {
                    returnValue.Add(type.GetLocalization(), type.Name);
                }
            }
            return returnValue;
        }

      


        [Obsolete("Obsolte, please use ContentDataTypes instead")]
        public static IDictionary<string, string> UserDefinedDataTypes
        {
            get { return ContentDataTypes(); }
        }

        [Obsolete("Obsolte, please use SystemDatatypes instead")]
        public static IDictionary<string, string> ObligateDataTypes
        {
            get { return SystemDataTypes(); }
        }

        public static DataType ByName(string dataTypeName)
        {
            if (DataTypes.ContainsKey(dataTypeName))
            {
                return DataTypes[dataTypeName];
            }
            Exceptions.LogException(
                new UnknownDataTypeException(string.Format("DataType \"{0}\" is not installed.", dataTypeName)));
            return DataTypes[DataTypeNames.UDT_DataType_String];
        }

        #endregion

        #region Public Interface

        public virtual string GetLocalization(string setting = "")
        {
            string fieldtype = setting != string.Empty ? string.Concat(Name, "_", setting) : Name;
            var result = Localization.GetString(fieldtype, LocalRescourceFile);
            if (string.IsNullOrEmpty(result))
            {
                Exceptions.LogException(
                    new MissingLocalizationForDataTypeException(
                        string.Format("No Rescource found for DataType \"{0}\", Setting \"{1}\".", Name, setting)));
                return fieldtype;
            }
            return result;
        }

        public virtual string LocalRescourceFile
        {
            get
            {
                return string.Format("{0}{1}{2}/SharedResources.resx", HostingEnvironment.ApplicationVirtualPath,
                                     Definition.PathOfModule, Localization.LocalResourceDirectory);
            }
        }


        public virtual EditControl EditControl
        {
            get
            {
                var typeName = ("DotNetNuke.Modules.UserDefinedTable.DataTypes.Edit" + Name);
                return ((EditControl) (Reflection.CreateObject(typeName, typeName)));
            }
        }

        public virtual IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get { return new FieldSettingType[0]; }
        }

        public virtual bool IsUserDefinedField
        {
            get { return true; }
        }

        public virtual string SystemTypeName
        {
            get { return "String"; }
        }

        public virtual bool SupportsDefaultValue
        {
            get { return false; }
        }

        public virtual bool SupportsEditing
        {
            get { return false; }
        }

        public virtual bool SupportsHideOnEdit
        {
            get { return false; }
        }

        public virtual IDictionary InputSettingsList
        {
            get { return null; }
        }

        public virtual bool SupportsInputSettings
        {
            get { return false; }
        }
        public virtual bool SupportsNormalizeFlag
        {
            get { return false; }
        }

        public virtual bool InputSettingsIsValueList
        {
            get { return SupportsInputSettings; }
        }

        public virtual string InputSettingDefault
        {
            get { return string.Empty; }
        }

        public virtual bool SupportsOutputSettings
        {
            get { return false; }
        }

        public virtual bool SupportsLateRendering
        {
            get { return false; }
        }

        public virtual bool SupportsMultipleValues
        {
            get { return false; }
        }


        public virtual bool SupportsValidation
        {
            get { return false; }
        }

        public virtual bool SupportsEditStyle
        {
            get { return false; }
        }

        public virtual bool SupportsSearch
        {
            get { return false; }
        }

        public virtual string SupportedCasts
        {
            get { return string.Format("String|{0}", Name); }
        }

        public virtual string SearchColumnAppendix
        {
            get { return ""; }
        }

        public virtual bool IsSeparator
        {
            get { return false; }
        }

        public virtual void SetStylesAndFormats(BoundField column, string format)
        {
            //No default formating
        }

        public virtual void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            //By Default, nothing happens
        }

      
        public abstract string Name { get; }

        #endregion

        protected object GetFieldSetting (string key, int id, DataSet ds)
        {
            return ds.Tables[DataSetTableName.FieldSettings].GetFieldSetting(key, id);
        }
    }

    [Serializable]
    public class UnknownDataTypeException : Exception
    {
        public UnknownDataTypeException()
        {
        }

        public UnknownDataTypeException(string message) : base(message)
        {
        }

        protected UnknownDataTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class MissingLocalizationForDataTypeException : Exception
    {
        public MissingLocalizationForDataTypeException()
        {
        }

        public MissingLocalizationForDataTypeException(string message) : base(message)
        {
        }

        protected MissingLocalizationForDataTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

