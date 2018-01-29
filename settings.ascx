<%@ Control Language="C#"  Inherits="DotNetNuke.Modules.UserDefinedTable.Settings" AutoEventWireup="True" CodeBehind="Settings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div style="margin-top: 14px;">
    <p>
        <asp:CheckBox ID="chkEditOwnData" runat="server" CssClass="Normal" resourcekey="EditOwnData"
            Text="User is only allowed to manipulate his own items."></asp:CheckBox></p>   
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
        <tr>
            <td>
                <dnn:Label class="Normal" id="lblForceCaptcha" runat="server" Suffix=":" ControlName="ddlCaptcha" />
            </td>
            <td>
                <asp:DropDownList ID="ddlCaptcha" runat="server" CssClass="Normal" />
            </td>
        </tr>
        <tr class="siteKeyRow" style="display:none;">
            <td><dnn:Label class="Normal" id="lblSiteKey" runat="server" Suffix=":" ControlName="txtSiteKey" /></td>
            <td><asp:TextBox ID="txtSiteKey" runat="server" /></td>
        </tr>
        <tr class="secretKeyRow" style="display:none;">
            <td><dnn:Label class="Normal" id="lblSecretKey" runat="server" Suffix=":" ControlName="txtSecretKey" /></td>
            <td><asp:TextBox ID="txtSecretKey" runat="server" /></td>
        </tr>
    </table>
</div>

<script type="text/javascript">
    $(document).ready(function () {
        FnlReCaptcha.update();
        $('#<%=ddlCaptcha.ClientID %>').change(function () {
            FnlReCaptcha.update();
        });
    });
    var FnlReCaptcha = {
        update: function () {
            if ($('#<%=ddlCaptcha.ClientID%>').val() == "ReCaptcha")
            {
                this.showControls();
            }
            else {
                this.hideControls();
            }
        },
        showControls: function () {
            $('.siteKeyRow').show('slow');
            $('.secretKeyRow').show('slow');            
        },
        hideControls: function () {
            $('.siteKeyRow').hide('slow');
            $('.secretKeyRow').hide('slow');
        }

    };
</script>