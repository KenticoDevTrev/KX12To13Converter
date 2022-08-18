using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses
{
    /* Cloned from Kentico code for serialization */

    [DataContract(Namespace = "", Name = "Configuration")]
    public sealed class EditableAreasConfiguration
    {
        /// <summary>
        /// Editable areas within the page.
        /// </summary>
        [DataMember]
        [JsonProperty("editableAreas")]
        public List<EditableAreaConfiguration> EditableAreas { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="EditableAreasConfiguration"/> class.
        /// </summary>
        public EditableAreasConfiguration()
        {
            EditableAreas = new List<EditableAreaConfiguration>();
        }
    }

    /// <summary>
    /// Represents configuration of editable area within the <see cref="TreeNode"/> instance.
    /// </summary>
    public sealed class EditableAreaConfiguration
    {
        /// <summary>
        /// Identifier of the editable area.  If null, empty, or IGNORE then will not processes
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public string Identifier { get; set; }


        /// <summary>
        /// Sections within editable area.
        /// </summary>
        [DataMember]
        [JsonProperty("sections")]
        public List<SectionConfiguration> Sections { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="EditableAreasConfiguration"/> class.
        /// </summary>
        public EditableAreaConfiguration()
        {
            Sections = new List<SectionConfiguration>();
        }
    }

    /// <summary>
    /// Represents configuration of section within the <see cref="EditableAreaConfiguration"/> instance.
    /// </summary>
    public sealed class SectionConfiguration
    {
        /// <summary>
        /// Identifier of the section.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Type section identifier.  if empty or IGNORE will not processes
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string TypeIdentifier { get; set; }


        /// <summary>
        /// Section properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }
        //public ISectionProperties Properties { get; set; }

        /// <summary>
        /// Zones within the section.
        /// </summary>
        [DataMember]
        [JsonProperty("zones")]
        public List<ZoneConfiguration> Zones { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="EditableAreasConfiguration"/> class.
        /// </summary>
        public SectionConfiguration()
        {
            Zones = new List<ZoneConfiguration>();
        }
    }

    /// <summary>
    /// Represents the zone within the <see cref="EditableAreasConfiguration"/> configuration class.
    /// </summary>
    [DataContract(Namespace = "", Name = "Zone")]
    public sealed class ZoneConfiguration
    {
        /// <summary>
        /// Identifier of the widget zone.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }

        /// <summary>
        /// Only way to "reorder" zones is to set specific zone names and match in KX13
        /// </summary>
        [DataMember]
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// List of widgets within the zone.
        /// </summary>
        [DataMember]
        [JsonProperty("widgets")]
        public List<WidgetConfiguration> Widgets { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="ZoneConfiguration"/> class.
        /// </summary>
        public ZoneConfiguration()
        {
            Widgets = new List<WidgetConfiguration>();
        }
    }
    /// <summary>
    /// Represents the configuration of a widget within the <see cref="ZoneConfiguration.Widgets"/> list.
    /// </summary>
    [DataContract(Namespace = "", Name = "Widget")]
    public sealed class WidgetConfiguration
    {
        /// <summary>
        /// Identifier of the widget instance.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Type widget identifier.
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string TypeIdentifier { get; set; }


        /// <summary>
        /// Personalization condition type identifier.
        /// </summary>
        [DataMember]
        [JsonProperty("conditionType", NullValueHandling = NullValueHandling.Ignore)]
        public string PersonalizationConditionTypeIdentifier { get; set; }


        /// <summary>
        /// List of widget variants.
        /// </summary>
        [DataMember]
        [JsonProperty("variants")]
        public List<WidgetVariantConfiguration> Variants { get; set; }


        /// <summary>
        /// Creates an instance of <see cref="WidgetConfiguration"/> class.
        /// </summary>
        public WidgetConfiguration()
        {
            Variants = new List<WidgetVariantConfiguration>();
        }
    }
    /// <summary>
    /// Represents the configuration variant of a widget within the <see cref="WidgetConfiguration.Variants"/> list.
    /// </summary>
    [DataContract(Namespace = "", Name = "Variant")]
    public sealed class WidgetVariantConfiguration
    {
        /// <summary>
        /// Identifier of the variant instance.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Widget variant name.
        /// </summary>
        [DataMember]
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }


        /// <summary>
        /// Widget variant properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }
        //public IWidgetProperties Properties { get; set; }


        /// <summary>
        /// Widget variant personalization condition type.
        /// </summary>
        /// <remarks>Only personalization condition type parameters are serialized to JSON.</remarks>
        [DataMember]
        [JsonProperty("conditionTypeParameters", NullValueHandling = NullValueHandling.Ignore)]
        public IConditionType PersonalizationConditionType { get; set; }
    }

    /// <summary>
    /// Interface for widget properties.
    /// </summary>
    public interface IWidgetProperties
    {
    }

    public interface IConditionType
    {
        /// <summary>
        /// Evaluate condition type.
        /// </summary>
        /// <returns>Returns <c>true</c> if implemented condition is met.</returns>
        bool Evaluate();
    }

    /// <summary>
    /// Page template configuration for the <see cref="TreeNode"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "PageTemplate")]
    public class PageTemplateConfiguration
    {
        /// <summary>
        /// Identifier of the page template.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public string Identifier { get; set; }


        /// <summary>
        /// Identifier of the page template configuration based on which the page was created.
        /// </summary>
        [DataMember]
        [JsonProperty("configurationIdentifier", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid ConfigurationIdentifier { get; set; }


        /// <summary>
        /// Page template properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        //public IPageTemplateProperties Properties { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    public interface IPageTemplateProperties : IComponentProperties
    {
    }

    /// <summary>
    /// Interface for component properties.
    /// </summary>
    public interface IComponentProperties
    {
    }

}
