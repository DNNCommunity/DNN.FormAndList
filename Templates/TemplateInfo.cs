using System;
using System.IO;
using System.Xml;

namespace DotNetNuke.Modules.UserDefinedTable.Templates
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class TemplateInfo
    {
        string _Name;
        string _Description;
        string _Export;
        XmlNode _AdditionalData;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public string ExportContent
        {
            get { return _Export; }
            set { _Export = value; }
        }

        public XmlNode AdditionalData
        {
            get { return _AdditionalData; }
            set { _AdditionalData = value; }
        }


        public string GetXml()
        {
            using (var strXML = new StringWriter())
            {
                using (var xmlWriter = new XmlTextWriter(strXML))
                {
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.WriteStartElement("moduletemplate");
                    xmlWriter.WriteAttributeString("title", Name);
                    xmlWriter.WriteAttributeString("description", Description);
                    xmlWriter.WriteAttributeString("xmlns", "ask", null, "DotNetNuke/ModuleTemplate");
                    xmlWriter.WriteRaw(ExportContent);
                    xmlWriter.WriteEndElement();
                    xmlWriter.Close();
                }

                return strXML.ToString();
            }
        }
    }
}