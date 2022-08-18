using CMS.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalToMVCProcessEditableAreaEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs { get; internal set; }
        /// <summary>
        /// Source Portal Engine Webpart Zone
        /// </summary>
        public PortalWebpartZone PortalEditableArea { get; internal set; }

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's Identifier to null / empty / IGNORE to not processes
        /// </summary>
        public EditableAreaConfiguration EditableAreaConfiguration { get; set; }

        /// <summary>
        /// The current Portal Engine Template Configuration. (May be null if no configuration provided)
        /// Can find the webpart zone info through TemplateConfiguration.Zones.Where(x => x.PE_WebpartZoneName.Equals(peZoneID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()
        /// </summary>
        public TemplateConfiguration TemplateConfiguration { get; internal set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool ZoneHandled { get; set; } = false;
        
    }
}
