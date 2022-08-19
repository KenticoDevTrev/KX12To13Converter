using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System.Collections.ObjectModel;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCProcessWidgetEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessWidgetEventArgs() { }
        
        public PortalToMVCProcessWidgetEventArgs(PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCProcessDocumentPrimaryEventArgs,
            PortalWidget portalWidget,
            ReadOnlyCollection<ConverterWidgetConfiguration> converterWidgetConfigurations
            )
        {
            PrimaryEventArgs = portalToMVCProcessDocumentPrimaryEventArgs;
            PortalEngineWidget = portalWidget;
            WidgetConfigurations = converterWidgetConfigurations;
        }

        public readonly PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs;

        /// <summary>
        /// The Source Portal Engine Widget
        /// </summary>
        public readonly PortalWidget PortalEngineWidget;

        /// <summary>
        /// Configurations Available, for reference only.  
        /// Can find proper config through:  
        /// WidgetConfigurations.Where(x => (x.PE_Widget?.PE_WidgetCodeName ?? Guid.NewGuid().ToString()).Equals(PortalEngineWidget.Widget.WebPartType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        /// </summary>
        public readonly ReadOnlyCollection<ConverterWidgetConfiguration> WidgetConfigurations;

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's TypeIdentifier to null / empty / IGNORE to not processes
        /// </summary>
        public WidgetConfiguration PageBuilderWidget { get; set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool WidgetHandled { get; set; } = false;
    }
}
