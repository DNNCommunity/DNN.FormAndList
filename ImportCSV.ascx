<%@ Control Language="C#" CodeBehind="ImportCSV.ascx.cs" Inherits="DotNetNuke.Modules.UserDefinedTable.ImportCsv" AutoEventWireup="false"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>


<div class="dnnForm">
    <div class="dnnFormItem">
        <dnn:Label ID="plFolder" runat="server" ControlName="cboFolders" Suffix=":" />
        <asp:DropDownList ID="cboFolders" runat="server" CssClass="NormalTextBox" Width="300" AutoPostBack="true"/>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plFile" runat="server" ControlName="txtFile" Suffix=":" />
        <asp:DropDownList ID="cboFiles" Runat="server" CssClass="NormalTextBox" Width="300" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plDelimiter" runat="server" controlname="rblDelimiter" suffix=":" />
        <asp:RadioButtonList ID="rblDelimiter" runat="server" RepeatDirection="Horizontal"
            CssClass="dnnFormRadioButtons ">
            <asp:ListItem Selected="True" Value="," resourcekey="comma"></asp:ListItem>
            <asp:ListItem Value=";" resourcekey="semicolon"></asp:ListItem>
        </asp:RadioButtonList>
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdImport" resourcekey="cmdImport" runat="server" CssClass="dnnPrimaryAction"
                Text="Import" BorderStyle="none" />
        </li>
        <li>
            <asp:LinkButton ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction"
                Text="Cancel" BorderStyle="none" CausesValidation="False" />
        </li>
    </ul>
</div>
