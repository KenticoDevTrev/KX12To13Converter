<%@ Page Language="C#" AutoEventWireup="true" Theme="Default" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder.RunConversion" %>

<asp:Content runat="server" ContentPlaceHolderID="plcBeforeContent">
</asp:Content>
<asp:Content ID="cntContent" ContentPlaceHolderID="plcContent" runat="Server">

    <div>
        <h1>Portal Engine to Page Builder</h1>
        <p>This page will processes normal pages (Page Builder) and generate the proper MVC Data.</p>
        <p>Include your configuration files below, as well as specify any NodeAliasPath (optional) you want to processes.</p>
        <p>Below are some notes:</p>
        <ul>
            <li>You can use the PortalToMVCEvents <a href="https://docs.xperience.io/k12sp/custom-development/handling-global-events">Global Events</a> to override / skip default logic.</li>
            <li>You can elect to Preview a single document, or convert multiple and documents, or send to your upgraded KX 13</li>
            <li>A copy of the outputed results will be saved to the table "KX12To13Converter_Conversions"</li>
        </ul>
        <br />
        <asp:Label runat="server" ID="lblConfigMode" CssClass="control-label" AssociatedControlID="ddlConfigMode" Text="Configuration Mode" />
        <asp:DropDownList CssClass="form-control" runat="server" Width="415px" ID="ddlConfigMode" ClientIDMode="Static">
            <asp:ListItem Value="UseSettings" Selected="True" Text="Pull Configuration from Settings" />
            <asp:ListItem Value="Manual" Text="Manually Enter Configuration" />
        </asp:DropDownList>

        <div id="pnlManualConfig" style="display: none;">
            <p>
                <asp:Label runat="server" ID="lbltemplateConfigJson" CssClass="control-label" AssociatedControlID="tbxTemplateConfigJson" Text="Template Configuration (JSON)" /><br />
                <asp:TextBox ID="tbxTemplateConfigJson" runat="server" TextMode="MultiLine" Width="400px" />
            </p>
            <p>
                <asp:Label runat="server" ID="lblDefaultSectionConfigJson" CssClass="control-label" AssociatedControlID="tbxDefaultSectionConfigJson" Text="Default Section Configuration (JSON)" /><br />
                <asp:TextBox ID="tbxDefaultSectionConfigJson" runat="server" TextMode="MultiLine" Width="400px" />
            </p>
            <p>
                <asp:Label runat="server" ID="lblSectionConfigJson" CssClass="control-label" AssociatedControlID="tbxSectionConfigJson" Text="Section Configuration (JSON)" /><br />
                <asp:TextBox ID="tbxSectionConfigJson" runat="server" TextMode="MultiLine" Width="400px" />
            </p>
            <p>
                <asp:Label runat="server" ID="lblWidgetConfiguration" CssClass="control-label" AssociatedControlID="tbxWidgetConfiguration" Text="Widget Configuration (JSON)" /><br />
                <asp:TextBox ID="tbxWidgetConfiguration" runat="server" TextMode="MultiLine" Width="400px" />
            </p>
        </div>
        <br />
        <asp:Label runat="server" ID="lblMode" CssClass="control-label" AssociatedControlID="ddlMode" Text="Conversion Mode" />
        <asp:DropDownList CssClass="form-control" runat="server" Width="415px" ID="ddlMode" ClientIDMode="Static">
            <asp:ListItem Value="Preview" Selected="True" Text="Preview (Single Page)" />
            <asp:ListItem Value="Process" Text="Convert and Store in Conversions" />
            <asp:ListItem Value="ProcessAndSave" Text="Convert and Store in Conversions + Save to Document" />
            <asp:ListItem Value="ProcessAndSend" Text="Convert and Store in Conversions + Send to KX13 instance*" />
        </asp:DropDownList>
        <small>* This requires installing the KX12To13Converter.KX13Receiver.Admin package on the KX13 admin interface and configuring settings.</small>

        <asp:Label runat="server" ID="lblSite" CssClass="control-label" AssociatedControlID="ddlSite" Text="Site" /> 
        <asp:DropDownList CssClass="form-control" Width="415px" runat="server" ID="ddlSite" />
        <br />
        <div id="pnlPreviewConfig">
            <asp:Label runat="server" ID="lblPreviewCulture" CssClass="control-label" AssociatedControlID="tbxPreviewCulture" Text="Culture" /><br />
            <asp:TextBox runat="server" ID="tbxPreviewCulture" CssClass="form-control" Width="415px" />
            <asp:Label runat="server" ID="lbltbxPath" CssClass="control-label" AssociatedControlID="tbxPreviewPath" Text="Page" /><br />
            <cms:FormControl ID="tbxPreviewPath" FormControlName="selectsinglepath" runat="server" />
        </div>
        <div id="pnlProcess" style="display:none;">
            <asp:Label runat="server" ID="Label1" CssClass="control-label" AssociatedControlID="tbxProcessPath" Text="Page or Path" /><br />
            <cms:FormControl ID="tbxProcessPath" FormControlName="selectpath" runat="server" />
        </div>
        <br />
        <asp:Button runat="server" ID="btnGenerate" OnClick="btnConvert_Click" CssClass="btn btn-primary" Text="Convert" />
        <hr />

        <p><strong>DocumentPageTemplateConfiguration JSON</strong> <a href="javascript:void(0);" id="btnCopyToClipboardTemplate">Copy to Clipboard <span class="icon-doc-copy"></span></a><br />
        <asp:TextBox runat="server" ID="tbxJsonResultTemplate" ClientIDMode="Static" TextMode="MultiLine" Height="200" Width="415px" />
        </p>
        <p>
        <strong>DocumentPageBuilderWidgets JSON</strong> <a href="javascript:void(0);" id="btnCopyToClipboardWidgets">Copy to Clipboard <span class="icon-doc-copy"></span></a><br />
        <asp:TextBox runat="server" ID="tbxJsonResultWidgets" ClientIDMode="Static" TextMode="MultiLine" Height="200" Width="415px" />
            </p>
        <script type="text/javascript">
            
            function ToggleManualConfig() {
                $cmsj("#pnlManualConfig").toggle($cmsj("#ddlConfigMode").val() == "Manual");
            }
            function ToggleProcessConfig(isPreview) {
                var isPreview = ($cmsj("#ddlMode").val() == "Preview");
                $cmsj("#pnlPreviewConfig").toggle(isPreview);
                $cmsj("#pnlProcess").toggle(!isPreview);
            }
            $cmsj(document).ready(function () {
                $cmsj("#ddlConfigMode").change(function () {
                    ToggleManualConfig();
                });
                $cmsj("#ddlMode").change(function () {
                    ToggleProcessConfig();
                });
                ToggleManualConfig();
                ToggleProcessConfig();
            });
            $cmsj("#btnCopyToClipboardTemplate").click(function () {
                var temp = $cmsj("<textarea></textarea>");
                $cmsj("body").append(temp);
                temp.val($cmsj("#tbxJsonResultTemplate").val()).select();
                document.execCommand("copy");
                temp.remove();
            });
            $cmsj("#btnCopyToClipboardWidgets").click(function () {
                var temp = $cmsj("<textarea></textarea>");
                $cmsj("body").append(temp);
                temp.val($cmsj("#tbxJsonResultWidgets").val()).select();
                document.execCommand("copy");
                temp.remove();
            });
            
        </script>
    </div>
</asp:Content>
