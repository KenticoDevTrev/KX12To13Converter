using CMS.Base;
using CMS.DocumentEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PortalToMVCProcessDocumentPrimaryEventArgs : CMSEventArgs
    {
        public TreeNode Page { get; set; }
        /// <summary>
        /// If set to true, the operation aborts
        /// </summary>
        public bool CancelOperation { get; set; } = false;

        /// <summary>
        /// If set to true, indicates that any additional processing logic can be skipped and it's ready to be saved
        /// </summary>
        public bool Handled { get; set; } = false;

        /// <summary>
        /// Information about the PortalEngine objects
        /// </summary>
        public PortalData PortalEngineData { get; internal set; }

        /// <summary>
        /// Page Builder (MVC) data, this is modifiable and ultimately what is saved to the database
        /// </summary>
        public PageBuilderData PageBuilderData { get; set; } = new PageBuilderData();

        /// <summary>
        /// Notes on the conversion, this will be saved to 
        /// </summary>
        public List<ConversionNote> ConversionNotes { get; set; } = new List<ConversionNote>();
    }
}
