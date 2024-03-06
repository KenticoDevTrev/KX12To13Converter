using Kentico.Forms.Web.Mvc;
using Generic.Components.Widgets.StaticTextWidget;

[assembly: RegisterWidget(StaticTextWidgetProperties.IDENTIFIER,
    "Static Text (Raw Html)", 
    typeof(StaticTextWidgetProperties),
    customViewName: "/Components/Widgets/StaticTextWidget/StaticTextWidget.cshtml",
    Description = "This widget can be used to place plain text content onto the page.",
    IconClass = "icon-w-static-text")]

namespace Generic.Components.Widgets.StaticTextWidget
{
    public class StaticTextWidgetProperties : IWidgetProperties, IComponentProperties
    {
        public const string IDENTIFIER = "Generic.StaticText";

        public StaticTextWidgetProperties()
        {

        }

        [EditingComponent(TextAreaComponent.IDENTIFIER, Order = 0, Label = "Text")]
        public string Text { get; set; } = string.Empty;
    }
}
