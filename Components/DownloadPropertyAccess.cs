using System.Globalization;
using System.IO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Tokens;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class DownloadPropertyAccess : IPropertyAccess
    {
        readonly string _fileIdentifier;
        IFileInfo _currentFile;
        readonly int _moduleId;
        readonly int _portalId;

        public DownloadPropertyAccess(string id, int portalId, int moduleId)
        {
            _fileIdentifier = id;
            _portalId = portalId;
            _moduleId = moduleId;
        }

        IFileInfo File
        {
            get
            {
                if (_currentFile == null)
                {
                    if (LikeOperator.LikeString(_fileIdentifier, "FileID=*", CompareMethod.Binary))
                    {
                        _currentFile = FileManager.Instance.GetFile(int.Parse(UrlUtils.GetParameterValue(_fileIdentifier)));
                    }
                    else
                    {
                        if (Globals.GetURLType(_fileIdentifier) == TabType.File)
                        {
                            var fileName = Path.GetFileName(_fileIdentifier);
                            var folderInfo = FolderManager.Instance.GetFolder(_portalId,
                                                                              _fileIdentifier.Replace(fileName, ""));
                            if (folderInfo != null)
                            {
                                _currentFile = FileManager.Instance.GetFile(folderInfo,fileName);
                            }
                        }
                    }
                }
                return _currentFile;
            }
        }


        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        public string GetProperty(string strPropertyName, string strFormat, CultureInfo formatProvider,
                                  UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            string outputFormat = strFormat == string.Empty ? "g" : strFormat;

            strPropertyName = strPropertyName.ToLowerInvariant();

            if (File != null)
            {
                switch (strPropertyName)
                {
                    case "name":
                        return PropertyAccess.FormatString(File.FileName, strFormat);
                    case "folder":
                        return
                            PropertyAccess.FormatString(
                                FolderManager.Instance.GetFolder( File.FolderId).FolderName ,
                                strFormat);
                    case "path":
                        return
                            PropertyAccess.FormatString(
                                FolderManager.Instance.GetFolder(File.FolderId).FolderPath,
                                strFormat);
                    case "size":
                        return File.Size.ToString(outputFormat, formatProvider);
                    case "sizemb":
                        return (File.Size/1024 ^ 2).ToString(outputFormat, formatProvider);
                    case "extension":
                        return PropertyAccess.FormatString(File.Extension, strFormat);
                }
            }
            if (strPropertyName == "clicks")
            {
                var tracking = new UrlController().GetUrlTracking(_portalId, _fileIdentifier, _moduleId);
                return tracking != null ? tracking.Clicks.ToString(outputFormat, formatProvider) : "";
            }
            propertyNotFound = true;
            return string.Empty;
        }
    }
}