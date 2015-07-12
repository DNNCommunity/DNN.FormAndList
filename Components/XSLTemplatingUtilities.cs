using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class XslTemplatingUtilities
    {
        public const string SpacePlaceholder = "{5A255853-D9A0-4f46-9E9D-F661DC4874CD}";
        //Any Uncommon String would do
        public const string HardSpace = "&#160;";


        public enum ContextValues
        {
            ApplicationPath,
            CurrentCulture,
            DisplayName,
            HomePath,
            IsAdministratorRole,
            ModuleId,
            OrderBy,
            OrderDirection,
            Parameter,
            PortalId,
            TabId,
            TabName,
            UserName,
            UserRoles,
            LocalizedSearchString,
            NowInTicks,
            TicksPerDay,
            LocalizedDate,
            Now
        }

        static string LoadXslScriptTemplate(string listView, string detailView, string headerView, bool pagingEnabled,
                                            bool sortingEnabled, bool searchEnabled, bool showDetailView,
                                            string currentListType)
        {
            var templateDoc = new XmlDocument();
            templateDoc.Load(
                HttpContext.Current.Request.MapPath("~/DesktopModules/UserDefinedTable/xslStyleSheets/xslScripts.xml"));
            var xslTemplate =
// ReSharper disable PossibleNullReferenceException
                HttpUtility.HtmlDecode(templateDoc.SelectSingleNode("/root/data[@name=\"XSLT\"]/value").InnerText);
// ReSharper restore PossibleNullReferenceException
            xslTemplate = LoadXslScriptOption(templateDoc, xslTemplate, "paging", pagingEnabled);
            xslTemplate = LoadXslScriptOption(templateDoc, xslTemplate, "sorting", sortingEnabled);
            xslTemplate = LoadXslScriptOption(templateDoc, xslTemplate, "searching", searchEnabled);
            xslTemplate = LoadXslScriptOption(templateDoc, xslTemplate, "detail", showDetailView);
            xslTemplate =
                (xslTemplate.Replace("[LISTVIEW]", listView).Replace("[DETAILVIEW]", detailView).Replace(
                    "[HEADERVIEW]", headerView));
            string opentag;
            var opentagclass = string.Empty;
            switch (currentListType)
            {
                case "table":
                    opentag = currentListType;
                    opentagclass = @" class=""dnnFormItem""";
                    break;
                case "ul":
                case "ol":
                    opentag = currentListType;
                    break;
                default:
                    opentag = "";
                    break;
            }
            if (opentag == "")
            {
                xslTemplate = (xslTemplate.Replace("[OPENTAG]", "").Replace("[/OPENTAG]", ""));
            }
            else
            {
                xslTemplate = xslTemplate
                    .Replace("[OPENTAG]", string.Format("<{0}{1}>", opentag, opentagclass))
                    .Replace("[/OPENTAG]", string.Format("</{0}>", opentag));
            }

            return xslTemplate;
        }

        static string LoadXslScriptOption(XmlDocument def, string xsl, string extension, bool isEnabled)
        {
// ReSharper disable PossibleNullReferenceException
            foreach (XmlElement element in def.SelectNodes(string.Format("root/data[@name=\"{0}\"]", extension)))
// ReSharper restore PossibleNullReferenceException
            {
                var placeholder = string.Format("{{{0}{1}}}", extension.ToUpperInvariant(),
                                                element.Attributes["number"].InnerText);
                xsl = xsl.Replace(placeholder, isEnabled ? HttpUtility.HtmlDecode(element.FirstChild.InnerText) : "");
            }
            return xsl;
        }

        public static string TransformTokenTextToXslScript(DataSet udtDataset, string tokentemplate)
        {
            return TransformTokenTextToXslScript(udtDataset, tokentemplate, string.Empty, string.Empty, false, false,
                                                 false, false, string.Empty);
        }

        public static string TransformTokenTextToXslScript(DataSet udtDataset, string listView, string detailView,
                                                           string headerView, bool pagingEnabled, bool sortingEnabled,
                                                           bool searchEnabled, bool showDetailView,
                                                           string currentListType)
        {
            headerView = Regex.Replace(headerView, "\\[((?:\\w|\\s)+)\\]", 
                sortingEnabled 
                    ? "<xsl:apply-templates select =\"udt:Fields[udt:FieldTitle=\'$1\']\"/>" 
                    : "$1");


            string template = LoadXslScriptTemplate(listView, detailView, headerView, pagingEnabled, sortingEnabled,
                                                    searchEnabled, showDetailView, currentListType);
            template =
                (template.Replace("[ ]", SpacePlaceholder).Replace(HardSpace, SpacePlaceholder).Replace("&nbsp;",
                                                                                                        SpacePlaceholder));
            const string attributeRegexPattern = "(?<==\\s*[\'\"][^\'\"<>]*)(\\[{0}\\])(?=[^\'\"<>]*[\"\'])";
            foreach (DataColumn col in udtDataset.Tables[DataSetTableName.Data].Columns)
            {
                template = Regex.Replace(template, string.Format(attributeRegexPattern, col.ColumnName),
                                         string.Format("{{udt:{0}}}", XmlConvert.EncodeName(col.ColumnName)));
                template = template.Replace(string.Format("[{0}]", col.ColumnName),
                                            string.Format(
                                                "<xsl:value-of select=\"udt:{0}\"  disable-output-escaping=\"yes\"/>",
                                                XmlConvert.EncodeName(col.ColumnName)));
            }
            foreach (var contextString in Enum.GetNames(typeof (ContextValues)))
            {
                template = Regex.Replace(template, string.Format(attributeRegexPattern, "Context:" + contextString),
                                         string.Format("{{/udt:UserDefinedTable/udt:Context/udt:{0}}}", contextString));
                template = template.Replace(string.Format("[Context:{0}]", contextString),
                                            string.Format(
                                                "<xsl:value-of select=\"/udt:UserDefinedTable/udt:Context/udt:{0}\"  disable-output-escaping=\"yes\"/>",
                                                contextString));
            }
            template = template.Replace("[UDT:EditLink]", "<xsl:call-template name =\"EditLink\"/>");
            template = template.Replace("[UDT:DetailView]", "<xsl:call-template name =\"DetailView\"/>");
            template = template.Replace("[UDT:ListView]", "<xsl:call-template name =\"ListView\"/>");


            return PrettyPrint(template).Replace(SpacePlaceholder, HardSpace);
        }

        public static string PrettyPrint(string template)
        {
            var doc = new XmlDocument();
            doc.LoadXml(template);
            return PrettyPrint(doc);
        }

        public static string PrettyPrint(XmlDocument doc)
        {
            using (var strXML = new StringWriter())
            {
                using (var writer = new XmlTextWriter(strXML))
                {
                    try
                    {
                        writer.Formatting = Formatting.Indented;
                        doc.WriteTo(writer);
                        writer.Flush();
                        writer.Close();
                        return strXML.ToString().Replace("  <xsl:template", string.Format("{0}  <xsl:template", "\r\n"));
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public static string GenerateDetailViewTokenText(DataTable fieldstable)
        {
            return GenerateDetailViewTokenText(fieldstable, true);
        }

        public static string GenerateDetailViewTokenText(DataTable fieldstable, bool includeEditLink)
        {
            using (var sw = new StringWriter())
            {
                using (var xw = new XmlTextWriter(sw) {Formatting = Formatting.Indented})
                {
                    xw.WriteStartElement("table");
                    foreach (DataRow row in fieldstable.Rows)
                    {
                        xw.WriteStartElement("tr");
                        xw.WriteStartElement("td");
                        xw.WriteAttributeString("class", "normalBold");
                        xw.WriteString(row[FieldsTableColumn.Title].ToString());
                        xw.WriteEndElement();
                        xw.WriteStartElement("td");
                        xw.WriteAttributeString("class", "Normal");
                        xw.WriteString(string.Format("[{0}]",
                                                     XmlConvert.DecodeName(row[FieldsTableColumn.ValueColumn].ToString())));
                        xw.WriteEndElement();
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.Flush();
                    xw.Close();
                }

                return includeEditLink 
                    ? string.Format("[UDT:ListView][UDT:EditLink]{0}{1}", "\r\n", sw) 
                    : sw.ToString();
            }
        }
    }
}