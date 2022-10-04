using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;
using System.Collections.Generic;
using System.Linq;

namespace KX12To13Converter.Events
{
    
    public class ProcessCamelCaseEvents
    {
        /// <summary>
        /// CamelCases Widget property values for you
        /// </summary>
        public static void ProcessWidgetAfter_CamelCase(object sender, PortalToMVCProcessWidgetEventArgs widgetEventArgs)
        {
            var pbwidget = widgetEventArgs.PageBuilderWidget;
            foreach(var variant in pbwidget.Variants)
            {
                var properties = new Dictionary<string, object>();
                foreach (var propertyKey in variant.Properties.Keys.ToList())
                {
                    object value = variant.Properties[propertyKey];
                    properties.Add(char.ToLower(propertyKey[0]) + propertyKey.Substring(1), value);
                }
                variant.Properties = properties;
            }
        }

        /// <summary>
        /// CamelCases section property values for you
        /// </summary>
        public static void ProcessSectionAfter_CamelCase(object sender, PortalToMVCProcessWidgetSectionEventArgs sectionEventArgs)
        {
            var pbSection = sectionEventArgs.PageBuilderSection;
            var properties = new Dictionary<string, object>();
            foreach (var propertyKey in pbSection.Properties.Keys.ToList())
            {
                object value = pbSection.Properties[propertyKey];
                properties.Add(char.ToLower(propertyKey[0]) + propertyKey.Substring(1), value);
            }
            pbSection.Properties = properties;
        }
    }
}
