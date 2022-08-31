using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
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
            // Only processes published, non linked node documents that have their culture the same as the site.
            if (!document.NodeIsContentOnly && document.IsPublished && document.NodeLinkedNodeID <= 0 && GetSiteCultures(document.NodeSiteName).Contains(document.DocumentCulture.ToLower()))
            {
                // Adds a marked record for scheduled task to handle
                KX12To13Queues.AddConversionToQueue(document.DocumentID, true);
            }
        }

        private List<string> GetSiteCultures(string siteName)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[] { $"{SiteInfo.OBJECT_TYPE}|all", $"{CultureInfo.OBJECT_TYPE}|all", $"{CultureSiteInfo.OBJECT_TYPE}|all" });
                }

                return CultureSiteInfoProvider.GetSiteCultures(siteName).Select(x => x.CultureCode.ToLower()).ToList();
            }, new CacheSettings(30, "GetSiteCultureKX12To13", siteName));
        }
    }
}
