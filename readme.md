


# Kentico Xperience 12 Portal to Kentico Xperience 13 Converter
This tool allows you to convert  your portal engine site to a KX13 upgradeable instance.

Kentico does not support migrating Kentico Xperience 12 Portal engine to Kentico Xperience 13, but it ***does*** support migrating a Kentico Xperience 12 MVC Site.  This tool does the heavy lifting of converting your portal engine site into a KX12 MVC Site so you can upgrade to KX13.  Features below.

# Installation
## KX12 Kentico Application ("Mother"/"Admin"):

1. Install `KX12To13Converter.Admin` Nuget Package on your Kentico Application
2. Either install the `KX12To13Converter.Base` Nuget Package on your Kentico Application, OR clone down the KX12To13Converter.Base class library from this repository and include it in your solution and reference it on your Kentico Application (for easier debugging).
3. You will probably also need to upgrade the `Kentico.Libraries library` to your KX12 instance Hotfix (since these packages depend on this library).
4. Rebuild your web application
5. Log into Kentico as a Global Administrator
6. Go to Modules
7. Search and edit `KX12 to 13 Converter`
8. Go to `Sites` and add to your site.

## KX13 Kentico Application ("Mother"/"Admin")
Once the upgrade is complete and your solution is hotfixed to at least 13.0.31, you can optionally install the [KX12To13Converter.KX13Receiver](https://www.nuget.org/packages/KX12To13Converter.KX13Receiver).  Installing this will install/upgrade the exsiting KX12To13Converter module and allow you to re-convert and push pages from your KX12 instance to your KX13.

The main purpose is that often times, post upgrade, you may realize that you had something misconfigured in the converter, or for long running upgrades, this allows you to push up 'new' pages (as long as there is a matching page on the KX13 side).

To install, simply install the [KX12To13Converter.KX13Receiver](https://www.nuget.org/packages/KX12To13Converter.KX13Receiver) on the KX13 CMSApp admin project and run your site.  This module does all operations in code only, so deploying to other environments if necessary will only require a code push.  Removing the nuget package removes the functionality.

Make sure that you configure the items in the settings, and that your hash values match between KX12 and KX13.

# Wiki
This repo has a [wiki](https://github.com/KenticoDevTrev/KX12To13Converter/wiki) with tips, tricks, sql queries and c# scripts to help you during this processes, please read through the [wiki](https://github.com/KenticoDevTrev/KX12To13Converter/wiki) before you begin your journey.

# Usage - Upgrade Operations
When attempting an upgrade, these Pre Upgrade Operations will help convert your portal engine site into a MVC-like site.  Each page has instructions on it to aid you.

**WARNING**: You should never perform these operations on a LIVE site.  Always clone the kentico instance you wish to upgrade and perform these steps on that clone, backing up along the way.

## 1 - Versioning and Workflow 
Version history should be eliminated prior to an upgrade, but before that occurs all documents must be in a checked in / published state.  This helps perform those operations.

## 2 - Remove Classes
Many page types in Portal engine were only to house Transformations, Queries, or were part of Kentico's default package.  This UI helps you find unused classes and optionally delete classes that you deem as obsolete.

## 3 - Convert Page Types
In KX12 MVC / KX13, all page types are either Content Only or Container Page Types. Additionally, during the upgrade to KX13 the upgrade tool looks for certain configurations in order to determine if Page Builder and/or Urls are enabled.  This UI allows you to properly convert and set these.   As well as alerting you to Container Page Types that may be important to the URL structure, which may need to be converted to a Content Only Page Type.

## 4 - Convert Forms
Only MVC Created forms are allowed during an upgrade (you can't upgrade at all if it detects any older forms).  This UI helps you convert your forms over to an MVC form, and map the form controls to your new components.

## 5 - Site and DB Prep
There are various old foreign key references and other database level issues that will stop an upgrade from occurring, this will correct these and also set your sites to "Content Only" (MVC) as the final step.

**********************
# Usage - Page Converter

Kentico Xperience 12 MVC / 13 have a drastically different structure for Page Templates, Sections, and Widgets than Portal Engine did.  However, much of the same data exists between the two.  The Page Converter system allows you to map Portal Engine Identities/Properties into your newly developed Page Builder Templates / Sections / Widgets.

It does this through a series of JSON structured configuration files (plus optional event hooks for further customization)

1. In the Admin, go to the `Page Converter` application.
2. Follow steps 1-3, generating your configuration files and storing them in a separate location.
3. Modify these Configurations according to the instructions on each page.
4. On Step 4, you can either manually paste your configurations, or you can store them in the Settings -> KX12 to 13 Converter -> Configurations
5. Select your Page/Pages and Conversion Mode.

## Conversion Modes
**Preview (Single Page)**:  Select a single page and the converted Template and Page Builder Widgets will show below.  This is useful to see how your configuration is working or if you already have an upgraded instance and want to do a database replace the `DocumentPageTemplateConfiguration` and `DocumentPageBuilderWidgets` fields.
**Convert and Store in Conversions Only**: This will processes the Page(s) you select and store them in the Conversions Table.  You can access this through the `KX12 to 13 Converter` -> `Conversions` Application.  Here you can see if they were successful, what conversion notes may exist, etc.
**Convert and Save to Document**: This should only be done on a cloned instance during your pre-upgrade work.  This will convert and update the `DocumentPageTemplateConfiguration` and `DocumentPageBuilderWidgets` fields on the document. (recommend you disable versioning first).
**Convert and Send to KX13 instance**: [Future] I'm working on a KX13 receiving module that you'll be able to 'push' the converted data from your KX12 to your KX13 instance, since most upgrades take a long time and often you want to adjust and re-test configurations.  This option will allow you to convert and send the files.

## Conversions Application
This UI shows you all the converted document status.  Here you can see the Conversion Notes and determine what may have went wrong, you can optionally re-try converting or send individual documents as well.

## Conversion Events
While the tool does a pretty good job of converting everything using the configuration file, sometimes you need to do some adjustments.  Below lists all the events you can implement through the [Global Events](https://docs.xperience.io/k12sp/custom-development/handling-global-events), found at `KX12To13Converter.Events.PortalToMVCEvents`.  

Each event has a Before and After, and each allows you to set a "Handle" boolean flag in the arguments to prevent default logic from occurring.

The Logic goes:
1. Execute the `Before` Event
2. If Handled, then skip the default logic
3. If not Handled, execute the default logic
4. Execute the `After` Event

* **ProcessPage**: When a document is first starting to processes
* **ProcessTemplate**: For when converting the Portal Template to the Page Builder Template
* **ProcessEditableArea**: For when converting a Template Webpart Zone  to Page Builder Editable Area
* **ProcessSection**: For when converting any Layout Widgets (ex: Bootstrap Layout) to Page Builder Sections
* **ProcessSectionZone**: For when converting any widget zones within those Layout Widgets to a Page Builder Section's Zones (ex the first, then second column of a bootstrap layout)
* **ProcessWidget**: For when converting a Portal Engine Widget to a Page Builder Widget

Also for your convenience, I have included a BootstrapLayout 3 to 4 conversion hook implementation since the formatting of those has changed.
```csharp 
PortalToMVCEvents.ProcessSection.After += ProcessSection_After_BootstrapLayout.ProcessSectionAfter_Bootstrap;
```

## Nuances / Conversion Info
Below are some nuances of the conversion processes:
* Since Portal Engine allows for any number of nested widget zones, and Page Builder does not, only the parent layout widget will be parsed into a section around the widgets.  Any ancestor layouts are listed in the ProcessWidget's event arguments if you need further adjustments.
* EditableText and EditableImages in the page templates are treated as Webpart Zones and a default section and Text/Image widget is placed within it.
* Since Inline Widgets do not exist in KX13, you can configure these to Ignore, Split, Wrap, or Add After (details on the Widget Configuration UI Step).  The Configurator has been updated to handle Split better, where it will detect inline widgets added at the END or BEGINNING of a rich text area and simply put them before / after the html content.  It will also, upon splitting, automatically resolve split end/start HTML tags to ensure the split out content is not broken HTML.  
* You can parse multiple zones into a single Editable Area if you wish.  The configuration has been updated to honor the **editable area order** in the configuration when mapping to a single zone, so if you combine "TopZone", "MiddleZone", and "BottomZone" into a single editable area, just make sure that they are defined in the Widget Configuration in that order.

# GUIDE ON UPGRADING
The [Wiki](https://github.com/KenticoDevTrev/KX12To13Converter/wiki) has an in depth guide to using this tool, and what to do during upgrade.  Please read this and follow along!

# Acknowledgement, Contributions, bug fixes and License

This tool is free for all to use, and probably my last major tool for the KX12 Portal Engine system.

# Compatibility
Can be used on any Kentico Xperience 12 Portal Site.
