using CMS.PortalEngine;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class InlineWidgetPortion
    {
        public bool IsInlineWidget { get; set; }
        public string Value { get; set; }
        public string WidgetName { get; set; }
        public WebPartInstance WidgetInstance { get; set; }
    }
}
