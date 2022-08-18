using CMS.Base;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses;

namespace KX12To13Converter.Base.Events
{
    public class ProcessSectionZoneEventHandler : AdvancedHandler<ProcessSectionZoneEventHandler, PortalToMVCProcessWidgetSectionZoneEventArgs>
    {
        public ProcessSectionZoneEventHandler()
        {

        }

        public ProcessSectionZoneEventHandler StartEvent(PortalToMVCProcessWidgetSectionZoneEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
