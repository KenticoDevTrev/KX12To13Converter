using KX12To13Converter.Base.Events;

namespace KX12To13Converter.Events
{
    public static class PortalToMVCEvents
    {
        public static BuildPageFindSectionWebpartEventHandler FindParentSectionWebpart;

        public static ProcessPageEventHandler ProcessPage;

        public static ProcessTemplateEventHandler ProcessTemplate;

        public static ProcessEditableAreaEventHandler ProcessEditableArea;

        public static ProcessSectionEventHandler ProcessSection;

        public static ProcessSectionZoneEventHandler ProcessSectionZone;

        public static ProcessWidgetEventHandler ProcessWidget;
        static PortalToMVCEvents()
        {
            FindParentSectionWebpart = new BuildPageFindSectionWebpartEventHandler()
            {
                Name = "PortalToMVCEvents.FindParentSectionWebpart"
            };

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

            ProcessWidget = new ProcessWidgetEventHandler()
            {
                Name = "PortalToMVCEvents.ProcessWidget"
            };
        }


    }
}
