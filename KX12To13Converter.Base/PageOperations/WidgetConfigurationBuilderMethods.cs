using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace KX12To13Converter.Base.PageOperations
{
    public class WidgetConfigurationBuilderMethods : IWidgetConfigurationBuilderMethods
    {

        public IEnumerable<int> GetWidgetIdsByWidgetName(IEnumerable<string> widgetNames)
        {
            return WidgetInfoProvider.GetWidgets()
                .Source(x => x.Join<WebPartInfo>(nameof(WidgetInfo.WidgetWebPartID), nameof(WebPartInfo.WebPartID)))
                .WhereIn(nameof(WebPartInfo.WebPartType), new int[] { 0, 5, 7 })
                .WhereIn(nameof(WidgetInfo.WidgetName), widgetNames.ToArray())
                .Columns(nameof(WidgetInfo.WidgetID))
                .TypedResult.Select(x => x.WidgetID);
        }

        private Dictionary<bool, IEnumerable<int>> GetIsInlineWidgetToWidgetIds()
        {
            // Better performance by doing in c# land
            var searchContent = ConnectionHelper.ExecuteQuery(@"select lower(coalesce(DocumentWebParts, '')+coalesce(DocumentContent, '')) as ValuesToScan from View_CMS_Tree_Joined where coalesce(nullif(DocumentWebParts, ''), nullif(documentcontent, '')) is not null and DocumentIsArchived = 0 and DocumentCanBePublished = 1 and COALESCE(DocumentPublishFrom, @minDate) < GETDATE() and COALESCE(DocumentPublishTo, @maxDate) > GETDATE()", new QueryDataParameters() { { "@minDate", DateTimeHelper.ZERO_TIME }, { "@maxDate", DateTime.MaxValue } }, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (string)x["ValuesToScan"]);

            var widgetNameToID = WidgetInfoProvider.GetWidgets()
                .Source(x => x.LeftJoin<WebPartInfo>(nameof(WidgetInfo.WidgetWebPartID), nameof(WebPartInfo.WebPartID)))
                .Where($"{nameof(WidgetInfo.WidgetForEditor)} = 1 or {nameof(WidgetInfo.WidgetForInline)} = 1")
                .WhereIn("WebpartType", new int[] { 0, 4, 5, 7 })
                .TypedResult
                .ToDictionary(key => key.WidgetName.ToLower(), value => value.WidgetID);

            List<int> widgetIds = new List<int>();
            List<int> inlineWidgetIds = new List<int>();
            foreach(string widgetName in widgetNameToID.Keys)
            {
                if(searchContent.Any(x => x.IndexOf($"type=\"{widgetName}\"") > -1))
                {
                    widgetIds.Add(widgetNameToID[widgetName]);
                }
                if(searchContent.Any(x => x.IndexOf($"(name){widgetName}") > -1))
                {
                    inlineWidgetIds.Add(widgetNameToID[widgetName]);
                }
                if (searchContent.Any(x => x.IndexOf($"{{^{widgetName}") > -1))
                {
                    inlineWidgetIds.Add(widgetNameToID[widgetName]);
                }
            }
            return new Dictionary<bool, IEnumerable<int>>()
            {
                {
                    true, inlineWidgetIds
                },
                { false, widgetIds }
            };
        }

        public List<ConverterWidgetConfiguration> GetWidgetConfigurations(IEnumerable<int> includedWidgetIds, IEnumerable<int> excludedWidgetIds)
        {
            var inlineToWidgetIds = GetIsInlineWidgetToWidgetIds();
            var widgetIds = inlineToWidgetIds.Values.SelectMany(x => x);


            var widgets = WidgetInfoProvider.GetWidgets()
                .WhereIn(nameof(WidgetInfo.WidgetID), widgetIds.Union(includedWidgetIds).Except(excludedWidgetIds).Distinct().ToArray())
                .OrderBy(nameof(WidgetInfo.WidgetName)).TypedResult;

            // Get list of templates to convert.
            List<ConverterWidgetConfiguration> widgetConfigurations = new List<ConverterWidgetConfiguration>
            {
                // Add default section configuration
                new ConverterWidgetConfiguration()
                {
                    PE_Widget = new PortalEngineWidget()
                    {
                        PE_WidgetCodeName = "statictextWidget",
                        KeyValues = new List<InKeyValueOutKeyValues>()
                        {
                            new InKeyValueOutKeyValues()
                            {
                                Key="Text",
                                OutKey="INHERIT",
                                Value="",
                                DefaultValue=""
                            }
                        }
                    },
                    PB_Widget = new PageBuilderWidget()
                    {
                        PB_WidgetIdentifier="EnterPageWidgetIdentifierIfNotProvidedCantDoInlineWidgets",
                        AdditionalKeyValues = new Dictionary<string, string>()
                        {
                            {"IGNORE", String.Empty }
                        }
                    }
                }
            };

            foreach (var widget in widgets)
            {
                // Already have this as part of the required inline widget configuration
                if (widget.WidgetName.Equals("statictextwidget", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ConverterWidgetConfiguration widgetConfiguration = new ConverterWidgetConfiguration()
                {
                    PE_Widget = new PortalEngineWidget()
                    {
                        PE_WidgetCodeName = widget.WidgetName,
                        IsEditorWidget = inlineToWidgetIds[false].Contains(widget.WidgetID) || (includedWidgetIds.Contains(widget.WidgetID) && widget.WidgetForEditor),
                        IsInlineWidget = inlineToWidgetIds[true].Contains(widget.WidgetID) || (includedWidgetIds.Contains(widget.WidgetID) && widget.WidgetForInline),
                        RenderIfInline = inlineToWidgetIds[true].Contains(widget.WidgetID) || (includedWidgetIds.Contains(widget.WidgetID) && widget.WidgetForInline),
                        KeyValues = new List<InKeyValueOutKeyValues>()
                    },
                    PB_Widget = new PageBuilderWidget()
                    {
                        PB_WidgetIdentifier = "INHERIT",
                        AdditionalKeyValues = new Dictionary<string, string>()
                        {
                            {"IGNORE", String.Empty }
                        }
                    }
                };

                // Editable Text and Editable Image special KeyValues
                if (widget.WidgetName.Equals("editabletext", StringComparison.OrdinalIgnoreCase))
                {
                    widgetConfiguration.PE_Widget.KeyValues.Add(new InKeyValueOutKeyValues()
                    {
                        Key = "EditableText",
                        OutKey = "html",
                        CanContainInlineWidgets = true
                    });
                    widgetConfiguration.PB_Widget.PB_WidgetIdentifier = "RichTextEditor";
                }
                if (widget.WidgetName.Equals("editableimage", StringComparison.OrdinalIgnoreCase))
                {
                    widgetConfiguration.PE_Widget.KeyValues.Add(new InKeyValueOutKeyValues()
                    {
                        Key = "EditableImageUrl",
                        OutKey = "INHERIT"
                    });
                }

                // Loop through publically set fields.
                WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(widget.WidgetWebPartID);
                string widgetProperties = FormHelper.MergeFormDefinitions(wpi.WebPartProperties, widget.WidgetProperties);

                XmlDocument widgetFields = new XmlDocument();
                widgetFields.LoadXml(widgetProperties);

                // Watch out for Content Before / After and 
                var beforeAfterColumns = new string[] { "contentbefore", "contentafter" };
                var containerColumns = new string[] { "container", "containertitle", "containercssclass", "containercustomcontent" };
                bool includeBeforeAfter = false;
                bool includeContainer = false;

                foreach (XmlNode property in widgetFields.SelectNodes("//field[@visible='true']"))
                {
                    string column = property.Attributes["column"].InnerText;
                    if (beforeAfterColumns.Contains(column.ToLowerInvariant()))
                    {
                        includeBeforeAfter = true;
                        var keyValueItem = new InKeyValueOutKeyValues()
                        {
                            Key = property.Attributes["column"].InnerText,
                            OutKey = column.Equals("contentbefore", StringComparison.OrdinalIgnoreCase) ? "htmlBefore" : "htmlAfter",
                            Value = ""
                        };
                        var defaultValue = property.SelectNodes("./properties/defaultvalue");
                        if (defaultValue.Count > 0)
                        {
                            keyValueItem.DefaultValue = defaultValue[0].InnerText;
                        }
                        widgetConfiguration.PE_Widget.KeyValues.Add(keyValueItem);
                    }
                    else if (containerColumns.Contains(column.ToLowerInvariant()))
                    {
                        includeContainer = true;
                        var keyValueItem = new InKeyValueOutKeyValues()
                        {
                            Key = property.Attributes["column"].InnerText,
                            OutKey = column.Equals("container", StringComparison.OrdinalIgnoreCase) ? "columnname" : char.ToLower(column[0]) + column.Substring(1),
                            Value = ""
                        };
                        var defaultValue = property.SelectNodes("./properties/defaultvalue");
                        if (defaultValue.Count > 0)
                        {
                            keyValueItem.DefaultValue = defaultValue[0].InnerText;
                        }
                        widgetConfiguration.PE_Widget.KeyValues.Add(keyValueItem);
                    }
                    else
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
                        widgetConfiguration.PE_Widget.KeyValues.Add(keyValueItem);
                    }
                }

                // Check default values only for html before/after and container
                if (!string.IsNullOrWhiteSpace(widget.WidgetDefaultValues) && (!includeBeforeAfter || !includeContainer))
                {
                    XmlDocument widgetDefaultValues = new XmlDocument();
                    widgetDefaultValues.LoadXml(widget.WidgetDefaultValues);
                    foreach (XmlNode property in widgetDefaultValues.SelectNodes("//field"))
                    {
                        string column = property.Attributes["column"].InnerText;
                        var defaultValue = property.SelectNodes("./properties/defaultvalue");
                        if (defaultValue.Count > 0)
                        {
                            if (beforeAfterColumns.Contains(column.ToLower()) && !includeBeforeAfter)
                            {
                                includeBeforeAfter = true;
                            }
                            if (containerColumns.Contains(column.ToLower()) && !includeContainer)
                            {
                                includeContainer = true;
                            }
                        }
                    }
                }

                widgetConfiguration.PE_Widget.IncludeHtmlBeforeAfter = includeBeforeAfter;
                widgetConfiguration.PE_Widget.IncludeWebpartContainerProperties = includeContainer;

                widgetConfigurations.Add(widgetConfiguration);
            }

            if(!widgetConfigurations.Any(x => x.PE_Widget.PE_WidgetCodeName.Equals("editabletext", StringComparison.OrdinalIgnoreCase)))
            {
                // Add default editabletext
                widgetConfigurations.Add(JsonConvert.DeserializeObject<ConverterWidgetConfiguration>(
                    @"{
                      ""PE_Widget"": {
                        ""PE_WidgetCodeName"": ""editabletext"",
                        ""IsInlineWidget"": false,
                        ""IsEditorWidget"": true,
                        ""IncludeHtmlBeforeAfter"": false,
                        ""IncludeWebpartContainerProperties"": false,
                        ""RenderIfInline"": false,
                        ""KeyValues"": [
                          {
                                    ""Key"": ""EditableText"",
                            ""Value"": null,
                            ""DefaultValue"": null,
                            ""OutKey"": ""html"",
                            ""CanContainInlineWidgets"": true,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                                    ""Key"": ""EncodeText"",
                            ""Value"": """",
                            ""DefaultValue"": ""false"",
                            ""OutKey"": ""IGNORE"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                                    ""Key"": ""HtmlAreaToolbar"",
                            ""Value"": """",
                            ""DefaultValue"": ""Full"",
                            ""OutKey"": ""IGNORE"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                                    ""Key"": ""HtmlAreaToolbarLocation"",
                            ""Value"": """",
                            ""DefaultValue"": ""Out:FCKToolbar"",
                            ""OutKey"": ""IGNORE"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          }
                        ]
                      },
                      ""PB_Widget"": {
                                ""PB_WidgetIdentifier"": ""RichTextEditor"",
                        ""AdditionalKeyValues"": {
                                    ""IGNORE"": """"
                        }
                            }
                        }"));
            }
            if (!widgetConfigurations.Any(x => x.PE_Widget.PE_WidgetCodeName.Equals("edtiableimage", StringComparison.OrdinalIgnoreCase)))
            {
                // Add default editableimage
                widgetConfigurations.Add(JsonConvert.DeserializeObject<ConverterWidgetConfiguration>(
                    @"{
                      ""PE_Widget"": {
                        ""PE_WidgetCodeName"": ""EditableImage"",
                        ""IsInlineWidget"": false,
                        ""IsEditorWidget"": true,
                        ""IncludeHtmlBeforeAfter"": false,
                        ""IncludeWebpartContainerProperties"": false,
                        ""RenderIfInline"": false,
                        ""KeyValues"": [
                          {
                            ""Key"": ""EditableImageUrl"",
                            ""Value"": null,
                            ""DefaultValue"": null,
                            ""OutKey"": ""INHERIT"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                            ""Key"": ""ImageTitle"",
                            ""Value"": """",
                            ""DefaultValue"": null,
                            ""OutKey"": ""INHERIT"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                            ""Key"": ""AlternateText"",
                            ""Value"": """",
                            ""DefaultValue"": null,
                            ""OutKey"": ""INHERIT"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                            ""Key"": ""ImageWidth"",
                            ""Value"": """",
                            ""DefaultValue"": null,
                            ""OutKey"": ""INHERIT"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          },
                          {
                            ""Key"": ""ImageHeight"",
                            ""Value"": """",
                            ""DefaultValue"": null,
                            ""OutKey"": ""INHERIT"",
                            ""CanContainInlineWidgets"": false,
                            ""InlineWidgetMode"": ""ADDAFTER""
                          }
                        ]
                      },
                      ""PB_Widget"": {
                        ""PB_WidgetIdentifier"": ""INHERIT"",
                        ""AdditionalKeyValues"": {
                          ""IGNORE"": """"
                        }
                      }
                    }"));
            }

            return widgetConfigurations;

        }
    }
}
