using CMS.Base;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;

namespace KX12To13Converter.Base.Events
{
    public class BuildPageFindSectionWebpartEventHandler : AdvancedHandler<BuildPageFindSectionWebpartEventHandler, PortalToMVCBuildPageFindSectionWebpartEventArgs>
    {
        public BuildPageFindSectionWebpartEventHandler()
        {

        }

        public BuildPageFindSectionWebpartEventHandler StartEvent(PortalToMVCBuildPageFindSectionWebpartEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
