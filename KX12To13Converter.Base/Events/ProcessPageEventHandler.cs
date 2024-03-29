﻿using CMS.Base;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;

namespace KX12To13Converter.Base.Events
{
    public class ProcessPageEventHandler : AdvancedHandler<ProcessPageEventHandler, PortalToMVCProcessDocumentPrimaryEventArgs>
    {
        public ProcessPageEventHandler()
        {

        }

        public ProcessPageEventHandler StartEvent(PortalToMVCProcessDocumentPrimaryEventArgs Args)
        {
            return base.StartEvent(Args);
        }
        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
