<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Control Language="C#" Inherits="DotNetNuke.Modules.UserDefinedTable.Token2Xsl"
    TargetSchema="http://schemas.microsoft.com/intellisense/ie5" Codebehind="Token2Xsl.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%-- Custom JavaScript Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.js" Priority="101" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/xml/xml.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/javascript/javascript.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/css/css.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/htmlmixed/htmlmixed.js" Priority="103" />


<script type="text/javascript">
	function OpenHelpWindow(url)
	{
		window.open(url, '',  'width=800, height=800, location=no, menubar=no, resizable=yes, scrollbars=yes, status=no, toolbar=no');
		window.event.returnValue = false;
	}
	
	function AddCurrentItemIntoTextBox(e, target, source, clear, tags) {
		var ctrlTarget = document.getElementById(target);
		//IE support
	    
	    if (document.selection) {
			ctrlTarget.focus();
			var sel = document.selection.createRange();
			if (tags == "True") {
				sel.text = "<td>" + document.getElementById(source).value + "</td>";
			} else {
				sel.text = document.getElementById(source).value;
			}
			ctrlTarget.focus();
	        var ctrlToClear = document.getElementById(clear);
	        if (ctrlToClear != null) ctrlToClear.innerHTML = "";
			window.event.returnValue = false;
		}
		//MOZILLA/NETSCAPE support
		else if (ctrlTarget.selectionStart || ctrlTarget.selectionStart == 0) {
			var startPos = ctrlTarget.selectionStart;
			var endPos = ctrlTarget.selectionEnd;
			if (tags == "True") {
				ctrlTarget.value = ctrlTarget.value.substring(0, startPos)
						+ "<td>"
						+ document.getElementById(source).value
						+ "</td>"
						+ ctrlTarget.value.substring(endPos, ctrlTarget.value.length);
			} else {
				ctrlTarget.value = ctrlTarget.value.substring(0, startPos)
						+ document.getElementById(source).value
						+ ctrlTarget.value.substring(endPos, ctrlTarget.value.length);
			}
	        ctrlToClear = document.getElementById(clear);
	        if (ctrlToClear != null) ctrlToClear.innerHTML = "";
			e.preventDefault();
		} else {
			ctrlTarget.value += document.getElementById(source).value;
	        ctrlToClear = document.getElementById(clear);
	        if (ctrlToClear != null) ctrlToClear.innerHTML = "";
			window.event.returnValue = false;
		}
	}

      jQuery(function ($) {
        var mimeType =  "text/xml";

        var setupModule = function () {
            CodeMirror.fromTextArea($("textarea[id$='<%= txtXslScript.ClientID %>']")[0], {
                lineNumbers: true,
                matchBrackets: true,
                lineWrapping: true,
                mode: mimeType
            });

        };

        setupModule();

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {

            // note that this will fire when _any_ UpdatePanel is triggered,
            // which may or may not cause an issue
            setupModule();

        });
    });
</script>
<style>
td > .dnnLabel {
    width: auto;
}
</style>

<div align="center" style="width: 80%">
    <div align="right">
        <asp:HyperLink ID="hlpColumns" runat="server" resourcekey="cmdColumnsHelp" NavigateUrl="#"
            CssClass="CommandButton" />
    </div>
    <div align="left">
        <dnn:SectionHead ID="dshHtml" IsExpanded="true" Section="tblHtml" IncludeRule="True"
            ResourceKey="dshHtml" runat="server" CssClass="Head" align="left" />
        <div id="tblHtml" runat="server" align="left">
            <dnn:SectionHead ID="dshListView" IsExpanded="true" Section="dvListView" IncludeRule="false"
                ResourceKey="dshListView" runat="server" CssClass="SubHead" />
            <div id="dvListview" runat="server" style="padding-bottom: 20px; padding-top: 10px">
                <table>
                    <tr>
                        <td class="SubHead" valign="top">
                            <dnn:Label ID="plListType" runat="server" Text="List type" ControlName="ddlListType"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlListTemplateType" runat="server" CssClass="Normal" BorderWidth="1px"
                                BorderColor="Lightgray" BorderStyle="Solid" AutoPostBack="True" >
                                <asp:ListItem Value="table" Selected="true" resourcekey="table" />
                                <asp:ListItem Value="div" resourcekey="div" />
                                <asp:ListItem Value="p" resourcekey="p" />
                                <asp:ListItem Value="ol" resourcekey="ol" />
                                <asp:ListItem Value="ul" resourcekey="ul" />
                                <asp:ListItem Value="nothing" resourcekey="nothing" />
                            </asp:DropDownList>
                        </td>
                        <td style="vertical-align:top" colspan="2">
                            <asp:LinkButton ID="cmdRebuildContent" resourcekey="cmdRebuildContent" runat="server"
                                CssClass="CommandButton" />
                        </td>
                    </tr>
                    <tr>
                        <td class="SubHead">
                            <dnn:Label ID="plDelimiter" runat="server" Text="Delimiter symbol" ControlName="txtDelimiter"
                                Suffix=":" />
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtListTemplateDelimiter" runat="server" CssClass="Normal" Columns="10">;</asp:TextBox>
                        </td>
                    </tr>
                    
                </table>
                <asp:Label ID="lblTemplateError" runat="server" CssClass="normalred" Visible="False"></asp:Label><br />
                <asp:Label ID="lblListTemplateHead" runat="server" CssClass="Normal" />
                <table>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="plHeaderList" runat="server" ResourceKey="plHeaderList" ControlName="ddlHeaders"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlHeaders" runat="server" CssClass="Normal" />
                        </td>
                        <td>
                            <asp:HyperLink ID="addHeader" resourcekey="cmdInsert" runat="server"
                                CssClass="CommandButton" NavigateUrl="#" />
                        </td>
                    </tr>
                </table>
                
                <asp:TextBox ID="txtHeaderTemplate" Style="font-family: consolas, courier" runat="server"
                    CssClass="Normal" Columns="70" Rows="3" TextMode="MultiLine" Width="100%" />
                <table>
                <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="plColumns" runat="server" Text="Available Columns" ControlName="ddlColumns"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlColumnsForListView" runat="server" CssClass="Normal" />
                        </td>
                        <td>
                            <asp:HyperLink ID="addColumnToListTemplate" resourcekey="cmdInsert" runat="server"
                                CssClass="CommandButton" NavigateUrl="#" />
                        </td>
                        <td>
                            <asp:HyperLink ID="addColumnWithTagsToListTemplate" resourcekey="cmdAddColumnWithTags"
                                runat="server" CssClass="CommandButton" NavigateUrl="#" />
                        </td>
                    </tr></table>
                <asp:TextBox ID="txtListTemplate" Style="font-family: consolas, courier" runat="server"
                    CssClass="Normal" Columns="70" Rows="15" TextMode="MultiLine" Width="100%" />
                <asp:Label ID="lblListTemplateFooter" runat="server" CssClass="Normal" />
            </div>
            <dnn:SectionHead ID="dshDetailView" IsExpanded="false" Section="dvDetailView" IncludeRule="false"
                ResourceKey="dshDetailView" runat="server" CssClass="SubHead" />
            <div id="dvDetailView" runat="server" style="padding-bottom: 20px; padding-top: 10px">
                <table>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="plColumns2" runat="server" ResourceKey="plColumns" ControlName="ddlColumnsForDetailView"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlColumnsForDetailView" runat="server" CssClass="Normal" />
                        </td>
                        <td>
                            <asp:HyperLink ID="addColumnToDetailTemplate" resourcekey="cmdInsert" runat="server"
                                CssClass="CommandButton" NavigateUrl="#" />
                        </td>
                        <td>
                            <asp:LinkButton ID="cmdRebuildDetail" resourcekey="cmdRebuildContent" runat="server"
                                CssClass="CommandButton"  />
                        </td>
                    </tr>
                </table>
                <asp:TextBox ID="txtDetailTemplate" Style="font-family: consolas, courier" runat="server"
                    CssClass="Normal" Columns="70" Rows="15" TextMode="MultiLine" Width="100%" />
            </div>
            <dnn:SectionHead ID="dshTrackingEmail" IsExpanded="true" Section="dvTrackingEmail"
                IncludeRule="false" ResourceKey="dshTrackingEmail" runat="server" CssClass="SubHead"
                Visible="false" />
            <div id="dvTrackingEmail" runat="server" style="padding-bottom: 20px; padding-top: 10px"
                visible="false">
                <table>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="plColumns3" runat="server" ResourceKey="plColumns" ControlName="ddlColumnsForTrackingEmail"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlColumnsForTrackingEmail" runat="server" CssClass="Normal" />
                        </td>
                        <td>
                            <asp:HyperLink ID="addColumnToTrackingEmail" resourcekey="cmdInsert" runat="server"
                                CssClass="CommandButton" NavigateUrl="#" />
                        </td>
                        <td>
                            <asp:LinkButton ID="cmdRebuildTrackingEmail" resourcekey="cmdRebuildContent" runat="server"
                                CssClass="CommandButton"  />
                        </td>
                    </tr>
                </table>
                <asp:TextBox ID="txtTrackingEmailTemplate" Style="font-family: consolas, courier" runat="server"
                    CssClass="Normal" Columns="70" Rows="15" TextMode="MultiLine" Width="100%" />
            </div>
            <dnn:SectionHead ID="dshOptions" IsExpanded="false" Section="dvOptions" IncludeRule="false"
                ResourceKey="dshOptions" runat="server" CssClass="SubHead" />
            <div id="dvOptions" runat="server" style="padding-bottom: 20px; padding-top: 20px">
                <table>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="lblEnableSorting" runat="server" ControlName="chkEnableSorting" ResourceKey="lblEnableSorting"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:CheckBox ID="chkEnableSorting" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="lblEnablePaging" runat="server" ControlName="chkEnablePaging" ResourceKey="lblEnablePaging"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:CheckBox ID="chkEnablePaging" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="lblEnableSearch" runat="server" ControlName="chkEnableSearch" ResourceKey="lblEnableSearch"
                                Suffix=":" />
                        </td>
                        <td>
                            <asp:CheckBox ID="chkEnableSearch" runat="server" /> <asp:Label runat=server ID="lblSearchIsObsolete"  text="(Obsolete)" CssClass="normalRed" />
                        </td>
                    </tr>
                    <tr>
                        <td class="SubHead" style="white-space: nowrap;">
                            <dnn:Label ID="lblShowDetailView" runat="server" ControlName="chkShowDetailView"
                                ResourceKey="lblShowDetailView" Suffix=":" />
                        </td>
                        <td>
                            <asp:CheckBox ID="chkShowDetailView" runat="server" AutoPostBack="True"  />
                        </td>
                    </tr>
                </table>
            </div>
            <asp:Label ID="lblXslScriptError" runat="server" CssClass="normalred" Visible="False"></asp:Label><p
                align="center">
                <asp:Button ID="cmdGenerateXslt" resourcekey="btnGenerateXslt" runat="server" CssClass="CommandButton" >
                </asp:Button>
            </p>
        </div>
        <dnn:SectionHead ID="dshXslt" IsExpanded="false" Section="tblXslt" IncludeRule="True"
            ResourceKey="dshXslt" runat="server" CssClass="Head"></dnn:SectionHead>
        <div id="tblXslt" runat="server" style="padding-bottom: 20px; padding-top: 10px">
            <asp:TextBox ID="txtXslScript" Style="font-family: consolas, courier" runat="server"
                CssClass="Normal" Columns="70" Rows="30" TextMode="MultiLine" Width="100%"></asp:TextBox>
        </div>
        <dnn:SectionHead ID="dshSave" IsExpanded="false" Section="tblSave" IncludeRule="True"
            ResourceKey="dshSave" runat="server" CssClass="Head"></dnn:SectionHead>
        <table id="tblSave" align="center" runat="server">
            <tr>
                <td colspan="2">
                    <asp:Label ID="lblSaveXslError" runat="server" CssClass="normalred" Visible="False"></asp:Label>
                </td>
            </tr>
            <tr align="center">
                <td class="SubHead" valign="top">
                    <dnn:Label ID="plFolderName" runat="server" ControlName="txtFolderName" Suffix=":">
                    </dnn:Label>
                </td>
                <td valign="top" align="left">
                    <asp:TextBox ID="txtFolderName" runat="server" Enabled="False"></asp:TextBox>
                </td>
            </tr>
            <tr align="center">
                <td class="SubHead" valign="top">
                    <dnn:Label ID="plFileName" runat="server" ControlName="txtFileName" Suffix=":"></dnn:Label>
                </td>
                <td valign="top" align="left">
                    <asp:TextBox ID="txtFileName" runat="server" Enabled="False" BackColor="LightGray"></asp:TextBox>
                    &nbsp;&nbsp;
                    <asp:LinkButton ID="cmdSaveFile" resourcekey="cmdSaveFile" runat="server" CssClass="CommandButton"
                        Enabled="false" ></asp:LinkButton>
                </td>
            </tr>
            <tr align="center">
                <td colspan="2">
                    <asp:Panel ID="panConfirm" runat="server" Visible="False">
                        <asp:Label ID="lblConfirm" CssClass="normalred" resourcekey="lblConfirm" runat="server"></asp:Label>&nbsp;&nbsp;
                        <asp:LinkButton ID="cmdConfirmOverwriteFile" CssClass="CommandButton" resourcekey="Yes"
                            runat="server" ></asp:LinkButton>&nbsp;&nbsp;
                        <asp:LinkButton ID="cmdDenyOverwriteFile" CssClass="CommandButton" resourcekey="No"
                            runat="server"></asp:LinkButton>
                    </asp:Panel>
                </td>
            </tr>
        </table>
    </div>
    <asp:LinkButton ID="cmdBack" resourcekey="cmdBack" runat="server" CssClass="CommandButton"
        Text="Back" BorderStyle="none" CausesValidation="False" ></asp:LinkButton>
</div>

