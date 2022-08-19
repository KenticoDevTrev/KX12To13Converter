using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;

namespace KX12To13Converter.Base.Events
{
    public class ProcessTemplateEventHandler : AdvancedHandler<ProcessTemplateEventHandler, PortalToMVCProcessDocumentPrimaryEventArgs>
    {
        public ProcessTemplateEventHandler()
        {

        }

        public ProcessTemplateEventHandler StartEvent(PortalToMVCProcessDocumentPrimaryEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
