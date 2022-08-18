using CMS.Base;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses;

namespace KX12To13Converter.Base.Events
{
    public class ProcessWidgetEventHandler : AdvancedHandler<ProcessWidgetEventHandler, PortalToMVCProcessWidgetEventArgs>
    {
        public ProcessWidgetEventHandler()
        {

        }

        public ProcessWidgetEventHandler StartEvent(PortalToMVCProcessWidgetEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
