using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using KX12To13Converter.Events;
using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PortalEngineToMVCConverter : IPortalEngineToMVCConverter
    {
        public PortalEngineToMVCConverter(List<TemplateConfiguration> templateConfigurations,
            List<ConverterSectionConfiguration> sectionConfigurations,
            PageBuilderSection defaultSectionConfiguration,
            List<ConverterWidgetConfiguration> widgetConfigurations)
        {
            TemplateConfigurations = templateConfigurations;
            SectionConfigurations = sectionConfigurations;
            DefaultSectionConfiguration = defaultSectionConfiguration;
            WidgetConfigurations = widgetConfigurations;
            WidgetTypeToConfiguration = WidgetConfigurations.GroupBy(key => key.PE_Widget.PE_WidgetCodeName.ToLowerInvariant()).ToDictionary(key => key.Key, value => value.First());

            if (widgetConfigurations.Any(wc => wc.PE_Widget.KeyValues.Count(kv => kv.CanContainInlineWidgets) > 1))
            {
                throw new NotSupportedException("System cannot processes widgets that contain multiple properties that may contain inline widgets.  Please limit Inline widget capable properties to only 1 property per widget.");
            }

            Initialize();
        }

        private void Initialize()
        {
            TemplateDictionary = PageTemplateInfoProvider.GetTemplates().TypedResult.ToDictionary(key => key.PageTemplateId, value => value);

            // If all configurations are present, then use database value for ignore if missing in configuration, otherwise don't.
            if ((TemplateConfigurations?.Any() ?? false) && (SectionConfigurations?.Any() ?? false) && DefaultSectionConfiguration != null && (WidgetConfigurations?.Any() ?? false))
            {
                IgnoreIfMissingInConfig = ValidationHelper.GetBoolean(SettingsKeyInfoProvider.GetValue("IgnoreIfMissingInConfiguration"), true);
            }
            else
            {
                IgnoreIfMissingInConfig = false;
            }
            WebpartDictionary = WebPartInfoProvider.GetWebParts().TypedResult.ToDictionary(key => key.WebPartID, value => value);



            // Build widget dictionaries of Layout, Inline, and Editors
            var allWidgets = WidgetInfoProvider.GetWidgets().TypedResult;

            var allWidgetNames = allWidgets.Select(x => x.WidgetName.ToLower());

            AllWidgetToVisibleProperties = allWidgets.ToDictionary(key => key.WidgetName.ToLowerInvariant(), value => GetVisibleProperties(value));

            LayoutWidgetDictionary = allWidgets.Where(x => WebpartDictionary[x.WidgetWebPartID].WebPartType == (int)WebPartTypeEnum.Layout).ToDictionary(key => key.WidgetName.ToLowerInvariant(), value => value);

            // Add Editable Text and Editable Image webparts don't have a widget variation
            if (!allWidgetNames.Contains("editabletext"))
            {
                AllWidgetToVisibleProperties.Add("editabletext", new List<FormFieldInfo>()
                {
                    new FormFieldInfo()
                    {
                        Name = "text",
                        DefaultValue = ""
                    }
                });
            }
            if (!allWidgetNames.Contains("editableimage"))
            {
                AllWidgetToVisibleProperties.Add("editableimage", new List<FormFieldInfo>()
                {
                    new FormFieldInfo()
                    {
                        Name = "EditableImageUrl",
                        DefaultValue = ""
                    },
                    new FormFieldInfo()
                    {
                        Name = "ImageTitle",
                        DefaultValue = ""
                    },
                    new FormFieldInfo()
                    {
                        Name = "AlternateText",
                        DefaultValue = ""
                    },
                    new FormFieldInfo()
                    {
                        Name = "ImageWidth",
                        DefaultValue = ""
                    },
                    new FormFieldInfo()
                    {
                        Name = "ImageHeight",
                        DefaultValue = ""
                    },
                });
            }

        }

        public List<FormFieldInfo> GetVisibleProperties(WidgetInfo widget)
        {
            WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(widget.WidgetWebPartID);
            string widgetProperties = FormHelper.MergeFormDefinitions(wpi.WebPartProperties, widget.WidgetProperties);
            FormInfo fi = PortalFormHelper.GetWidgetFormInfo(widget.WidgetName, WidgetZoneTypeEnum.Editor, widgetProperties, true);
            return fi.GetFields(true, false, false, false, false);
        }
        public List<TemplateConfiguration> TemplateConfigurations { get; }
        public List<ConverterSectionConfiguration> SectionConfigurations { get; }
        public PageBuilderSection DefaultSectionConfiguration { get; }
        public List<ConverterWidgetConfiguration> WidgetConfigurations { get; }
        public Dictionary<string, ConverterWidgetConfiguration> WidgetTypeToConfiguration { get; }
        public Dictionary<int, PageTemplateInfo> TemplateDictionary { get; private set; }
        public Dictionary<string, List<FormFieldInfo>> AllWidgetToVisibleProperties { get; private set; }
        public Dictionary<string, WidgetInfo> LayoutWidgetDictionary { get; private set; }
        public Dictionary<int, WebPartInfo> WebpartDictionary { get; private set; }
        public bool IgnoreIfMissingInConfig { get; private set; }

        public void ProcessesDocuments(IEnumerable<TreeNode> documents, Func<TreeNode, PortalToMVCProcessDocumentPrimaryEventArgs, bool> handler)
        {
            foreach (var document in documents)
            {
                var pageBuilderData = ProcessDocument(document);
                handler(document, pageBuilderData);
            }
        }

        public PortalToMVCProcessDocumentPrimaryEventArgs ProcessDocument(TreeNode document)
        {
            // Start with template
            var portalToMVCEventArgs = BuildPortalToMVCProcessDocumentPrimaryEventArgs(document);
            // Will store editable areas in case multiple sections point to the same editable area.
            var existingPBEditableAreas = new Dictionary<string, EditableAreaConfiguration>();
            // Context is used to pass betweeen internal methods so we can split up logic easily.

            PortalConversionProcessorContext context = new PortalConversionProcessorContext()
            {
                document = document,
                portalToMVCEventArgs = portalToMVCEventArgs,
                existingPBEditableAreas = existingPBEditableAreas
            };

            // ProcessPage.Before
            using (var processPageEventHandler = PortalToMVCEvents.ProcessPage.StartEvent(portalToMVCEventArgs))
            {
                if (portalToMVCEventArgs.Handled)
                {
                    return portalToMVCEventArgs;
                }

                HandleTemplate(context);

                if (portalToMVCEventArgs.Handled)
                {
                    return portalToMVCEventArgs;
                }

                // ProcessPage.After
                processPageEventHandler.FinishEvent();
            }

            return portalToMVCEventArgs;

        }

        #region "Template"
        private void HandleTemplate(PortalConversionProcessorContext context)
        {
            var portalToMVCEventArgs = context.portalToMVCEventArgs;
            var templateConfiguration = new PageTemplateConfiguration();
            var templateEventArgs = new PortalToMVCProcessTemplateEventArgs(portalToMVCEventArgs, portalToMVCEventArgs.PortalEngineData.Template, TemplateConfigurations.AsReadOnly())
            {
                PageBuilderTemplate = templateConfiguration
            };

            //ProcessTemplate.Before
            using (var processTemplateEventHandler = PortalToMVCEvents.ProcessTemplate.StartEvent(templateEventArgs))
            {
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }

                if (!templateEventArgs.TemplateHandled)
                {
                    // ProcessTemplate.Execute
                    ProcessTemplate_Execute(templateEventArgs.PortalEngineTemplate, templateEventArgs.PageBuilderTemplate);
                }
                // If configuration missing
                if (string.IsNullOrWhiteSpace(templateEventArgs.PageBuilderTemplate.Identifier) || templateEventArgs.PageBuilderTemplate.Identifier.Equals("IGNORE", StringComparison.OrdinalIgnoreCase))
                {
                    portalToMVCEventArgs.ConversionNotes.Add(new ConversionNote()
                    {
                        IsError = false,
                        Source = "HandleTemplate",
                        Type = "SkippedConfigurationTemplate",
                        Description = $"Template {templateEventArgs.PortalEngineTemplate.CodeName} is set to either IGNORE or not in the template configuration.  Stopping Conversion."
                    });
                    portalToMVCEventArgs.CancelOperation = true;
                    return;
                }

                // ProcessTemplate After
                processTemplateEventHandler.FinishEvent();
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
            }

            // Set in case they adjusted
            portalToMVCEventArgs.PageBuilderData.TemplateConfiguration = templateEventArgs.PageBuilderTemplate;

            // set to context for other operations
            var templateConfig = TemplateConfigurations.Where(x => x.PE_CodeName.Equals(portalToMVCEventArgs.PortalEngineData.Template.CodeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // Handle Webpart Zones
            HandleWebpartZones(templateConfig, context);
        }

        private void ProcessTemplate_Execute(PageTemplateInfo peTemplate, PageTemplateConfiguration pbTemplate)
        {
            var templateConfig = TemplateConfigurations.Where(x => x.PE_CodeName.Equals(peTemplate.CodeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // If missing configuration
            if (IgnoreIfMissingInConfig && templateConfig == null)
            {
                pbTemplate.Identifier = string.Empty;
                return;
            }

            pbTemplate.ConfigurationIdentifier = Guid.NewGuid();

            if (templateConfig != null)
            {
                pbTemplate.Identifier = templateConfig.PB_Template.PB_TemplateIdentifier.Replace("INHERIT", peTemplate.CodeName);
                pbTemplate.Properties = templateConfig.PB_Template.PB_KeyValuePairs.Where(x => !x.Key.Equals("IGNORE", StringComparison.OrdinalIgnoreCase)).ToDictionary(key => key.Key, value => value.Value);
            }
            else
            {
                pbTemplate.Identifier = peTemplate.CodeName;
                pbTemplate.Properties = null;
            }
        }

        #endregion

        #region "Webpart Zones / Editable Areas"

        private void HandleWebpartZones(TemplateConfiguration templateConfig, PortalConversionProcessorContext context)
        {
            var portalToMVCEventArgs = context.portalToMVCEventArgs;
            var existingPBEditableAreas = context.existingPBEditableAreas;

            var templateZonesToIndex = new List<KeyValuePair<string, int>>();
            if (templateConfig != null && templateConfig.Zones.Any())
            {
                for (int i = 0; i < templateConfig.Zones.Count; i++)
                {
                    templateZonesToIndex.Add(new KeyValuePair<string, int>(templateConfig.Zones[i].PE_WebpartZoneName.ToLower(), i));
                }
            }

            // Loop through webparts zones, ordering by if they are found in the template and the index of the template zones
            foreach (var peZoneEditableArea in portalToMVCEventArgs.PortalEngineData.TemplateZones.OrderBy(x =>
            {
                var match = templateZonesToIndex.Where(t => t.Key.Equals(GetPEZoneID(x), StringComparison.OrdinalIgnoreCase));
                return match.Any() ? match.First().Value : 100;
            }))
            {
                HandleWebpartZone(peZoneEditableArea, templateConfig, context);
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
            }

            // Add Editable Areas
            foreach (var pbZoneEditableArea in existingPBEditableAreas.Values.Where(x => x.Sections.Any()))
            {
                // Add the editable area
                portalToMVCEventArgs.PageBuilderData.ZoneConfiguration.EditableAreas.Add(pbZoneEditableArea);
            }
        }

        private void HandleWebpartZone(PortalWebpartZone peZoneEditableArea, TemplateConfiguration templateConfig, PortalConversionProcessorContext context)
        {
            var portalToMVCEventArgs = context.portalToMVCEventArgs;
            var existingPBEditableAreas = context.existingPBEditableAreas;

            PortalToMVCProcessEditableAreaEventArgs editableAreaArgs = new PortalToMVCProcessEditableAreaEventArgs(portalToMVCEventArgs, peZoneEditableArea, templateConfig)
            {
                EditableAreaConfiguration = new EditableAreaConfiguration()
            };

            // ProcessZone.Before
            using (var processEditableAreaEventHandler = PortalToMVCEvents.ProcessEditableArea.StartEvent(editableAreaArgs))
            {
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }

                if (!editableAreaArgs.ZoneHandled)
                {
                    // ProcessZone Default
                    ProcessZone_Execute(templateConfig, editableAreaArgs.PortalEditableArea, editableAreaArgs.EditableAreaConfiguration);
                }

                // If configuration missing
                if (string.IsNullOrWhiteSpace(editableAreaArgs.EditableAreaConfiguration?.Identifier ?? String.Empty))
                {
                    portalToMVCEventArgs.ConversionNotes.Add(new ConversionNote()
                    {
                        IsError = false,
                        Source = "HandleWebpartZone",
                        Type = "SkippedConfigurationWebpartZone",
                        Description = $"No configuration found for zone {GetPEZoneID(editableAreaArgs.PortalEditableArea)} in template configuration, skipping."
                    });
                    return;
                }
                else if (editableAreaArgs.EditableAreaConfiguration?.Identifier?.Equals("IGNORE", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Ignoring, no need for note.
                    return;
                }

                // ProcessZone.After
                processEditableAreaEventHandler.FinishEvent();
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
            }

            // Get the new zone ID
            var pbZoneID = editableAreaArgs.EditableAreaConfiguration?.Identifier ?? String.Empty;

            // Do not processes if empty or ignore
            if (DataHelper.GetNotEmpty(pbZoneID, "IGNORE").Equals("IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // If zone doesn't exist yet, create it
            if (!existingPBEditableAreas.ContainsKey(pbZoneID.ToLowerInvariant()))
            {
                // Add it
                existingPBEditableAreas.Add(pbZoneID.ToLowerInvariant(), editableAreaArgs.EditableAreaConfiguration);
            }

            // Get the zone and it's config
            var pbZoneEditableArea = existingPBEditableAreas[pbZoneID.ToLowerInvariant()];

            // Handle sections next
            HandleWidgetSections(peZoneEditableArea, pbZoneEditableArea, context);
        }

        private void ProcessZone_Execute(TemplateConfiguration templateConfig, PortalWebpartZone peZoneEditableArea, EditableAreaConfiguration pbZoneEditableArea)
        {
            string identifier = GetPBEditableAreaIdentifier(peZoneEditableArea, templateConfig);
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return;
            }
            pbZoneEditableArea.Identifier = GetPBEditableAreaIdentifier(peZoneEditableArea, templateConfig);
        }

        #endregion

        #region "Layout Widgets / Sections"

        private void HandleWidgetSections(PortalWebpartZone peZoneEditableArea, EditableAreaConfiguration pbZoneEditableArea, PortalConversionProcessorContext context)
        {
            // Now handle Layout / Sections
            foreach (var peSection in peZoneEditableArea.WidgetSections)
            {
                HandleWidgetSection(peSection, pbZoneEditableArea, context);
            }
        }

        private void HandleWidgetSection(PortalWidgetSection peSection, EditableAreaConfiguration pbZoneEditableArea, PortalConversionProcessorContext context)
        {

            var portalToMVCEventArgs = context.portalToMVCEventArgs;
            var pbSection = new SectionConfiguration();

            var sectionEventArgs = new PortalToMVCProcessWidgetSectionEventArgs(portalToMVCEventArgs, peSection, SectionConfigurations.AsReadOnly(), DefaultSectionConfiguration)
            {
                PageBuilderSection = pbSection,
            };

            using (var sectionEventHandler = PortalToMVCEvents.ProcessSection.StartEvent(sectionEventArgs))
            {
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }

                if (!sectionEventArgs.SectionHandled)
                {
                    // ProcessSection.Execute
                    ProcessSectionExecute(sectionEventArgs.PortalEngineWidgetSection, sectionEventArgs.PageBuilderSection);
                }
                // If configuration missing
                if (string.IsNullOrWhiteSpace(sectionEventArgs.PageBuilderSection?.TypeIdentifier ?? String.Empty))
                {
                    portalToMVCEventArgs.ConversionNotes.Add(new ConversionNote()
                    {
                        IsError = false,
                        Source = "HandleWidgetSection",
                        Type = "SkippedConfigurationSection",
                        Description = $"No configuration found for {(sectionEventArgs.PortalEngineWidgetSection.IsDefault ? "DEFAULT" : sectionEventArgs.PortalEngineWidgetSection?.SectionWidget?.WebPartType ?? "[No Section Found]")} in Sections configuration, skipping."
                    });
                    return;
                }
                else if (sectionEventArgs.PageBuilderSection?.TypeIdentifier?.Equals("IGNORE", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // no need to report
                    return;
                }


                sectionEventHandler.FinishEvent();
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
            }

            // If ignoring the section completely, then return
            if (DataHelper.GetNotEmpty(sectionEventArgs.PageBuilderSection?.TypeIdentifier ?? String.Empty, "IGNORE").Equals("IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // re-assign in case new object
            pbSection = sectionEventArgs.PageBuilderSection;
            peSection = sectionEventArgs.PortalEngineWidgetSection;

            HandleWidgetSectionZones(peSection, pbSection, context);

            // Add section
            pbZoneEditableArea.Sections.Add(pbSection);
        }

        private void ProcessSectionExecute(PortalWidgetSection peSection, SectionConfiguration pbSection)
        {
            var sectionConfiguration = peSection.IsDefault ? null : SectionConfigurations.Where(x => (x.PE_WidgetZoneSection?.SectionWidgetCodeName ?? String.Empty).Equals(peSection.IsDefault ? Guid.NewGuid().ToString() : peSection.SectionWidget.WebPartType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // Skip if missing config
            if (!peSection.IsDefault && IgnoreIfMissingInConfig && sectionConfiguration == null)
            {
                pbSection.TypeIdentifier = String.Empty;
                return;
            }
            // Skip if default and no default section
            if (peSection.IsDefault && DefaultSectionConfiguration == null)
            {
                pbSection.TypeIdentifier = String.Empty;
                return;
            }

            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            pbSection.Identifier = Guid.NewGuid();

            if (sectionConfiguration != null)
            {
                pbSection.TypeIdentifier = sectionConfiguration.PB_Section.SectionIdentifier.Replace("INHERIT", peSection.SectionWidget.WebPartType);

                // Add Keys based on config
                if (sectionConfiguration.PE_WidgetZoneSection != null && peSection.SectionWidget != null)
                {
                    var peZoneSection = sectionConfiguration.PE_WidgetZoneSection;
                    foreach (var keyValue in peZoneSection.KeyValues.Where(x => !(x.OutKey?.Equals("IGNORE") ?? false)))
                    {
                        var propertyValue = !string.IsNullOrWhiteSpace(keyValue.Value) ? keyValue.Value : peSection.SectionWidget.GetValue(keyValue.Key)?.ToString() ?? keyValue.DefaultValue;
                        keyValues.Add(DataHelper.GetNotEmpty(keyValue.OutKey, "INHERIT").Replace("INHERIT", keyValue.Key), propertyValue);
                    }
                }

                // Add additional keys
                if (sectionConfiguration.PB_Section != null && sectionConfiguration.PB_Section.AdditionalKeyValues != null)
                {
                    foreach (string key in sectionConfiguration.PB_Section.AdditionalKeyValues.Keys.Where(x => !x.Equals("IGNORE", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (keyValues.ContainsKey(key))
                        {
                            keyValues[key] = sectionConfiguration.PB_Section.AdditionalKeyValues[key]?.ToString();
                        }
                        else
                        {
                            keyValues.Add(key, sectionConfiguration.PB_Section.AdditionalKeyValues[key]?.ToString());
                        }
                    }
                }
            }
            else if (!peSection.IsDefault && peSection.SectionWidget != null)
            {
                // Add values based on widget
                pbSection.TypeIdentifier = peSection.SectionWidget.WebPartType;

                var widgetFields = peSection.IsDefault ? new List<FormFieldInfo>() : AllWidgetToVisibleProperties[peSection.SectionWidget.WebPartType.ToLowerInvariant()];

                foreach (var field in widgetFields)
                {
                    keyValues.Add(field.Name, peSection.SectionWidget.GetValue(field.Name)?.ToString() ?? field.DefaultValue);
                }
            }
            else // Use Default section finally
            {
                pbSection.TypeIdentifier = DefaultSectionConfiguration.SectionIdentifier;
                if (DefaultSectionConfiguration.AdditionalKeyValues != null)
                {
                    foreach (string key in DefaultSectionConfiguration.AdditionalKeyValues.Keys.Where(x => !x.Equals("IGNORE", StringComparison.OrdinalIgnoreCase)))
                    {
                        keyValues.Add(key, DefaultSectionConfiguration.AdditionalKeyValues[key]);
                    }

                }
            }
            pbSection.Properties = keyValues;

        }

        #endregion

        #region "Section Zones"

        private void HandleWidgetSectionZones(PortalWidgetSection peSection, SectionConfiguration pbSection, PortalConversionProcessorContext context)
        {
            int zoneIndex = 0;
            foreach (var peWidgetZoneWidgets in peSection.SectionZoneToWidgets)
            {
                HandleWidgetSectionZone(peWidgetZoneWidgets, peSection, pbSection, zoneIndex, context);

                zoneIndex++;
            }

        }

        private void HandleWidgetSectionZone(List<PortalWidget> peWidgetZoneWidgets, PortalWidgetSection peSection, SectionConfiguration pbSection, int zoneIndex, PortalConversionProcessorContext context)
        {
            var portalToMVCEventArgs = context.portalToMVCEventArgs;
            var pbWidgetZone = new ZoneConfiguration()
            {
                Identifier = Guid.NewGuid()
            };
            PortalToMVCProcessWidgetSectionZoneEventArgs sectionZoneEventArgs = new PortalToMVCProcessWidgetSectionZoneEventArgs(portalToMVCEventArgs, peSection, pbSection, zoneIndex)
            {
                PageBuilderZoneConfig = pbWidgetZone
            };

            using (var sectionZoneEventHandler = PortalToMVCEvents.ProcessSectionZone.StartEvent(sectionZoneEventArgs))
            {
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
                if (!sectionZoneEventArgs.SectionZoneHandled)
                {
                    // Default logic...there is none
                }
                sectionZoneEventHandler.FinishEvent();
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
            }

            // reassign in case new
            pbSection = sectionZoneEventArgs.PageBuilderSection;
            pbWidgetZone = sectionZoneEventArgs.PageBuilderZoneConfig;

            // Ignore if the Zone name is Ignore, but in this case not if empty or null as the zone name is optional
            if ((pbWidgetZone?.Name ?? String.Empty).Equals("IGNORE"))
            {
                return;
            }

            HandleWidgets(peWidgetZoneWidgets, pbSection, pbWidgetZone, context);


            pbSection.Zones.Add(pbWidgetZone);

        }

        #endregion

        #region "Widgets"

        private void HandleWidgets(List<PortalWidget> peWidgetZoneWidgets, SectionConfiguration pbSection, ZoneConfiguration pbWidgetZone, PortalConversionProcessorContext context)
        {
            foreach (var peWidget in peWidgetZoneWidgets)
            {
                HandleWidget(peWidget, pbWidgetZone, context);
            }
        }

        private void HandleWidget(PortalWidget peWidget, ZoneConfiguration pbWidgetZone, PortalConversionProcessorContext context)
        {
            var portalToMVCEventArgs = context.portalToMVCEventArgs;
            var pbWidget = new WidgetConfiguration();

            var widgetEventArgs = new PortalToMVCProcessWidgetEventArgs(portalToMVCEventArgs, peWidget, WidgetConfigurations.AsReadOnly())
            {
                PageBuilderWidget = pbWidget
            };

            using (var widgetEventHandler = PortalToMVCEvents.ProcessWidget.StartEvent(widgetEventArgs))
            {
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }

                if (!widgetEventArgs.WidgetHandled)
                {
                    ProcessWidgetExecute(portalToMVCEventArgs, peWidget, pbWidget);
                }
                // If configuration missing
                if (string.IsNullOrWhiteSpace(pbWidget?.TypeIdentifier ?? String.Empty))
                {
                    // only give error if the given widget is not a layout widget, as layout widgets without any inner content should be ignored.
                    if (!LayoutWidgetDictionary.ContainsKey(peWidget?.Widget?.WebPartType?.ToLower() ?? String.Empty))
                    {
                        portalToMVCEventArgs.ConversionNotes.Add(new ConversionNote()
                        {
                            IsError = false,
                            Source = "HandleWidget",
                            Type = "SkippedConfigurationWidget",
                            Description = $"No configuration found for {peWidget.Widget.WebPartType} in Widgets configuration, skipping."
                        });
                    }
                    return;
                }
                else if (pbWidget?.TypeIdentifier?.Equals("IGNORE", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Ignored, just return.
                    return;
                }

                widgetEventHandler.FinishEvent();
                if (portalToMVCEventArgs.Handled)
                {
                    return;
                }
            }

            pbWidget = widgetEventArgs.PageBuilderWidget;
            if (DataHelper.GetNotEmpty(pbWidget?.TypeIdentifier ?? String.Empty, "IGNORE").Equals("IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Add widget to section
            pbWidgetZone.Widgets.Add(pbWidget);
        }

        private void ProcessWidgetExecute(PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCEventArgs, PortalWidget peWidget, WidgetConfiguration pbWidget)
        {
            var webpartInstance = peWidget.Widget;
            if (webpartInstance.WebPartType.Equals("widget", StringComparison.OrdinalIgnoreCase) && webpartInstance.CurrentVariantInstance != null)
            {
                webpartInstance = webpartInstance.CurrentVariantInstance;
            }

            var widgetConfiguation = WidgetConfigurations.Where(x => (x.PE_Widget?.PE_WidgetCodeName ?? Guid.NewGuid().ToString()).Equals(webpartInstance.WebPartType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // If missing configuration
            if (IgnoreIfMissingInConfig && widgetConfiguation == null)
            {
                pbWidget.TypeIdentifier = string.Empty;
                return;
            }

            pbWidget.Identifier = Guid.NewGuid();

            // Can't process if no properties found
            List<FormFieldInfo> widgetFields = new List<FormFieldInfo>();
            if (!AllWidgetToVisibleProperties.ContainsKey(webpartInstance.WebPartType.ToLowerInvariant()))
            {
                portalToMVCEventArgs.ConversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "ProcessWidget",
                    Type = "WidgetNotFound",
                    Description = $"Could not find a widget with webpart type name {webpartInstance.WebPartType}"
                });
            }
            else
            {
                widgetFields = AllWidgetToVisibleProperties[webpartInstance.WebPartType.ToLowerInvariant()];
            }

            var reservedKeys = new string[] { "contentbefore", "contentafter", "htmlbefore", "htmlafter", "container", "containername", "containertitle", "containercssclass", "containercustomcontent" };

            if (widgetConfiguation != null)
            {
                pbWidget.TypeIdentifier = widgetConfiguation.PB_Widget.PB_WidgetIdentifier.Replace("INHERIT", webpartInstance.WebPartType);
                pbWidget.Variants = new List<WidgetVariantConfiguration>();

                Dictionary<string, string> properties = new Dictionary<string, string>();
                // Configuration Keys
                foreach (var keyvalue in widgetConfiguation.PE_Widget.KeyValues.Where(x => !(x.OutKey?.Equals("IGNORE", StringComparison.OrdinalIgnoreCase) ?? false) && !reservedKeys.Contains(x.Key.ToLowerInvariant())))
                {
                    var widgetPropertyValue = keyvalue.Value?.Replace("INHERIT", string.Empty) ?? string.Empty;
                    widgetPropertyValue = !string.IsNullOrWhiteSpace(widgetPropertyValue) ? widgetPropertyValue : webpartInstance.GetValue(keyvalue.Key)?.ToString();
                    widgetPropertyValue = !string.IsNullOrWhiteSpace(widgetPropertyValue) ? widgetPropertyValue : keyvalue.DefaultValue;
                    // Handle editable image and text, the WidgetType may be either EditableImage/EditableTExt or Webpart if it's a widget version of it, so need to check values.
                    if ((peWidget.WidgetType == WebpartType.EditableImage || !string.IsNullOrWhiteSpace(peWidget.EditableImageUrl)) && keyvalue.Key.Equals("EditableImageUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        widgetPropertyValue = peWidget.EditableImageUrl;
                    }
                    else if ((peWidget.WidgetType == WebpartType.EditableImage || !string.IsNullOrWhiteSpace(peWidget.EditableImageAlt)) && keyvalue.Key.Equals("AlternateText", StringComparison.OrdinalIgnoreCase))
                    {
                        widgetPropertyValue = DataHelper.GetNotEmpty(peWidget.EditableImageAlt, widgetPropertyValue);
                    }
                    else if ((peWidget.WidgetType == WebpartType.EditableText || !string.IsNullOrWhiteSpace(peWidget.EditableText)) && keyvalue.Key.Equals("EditableText", StringComparison.OrdinalIgnoreCase))
                    {
                        widgetPropertyValue = peWidget.EditableText;
                    }
                    string key = (!string.IsNullOrWhiteSpace((keyvalue.OutKey ?? String.Empty).Replace("INHERIT", "")) ? keyvalue.OutKey : keyvalue.Key);
                    try
                    {
                        properties.Add(key, widgetPropertyValue);
                    }
                    catch (ArgumentException ex)
                    {
                        if (ex.Message.Contains("An item with the same key has already been added."))
                        {
                            portalToMVCEventArgs.ConversionNotes.Add(new ConversionNote()
                            {
                                IsError = true,
                                Source = "ProcessWidgetExecute",
                                Type = "DuplicateOutKey",
                                Description = $"Outkey of {key} found, please check Widget Configuration for ${widgetConfiguation.PE_Widget.PE_WidgetCodeName}"
                            });
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
                // Addiitonal Keys
                foreach (var key in widgetConfiguation.PB_Widget.AdditionalKeyValues.Keys.Where(x => !x.Equals("IGNORE", StringComparison.OrdinalIgnoreCase) && !reservedKeys.Contains(x.ToLowerInvariant())))
                {
                    if (properties.ContainsKey(key))
                    {
                        properties[key] = widgetConfiguation.PB_Widget.AdditionalKeyValues[key];
                    }
                    else
                    {
                        properties.Add(key, widgetConfiguation.PB_Widget.AdditionalKeyValues[key]);
                    }
                }

                // Html Before/After and WebpartContainers
                if (widgetConfiguation.PE_Widget.IncludeHtmlBeforeAfter)
                {
                    // Data should be: Defined on the portal widget, followed by the config's value, followed by the widget's value, followed by default value.
                    var contentBeforeConfig = widgetConfiguation.PE_Widget.KeyValues.Where(x => x.Key.Equals("contentbefore", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var ContentBefore = DataHelper.GetNotEmpty(peWidget.HtmlBefore, string.Empty);
                    ContentBefore = DataHelper.GetNotEmpty(ContentBefore, contentBeforeConfig?.Value ?? string.Empty);
                    ContentBefore = DataHelper.GetNotEmpty(ContentBefore, webpartInstance.GetValue("ContentBefore")?.ToString() ?? String.Empty);
                    ContentBefore = DataHelper.GetNotEmpty(ContentBefore, contentBeforeConfig?.DefaultValue ?? string.Empty);

                    var contentAfterConfig = widgetConfiguation.PE_Widget.KeyValues.Where(x => x.Key.Equals("contentafter", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var ContentAfter = DataHelper.GetNotEmpty(peWidget.HtmlAfter, string.Empty);
                    ContentAfter = DataHelper.GetNotEmpty(ContentAfter, contentAfterConfig?.Value ?? string.Empty);
                    ContentAfter = DataHelper.GetNotEmpty(ContentAfter, webpartInstance.GetValue("ContentAfter")?.ToString() ?? String.Empty);
                    ContentAfter = DataHelper.GetNotEmpty(ContentAfter, contentAfterConfig?.DefaultValue ?? string.Empty);

                    if (!string.IsNullOrWhiteSpace(ContentBefore))
                    {
                        properties.Add("htmlBefore", ContentBefore);
                    }
                    if (!string.IsNullOrWhiteSpace(ContentAfter))
                    {
                        properties.Add("htmlAfter", ContentAfter);
                    }
                }
                if (widgetConfiguation.PE_Widget.IncludeWebpartContainerProperties)
                {
                    // Data should be: Defined on the portal widget, followed by the widget's value, config's value, followed by the widget's value, followed by default value.
                    var containerConfig = widgetConfiguation.PE_Widget.KeyValues.Where(x => x.Key.Equals("Container", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var Container = DataHelper.GetNotEmpty(peWidget.ContainerName, string.Empty);
                    Container = DataHelper.GetNotEmpty(Container, containerConfig?.Value ?? string.Empty);
                    Container = DataHelper.GetNotEmpty(Container, webpartInstance.GetValue("Container")?.ToString() ?? String.Empty);
                    Container = DataHelper.GetNotEmpty(Container, containerConfig?.DefaultValue ?? string.Empty);


                    var containerTitleConfig = widgetConfiguation.PE_Widget.KeyValues.Where(x => x.Key.Equals("ContainerTitle", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var ContainerTitle = DataHelper.GetNotEmpty(peWidget.ContainerTitle, string.Empty);
                    ContainerTitle = DataHelper.GetNotEmpty(ContainerTitle, containerTitleConfig?.Value ?? string.Empty);
                    ContainerTitle = DataHelper.GetNotEmpty(ContainerTitle, webpartInstance.GetValue("ContainerTitle")?.ToString() ?? String.Empty);
                    ContainerTitle = DataHelper.GetNotEmpty(ContainerTitle, containerTitleConfig?.DefaultValue ?? string.Empty);

                    var containerCSSConfig = widgetConfiguation.PE_Widget.KeyValues.Where(x => x.Key.Equals("ContainerCSSClass", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var ContainerCSSClass = DataHelper.GetNotEmpty(peWidget.ContainerCSSClass, string.Empty);
                    ContainerCSSClass = DataHelper.GetNotEmpty(ContainerCSSClass, containerCSSConfig?.Value ?? string.Empty);
                    ContainerCSSClass = DataHelper.GetNotEmpty(ContainerCSSClass, webpartInstance.GetValue("ContainerCSSClass")?.ToString() ?? String.Empty);
                    ContainerCSSClass = DataHelper.GetNotEmpty(ContainerCSSClass, containerCSSConfig?.DefaultValue ?? string.Empty);

                    var containerCustomContentConfig = widgetConfiguation.PE_Widget.KeyValues.Where(x => x.Key.Equals("ContainerCustomContent", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var ContainerCustomContent = DataHelper.GetNotEmpty(peWidget.ContainerCustomContent, string.Empty);
                    ContainerCustomContent = DataHelper.GetNotEmpty(ContainerCustomContent, containerCustomContentConfig?.Value ?? string.Empty);
                    ContainerCustomContent = DataHelper.GetNotEmpty(ContainerCustomContent, webpartInstance.GetValue("ContainerCustomContent")?.ToString() ?? String.Empty);
                    ContainerCustomContent = DataHelper.GetNotEmpty(ContainerCustomContent, containerCustomContentConfig?.DefaultValue ?? string.Empty);

                    if (!string.IsNullOrWhiteSpace(Container))
                    {
                        properties.Add("containerName", Container);
                    }
                    if (!string.IsNullOrWhiteSpace(ContainerTitle))
                    {
                        properties.Add("containerTitle", ContainerTitle);
                    }
                    if (!string.IsNullOrWhiteSpace(ContainerCSSClass))
                    {
                        properties.Add("containerCSSClass", ContainerCSSClass);
                    }
                    if (!string.IsNullOrWhiteSpace(ContainerCustomContent))
                    {
                        properties.Add("containerCustomContent", ContainerCustomContent);
                    }
                }

                pbWidget.Variants.Add(new WidgetVariantConfiguration()
                {
                    Identifier = Guid.NewGuid(),
                    //Name = peWidget.Widget.WebPartType, // Seems to be not needed in 13
                    Properties = properties
                });
            }
            else
            {
                // no configuration, just convert any visible property over
                pbWidget.TypeIdentifier = webpartInstance.WebPartType;
                pbWidget.Variants = new List<WidgetVariantConfiguration>();

                Dictionary<string, string> properties = new Dictionary<string, string>();
                foreach (var property in widgetFields)
                {
                    properties.Add(property.Name, webpartInstance.GetValue(property.Name)?.ToString() ?? property.DefaultValue);
                }
                // Handle editable image and text
                if (peWidget.WidgetType == WebpartType.EditableImage || !string.IsNullOrWhiteSpace(peWidget.EditableImageUrl) || !string.IsNullOrWhiteSpace(peWidget.EditableImageAlt))
                {
                    properties.Add("EditableImageUrl", peWidget.EditableImageUrl);
                    if (properties.ContainsKey("AlternateText"))
                    {
                        properties["AlternativeText"] = DataHelper.GetNotEmpty(DataHelper.GetNotEmpty(peWidget.EditableImageAlt, properties["AlternativeText"]), string.Empty);
                    }
                    else
                    {
                        properties.Add("AlternativeText", DataHelper.GetNotEmpty(peWidget.EditableImageAlt, string.Empty));
                    }

                }
                else if (peWidget.WidgetType == WebpartType.EditableText || !string.IsNullOrWhiteSpace(peWidget.EditableText))
                {
                    properties.Add("EditableText", peWidget.EditableText);
                }

                pbWidget.Variants.Add(new WidgetVariantConfiguration()
                {
                    Identifier = Guid.NewGuid(),
                    //Name = peWidget.Widget.WebPartType, // Seems to be not needed
                    Properties = properties
                });
            }


        }

        #endregion

        #region "Building Page Args"

        private PortalToMVCProcessDocumentPrimaryEventArgs BuildPortalToMVCProcessDocumentPrimaryEventArgs(TreeNode document)
        {
            int templateId = document.DocumentPageTemplateID > 0 ? document.DocumentPageTemplateID : document.NodeTemplateID;
            var checkDoc = document;
            if (templateId == 0)
            {
                while (checkDoc != null && checkDoc.NodeInheritPageTemplate && templateId == 0)
                {
                    checkDoc = checkDoc.Parent;
                    if (checkDoc != null)
                    {
                        templateId = checkDoc.DocumentPageTemplateID > 0 ? checkDoc.DocumentPageTemplateID : checkDoc.NodeTemplateID;
                    }
                }
            }

            var template = GetGeneralTemplateConfiguration(templateId);

            // Can't proceed if no template found
            if (template == null)
            {
                return new PortalToMVCProcessDocumentPrimaryEventArgs()
                {
                    CancelOperation = true,
                    Page = document,
                    ConversionNotes = new List<ConversionNote>()
                    {
                        new ConversionNote()
                        {
                            Source = "BuildPortalToMVCProcessDocumentPrimaryEventArgs",
                            IsError = true,
                            Description = $"No Page template found for document {document.NodeAliasPath} [{document.DocumentCulture}] - {document.NodeSiteName}]",
                            Type = "TemplateNotFound"
                        }
                    }
                };
            }

            var eventArgs = new PortalToMVCProcessDocumentPrimaryEventArgs(template)
            {
                Page = document
            };

            // Now build the portal engine data using the document
            var editableWebParts = document.DocumentContent.EditableWebParts;
            var pageTemplateInstance = new PageTemplateInstance(document.DocumentWebParts)
            {
                ParentPageTemplate = template.Template
            };

            List<string> documentZonesFound = new List<string>();

            // Take care of editable zones on template first
            foreach (var webpartZone in eventArgs.PortalEngineData.TemplateZones.Where(x => x.ZoneType == WebpartType.EditableText || x.ZoneType == WebpartType.EditableImage))
            {
                if (webpartZone.ZoneType == WebpartType.EditableImage && editableWebParts.ContainsKey(webpartZone.EditableWebpartInstance?.ControlID.ToLowerInvariant() ?? ""))
                {

                    // Add single Editable Image
                    var editableImageInstance = webpartZone.EditableWebpartInstance;
                    var editableImageContentXml = editableWebParts[webpartZone.EditableWebpartInstance.ControlID.ToLowerInvariant()];

                    var editableImagePortalWidget = GetEditableImagePortalWidget(editableImageInstance, editableImageContentXml);
                    var widgetSection = new PortalWidgetSection()
                    {
                        IsDefault = true,
                        SectionZoneToWidgets = new List<List<PortalWidget>>()
                        {
                            new List<PortalWidget>(){ editableImagePortalWidget }
                        }
                    };

                    webpartZone.WidgetSections.Add(widgetSection);
                }
                else if (webpartZone.ZoneType == WebpartType.EditableText && editableWebParts.ContainsKey(webpartZone.EditableWebpartInstance?.ControlID.ToLowerInvariant() ?? ""))
                {
                    // Add single Editable Text
                    var editableTextInstance = webpartZone.EditableWebpartInstance;
                    var editableTextContent = editableWebParts[webpartZone.EditableWebpartInstance.ControlID.ToLowerInvariant()];

                    var editableTextPortalWidgets = GetEditableTextPartWidgets(eventArgs, editableTextInstance, editableTextContent, editableWebParts);
                    var widgetSection = new PortalWidgetSection()
                    {
                        IsDefault = true,
                        SectionZoneToWidgets = new List<List<PortalWidget>>()
                            {
                                editableTextPortalWidgets
                            }
                    };

                    webpartZone.WidgetSections.Add(widgetSection);
                }
            }

            var allTemplateZoneIds = eventArgs.PortalEngineData.TemplateZones.Where(x => x.WebpartZoneInstance != null).Select(x => x.WebpartZoneInstance.ZoneID.ToLowerInvariant());
            var allLayoutWebparts = pageTemplateInstance.WebPartZones.SelectMany(x => x.WebParts.Where(y => LayoutWidgetDictionary.Keys.Any(z => z.Equals(y.WebPartType, StringComparison.OrdinalIgnoreCase))));
            var allLayoutWebpartNames = allLayoutWebparts.Select(x => x.WebPartType).ToList();

            var allNonLayoutWebparts = pageTemplateInstance.WebPartZones.SelectMany(x => x.WebParts.Where(y => !LayoutWidgetDictionary.Keys.Any(z => z.Equals(y.WebPartType, StringComparison.OrdinalIgnoreCase))));
            var allNonLayoutWebpartsNames = allNonLayoutWebparts.Select(x => x.WebPartType);

            List<string> fakeLayoutControlIds = new List<string>();
            Dictionary<string, List<PortalWidgetSection>> parentControlIDToPortalWidgetSections = new Dictionary<string, List<PortalWidgetSection>>();

            Dictionary<string, int> MultiZoneSectionWithCurrentZoneCount = new Dictionary<string, int>();

            List<string> ZoneIdsWithNoParent = new List<string>();
            
            // Build Zone and Parent Control ID Dictionaries
            Dictionary<string, List<string>> parentControlIDToZoneIDs = new Dictionary<string, List<string>>();
            Dictionary<string, string> zoneIdToParentControlID = new Dictionary<string, string>();
            Dictionary<string, WebPartInstance> zoneIdToParentControl = new Dictionary<string, WebPartInstance>();
            Dictionary<string, string> controlIdToParentControlID = new Dictionary<string, string>();
            Dictionary<string, string> controlIdToZoneId = new Dictionary<string, string>();
           
            foreach (var zone in pageTemplateInstance.WebPartZones.Where(x => !allTemplateZoneIds.Contains(x.ZoneID.ToLowerInvariant()))
                .OrderBy(x => x.ZoneID))
            {
                string zoneId = zone.ZoneID.ToLowerInvariant();
                // Get parent control ID
                PortalToMVCBuildPageFindSectionWebpartEventArgs findSectionEventArgs = FindParentSection(zone, allLayoutWebparts);

                if (findSectionEventArgs.ParentLayoutWebpart == null)
                {
                    // orphan
                    eventArgs.ConversionNotes.Add(new ConversionNote()
                    {
                        IsError = false,
                        Source = "BuildPortalToMVCProcessDocumentPrimaryEventArgs",
                        Type = "OrphanedWebpartZone",
                        Description = $"Could not find parent for zone {zone.ZoneID} among all the layout webparts ({allLayoutWebparts.Select(x => x.ControlID).Join(", ")})."
                    });
                    ZoneIdsWithNoParent.Add(zone.ZoneID.ToLowerInvariant());
                }

                string parentControlId = findSectionEventArgs.ParentLayoutWebpart != null ? findSectionEventArgs.ParentLayoutWebpart.ControlID.ToLowerInvariant() : findSectionEventArgs.ParentControlID.ToLowerInvariant();

                if (!parentControlIDToZoneIDs.ContainsKey(parentControlId))
                {
                    parentControlIDToZoneIDs.Add(parentControlId, new List<string>());
                }
                parentControlIDToZoneIDs[parentControlId].Add(zoneId);
                zoneIdToParentControlID.Add(zoneId, parentControlId);
                zoneIdToParentControl.Add(zoneId, findSectionEventArgs.ParentLayoutWebpart);
                zone.WebParts.ForEach(x => controlIdToParentControlID.Add(x.ControlID.ToLowerInvariant(), parentControlId));
                zone.WebParts.ForEach(x => controlIdToZoneId.Add(x.ControlID.ToLowerInvariant(), zoneId));
            }

            // loop through all layout widgets that have both layout widgets and non layout widgets, and wrap the non layout widgets in a fake default layout widget.
            foreach (var zone in pageTemplateInstance.WebPartZones.Where(x => 
                !ZoneIdsWithNoParent.Contains(x.ZoneID.ToLowerInvariant()) 
                && !allTemplateZoneIds.Contains(x.ZoneID.ToLowerInvariant()) 
                && x.WebParts.Any(y => allNonLayoutWebpartsNames.Contains(y.WebPartType, StringComparer.OrdinalIgnoreCase)) 
                && x.WebParts.Any(y => allLayoutWebpartNames.Contains(y.WebPartType, StringComparer.OrdinalIgnoreCase))
                )
                .OrderBy(x => x.ZoneID))
            {
                string zoneId = zone.ZoneID.ToLowerInvariant();
                List<WebPartInstance> newWebpartInstanceList = new List<WebPartInstance>();

                if (zoneIdToParentControlID.ContainsKey(zone.ZoneID.ToLowerInvariant()))
                {
                    string parentControlId = zoneIdToParentControlID[zone.ZoneID.ToLowerInvariant()];

                    foreach (var webpartInstance in zone.WebParts)
                    {
                        string webpartInstanceControlId = webpartInstance.ControlID.ToLowerInvariant();
                        if (allLayoutWebpartNames.Contains(webpartInstance.WebPartType, StringComparer.OrdinalIgnoreCase))
                        {
                            newWebpartInstanceList.Add(webpartInstance);
                        }
                        else if (allNonLayoutWebpartsNames.Contains(webpartInstance.WebPartType, StringComparer.OrdinalIgnoreCase))
                        {
                            // Add to the layout webpart names
                            if (!allLayoutWebpartNames.Contains("DEFAULT_SECTION"))
                            {
                                allLayoutWebpartNames.Add("DEFAULT_SECTION");
                            }

                            // fake a layout widget then add that fake.
                            var fakeControlID = $"default_section_{Guid.NewGuid().ToString().Replace("-", "")}".ToLowerInvariant();
                            var fakeWebpartInstance = new WebPartInstance()
                            {
                                WebPartType = "DEFAULT_SECTION",
                                ControlID = fakeControlID,
                                ParentZone = zone,
                            };
                            fakeLayoutControlIds.Add(fakeControlID);
                            newWebpartInstanceList.Add(fakeWebpartInstance);

                            // Adjust control to parent dictionaries
                            controlIdToParentControlID.Remove(webpartInstanceControlId);
                            controlIdToParentControlID.Add(webpartInstanceControlId, fakeControlID);
                            controlIdToParentControlID.Add(fakeControlID, parentControlId);
                            

                            // Add the fake section to the webpart zones of the page template instance
                            string fakeZoneID = fakeControlID + "_1";
                            pageTemplateInstance.WebPartZones.Add(new WebPartZoneInstance()
                            {
                                ZoneID = fakeZoneID,
                                WebParts = new List<WebPartInstance>() { webpartInstance }
                            });

                            controlIdToZoneId.Remove(webpartInstanceControlId);
                            controlIdToZoneId.Add(webpartInstanceControlId, fakeZoneID);
                            controlIdToZoneId.Add(fakeControlID, zoneId);

                            // Add to primary dictionaries
                            parentControlIDToZoneIDs.Add(fakeControlID, new List<string>() { fakeZoneID });
                            zoneIdToParentControlID.Add(fakeZoneID, fakeControlID);
                        }
                    }

                    // Add back the webpart zones
                    zone.WebParts = newWebpartInstanceList;
                }
            }

            // loop through any layout widgets now that have section with only layouts and sections only non-layout, and make the non-layout also fake sections
            var parentControlIdToZoneIDKeys = parentControlIDToZoneIDs.Keys.ToArray();
            foreach (var parentControlId in parentControlIdToZoneIDKeys)
            {
                bool hasZonesWithOnlyLayout = pageTemplateInstance.WebPartZones.Any(x => 
                    parentControlIDToZoneIDs[parentControlId].Contains(x.ZoneID.ToLowerInvariant())
                    && x.WebParts.Any() 
                    && x.WebParts.All(y => allLayoutWebpartNames.Contains(y.WebPartType, StringComparer.OrdinalIgnoreCase))
                );
                if (hasZonesWithOnlyLayout)
                {
                    var zonesNeedingLayoutWrapper = pageTemplateInstance.WebPartZones.Where(x =>
                        parentControlIDToZoneIDs[parentControlId].Contains(x.ZoneID.ToLowerInvariant())
                        && (!x.WebParts.Any() || x.WebParts.All(y => allNonLayoutWebpartsNames.Contains(y.WebPartType, StringComparer.OrdinalIgnoreCase)))).ToArray();
                    for(int i=0; i< zonesNeedingLayoutWrapper.Length; i++)
                    {
                        var zoneNeedingLayoutWrapper = zonesNeedingLayoutWrapper[i];
                        // Add to the layout webpart names
                        if (!allLayoutWebpartNames.Contains("DEFAULT_SECTION"))
                        {
                            allLayoutWebpartNames.Add("DEFAULT_SECTION");
                        }

                        // fake a layout widget then add that fake.
                        var fakeControlID = $"default_section_{Guid.NewGuid().ToString().Replace("-", "")}".ToLowerInvariant();
                        var fakeWebpartInstance = new WebPartInstance()
                        {
                            WebPartType = "DEFAULT_SECTION",
                            ControlID = fakeControlID,
                            ParentZone = zoneNeedingLayoutWrapper,
                        };
                        fakeLayoutControlIds.Add(fakeControlID);


                        // Add the fake section to the webpart zones of the page template instance
                        string fakeZoneID = fakeControlID + "_1";
                        pageTemplateInstance.WebPartZones.Add(new WebPartZoneInstance()
                        {
                            ZoneID = fakeZoneID,
                            WebParts = zoneNeedingLayoutWrapper.WebParts
                        });

                        // Adjust control to parent dictionaries
                        zoneNeedingLayoutWrapper.WebParts.ForEach(x =>
                        {
                            controlIdToParentControlID.Remove(x.ControlID.ToLowerInvariant());
                            controlIdToParentControlID.Add(x.ControlID.ToLowerInvariant(), fakeControlID);
                            controlIdToZoneId.Remove(x.ControlID.ToLowerInvariant());
                            controlIdToZoneId.Add(x.ControlID.ToLowerInvariant(), fakeZoneID);
                        });
                        controlIdToZoneId.Add(fakeControlID, zoneNeedingLayoutWrapper.ZoneID.ToLowerInvariant());

                        // Set zone to just the new fake webpart instance
                        zoneNeedingLayoutWrapper.WebParts = new List<WebPartInstance>() { fakeWebpartInstance };

                        // Add to primary dictionaries
                        parentControlIDToZoneIDs.Add(fakeControlID, new List<string>() { fakeZoneID });
                        zoneIdToParentControlID.Add(fakeZoneID, fakeControlID);
                    }
                }
            }


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // At this point, each widget zone has either NO layout sections, or has a Layout Section with either ONLY widgets or ONLY Layout Sections within them.   ///
            // This means that we should be able to processes only Widget Zones that have NO Layouts in them 
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // loop through all layout widget derived zones
            foreach (var zone in pageTemplateInstance.WebPartZones.Where(x => 
                !ZoneIdsWithNoParent.Contains(x.ZoneID.ToLowerInvariant()) 
                && !allTemplateZoneIds.Contains(x.ZoneID.ToLowerInvariant())
                && zoneIdToParentControlID.ContainsKey(x.ZoneID.ToLowerInvariant())
                )
                .OrderBy(x => x.ZoneID)
            )
            {
                string zoneId = zone.ZoneID.ToLowerInvariant();
                
                string parentControlID = zoneIdToParentControlID[zoneId];
                WebPartInstance parentWebpartInstance = zoneIdToParentControl.ContainsKey(zoneId) ? zoneIdToParentControl[zoneId] : null;
                bool isDefaultSection = parentWebpartInstance == null;

                // Add to the found zone list
                documentZonesFound.Add(zoneId);

                // If this webpart zone is only Layouts, then add to ControlIDs with Layout Children and skip, this is not a "bottom Level"
                if (zone.WebParts.Any() && zone.WebParts.All(y => allLayoutWebpartNames.Contains(y.WebPartType, StringComparer.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Processes bottom level
                if (!parentControlIDToPortalWidgetSections.ContainsKey(parentControlID))
                {
                    parentControlIDToPortalWidgetSections.Add(parentControlID, new List<PortalWidgetSection>()
                    {
                        new PortalWidgetSection()
                        {
                            SectionWidget = parentWebpartInstance,
                            IsDefault = isDefaultSection,
                            SectionZoneToWidgets = new List<List<PortalWidget>>()
                        }
                    });
                }

                var section = parentControlIDToPortalWidgetSections[parentControlID][0];

                // Handle "empty" zones before other zones for sections with multiple zones (ex a bootstrap layout with 4 zones, the first empty, the rest have content)
                if (section != null)
                {
                    var count = zone.ZoneID.ToLower().Replace(parentControlID + "_", "");
                    if (int.TryParse(count, out int zoneNum))
                    {
                        if (!MultiZoneSectionWithCurrentZoneCount.ContainsKey(parentControlID))
                        {
                            MultiZoneSectionWithCurrentZoneCount.Add(parentControlID, 1);
                        }

                        int curNum = MultiZoneSectionWithCurrentZoneCount[parentControlID];

                        for (int i = curNum; i < zoneNum; i++)
                        {
                                section.SectionZoneToWidgets.Add(new List<PortalWidget>());
                        }
                        MultiZoneSectionWithCurrentZoneCount[parentControlID] = zoneNum+1;
                    }
                }

                // Add zone to the section
                section.SectionZoneToWidgets.Add(zone.WebParts.SelectMany(x => GetBasicWidgetToPortalWidgets(eventArgs, x, editableWebParts)).ToList());
            }

            var currentLevelProcessedParentControlIds = new List<string>();
            var allLayoutWebPartControlIDs = allLayoutWebparts.Select(x => x.ControlID.ToLowerInvariant()).Union(fakeLayoutControlIds);
            var controlIdsToCheck = parentControlIDToPortalWidgetSections.Keys.ToList();
            List<string> controlIdsChecked = new List<string>();
            // Need to take the parentControlIDToPortalWidget and start combining with any sibling widgets
            bool nextLevelFound = true;
            while (nextLevelFound)
            {
                nextLevelFound = false;
                // On First Level (0), also exclude any Controls that had Child layouts, even if they also had non child layouts.

                WebPartInstance parentControl;
                string documentZoneFound = string.Empty;
                // Get any Webpart Zones that contain these controls and not part of the template zones (AKA are a widget generated zone)

                foreach (var widgetGeneratedZone in pageTemplateInstance.WebPartZones.Where(x =>
                    zoneIdToParentControlID.ContainsKey(x.ZoneID.ToLowerInvariant())
                    && !allTemplateZoneIds.Contains(x.ZoneID, StringComparer.OrdinalIgnoreCase) 
                    && x.WebParts.Any(wp => controlIdsToCheck.Contains(wp.ControlID.ToLowerInvariant())))
                    )
                {
                    string zoneId = widgetGeneratedZone.ZoneID.ToLowerInvariant();
                    string parentControlIdLookup = zoneIdToParentControlID[zoneId];
                    documentZonesFound.Add(zoneId);
                    parentControl = zoneIdToParentControl.ContainsKey(zoneId) ? zoneIdToParentControl[zoneId] : null;

                    // Skip this if there are any layout webparts that aren't in the controlIdsToCheck, must do bottom level first only
                    var parentsWebpartZones = pageTemplateInstance.WebPartZones.Where(x =>
                        parentControlIDToZoneIDs[parentControlIdLookup].Contains(x.ZoneID.ToLowerInvariant())
                        );
                    if (parentsWebpartZones.SelectMany(x => x.WebParts).Any(wp =>
                        allLayoutWebPartControlIDs.Contains(wp.ControlID.ToLowerInvariant())
                        && !controlIdsToCheck.Union(controlIdsChecked).Contains(wp.ControlID.ToLowerInvariant()))
                      )
                    {
                        continue;
                    }

                    // At this point is the 'next level'
                    nextLevelFound = true;

                    // Add parent control to the full listing
                    if (!parentControlIDToPortalWidgetSections.ContainsKey(parentControlIdLookup))
                    {
                        parentControlIDToPortalWidgetSections.Add(parentControlIdLookup, new List<PortalWidgetSection>());
                        controlIdsToCheck.Add(parentControlIdLookup);
                    }

                    var parentSections = parentControlIDToPortalWidgetSections[parentControlIdLookup];

                    // Loop through webparts and add them to the new parent section
                    var groupedWidgetSection = new PortalWidgetSection()
                    {
                        IsDefault = true,
                        SectionWidget = null,
                        SectionZoneToWidgets = new List<List<PortalWidget>>() { new List<PortalWidget>() }
                    };

                    foreach (var webpart in widgetGeneratedZone.WebParts)
                    {
                        var webpartLookup = webpart.ControlID.ToLowerInvariant();
                        if (controlIdsToCheck.Contains(webpartLookup))
                        {
                            controlIdsToCheck.Remove(webpartLookup);
                            controlIdsChecked.Add(webpartLookup);

                            // Add previously grouped to array
                            if (groupedWidgetSection.SectionZoneToWidgets[0].Any())
                            {
                                parentSections.Add(groupedWidgetSection);
                                groupedWidgetSection = new PortalWidgetSection()
                                {
                                    IsDefault = true,
                                    SectionWidget = null,
                                    SectionZoneToWidgets = new List<List<PortalWidget>>() { new List<PortalWidget>() }
                                };
                            }

                            // Add layout webpart section to new parent
                            parentControlIDToPortalWidgetSections[webpartLookup].ForEach(l => l.SectionZoneToWidgets.ForEach(sw => sw.ForEach(pw => pw.AdditionalAncestorZones.Add(parentControl))));
                            parentSections.AddRange(parentControlIDToPortalWidgetSections[webpartLookup]);
                            currentLevelProcessedParentControlIds.Add(webpartLookup);
                        }
                        else
                        {
                            // Add the widget to the grouping
                            var widgets = GetBasicWidgetToPortalWidgets(eventArgs, webpart, editableWebParts);
                            widgets.ForEach(x => x.AdditionalAncestorZones.Add(parentControl));
                            groupedWidgetSection.SectionZoneToWidgets.Add(widgets);
                        }
                    }
                    if (groupedWidgetSection.SectionZoneToWidgets[0].Any())
                    {
                        parentSections.Add(groupedWidgetSection);
                    }

                    
                }
            }
            // At this point, the parentTemplateZoneIDToPortalWidget should contain all the layout widgets with sub widgets in order.

            // lastly, loop through only the template level webpart zones and add all the sections to them
            foreach (var templateZone in eventArgs.PortalEngineData.TemplateZones.Where(x => x.ZoneType == WebpartType.Webpart))
            {
                var lookup = templateZone.WebpartZoneInstance.ZoneID.ToLowerInvariant();
                var webpartZone = pageTemplateInstance.WebPartZones.Where(x => x.ZoneID.Equals(lookup, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var widgetSections = new List<PortalWidgetSection>();
                var groupedWidgetSection = new PortalWidgetSection()
                {
                    IsDefault = true,
                    SectionZoneToWidgets = new List<List<PortalWidget>>() { new List<PortalWidget>() }
                };

                // Only processes if webpart zone found in page
                if (webpartZone != null)
                {
                    documentZonesFound.Add(lookup);
                    foreach (var widget in webpartZone.WebParts)
                    {
                        var widgetLookup = widget.ControlID.ToLowerInvariant();
                        if (!parentControlIDToPortalWidgetSections.ContainsKey(widgetLookup))
                        {
                            groupedWidgetSection.SectionZoneToWidgets[0].AddRange(GetBasicWidgetToPortalWidgets(eventArgs, widget, editableWebParts));
                        }
                        else
                        {
                            // is a layout, add any current widgets in the default section, reinitialize, then add the ones in this layout
                            if (groupedWidgetSection.SectionZoneToWidgets[0].Any())
                            {
                                templateZone.WidgetSections.Add(groupedWidgetSection);
                                groupedWidgetSection = new PortalWidgetSection()
                                {
                                    IsDefault = true,
                                    SectionZoneToWidgets = new List<List<PortalWidget>>() { new List<PortalWidget>() }
                                };
                            }
                            templateZone.WidgetSections.AddRange(parentControlIDToPortalWidgetSections[widgetLookup]);
                        }
                    }
                }

                // Add any remaining current widgets in default section
                if (groupedWidgetSection.SectionZoneToWidgets[0].Any())
                {
                    templateZone.WidgetSections.Add(groupedWidgetSection);
                }
            }

            var orphanedPageZones = pageTemplateInstance.WebPartZones.Select(x => x.ZoneID.ToLower()).Except(documentZonesFound).ToList();
            if (orphanedPageZones.Any())
            {
                eventArgs.ConversionNotes.Add(new ConversionNote()
                {
                    IsError = false,
                    Source = "BuildPortalToMVCProcessDocumentPrimaryEventArgs",
                    Type = "OrphanedWebpartZones",
                    Description = $"Could not find the following webpart zones [{orphanedPageZones.Join(", ")}] among all the layout webparts ({allLayoutWebparts.Select(x => x.ControlID).Join(", ")})."
                });
            }

            return eventArgs;
        }

        #endregion

        #region "Helpers / Others"

        private PortalToMVCBuildPageFindSectionWebpartEventArgs FindParentSection(WebPartZoneInstance widgetGeneratedZone, IEnumerable<WebPartInstance> allLayoutWebparts)
        {
            PortalToMVCBuildPageFindSectionWebpartEventArgs findSectionEventArgs = new PortalToMVCBuildPageFindSectionWebpartEventArgs(widgetGeneratedZone, allLayoutWebparts)
            {
                ParentLayoutWebpart = null
            };

            // ProcessPage.Before
            using (var findParentSectionWebpart = PortalToMVCEvents.FindParentSectionWebpart.StartEvent(findSectionEventArgs))
            {
                if (!findSectionEventArgs.Handled)
                {
                    // Go by parent control next
                    var zoneArray = widgetGeneratedZone.ZoneID.Split('_');
                    string parentControlIdLookup = zoneArray.Length > 1 ? zoneArray.Take(zoneArray.Length - 1).Join("_").ToLowerInvariant() : widgetGeneratedZone.ZoneID.ToLowerInvariant();
                    var sectionWebpart = allLayoutWebparts.Where(x => x.ControlID.Equals(parentControlIdLookup, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    findSectionEventArgs.ParentControlID = parentControlIdLookup;
                    findSectionEventArgs.ParentLayoutWebpart = sectionWebpart;
                }

                // ProcessPage.After
                findParentSectionWebpart.FinishEvent();
            }
            return findSectionEventArgs;
        }
        private string GetPEZoneID(PortalWebpartZone peZoneEditableArea)
        {
            var peZoneID = string.Empty;
            switch (peZoneEditableArea.ZoneType)
            {
                case WebpartType.Webpart:
                    peZoneID = peZoneEditableArea.WebpartZoneInstance.ZoneID;
                    break;
                case WebpartType.EditableText:
                case WebpartType.EditableImage:
                    peZoneID = peZoneEditableArea.EditableWebpartInstance.ControlID;
                    break;
            }
            return peZoneID;
        }

        private string GetPBEditableAreaIdentifier(PortalWebpartZone peZoneEditableArea, TemplateConfiguration templateConfig)
        {
            var peZoneID = GetPEZoneID(peZoneEditableArea);
            var zoneConfiguration = templateConfig?.Zones.Where(x => x.PE_WebpartZoneName.Equals(peZoneID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            return zoneConfiguration != null ? zoneConfiguration.PB_EditableAreaName.Replace("INHERIT", zoneConfiguration.PE_WebpartZoneName) : (IgnoreIfMissingInConfig ? String.Empty : peZoneID);
        }

        private List<PortalWidget> GetBasicWidgetToPortalWidgets(PortalToMVCProcessDocumentPrimaryEventArgs eventArgs, WebPartInstance widget, MultiKeyDictionary<string> editableWebParts)
        {
            List<PortalWidget> widgets = new List<PortalWidget>();
            var configuration = WidgetTypeToConfiguration.ContainsKey(widget.WebPartType.ToLowerInvariant()) ? WidgetTypeToConfiguration[widget.WebPartType.ToLowerInvariant()] : null;

            if (configuration == null || !configuration.PE_Widget.KeyValues.Any(x => x.CanContainInlineWidgets))
            {
                widgets.Add(GetBasicWidgetToPortalWidget(eventArgs, widget, editableWebParts));
                return widgets;
            }

            // There are possible inline widget fields, get values and split if needed.
            var propertyWithInlineWidget = configuration.PE_Widget.KeyValues.First(x => x.CanContainInlineWidgets);

            var value = editableWebParts[widget.ControlID] != null ? editableWebParts[widget.ControlID] : widget.GetValue(propertyWithInlineWidget.Key);

            if (value != null && value.ToString().Contains("{^"))
            {
                widgets.AddRange(ConvertInlinedContentToPortalWidgets(eventArgs, widget, propertyWithInlineWidget, value.ToString(), editableWebParts));
                return widgets;
            }

            widgets.Add(GetBasicWidgetToPortalWidget(eventArgs, widget, editableWebParts));
            return widgets;

        }

        private List<PortalWidget> ConvertInlinedContentToPortalWidgets(PortalToMVCProcessDocumentPrimaryEventArgs eventArgs, WebPartInstance widget, InKeyValueOutKeyValues conversionKey, string contentWithInlineWidgets, MultiKeyDictionary<string> editableWebParts)
        {
            List<PortalWidget> widgets = new List<PortalWidget>();
            // At this point we need to split.
            string strValue = contentWithInlineWidgets;
            List<InlineWidgetPortion> inlineWidgetPortions = new List<InlineWidgetPortion>();
            while (strValue.Length > 0)
            {
                int inlineWidgetIndex = strValue.IndexOf("{^");
                if (inlineWidgetIndex == -1)
                {
                    inlineWidgetPortions.Add(new InlineWidgetPortion()
                    {
                        IsInlineWidget = false,
                        Value = strValue
                    });
                    strValue = String.Empty;
                }
                else
                {
                    string beforeText = strValue.Substring(0, inlineWidgetIndex);
                    inlineWidgetPortions.Add(new InlineWidgetPortion()
                    {
                        IsInlineWidget = false,
                        Value = beforeText
                    });

                    strValue = strValue.Substring(beforeText.Length);

                    int inlineWidgetEndIndex = strValue.IndexOf("^}") + 2;
                    string inlineWidgetVal = strValue.Substring(0, inlineWidgetEndIndex);
                    var inlineWidget = GetInlineWidgetPortion(eventArgs, widget, inlineWidgetVal);
                    if (inlineWidget != null)
                    {
                        inlineWidgetPortions.Add(inlineWidget);
                    }
                    else
                    {
                        eventArgs.ConversionNotes.Add(new ConversionNote()
                        {
                            IsError = false,
                            Source = "ConvertInlineContentToPortalWidget",
                            Type = "InlineWidgetNameNotFound",
                            Description = $"Could not parse widget name from expression {inlineWidgetVal}"
                        });
                    }
                    strValue = strValue.Substring(inlineWidgetEndIndex);
                }
            }

            // Only select Html inline widgets, inline widgest without configuration, or those with configuration but set to not ignore.
            inlineWidgetPortions = inlineWidgetPortions.Where(x => !x.IsInlineWidget || !WidgetTypeToConfiguration.ContainsKey(x.WidgetName.ToLowerInvariant()) || WidgetTypeToConfiguration[x.WidgetName.ToLowerInvariant()].PE_Widget.RenderIfInline).ToList();

            int inlineWidgetPortionCount = inlineWidgetPortions.Count(x => x.IsInlineWidget);


            // If the property is set to remove all inline widgets, then remove and add the widget.
            if (conversionKey.InlineWidgetMode.Equals("REMOVE", StringComparison.OrdinalIgnoreCase) || inlineWidgetPortionCount == 0)
            {
                // Remove inline widgets and return
                widget.SetValue(conversionKey.Key, inlineWidgetPortions.Where(x => !x.IsInlineWidget).Select(x => x.Value).Join(""));
                widgets.Add(GetBasicWidgetToPortalWidget(eventArgs, widget, editableWebParts));
                return widgets;
            }

            // Remove normal whitespace items
            inlineWidgetPortions = inlineWidgetPortions.Where(x => x.IsInlineWidget || x.Value.Replace("&nbsp;", "").Trim().Length > 0).ToList();

            // If remaining HTML items are 'whitespace' (no text / script / styles, ignoring normal html tags) then just render the inline widgets
            if (!inlineWidgetPortions.Any(x => !x.IsInlineWidget && HtmlCleanup(x.Value).Trim().Length > 0))
            {
                // Only Inline widgets and whitespace, remove rest
                foreach (var inlineWidget in inlineWidgetPortions.Where(x => x.IsInlineWidget))
                {
                    widgets.Add(GetBasicWidgetToPortalWidget(eventArgs, inlineWidget.WidgetInstance, editableWebParts));
                };
                return widgets;
            }

            // Processes inline widgets now...
            if (inlineWidgetPortionCount == 1 && conversionKey.InlineWidgetMode.Equals("WRAP", StringComparison.OrdinalIgnoreCase) && AllowsHtmlBeforeAfter(inlineWidgetPortions.First(x => x.IsInlineWidget).WidgetName))
            {
                string htmlBefore = "";
                string htmlAfter = "";
                WebPartInstance inlineInstance = null;
                foreach (var inlineWidgetPortion in inlineWidgetPortions)
                {
                    if (!inlineWidgetPortion.IsInlineWidget)
                    {
                        if (inlineInstance == null)
                        {
                            htmlBefore += inlineWidgetPortion.Value;
                        }
                        else
                        {
                            htmlAfter += inlineWidgetPortion.Value;
                        }
                    }
                    else
                    {
                        inlineInstance = inlineWidgetPortion.WidgetInstance;
                    }
                }
                var newWidget = GetBasicWidgetToPortalWidget(eventArgs, inlineInstance, editableWebParts);
                newWidget.HtmlBefore = htmlBefore;
                newWidget.HtmlAfter = htmlAfter;
                widgets.Add(newWidget);
            }
            else if (conversionKey.InlineWidgetMode.Equals("SPLIT", StringComparison.OrdinalIgnoreCase))
            {
                // Handle special case where the inline widgets are at the end as well
                bool contentBeforeInline = false;
                bool contentAfterInline = false;
                bool inlineHit = false;

                var isInlineAndConvertedWidgetList = new List<Tuple<bool, PortalWidget>>();
                // Split into the Static html widgets
                foreach (var inlineWidgetPortion in inlineWidgetPortions)
                {
                    if (inlineWidgetPortion.IsInlineWidget)
                    {
                        inlineHit = true;
                        isInlineAndConvertedWidgetList.Add(new Tuple<bool, PortalWidget>(true, GetBasicWidgetToPortalWidget(eventArgs, inlineWidgetPortion.WidgetInstance, editableWebParts)));
                    }
                    else
                    {

                        try
                        {
                            // Try closing the document tags
                            var doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(inlineWidgetPortion.Value);
                            var fixedHtml = doc.DocumentNode.OuterHtml;
                            if (!string.IsNullOrWhiteSpace(Regex.Replace(doc.DocumentNode.InnerText, @"\s+", "")))
                            {
                                if (!inlineHit)
                                {
                                    contentBeforeInline = true;
                                }
                                else
                                {
                                    contentAfterInline = true;
                                }
                                isInlineAndConvertedWidgetList.Add(new Tuple<bool, PortalWidget>(false, new PortalWidget()
                                {
                                    WidgetType = WebpartType.Webpart,
                                    Widget = GetStaticTextWidgetInstance(eventArgs, widget, fixedHtml),
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            // Add as static widget
                            isInlineAndConvertedWidgetList.Add(new Tuple<bool, PortalWidget>(false, new PortalWidget()
                            {
                                WidgetType = WebpartType.Webpart,
                                Widget = GetStaticTextWidgetInstance(eventArgs, widget, inlineWidgetPortion.Value)
                            }));
                        }
                    }
                }
                // If the inline widget was first, then content, or the inline widget was last and no more content, then adjust original widget to just be it's normal HTML, and then add the inline widgets before/after
                if (!(contentBeforeInline && contentAfterInline))
                {
                    var portalWidget = GetBasicWidgetToPortalWidget(eventArgs, widget, editableWebParts);
                    // If it's an editable text type, then put in adjusted content
                    if (!string.IsNullOrWhiteSpace(portalWidget.EditableText))
                    {
                        portalWidget.EditableText = inlineWidgetPortions.Where(x => !x.IsInlineWidget).Select(x => x.Value).Join("").Trim();
                    }

                    if (contentBeforeInline)
                    {
                        widgets.Add(portalWidget);
                        widgets.AddRange(isInlineAndConvertedWidgetList.Where(x => x.Item1).Select(x => x.Item2));
                    }
                    else
                    {
                        widgets.AddRange(isInlineAndConvertedWidgetList.Where(x => x.Item1).Select(x => x.Item2));
                        widgets.Add(portalWidget);
                    }
                }
                else
                {
                    // Add them in order
                    widgets.AddRange(isInlineAndConvertedWidgetList.Select(x => x.Item2));
                }
            }
            else
            {
                // ADD After as default
                widget.SetValue(conversionKey.Key, inlineWidgetPortions.Where(x => !x.IsInlineWidget).Select(x => x.Value).Join(""));

                var portalWidget = GetBasicWidgetToPortalWidget(eventArgs, widget, editableWebParts);
                // If it's an editable text type, then put in adjusted content
                if (!string.IsNullOrWhiteSpace(portalWidget.EditableText))
                {
                    portalWidget.EditableText = inlineWidgetPortions.Where(x => !x.IsInlineWidget).Select(x => x.Value).Join("");
                }
                widgets.Add(portalWidget);

                foreach (var inlineWidgetPortion in inlineWidgetPortions.Where(x => x.IsInlineWidget))
                {
                    widgets.Add(GetBasicWidgetToPortalWidget(eventArgs, inlineWidgetPortion.WidgetInstance, editableWebParts));
                }
            }
            return widgets;
        }

        private bool AllowsHtmlBeforeAfter(string widgetName)
        {
            var configuration = WidgetTypeToConfiguration.ContainsKey(widgetName.ToLowerInvariant()) ? WidgetTypeToConfiguration[widgetName.ToLowerInvariant()] : null;
            return configuration?.PE_Widget.IncludeHtmlBeforeAfter ?? false;
        }

        private InlineWidgetPortion GetInlineWidgetPortion(PortalToMVCProcessDocumentPrimaryEventArgs eventArgs, WebPartInstance originWidgetInstance, string inlineWidgetVal)
        {
            var inlineWidgetRegex = RegexHelper.GetRegex("(?:(?<type>%%control:)(?<macro>(?:[^%]|%[^%])+)%%)|(?:\\{(?<index>(?:\\([0-9]+\\))?)(?<type>\\^)(?<macro>(?:(?:(?!\\k<type>).)|(?:\\k<type>(?!\\k<index>\\})))*)(?:\\k<type>)(?:\\k<index>)(?:\\}))");
            var m = inlineWidgetRegex.Matches(inlineWidgetVal)[0];
            string type = m.Groups["type"].ToString();
            string expression = m.Groups["macro"].ToString();
            Hashtable parameters = null;

            string controlName = string.Empty;
            string controlParameter = null;

            // Old macro %%control:name?parameter%%
            if (type.StartsWith("%%", StringComparison.Ordinal))
            {
                // Check the parameter presence
                int queryIndex = expression.IndexOf("?", StringComparison.Ordinal);
                if (queryIndex >= 0)
                {
                    expression = expression.Substring(queryIndex + 1);
                    controlName = expression.Substring(0, queryIndex);
                }
            }

            // Parse inline parameters
            parameters = ParseInlineParameters(expression, ref controlParameter, ref controlName);


            // Could not find a name to the inline widget, ignore.
            if (string.IsNullOrWhiteSpace(controlName))
            {
                return null;
            }

            var inlineInstance = new WebPartInstance()
            {
                ControlID = $"inlinewidget_{controlName}_{Guid.NewGuid()}",
                InstanceGUID = Guid.NewGuid(),
                WebPartType = controlName,
                IsWidget = true,
                Minimized = false,
                CurrentVariantInstance = originWidgetInstance,
                ParentZone = originWidgetInstance.ParentZone,
                VariantID = 0,
                PartInstanceVariants = new List<WebPartInstance>(),
                Removed = false,
                VariantMode = VariantModeEnum.None
            };
            if (parameters != null)
            {
                foreach (DictionaryEntry parameter in parameters)
                {
                    if (!ValidationHelper.GetString(parameter.Key, "name").Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        inlineInstance.SetValue(parameter.Key.ToString(), (parameter.Value.GetType() == typeof(string) ? parameter.Value.ToString().Replace("+", " ") : parameter.Value));
                    }
                }
            }
            return new InlineWidgetPortion()
            {
                WidgetName = controlName,
                WidgetInstance = inlineInstance,
                IsInlineWidget = true,
                Value = inlineWidgetVal
            };
        }

        /// <summary>
        /// Parses inline macro values, creates collection of this parameters, sets control name and control parameter if is available.
        /// </summary>
        /// <param name="expression">Inline macro without starting {^ and trailing ^}</param>
        /// <param name="controlParameter">Control parameter</param>
        /// <param name="controlName">Control name</param>
        /// <returns>Returns collection of parameters</returns>
        private static Hashtable ParseInlineParameters(string expression, ref string controlParameter, ref string controlName)
        {
            if (!String.IsNullOrEmpty(expression))
            {
                Hashtable parameters = null;

                // Replace escaped slash due to parsing of parameters
                expression = expression.Replace("\\|", "##ESCSLASH##");

                // Parse the parameters
                int paramIndex = expression.LastIndexOf("|", StringComparison.Ordinal);
                while (paramIndex >= 0)
                {
                    // Get the parameter
                    string parameter = expression.Substring(paramIndex + 1).Replace("##ESCSLASH##", "\\|");

                    int nameStart = parameter.IndexOf("(", StringComparison.Ordinal) + 1;
                    int nameEnd = parameter.IndexOf(")", StringComparison.Ordinal);

                    if ((nameStart > 0) && (nameEnd >= 0))
                    {
                        // Create parameters array
                        if (parameters == null)
                        {
                            parameters = new Hashtable();
                        }

                        // Get the parameter parts
                        string parameterName = parameter.Substring(nameStart, nameEnd - nameStart);
                        string parameterValue = parameter.Substring(nameEnd + 1);

                        parameters[parameterName] = HttpUtility.UrlDecode(HTMLHelper.HTMLDecode(MacroProcessor.UnescapeParameterValue(parameterValue).Replace("+", " ")));
                    }
                    else
                    {
                        // Unnamed parameter - default one
                        controlParameter = HttpUtility.UrlDecode(HTMLHelper.HTMLDecode(MacroProcessor.UnescapeParameterValue(parameter).Replace("+", " ")));
                    }

                    // Get next parameter
                    expression = expression.Substring(0, paramIndex);
                    paramIndex = expression.LastIndexOf("|", StringComparison.Ordinal);
                }
                if (string.IsNullOrWhiteSpace(controlName) && parameters.ContainsKey("name"))
                {
                    controlName = ValidationHelper.GetString(parameters["name"], string.Empty);
                }

                return parameters;
            }

            return null;
        }

        private WebPartInstance GetStaticTextWidgetInstance(PortalToMVCProcessDocumentPrimaryEventArgs eventArgs, WebPartInstance originWidgetInstance, string value)
        {

            var splitConfiguration = WidgetTypeToConfiguration.ContainsKey("statictextwidget") ? WidgetTypeToConfiguration["statictextwidget"] : null;
            if (splitConfiguration == null || splitConfiguration.PB_Widget.PB_WidgetIdentifier.Equals("EnterPageWidgetIdentifierIfNotProvidedCantDoInlineWidgets", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("No Widget Configuration for statictextwidget provided, cannot split inline widgets.  Please either provide configuration or choose a method besides splitting.");
            }

            var staticInstance = new WebPartInstance()
            {
                ControlID = "inlinewidget_split_htmlcontent_" + Guid.NewGuid().ToString(),
                InstanceGUID = Guid.NewGuid(),
                WebPartType = "statictextWidget",
                IsWidget = true,
                Minimized = false,
                CurrentVariantInstance = originWidgetInstance,
                ParentZone = originWidgetInstance.ParentZone,
                VariantID = 0,
                PartInstanceVariants = new List<WebPartInstance>(),
                Removed = false,
                VariantMode = VariantModeEnum.None
            };
            staticInstance.SetValue("Text", value.Trim());
            return staticInstance;

        }

        private string HtmlCleanup(string htmlCode)
        {
            htmlCode = htmlCode.Replace("\n", " ");
            htmlCode = htmlCode.Replace("\t", " ");
            htmlCode = HTMLHelper.RegexHtmlToTextHead.Replace(htmlCode, " ");
            htmlCode = HTMLHelper.RegexHtmlToTextTags.Replace(htmlCode, " ");
            htmlCode = HttpUtility.HtmlDecode(htmlCode);
            htmlCode = HTMLHelper.RegexHtmlToTextWhiteSpace.Replace(htmlCode, " ");
            return htmlCode;
        }

        private PortalWidget GetBasicWidgetToPortalWidget(PortalToMVCProcessDocumentPrimaryEventArgs eventArgs, WebPartInstance widget, MultiKeyDictionary<string> editableWebParts)
        {
            if (!editableWebParts.ContainsKey(widget.ControlID.ToLowerInvariant()))
            {
                return new PortalWidget()
                {
                    AdditionalAncestorZones = new List<WebPartInstance>(),
                    Widget = widget,
                    WidgetType = WebpartType.Webpart
                };
            }
            else
            {

                if (widget.WebPartType.Equals("editableimage", StringComparison.OrdinalIgnoreCase))
                {
                    return GetEditableImagePortalWidget(widget, editableWebParts[widget.ControlID.ToLowerInvariant()], true);
                }
                else
                {
                    return GetEditableTextPortalWidget(widget, editableWebParts[widget.ControlID.ToLowerInvariant()]);
                }
            }
        }

        /// <summary>
        /// Checks for inline widgets and returns the Portal Webparts
        /// </summary>
        /// <param name="editableTextInstance"></param>
        /// <param name="editableTextContent"></param>
        /// <returns></returns>
        private List<PortalWidget> GetEditableTextPartWidgets(PortalToMVCProcessDocumentPrimaryEventArgs eventArgs, WebPartInstance editableTextInstance, string editableTextContent, MultiKeyDictionary<string> editableWebParts)
        {
            List<PortalWidget> widgets = new List<PortalWidget>();
            var configuration = WidgetTypeToConfiguration.ContainsKey(editableTextInstance.WebPartType.ToLower()) ? WidgetTypeToConfiguration[editableTextInstance.WebPartType.ToLowerInvariant()] : null;
            var inlineConfig = configuration?.PE_Widget.KeyValues.First(x => x.CanContainInlineWidgets);
            if (inlineConfig == null || !(editableTextContent?.Contains("{^") ?? false))
            {
                widgets.Add(new PortalWidget()
                {
                    AdditionalAncestorZones = new List<WebPartInstance>(),
                    WidgetType = WebpartType.EditableText,
                    Widget = editableTextInstance,
                    EditableText = editableTextContent
                });
            }
            else
            {
                widgets.AddRange(ConvertInlinedContentToPortalWidgets(eventArgs, editableTextInstance, inlineConfig, editableTextContent, editableWebParts));
            }
            return widgets;
        }

        /// <summary>
        /// This is a singleton, do NOT check for inline widgets within, not supported.
        /// </summary>
        /// <param name="editableTextInstance"></param>
        /// <param name="editableTextContent"></param>
        /// <returns></returns>
        private PortalWidget GetEditableTextPortalWidget(WebPartInstance editableTextInstance, string editableTextContent)
        {
            return new PortalWidget()
            {
                AdditionalAncestorZones = new List<WebPartInstance>(),
                WidgetType = WebpartType.Webpart,
                Widget = editableTextInstance,
                EditableText = editableTextContent
            };
        }

        private PortalWidget GetEditableImagePortalWidget(WebPartInstance editableImageInstance, string editableImageContentXml, bool isStandAlone = false)
        {
            XmlDocument imageData = new XmlDocument();
            imageData.LoadXml(editableImageContentXml);
            var urlNode = imageData.SelectSingleNode("//property[@name='imagepath']");
            var altTextNode = imageData.SelectSingleNode("//property[@name='alttext']");
            var imageUrl = (urlNode != null ? urlNode.InnerText : ValidationHelper.GetString(editableImageInstance.GetValue("DefaultImage"), ""));
            var imageAltText = (altTextNode != null ? altTextNode.InnerText : ValidationHelper.GetString(editableImageInstance.GetValue("AlternateText"), ""));
            return new PortalWidget()
            {
                AdditionalAncestorZones = new List<WebPartInstance>(),
                WidgetType = WebpartType.EditableImage,
                Widget = editableImageInstance,
                EditableImageUrl = imageUrl,
                EditableImageAlt = imageAltText
            };
        }

        private PortalData GetGeneralTemplateConfiguration(int templateId)
        {
            // if template not found, can't proceed
            if (!TemplateDictionary.ContainsKey(templateId))
            {
                return null;
            }
            var template = TemplateDictionary[templateId];
            var templateInstance = new PageTemplateInstance(template.WebParts);
            var templateZones = new List<PortalWebpartZone>();

            // Find all widget zones
            foreach (var zone in templateInstance.WebPartZones.Where(x => x.WidgetZoneType == WidgetZoneTypeEnum.Editor))
            {
                templateZones.Add(new PortalWebpartZone()
                {
                    ZoneType = WebpartType.Webpart,
                    WebpartZoneInstance = zone
                });
            }
            foreach (var editableText in templateInstance.WebPartZones.SelectMany(x => x.WebParts.Where(y => !y.IsWidget && y.WebPartType.Equals("editabletext", StringComparison.OrdinalIgnoreCase))))
            {
                templateZones.Add(new PortalWebpartZone()
                {
                    ZoneType = WebpartType.EditableText,
                    EditableWebpartInstance = editableText
                });
            }
            foreach (var editableImage in templateInstance.WebPartZones.SelectMany(x => x.WebParts.Where(y => !y.IsWidget && y.WebPartType.Equals("EditableImage", StringComparison.OrdinalIgnoreCase))))
            {
                templateZones.Add(new PortalWebpartZone()
                {
                    ZoneType = WebpartType.EditableImage,
                    EditableWebpartInstance = editableImage
                });
            }
            return new PortalData(template)
            {
                TemplateZones = templateZones
            };

        }

        #endregion

    }


    internal class PortalConversionProcessorContext
    {
        internal TreeNode document;
        internal PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCEventArgs;
        internal Dictionary<string, EditableAreaConfiguration> existingPBEditableAreas;

        public PortalConversionProcessorContext()
        {
        }
    }
}