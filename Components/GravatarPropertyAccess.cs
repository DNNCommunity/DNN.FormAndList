using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class GravatarPropertyAccess : IPropertyAccess
    {
        readonly string _email;

        public GravatarPropertyAccess(string email)
        {
            _email = email.Trim().ToLowerInvariant();
        }

        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string strPropertyName, string strFormat, CultureInfo formatProvider,
                                  UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            strPropertyName = strPropertyName.ToLowerInvariant();
            switch (strPropertyName)
            {
                case "md5hash":
                    return PropertyAccess.FormatString(Md5Hash(_email), strFormat);
                case "url":
                    return
                        PropertyAccess.FormatString(
                            string.Format("http://www.gravatar.com/avatar/{0}", Md5Hash(_email)), strFormat);
                case "image":
                    return
                        PropertyAccess.FormatString(
                            string.Format("<img alt=\"gravatar\" src=\"http://www.gravatar.com/avatar/{0}\" />",
                                          Md5Hash(_email)), strFormat);
                default:
                    propertyNotFound = true;
                    return string.Empty;
            }
        }

        static string Md5Hash(string value)
        {
            using (var serviceProvider = new MD5CryptoServiceProvider())
            {
                var bytesToHash = Encoding.ASCII.GetBytes(value);
                bytesToHash = serviceProvider.ComputeHash(bytesToHash);

                var sb = new StringBuilder();
                foreach (var b in bytesToHash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}