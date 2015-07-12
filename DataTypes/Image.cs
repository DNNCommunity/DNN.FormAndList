using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

namespace DotNetNuke.Modules.UserDefinedTable.DataTypes
{

    #region EditControl

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Edit and Validation Control for DataType "Image"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class EditImage : EditURL
    {
        void EditImage_Load(object sender, EventArgs e)
        {
            if (! Page.IsPostBack && IsNotAListOfValues)
            {
                CtlUrl.Required = Required;
                CtlUrl.FileFilter = Globals.glbImageFileTypes;
                CtlUrl.ShowLog = false;
                CtlUrl.ShowTrack = false;
                CtlUrl.ShowTabs = false;
                CtlUrl.ShowUrls = true;
                CtlUrl.ShowFiles = true;
                CtlUrl.ShowNewWindow = false;
            }
        }

        public EditImage()
        {
            Load += EditImage_Load;
        }
    }

    #endregion

    #region DataType

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   MetaData and Formating for DataType "Image"
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DataTypeImage : DataType
    {
        readonly FieldSettingType[] _fieldSettingTypes= new[]
                    {
                        new FieldSettingType {Key = "AltCaption", Section = "List", SystemType = "String"},
                        new FieldSettingType {Key = "AsLink", Section = "List", SystemType = "Boolean"},
                        new FieldSettingType {Key = "Width", Section = "List", SystemType = "Int"},
                        new FieldSettingType {Key = "Height", Section = "List", SystemType = "Int"}
                    };

        public override  IEnumerable<FieldSettingType> FieldSettingTypes
        {
            get
            {
                return _fieldSettingTypes;
            }
        }
        public override string Name
        {
            get { return "Image"; }
        }



        struct FieldSetting
        {
            public string Title;
            public string AltCaption;
            public bool AsLink;
            public int Width;
            public int Height;
        }

        struct ImageFields
        {
            public string Value;
       
            public string Original;
            public string Url;
        }


        public override void RenderValuesToHtmlInsideDataSet(DataSet ds, int moduleId, bool noScript)
        {
            var fields = new ArrayList();
            var tableData = ds.Tables[DataSetTableName.Data];
            var tokenReplace = new TokenReplace {ModuleId = moduleId};
            foreach (DataRow row in ds.Tables[DataSetTableName.Fields].Rows)
            {
                if (row[FieldsTableColumn.Type].ToString() == Name)
                {
                    var fieldId = (int) row[FieldsTableColumn.Id];
                    var field = new FieldSetting
                                    {
                                        Title = row[FieldsTableColumn.Title].ToString(),
                                        AltCaption = GetFieldSetting("AltCaption",fieldId,ds).AsString(),
                                        AsLink = GetFieldSetting("AsLink", fieldId, ds).AsBoolean(),
                                        Width = GetFieldSetting("Width", fieldId, ds).AsInt(),
                                        Height = GetFieldSetting("Height", fieldId, ds).AsInt(),
                                    };
                    fields.Add(field);
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Url, typeof (string)));
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Caption, typeof (string)));
                    tableData.Columns.Add(new DataColumn(field.Title + DataTableColumn.Appendix_Original,
                                                         typeof (string)));
                }
            }
           
            if (fields.Count > 0)
            {
                var portalSecurity = new PortalSecurity();
                var portalId = Null.NullInteger; 
                if (HttpContext.Current != null)
                {
                    var portalSettings = PortalController.GetCurrentPortalSettings();
                    portalId = portalSettings.PortalId;
                }

                var cache = new Dictionary<string, ImageFields>();
                foreach (DataRow row in ds.Tables[DataSetTableName.Data].Rows)
                {
                    foreach (FieldSetting setting in fields)
                    {
                        var altCaption = GetAltAttributeForImage(row, setting, tokenReplace);
                        var storedValue = row[setting.Title].ToString();
                        ImageFields imageFields;
                        if (cache.ContainsKey( storedValue )) 
                            imageFields = cache[storedValue];
                        else {
                            imageFields = GetImageFields(storedValue, setting, portalId);
                            cache[storedValue] = imageFields;
                        }
                        row[setting.Title] = String.Format( imageFields.Value,altCaption );
                        row[setting.Title + DataTableColumn.Appendix_Caption] = altCaption ;
                        row[setting.Title + DataTableColumn.Appendix_Original] = imageFields.Original;
                        row[setting.Title + DataTableColumn.Appendix_Url] = imageFields.Url;
                    }
                }
            }
        }

        static string GetAltAttributeForImage(DataRow row, FieldSetting setting, TokenReplace tokenReplace)
        {
            var altTag = setting.AltCaption;
            if (altTag == string.Empty)
            {
                altTag = setting.Title;
            }
            else
            {
                if (altTag != string.Empty)
                {
                    altTag = tokenReplace.ReplaceEnvironmentTokens(altTag, row);
                }
            }
            altTag = new PortalSecurity().InputFilter(altTag, PortalSecurity.FilterFlag.NoMarkup);
            return altTag;
        }

        static ImageFields GetImageFields(string value, FieldSetting setting, int portalId)
        {
            var strFieldvalue = value;
            var url = string.Empty;
            var imageUrl = string.Empty;
            var path = string.Empty;
            if (strFieldvalue != string.Empty)
            {
                if (strFieldvalue.StartsWith("http:") || strFieldvalue.StartsWith("https:"))
                {
                    imageUrl = strFieldvalue;
                    url = imageUrl;
                }
                else
                {
                    var fileInfo = strFieldvalue.StartsWith("FileID=")
                                       ? FileManager.Instance.GetFile(int.Parse(UrlUtils.GetParameterValue(strFieldvalue)))
                                       : FileManager.Instance.GetFile(portalId, strFieldvalue);
                    if (fileInfo != null)
                    {
                        imageUrl = FileManager.Instance.GetUrl(fileInfo);
                        path = Path.Combine(fileInfo.Folder, fileInfo.FileName);
                    }
                    var parms = "";
                    if (setting.Width > 0)
                    {
                        parms = string.Format("{0}&w={1}", parms, setting.Width);
                    }
                    if (setting.Height > 0)
                    {
                        parms = string.Format("{0}&h={1}", parms, setting.Height);
                    }
                    if (parms != "")
                    {
                        url = string.Format("{0}?image={1}{2}&PortalId={3}",
                                            Globals.ResolveUrl(string.Format("~{0}MakeThumbnail.ashx",
                                                                             Definition.PathOfModule)),
                                            HttpUtility.UrlEncode(path), parms,
                                            portalId);
                    }
                    else
                    {
                        url = imageUrl;
                    }
                }
              
                url = HttpUtility.HtmlEncode(url);
                if (setting.AsLink)
                {
                    strFieldvalue =
                        string.Format(
                            "<a href=\"{0}\" target=\"_blank\"><img alt=\"{1}\" title=\"{1}\" border=\"0\" src=\"{2}\" /></a>",
                            imageUrl, "{0}", url);
                }
                else
                {
                    strFieldvalue = string.Format("<img alt=\"{0}\" title=\"{0}\" src=\"{1}\" />", "{0}",
                                                  url);
                }
            }
            var imageFields = new ImageFields
                                  {
                                      Value = strFieldvalue,
                                     
                                      Original = value,
                                      Url = url
                                  };
            return imageFields;
        }

        public override string SupportedCasts
        {
            get { return string.Format("{0}|URL", base.SupportedCasts); }
        }


        public override bool SupportsDefaultValue
        {
            get { return true; }
        }

        public override bool SupportsEditing
        {
            get { return true; }
        }

        public override bool SupportsOutputSettings
        {
            get { return false; }
        }

        public override bool SupportsInputSettings
        {
            get { return true; }
        }

        public override bool SupportsSearch
        {
            get { return true; }
        }
    }

    #endregion
}