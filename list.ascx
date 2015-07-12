<%@ Control Language="C#" Inherits="DotNetNuke.Modules.UserDefinedTable.List" AutoEventWireup="false" CodeBehind="List.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Import Namespace="DotNetNuke.Entities.Icons" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<div class="dnnForm dnnClear">
    <asp:Panel ID="panSearch" Width="100%" CssClass="Normal" runat="server" Visible="false" DefaultButton="cmdSearch">
        <asp:PlaceHolder ID="phSearchSentence" runat="server"/>&nbsp;&nbsp;
        <asp:LinkButton ID="cmdSearch" CssClass="CommandButton" runat="server" resourcekey="cmdSearch" />&nbsp;&nbsp;
        <asp:LinkButton ID="cmdResetSearch" CssClass="CommandButton" runat="server" resourcekey="cmdResetSearch" />&nbsp;&nbsp;
        <dnn:Label ID="plSearch" runat="server" Text="" />
        <br />
        <asp:Label ID="lblNoRecordsFound" CssClass="Normal" runat="server" Visible="False" resourcekey="lblNoRecordsFound" />
    </asp:Panel>
    <asp:GridView ID="grdData" runat="server" Visible="False" EnableViewState="False"
                  PagerSettings-Visible="false" GridLines="None" CellPadding="4" AutoGenerateColumns="False"
                  AllowSorting="True" CssClass="dnnGrid" BorderWidth="0px">
        <AlternatingRowStyle CssClass="dnnGridAltItem" />
        <RowStyle CssClass="dnnGridItem" horizontalalign="Left"/>
        <HeaderStyle CssClass="dnnGridHeader" verticalalign="Top"/>
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Label runat="server" 
                               Text='<%# DataBinder.Eval(Container, "DataItem.EditLink", "<a href=\"{0}\"><img src=\"" + IconController.IconURL("Edit") + "\" alt=\"edit\" border=\"0\"/></a>") %>'>
                    </asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <dnn:PagingControl ID="ctlPagingControl" runat="server" totalrecord="0" PageSize="1" CurrentPage="1" Visible="False" Mode="PostBack" />
    <asp:PlaceHolder ID="XslOutput" runat="server" Visible="False" />
    <div runat="server" ID="placeholderActions" Visible="false" >
        <ul class="dnnActions dnnClear">
            <li>
                <asp:HyperLink runat="server" CssClass="dnnPrimaryAction" ID="ActionLink" Text="Action"></asp:HyperLink>
            </li>
        </ul>
    </div>
</div>