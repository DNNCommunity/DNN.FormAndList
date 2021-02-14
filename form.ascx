<%@ Control Language="C#"  Inherits="DotNetNuke.Modules.UserDefinedTable.EditForm" CodeBehind="Form.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/Themes/jquery-ui.css"/>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/Themes/jquery.ui.theme.css"/>
<div runat="server" id="divForm"  class="dnnForm fnlForm dnnClear">

    <div runat="server" ID="EditFormPlaceholder"  />
    <asp:Panel ID="gRecaptcha" CssClass="recaptcha-container" runat="server" />
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" Text="Update" runat="server" resourcekey="cmdUpdate" cssclass="dnnPrimaryAction reCaptchaSubmit" />
        </li>
        <li> 
            <asp:LinkButton ID="cmdCancel" Text="Cancel" CausesValidation="False" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" />
        </li>
        <li>
            <asp:LinkButton ID="cmdDelete" Text="Delete" CausesValidation="False" resourcekey="cmdDelete" runat="server" class="dnnSecondaryAction"  />
        </li>
         <li>
                <asp:HyperLink runat="server" CssClass="dnnSecondaryAction" ID="cmdShowRecords" Visible="False"></asp:HyperLink>
          </li>
    </ul>
</div>
 <div runat="server" ID="MessagePlaceholder"  />
<script type="text/javascript">
/* Wrap your code in a function to avoid global scope and pass in any global objects */
/* globals jQuery, window, Sys */
(function($, Sys) {

    /* wire up any plugins or other logic here */

    function setUpMyModule() {
        $('#<%=EditFormPlaceholder.ClientID%>').dnnPanels();
        $('.fnl-datepicker').datepicker({
            monthNames:  [<%=LocalizeString("MonthNames")%>],
            dayNames:    [<%=LocalizeString("DayNames")%>],
            dayNamesMin: [<%=LocalizeString("DayNamesMin")%>],
            firstDay:    <%:LocalizeString("FirstDay")%>,
            dateFormat:  '<%:JsUiDatePattern %>'
        });
    }

    /* wire up the call to your function on document ready */
    $(document).ready(function() {

        setUpMyModule();

        /* Wire up the call to your function after an update panel request */
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {
            setUpMyModule();
        });
    });

}(jQuery, window.Sys))
</script>