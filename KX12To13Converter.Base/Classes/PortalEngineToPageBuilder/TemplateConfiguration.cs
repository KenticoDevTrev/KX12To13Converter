using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    [Serializable]
    public class TemplateConfiguration
    {
        public string PE_CodeName { get; set; }
        public PageBuilderTemplate PB_Template { get; set; }

        public List<WebpartZoneConfiguration> Zones { get; set; } = new List<WebpartZoneConfiguration>();

    }

    [Serializable]
    public class PageBuilderTemplate
    {
        public string PB_TemplateIdentifier { get; set; }
        public Dictionary<string, string> PB_KeyValuePairs { get; set; }
    }

    [Serializable]
    public class WebpartZoneConfiguration
    {
        /// <summary>
        /// Portal Engine Webpart Zone Name.  In case of Editable Text / Editable Image, this will be the webpart's ID of the Editable Image / Editable Text
        /// </summary>
        public string PE_WebpartZoneName { get; set; }

        /// <summary>
        /// The Page Builder Editable Area name.  If INHERIT then will look for an editable area with the same name.  
        /// 
        /// Can map multiple Webpart Zones to a single Editable Area Zone if you wish, all widgets will be appended in that one zone.
        /// </summary>
        public string PB_EditableAreaName { get; set; } = "INHERIT";
        /// <summary>
        /// If true, then this 'zone' will add a Rich Text Widget to the PB_EditableAreaName during processing
        /// </summary>
        public bool isEditableText { get; set; } = false;
        /// <summary>
        /// If true, then this 'zone' will add a Image Widget to the PB_EditableAreaName during processing
        /// </summary>
        public bool isEditableImage { get; set; } = false;

    }
}