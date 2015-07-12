namespace DotNetNuke.Modules.UserDefinedTable.Interfaces
{
    public interface IPortable2
    {
        bool ManagesModuleSettings { get; }
        bool ManagesTabModuleSettings { get; }
        string ExportModule(int moduleId, int tabId);
        string ExportModule(int moduleId, int tabId, int maxNumberOfItems);
        void ImportModule(int moduleId, int tabId, string content, string version, int userId, bool isInstance);
    }
}