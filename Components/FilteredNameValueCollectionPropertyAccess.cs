using System.Collections.Specialized;
using System.Globalization;
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
                value = security.InputFilter(value, PortalSecurity.FilterFlag.NoScripting);
                return security.InputFilter(PropertyAccess.FormatString(value, strFormat),
                                            PortalSecurity.FilterFlag.NoScripting);
            }
            else
            {
                PropertyNotFound = true;
                return string.Empty;
            }
        }
    }
}