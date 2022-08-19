using System;

namespace KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses
{
    /// <summary>
    /// Request sent from KX12 to update a document on KX13
    /// </summary>
    public class KX12To13ConversionRequest
    {
        public DateTime lastModified { get; set; }
        public Guid nodeGuid { get; set; }
        public string nodeAliasPath { get; set; }
        public string siteCodeName { get; set; }
        public string documentCulture { get; set; }
        public Guid documentGuid { get; set; }
        public string documentPageTemplateConfiguration { get; set; }
        public string documentPageBuilderWidgets { get; set; }
        public string noonce { get; set; }
    }
}
