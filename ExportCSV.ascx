<%@ Control Language="C#" CodeBehind="ExportCSV.ascx.cs" Inherits="DotNetNuke.Modules.UserDefinedTable.ExportCsv" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/Themes/jquery-ui.css" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/Themes/jquery.ui.theme.css" />

<div class="dnnForm">
    <div class="dnnFormItem">
        <dnn:Label ID="plFolder" runat="server" ControlName="cboFolders" Suffix=":" />
        <asp:DropDownList ID="cboFolders" runat="server" CssClass="NormalTextBox" Width="300" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plFile" runat="server" ControlName="txtFile" Suffix=":" />
        <asp:TextBox ID="txtFile" CssClass="NormalTextBox" runat="server" MaxLength="200"
            Width="300" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plDelimiter" runat="server" controlname="rblDelimiter" suffix=":" />
        <asp:RadioButtonList ID="rblDelimiter" runat="server" RepeatDirection="Horizontal"
            CssClass="dnnFormRadioButtons ">
            <asp:ListItem Selected="True" Value="," resourcekey="comma"></asp:ListItem>
            <asp:ListItem Value=";" resourcekey="semicolon"></asp:ListItem>
        </asp:RadioButtonList>
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plSystemFields" runat="server" controlname="cbSystemFields" />
        <asp:CheckBox ID="cbSystemFields" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plInitialDate" runat="server" controlname="cbSystemFields" />
        <asp:TextBox ID="txtInitialDate" CssClass="NormalTextBox" runat="server" MaxLength="200"
            Width="300" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plFinalDate" runat="server" controlname="cbSystemFields" />
        <asp:TextBox ID="txtFinalDate" CssClass="NormalTextBox" runat="server" MaxLength="200"
            Width="300" />
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdExport" resourcekey="cmdExport" runat="server" CssClass="dnnPrimaryAction"
                Text="Export" BorderStyle="none"></asp:LinkButton></li>
        <li>
            <asp:LinkButton ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction"
                Text="Cancel" BorderStyle="none" CausesValidation="False"></asp:LinkButton></li>
    </ul>

</div>

<script>
    $(function () {
      $("#<%= txtInitialDate.ClientID %>").attr("readonly", "readonly");
      $("#<%= txtInitialDate.ClientID %>").datepicker({ dateFormat: "yy/mm/dd" });
      $("#<%= txtFinalDate.ClientID %>").attr("readonly", "readonly");
      $("#<%= txtFinalDate.ClientID %>").datepicker({ dateFormat: "yy/mm/dd" });
  });
</script>
