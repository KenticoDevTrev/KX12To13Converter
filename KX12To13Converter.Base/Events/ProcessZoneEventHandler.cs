using CMS.Base;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses;

namespace KX12To13Converter.Base.Events
{
    public class ProcessEditableAreaEventHandler : AdvancedHandler<ProcessEditableAreaEventHandler, PortalToMVCProcessEditableAreaEventArgs>
    {
        public ProcessEditableAreaEventHandler()
        {

        }

        public ProcessEditableAreaEventHandler StartEvent(PortalToMVCProcessEditableAreaEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
