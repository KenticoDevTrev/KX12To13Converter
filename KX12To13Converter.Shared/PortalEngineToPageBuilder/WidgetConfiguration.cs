using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System;
using System.Collections.Generic;

namespace KX12To13Converter.PortalEngineToPageBuilder
{

    [Serializable]
    public class ConverterWidgetConfiguration
    {
        /// <summary>
        /// The Portal Engine Widget Data
        /// </summary>
        public PortalEngineWidget PE_Widget { get; set; }

        /// <summary>
        /// The Page Builder Widget Data
        /// </summary>
        public PageBuilderWidget PB_Widget { get; set; }

    }

    [Serializable]
    public class PortalEngineWidget
    {
        /// <summary>
        /// The Portal Engine Widget Code Name
        /// </summary>
        public string PE_WidgetCodeName { get; set; }

        /// <summary>
        /// If this is a widget used as an inline widget
        /// </summary>
        public bool IsInlineWidget { get; set; } = false;

        /// <summary>
        /// If this is a widget that was used in a widget zone
        /// </summary>
        public bool IsEditorWidget { get; set; } = false;

        /// <summary>
        /// If this webpart should include ContentBefore / ContentAfter properties, your KX13 page builder widget can leverage PageBuilderContainers.Kentico (Admin) + PageBuilderContainers.Kentico.MVC.Core to use this, or just set the HtmlBefore / HtmlAfter to a custom property
        /// </summary>
        public bool IncludeHtmlBeforeAfter { get; set; } = false;

        /// <summary>
        /// If this webpart should include Container properties (Container, ContainerTitle, ContainerCssClass, ContainerCustomContent), your KX13 page builder widget can leverage PageBuilderContainers.Kentico (Admin) + PageBuilderContainers.Kentico.MVC.Core to use this
        /// </summary>
        public bool IncludeWebpartContainerProperties { get; set; } = false;

        /// <summary>
        /// If this widget should render if it's inline.  If set to FALSE then even if it's used inline it won't render.
        /// </summary>
        public bool RenderIfInline { get; set; } = false;
        
        /// <summary>
        /// Widget properties
        /// </summary>
        public List<InKeyValueOutKeyValues> KeyValues { get; set; }
    }

    [Serializable]
    public class PageBuilderWidget
    {
        /// <summary>
        /// The Page Builder Widget Identifier
        /// </summary>
        public string PB_WidgetIdentifier { get; set; }
        
        /// <summary>
        /// Additional ones not listed in the PortalEngineWidget keyvalues
        /// </summary>
        public Dictionary<string, string> AdditionalKeyValues { get; set; }
    }
}