using CMS.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalToMVCProcessWidgetSectionEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs { get; internal set; }

        /// <summary>
        /// Source Portal Engine Widget Section
        /// </summary>
        public PortalWidgetSection PortalEngineWidgetSection { get; internal set; }

        /// <summary>
        /// Configurations Available, for reference only.  
        /// Can find proper config through:  
        /// PortalEngineWidgetSection.IsDefault ? null : SectionConfigurations.Where(x => (x.PE_WidgetZoneSection?.SectionWidgetCodeName ?? String.Empty).Equals(PortalEngineWidgetSection.IsDefault ? Guid.NewGuid().ToString() : PortalEngineWidgetSection.SectionWidget.WebPartType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        /// </summary>
        public ReadOnlyCollection<ConverterSectionConfiguration> SectionConfigurations { get; internal set; }

        /// <summary>
        /// The Default Section Configuration (if given), used for if there are no sections
        /// </summary>
        public PageBuilderSection DefaultSectionConfiguration { get; internal set; }

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's TypeIdentifier to null / empty / IGNORE to not processes
        /// </summary>
        public SectionConfiguration PageBuilderSection { get; set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool SectionHandled { get; set; } = false;
    }
}
