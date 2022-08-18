using CMS.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalToMVCProcessWidgetSectionZoneEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs { get; internal set; }

        /// <summary>
        /// The Source Portal Engine Layout Widget
        /// </summary>
        public PortalWidgetSection PortalSectionWidget { get; internal set; }

        /// <summary>
        /// The Page Builder Section Containing this zone
        /// </summary>
        public SectionConfiguration PageBuilderSection { get; internal set; }

        /// <summary>
        /// The index of the zone
        /// </summary>
        public int ZoneIndex { get; internal set; }

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's Name to IGNORE to not processes
        /// </summary>
        public ZoneConfiguration PageBuilderZoneConfig { get; set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool SectionZoneHandled { get; set; } = false;
    }
}
