using Kentico.Forms.Web.Mvc;
using Kentico.Components.Web.Mvc.FormComponents;
using Generic.Components.Widgets.StaticImageWidget;
using PageBuilderContainers;

[assembly: RegisterWidget(StaticImageWidgetProperties.IDENTIFIER, "Static Image",
    typeof(StaticImageWidgetProperties),
    customViewName: "/Components/Widgets/StaticImageWidget/StaticImageWidget.cshtml",
    Description = "Allows content editors to select a single image through the widget configuration.",
    IconClass = "icon-w-editable-image")]
namespace Generic.Components.Widgets.StaticImageWidget
{
    public class StaticImageWidgetProperties : PageBuilderWidgetProperties, IWidgetProperties
    {
        public const string IDENTIFIER = "Generic.StaticImage";

        [EditingComponent(UrlSelector.IDENTIFIER, Label = "Image URL", Order = 1)]
        [EditingComponentProperty(nameof(UrlSelectorProperties.Tabs), ContentSelectorTabs.Attachment | ContentSelectorTabs.Media)]
        [EditingComponentProperty(nameof(UrlSelectorProperties.MediaAllowedExtensions), ".gif;.png;.jpg;.jpeg")]
        [EditingComponentProperty(nameof(UrlSelectorProperties.AttachmentAllowedExtensions), ".gif;.png;.jpg;.jpeg")]
        public string Image { get; set; } = string.Empty;

        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Image Title", Order = 2)]
        public string? ImageTitle { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Image Alternative Text", Order = 3)]
        public string? AlternativeText { get; set; }

        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "Image Width", Order = 4)]
        public int? ImageWidth { get; set; }

        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "Image Height", Order = 5)]
        public int? ImageHeight { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Image CSS Class", Order = 6)]
        public string? ImageCssClass { get; set; }

        [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "Image Style", Order = 7)]
        public string? InlineStyle { get; set; }

        public StaticImageWidgetProperties()
        {

        }
    }
}
