using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using System.Globalization;
using System.Web;
using System.Xml;

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

    public class TokenReplaceForForms : Services.Tokens.BaseCustomTokenReplace
    {
        class PlaceHolderForControl_PropertyAccess : IPropertyAccess
        {
            readonly string _idprefix;
            public PlaceHolderForControl_PropertyAccess(string idprefix)
            {
                _idprefix = idprefix;
            }

            public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
            {
                return $"  <asp:PlaceHolder runat=\"server\" ID=\"{_idprefix}_{propertyName.SafeId()}\"/>";
            }

            public CacheLevel Cacheability
            {
                get { return CacheLevel.fullyCacheable; }
            }
        }
        public TokenReplaceForForms()
        {
            PropertySource["label-for"] = new PlaceHolderForControl_PropertyAccess("label_for");
            PropertySource["editor-for"] = new PlaceHolderForControl_PropertyAccess("editor_for");
        }
        public string GetControlTemplate(string input)
        {
            return this.ReplaceTokens(input);
        }
    }
}