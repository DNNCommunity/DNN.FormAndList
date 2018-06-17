<%@ Control Language="C#"CodeBehind="Field.ascx.cs" Inherits="DotNetNuke.Modules.UserDefinedTable.Controls.Field" AutoEventWireup="false"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Import Namespace="DotNetNuke.Entities.Icons" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<%@ Register src="FieldSettings.ascx" tagname="FieldSettings" tagprefix="fnl" %>

<div id="FieldSettings" class="dnnDialog dnnForm" >
    <div class="dnnFormMessage dnnFormWarning" runat="server" Visible="false" id="divWarning"/>
     <div class="dnnFormItem">
        <dnn:Label runat="server" resourcekey="Type" ControlName="cboFieldType" />
        <asp:Label ID="lblType" runat="server"  />
        <asp:Label ID="lblFieldType" runat="server"  />
        <asp:DropDownList ID="cboFieldType" runat="server" DataTextField="Key" 
                          DataValueField="Value"  AutoPostBack="true"/>
    </div>
    <div class="dnnFormItem">
        <dnn:Label  runat="server" resourcekey="Title" cboFieldType="txtFieldTitle"/>
        <asp:TextBox runat="server" ID="txtFieldTitle"  Width="400px" MaxLength="2000" 
                     TextMode="MultiLine" CssClass="dnn2rows dnnFormRequired" Rows="2"/>
        <span class="dnnFormMessage dnnFormError" runat="server" Visible="false" id="divError"/>  
        <asp:RequiredFieldValidator  runat="server" ErrorMessage="RequiredFieldValidator" 
                                     ControlToValidate="txtFieldTitle"  resourcekey="TitleIsRequired" CssClass="dnnFormMessage dnnFormError"/>
        <asp:CustomValidator 
            ID="valNoPipesInTitle"            
            runat="server" 
            ErrorMessage="The title may not contain the pipe ( | ) character" 
            ControlToValidate="txtFieldTitle" 
            resourcekey="TitleCannotHavePipe" 
            CssClass="dnnFormMessage dnnFormError" 
            OnServerValidate="valNoPipesInTitle_ServerValidate" 
            ClientValidationFunction="validateNoPipes" 
            EnableClientScript="true" 
            />
        
    </div>
    <div class="dnnFormItem" id="panHelpText" runat="server">
        <dnn:Label ID="lblHelpText" runat="server" ResourceKey="ControlHelpText" Suffix=":" />
        <asp:TextBox ID="txtHelpText" runat="server"  Width="400px" 
                     TextMode="MultiLine " Rows="5" />
    </div>
    <div class="dnnFormItem" id="panDefault" runat="server">
        <dnn:Label ID="DefaultLabel" runat="server" ResourceKey="DefaultValue" />
        <asp:TextBox ID="txtDefault" runat="server"  Width="400px" />
    </div>   
    <div class="dnnFormItem">
        <dnn:Label runat="server" resourcekey="Required" ControlName="chkRequired" />
        <asp:Image runat="server" IconKey="checked" ID="imgRequired" />
        <asp:CheckBox runat="server" ID="chkRequired"  />
    </div>
    <div class="dnnFormItem">
        <dnn:Label runat="server" resourcekey="DisplayOnList" ControlName="chkDisplayOnList" />
        <asp:CheckBox runat="server" ID="chkDisplayOnList"  />
    </div>
    <div class="dnnFormItem">
        <dnn:Label  runat="server" resourcekey="RestrictedFormField" ControlName="chkRestrictedFormField" />
        <asp:CheckBox runat="server" ID="chkRestrictedFormField"  />
    </div>
    <div class="dnnFormItem">
        <dnn:Label  runat="server" resourcekey="Searchable" ControlName="chkSearchable" ID="lblSearchable" />
        <asp:CheckBox runat="server" ID="chkSearchable"  />
    </div>
    
    <h2 class="dnnFormSectionHead"><asp:Label runat="server" ID="lblFormsettings" ResourceKey="FormSettings" /></h2>
    
    <fieldset>
    <div>
        <div class="dnnFormItem" id="panInputSettings" runat="server">
            <dnn:Label ID="InputSettingsLabel" runat="server"  />
            <asp:TextBox ID="txtInputSettings" runat="server" Width="400px" />
            <asp:DropDownList ID="cboInputSettings" runat="server" CssClass="NormalTextBox" Width="400px" Visible="False" DataTextField="Key" DataValueField="Value" />
            <asp:RadioButtonList ID="rblListType" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons pushRight">
                <asp:ListItem Selected="True" Value="DropDown" resourcekey="DropDown"></asp:ListItem>
                <asp:ListItem Value="RadioButtons" resourcekey="RadioButtons"></asp:ListItem>
                <asp:ListItem Value="RadioButtonsHorizontal" resourcekey="RadioButtonsHorizontal"></asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="dnnFormItem" id="panMultipleValues" runat="server">
            <dnn:Label ID="MultipleValuesLabel" runat="server" ResourceKey="MultipleValues" />
            <asp:CheckBox ID="chkMultipleValues" runat="server"  />
        </div>
        <div class="dnnFormItem" id="panValidationRule" runat="server">
            <dnn:Label ID="lblValidationRule" runat="server" ResourceKey="ValidationRule" Suffix=":" />
            <asp:TextBox ID="txtValidationRule" runat="server"  Width="400px" MaxLength="512" />
        </div>
        <div class="dnnFormItem" id="panValidationMessage" runat="server">
            <dnn:Label ID="lblValidationMessage" runat="server" ResourceKey="ValidationMessage" Suffix=":" />
            <asp:TextBox ID="txtValidationMessage" runat="server"  Width="400px" MaxLength="512" />
        </div>
    
        <div class="dnnFormItem" id="panEditStyle" runat="server">
            <dnn:Label ID="lblEditStyle" runat="server" ResourceKey="EditCssStyle" Suffix=":" />
            <asp:TextBox ID="txtEditStyle" runat="server" Width="400px" MaxLength="512" />
        </div>
        <div class="dnnFormItem" id="panShowOnEdit" runat="server">
            <dnn:Label ID="ShowOnEditLabel" runat="server" ResourceKey="ShowOnEdit" Suffix=":" />
            <asp:CheckBox ID="chkShowOnEdit" runat="server"  />
        </div> 
        <fnl:FieldSettings ID="FormFieldSettings" runat="server" Section="Form" />   
        </div>
    </fieldset>
    
    <h2 class="dnnFormSectionHead"><asp:Label runat="server" ID="lblListSettings" ResourceKey="ListSettings" /></h2>
    <fieldset>
   <div>
        <div class="dnnFormItem" id="panOutputSettings" runat="server"> 
            <dnn:Label ID="OutputSettingsLabel" runat="server"  />
            <asp:TextBox ID="txtOutputSettings" runat="server" Width="400px" 
                         MaxLength="2000" />
        </div>
   
        <div class="dnnFormItem" id="panNormalizeFlag" runat="server">
            <dnn:Label ID="NormalizeFlagLabel" runat="server" Suffix="?"  />
            <asp:CheckBox ID="chkNormalizeFlag" runat="server"  />
        </div>
        <fnl:FieldSettings ID="ListFieldSettings" runat="server" Section ="List" />
        </div>
    </fieldset>

    <ul class="dnnActions dnnClear">
        <li><dnn:CommandButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" ResourceKey="cmdSave" /></li>
        <li><dnn:CommandButton ID="cmdCancel" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdCancel"  ValidationGroup="None"/></li> 
        <li><asp:HyperLink ID="hlpToken" CssClass="dnnSecondaryAction" runat="server" resourcekey="cmdTokensHelp"  NavigateUrl="#" /></li>
        <li><asp:HyperLink ID="hlpColumns" CssClass="dnnSecondaryAction" runat="server" resourcekey="cmdColumnsHelp" NavigateUrl="#" /></li>
    </ul>
</div>

<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/UserDefinedTable/CaptionValueEditor.js" />
  


<script type="text/javascript">
   
    (function($) {
        $(document).ready(function() {


            $("#<%=txtInputSettings.ClientID%>").CaptionValueEditor({
                    captionHeaderText: '<%=LocalizeString("CaptionHeaderText")%>',
                    valueHeaderText: '<%=LocalizeString("ValueHeaderText")%>',
                    editLinkText: '<%=LocalizeString("EditLinkText")%>',
                    addButtonText: '<%=LocalizeString("AddButtonText")%>',
                    removeButtonText: '<%=LocalizeString("RemoveButtonText")%>',
                    saveButtonText: '<%=LocalizeString("SaveButtonText")%>',
                    cancelButtonText: '<%=LocalizeString("CancelButtonText")%>',
                    title: '<%=LocalizeString("EditCaptionTitle")%>',
                    editImageUrl: '<%=IconController.IconURL("Edit")%>',
                    addImageUrl: '<%=IconController.IconURL("Add")%>',
                    removeImageUrl: '<%=IconController.IconURL("Delete")%>'
                });

        });
    }(jQuery));

    function OpenHelpWindow(url) {
        window.open(url, '', 'width=800, height=800, location=no, menubar=no, resizable=yes, scrollbars=yes, status=no, toolbar=no');
        window.event.returnValue = false;
    }

    function validateNoPipes(sender, args) {
        var text = args.Value;
        if (text.indexOf('|')>=0) {
            args.IsValid = false;
            return;
        }
        args.IsValid = true;
    }
</script>