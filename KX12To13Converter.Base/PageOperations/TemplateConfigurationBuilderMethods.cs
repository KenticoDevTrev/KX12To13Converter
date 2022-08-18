using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.PageOperations
{
    public class TemplateConfigurationBuilderMethods
    {
        public static IEnumerable<int> GetTemplateIdsByCodeName(IEnumerable<string> templateCodeNames)
        {
            return PageTemplateInfoProvider.GetTemplates()
                .WhereIn("PageTemplateCodeName", templateCodeNames.ToArray())
                .Columns("PageTemplateID")
                .TypedResult.Select(x => x.PageTemplateId);
        }

        public static IEnumerable<int> GetCurrentTemplateIds()
        {
            return ConnectionHelper.ExecuteQuery(@"select distinct TemplateID from (
select COALESCE(DocumentPageTemplateID, NodeTemplateID) as TemplateID from View_CMS_Tree_Joined where
DocumentIsArchived = 0 and DocumentCanBePublished = 1 and COALESCE(DocumentPublishFrom, @minDate) < GETDATE() and COALESCE(DocumentPublishTo, @maxDate) > GETDATE()
) templates where TemplateID is not null", new QueryDataParameters() { { "@minDate", DateTimeHelper.ZERO_TIME }, { "@maxDate", DateTime.MaxValue } }, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x["TemplateID"]);
        }

        public static List<TemplateConfiguration> GetTemplateConfigurations(IEnumerable<int> currentTemplateIds, IEnumerable<int> includedTemplateIds, IEnumerable<int> excludedTemplateIds)
        {
            List<TemplateConfiguration> templateConfigurations = new List<TemplateConfiguration>();

            foreach (var template in PageTemplateInfoProvider.GetTemplates()
                .WhereIn(nameof(PageTemplateInfo.PageTemplateId), currentTemplateIds.Union(includedTemplateIds).Except(excludedTemplateIds).Distinct().ToArray())
                .OrderBy("PageTemplateCodeName").TypedResult)
            {
                var templateInstance = new PageTemplateInstance(template.WebParts);

                TemplateConfiguration templateConfiguration = new TemplateConfiguration()
                {
                    PE_CodeName = template.CodeName,
                    PB_Template = new PageBuilderTemplate()
                    {
                        PB_TemplateIdentifier = "INHERIT",
                        PB_KeyValuePairs = new Dictionary<string, string>()
                        {
                            { "IGNORE", String.Empty }
                        }
                    },
                    Zones = new List<WebpartZoneConfiguration>()
                };

                // Find all widget zones
                foreach (var zone in templateInstance.WebPartZones.Where(x => x.WidgetZoneType == WidgetZoneTypeEnum.Editor))
                {
                    WebpartZoneConfiguration zoneConfiguration = new WebpartZoneConfiguration()
                    {
                        PE_WebpartZoneName = zone.ZoneID,
                        PB_EditableAreaName = "INHERIT"
                    };
                    templateConfiguration.Zones.Add(zoneConfiguration);
                }
                foreach (var editableText in templateInstance.WebPartZones.SelectMany(x => x.WebParts.Where(y => !y.IsWidget && y.WebPartType.Equals("editabletext", StringComparison.OrdinalIgnoreCase))))
                {
                    WebpartZoneConfiguration zoneConfiguration = new WebpartZoneConfiguration()
                    {
                        PE_WebpartZoneName = editableText.ControlID,
                        PB_EditableAreaName = "INHERIT",
                        isEditableText = true
                    };
                    templateConfiguration.Zones.Add(zoneConfiguration);
                }
                foreach (var editableImage in templateInstance.WebPartZones.SelectMany(x => x.WebParts.Where(y => !y.IsWidget && y.WebPartType.Equals("EditableImage", StringComparison.OrdinalIgnoreCase))))
                {
                    WebpartZoneConfiguration zoneConfiguration = new WebpartZoneConfiguration()
                    {
                        PE_WebpartZoneName = editableImage.ControlID,
                        PB_EditableAreaName = "INHERIT",
                        isEditableImage = true
                    };
                    templateConfiguration.Zones.Add(zoneConfiguration);
                }


                templateConfigurations.Add(templateConfiguration);
            }
            return templateConfigurations;
        }
    }
}
