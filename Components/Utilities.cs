using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public  static class Utilities
    {
        #region  Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   returns default, when value is nothing, otherwise tries to convert value to string and returns it.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string AsString(this object value)
        {
            return value.AsString("");
        }

        public static string AsString(this object value, string @default )
        {
            if (value == DBNull.Value ||value==null|| String.IsNullOrEmpty(value.ToString() ))
            {
                return @default;
            }
            return (value.ToString());
        }

        public static string AsString(this DataRow row, string columnname )
        {
            return row.AsString(columnname, "");
        }

        public static string AsString(this DataRow row, string columnname, string @default)
        {
            object value = null;
            if (row.Table.Columns.Contains(columnname))
            {
                value = row[columnname];
            }
            return AsString(value, @default);
        }

        public static Boolean Like(this string text, string pattern)
        {
            return LikeOperator.LikeString(text, pattern, CompareMethod.Text);
        }

        public static bool AsBoolean(this object value)
        {
            return value.AsBoolean(false);
        }

        public static bool AsBoolean(this object value, bool @default)
        {
            if (value == null)
            {
                return @default;
            }
            if (value == DBNull.Value)
            {
                return @default;
            }
            if (value is bool) return (bool) value;
            try
            {
                return Boolean.Parse(value.ToString());
            }
            catch
            {
                return false;
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   returns Null.NullInteger, when value is nothing, otherwise tries to convert value to Integer and returns it.
        /// </summary>
        /// <param name = "value">object containing value to return</param>
        /// -----------------------------------------------------------------------------
        public static int AsInt(this object value)
        {
            return AsInt(value, 0);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   returns default, when value is nothing, otherwise tries to convert value to Integer and returns it.
        /// </summary>
        /// <param name = "value">object containing value to return</param>
        /// <param name = "default">Default value</param>
        /// -----------------------------------------------------------------------------
        public static int AsInt(this object value, int @default)
        {
            if (value == null)
            {
                return @default;
            }
            if (value == DBNull.Value)
            {
                return @default;
            }
            if (value is int) return (int) value;
            int i;
            return Int32.TryParse(value.ToString(), out i) ? i : Null.NullInteger;
        }

        public static bool ValidateRegEx(this string value, string pattern)
        {
            var r = new Regex(pattern);
            return r.IsMatch(value);
        }

        public static bool HasWritePermission(string folder, int portalid)
        {
            var folderInfo = FolderManager.Instance.GetFolder(portalid, folder);
            return folder.StartsWith(Definition.XSLFolderName) || (folderInfo is FolderInfo && FolderPermissionController.CanAdminFolder((FolderInfo)folderInfo));
        }

        public static bool SaveScript(string fileContent, string fileName, IFolderInfo folder, bool forceOverWrite)
        {
            if (forceOverWrite || !FileManager.Instance.FileExists(folder,fileName ))
            {
                var utf8 = new UTF8Encoding();
                FileManager.Instance.AddFile(folder, fileName, new MemoryStream(utf8.GetBytes(fileContent)),forceOverWrite);
                return true;
            }
            return false;
        }

        static public string ReadStringFromFile(string homeFilePath, int portalId)
        {
            string filecontent;
            var file =  FileManager.Instance.GetFile(portalId, homeFilePath);
            using (var fileStream = FileManager.Instance.GetFileContent(file))
            {
                using (var tx = new StreamReader(fileStream))
                {
                    filecontent = tx.ReadToEnd();
                }
            }
            return filecontent;
        }

        public static IFolderInfo GetFolder(PortalSettings portalsettings, string homeFolderName)
        {
            return GetFolder(portalsettings, homeFolderName, homeFolderName);
        }

        public static IFolderInfo GetFolder(PortalSettings portalsettings, string homeFolderName,
                                           string defaultFolderName)
        {
            var folder=  FolderManager.Instance.GetFolder(portalsettings.PortalId, homeFolderName) ??
                         FolderManager.Instance.AddFolder(portalsettings.PortalId, defaultFolderName);
            return folder;
        }

        public static string UrlHexEncode(this string value)
        {
            return BitConverter.ToString(Encoding.UTF8.GetBytes(value)).Replace("-", "");
        }

        public static string UrlHexDecode(this string value)
        {
            var i = 0;
            var x = 0;
            var bytes = new byte[value.Length/2 - 1 + 1];
            while (value.Length > i + 1)
            {
                var byteValue = Convert.ToInt32(value.Substring(i, 2), 16);
                bytes[x] = Convert.ToByte((short) byteValue);
                i += 2;
                x++;
            }
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion

        public static string IconURL(string iconKey)
        {
            return IconController.IconURL(iconKey);
            //switch (iconKey.ToLowerInvariant()  )
            //{
            //    case "wizard":
            //        iconKey = "icon_wizard_16px";
            //        break;
            //}
            //return String.Format("~/images/{0}.gif", iconKey);
        }
    }
}