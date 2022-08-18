using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace KX12To13Converter.Base.PageOperations
{
    public class WidgetConverterMethods
    {
        public static IEnumerable<TreeNode> GetDocumentsByPath(string path)
        {
            return DocumentHelper.GetDocuments()
                    .Published(false)
                    .LatestVersion(true)
                    .Path(path, PathTypeEnum.Explicit)
                    .TypedResult;
        }

        public static TreeNode GetDocumentByPath(string path)
        {
            return DocumentHelper.GetDocuments().Path(path, PathTypeEnum.Single).TypedResult.FirstOrDefault();
        }

        public static bool HandleProcessAndSaveDocument(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results)
        {
            // Creates an instance of the Tree provider
            try
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                WorkflowManager workflowManager = WorkflowManager.GetInstance(tree);
                WorkflowInfo workflow = workflowManager.GetNodeWorkflow(document);

                bool wasCheckedOut = document.IsCheckedOut;
                bool isPublished = document.IsPublished;
                if (!document.DocumentCheckedOutAutomatically && !wasCheckedOut)
                {
                    document.CheckOut();
                }
                document.SetValue("DocumentPageTemplateConfiguration", JsonConvert.SerializeObject(results.PageBuilderData.TemplateConfiguration, Formatting.None));
                document.SetValue("DocumentPageBuilderWidgets", JsonConvert.SerializeObject(results.PageBuilderData.ZoneConfiguration, Formatting.None));

                document.Update();

                if (!document.DocumentCheckedOutAutomatically && !wasCheckedOut)
                {
                    document.CheckIn();
                }
                if (workflow != null && isPublished)
                {
                    document.Publish("Published with Converted Page Builder values");
                }

                results.ConversionNotes.Add(new ConversionNote()
                {
                    IsError = false,
                    Source = "HandleProcessAndSaveDocument",
                    Type = "SaveDocumentSuccess"
                });
            } catch(Exception ex)
            {
                results.ConversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "HandleProcessAndSaveDocument",
                    Type = "SaveDocumentError",
                    Description = $"Error updating document: {ex.Message}."
                });
            }
            return true;
        }

        public static bool HandleProcessAndSendDocument(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results)
        {
            // TODO: Add data to custom module class for tracking
            string url = SettingsKeyInfoProvider.GetValue("ConverterReceiverBaseUrl", SiteContext.CurrentSiteID) + "/kx12to13api/updatedoc";
            string hashValue = SettingsKeyInfoProvider.GetValue("ConverterHashCode", SiteContext.CurrentSiteID);

            var request = new KX12To13ConversionRequest()
            {
                lastModified = document.DocumentModifiedWhen,
                nodeGuid = document.NodeGUID,
                nodeAliasPath = document.NodeAliasPath,
                siteCodeName = document.NodeSiteName,
                documentCulture = document.DocumentCulture,
                documentGuid = document.DocumentGUID,
                documentPageTemplateConfiguration = JsonConvert.SerializeObject(results.PageBuilderData.TemplateConfiguration, Formatting.None),
                documentPageBuilderWidgets = JsonConvert.SerializeObject(results.PageBuilderData.ZoneConfiguration, Formatting.None)
            };
            request.noonce = KX12To13ConversionRequest.GetNoonce(request, hashValue);

            string result = string.Empty;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(request);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                using (var response = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    if (httpWebRequest.HaveResponse && response != null)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
                results.ConversionNotes.Add(new ConversionNote()
                {
                    IsError = false,
                    Source = "HandleProcessAndSendDocument",
                    Type = "SendDocumentResult",
                    Description = $"Document Sent Message {result}."
                });
                return true;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)e.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                           
                        }
                    }

                }
                results.ConversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "HandleProcessAndSendDocument",
                    Type = "SendDocumentError",
                    Description = $"Exception: {e.Message} - {result}"
                });
                return false;
            }
        }
    }
}
