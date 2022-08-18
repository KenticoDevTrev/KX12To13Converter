using CMS.Base;
using CMS.PortalEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PortalToMVCProcessTemplateEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs { get; internal set; }

        /// <summary>
        /// Source Portal Engine Template
        /// </summary>
        public PageTemplateInfo PortalEngineTemplate { get; internal set; }
        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's Identifier to null / empty / IGNORE to not processes
        /// </summary>
        public PageTemplateConfiguration PageBuilderTemplate { get; set; }

        /// <summary>
        /// Configurations Available, for reference only.  
        /// Can find proper config through TemplateConfigurations.Where(x => x.PE_CodeName.Equals(PortalEngineTemplate.CodeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        /// </summary>
        public ReadOnlyCollection<TemplateConfiguration> TemplateConfigurations { get; internal set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool TemplateHandled { get; set; }
    }
}
