using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.GlobalEvents;
using KX12To13Converter.Base.PageOperations;
using KX12To13Converter.Base.QueueProcessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterModule(typeof(KX12To13ConverterEventsModule))]

namespace KX12To13Converter.Base.GlobalEvents
{
    public class KX12To13ConverterEventsModule : Module
    {
        public KX12To13ConverterEventsModule() : base("KX12To13ConverterEventsModule") { }
        protected override void OnInit()
        {
            base.OnInit();

            DocumentEvents.Update.After += Document_Update_After;
        }

        private void Document_Update_After(object sender, DocumentEventArgs e)
        {
            var document = e.Node;
            if (document.IsPublished)
            {
                // Adds a marked record for scheduled task to handle
                KX12To13Queues.AddConversionToQueue(document.DocumentID, true);

            }
        }
    }
}
