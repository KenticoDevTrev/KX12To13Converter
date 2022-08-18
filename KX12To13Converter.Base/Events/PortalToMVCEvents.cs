using KX12To13Converter.Base.Events;

namespace KX12To13Converter
{
    public static class PortalToMVCEvents
    {
        public static ProcessPageEventHandler ProcessPage;

        public static ProcessTemplateEventHandler ProcessTemplate;

        public static ProcessEditableAreaEventHandler ProcessEditableArea;

        public static ProcessSectionEventHandler ProcessSection;

        public static ProcessSectionZoneEventHandler ProcessSectionZone;

        public static ProcessWidgetEventHandler ProcessWidgetZone;
        static PortalToMVCEvents()
        {
            ProcessPage = new ProcessPageEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessPage"
            };

            ProcessTemplate = new ProcessTemplateEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessTemplate"
            };

            ProcessEditableArea = new ProcessEditableAreaEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessEditableArea"
            };

            ProcessSection = new ProcessSectionEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessSection"
            };

            ProcessSectionZone = new ProcessSectionZoneEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessSectionZone"
            };

            ProcessWidgetZone = new ProcessWidgetEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessWidgetZone"
            };
        }


    }
}
