<%@ Page Language="C#" AutoEventWireup="true" Theme="Default" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder.WidgetConfigurationBuilder" %>

<asp:Content runat="server" ContentPlaceHolderID="plcBeforeContent">
</asp:Content>
<asp:Content ID="cntContent" ContentPlaceHolderID="plcContent" runat="Server">

    <div>
        <h1>Widget Configuration Generator</h1>
        <p>The Portal engine to Page Builder conversion tool accepts various Configuration JSON structures to help automate conversion processes.</p>
        <p>Press the button below to generate the Widget Configuration, which instructs the system how to convert widgets to the Page Builder Widgets (which you'll need to create).</p>
        <p><strong>Performance:</strong> This script scans all page's Widgets and editable text for widget usage.  Generating the Widget Configuration on sites with many pages and/or a lot of widget content may take some time.  Be patient, and make sure you run the site in debug to prevent timeouts.</p>
        <p>Below are some notes:</p>
        <ul>
            <li>Only Widget with Security of "This widget can be used in editor zones" and "This widget can be used as an inline widget" are processed, and only those used in published documents.</li>
            <li>Widget Properties that display are either:
                <ul>
                    <li>Visible Widget Properties</li>
                    <li>Have a "Default Value" defined</li>
                    <li>Have either a visible or default valued ContentBefore/ContentAfter</li>
                    <li>Have either a visible or default valued Webpart Container properties</li>
                </ul>
            </li>
            <li>Inline Widgets:
                    <ul>
                        <li>Only parsed with "CanContainerInlineWidgets" is true.  Limit 1 of these fields per widget (editable text's value defaults to true).</li>
                        <li>Can be configured to either REMOVE, or SPLIT content</li>
                        <li>If split, will split the content around it into 2 static text widgets with the inline widget in between.  Must provide what that static text widget is in the first Widget Configuration for 'statictextWidget').</li>
                    </ul>
            </li>
        </ul>
        <br />

        <p>
            <asp:Label runat="server" ID="lblIncluded" CssClass="control-label" AssociatedControlID="tbxIncluded" Text="Widget Code Names to force include (one per line)" /><br />
            <asp:TextBox runat="server" ID="tbxIncluded" TextMode="MultiLine" Width="400px" />
        </p>
        <p>
            <asp:Label runat="server" ID="lblExcluded" CssClass="control-label" AssociatedControlID="tbxExcluded" Text="Widget Code Names to force exclude (one per line)" /><br />
            <asp:TextBox runat="server" ID="tbxExcluded" TextMode="MultiLine" Width="400px" />
        </p>
        <p>
            <asp:Button runat="server" ID="btnGenerate" OnClick="btnGenerate_Click" CssClass="btn btn-primary" Text="Generate Widget Configuration" />
        </p>
        <p>
            <br />
            <strong>Instructions</strong>:<br />
        </p>
        <ul>
            <li><strong>INHERIT</strong> = Keep same name</li>
            <li><strong>IGNORE</strong> = Do Not Processes</li>

            <li><strong>PE_Widget</strong>
                <ul>
                    <li><strong>PE_WidgetCodeName</strong>: Portal Engine Widget Code Name</li>
                    <li><strong>IsInlineWidget</strong>: reference only, if it could be used as an inline widget.</li>
                    <li><strong>IsEditorWidget</strong>: reference only, if it could be used as an editor widget (widget zone).</li>
                    <li><strong>IncludeHtmlBeforeAfter</strong>: If true, then ContentBefore and ContentAfter fields get translated (default to htmlAfter and htmlBefore).</li>
                    <li><strong>IncludeWebpartContainerProperties</strong>: If true, then Container, ContainerTitle, ContainerCSSClass, ContainerCustomContent fields get translated (default to containerName, containerTitle, containerCSSClass, containerCustomContent).</li>
                    <li><strong>KeyValues</strong>
                        <ul>
                            <li><strong>Key</strong>: The Portal Engine column name</li>
                            <li><strong>Value</strong>: Forced value, if set will ignore existing widget values</li>
                            <li><strong>DefaultValue</strong>: Will be used if the widget value is empty</li>
                            <li><strong>OutKey</strong>: The field name for the Page Builder matching value (INHERIT, IGNORE, or a text value of the field name)</li>
                            <li><strong>CanContainInlineWidgets</strong>: True if the field's value can have inline widgets, due to complexity can only have 1 of these fields be true per widget</li>
                            <li><strong>InlineWidgetMode</strong>: How inline widgets are processed:
                                     <ul>
                                         <li><strong>ADDAFTER</strong> [default] = Adds inline widgets after the containing widget</li>
                                         <li><strong>WRAP</strong> = If only 1 inline widget found, adds HtmlBefore and HtmlAfter attributes with the html content surround it (use <a href="https://www.nuget.org/packages/PageBuilderContainers.Kentico" target="_blank">PageBuilderContainers.Kentico</a>/<a href="https://www.nuget.org/packages/PageBuilderContainers.Kentico.MVC.Core" target="_blank">PageBuilderContainers.Kentico.MVC.Core</a> module)
                                            <ul>
                                                <li>Must also ensure inline widgets have <strong>IncludeHtmlBeforeAfter</strong> set to <strong>true</strong></li>
                                            </ul>
                                         </li>
                                         <li><strong>SPLIT</strong> = Split the html into static html widgets around the inline widget (must define the <strong>statictextWidget</strong> widget conversion in configuration, it's the first entry)</li>
                                         <li><strong>IGNORE</strong> = Remove inline widgets from html markup</li>
                                     </ul>
                            </li>
                        </ul>
                    </li>
                </ul>
            </li>
            <li><strong>PB_Widget</strong>
                <ul>
                    <li><strong>PB_WidgetIdentifier</strong>: The Widget Identifier  for Page Builder</li>
                    <li><strong>AdditionalKeyValues</strong>: New Widget properties that do do not exist on the Portal engine that you wish to add during conversion (ex: { "MyNewParam":"Hello", "MyOtherParam",: "World" })</li>
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
