using CMS.PortalEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PortalData
    {
        public PageTemplateInfo Template { get; internal set; }
        public List<PortalWebpartZone> TemplateZones { get; set; } = new List<PortalWebpartZone>();

    }
}
