<%@ Control Language="C#"  Inherits="DotNetNuke.Modules.UserDefinedTable.EditForm" CodeBehind="Form.ascx.cs" AutoEventWireup="false" %>
<div runat="server" id="divForm"  class="dnnForm fnlForm dnnClear">

    <div runat="server" ID="EditFormPlaceholder"  />
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" Text="Update" runat="server" resourcekey="cmdUpdate" cssclass="dnnPrimaryAction"  />
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