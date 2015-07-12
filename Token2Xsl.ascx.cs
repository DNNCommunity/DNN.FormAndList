using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;


namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Token2Xsl Class provides an option to create a XSL rendering file derived
    ///   from a html template for the UserDefinedTable
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Token2Xsl : ModuleUserControlBase
    {
        #region Controls & Constants & Properties

        DataSet _schemaDataSet;

        public enum ErrorOutput
        {
            TokenTemplate,
            XslTranformation,
            Save
        }

        UserDefinedTableController _udtController;

        UserDefinedTableController UdtController
        {
            get { return _udtController ?? (_udtController = new UserDefinedTableController(ModuleContext)); }
        }

        string CurrentListType
        {
            get { return ViewState["CurrentListType"].AsString(ddlListTemplateType.SelectedValue); }
            set { ViewState["CurrentListType"] = value; }
        }

        string ReturnUrl
        {
            get { return ViewState["ReturnUrl"].AsString(ModuleContext.EditUrl("Manage")); }
            set { ViewState["ReturnUrl"] = value; }
        }

        bool IsTrackingEmailMode
        {
            get
            {
                return ViewState["tracking"].AsBoolean(Convert.ToBoolean(Request.QueryString["tracking"].AsString().Length > 0));
            }
            set { ViewState["tracking"] = value; }
        }

        #endregion

        #region Private Methods

        void TemplatesPopulateColumnDropDownLists()
        {
            ddlColumnsForListView.Items.Clear();
            ddlColumnsForDetailView.Items.Clear();
            ddlColumnsForTrackingEmail.Items.Clear();


            ddlColumnsForListView.Items.Add(new ListItem("UDT:EditLink", "[UDT:EditLink]"));
            ddlColumnsForTrackingEmail.Items.Add(new ListItem("UDT:EditLink", "[UDT:EditLink]"));
            if (chkShowDetailView.Checked)
            {
                ddlColumnsForListView.Items.Add(new ListItem("UDT:DetailView", "[UDT:DetailView]"));
            }

            ddlColumnsForDetailView.Items.Add(new ListItem("UDT:EditLink", "[UDT:EditLink]"));
            ddlColumnsForDetailView.Items.Add(new ListItem("UDT:ListView", "[UDT:ListView]"));
            foreach (DataColumn col in _schemaDataSet.Tables[DataSetTableName.Data].Columns)
            {
                var colName = col.ColumnName;
                if (colName != "EditLink")
                {
                    ddlColumnsForListView.Items.Add(new ListItem(colName, string.Format("[{0}]", colName)));
                    ddlColumnsForDetailView.Items.Add(new ListItem(colName, string.Format("[{0}]", colName)));
                    ddlColumnsForTrackingEmail.Items.Add(new ListItem(colName, string.Format("[{0}]", colName)));
                }
            }
            ddlColumnsForListView.Items.Add(new ListItem("[ ] Hard Space", "[ ]"));
            ddlColumnsForDetailView.Items.Add(new ListItem("[ ] Hard Space", "[ ]"));
            ddlColumnsForTrackingEmail.Items.Add(new ListItem("[ ] Hard Space", "[ ]"));
            foreach (var contextString in Enum.GetNames(typeof (XslTemplatingUtilities.ContextValues)))
            {
                ddlColumnsForListView.Items.Add(new ListItem(string.Format("Context:{0}", contextString),
                                                             string.Format("[Context:{0}]", contextString)));
                ddlColumnsForDetailView.Items.Add(new ListItem(string.Format("Context:{0}", contextString),
                                                               string.Format("[Context:{0}]", contextString)));
                ddlColumnsForTrackingEmail.Items.Add(new ListItem(string.Format("Context:{0}", contextString),
                                                                  string.Format("[Context:{0}]", contextString)));
            }

            ddlHeaders.Items.Clear();
            foreach (DataRow row in _schemaDataSet.Tables[DataSetTableName.Fields].Rows)
            {
                var title = row[FieldsTableColumn.Title].ToString();
                ddlHeaders.Items.Add(new ListItem(title, string.Format("[{0}]", title)));
            }
        }

        void TemplatesSetVisibility(bool isViewMode)
        {
            if (isViewMode)
            {
                dshListView.Visible = true;
                dshOptions.Visible = true;
                dshTrackingEmail.Visible = false;
                dvListview.Visible = true;
                dvOptions.Visible = true;
                dvTrackingEmail.Visible = false;

                var showDetails = chkShowDetailView.Checked;
                dshDetailView.Visible = showDetails;
                dvDetailView.Visible = showDetails;
                if (showDetails && txtListTemplate.Text.IndexOf("UDT:DetailView") < 0)
                {
                    txtListTemplate.Text = txtListTemplate.Text.Replace("UDT:EditLink", "UDT:DetailView");
                }
                if (! showDetails)
                {
                    txtListTemplate.Text = txtListTemplate.Text.Replace("UDT:DetailView", txtListTemplate.Text.IndexOf("UDT:EditLink") > 0 
                        ? "" : 
                        "UDT:EditLink");
                }
            }
            else
            {
                dshListView.Visible = false;
                dshOptions.Visible = false;
                dshDetailView.Visible = false;
                dshTrackingEmail.Visible = true;

                dvListview.Visible = false;
                dvOptions.Visible = false;
                dvDetailView.Visible = false;
                dvTrackingEmail.Visible = true;
            }
        }


        IList GetBasicElements()
        {
            var elements = new ArrayList {"[UDT:EditLink]"};
            foreach (DataRow row in _schemaDataSet.Tables[DataSetTableName.Fields].Rows)
            {
                if (Convert.ToBoolean(row[FieldsTableColumn.Visible]))
                {
                    elements.Add(string.Format("[{0}]",
                                               XmlConvert.DecodeName(row[FieldsTableColumn.ValueColumn].ToString())));
                }
            }
            return elements;
        }

        IList GetCurrentElements()
        {
            var txt = txtListTemplate.Text;
            try
            {
                switch (CurrentListType)
                {
                    case "table":
                        var doc1 = new XmlDocument();
                        doc1.LoadXml(txt);
                        var elements = new ArrayList();
                        var cells = doc1.SelectNodes("/tr/td");
                        if (cells != null)
                            foreach (XmlElement node in cells)
                            {
                                elements.Add(node.InnerXml);
                            }
                        return elements;
                    default:

                        if (CurrentListType != "nothing")
                        {
                            var doc = new XmlDocument();
                            doc.LoadXml(txt);
                            var node = doc.SelectSingleNode(string.Format("/{0}", GetOuterTag(CurrentListType)));
                            if (node != null)
                            {
                                txt = node.InnerXml;
                            }
                        }
                        return Regex.Split(txt, GetDelimiter(false));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        string BuildContent(IList elements)
        {
            var listType = ddlListTemplateType.SelectedValue;
            if (elements != null)
            {
                CurrentListType = listType;
                using (var sw = new StringWriter())
                {
                    var delimiter = GetDelimiter();
                    using (var xw = new XmlTextWriter(sw))
                    {
                        var notFirst = false;
                        xw.Formatting = Formatting.Indented;
                        var outerTag = GetOuterTag(ddlListTemplateType.SelectedValue);
                        if (outerTag != string.Empty)
                        {
                            xw.WriteStartElement(outerTag);
                            xw.WriteAttributeString("class", "dnnGridItem");
                        }
                        foreach (string element in elements)
                        {
                            if (GetInnerTag() != string.Empty)
                            {
                                xw.WriteStartElement(GetInnerTag());
                            }
                            if (notFirst)
                            {
                                xw.WriteRaw(delimiter);
                            }
                            else
                            {
                                notFirst = true;
                            }
                            xw.WriteRaw(element);
                            if (GetInnerTag() != string.Empty)
                            {
                                xw.WriteEndElement();
                            }
                        }
                        if (outerTag != string.Empty)
                        {
                            xw.WriteEndElement();
                        }
                        xw.Flush();
                    }

                    return sw.ToString();
                }
            }
            return string.Empty;
        }

        string GetDelimiter(bool notCurrent = true)
        {
            if (notCurrent && ddlListTemplateType.SelectedValue == "table")
            {
                return ("");
            }
            var delimiter = "";
            if (txtListTemplateDelimiter.Text != "")
            {
                delimiter = txtListTemplateDelimiter.Text;
            }
            delimiter += "\r\n";
            return delimiter;
        }

        string GetOuterTag(string listType)
        {
            switch (listType)
            {
                case "table":
                    return "tr";
                case "nothing":
                    return "";
                case "ol":
                case "ul":
                    return "li";
                default:
                    return listType;
            }
        }

        string GetInnerTag()
        {
            switch (ddlListTemplateType.SelectedValue)
            {
                case "table":
                    return "td";
                default:
                    return "";
            }
        }

        void ListTemplateSetHeaderAndFooter()
        {
            switch (ddlListTemplateType.SelectedValue)
            {
                case "table":
                    lblListTemplateHead.Text = @"&lt;table class=""dnnFormItem""&gt;";
                    lblListTemplateFooter.Text = @"...<br/>&lt;/table&gt;";
                    break;
                case "div":
                    lblListTemplateHead.Text = "";
                    lblListTemplateFooter.Text = @"...";
                    break;
                case "ol":
                    lblListTemplateHead.Text = @"&lt;ol&gt;";
                    lblListTemplateFooter.Text = @"...<br/>&lt;/ol&gt;";
                    break;
                case "ul":
                    lblListTemplateHead.Text = @"&lt;ul&gt;";
                    lblListTemplateFooter.Text = @"...<br/>&lt;/ul&gt;";
                    break;
                case "p":
                    lblListTemplateHead.Text = "";
                    lblListTemplateFooter.Text = @"...";
                    break;
                case "nothing":
                    lblListTemplateHead.Text = @"...";
                    lblListTemplateFooter.Text = "";
                    break;
            }
        }

        bool isValid(string x, ErrorOutput pos, bool addRoot)
        {
            if (addRoot)
            {
                x = string.Format("<root>{0}</root>", x);
            }
            using (var rdr = new XmlTextReader(new StringReader(x)))
            {
                try
                {
                    while (rdr.Read())
                    {
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    switch (pos)
                    {
                        case ErrorOutput.TokenTemplate:
                            lblTemplateError.Text = (Localization.GetString("error.Text", LocalResourceFile) +
                                                     ex.Message);
                            lblTemplateError.Visible = true;
                            break;
                        case ErrorOutput.XslTranformation:
                            lblXslScriptError.Text = (Localization.GetString("error.Text", LocalResourceFile) +
                                                      ex.Message);
                            lblXslScriptError.Visible = true;
                            break;
                        case ErrorOutput.Save:
                            lblSaveXslError.Text = (Localization.GetString("error.Text", LocalResourceFile) + ex.Message);
                            lblSaveXslError.Visible = true;
                            break;
                    }
                    return false;
                }
            }
        }

        void setupDelimiter()
        {
            if (ddlListTemplateType.SelectedValue == "table")
            {
                txtListTemplateDelimiter.Enabled = false;
                txtListTemplateDelimiter.BackColor = Color.LightGray;
                addColumnWithTagsToListTemplate.Enabled = true;
            }
            else
            {
                txtListTemplateDelimiter.Enabled = true;
                txtListTemplateDelimiter.BackColor = Color.White;
                addColumnWithTagsToListTemplate.Enabled = false;
            }
        }

        void LockControls(bool isLockRequested)
        {
            if (isLockRequested)
            {
                cmdRebuildContent.Enabled = false;
                cmdSaveFile.Enabled = false;
                cmdGenerateXslt.Enabled = false;
                addColumnToListTemplate.Enabled = false;
                addColumnWithTagsToListTemplate.Enabled = false;
                ddlListTemplateType.Enabled = false;
                ddlColumnsForListView.Enabled = false;
                txtListTemplate.Enabled = false;
                txtFileName.Enabled = false;
                txtXslScript.Enabled = false;
            }
            else
            {
                cmdRebuildContent.Enabled = true;
                cmdSaveFile.Enabled = true;
                cmdGenerateXslt.Enabled = true;
                addColumnToListTemplate.Enabled = true;
                addColumnWithTagsToListTemplate.Enabled = true;
                ddlListTemplateType.Enabled = true;
                ddlColumnsForListView.Enabled = true;
                txtListTemplate.Enabled = true;
                txtFileName.Enabled = true;
                txtXslScript.Enabled = true;
            }
        }

        void ShowXslEditor()
        {
            txtFileName.BackColor = Color.White;
            txtFileName.Enabled = true;
            cmdSaveFile.Enabled = true;
            dshHtml.IsExpanded = false;
            dshXslt.IsExpanded = true;
            dshSave.IsExpanded = true;
            dshDetailView.IsExpanded = false;
            dshOptions.IsExpanded = false;
            dshListView.IsExpanded = false;
        }


        void SetupClientScripts()
        {
            cmdRebuildContent.Attributes.Add("onclick",
                                             string.Format("return confirm(\'{0}\')",
                                                           Localization.GetString("confirmOnRebuild.Text",
                                                                                  LocalResourceFile)));
            addColumnToListTemplate.Attributes.Add("onclick",
                                                   string.Format(
                                                       "AddCurrentItemIntoTextBox(event, \'{0}\', \'{1}\', \'{2}\', \'False\')",
                                                       txtListTemplate.ClientID, ddlColumnsForListView.ClientID,
                                                       txtXslScript.ClientID));
            addColumnWithTagsToListTemplate.Attributes.Add("onclick",
                                                           string.Format(
                                                               "AddCurrentItemIntoTextBox(event, \'{0}\', \'{1}\', \'{2}\', \'True\')",
                                                               txtListTemplate.ClientID, ddlColumnsForListView.ClientID,
                                                               txtXslScript.ClientID));
            addColumnToDetailTemplate.Attributes.Add("onclick",
                                                     string.Format(
                                                         "AddCurrentItemIntoTextBox(event, \'{0}\', \'{1}\', \'{2}\', \'False\')",
                                                         txtDetailTemplate.ClientID, ddlColumnsForDetailView.ClientID,
                                                         txtXslScript.ClientID));
            addColumnToTrackingEmail.Attributes.Add("onclick",
                                                    string.Format(
                                                        "AddCurrentItemIntoTextBox(event, \'{0}\', \'{1}\', \'{2}\', \'False\')",
                                                        txtTrackingEmailTemplate.ClientID,
                                                        ddlColumnsForTrackingEmail.ClientID, txtXslScript.ClientID));
            addHeader.Attributes.Add("onclick",
                                     string.Format(
                                         "AddCurrentItemIntoTextBox(event, \'{0}\', \'{1}\', \'{2}\', \'False\')",
                                         txtHeaderTemplate.ClientID, ddlHeaders.ClientID, txtXslScript.ClientID));
            var url = ResolveUrl("HelpPopup.aspx?resourcekey=Help_HiddenColumns");
            hlpColumns.NavigateUrl = string.Format("javascript:OpenHelpWindow(\'{0}\');", url);
        }

        string TokenTemplateSettingsAsXml()
        {
            using (var strXml = new StringWriter())
            {
                using (var writer = new XmlTextWriter(strXml))
                {
                    writer.WriteStartElement("udt:template");
                    writer.WriteAttributeString("listType", ddlListTemplateType.SelectedValue);
                    writer.WriteAttributeString("delimiter", txtListTemplateDelimiter.Text);
                    writer.WriteAttributeString("listView", txtListTemplate.Text);
                    writer.WriteAttributeString("headerView", txtHeaderTemplate.Text);
                    writer.WriteAttributeString("detailView", txtDetailTemplate.Text);
                    writer.WriteAttributeString("trackingEmail", txtTrackingEmailTemplate.Text);
                    if (chkEnablePaging.Checked)
                    {
                        writer.WriteAttributeString("paging", "true");
                    }
                    if (chkEnableSearch.Checked)
                    {
                        writer.WriteAttributeString("searching", "true");
                    }
                    if (chkEnableSorting.Checked)
                    {
                        writer.WriteAttributeString("sorting", "true");
                    }
                    if (chkShowDetailView.Checked)
                    {
                        writer.WriteAttributeString("showDetailView", "true");
                    }
                    if (IsTrackingEmailMode)
                    {
                        writer.WriteAttributeString("isTrackingMode", "true");
                    }
                    writer.WriteEndElement();
                }

                return strXml.ToString();
            }
        }


        void EditExistingScriptAndTemplates(IFileInfo file)
        {
            ViewState[Definition.XSLFolderName] = file.Folder;
            txtFolderName.Text = file.Folder;
            txtFileName.Text = file.FileName;
            txtFileName.MaxLength =
                Convert.ToInt32(200 -
                                (ModuleContext.PortalSettings.HomeDirectoryMapPath + "/" + file.Folder).Length);
            string fileContent;
            using (var stream = FileManager.Instance.GetFileContent(file))
            {
                using (var x = new StreamReader(stream))
                {
                    fileContent = x.ReadToEnd();
                }
            }

            fileContent = fileContent.Replace(XslTemplatingUtilities.HardSpace,
                                              XslTemplatingUtilities.SpacePlaceholder);
            var doc = new XmlDocument();
            doc.LoadXml(fileContent);
            var nameSpaceManager = new XmlNamespaceManager(doc.NameTable);
            nameSpaceManager.AddNamespace("udt", "DotNetNuke/UserDefinedTable");
            nameSpaceManager.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");
            var scriptRoot = doc.SelectSingleNode("/xsl:stylesheet", nameSpaceManager);
            var templateDefinition =
                // ReSharper disable PossibleNullReferenceException
                (XmlElement) (scriptRoot.SelectSingleNode("udt:template", nameSpaceManager));

            if (templateDefinition != null)
            {
                if (templateDefinition.HasAttribute("headerView"))
                {
                    txtHeaderTemplate.Text =
                        (HttpUtility.HtmlDecode(templateDefinition.Attributes["headerView"].Value).Replace(
                            XslTemplatingUtilities.SpacePlaceholder, XslTemplatingUtilities.HardSpace));
                }

                if (templateDefinition.HasAttribute("listView"))
                {
                    txtListTemplate.Text =
                        (HttpUtility.HtmlDecode(templateDefinition.Attributes["listView"].Value).Replace(
                            XslTemplatingUtilities.SpacePlaceholder, XslTemplatingUtilities.HardSpace));
                }
                if (templateDefinition.HasAttribute("detailView"))
                {
                    txtDetailTemplate.Text =
                        (HttpUtility.HtmlDecode(templateDefinition.Attributes["detailView"].Value).Replace(
                            XslTemplatingUtilities.SpacePlaceholder, XslTemplatingUtilities.HardSpace));
                }
                chkEnablePaging.Checked = templateDefinition.HasAttribute("paging");
                chkEnableSorting.Checked = templateDefinition.HasAttribute("sorting");
                chkEnableSearch.Checked = templateDefinition.HasAttribute("searching");
                chkShowDetailView.Checked = templateDefinition.HasAttribute("showDetailView");
                if (templateDefinition.HasAttribute("isTrackingMode"))
                {
                    IsTrackingEmailMode = true;
                    txtTrackingEmailTemplate.Text =
                        (HttpUtility.HtmlDecode(templateDefinition.Attributes["trackingEmail"].Value).Replace(
                            XslTemplatingUtilities.SpacePlaceholder, XslTemplatingUtilities.HardSpace));
                }

                if (templateDefinition.HasAttribute("listType"))
                {
                    ddlListTemplateType.SelectedValue = templateDefinition.Attributes["listType"].Value;
                    setupDelimiter();
                }
                if (templateDefinition.HasAttribute("delimiter"))
                {
                    txtListTemplateDelimiter.Text =
                        HttpUtility.HtmlDecode(templateDefinition.Attributes["delimiter"].Value);
                }
                scriptRoot.RemoveChild(templateDefinition);
            }
            txtXslScript.Text =
                (XslTemplatingUtilities.PrettyPrint(doc).Replace(XslTemplatingUtilities.SpacePlaceholder,
                                                                 XslTemplatingUtilities.HardSpace));
            ShowXslEditor();
            // ReSharper restore PossibleNullReferenceException
        }

        #endregion

        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            Load += Page_Load;
            cmdRebuildContent.Click += cmdRebuildContent_Click;
            cmdGenerateXslt.Click += cmdGenerateXslt_Click;
            cmdBack.Click += cmdBack_Click;
            cmdSaveFile.Click += cmdSaveFile_Click;
            cmdConfirmOverwriteFile.Click += cmdConfirmOverwriteFile_Click;
            cmdDenyOverwriteFile.Click += cmdDenyOverwriteFile_Click;
            chkShowDetailView.CheckedChanged += chkShowDetailView_CheckedChanged;
            cmdRebuildDetail.Click += cmdRebuildDetail_Click;
            cmdRebuildContent.Click  += cmdRebuildContent_Click;
            cmdRebuildTrackingEmail.Click += cmdRebuildTrackingEmail_Click;
            //hlpColumns.

            ddlListTemplateType.SelectedIndexChanged += ddlListType_SelectedIndexChanged;
        }
        void Page_Load(object sender, EventArgs e)
        {
            _schemaDataSet = UdtController.GetSchemaDataset();
            SetupClientScripts();

            if (! IsPostBack)
            {
                if (Request.QueryString["fileid"].AsString().Length > 0 ||
                    Request.QueryString["Edit"].AsString().ToLowerInvariant() == "current")
                {
                    IFileInfo file=null;
                    if (Request.QueryString["Edit"].AsString().ToLowerInvariant() == "current")
                    {
                        var script = ModuleContext.Settings[SettingName.XslUserDefinedStyleSheet].ToString();
                        if (!string.IsNullOrEmpty( script))
                            file = FileManager.Instance.GetFile(ModuleContext.PortalId,script);
                        ReturnUrl = Globals.NavigateURL();
                    }
                    else
                    {
                        var fileId = int.Parse(Request.QueryString["fileid"]);
                        file = FileManager.Instance.GetFile(fileId);
                        ReturnUrl = ModuleContext.EditUrl("Manage");
                    }
                    if (file != null 
                        && file.Extension.ToLowerInvariant().StartsWith("xsl") 
                        && Utilities.HasWritePermission(file.Folder, ModuleContext.PortalId))
                    {
                        EditExistingScriptAndTemplates(file);
                    }
                    else
                        InitializeNewScript();
                }
                else
                {
                    InitializeNewScript();
                }
                ListTemplateSetHeaderAndFooter();
                TemplatesPopulateColumnDropDownLists();
                TemplatesSetVisibility( ! IsTrackingEmailMode);
            }
        }

        void InitializeNewScript()
        {
            ViewState[Definition.XSLFolderName] = Definition.XSLFolderName;
            if (IsTrackingEmailMode)
            {
                txtTrackingEmailTemplate.Text = string.Concat("<style type=\"text/css\">", "\r\n",
                                                              ".normal, .normalBold {font-family: Verdana, Tahoma, Arial, Helvetica;font-size: 11px;font-weight: normal;}",
                                                              "\r\n", ".normalBold{font-weight: bold;}", "\r\n",
                                                              "</style>", "\r\n",
                                                              XslTemplatingUtilities.GenerateDetailViewTokenText
                                                                  (UdtController.GetSchemaDataset().Tables[
                                                                      DataSetTableName.Fields],
                                                                   includeEditLink: false));
            }
            else
            {
                txtListTemplate.Text = BuildContent(GetBasicElements());
                txtListTemplateDelimiter.Enabled = false;
                txtListTemplateDelimiter.BackColor = Color.LightGray;
                txtDetailTemplate.Text =
                    XslTemplatingUtilities.GenerateDetailViewTokenText(
                        _schemaDataSet.Tables[DataSetTableName.Fields]);
            }
            txtFolderName.Text = Definition.XSLFolderName;
            txtFileName.MaxLength =
                Convert.ToInt32(200 -
                                (ModuleContext.PortalSettings.HomeDirectoryMapPath + "/" +
                                 Definition.XSLFolderName + "/").Length);
        }

        void cmdRebuildContent_Click(object sender, EventArgs e)
        {
            ddlListTemplateType.Enabled = true;
            lblTemplateError.Visible = false;
            lblXslScriptError.Visible = false;
            lblSaveXslError.Visible = false;

            txtListTemplate.Text = BuildContent(GetBasicElements());
            if (chkShowDetailView.Checked && txtListTemplate.Text.IndexOf("UDT:DetailView") < 0)
            {
                txtListTemplate.Text = txtListTemplate.Text.Replace("UDT:EditLink", "UDT:DetailView");
            }
            txtXslScript.Text = "";
            txtFileName.BackColor = Color.LightGray;
            txtFileName.Enabled = false;
            cmdSaveFile.Enabled = false;
        }

        void cmdGenerateXslt_Click(object sender, EventArgs e)
        {
            lblTemplateError.Visible = false;
            lblXslScriptError.Visible = false;
            lblSaveXslError.Visible = false;

            var listTemplate = txtListTemplate.Text;
            var detailTemplate = txtDetailTemplate.Text;
            var headerTemplate = txtHeaderTemplate.Text;
            if (IsTrackingEmailMode)
            {
                if (isValid(txtTrackingEmailTemplate.Text, ErrorOutput.XslTranformation, addRoot: true))
                {
                    txtXslScript.Text =
                        XslTemplatingUtilities.TransformTokenTextToXslScript(UdtController.GetSchemaDataset(),
                                                                             txtTrackingEmailTemplate.Text);
                    txtXslScript.Enabled = true;
                    ShowXslEditor();
                }
            }
            else
            {
                if (isValid(listTemplate, ErrorOutput.XslTranformation, addRoot: true) &&
                    (! chkShowDetailView.Checked || isValid(detailTemplate, ErrorOutput.XslTranformation, addRoot: true)))
                {
                    txtXslScript.Text = XslTemplatingUtilities.TransformTokenTextToXslScript(_schemaDataSet, listTemplate,
                                                                                             detailTemplate,
                                                                                             headerTemplate,
                                                                                             chkEnablePaging.Checked,
                                                                                             chkEnableSorting.Checked,
                                                                                             chkEnableSearch.Checked,
                                                                                             chkShowDetailView.Checked,
                                                                                             CurrentListType);
                    txtXslScript.Enabled = true;
                    ShowXslEditor();
                }
            }
        }

        void cmdBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(ReturnUrl, true);
        }

        void ddlListType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblTemplateError.Visible = false;
            lblXslScriptError.Visible = false;
            lblSaveXslError.Visible = false;
            //content must always be valid XML
            if (isValid(txtListTemplate.Text, ErrorOutput.TokenTemplate, true))
            {
                var elements = GetCurrentElements();
                if (elements != null)
                {
                    txtListTemplate.Text = BuildContent(GetCurrentElements());
                    setupDelimiter();
                }
                else
                {
                    ddlListTemplateType.SelectedValue = CurrentListType;
                    lblTemplateError.Text = Localization.GetString("noTransformButValid", LocalResourceFile);
                    lblTemplateError.Visible = true;
                }
            }
            else
            {
                ddlListTemplateType.SelectedValue = CurrentListType;
            }
            ListTemplateSetHeaderAndFooter();
            txtXslScript.Text = "";
            txtFileName.BackColor = Color.LightGray;
            txtFileName.Enabled = false;
            cmdSaveFile.Enabled = false;
        }

        string GetFileName()
        {
            var fileName = txtFileName.Text.Trim();
            fileName = Globals.CleanFileName(fileName);
            if (! fileName.ToLowerInvariant().EndsWith(".xsl"))
            {
                fileName += ".xsl";
            }
            txtFileName.Text = fileName;
            return fileName;
        }

        string GetFileContent()
        {
            var fileContent = txtXslScript.Text;
            fileContent = fileContent.Replace("</xsl:stylesheet>",
                                              (TokenTemplateSettingsAsXml() + "\r\n" + "</xsl:stylesheet>"));
            return fileContent;
        }


        IFolderInfo GetFolder()
        {
            return Utilities.GetFolder(ModuleContext.PortalSettings,
                                       ViewState[Definition.XSLFolderName].AsString(), Definition.XSLFolderName);
        }


        static bool SaveScript(string fileContent, string fileName, IFolderInfo folder, bool forceOverWrite)
        {
            return Utilities.SaveScript( fileContent, fileName, folder, forceOverWrite);
        }

        void cmdSaveFile_Click(object sender, EventArgs e)
        {
            if (isValid(txtXslScript.Text, ErrorOutput.Save, false)) //XSLTstylesheet must be at least valid XML!
            {
                //get content
                var fileContent = GetFileContent();
                var fileName = GetFileName();

                if (fileName.Length > 4)
                {
                    var folder = GetFolder();

                    if (SaveScript(fileContent, fileName, folder, forceOverWrite: false))
                    {
                        //update setting:
                        UpdateSettings(folder.FolderPath + fileName);
                        Response.Redirect(ReturnUrl, true);
                    }
                    else
                    {
                        LockControls(true);
                        panConfirm.Visible = true;
                    }
                }
            }
        }

        void cmdConfirmOverwriteFile_Click(object sender, EventArgs e)
        {
            var fileContent = GetFileContent();
            var fileName = GetFileName();
            var folder = GetFolder();
            SaveScript(fileContent, fileName, folder, forceOverWrite: true);
            UpdateSettings(folder.FolderPath + fileName);
            Response.Redirect(ReturnUrl, true);
        }

        void UpdateSettings(string fileWithPath)
        {
            var moduleController = new ModuleController();
            if (IsTrackingEmailMode)
            {
                moduleController.UpdateTabModuleSetting(ModuleContext.TabModuleId, SettingName.TrackingScript, fileWithPath);
            }
            else
            {
                moduleController.UpdateTabModuleSetting(ModuleContext.TabModuleId, SettingName.RenderingMethod,
                                              RenderingMethod.UserdefinedXSL);
                moduleController.UpdateTabModuleSetting(ModuleContext.TabModuleId, SettingName.XslUserDefinedStyleSheet,
                                              fileWithPath);
            }
        }

        void cmdDenyOverwriteFile_Click(object sender, EventArgs e)
        {
            txtFileName.Text = "";
            LockControls(false);
            panConfirm.Visible = false;
        }

        //void cmdColumnsHelp_Click(object sender, EventArgs e)
        //{
        //    var url = ResolveUrl("HelpPopup.aspx?resourcekey=Help_HiddenColumns");

        //    var columnsHelpPopup = "<script language=\'text/javascript\'>" + "   window.open(\'" + url + "\', \'\', " +
        //                           "               \'width=800, height=400, location=no, menubar=no, resizable=yes, scrollbars=yes, status=no, toolbar=no\')" +
        //                           "</script>";
        //    Page.ClientScript.RegisterStartupScript(GetType(), "columnsHelpPopup", columnsHelpPopup);
        //}

        void chkShowDetailView_CheckedChanged(object sender, EventArgs e)
        {
            TemplatesSetVisibility(isViewMode: true);
            TemplatesPopulateColumnDropDownLists();
            dshDetailView.IsExpanded = chkShowDetailView.Checked;
        }

        void cmdRebuildDetail_Click(object sender, EventArgs e)
        {
            txtDetailTemplate.Text =
                XslTemplatingUtilities.GenerateDetailViewTokenText(_schemaDataSet.Tables[DataSetTableName.Fields]);
        }

        protected void cmdRebuildTrackingEmail_Click(object sender, EventArgs e)
        {
            txtTrackingEmailTemplate.Text = string.Concat("<style type=\"text/css\">", "\r\n",
                                                          ".normal, .normalBold {font-family: Verdana, Tahoma, Arial, Helvetica;font-size: 11px;font-weight: normal;}",
                                                          "\r\n", ".normalBold{font-weight: bold;}", "\r\n", "</style>",
                                                          "\r\n",
                                                          XslTemplatingUtilities.GenerateDetailViewTokenText(
                                                              UdtController.GetSchemaDataset().Tables[
                                                                  DataSetTableName.Fields], includeEditLink: false));
        }

        #endregion
    }
}