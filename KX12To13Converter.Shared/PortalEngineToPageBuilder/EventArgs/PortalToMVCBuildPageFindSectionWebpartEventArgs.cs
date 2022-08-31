using CMS.Base;
using CMS.PortalEngine;
using System.Collections;
using System.Collections.Generic;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCBuildPageFindSectionWebpartEventArgs : CMSEventArgs
    {
        public PortalToMVCBuildPageFindSectionWebpartEventArgs() { }
        public PortalToMVCBuildPageFindSectionWebpartEventArgs(WebPartZoneInstance childZone, IEnumerable<WebPartInstance> allLayoutWebparts) {
            ChildZone = childZone;
            AllLayoutWebparts = allLayoutWebparts;
        }

        /// <summary>
        /// The Zone we need to find the parent for
        /// </summary>
        public readonly WebPartZoneInstance ChildZone;

        /// <summary>
        /// List of all the Layout Webparts found on the document
        /// </summary>
        public readonly IEnumerable<WebPartInstance> AllLayoutWebparts;

        /// <summary>
        /// SET This value with the found WebpartInstance from the AllLayoutWebparts
        /// </summary>
        public WebPartInstance ParentLayoutWebpart { get; set; }

        /// <summary>
        /// Set to true if you handled this.
        /// </summary>
        public bool Handled { get; set; } = false;

    }
}
