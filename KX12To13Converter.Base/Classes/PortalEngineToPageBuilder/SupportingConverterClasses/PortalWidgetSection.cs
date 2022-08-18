using CMS.PortalEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PortalWidgetSection
    {
        /// <summary>
        /// True if the widgets within here did not have any Layout Widget containers
        /// </summary>
        public bool IsDefault { get; set; } = false;
        /// <summary>
        /// The Layout widget code name surrounding these widgets, will be null if IsDefault is true
        /// </summary>
        public WebPartInstance SectionWidget { get; set; }

        /// <summary>
        /// If set to true, indicates that the Widget section conversion was handled and can skip the default widget zone translation.
        /// </summary>
        public bool WidgetSectionHandled { get; set; } = false;

        /// <summary>
        /// Sections can contain multiple zones.  There's no zone names in Portal Engine, so just handled incrementally (1st, 2nd, 3rd, etc)
        /// If you wish to map the nth section to a named zone, you'll have to handle that with custom event hooks or logic.
        /// </summary>
        public List<List<PortalWidget>> SectionZoneToWidgets { get; set; } = new List<List<PortalWidget>>();
    }
}
