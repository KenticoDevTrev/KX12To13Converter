namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class ConversionNote
    {
        public bool IsError { get; set; } = false;
        public string Type { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }
    }
}
