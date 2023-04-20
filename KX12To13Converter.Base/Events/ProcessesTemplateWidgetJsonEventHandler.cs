using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;

namespace KX12To13Converter.Base.Events
{
    public class ProcessesTemplateWidgetJsonEventHandler : AdvancedHandler<ProcessesTemplateWidgetJsonEventHandler, ProcessesTemplateWidgetJsonEventArgs>
    {
        public ProcessesTemplateWidgetJsonEventHandler()
        {

        }

        public ProcessesTemplateWidgetJsonEventHandler StartEvent(ProcessesTemplateWidgetJsonEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
