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

            string portalEngineType = peSection.SectionWidget?.WebPartType ?? string.Empty;
            if ((pbSection?.TypeIdentifier ?? String.Empty).IndexOf("Bootstrap4LayoutTool", 0, StringComparison.OrdinalIgnoreCase) == -1 && (pbSection?.TypeIdentifier ?? String.Empty).IndexOf("Bootstrap5LayoutTool", 0, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return;
            }
            if (portalEngineType.IndexOf("columnlayout", 0, StringComparison.OrdinalIgnoreCase) == -1 || portalEngineType.IndexOf("bootstrap", 0, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return;
            }

            int portalEngineBootstrapVersion = portalEngineType.IndexOf("4", 0) == -1 ? 3 : 4;
            int pageBuilderBootstrapVersion = pbSection.TypeIdentifier.IndexOf("5", 0, StringComparison.OrdinalIgnoreCase) > -1 ? 5 : 4;

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
                        pbSection.Properties[propertyKey] = $"-{pbSection.Properties[propertyKey].Trim('-')}".Replace("-0","0"); // -0 to 0 for "flex" value of 0
                    }
                }
                if (propertyKey.Equals("columnCSSPrepend", StringComparison.OrdinalIgnoreCase))
                {
                    pbSection.Properties[propertyKey] = pbSection.Properties[propertyKey].Trim('-');
                }

                // No other conversion needed
                if (portalEngineBootstrapVersion == pageBuilderBootstrapVersion)
                {
                    return;
                }

                // Handle bootstrap 3 to 4/5 conversion
                string value = pbSection.Properties[propertyKey];
                if (!string.IsNullOrWhiteSpace(value) && (propertyKey.ToLower().Contains("additionalcss") || propertyKey.ToLower().Contains("cssprepend")))
                {

                    if (portalEngineBootstrapVersion == 3)
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
                            value = value.Replace("-xs", "-sm");
                        }
                    }

                    if (propertyKey.ToLower().Contains("additionalcss") && !string.IsNullOrWhiteSpace(value))
                    {
                        if (portalEngineBootstrapVersion == 3)
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

                            // Hidden / visible
                            var hiddenRegex = new Regex(@"(hidden-){1}([a-z]{2})");
                            var hiddenMatches = hiddenRegex.Matches(value);
                            foreach (Match match in hiddenMatches)
                            {
                                if (match.Success)
                                {
                                    // Handle new d-block
                                    switch (match.Groups[2].Value.ToLower())
                                    {
                                        case "xs":
                                            // Can't hit since it was replaced above
                                            break;
                                        case "sm":
                                            // Special case since xs is now sm
                                            value = value.Replace(match.Value, $"d-none d-sm-block");
                                            break;
                                        case "md":
                                            value = value.Replace(match.Value, $"d-md-none d-lg-block");
                                            break;
                                        case "lg":
                                            value = value.Replace(match.Value, $"d-lg-none d-xl-block");
                                            break;
                                        case "xl":
                                            value = value.Replace(match.Value, $"d-xl-none");
                                            break;
                                    }
                                }
                            }

                            var visibleRegex = new Regex(@"(visible-){1}([a-z]{2})");
                            var visibleMatches = visibleRegex.Matches(value);
                            foreach (Match match in visibleMatches)
                            {
                                if (match.Success)
                                {
                                    var size = match.Groups[2].Value;
                                    // Handle new d-block
                                    switch (match.Groups[2].Value.ToLower())
                                    {
                                        case "xs":
                                            // Can't hit since it was replaced above
                                            break;
                                        case "sm":
                                            // Special case for how "xs" is now "sm" for bootstrap 3
                                            value = value.Replace(match.Value, $"d-block d-md-none");
                                            break;
                                        case "md":
                                            value = value.Replace(match.Value, $"d-none d-md-block d-lg-none");
                                            break;
                                        case "lg":
                                            value = value.Replace(match.Value, $"d-none d-lg-block d-xl-none");
                                            break;
                                        case "xl":
                                            // Also a special case since there is no xxl so just keep xl as large
                                            value = value.Replace(match.Value, $"d-none d-xl-block");
                                            break;
                                    }

                                    var nextSize = size.Replace("lg", "xl").Replace("md", "lg").Replace("sm", "md").Replace("xs", "sm");
                                    value = value.Replace(match.Value, $"d-none d-{size}-block d-{nextSize}-none")
                                        .Replace("d-none d-xs-block d-sm-none", "d-block d-sm-none");
                                }
                            }

                        }

                        // Floats
                        if (portalEngineBootstrapVersion == 3 && pageBuilderBootstrapVersion >= 4)
                        {
                            value = value.Replace("pull-left", "float-left");
                            value = value.Replace("pull-right", "float-right");
                        }
                        if (portalEngineBootstrapVersion <= 4 && pageBuilderBootstrapVersion == 5)
                        {
                            value = value.Replace("float-left", "float-start");
                            value = value.Replace("float-right", "float-end");

                            value = value.Replace("left-", "start-");
                            value = value.Replace("right-", "end-");
                            value = value.Replace("-left", "-start");
                            value = value.Replace("-right", "-end");
                            value = value.Replace("ml-", "ms-");
                            value = value.Replace("mr-", "me-");
                            value = value.Replace("pl-", "ps-");
                            value = value.Replace("pr-", "pe-");
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
