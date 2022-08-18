using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    [Serializable]
    public class SectionsConfiguration
    {
        public List<ConverterSectionConfiguration> Sections { get; set; }
    }

    [Serializable]
    public class ConverterSectionConfiguration
    {
        public PortalEngineWidgetZoneSection PE_WidgetZoneSection { get; set; }
        public PageBuilderSection PB_Section { get; set; }
    }

    [Serializable]
    public class PortalEngineWidgetZoneSection
    {
        /// <summary>
        /// Used only so you can dictate which section to put page builder widgets for those who have no layout widget container
        /// </summary>
        public bool IsDefault { get; set; } = true;
        public string SectionWidgetCodeName { get; set; }
        public List<InKeyValueOutKeyValues> KeyValues { get; set; }
    }

    [Serializable]
    public class PageBuilderSection
    {
        public string SectionIdentifier { get; set; }
        
        /// <summary>
        /// These are not linked to existing Key/Values on the widget zone properties
        /// </summary>
        public Dictionary<string, string> AdditionalKeyValues { get; set; }
    }

    
}