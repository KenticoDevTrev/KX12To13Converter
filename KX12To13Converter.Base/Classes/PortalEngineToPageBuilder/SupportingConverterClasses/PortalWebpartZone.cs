using CMS.PortalEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PortalWebpartZone
    {
        /// <summary>
        /// What 'type' the zone is.  If WebpartZone, then it's a normal widget zone and teh "WebpartZoneInstance" will have the information.  If EditableText or EditableImage, then 
        /// (which are treated as individual widget zones during conversion) then the EditableWebpartInstance will have the information.
        /// </summary>
        public WebpartType ZoneType { get; set; }

        /// <summary>
        /// The WebpartZoneInstance for Webpart zone types
        /// </summary>
        public WebPartZoneInstance WebpartZoneInstance { get; set; }
        /// <summary>
        /// The WebPartInstance for EditableText / EditableImage zone types.
        /// </summary>
        public WebPartInstance EditableWebpartInstance { get; set; }

        /// <summary>
        /// Widget "Sections", keep in mind that sections that contain multiple zones (
        /// </summary>
        public List<PortalWidgetSection> WidgetSections { get; set; } = new List<PortalWidgetSection>();


    }
}
