<%@ Page Language="C#" AutoEventWireup="true" Theme="Default" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder.TemplateConfigurationBuilder" %>

<asp:Content runat="server" ContentPlaceHolderID="plcBeforeContent">
</asp:Content>
<asp:Content ID="cntContent" ContentPlaceHolderID="plcContent" runat="Server">

    <div>
        <h1>Template Configuration Generator</h1>
        <p>The Portal engine to Page Builder conversion tool accepts various Configuration JSON structures to help automate conversion processes.</p>
        <p>Press the button below to generate the Template Configuration, which instructs the system how to convert Page Templates (and their widget zones) into Page Builder Templates.</p>
        <p>Below are some notes:</p>
        <ul>
            <li>Only Widget Zones will be converted, Webpart Zones and their webparts will be 'hard coded' in the Page Builder system, so no conversion is necessary.</li>
            <li>Widget Zones within Widget Zones will not be part of this conversion, those will be considered "Sections" in the Widget Configuration builder.</li>
            <li>Editable Text / Editable Image webparts will be included as editable widget zones with their webpart ID as the editable zone name.  You can add these to existing Page Builder editable areas or make them their own Page Builder Editable Areas</li>
            <li>By default, only page templates that are in use are generated.  You may want to delete archived/unpbulished pages that you do not want to migrate over to reduce the list of page templates.  Additionally you can manually force include/exclude below.</li>
        </ul>
        <br />
        <p>
            <asp:Label runat="server" ID="lblIncluded" CssClass="control-label" AssociatedControlID="tbxIncluded" Text="Page Template Code Names to force include (one per line)" /><br />
            <asp:TextBox runat="server" ID="tbxIncluded" TextMode="MultiLine" Width="400px" />
        </p>
        <p>
            <asp:Label runat="server" ID="lblExcluded" CssClass="control-label" AssociatedControlID="tbxExcluded" Text="Page Template Code Names to force exclude (one per line)" /><br />
            <asp:TextBox runat="server" ID="tbxExcluded" TextMode="MultiLine" Width="400px" />
        </p>
        <hr />
        <p>
            <asp:Button runat="server" ID="btnGenerateTemplates" OnClick="btnGenerateTemplates_Click" CssClass="btn btn-primary" Text="Generate Template Configuration" />
        </p>
        <p>
            <br />
            <strong>Instructions</strong>:
        </p>
        <ul>
            <li><strong>INHERIT</strong> = Keep same name</li>
            <li><strong>IGNORE</strong> = Do Not Processes</li>
            <li><strong>PE_CodeName</strong>: The Portal Engine Template Code Name</li>
            <li><strong>PB_Template</strong>:
                <ul>
                    <li><strong>PB_TemplateIdentifier</strong>: The Page Builder Template Code Name</li>
                    <li><strong>PB_KeyValuePairs</strong>: Template properties that you wish to add during conversion (ex: { "MyNewParam":"Hello", "MyOtherParam",: "World" })</li>
                </ul>
            </li>
            <li><strong>Zones</strong>: Zones present in the template
                <ul>
                    <li><strong>PE_WebpartZoneName</strong>: The Portal Engine Webpart Zone Name (or Widget name if it's an editable image/text)</li>
                    <li><strong>PB_EditableAreaName</strong>: The Page Builder editable area code name that the contents will be pushed into.</li>
                    <li><strong>isEditableText</strong>: If this zone is actually just an Edtiable Text webpart (treated as a zone in Page Builder)</li>
                    <li><strong>isEditableImage</strong>: If this zone is actually just an Edtiable Image webpart (treated as a zone in Page Builder)</li>
                </ul>
            </li>
        </ul>

        <p>
            <a href="javascript:void(0);" id="btnCopyToClipboard">Copy to Clipboard <span class="icon-doc-copy"></span></a>
        </p>

        <asp:TextBox runat="server" ID="tbxJsonStructure" TextMode="MultiLine" ClientIDMode="Static" Height="400px" Width="100%" />
        <script type="text/javascript">
            $cmsj("#btnCopyToClipboard").click(function () {
                var temp = $cmsj("<textarea></textarea>");
                $cmsj("body").append(temp);
                temp.val($cmsj("#tbxJsonStructure").val()).select();
                document.execCommand("copy");
                temp.remove();
            });
        </script>
        </p>
    </div>
</asp:Content>
