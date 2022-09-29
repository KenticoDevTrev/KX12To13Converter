using CMS.DataEngine;
using CMS.Modules;
using CMS.SiteProvider;
using System;
using System.Web.Http;
using CMS.Core;
using KX12To13Converter.KX13Receiver;
using CMS;

[assembly: RegisterModule(typeof(KX13ConverterReceiverInitializationModule))]

namespace KX12To13Converter.KX13Receiver
{
    public class KX13ConverterReceiverInitializationModule : Module
    {
        public KX13ConverterReceiverInitializationModule() : base("KX13ConverterReceiverInitializationModule")
        {

        }

        protected override void OnInit()
        {
            base.OnInit();

            // Now register routes
            GlobalConfiguration.Configuration.Routes.MapHttpRoute("KX13ConverterReceiver", "kx12to13api/{action}", new { controller = "KX13ConverterReceiver" });

            var resourceInfoProvider = Service.Resolve<IResourceInfoProvider>();
            var settingsCategoryInfoProvider = Service.Resolve<ISettingsCategoryInfoProvider>();

            ResourceInfo converter = resourceInfoProvider.Get("KX12To13Converter");
            bool createSettings = false;
            if (converter == null)
            {
                createSettings = true;
                converter = new ResourceInfo();
                SetConverterResource(converter);
                resourceInfoProvider.Set(converter);

                var siteInfoProvider = Service.Resolve<ISiteInfoProvider>();
                var resourceSiteInfoProvider = Service.Resolve<IResourceSiteInfoProvider>();

                // Add to sites
                foreach (var site in siteInfoProvider.Get().TypedResult)
                {
                    resourceSiteInfoProvider.Add(converter.ResourceID, site.SiteID);
                }

                // Add Category Group
                var rootCategory = settingsCategoryInfoProvider.Get("CMS.Settings");

                var category = new SettingsCategoryInfo()
                {
                    CategoryDisplayName = "KX12To13ConverterReceiver",
                    CategoryName = "KX12To13ConverterReceiver",
                    CategoryParentID = rootCategory.CategoryID,
                    CategoryResourceID = converter.ResourceID,
                    CategoryIsGroup = false
                };
                settingsCategoryInfoProvider.Set(category);

                // Add Configurations
                var configurations = new SettingsCategoryInfo()
                {
                    CategoryDisplayName = "Configurations",
                    CategoryName = "KX12To13Configurations",
                    CategoryParentID = category.CategoryID,
                    CategoryResourceID = converter.ResourceID,
                    CategoryIsGroup = false
                };
                settingsCategoryInfoProvider.Set(category);

                var kx13ConversionReceiver = new SettingsCategoryInfo()
                {
                    CategoryDisplayName = "KX13 Conversion Receiver",
                    CategoryName = "KX13ConversionReceiver",
                    CategoryParentID = configurations.CategoryParentID,
                    CategoryResourceID = converter.ResourceID,
                    CategoryIsGroup = true,
                };
                settingsCategoryInfoProvider.Set(kx13ConversionReceiver);

                // Add hash code setting
                var hashSettingsKey = new SettingsKeyInfo()
                {
                    KeyName = "ConverterHashCode",
                    KeyDisplayName = "Converter Security Hash Value",
                    KeyDescription = "Generate a random string and enter it here, as well as on the KX13 instance, this will be used to encode and validate requests sent to the KX13 Receiver to update page builder data for the matching page.   Used for longer running upgrades that you want to possibly reconfigure and repush portal engine pages to page builder scripts.",
                    KeyType = "longtext",
                    KeyCategoryID = kx13ConversionReceiver.CategoryID,
                    KeyIsGlobal = false,
                    KeyIsHidden = false,
                    KeyExplanationText = "hover for instructions"
                };
                SettingsKeyInfoProvider.SetSettingsKeyInfo(hashSettingsKey);
            }
            else if (converter.ResourceVersion.StartsWith("12."))
            {
                createSettings = true;

                // Upgrade
                SetConverterResource(converter);
                resourceInfoProvider.Set(converter);

                // Delete UI Elements
                foreach(var childElement in UIElementInfoProvider.GetChildUIElements("KX12To13Converter", "UpgradeOperations"))
                {
                    childElement.Delete();
                }
                foreach (var childElement in UIElementInfoProvider.GetChildUIElements("KX12To13Converter", "UpgradeOperations"))
                {
                    childElement.Delete();
                }
                foreach (var childElement in UIElementInfoProvider.GetChildUIElements("KX12To13Converter", "PageConverter"))
                {
                    childElement.Delete();
                }
                foreach (var childElement in UIElementInfoProvider.GetChildUIElements("KX12To13Converter", "KX12To13Converter"))
                {
                    childElement.Delete();
                }
                UIElementInfoProvider.GetUIElementInfo("KX12To13Converter", "KX12To13Converter");

                // Delete un-needed settings
                foreach (var settingsKey in SettingsKeyInfoProvider.GetSettingsKeys()
                    .WhereIn(nameof(SettingsKeyInfo.KeyName), new string[]
                    {
                        "ConverterReceiverBaseUrl",
                        "AutomaticallyConvertAndSetSendQueue",
                        "ConversionSendQueueEnabled",
                        "ConverterTemplateConfigJson",
                        "ConverterDefaultSectionConfigJson",
                        "ConverterSectionConfigJson",
                        "ConverterWidgetConfigJson",
                        "IgnoreIfMissingInConfiguration"
                    }).TypedResult)
                {
                    settingsKey.Delete();
                }
                settingsCategoryInfoProvider.Get("PortalToPageBuilderConfigurations")?.Delete();

                
            }

            if(createSettings)
            {

                var configurations = settingsCategoryInfoProvider.Get("KX13ConversionReceiver");

                var enabledKey = new SettingsKeyInfo()
                {
                    KeyName = "KX12To13ConverterReceiverEnabled",
                    KeyDisplayName = "Converter Receiver Enabled",
                    KeyDescription = "If true, then this service can be hit by your KX12 instance to update pages.",
                    KeyType = "boolean",
                    KeyCategoryID = configurations.CategoryID,
                    KeyIsGlobal = false,
                    KeyIsHidden = false,
                    KeyValue = "True",
                    KeyDefaultValue = "True"
                };
                SettingsKeyInfoProvider.SetSettingsKeyInfo(enabledKey);

                var overwrite = new SettingsKeyInfo()
                {
                    KeyName = "KX12To13ConverterReceiverOverwriteEvenIfNewer",
                    KeyDisplayName = "Overwrite even if newer on KX13",
                    KeyDescription = "If the KX12 instance tries to push an update for a page, and that page was updated on KX13 then this needs to be true to overwrite.  Used to prevent pages already touched on KX13 from being overwritten when re-parsing conversions.",
                    KeyType = "boolean",
                    KeyCategoryID = configurations.CategoryID,
                    KeyIsGlobal = false,
                    KeyIsHidden = false,
                    KeyValue = "False",
                    KeyDefaultValue = "False"
                };
                SettingsKeyInfoProvider.SetSettingsKeyInfo(overwrite);

                var ignoreBeforeDate = new SettingsKeyInfo()
                {
                    KeyName = "KX12To13ConverterReceiverUpgradedDate",
                    KeyDisplayName = "Date of Upgrade (not new if prior)",
                    KeyDescription = "The date the site was upgraded to KX13, any documents updated prior to this will not be considered 'newly updated' for the 'Overwrite even if newer on KX13'",
                    KeyType = "text",
                    KeyCategoryID = configurations.CategoryID,
                    KeyIsGlobal = true,
                    KeyIsHidden = false,
                    KeyValue = DateTime.Now.ToString("MM/dd/yyyy"),
                    KeyExplanationText = "in MM/dd/YYYY format"
                };
                SettingsKeyInfoProvider.SetSettingsKeyInfo(ignoreBeforeDate);
            }

        }
        private void SetConverterResource(ResourceInfo converter)
        {
            converter.ResourceDisplayName = "KX12 to 13 Converter (Receiver)";
            converter.ResourceName = "KX12To13Converter";
            converter.ResourceDescription = "Receiver to receive converted documents from their KX12 instance.  Remove once site is fully upgraded.";
            converter.ShowInDevelopment = true;
            converter.ResourceUrl = "https://github.com/KenticoDevTrev/KX12To13Converter";
            converter.ResourceIsInDevelopment = true;
            converter.ResourceHasFiles = false;
            converter.ResourceVersion = "13.0.0";
            converter.ResourceAuthor = "Trevor Fayas @ Hbs.net";
            converter.ResourceInstallationState = "installed";
            converter.ResourceInstalledVersion = "13.0.0";
        }
    }
}
