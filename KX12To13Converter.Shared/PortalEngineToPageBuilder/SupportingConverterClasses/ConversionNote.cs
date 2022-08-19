namespace KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses
{
    /// <summary>
    /// Shown in the Conversions UI to help give feedback on conversion issues
    /// </summary>
    public class ConversionNote
    {
        /// <summary>
        /// If the note is reporting an error.  You may want to consider cancelling the operation as well if a fatal error
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// The Type of the error, whatever you wish to put
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The Source of the error, whatever you wish to put.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Your description
        /// </summary>
        public string Description { get; set; }
    }
}
