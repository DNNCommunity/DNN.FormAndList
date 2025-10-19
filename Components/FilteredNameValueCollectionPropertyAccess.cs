using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class FilteredNameValueCollectionPropertyAccess : IPropertyAccess
    {
        readonly NameValueCollection NameValueCollection;

        public FilteredNameValueCollectionPropertyAccess(NameValueCollection list)
        {
            NameValueCollection = list;
        }


        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string strPropertyName, string strFormat, CultureInfo formatProvider,
                                  UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (NameValueCollection == null)
            {
                return string.Empty;
            }
            var value = NameValueCollection[strPropertyName];
        
            if (value != null)
            {
                var security = new PortalSecurity();
                value = WebUtility.HtmlEncode(value);
                return WebUtility.HtmlDecode(PropertyAccess.FormatString(value, strFormat));
            }
            else
            {
                PropertyNotFound = true;
                return string.Empty;
            }
        }
    }
}