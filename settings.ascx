<%@ Control Language="C#"  Inherits="DotNetNuke.Modules.UserDefinedTable.Settings" AutoEventWireup="false" CodeBehind="Settings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div style="margin-top: 14px;">
    <p>
        <asp:CheckBox ID="chkEditOwnData" runat="server" CssClass="Normal" resourcekey="EditOwnData"
            Text="User is only allowed to manipulate his own items."></asp:CheckBox></p>
    <p>
        <asp:CheckBox ID="chkCaptcha" Text="Force Captcha control during edit for Anonymous to fight spam."
            resourcekey="ForceCaptcha" CssClass="Normal" runat="server"></asp:CheckBox></p>
    <p>
        <asp:CheckBox ID="chkInputFiltering" Text="Filter input for markup code or script input. Note: Filtering is always enabled for Anonymous."
            resourcekey="InputFiltering" CssClass="Normal" runat="server"></asp:CheckBox></p>
    <p>
        <asp:CheckBox ID="chkDisplayColumns" Text="Negate permission/feature DisplayColumns for Administators."
            resourcekey="DisplayColumns" CssClass="Normal" runat="server"></asp:CheckBox></p>
              <p>
        <asp:CheckBox ID="chkHideSystemColumns" 
            resourcekey="DisplaySystemColumns" CssClass="Normal" runat="server"></asp:CheckBox></p>
    <p>
        <asp:CheckBox ID="chkPrivateColumns" Text="Negate permission/feature PrivateColumns for Administators."
            resourcekey="PrivateColumns" CssClass="Normal" runat="server"></asp:CheckBox></p>
    <table cellspacing="4" cellpadding="0" border="0" class="Normal">
        <tr>
            <td>
                <dnn:Label class="Normal" ID="lblUserRecordQuota" ResourceKey="plUserRecordQuota"
                    runat="server" Suffix=":" ControlName="txtUserRecordQuota" />
            </td>
            <td>
                <asp:TextBox ID="txtUserRecordQuota" runat="server" CssClass="Normal" MaxLength="4"
                    Width="40" />
            </td>
        </tr>
    </table>
</div>

