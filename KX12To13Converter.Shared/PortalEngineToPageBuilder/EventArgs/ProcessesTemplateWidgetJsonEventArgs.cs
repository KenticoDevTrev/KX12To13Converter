using CMS.Base;
using CMS.DocumentEngine;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class ProcessesTemplateWidgetJsonEventArgs : CMSEventArgs
    {
        public ProcessesTemplateWidgetJsonEventArgs() { }
        
        public ProcessesTemplateWidgetJsonEventArgs(string pageTemplateJson, string widgetConfigurationJson, TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs primaryEventArgs)
        {
            PageTemplateJson = pageTemplateJson;
            WidgetConfigurationJson = widgetConfigurationJson;
            Document = document;
            PrimaryEventArgs = primaryEventArgs;
        }

        /// <summary>
        /// The Page Template JSON String, you can modify this.
        /// </summary>
        public string PageTemplateJson { get; set; }

        /// <summary>
        /// The Widget Configuration JSON String, you can modify this.
        /// </summary>
        public string WidgetConfigurationJson { get; set; }

        /// <summary>
        /// Read only, the document processes info
        /// </summary>
        public readonly PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs;

        /// <summary>
        /// Read only, the document being modified
        /// </summary>
        public readonly TreeNode Document;
    }
}
