using CMS.Base;
using CMS.DocumentEngine;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System.Collections.Generic;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCProcessDocumentPrimaryEventArgs : CMSEventArgs
    {

        public PortalToMVCProcessDocumentPrimaryEventArgs() { }

        public PortalToMVCProcessDocumentPrimaryEventArgs(PortalData portalData)
        {
            PortalEngineData = portalData;
        }

        public TreeNode Page { get; set; }
        /// <summary>
        /// If set to true, the operation aborts and will mark as failed to convert
        /// </summary>
        private bool _cancelOperation = false;
        public bool CancelOperation
        {
            get
            {
                return _cancelOperation;
            }
            set
            {
                _cancelOperation = value;
                if (value)
                {
                    Handled = true;
                }
            }
        }

        /// <summary>
        /// If set to true, indicates that any additional processing logic can be skipped and it's ready to be saved
        /// </summary>
        public bool Handled { get; set; } = false;

        /// <summary>
        /// Information about the PortalEngine objects
        /// </summary>
        public readonly PortalData PortalEngineData;

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
