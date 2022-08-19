using CMS.PortalEngine;
using System.Collections.Generic;

namespace KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalWidget
    {
        /// <summary>
        /// Portal engine allows multiple Layout Widgets to be used in a nested pattern, MVC does not support this, so if there are
        /// additional nested zones they will be included here.  By default the parent PortalWidgetSection is the direct container, and these
        /// are the layout widgets that contain that parent PortalWidgetSection
        /// </summary>
        public List<WebPartInstance> AdditionalAncestorZones { get; set; } = new List<WebPartInstance>();
        /// <summary>
        /// The widget and it's info, if it's an Editable Text / Editable Image, then the main info is in the adjacent text.
        /// </summary>
        public WebPartInstance Widget { get; set; }
        /// <summary>
        /// What type the widget is
        /// </summary>
        public WebpartType WidgetType { get; set; } = WebpartType.Webpart;
        /// <summary>
        /// If the WidgetType is Editable Image, this is the Url
        /// </summary>
        public string EditableImageUrl { get; set; }
        /// <summary>
        /// If the WidgetType is Editable Image, this is the ImageAlt
        /// </summary>
        public string EditableImageAlt { get; set; }
        /// <summary>
        /// If the WidgetType is Editable Text, this is the content
        /// </summary>
        public string EditableText { get; set; }

        /// <summary>
        /// If set to true, indicates that the Widget conversion was handled and can skip the default widget translation.
        /// </summary>
        public bool WidgetHandled { get; set; } = false;

        /// <summary>
        /// Adds property if value exists, use PageBuilderContainers.Kentico module to leverage or write own custom handling
        /// </summary>
        public string HtmlBefore { get; set; }
        /// <summary>
        /// Adds property if value exists, use PageBuilderContainers.Kentico module to leverage or write own custom handling
        /// </summary>
        public string HtmlAfter { get; set; }
        /// <summary>
        /// Adds property if value exists, use PageBuilderContainers.Kentico module to leverage or write own custom handling
        /// </summary>
        public string ContainerCSSClass { get; set; }
        /// <summary>
        /// Adds property if value exists, use PageBuilderContainers.Kentico module to leverage or write own custom handling
        /// </summary>
        public string ContainerCustomContent { get; set; }
        /// <summary>
        /// Adds property if value exists, use PageBuilderContainers.Kentico module to leverage or write own custom handling
        /// </summary>
        public string ContainerName { get; set; }
        /// <summary>
        /// Adds property if value exists, use PageBuilderContainers.Kentico module to leverage or write own custom handling
        /// </summary>
        public string ContainerTitle { get; set; }

    }
}
