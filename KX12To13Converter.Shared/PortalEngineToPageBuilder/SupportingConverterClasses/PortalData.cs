using CMS.PortalEngine;
using System.Collections.Generic;

namespace KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalData
    {

        public PortalData() { }

        public PortalData(PageTemplateInfo pageTemplateInfo)
        {
            Template = pageTemplateInfo;
        }

        /// <summary>
        /// The original Portal Engine Page Template
        /// </summary>
        public readonly PageTemplateInfo Template;
        
        /// <summary>
        /// The Portal Webpart Zones in this template
        /// </summary>
        public List<PortalWebpartZone> TemplateZones { get; set; } = new List<PortalWebpartZone>();

    }
}
