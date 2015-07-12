using System.Web;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class TokenReplace : Services.Tokens.TokenReplace
    {
        readonly bool _escapeApostrophe;

        public TokenReplace() : this(false)
        {
        }

        public TokenReplace(bool escapeApostrophe)
        {
            _escapeApostrophe = escapeApostrophe;
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                PropertySource["querystring"] = new FilteredNameValueCollectionPropertyAccess(request.QueryString);
                PropertySource["form"] = new FilteredNameValueCollectionPropertyAccess(request.Form);
                PropertySource["server"] = new FilteredNameValueCollectionPropertyAccess(request.ServerVariables);
            }
        }

        protected override string replacedTokenValue(string objectName, string propertyName, string formatString)
        {
            var returnvalue = base.replacedTokenValue(objectName, propertyName, formatString);
            if (_escapeApostrophe)
            {
                returnvalue = returnvalue.Replace("\'", "\'\'");
            }
            return returnvalue;
        }
    }
}