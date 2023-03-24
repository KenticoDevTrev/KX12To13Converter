using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using KX12To13Converter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace KX12To13Converter.Base.PageOperations
{
    public class PreUpgrade4ConvertForms : IPreUpgrade4ConvertForms
    {

        public IEnumerable<string> GetAllFormControlNames()
        {
            var formControls = new List<string>();
            foreach (var classObj in DataClassInfoProvider.GetClasses()
                .Where("ClassID in (select FormClassid from CMS_Form)")
                .TypedResult)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(classObj.ClassFormDefinition);
                foreach (XmlNode controlName in doc.SelectNodes("//controlname"))
                {
                    formControls.Add(controlName.InnerText.ToLower());
                }
            }
            return formControls.Distinct().OrderBy(x => x).ToList();
        }

        public void ConvertForms(Dictionary<string, string> oldToNew, string defaultSectionIdentifier, string defaultSectionType)
        {
            foreach (var form in BizFormInfoProvider.GetBizForms())
            {
                var classObj = DataClassInfoProvider.GetDataClassInfo(form.FormClassID);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(classObj.ClassFormDefinition);


                // Loop through Forms and generate Form Builder json
                string formBuilderJson = $"{{    \"editableAreas\": [      {{        \"identifier\": \"{defaultSectionIdentifier}\",        \"sections\": [          {{            \"identifier\": \"{Guid.NewGuid()}\",            \"type\": \"{defaultSectionType}\",            \"zones\": [              {{                \"identifier\": \"{Guid.NewGuid()}\",                \"formComponents\": [                  ";
                List<string> formElements = new List<string>();
                foreach (XmlNode fieldNode in xmlDoc.SelectNodes("//field[@visible='true']"))
                {
                    var identifier = fieldNode.Attributes["guid"].Value;
                    formElements.Add($"{{                    \"identifier\": \"{identifier}\"                  }}");

                    // now update the Field Settings to set the controlname to unknown and add the componentidentifier
                    var controlNode = fieldNode.SelectSingleNode("./settings/controlname");
                    string oldName = controlNode.InnerText;
                    controlNode.InnerText = "unknown";
                    var settingsNode = fieldNode.SelectSingleNode("./settings");
                    var componentidentifierNode = xmlDoc.CreateNode(XmlNodeType.Element, "componentidentifier", xmlDoc.NamespaceURI);
                    componentidentifierNode.InnerText = oldToNew[oldName.ToLower()];
                    
                    // handle error in the Convert Forms default Components where i put the class name instead of the identity
                    switch (componentidentifierNode.InnerText)
                    {
                        case "CheckBoxComponent":
                        case "DropDownComponent":
                        case "EmailInputComponent":
                        case "TextAreaComponent":
                        case "RadioButtonsComponent":
                        case "MultipleChoiceComponent":
                        case "RecaptchaComponent":
                        case "TextInputComponent":
                        case "FileUploaderComponent":
                        case "USPhoneComponent":
                            componentidentifierNode.InnerText = $"Kentico.{componentidentifierNode.InnerText.Replace("Component", "")}";
                            break;
                    }

                    settingsNode.AppendChild(componentidentifierNode);
                }
                formBuilderJson += string.Join(",", formElements);
                formBuilderJson += $"                ]              }}            ]          }}        ]      }}    ]  }}";

                // Save the form as an MVC
                form.FormBuilderLayout = formBuilderJson;
                form.FormDevelopmentModel = 1;
                BizFormInfoProvider.SetBizFormInfo(form);

                // Update the class to have the proper identifier info
                classObj.ClassFormDefinition = xmlDoc.OuterXml;
                DataClassInfoProvider.SetDataClassInfo(classObj);
            }
            CacheHelper.ClearCache();
            SystemHelper.RestartApplication();
        }
    }
}
