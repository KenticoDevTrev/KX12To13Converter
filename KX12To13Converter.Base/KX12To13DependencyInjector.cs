using CMS;
using KX12To13Converter.Base.PageOperations;
using KX12To13Converter.Interfaces;
using CMS.Core;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;

[assembly: RegisterImplementation(typeof(IConversionProcessingMethods), typeof(ConversionProcessingMethods), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPortalEngineToMVCConverter), typeof(PortalEngineToMVCConverter), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPortalEngineToMVCConverterRetriever), typeof(PortalEngineToMVCConverterRetriever), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPreUpgrade1VersioningWorkflow), typeof(PreUpgrade1VersioningWorkflow), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPreUpgrade2RemoveClasses), typeof(PreUpgrade2RemoveClasses), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPreUpgrade3ConvertPageTypesForMVC), typeof(PreUpgrade3ConvertPageTypesForMVC), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPreUpgrade4ConvertForms), typeof(PreUpgrade4ConvertForms), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IPreUpgrade5SiteAndDBPrep), typeof(PreUpgrade5SiteAndDBPrep), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IRunConversionMethods), typeof(RunConversionMethods), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(ISectionConfigurationBuilderMethods), typeof(SectionConfigurationBuilderMethods), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(ITemplateConfigurationBuilderMethods), typeof(TemplateConfigurationBuilderMethods), Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IWidgetConfigurationBuilderMethods), typeof(WidgetConfigurationBuilderMethods), Priority = RegistrationPriority.SystemDefault)]
