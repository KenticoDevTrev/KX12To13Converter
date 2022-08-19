using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KX12To13Converter.Events
{
    /// <summary>
    /// Bootstrap layout parsing event
    /// </summary>
    public class ProcessSection_After_BootstrapLayout
    {
        public static void ProcessSectionAfter_Bootstrap(object sender, PortalToMVCProcessWidgetSectionEventArgs sectionEventArgs)
        {
            var pbSection = sectionEventArgs.PageBuilderSection;
            var peSection = sectionEventArgs.PortalEngineWidgetSection;

            if (pbSection.TypeIdentifier.Equals("Bootstrap4LayoutTool", StringComparison.OrdinalIgnoreCase) && !peSection.IsDefault)
            {
                foreach (var propertyKey in pbSection.Properties.Keys.ToList())
                {
                    if (propertyKey.ToLower().Contains("width"))
                    {
                        if (string.IsNullOrWhiteSpace(pbSection.Properties[propertyKey]))
                        {
                            pbSection.Properties[propertyKey] = "-12";
                        }
                        else
                        {
                            pbSection.Properties[propertyKey] = $"-{pbSection.Properties[propertyKey].Trim('-')}";
                        }
                    }
                    if (propertyKey.Equals("columnCSSPrepend", StringComparison.OrdinalIgnoreCase))
                    {
                        pbSection.Properties[propertyKey] = pbSection.Properties[propertyKey].Trim('-');
                    }

                    // Handle bootstrap 3 to 4 conversion
                    if (peSection.SectionWidget?.WebPartType.Equals("ColumnLayout_Bootstrap", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        string value = pbSection.Properties[propertyKey];
                        if (!string.IsNullOrWhiteSpace(value) && (propertyKey.ToLower().Contains("additionalcss") || propertyKey.ToLower().Contains("cssprepend")))
                        {

                            if (value.Contains("-lg"))
                            {
                                value = value.Replace("-lg", "-xl");
                            }
                            if (value.Contains("-md"))
                            {
                                value = value.Replace("-md", "-lg");
                            }
                            if (value.Contains("-sm"))
                            {
                                value = value.Replace("-sm", "-md");
                            }
                            if (value.Contains("-xs"))
                            {
                                value = value.Replace("-xs", "-md");
                            }

                            if (propertyKey.ToLower().Contains("additionalcss") && !string.IsNullOrWhiteSpace(value))
                            {


                                // handle offsets
                                var offsetRegex = new Regex(@"(col-){1}([a-z]{2})(-offset-){1}([0-9]{1,2})");
                                var offsetMatches = offsetRegex.Matches(value);
                                foreach (Match match in offsetMatches)
                                {
                                    if (match.Success)
                                    {
                                        value = value.Replace(match.Value, $"offset-{match.Groups[2].Value}-{match.Groups[4].Value}");
                                    }
                                }

                                // Floats
                                value = value.Replace("pull-right", "float-right");
                                value = value.Replace("pull-left", "float-left");

                                // Hidden / visible
                                var hiddenRegex = new Regex(@"(hidden-){1}([a-z]{2})");
                                var hiddenMatches = hiddenRegex.Matches(value);
                                foreach (Match match in hiddenMatches)
                                {
                                    if (match.Success)
                                    {
                                        value = value.Replace(match.Value, $"d-{match.Groups[2].Value}-none").Replace("d-xs-none", "d-none");
                                    }
                                }

                                var visibleRegex = new Regex(@"(visible-){1}([a-z]{2})");
                                var visibleMatches = visibleRegex.Matches(value);
                                foreach (Match match in visibleMatches)
                                {
                                    if (match.Success)
                                    {
                                        var size = match.Groups[2].Value;
                                        var nextSize = size.Replace("lg", "xl").Replace("md", "lg").Replace("sm", "md").Replace("xs", "sm");
                                        value = value.Replace(match.Value, $"d-none d-{size}-block d-{nextSize}-none")
                                            .Replace("d-none d-xs-block d-sm-none", "d-block d-sm-none");
                                    }
                                }

                                if (value.Contains("push-") || value.Contains("pull-"))
                                {
                                    sectionEventArgs.PrimaryEventArgs.ConversionNotes.Add(new ConversionNote()
                                    {
                                        Source = $"Bootstrap3To4Conversion",
                                        Type = "OrderingDetected",
                                        Description = $"Bootstrap layout detected with push or pull classes, which must be manually switched to order-[size]-[#]"
                                    });
                                }
                            }

                            // Set value
                            pbSection.Properties[propertyKey] = value;
                        }

                    }
                }
            }
        }
    }
}
