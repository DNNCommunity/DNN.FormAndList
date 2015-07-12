<%@ Control Language="C#"  Inherits="DotNetNuke.Modules.UserDefinedTable.TemplateList"
            CodeBehind="TemplateList.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<div style="margin-bottom:1em;">
    <asp:Label runat="server" ID="lblNewConfig" resourcekey="lblNewConfig">Start with a new </asp:Label>&nbsp; 
    <asp:HyperLink ID="cmdConfig" runat="server" CssClass="dnnPrimaryAction">
        <asp:Label ID="lblConfig" runat="server" resourcekey="lblConfig">Configuration</asp:Label>
    </asp:HyperLink>
    &nbsp;<asp:Label runat="server" ID="lblTemplate" resourcekey="lblTemplate">or choose a template:</asp:Label>
</div>
<asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataSourceID="TemplateDataSource"
              PagerSettings-Visible="false" BorderWidth="0px" GridLines="None" CellPadding="4">
    <AlternatingRowStyle CssClass="dnnGridAltItem" />

    <RowStyle CssClass="dnnGridItem" />
    <HeaderStyle CssClass="dnnGridHeader" />
    <Columns>
        <asp:TemplateField ShowHeader="False">
            <ItemTemplate>
                <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Select"
                                resourcekey="Select.Text" CssClass="dnnSecondaryAction"></asp:LinkButton>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
    </Columns>
    <PagerSettings Visible="False" />
</asp:GridView>
<asp:ObjectDataSource ID="TemplateDataSource" runat="server" SelectMethod="TemplateList"
                      TypeName="DotNetNuke.Modules.UserDefinedTable.Templates.TemplateController" />
<div>
    <asp:Label runat="server" ID ="lblCustomizeTemplate"></asp:Label>
    <dnn:CollectionEditorControl runat="server" ID="TemplateCustomValuesEditor" EditorDataField="Editor"
                                 EnableClientValidation="False" GroupByMode="None" NameDataField="Caption" ValueDataField="Value"
                                 LengthDataField="Length" VisibleDataField="TrueColumn" AutoGenerate="true" HelpDisplayMode="Never"
                                 EditControlStyle-CssClass="NormalTextBox" ErrorStyle-CssClass="NormalRed" GroupHeaderStyle-CssClass="Head"
                                 EditControlStyle-Width="525px" GroupHeaderIncludeRule="True" HelpStyle-CssClass="Help"
                                 LabelStyle-CssClass="SubHead" VisibilityStyle-CssClass="Normal" Width="700px" >
    </dnn:CollectionEditorControl>
    <asp:LinkButton CssClass="CommandButton" runat="server" ID="cmdApply" resourcekey="Select"
                    Visible="False"  OnClick="cmdApply_Click"/>
</div>