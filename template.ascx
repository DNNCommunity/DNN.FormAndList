<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Control Language="C#"  Inherits="DotNetNuke.Modules.UserDefinedTable.Template"
    CodeBehind="Template.ascx.cs" AutoEventWireup="false" %>



<div class="dnnForm">
    <div class="dnnFormMessage dnnFormWarning" runat="server" visible="false" id="divWarning" />
    <div class="dnnFormItem">
        <dnn:Label ID="plTitle" runat="server" ControlName="txtTitle" Suffix=":"></dnn:Label>
        <asp:TextBox ID="txtTitle" runat="server" Columns="80"></asp:TextBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plDescription" runat="server" ControlName="txtDescription" Suffix=":">
        </dnn:Label>
        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Columns="62"
            Rows="5"></asp:TextBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plNumberofRecords" runat="server" ControlName="txtNumbers" Suffix=":">
        </dnn:Label>
        <asp:TextBox ID="txtNumbers" runat="server" Columns="4" Text="1"></asp:TextBox>
    </div>
    <ul class="dnnActions dnnClear" id="panSave" runat="server"  >
        <li>
            <asp:LinkButton ID="cmdSaveFile" resourcekey="cmdSaveFile" runat="server" CssClass="dnnPrimaryAction"
                OnClick="cmdSaveFile_Click"></asp:LinkButton></li>
    </ul>
    <ul id="panConfirm" runat="server" visible="False" class="dnnActions dnnClear">
        <li>
            <asp:Label ID="lblConfirm" CssClass="normalred" resourcekey="lblConfirm" runat="server"></asp:Label></li>
        <li>
            <asp:LinkButton ID="cmdConfirmOverwriteFile" CssClass="dnnPrimaryAction" resourcekey="Yes"
                runat="server" OnClick="cmdConfirmOverwriteFile_Click"></asp:LinkButton>
        </li>
        <li>
            <asp:LinkButton ID="cmdDenyOverwriteFile" CssClass="dnnSecondaryAction" resourcekey="No"
                runat="server" OnClick="cmdDenyOverwriteFile_Click"></asp:LinkButton>
        </li>
    </ul>
</div>

