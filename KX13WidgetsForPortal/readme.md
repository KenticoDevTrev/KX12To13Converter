# Widgets for Portal Conversion
There are some commonly used widgets / webparts in Portal Engine Sites.  When converting, the Widget Conversion JSON will allow you to cast your old portal engine Widgets into new Page Builder Widgets.

Here's the various configurations and code for you to help with these widgets.


## editabletext Webpart / Widget
EditableText webpart is often used to give the users a text box on a page (usually defined in the page template).  This should be mapped to the default Kentico Rich Text Widget as it provides the same on page WYSIWYG editing experience.

```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "editabletext",
          "IsInlineWidget": false,
          "IsEditorWidget": true,
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false,
          "KeyValues": [
            {
              "Key": "EditableText",
              "Value": null,
              "DefaultValue": null,
              "OutKey": "content",
              "CanContainInlineWidgets": true,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "EncodeText",
              "Value": "",
              "DefaultValue": "false",
              "OutKey": "IGNORE",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "HtmlAreaToolbar",
              "Value": "",
              "DefaultValue": "Full",
              "OutKey": "IGNORE",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "HtmlAreaToolbarLocation",
              "Value": "",
              "DefaultValue": "Out:FCKToolbar",
              "OutKey": "IGNORE",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Kentico.Widget.RichText",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```

## EditableImage
This allowed the user to insert an image (usually through a Webpart in a template, but could also be a widget).

It is recommended that you map these to the Static Image Widget found in `KX13WidgetsForPortal/Containered|NonContainered/Components/Widgets/StaticImageWidget`.  This widget was created to house these editable images.

```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "EditableImage",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": true, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false,  // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "EditableImageUrl",
              "Value": null,
              "DefaultValue": null,
              "OutKey": "image",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "ImageTitle",
              "Value": "",
              "DefaultValue": null,
              "OutKey": "imageTitle",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "AlternateText",
              "Value": "",
              "DefaultValue": null,
              "OutKey": "alternativeText",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "ImageWidth",
              "Value": "",
              "DefaultValue": null,
              "OutKey": "imageWidth",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "ImageHeight",
              "Value": "",
              "DefaultValue": null,
              "OutKey": "imageHeight",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "ImageCssClass",
              "Value": "",
              "DefaultValue": null,
              "OutKey": "imageCssClass",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "ImageStyle",
              "Value": "",
              "DefaultValue": null,
              "OutKey": "inlineStyle",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Generic.StaticImage",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }

```

## BizForm (online form)
This form can both be inline or as a widget, and should be mapped to Kentico's built in `Kentico.FormWidget`.  Note that conversions and form layout are not an option for the KX13 Form Builder, so only the Form name will be carried over.

```json
{
    "PE_Widget": {
      "PE_WidgetCodeName": "BizForm",
      "IsInlineWidget": true, // This value may be different on your instance
      "IsEditorWidget": false, // This value may be different on your instance
      "IncludeHtmlBeforeAfter": false,
      "IncludeWebpartContainerProperties": false,
      "RenderIfInline": true, // This value may be different on your instance
      "KeyValues": [
        {
          "Key": "BizFormName",
          "Value": "",
          "DefaultValue": null,
          "OutKey": "selectedForm",
          "CanContainInlineWidgets": false,
          "InlineWidgetMode": "ADDAFTER"
        },
        {
          "Key": "DefaultFormLayout",
          "Value": "",
          "DefaultValue": "Standard",
          "OutKey": "IGNORE",
          "CanContainInlineWidgets": false,
          "InlineWidgetMode": "ADDAFTER"
        },
        {
          "Key": "TrackConversionName",
          "Value": "",
          "DefaultValue": null,
          "OutKey": "IGNORE",
          "CanContainInlineWidgets": false,
          "InlineWidgetMode": "ADDAFTER"
        },
        {
          "Key": "ConversionValue",
          "Value": "",
          "DefaultValue": null,
          "OutKey": "IGNORE",
          "CanContainInlineWidgets": false,
          "InlineWidgetMode": "ADDAFTER"
        }
      ]
    },
    "PB_Widget": {
      "PB_WidgetIdentifier": "Kentico.FormWidget",
      "AdditionalKeyValues": {
        "IGNORE": ""
      }
    }
  }
```

## Richtext Widget
The Rich Text widget (based off of Static HTML webpart) allowed for rich text content to be entered through the widgets properties.  You have two options with this:

### Option 1: Map to Custom Rich Text Widget
Located under `KX13WidgetsForPortal/Containered|NonContainered/Components/Widgets/RichTextWidget` exists a widget that was built to be the 'replica' of this widget in functionality.  use this configuration:

```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "richtext",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": false, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false, // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "Text",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "html",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Generic.RichText",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```

### Option 2: Convert to Kentico Rich Text Widget
You can optionally make these widgets behave like inline WYSIWYG widgets by simply configuring to map to the normal Kentico Rich Text Widget.  Then content is editable not through the widget property:

```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "richtext",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": false, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false, // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "Text",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "content",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Kentico.Widget.RichText",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```

## StatictextWidget
This is both a widget (based on the Static HTML Webpart) as well as used by the Converter to split HTML segments into multiple widgets for inline widgets. 

For this widget, use the Generic.StaticText or Generic.StaticTextWithTag widgets (found in the `KX13WidgetsForPortal/Containered|NonContainered|Components/Widgets/StaticTextWidget|StaticTextWithTagWidget`).

These widgets both output static HTML, only differing in one has a spot to put the Tag value that the original Static text widget provided.
```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "statictextWidget",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": false, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false, // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "Text",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "text",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "Tag",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "tag",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Generic.StaticTextWithTag",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```
OR
```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "statictextWidget",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": false, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false, // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "Text",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "text",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "Tag",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "IGNORE",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Generic.StaticText",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```

## Text widget
This widget is identical in nature to the statictextWidget, so configure the same way:
```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "Text",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": false, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false, // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "Text",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "text",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "Tag",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "tag",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Generic.StaticTextWithTag",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```
OR
```json
{
        "PE_Widget": {
          "PE_WidgetCodeName": "Text",
          "IsInlineWidget": false, // This value may be different on your instance
          "IsEditorWidget": false, // This value may be different on your instance
          "IncludeHtmlBeforeAfter": false,
          "IncludeWebpartContainerProperties": false,
          "RenderIfInline": false, // This value may be different on your instance
          "KeyValues": [
            {
              "Key": "Text",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "text",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            },
            {
              "Key": "Tag",
              "Value": "",
              "DefaultValue": "",
              "OutKey": "IGNORE",
              "CanContainInlineWidgets": false,
              "InlineWidgetMode": "ADDAFTER"
            }
          ]
        },
        "PB_Widget": {
          "PB_WidgetIdentifier": "Generic.StaticText",
          "AdditionalKeyValues": {
            "IGNORE": ""
          }
        }
      }
```