using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Modules.UserDefinedTable.Templates;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Editor for Module Templates
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class TemplateList : PortalModuleBase, IActionable, IPostBackEventHandler
    {
        readonly List<TemplateValueInfo> _customizations = new List<TemplateValueInfo>();

        #region Optional Interfaces

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                try
                {
                    actions.Add(ModuleContext.GetNextActionID(), Localization.GetString("Rescan", LocalResourceFile), "",
                                "", Utilities.IconURL("Refresh"),
                                string.Format("javascript:{0}",
                                              Page.ClientScript.GetPostBackEventReference(this, "Rescan")), false,
                                SecurityAccessLevel.Edit, true, false);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
                return actions;
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            if (eventArgument == "Rescan")
            {
                TemplateController.ClearCache();
                Response.Redirect(Globals.NavigateURL(ModuleContext.TabId), true);
            }
        }

        #endregion

        #region Events

        protected override void OnInit(EventArgs e)
        {
            Load+=Page_Load;
            GridView1.SelectedIndexChanged += GridView1_SelectedIndexChanged;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           
            cmdConfig.NavigateUrl =  ModuleContext.EditUrl("Manage");
            if (! IsPostBack)
            {
                Localization.LocalizeGridView(ref GridView1, LocalResourceFile);
            }
            BindData();
            if (GridView1.Rows.Count == 0)
            {
                lblTemplate.Visible = false;
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
            if (_customizations.Count > 0)
            {
                GridView1.Visible = false;
                lblCustomizeTemplate.Text = string.Format(Localization.GetString("CustomizeTemplate", LocalResourceFile),
                                                 GridView1.SelectedRow.Cells[1].Text,
                                                 GridView1.SelectedRow.Cells[2].Text);

                cmdApply.Visible = true;
            }
            else
            {
                TemplateController.LoadTemplate(HttpUtility.HtmlDecode(GridView1.SelectedRow.Cells[1].Text),
                                                ModuleContext.PortalId, ModuleContext.TabId);
                DeleteMe();
            }
        }

        protected void cmdApply_Click(object sender, EventArgs e)
        {
            XmlDocument doc = null;
            foreach (var customizedValue in _customizations)
            {
                var parentNode = customizedValue.Node.ParentNode;
                // ReSharper disable PossibleNullReferenceException
                if (customizedValue.ValueSource != string.Empty)
                {
                    ((XmlElement) parentNode).SetAttribute(customizedValue.ValueSource, customizedValue.Value);
                    ((XmlElement) parentNode).SetAttribute("installmode", "force");
                }
                else
                {
                    parentNode.InnerText = customizedValue.Value;
                }
                // ReSharper restore PossibleNullReferenceException
                doc = customizedValue.Node.OwnerDocument;
            }

            TemplateController.LoadTemplate(doc, ModuleContext.PortalId, ModuleContext.TabId);
            DeleteMe();
        }

        #endregion

        #region Private functions

        void DeleteMe()
        {
            var m = new ModuleController();
            m.DeleteTabModule(ModuleContext.TabId, ModuleContext.ModuleId, false);
            m.DeleteModule(ModuleContext.ModuleId);
            Response.Redirect(Globals.NavigateURL(ModuleContext.TabId), true);
        }

        void LoadCustomization()
        {
            // Customizations.Clear()

            var doc = new XmlDocument();
            doc.LoadXml(
                (TemplateController.Templates[HttpUtility.HtmlDecode((GridView1.SelectedRow.Cells[1].Text))].
                    ExportContent));

            var lists = new ListController();

            var xmlnsManager = new XmlNamespaceManager(doc.NameTable);
            xmlnsManager.AddNamespace("ask", "DotNetNuke/ModuleTemplate");

            foreach (XmlElement node in doc.SelectNodes("//ask:user", xmlnsManager))
            {
                var vsource = node.GetAttribute("valuesource");

                string value = vsource != string.Empty 
                                   ? ((XmlElement) node.ParentNode).GetAttribute(vsource) 
                                   : node.ParentNode.InnerText.Trim();
                var editor = node.GetAttribute("editor").AsString("Text");
                var length = int.Parse(node.GetAttribute("length").AsString("255"));
                var editorId = lists.GetListEntryInfo("DataType", editor).EntryID;

                var caption = node.GetAttribute("caption").AsString(vsource.AsString(node.ParentNode.Name));

                _customizations.Add(new TemplateValueInfo
                                       {
                                           Caption = caption,
                                           Editor = editorId,
                                           Node = node,
                                           Value = value,
                                           ValueSource = vsource,
                                           Length = length
                                       });
            }
        }

        void BindData()
        {
            if (GridView1.SelectedRow != null)
            {
                LoadCustomization();
                TemplateCustomValuesEditor.DataSource = _customizations;
                TemplateCustomValuesEditor.DataBind();
            }
        }

        #endregion
    }
}