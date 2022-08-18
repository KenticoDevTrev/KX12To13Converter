using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{

    [Serializable]
    public class ConverterWidgetConfiguration
    {
        public PortalEngineWidget PE_Widget { get; set; }
        public PageBuilderWidget PB_Widget { get; set; }

    }

    [Serializable]
    public class PortalEngineWidget
    {
        public string PE_WidgetCodeName { get; set; }
        public bool IsInlineWidget { get; set; } = false;
        public bool IsEditorWidget { get; set; } = false;
        public bool IncludeHtmlBeforeAfter { get; set; } = false;
        public bool IncludeWebpartContainerProperties { get; set; } = false;
        public bool RenderIfInline { get; set; } = false;
        public List<InKeyValueOutKeyValues> KeyValues { get; set; }
    }

    [Serializable]
    public class PageBuilderWidget
    {
        public string PB_WidgetIdentifier { get; set; }
        
        /// <summary>
        /// Additional ones not listed in the PortalEngineWidget keyvalues
        /// </summary>
        public Dictionary<string, string> AdditionalKeyValues { get; set; }
    }
}