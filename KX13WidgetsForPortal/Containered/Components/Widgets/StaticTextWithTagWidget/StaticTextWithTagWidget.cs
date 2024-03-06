using Generic.Components.Widgets.StaticTextWithTagWidget;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using PageBuilderContainers;

[assembly: RegisterWidget(StaticTextWithTagWidgetProperties.IDENTIFIER, "Static Text With Tag",
    typeof(StaticTextWithTagWidgetProperties),
    customViewName: "/Components/Widgets/StaticTextWithTagWidget/StaticTextWithTagWidget.cshtml",
    Description = "The Static text widget can be used to place HTML content onto the page.",
    IconClass = "icon-w-static-text")]
namespace Generic.Components.Widgets.StaticTextWithTagWidget
{
    public class StaticTextWithTagWidgetProperties : PageBuilderWidgetProperties, IWidgetProperties 
    {
        public const string IDENTIFIER = "Generic.StaticTextWithTag";
        [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "Text Content", Order = 0)]
        public string Text { get; set; } = string.Empty;

        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Tag", Order = 1)]
        public string? Tag { get; set; }

        public RichTextWidgetProperties()
        {

        }
    }
}