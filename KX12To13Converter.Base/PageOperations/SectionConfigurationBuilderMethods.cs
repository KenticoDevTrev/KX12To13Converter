using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace KX12To13Converter.Base.PageOperations
{
    public class SectionConfigurationBuilderMethods
    {
        public static IEnumerable<int> GetSectionWidgetIdsByWidgetName(IEnumerable<string> widgetNames)
        {
            return WidgetInfoProvider.GetWidgets()
                .Source(x => x.Join<WebPartInfo>(nameof(WidgetInfo.WidgetWebPartID), nameof(WebPartInfo.WebPartID)))
                .WhereEquals(nameof(WebPartInfo.WebPartType), WebPartTypeEnum.Layout)
                .WhereIn(nameof(WidgetInfo.WidgetName), widgetNames.ToArray())
                .Columns(nameof(WidgetInfo.WidgetID))
                .TypedResult.Select(x => x.WidgetID);
        }

        public static IEnumerable<int> GetCurrentSectionWidgets()
        {
            return ConnectionHelper.ExecuteQuery(@"select WidgetID from CMS_Widget
left join CMS_WebPart on WebpartID = WidgetWebPartID
where WebPartType = 6 and exists (select 0 from View_CMS_Tree_Joined where DocumentWebParts like '%type=""'+WidgetName+'""%' and 
DocumentIsArchived = 0 and DocumentCanBePublished = 1 and COALESCE(DocumentPublishFrom, @minDate) < GETDATE() and COALESCE(DocumentPublishTo, @maxDate) > GETDATE())", new QueryDataParameters() { { "@minDate", DateTimeHelper.ZERO_TIME }, { "@maxDate", DateTime.MaxValue } }, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x["WidgetID"]);
        }

        public static List<ConverterSectionConfiguration> GetSectionConfigurations(IEnumerable<int> currentWidgetIds, IEnumerable<int> includedWidgetIds, IEnumerable<int> excludedWidgetIds)
        {
            List<ConverterSectionConfiguration> sectionConfigurations = new List<ConverterSectionConfiguration>();

            foreach (var widget in WidgetInfoProvider.GetWidgets()
                .WhereIn(nameof(WidgetInfo.WidgetID), currentWidgetIds.Union(includedWidgetIds).Except(excludedWidgetIds).Distinct().ToArray())
                .OrderBy(nameof(WidgetInfo.WidgetName)).TypedResult)
            {
                ConverterSectionConfiguration sectionConfiguration = new ConverterSectionConfiguration()
                {
                    PE_WidgetZoneSection = new PortalEngineWidgetZoneSection()
                    {
                        SectionWidgetCodeName = widget.WidgetName,
                        KeyValues = new List<InKeyValueOutKeyValues>()
                    },
                    PB_Section = new PageBuilderSection()
                    {
                        AdditionalKeyValues = new Dictionary<string, string>() { { "IGNORE", "" } },
                        SectionIdentifier = widget.WidgetName.StartsWith("ColumnLayout_Bootstrap") ? "Bootstrap4LayoutTool" : "INHERIT"
                    }
                };


                // Loop through publically set fields.
                WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(widget.WidgetWebPartID);
                string widgetProperties = FormHelper.MergeFormDefinitions(wpi.WebPartProperties, widget.WidgetProperties);

                XmlDocument widgetFields = new XmlDocument();
                widgetFields.LoadXml(widgetProperties);
                foreach (XmlNode property in widgetFields.SelectNodes("//field[@visible='true']"))
                {
                    var keyValueItem = new InKeyValueOutKeyValues()
                    {
                        Key = property.Attributes["column"].InnerText,
                        OutKey = "INHERIT",
                        Value = ""
                    };
                    var defaultValue = property.SelectNodes("./properties/defaultvalue");
                    if (defaultValue.Count > 0)
                    {
                        keyValueItem.DefaultValue = defaultValue[0].InnerText;
                    }
                    sectionConfiguration.PE_WidgetZoneSection.KeyValues.Add(keyValueItem);
                }

                sectionConfigurations.Add(sectionConfiguration);
            }
            return sectionConfigurations;
        }

        public static PageBuilderSection GetDefaultSectionConfiguration()
        {
            return new PageBuilderSection()
            {
                SectionIdentifier = "YourDefaultTypeIdentifier",
                AdditionalKeyValues = new Dictionary<string, string>() { { "PropertyName", "PropertyValueIfAny" } }
            };
        }
    }
}
