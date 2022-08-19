using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;

namespace KX12To13Converter.Base.Events
{
    public class ProcessSectionEventHandler : AdvancedHandler<ProcessSectionEventHandler, PortalToMVCProcessWidgetSectionEventArgs>
    {
        public ProcessSectionEventHandler()
        {

        }

        public ProcessSectionEventHandler StartEvent(PortalToMVCProcessWidgetSectionEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
