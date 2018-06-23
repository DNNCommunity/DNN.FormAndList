using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using Microsoft.VisualBasic;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The UserDefinedTable Class provides the UI for displaying the UserDefinedTable
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class List : PortalModuleBase, IActionable, IPostBackEventHandler
    {
        #region Search

        class SearchManager
        {
            readonly List _parent;
            readonly PlaceHolder _searchPlaceHolder;
            readonly LinkButton _cmdSearch;
            readonly LinkButton _cmdResetSearch;

            enum SearchControlTypes
            {
                Columns = 1,
                @Operator = 2,
                Search = 3
            }

            public SearchManager(List parent)
            {
                _parent = parent;
                _cmdSearch = parent.cmdSearch;
                _cmdSearch.Click += cmdSearch_Click;
                _cmdResetSearch = parent.cmdResetSearch;
                _cmdResetSearch.Click += cmdResetSearch_Click;
                _searchPlaceHolder = parent.phSearchSentence;
            }

            public bool IsSearching
            {
                get
                {
                    return _parent.ViewState["IsSearching"].AsBoolean() ||
                           ! string.IsNullOrEmpty(_parent.Request.QueryString[string.Format("u{0}q", _parent.ModuleId)]);
                }
                private set { _parent.ViewState["IsSearching"] = value; }
            }

            TextBox TxtSearch
            {
                get { return ((TextBox) (SearchControl(SearchControlTypes.Search))); }
            }

            DropDownList DrpSearchMode
            {
                get { return ((DropDownList) (SearchControl(SearchControlTypes.Operator))); }
            }

            DropDownList DrpSearchableColumns
            {
                get { return ((DropDownList) (SearchControl(SearchControlTypes.Columns))); }
            }

            public string Filter()
            {
                var searchString = string.Empty;

                if (TxtSearch.Text == string.Empty)
                {
                    return string.Empty;
                }
                var dataSet = _parent.DataSet;
                foreach (DataRow row in dataSet.Tables[DataSetTableName.Fields].Rows)
                {
                    if (TxtSearch.Visible && Convert.ToBoolean(row[FieldsTableColumn.Searchable]))
                    {
                        var fieldTitle = row[FieldsTableColumn.Title].ToString();
                        if (DrpSearchableColumns.SelectedValue == "allcolumns" ||
                            DrpSearchableColumns.SelectedValue == fieldTitle)
                        {
                            //add to search expression:
                            if (searchString != string.Empty)
                            {
                                searchString += " OR ";
                            }

                            if (_parent.Settings.UrlSearch  &&
                                dataSet.Tables[DataSetTableName.Data].Columns.Contains(fieldTitle +
                                                                                       DataTableColumn.Appendix_Url))
                            {
                                fieldTitle += DataTableColumn.Appendix_Url;
                            }
                            else if (
                                dataSet.Tables[DataSetTableName.Data].Columns.Contains(fieldTitle +
                                                                                       DataTableColumn.
                                                                                           Appendix_Caption))
                            {
                                fieldTitle += DataTableColumn.Appendix_Caption;
                            }

                            if (dataSet.Tables[DataSetTableName.Data].Columns[fieldTitle].DataType ==
                                typeof (string))
                            {
                                searchString += string.Format("([{0}] Like \'[UDT:Search]\')", fieldTitle);
                            }
                            else
                            {
                                searchString +=
                                    string.Format("(Convert([{0}], \'System.String\') Like \'[UDT:Search]\')",
                                                  fieldTitle);
                            }
                        }
                    }
                }
                var searchpattern = TxtSearch.Text;
                if (DrpSearchMode.Visible)
                {
                    switch (DrpSearchMode.SelectedValue)
                    {
                        case "contain":
                            searchpattern = string.Format("*{0}*", searchpattern);
                            break;
                        case "startwith":
                            searchpattern = string.Format("{0}*", searchpattern);
                            break;
                        case "endwith":
                            searchpattern = string.Format("*{0}", searchpattern);
                            break;
                        case "equal":
                            break;
                    }
                }
                else
                {
                    if (searchpattern.StartsWith("|") && searchpattern.EndsWith("|"))
                    {
                        searchpattern = searchpattern.Substring(1, Convert.ToInt32(searchpattern.Length - 2));
                    }
                    else if (searchpattern.StartsWith("|"))
                    {
                        searchpattern = string.Format("{0}*", searchpattern.Substring(1));
                    }
                    else if (searchpattern.EndsWith("|"))
                    {
                        searchpattern = string.Format("*{0}",
                                                      searchpattern.Substring(0,
                                                                              Convert.ToInt32(searchpattern.Length -
                                                                                              1)));
                    }
                    else
                    {
                        searchpattern = string.Format("*{0}*", searchpattern);
                    }
                }
                return searchString.Replace("[UDT:Search]", EscapeSearchInput(searchpattern));
            }

            static string EscapeSearchInput(string input)
            {
                return input.Replace("\'", "\'\'");
            }

            void Reset()
            {
                TxtSearch.Text = "";
                DrpSearchMode.SelectedValue = "contain";
                DrpSearchableColumns.SelectedIndex = 0;
            }

            public bool DataBindingNeeded()
            {
                var isSearchable = _parent.DataSet.Tables[DataSetTableName.Fields].Rows
                    .Cast<DataRow>()
                    .Any(row => row[FieldsTableColumn.Searchable].AsBoolean());

                if (isSearchable)
                {
                    _parent.panSearch.Visible = true;
                    if (! _parent.Settings.SimpleSearch )
                    {
                        _parent.plSearch.Visible = false;
                    }
                    else
                    {
                        foreach (Control c in _searchPlaceHolder.Controls)
                        {
                            c.Visible = c.ID == "txtSearch";
                        }
                    }
                    if (IsSearching == false &&
                        ! _parent.Settings.ShowNoRecordsUntilSearch )
                    {
                        IsSearching = true;
                        //always set flag to true, if ShowNoRecordsUntilSearch is not checked
                    }
                }

                _parent.cmdResetSearch.Visible =
                    TxtSearch.Text != "" || DrpSearchMode.SelectedValue != "contain";

                //'Show Datagrid if
                //' - Module has no search columns OR
                //' - IsSearching = True OR
                //' - search box is not empty
                return (! isSearchable || IsSearching || TxtSearch.Text != "");
            }

            public void LoadControls()
            {
                _searchPlaceHolder.Controls.Clear();
                const string regexPattern = "(?<=\\{)(?<control>\\d)(?=\\})|(?<text>[^\\{\\}]+)";
                foreach (
                    Match m in
                        new Regex(regexPattern).Matches(Localization.GetString("SearchSentence",
                                                                               _parent.LocalResourceFile)))
                {
                    if (m.Result("${text}").AsString() != string.Empty)
                    {
                        _searchPlaceHolder.Controls.Add(new LiteralControl(m.Result("${text}")));
                    }
                    if (m.Result("${control}").AsString() != string.Empty)
                    {
                        var controlType = (SearchControlTypes) (int.Parse(m.Result("${control}")));
                        SearchControl(controlType);
                    }
                }
                //ensure Search Controls
                SearchControl(SearchControlTypes.Columns);
                SearchControl(SearchControlTypes.Operator);
                SearchControl(SearchControlTypes.Search);
            }

            Control SearchControl(SearchControlTypes controlType)
            {
                var moduleId = _parent.ModuleId;
                switch (controlType)
                {
                    case SearchControlTypes.Columns:
                        var drpSearchableColumns =
                            (DropDownList) (_searchPlaceHolder.FindControl("drpSearchableColumns"));
                        if (drpSearchableColumns == null)
                        {
                            var drp = new DropDownList {ID = "drpSearchableColumns", CssClass = "NormalTextBox"};
                            _searchPlaceHolder.Controls.Add(drp);
                            var lbl = new Label {ID = "lblSearchableColumn"};
                            lbl.Font.Bold = true;
                            lbl.Visible = false;
                            _searchPlaceHolder.Controls.Add(lbl);
                            LoadColumns();
                            drpSearchableColumns = drp;
                            if (! string.IsNullOrEmpty(_parent.Request.QueryString[string.Format("u{0}c", moduleId)]))
                            {
                                drpSearchableColumns.SelectedValue =
                                    _parent.Request.QueryString[string.Format("u{0}c", moduleId)].UrlHexDecode();
                            }
                        }
                        return drpSearchableColumns;
                    case SearchControlTypes.Operator:
                        var drpSearchMode = (DropDownList) (_searchPlaceHolder.FindControl("drpSearchMode"));
                        if (drpSearchMode == null)
                        {
                            var drp = new DropDownList {ID = "drpSearchMode", CssClass = "NormalTextBox"};
                            drp.Items.Add(
                                new ListItem(Localization.GetString("SearchMode.Contain", _parent.LocalResourceFile),
                                             "contain"));
                            drp.Items.Add(
                                new ListItem(Localization.GetString("SearchMode.StartWith", _parent.LocalResourceFile),
                                             "startwith"));
                            drp.Items.Add(
                                new ListItem(Localization.GetString("SearchMode.EndWith", _parent.LocalResourceFile),
                                             "endwith"));
                            drp.Items.Add(
                                new ListItem(Localization.GetString("SearchMode.Equal", _parent.LocalResourceFile),
                                             "equal"));
                            _searchPlaceHolder.Controls.Add(drp);
                            drpSearchMode = drp;
                            if (! string.IsNullOrEmpty(_parent.Request.QueryString[string.Format("u{0}m", moduleId)]))
                            {
                                drpSearchMode.SelectedValue =
                                    _parent.Request.QueryString[string.Format("u{0}m", moduleId)];
                            }
                        }
                        return drpSearchMode;
                    case SearchControlTypes.Search:
                        var txtSearch = (TextBox) (_searchPlaceHolder.FindControl("txtSearch"));
                        if (txtSearch == null)
                        {
                            txtSearch = new TextBox {ID = "txtSearch"};
                            _searchPlaceHolder.Controls.Add(txtSearch);
                            if (! string.IsNullOrEmpty(_parent.Request.QueryString[string.Format("u{0}q", moduleId)]))
                            {
                                txtSearch.Text =
                                    _parent.Request.QueryString[string.Format("u{0}q", moduleId)].UrlHexDecode();
                            }
                        }
                        return txtSearch;
                }
                return null;
            }

            void LoadColumns()
            {
                DrpSearchableColumns.Items.Clear();
                DrpSearchableColumns.Items.Add(
                    new ListItem(Localization.GetString("AllColumns", _parent.LocalResourceFile), "allcolumns"));

                var fieldTitle = string.Empty;
                foreach (DataRow row in _parent.DataSet.Tables[DataSetTableName.Fields].Rows)
                {
                    var searchable = Convert.ToBoolean(row[FieldsTableColumn.Searchable]);
                    if (searchable)
                    {
                        fieldTitle = row[FieldsTableColumn.Title].ToString();
                        if (fieldTitle != "allcolumns")
                        {
                            DrpSearchableColumns.Items.Add(new ListItem(fieldTitle, fieldTitle));
                        }
                    }
                }

                if (DrpSearchableColumns.Items.Count == 2)
                {
                    //display label instead of dropdownlist if there is only one searchable column
                    DrpSearchableColumns.Items.RemoveAt(0);
                    //remove "all searchable columns"
                    DrpSearchableColumns.Visible = false;
                    var lblSearchableColumn = (Label) (_searchPlaceHolder.FindControl("lblSearchableColumn"));
                    lblSearchableColumn.Text = fieldTitle;
                    lblSearchableColumn.Visible = true;
                }
            }

            void cmdSearch_Click(object sender, EventArgs e)
            {
                if (! _parent.RenderMethod.StartsWith("UDT_Xsl"))
                {
                    _parent.CurrentPage = 1;
                    IsSearching = true;
                }
                else
                {
                    var @params = new List<string>();
                    if (_parent.Request.QueryString["show"] == "records")
                    {
                        @params.Add("show/records");
                    }
                    var moduleId = _parent.ModuleId;
                    if (! string.IsNullOrEmpty(TxtSearch.Text))
                    {
                        @params.Add(string.Format("u{0}q={1}", moduleId, TxtSearch.Text.UrlHexEncode()));
                        if (! _parent.Settings.SimpleSearch )
                        {
                            @params.Add(string.Format("u{0}c={1}", moduleId,
                                                      DrpSearchableColumns.SelectedValue.UrlHexEncode()));
                            @params.Add(string.Format("u{0}m={1}", moduleId, DrpSearchMode.SelectedValue));
                        }
                        var url = Globals.NavigateURL(_parent.TabId, "", @params.ToArray());
                        _parent.Response.Redirect(url);
                    }
                    // Such paramter
                }
            }

            void cmdResetSearch_Click(object sender, EventArgs e)
            {
                if (! _parent.RenderMethod.StartsWith("UDT_Xsl"))
                {
                    Reset();
                    _parent.CurrentPage = 1;
                    IsSearching = false;
                }
                else
                {
                    string parameters = _parent.Request.QueryString["show"] == "records" ? "show/records" : "";
                    
                    _parent.Response.Redirect(Globals.NavigateURL(_parent.TabId, "", parameters));
                }
            }
        }

        #endregion

        #region Private Members

        DataSet _dataSet;
        SearchManager _search;
        
        Components.Settings _settings;
        new Components.Settings Settings
        { get { return _settings ?? (_settings = new Components.Settings(ModuleContext.Settings)); } }
        #endregion

        #region Private Properties

        UserDefinedTableController _udtController;

        UserDefinedTableController UdtController
        {
            get { return _udtController ?? (_udtController = new UserDefinedTableController(ModuleContext)); }
        }

        DataSet DataSet
        {
            get { return _dataSet ?? (_dataSet = UdtController.GetDataSet()); }
        }

          string RenderMethod
        {
            get { return string.Format("UDT_{0}", Settings.RenderingMethod ); }
        }

        #region properties Only Needed in Grid Mode

        int CurrentPage
        {
            get
            {
                if (ViewState["CurrentPage"] == null)
                {
                    return 1;
                }
                return Convert.ToInt32(ViewState["CurrentPage"]);
            }
            set { ViewState["CurrentPage"] = value; }
        }

        string SortField
        {
            get
            {
                if (ViewState["SortField"] == null)
                {
                    if (Settings.SortFieldId!=Null.NullInteger  )
                    {
                        ViewState["SortField"] = GetFieldTitle(Settings.SortFieldId );
                    }
                }
                return ViewState["SortField"].AsString();
            }
            set { ViewState["SortField"] = value; }
        }


        string SortOrder
        {
            get
            {
                if (ViewState["SortOrder"] == null)
                {
                    return  Settings.SortOrder;
                }
                return  ViewState["SortOrder"].AsString();
            }
            set { ViewState["SortOrder"] = value; }
        }

        #endregion

        #endregion

        #region Data Binding

        void BindData()
        {
            if (!SchemaIsDefined() )
            {
                ShowModuleMessage("NoFieldsDefined.ErrorMessage", ModuleContext.EditUrl("Manage"), "No fields defined");
            }
            else
            {
                var mustBind = true;
                if (Settings.ShowSearchTextBox)
                {
                    if (! _search.DataBindingNeeded())
                    {
                        ctlPagingControl.TotalRecords = 0;
                        ctlPagingControl.Visible = false;
                        lblNoRecordsFound.Visible = false;
                        mustBind = false;
                    }
                }
                else
                {
                    panSearch.Visible = false;
                }

                if (mustBind)
                {
                    switch (RenderMethod)
                    {
                        case SettingName.XslUserDefinedStyleSheet:
                            var styleSheet = (string) ModuleContext.Settings[RenderMethod];
                            if (string.IsNullOrEmpty( styleSheet ))
                            {
                                BindDataToDataGrid();
                            }
                            else
                            BindDataUsingXslTransform(styleSheet);
                            break;
                        case SettingName.XslPreDefinedStyleSheet:
                            
                            var oldStyleSheet = (string) ModuleContext.Settings[RenderMethod];
                            var newStyleSheet = CopyScriptToPortal(oldStyleSheet);

                            var mc = new ModuleController();
                            mc.UpdateTabModuleSetting(TabModuleId, SettingName.RenderingMethod, "XslUserDefinedStyleSheet");
                            mc.UpdateTabModuleSetting(TabModuleId,SettingName.XslUserDefinedStyleSheet , newStyleSheet );
                            
                            BindDataUsingXslTransform(newStyleSheet);
                           
                            break;
                        default: //grid rendering
                            BindDataToDataGrid();
                            break;
                    }
                }
            }
        }

        string CopyScriptToPortal(string styleSheet)
        {
            var path = MapPath(styleSheet);
            var filename = Path.GetFileName(path);
            
            var folderManager = FolderManager.Instance;
            if (!folderManager.FolderExists(PortalId, "XslStyleSheets"))
                folderManager.AddFolder(PortalId, "XslStyleSheets");

            const string legacyPath = "XslStyleSheets/UDT Legacy";
            
            var folderInfo = folderManager.FolderExists(PortalId, legacyPath)?
                folderManager.GetFolder(PortalId, legacyPath) :
                folderManager.AddFolder(PortalId, legacyPath);

            using (var source= new FileStream(path,FileMode.Open ))
            {
                var fileInfo = !FileManager.Instance.FileExists(folderInfo, filename) 
                    ? FileManager.Instance.AddFile(folderInfo, filename, source) 
                    : FileManager.Instance.GetFile(folderInfo, filename);
                return string.Format("{0}/{1}", legacyPath, fileInfo.FileName);
            }
        }

        void ShowModuleMessage(string keyForLocalization, string parameter, string fallBackMessage)
        {
            //Dummy text
            var strMessageFormat = Localization.GetString(keyForLocalization, LocalResourceFile).AsString();
            if (strMessageFormat != string.Empty)
            {
                fallBackMessage = string.Format(strMessageFormat, parameter);
            }
            ShowModuleMessage(fallBackMessage);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   BindDataToDataGrid fetchs the data from the database and binds it to the grid
        /// </summary>
        /// -----------------------------------------------------------------------------
        void BindDataToDataGrid()
        {
            PopulateDataGridColumns();

            var dataTable = DataSet.Tables[DataSetTableName.Data];

            dataTable
                .FilterAndSort( GetRowFilter(Settings.Filter, _search.Filter()), SortField, SortOrder)
                .Top(Settings.TopCount);
            
            //Paging
            var paging = new
                {
                    PageSize = Settings.Paging,
                    AllowPaging = Settings.Paging != Null.NullInteger,
                    PageIndex = Convert.ToInt32(
                                Math.Min(CurrentPage, Math.Ceiling(((double)dataTable.Rows.Count / Settings.Paging))) - 1),
                    TotalRecords = dataTable.Rows.Count 
                };

            dataTable.Page( paging.PageIndex, paging.PageSize);


            using (var data = new DataView(dataTable))
            {
                  if (data.Count > 0) 
                {
                    lblNoRecordsFound.Visible = false;
         
                    try
                    {
                        grdData.DataSource = data;
                        grdData.DataBind();
                    }
                    catch (FormatException e)
                    {
                        HandleException(e, "Databind Exception");
                    }
                    grdData.Visible = true;
                    ctlPagingControl.Visible = paging.AllowPaging;
                    ctlPagingControl.PageSize = paging.PageSize;
                    ctlPagingControl.CurrentPage = paging.PageIndex + 1;
                    ctlPagingControl.TotalRecords = paging.TotalRecords;
                }
                else
                {
                    if (_search.IsSearching)
                    {
                        lblNoRecordsFound.Visible = true;
                    }
                    ctlPagingControl.TotalRecords = 0;
                    ctlPagingControl.Visible = false;
                }
            }
        }


        PortalModuleBase GetModuleControl()
        {
            return (PortalModuleBase) (((Parent.Parent ) is PortalModuleBase) ? Parent.Parent  : this);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   set up Fielddefinitions for Datagrid
        /// </summary>
        /// -----------------------------------------------------------------------------
        void PopulateDataGridColumns()
        {
            for (var intColumn = grdData.Columns.Count - 1; intColumn >= 1; intColumn--)
            {
                grdData.Columns.RemoveAt(intColumn);
            }
            foreach (DataRow row in DataSet.Tables[DataSetTableName.Fields].Rows)
            {
                //Add Fields dynamically to the grdData
                var colField = new BoundField
                                   {
                                       HeaderText = row[FieldsTableColumn.Title].ToString(),
                                       DataField = row[FieldsTableColumn.Title].ToString(),
                                       HtmlEncode = false,
                                       Visible = Convert.ToBoolean(row[FieldsTableColumn.Visible]),
                                       SortExpression = string.Format("{0}|ASC", row[FieldsTableColumn.Title])
                                   };
              //Add a sorting indicator to the headertext
              if (    row[FieldsTableColumn.Title].ToString() == SortField && colField.Visible)
               {
                    if (SortOrder == "ASC")
                    {
                        colField.HeaderText +=
                            string.Format(
                                "<img src=\"{0}/images/sortascending.gif\" border=\"0\" alt=\"Sorted By {1} In Ascending Order\"/>",
                                (Request.ApplicationPath == "/" ? string.Empty : Request.ApplicationPath), SortField);
                    }
                    else
                    {
                        colField.HeaderText +=
                            string.Format(
                                "<img src=\"{0}/images/sortdescending.gif\" border=\"0\" alt=\"Sorted By {1} In Descending Order\"/>",
                                (Request.ApplicationPath == "/" ? string.Empty : Request.ApplicationPath), SortField);
                    }
                }
                //Column settings depending on the fieldtype
                DataType.ByName(row[FieldsTableColumn.Type].ToString()).
                    SetStylesAndFormats(colField,row[FieldsTableColumn.OutputSettings].AsString());
                grdData.Columns.Add(colField);
            }
            var editLinkNeed = DataSet.Tables[DataSetTableName.Data].Rows
                .Cast<DataRow>()
                .Any(row => ! string.IsNullOrEmpty(row[DataTableColumn.EditLink] as string));
            if (! editLinkNeed)
            {
                grdData.Columns.RemoveAt(0);
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   BindDataUsingXslTransform fetchs the data from the database and binds it to the grid
        /// </summary>
        /// -----------------------------------------------------------------------------
        void BindDataUsingXslTransform(string styleSheet)
        {
            try
            {
                DataSet.Tables.Add(UdtController.Context("",
                    Settings.SortFieldId.ToString(),
                    Settings.SortOrder ,
                    Settings.Paging.ToString()));

                DataSet.Tables[DataSetTableName.Data]
                    .FilterAndSort(GetRowFilter(Settings.Filter, _search.Filter()), SortField, SortOrder)
                    .Top(Settings.TopCount);
              
                var xslTrans = new XslCompiledTransform();

                var script = Utilities.ReadStringFromFile(styleSheet, PortalId);
                var reader = new XmlTextReader(new StringReader(script));
                xslTrans.Load(reader);

                using (XmlReader xmlData = new XmlTextReader(new StringReader(DataSet.GetXml())))
                {
                    using (var stringWriter = new StringWriter())
                    {
                        //Dynamic UDT Params. Add all Request parameters that starts with "UDT_{moduleid}_Param*" as "Param*" named XSL parameter
                        var args = new XsltArgumentList();
                        var udtParameterPrefix = string.Format(Definition.QueryStringParameter,ModuleContext.ModuleId);
                        foreach (string paramKey in Request.Params.Keys)
                        {
                            if (paramKey != null && paramKey.ToLowerInvariant().StartsWith(udtParameterPrefix))
                            {
                                args.AddParam(paramKey.ToLowerInvariant().Substring(udtParameterPrefix.Length - 5),
                                              string.Empty, Request[paramKey]);
                            }
                        }
                        xslTrans.Transform(xmlData, args, stringWriter);
                        XslOutput.Controls.Add(new LiteralControl(stringWriter.ToString()));
                    }
                }

                XslOutput.Visible = true;
            }
            catch (Exception exc)
            {
                HandleException(exc,
                                string.Format("{0}<br/>StyleSheet:{1}",
                                              Localization.GetString("XslFailed.ErrorMessage", LocalResourceFile),
                                              styleSheet));
                BindDataToDataGrid();
            }
        }

        void HandleException(Exception exc, string localizedMessage)
        {
            var message = new PortalSecurity().InputFilter(exc.Message, PortalSecurity.FilterFlag.NoScripting);
            message = string.Format("{0}<br/>Error Description: {1}", localizedMessage, message);
            ShowModuleMessage(message);
            Exceptions.LogException(exc);
        }

        void ShowModuleMessage(string message)
        {
            var moduleControl = GetModuleControl();
            var modSecurity = new ModuleSecurity(ModuleContext);
            if (modSecurity.IsAllowedToAdministrateModule())
            {
                 UI.Skins.Skin.AddModuleMessage(moduleControl, message,ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }


       

        #endregion

        #region Private Subs and Functions

        string GetRowFilter(string filter, string search)
        {
            var tokenReplace = new TokenReplace(escapeApostrophe: true) {ModuleInfo = ModuleContext.Configuration};
            if (filter != string.Empty)
            {
                filter = tokenReplace.ReplaceEnvironmentTokens(filter);
            }
            if (filter != string.Empty && search != string.Empty)
            {
                return string.Format("{0} AND ({1})", filter, search);
            }
            return filter + search;
        }

        string GetFieldTitle(int fieldId)
        {
            using (
                var dv = new DataView(DataSet.Tables[DataSetTableName.Fields])
                             {RowFilter = string.Format("{0}={1}", FieldsTableColumn.Id, fieldId)})
            {
                if (dv.Count > 0)
                {
                    return (string) (dv[0][FieldsTableColumn.Title]);
                }
                return "";
            }
        }


        bool IsKnownField(string fieldName)
        {
            var fieldstable = DataSet.Tables[DataSetTableName.Fields];
            fieldName = fieldName.Replace("\'", "\'\'");
            return fieldstable.Select(string.Format("{0}=\'{1}\'", FieldsTableColumn.Title, fieldName)).Length >= 0;
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e); 
            ModuleContext.HelpURL = string.Format("http://www.dotnetnuke.com/{0}?tabid=457", Globals.glbDefaultPage);

            Load += Page_Load;
            PreRender += Page_PreRender;
            ctlPagingControl.PageChanged += ctlPagingControl_CurrentPageChanged;
            grdData.Sorting += grdData_Sorting;

            try
            {
                _search = new SearchManager(this);
                _search.LoadControls();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (! IsPostBack)
                {
                    CurrentPage = 1;
                    EnsureActionButton();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ctlPagingControl_CurrentPageChanged(object sender, EventArgs e)
        {
            CurrentPage = ctlPagingControl.CurrentPage;
        }

        protected void grdData_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                var strSort = e.SortExpression.Split('|');

                var newSortField = strSort[0];
                if (IsKnownField(newSortField))
                {
                    if ((newSortField == SortField && SortOrder == "ASC") ||
                        (newSortField != SortField && strSort[1] == "DESC"))
                    {
                        SortOrder = "DESC";
                    }
                    else
                    {
                        SortOrder = "ASC";
                    }
                    SortField = newSortField;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                BindData();
            }
            catch (Exception exc)
            {
                HandleException(exc, Localization.GetString("DataBindingFailed.ErrorMessage", LocalResourceFile));
            }
        }

        #endregion

        #region Optional Interfaces

        public void EnsureActionButton()
        {

            var modSecurity = new ModuleSecurity(ModuleContext);
            var useButtons = Settings.UseButtonsInForm ;

            if (Settings.OnlyFormIsShown )
            {
                var url = Globals.NavigateURL(ModuleContext.TabId);
                var title = Localization.GetString("BackToForm.Action", LocalResourceFile);

                ActionLink.NavigateUrl = url;
                ActionLink.Text = title;
                placeholderActions.Visible = useButtons;
            }
            else if (Settings.OnlyListIsShown && modSecurity.IsAllowedToAddRow() && SchemaIsDefined() &&
                     (modSecurity.IsAllowedToAdministrateModule() || HasAddPermissonByQuota()))
            {
                var url = ModuleContext.EditUrl();
                var title = Localization.GetString(ModuleActionType.AddContent, LocalResourceFile);

                ActionLink.NavigateUrl = url;
                ActionLink.Text = title;
                placeholderActions.Visible = useButtons;
            }
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                try
                {
                    var modSecurity = new ModuleSecurity(ModuleContext);
                    var useButtons = Settings.UseButtonsInForm ;
                    var cmdName = useButtons ? "": ModuleActionType.AddContent;
                    
                    try
                    {
                        if (Settings.OnlyFormIsShown )
                        {
                            var url = Globals.NavigateURL(ModuleContext.TabId);
                            var title = Localization.GetString("BackToForm.Action", LocalResourceFile);
                            actions.Add(ModuleContext.GetNextActionID(),
                                        title,
                                        cmdName, "", Utilities.IconURL("Lt"),
                                        url, false, SecurityAccessLevel.View, true,
                                        false);
                           
                        }
                        else if (Settings.OnlyListIsShown  && modSecurity.IsAllowedToAddRow() && SchemaIsDefined() &&
                                 (modSecurity.IsAllowedToAdministrateModule() || HasAddPermissonByQuota()))
                        {
                            var url = ModuleContext.EditUrl();
                            var title = Localization.GetString(ModuleActionType.AddContent, LocalResourceFile);
                            actions.Add(ModuleContext.GetNextActionID(),
                                        title,
                                        cmdName , "",Utilities.IconURL("Add"), url,
                                        false, SecurityAccessLevel.View, true, false);
                       }
                      
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
// ReSharper restore EmptyGeneralCatchClause
                    {
                        // This try/catch is to avoid loosing control about your current UDT module - if an error happens inside GetDatSet, it will be raised and handled again inside databind
                    }

                    actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("ShowXML.Action", LocalResourceFile), "", "",
                                ResolveUrl("~/images/XML.gif"),
                                (ResolveUrl(string.Format("~{0}ShowXml.ashx", Definition.PathOfModule)) + "?tabid=" +
                                 ModuleContext.TabId + "&mid=" + ModuleContext.ModuleId), false,
                                SecurityAccessLevel.Edit, true, true);
                    //Add 'DeleteAll' command:
                    if (DataSet.Tables[DataSetTableName.Data].Rows.Count > 0)
                    {
                        var urlDelete = Page.ClientScript.GetPostBackEventReference(this, "DeleteAll");
                        urlDelete = string.Format("javascript:if (confirm(\'{0}\')) {1}",
                                                  Localization.GetString("DeleteAll.Confirm", LocalResourceFile).AsString().Replace(
                                                                                             "\'", "\\\'"), urlDelete);
                        actions.Add(ModuleContext.GetNextActionID(),
                                    Localization.GetString("DeleteAll.Action", LocalResourceFile), "", "",
                                    Utilities.IconURL("Delete"), urlDelete, false, SecurityAccessLevel.Edit, true,
                                    false);
                    }

                    if (RenderMethod  == SettingName.XslUserDefinedStyleSheet)
                    {
                        actions.Add(ModuleContext.GetNextActionID(),
                                    Localization.GetString("EditXsl.Action", LocalResourceFile), "", "",
                                    Utilities.IconURL("Wizard"),
                                    ModuleContext.EditUrl("Edit", "Current", "GenerateXsl"), false,
                                    SecurityAccessLevel.Edit,
                                    true, false);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
                return actions;
            }
        }

       

        bool SchemaIsDefined()
        {
            return DataSet.Tables[DataSetTableName.Fields].Rows.Count > 4;
        }

        bool HasAddPermissonByQuota()
        {
            return ModuleSecurity.HasAddPermissonByQuota(DataSet.Tables[DataSetTableName.Fields],
                                                         DataSet.Tables[DataSetTableName.Data],
                                                         Settings.UserRecordQuota ,
                                                         ModuleContext.PortalSettings.UserInfo.GetSafeUsername());
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            var modSecurity = new ModuleSecurity(ModuleContext);
            if (eventArgument == "DeleteAll" && modSecurity.IsAllowedToAdministrateModule())
            {
                UdtController.DeleteRows();
                Response.Redirect(Globals.NavigateURL(ModuleContext.TabId), true);
            }
        }

        #endregion
    }
}