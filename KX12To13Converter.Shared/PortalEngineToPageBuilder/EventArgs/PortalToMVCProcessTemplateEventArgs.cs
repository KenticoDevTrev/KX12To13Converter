using CMS.Base;
using CMS.PortalEngine;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System.Collections.ObjectModel;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCProcessTemplateEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessTemplateEventArgs() { }
        public PortalToMVCProcessTemplateEventArgs(PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCProcessDocumentPrimaryEventArgs,
            PageTemplateInfo pageTemplateInfo,
            ReadOnlyCollection<TemplateConfiguration> templateConfigurations)
        {
            PrimaryEventArgs = portalToMVCProcessDocumentPrimaryEventArgs;
            PortalEngineTemplate = pageTemplateInfo;
            TemplateConfigurations = templateConfigurations;
        }
        public readonly PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs;

        /// <summary>
        /// Source Portal Engine Template
        /// </summary>
        public readonly PageTemplateInfo PortalEngineTemplate;
        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's Identifier to null / empty / IGNORE to not processes
        /// </summary>
        public PageTemplateConfiguration PageBuilderTemplate { get; set; }

        /// <summary>
        /// Configurations Available, for reference only.  
        /// Can find proper config through TemplateConfigurations.Where(x => x.PE_CodeName.Equals(PortalEngineTemplate.CodeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        /// </summary>
        public readonly ReadOnlyCollection<TemplateConfiguration> TemplateConfigurations;

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool TemplateHandled { get; set; }
    }
}
