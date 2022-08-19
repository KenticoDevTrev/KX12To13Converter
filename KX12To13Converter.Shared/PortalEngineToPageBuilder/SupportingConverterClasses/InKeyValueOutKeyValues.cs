using System;

namespace KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses
{
    [Serializable]
    public class InKeyValueOutKeyValues
    {
        public string Key { get; set; }
        /// <summary>
        /// Value, if not provided then will use the value on the widget
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Default Value, if widget does not have a value then will use this as the default value
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// What the Page Builder value should be to match, IGNORE = do not processes, NULL/INHERIT = Use the Key name
        /// </summary>
        public string OutKey { get;  set; }

        /// <summary>
        /// If true, will check this field's values for inline widgets
        /// </summary>
        public bool CanContainInlineWidgets { get; set; } = false;

        /// <summary>
        /// Possible values are ADDAFTER, SPLIT, WRAP, REMOVE.  ADDAFTER puts the inline widget after the main widget.  SPLIT splits the value into static HTML widgets.  WRAP adds properties of the HtmlBefore and HtmlAfter which is compatible with PageBuilderContainers module (Can only occur if there is 1 inline widget, defaults to ADDAFTER if more than 1). REMOVE removes inline widgets from the content.  
        /// </summary>
        public string InlineWidgetMode { get; set; } = "ADDAFTER";
    }
}