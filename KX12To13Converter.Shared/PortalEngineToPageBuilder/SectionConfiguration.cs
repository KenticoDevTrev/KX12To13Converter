using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System;
using System.Collections.Generic;

namespace KX12To13Converter.PortalEngineToPageBuilder
{
    [Serializable]
    public class SectionsConfiguration
    {
        /// <summary>
        /// All the sections in the given zone
        /// </summary>
        public List<ConverterSectionConfiguration> Sections { get; set; }
    }

    [Serializable]
    public class ConverterSectionConfiguration
    {
        /// <summary>
        /// The Portal Engine Layout Widget info
        /// </summary>
        public PortalEngineWidgetZoneSection PE_WidgetZoneSection { get; set; }

        /// <summary>
        /// The Page Builder Section to convert to
        /// </summary>
        public PageBuilderSection PB_Section { get; set; }
    }

    [Serializable]
    public class PortalEngineWidgetZoneSection
    {
        /// <summary>
        /// Used only so you can dictate which section to put page builder widgets for those who have no layout widget container
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// The Layout Widget's Code Name if it isn't default
        /// </summary>
        public string SectionWidgetCodeName { get; set; }

        /// <summary>
        /// Properties visible on the layout widget
        /// </summary>
        public List<InKeyValueOutKeyValues> KeyValues { get; set; }
    }

    [Serializable]
    public class PageBuilderSection
    {
        /// <summary>
        /// The Section's Identifier (string)
        /// </summary>
        public string SectionIdentifier { get; set; }
        
        /// <summary>
        /// These are not linked to existing Key/Values on the widget zone properties
        /// </summary>
        public Dictionary<string, string> AdditionalKeyValues { get; set; }
    }

    
}