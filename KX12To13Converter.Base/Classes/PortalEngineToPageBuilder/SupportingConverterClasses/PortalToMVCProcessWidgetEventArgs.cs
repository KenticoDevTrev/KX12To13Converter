using CMS.Base;
using System.Collections.ObjectModel;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalToMVCProcessWidgetEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs { get; internal set; }

        /// <summary>
        /// The Source Portal Engine Widget
        /// </summary>
        public PortalWidget PortalEngineWidget { get; internal set; }

        /// <summary>
        /// Configurations Available, for reference only.  
        /// Can find proper config through:  
        /// WidgetConfigurations.Where(x => (x.PE_Widget?.PE_WidgetCodeName ?? Guid.NewGuid().ToString()).Equals(PortalEngineWidget.Widget.WebPartType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        /// </summary>
        public ReadOnlyCollection<ConverterWidgetConfiguration> WidgetConfigurations { get; internal set; }

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
