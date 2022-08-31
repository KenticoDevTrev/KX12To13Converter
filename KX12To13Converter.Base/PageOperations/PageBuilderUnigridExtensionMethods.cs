using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using KX12To13Converter.Enums;
using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using Newtonsoft.Json;

namespace KX12To13Converter.Base.PageOperations
{
    public class PageBuilderUnigridExtensionMethods : IPageBuilderUnigridExtensionMethods
    {
        public Tuple<CustomMessageType, string> Control_OnAction(string actionName, object actionArgument)
        {
            if (actionName.Equals("#retry"))
            {
                int id = Convert.ToInt32(actionArgument);

                IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
                bool conversionResults = conversionProcessingMethods.ReProcesses(PageBuilderConversionsInfoProvider.GetPageBuilderConversionsInfo(id));
                if (conversionResults)
                {
                    return new Tuple<CustomMessageType, string>(CustomMessageType.Confirmation, "Conversion Successful");
                }
                else
                {
                    return new Tuple<CustomMessageType, string>(CustomMessageType.Error, "Conversion failed, see conversion notes for more details");
                }
            }
            else if (actionName.Equals("#send"))
            {
                int id = Convert.ToInt32(actionArgument);
                IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
                bool result = conversionProcessingMethods.SendDocument(PageBuilderConversionsInfoProvider.GetPageBuilderConversionsInfo(id));
                if (result)
                {
                    return new Tuple<CustomMessageType, string>(CustomMessageType.Confirmation, "Send Successful");
                }
                else
                {
                    return new Tuple<CustomMessageType, string>(CustomMessageType.Error, "Send Error, see conversion notes for more details");
                }
            }
            else if (actionName.Equals("#save"))
            {
                int id = Convert.ToInt32(actionArgument);

                IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
                bool conversionResults = conversionProcessingMethods.SaveDocument(PageBuilderConversionsInfoProvider.GetPageBuilderConversionsInfo(id));
                if (conversionResults)
                {
                    return new Tuple<CustomMessageType, string>(CustomMessageType.Confirmation, "Save Successful");
                }
                else
                {
                    return new Tuple<CustomMessageType, string>(CustomMessageType.Error, "Save failed, see conversion notes for more details");
                }
            }
            return new Tuple<CustomMessageType, string>(CustomMessageType.Nothing, "");
        }

        public object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {

            if (sourceName.Equals("HasErrors"))
            {
                string notes = ValidationHelper.GetString(parameter, string.Empty);
                if (!string.IsNullOrEmpty(notes) && !notes.Equals("[]"))
                {
                    List<ConversionNote> notesList = JsonConvert.DeserializeObject<List<ConversionNote>>(notes);
                    return notesList.Any(x => x.IsError) ? "Yes" : "No";
                }
                else
                {
                    return "No";
                }
            }
            if (sourceName.Equals("Notes"))
            {
                string notes = ValidationHelper.GetString(parameter, string.Empty);
                if (!notes.Equals("[]"))
                {
                    return $"<span style=\"pointer: cursor; text-decoration: underline; color: blue;\" title=\"{HTMLHelper.EncodeForHtmlAttribute(notes)}\">Hover</span>";
                }
                else
                {
                    return "";
                }
            }
            if (sourceName.Equals("ClassName"))
            {
                var document = GetDocument(ValidationHelper.GetInteger(parameter, 0));
                if (document != null)
                {
                    return document.ClassName;
                }
                else
                {
                    return "[Unknown]";
                }
            }
            if (sourceName.Equals("Template"))
            {
                var document = GetDocument(ValidationHelper.GetInteger(parameter, 0));
                if (document != null)
                {
                    int templateID = document.DocumentPageTemplateID > 0 ? document.DocumentPageTemplateID : document.NodeTemplateID;
                    var checkDoc = document;
                    if (templateID == 0)
                    {
                        while (checkDoc != null && checkDoc.NodeInheritPageTemplate && templateID == 0)
                        {
                            checkDoc = GetDocument(checkDoc.NodeParentID, checkDoc.DocumentCulture);
                            if (checkDoc != null)
                            {
                                templateID = checkDoc.DocumentPageTemplateID > 0 ? checkDoc.DocumentPageTemplateID : checkDoc.NodeTemplateID;
                            }
                        }
                    }
                    var template = GetTemplate(templateID);
                    if (template != null)
                    {
                        return $"{template.DisplayName}";
                    }
                }
                return "[Unknown]";
            }

            return parameter;
        }

        public Tuple<CustomMessageType, string> HeaderActions_ActionPerformed(IEnumerable<int> pageConversionIDs, object sender, string commandName)
        {
            IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
            int itemsSuccess = 0;
            int itemsFailure = 0;

            foreach (var pageBuilderConversion in PageBuilderConversionsInfoProvider.GetPageBuilderConversions().WhereIn(nameof(PageBuilderConversionsInfo.PageBuilderConversionsID), pageConversionIDs.ToArray()).TypedResult)
            {
                try
                {
                    switch (commandName.ToLower())
                    {
                        case "massretry":
                            if (conversionProcessingMethods.ReProcesses(pageBuilderConversion))
                            {
                                itemsSuccess++;
                            }
                            else
                            {
                                itemsFailure++;
                            }
                            break;
                        case "masssend":
                            if (conversionProcessingMethods.SendDocument(pageBuilderConversion))
                            {
                                itemsSuccess++;
                            }
                            else
                            {
                                itemsFailure++;
                            }
                            break;
                        case "masssave":
                            if (conversionProcessingMethods.SaveDocument(pageBuilderConversion))
                            {
                                itemsSuccess++;
                            }
                            else
                            {
                                itemsFailure++;
                            }
                            break;

                    }
                }
                catch (Exception ex)
                {
                    itemsFailure++;
                }
            }
            return new Tuple<CustomMessageType, string>(CustomMessageType.Information, $"{itemsSuccess} Successfully processed, {itemsFailure} Failed");
        }


        private TreeNode GetDocument(int nodeID, string documentCulture)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"nodeid|{nodeID}");

                }
                return DocumentHelper.GetDocuments()
                    .Published(false)
                    .LatestVersion(true)
                    .Culture(documentCulture)
                    .CombineWithAnyCulture()
                    .WhereEquals(nameof(TreeNode.NodeID), nodeID)
                    .TypedResult.FirstOrDefault();
            }, new CacheSettings(30, "GetDocumentByNodeForPageBuilderExtension", nodeID, documentCulture));
        }

        private TreeNode GetDocument(int documentID)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"documentid|{documentID}");

                }
                return DocumentHelper.GetDocuments()
                    .Published(false)
                    .LatestVersion(true)
                    .CombineWithAnyCulture()
                    .WhereEquals(nameof(TreeNode.DocumentID), documentID)
                    .TypedResult.FirstOrDefault();
            }, new CacheSettings(30, "GetDocumentForPageBuilderExtension", documentID));
        }

        private PageTemplateInfo GetTemplate(int templateID)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"cms.pagetemplate|byid|{templateID}");

                }
                return PageTemplateInfoProvider.GetPageTemplateInfo(templateID);
            }, new CacheSettings(30, "GetTemplateForPageBuilderExtension", templateID));
        }
    }
}
