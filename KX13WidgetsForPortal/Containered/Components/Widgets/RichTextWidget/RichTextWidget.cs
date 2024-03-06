using Generic.Components.Widgets.RichTextWidget;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using PageBuilderContainers;

[assembly: RegisterWidget(RichTextWidgetProperties.IDENTIFIER, "Rich Text",
    typeof(RichTextWidgetProperties),
    customViewName: "/Components/Widgets/RichTextWidget/RichTextWidget.cshtml",
    Description = "Text area where the content will be rendered as is as HTML through the widget dialog",
    IconClass = "icon-l-lightbox")]
namespace Generic.Components.Widgets.RichTextWidget
{
    public class RichTextWidgetProperties : PageBuilderWidgetProperties, IWidgetProperties 
    {
        public const string IDENTIFIER = "Generic.RichText";
        [EditingComponent(RichTextComponent.IDENTIFIER, Label = "Html Content")]
        public string Html { get; set; } = string.Empty;

        public RichTextWidgetProperties()
        {

        }
    }
}