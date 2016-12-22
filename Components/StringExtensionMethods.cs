using System.Text.RegularExpressions;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public static class StringExtensionMethods
    {
        public static string SafeId(this string name)
        {
            return new Regex("[^a-z_A-Z0-9-]")
                .Replace(name,"")
                .Replace('-', '_');
        }
    }
}