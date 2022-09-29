<%@ Page Language="C#" AutoEventWireup="true" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder.SectionConfigurationBuilder"
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" %>

<asp:Content runat="server" ContentPlaceHolderID="plcBeforeContent">
</asp:Content>
<asp:Content ID="cntContent" ContentPlaceHolderID="plcContent" runat="Server">

    <div>
        <h1>Section Configuration Generator</h1>
        <p>The Portal engine to Page Builder conversion tool accepts various Configuration JSON structures to help automate conversion processes.</p>
        <p>Press the button below to generate the Section Configuration, which instructs the system how to convert "Layout" widgets (widgets that contain widget zones).</p>
        <p>Below are some notes:</p>
        <ul>
            <li>Only Widget that are from Webparts with type "Layout" will be configurable, and only those used in published documents.</li>
            <li>Portal Engine Widget Zones did not require any section within them, so a Default Section Configuration is provided so you can define the section for these instances.</li>
            <li>Any user-editable properties will be present, hidden fields can be appended manually.</li>
        </ul>
        <br />
        <p>
            <asp:Label runat="server" ID="lbltbxIncluded" AssociatedControlID="tbxIncluded" Text="Layout Widget Code Names to force include (one per line)" CssClass="control-label" />
            <asp:TextBox runat="server" ID="tbxIncluded" TextMode="MultiLine" Width="400px" />
        </p>

        <p>
            <asp:Label runat="server" ID="lbltbxExcluded" AssociatedControlID="tbxExcluded" Text="Layout Widget Code Names to force exclude (one per line)" CssClass="control-label" />
            <asp:TextBox runat="server" ID="tbxExcluded" TextMode="MultiLine" Width="400px" />
        </p>
        <hr />
        <p>
            <asp:Button runat="server" ID="btnGenerate" OnClick="btnGenerate_Click" CssClass="btn btn-primary" Text="Generate Sections Configuration" />
        </p>
        <p>
            <br />
            <strong>Instructions</strong>:
        </p>
        <ul>
            <li><strong>INHERIT</strong> = Keep same name</li>
            <li><strong>IGNORE</strong> = Do Not Processes</li>
            <li><strong>PE_WidgetZoneSection</strong>:
                <ul>
                    <li><strong>SectionWidgetCodeName</strong>: The Widget Code Name for Portal Enginethe layout widget</li>
                    <li><strong>KeyValues</strong>
                        <ul>
                            <li><strong>Key</strong>: The Portal Engine column name</li>
                            <li><strong>Value</strong>: Forced value, if set will ignore existing widget values</li>
                            <li><strong>DefaultValue</strong>: Will be used if the widget value is empty</li>
                            <li><strong>OutKey</strong>: The field name for the Page Builder matching value (INHERIT, IGNORE, or a text value of the field name)</li>
                        </ul>
                    </li>
                </ul>
            </li>
            <li><strong>PB_Section</strong>:
                <ul>
                    <li><strong>SectionIdentifier</strong>: The Page Builder Section Identifier</li>
                    <li><strong>AdditionalKeyValues</strong>: New properties that do do not exist on the Portal engine layout widget that you wish to add during conversion (ex: { "MyNewParam":"Hello", "MyOtherParam",: "World" })</li>
                </ul>
            </li>
        </ul>
        <p>
            <br />
            <strong>Default Section Configuration:</strong> If there is no Layout widgets in the portal engine widget zone or you do not with to specify, please use and fill in the below to define the default Section for your MVC Site.
            <br />
            <a href="javascript:void(0);" id="btnCopyDefaultSectionToClipboard">Copy to Clipboard <span class="icon-doc-copy"></span></a>OR <a href="javascript:void(0);" id="btnCopyDefaultBootstrapSectionToClipboard" title="If using the Bootstrap4Layout tool as your default KX13 section, click here to use that instead.">Copy Bootstrap 4 Layout Tool Default Section<span class="icon-doc-copy"></span></a>
            <br />
            <asp:TextBox runat="server" ID="tbxDefaultSectionJson" TextMode="MultiLine" ClientIDMode="Static" Height="125px" Width="100%" />
            <span style="display: none;">
                <textarea id="tbxDefaultBootstrapSectionJson">
{
    "SectionIdentifier": "Bootstrap4LayoutTool",
    "AdditionalKeyValues": {
        "columns": 1,
        "columnCSSPrepend": "col-md",
        "containerCSS": null,
        "column1Width": "-12",
        "column1AdditionalCSS": null,
        "column2Width": "-12",
        "column2AdditionalCSS": null,
        "column3Width": "-12",
        "column3AdditionalCSS": null,
        "column4Width": "-12",
        "column4AdditionalCSS": null,
        "column5Width": "-12",
        "column5AdditionalCSS": null,
        "column6Width": "-12",
        "column6AdditionalCSS": null,
        "column7Width": "-12",
        "column7AdditionalCSS": null,
        "column8Width": "-12",
        "column8AdditionalCSS": null,
        "column9Width": "-12",
        "column9AdditionalCSS": null,
        "column10Width": "-12",
        "column10AdditionalCSS": null,
        "column11Width": "-12",
        "column11AdditionalCSS": null,
        "column12Width": "-12",
        "column12AdditionalCSS": null,
        "containerName": null,
        "containerTitle": null,
        "containerCSSClass": null,
        "containerCustomContent": null,
        "htmlBefore": null,
        "htmlAfter": null
    }
}
</textarea>
            </span>
            <script type="text/javascript">
                $cmsj("#btnCopyDefaultSectionToClipboard").click(function () {
                    var temp = $cmsj("<textarea></textarea>");
                    $cmsj("body").append(temp);
                    temp.val($cmsj("#tbxDefaultSectionJson").val()).select();
                    document.execCommand("copy");
                    temp.remove();
                });
                $cmsj("#btnCopyDefaultBootstrapSectionToClipboard").click(function () {
                    var temp = $cmsj("<textarea></textarea>");
                    $cmsj("body").append(temp);
                    temp.val($cmsj("#tbxDefaultBootstrapSectionJson").val()).select();
                    document.execCommand("copy");
                    temp.remove();
                });
            </script>
        </p>
        <p>
            <a href="javascript:void(0);" id="btnCopyToClipboard">Copy to Clipboard <span class="icon-doc-copy"></span></a>
            <br />
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
