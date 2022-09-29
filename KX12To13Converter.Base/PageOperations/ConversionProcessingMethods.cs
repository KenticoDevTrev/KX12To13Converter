using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.QueueProcessor;
using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KX12To13Converter.Base.PageOperations
{
    public class ConversionProcessingMethods : IConversionProcessingMethods
    {
        #region "Handling"

        public bool HandleProcessOnly(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results)
        {
            KX12To13Queues.GenerateConversionInfo(document, results, false);
            return true;
        }

        /// <summary>
        /// Saves the conversion results to a PageBuilderConversionsInfo object and attempts to push right away
        /// </summary>
        /// <param name="document"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public bool HandleProcessAndSendDocument(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results)
        {
            var pageBuilderConversionsInfo = KX12To13Queues.GenerateConversionInfo(document, results, true);
            return SendDocument(document, pageBuilderConversionsInfo);
        }

        /// <summary>
        /// Takes the results and saves it to the DocumentPageTemplateConversion and DocumentPageBuilderWidgets
        /// </summary>
        /// <param name="document"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public bool HandleProcessAndSaveDocument(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results)
        {
            // Generate it
            var conversionItem = KX12To13Queues.GenerateConversionInfo(document, results, false);

            SaveDocument(document, conversionItem);

            return true;
        }


        #endregion

        #region "Processing"

        public bool ReProcesses(PageBuilderConversionsInfo pageBuilderConversionsInfo)
        {
            var document = DocumentHelper.GetDocuments().WhereEquals(nameof(TreeNode.DocumentID), pageBuilderConversionsInfo.PageBuilderConversionDocumentID)
                .Published(true)
                .LatestVersion(false)
                .CombineWithAnyCulture(true)
                .FirstOrDefault();
            if (document == null)
            {
                return false;
            }
            int siteID = document.NodeSiteID;
            IPortalEngineToMVCConverter converter = CacheHelper.Cache(cs =>
            {
                IPortalEngineToMVCConverterRetriever portalEngineToMVCConverterRetriever = Service.Resolve<IPortalEngineToMVCConverterRetriever>();
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                                $"{SettingsKeyInfo.OBJECT_TYPE}|all"
                    });
                }
                var templateConfiguration = JsonConvert.DeserializeObject<List<TemplateConfiguration>>(SettingsKeyInfoProvider.GetValue("ConverterTemplateConfigJson", siteID));
                var sectionConfiguration = JsonConvert.DeserializeObject<List<ConverterSectionConfiguration>>(SettingsKeyInfoProvider.GetValue("ConverterSectionConfigJson", siteID));
                var defaultSectionConfiguration = JsonConvert.DeserializeObject<PageBuilderSection>(SettingsKeyInfoProvider.GetValue("ConverterDefaultSectionConfigJson", siteID));
                var widgetConfiguration = JsonConvert.DeserializeObject<List<ConverterWidgetConfiguration>>(SettingsKeyInfoProvider.GetValue("ConverterWidgetConfigJson", siteID));
                return portalEngineToMVCConverterRetriever.GetConverter(templateConfiguration, sectionConfiguration, defaultSectionConfiguration, widgetConfiguration);
            }, new CacheSettings(30, "GetConverter", siteID));
            var results = converter.ProcessDocument(document);

            // Update results
            pageBuilderConversionsInfo.PageBuilderConversionDateProcessed = DateTime.Now;
            pageBuilderConversionsInfo.PageBuilderConversionMarkForConversion = false;
            pageBuilderConversionsInfo.PageBuilderConversionPageBuilderJSON = JsonConvert.SerializeObject(results.PageBuilderData.ZoneConfiguration, Formatting.None);
            pageBuilderConversionsInfo.PageBuilderConversionTemplateJSON = JsonConvert.SerializeObject(results.PageBuilderData.TemplateConfiguration, Formatting.None);
            pageBuilderConversionsInfo.PageBuilderConversionSuccessful = !results.CancelOperation;
            pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(results.ConversionNotes, Formatting.Indented);

            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);

            return !results.CancelOperation;
        }


        #endregion

        #region "Send"
        public bool SendDocument(PageBuilderConversionsInfo pageBuilderConversionsInfo)
        {
            var document = DocumentHelper.GetDocuments().WhereEquals(nameof(TreeNode.DocumentID), pageBuilderConversionsInfo.PageBuilderConversionDocumentID).Published(true).LatestVersion(false).CombineWithAnyCulture(true).FirstOrDefault();
            if (document == null)
            {
                return false;
            }
            return SendDocument(document, pageBuilderConversionsInfo);
        }

        /// <summary>
        /// Sends the document to the KX13 receiving API
        /// </summary>
        /// <param name="document"></param>
        /// <param name="pageBuilderConversionsInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool SendDocument(TreeNode document, PageBuilderConversionsInfo pageBuilderConversionsInfo)
        {

            string url = SettingsKeyInfoProvider.GetValue("ConverterReceiverBaseUrl", SiteContext.CurrentSiteID) + "/kx12to13api/updatedoc";
            string hashValue = SettingsKeyInfoProvider.GetValue("ConverterHashCode", SiteContext.CurrentSiteID);
            List<ConversionNote> conversionNotes = !string.IsNullOrWhiteSpace(pageBuilderConversionsInfo.PageBuilderConversionNotes) ? JsonConvert.DeserializeObject<List<ConversionNote>>(pageBuilderConversionsInfo.PageBuilderConversionNotes) : new List<ConversionNote>();
            var request = new KX12To13ConversionRequest()
            {
                lastModified = document.DocumentModifiedWhen,
                nodeGuid = document.NodeGUID,
                nodeAliasPath = document.NodeAliasPath,
                siteCodeName = document.NodeSiteName,
                documentCulture = document.DocumentCulture,
                documentGuid = document.DocumentGUID,
                documentPageTemplateConfiguration = pageBuilderConversionsInfo.PageBuilderConversionTemplateJSON,
                documentPageBuilderWidgets = pageBuilderConversionsInfo.PageBuilderConversionPageBuilderJSON
            };
            request.noonce = GetNoonce(request, hashValue);
            string result = string.Empty;


            try
            {
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

                using (var response = httpWebRequest.GetResponse() as HttpWebResponse)
                {

                    if (httpWebRequest.HaveResponse && response != null)
                    {
                        string responseText = string.Empty;
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            conversionNotes.Add(new ConversionNote()
                            {
                                IsError = false,
                                Source = "HandleProcessAndSendDocument",
                                Type = "SendDocumentResult",
                                Description = $"Document Sent Message {result}."
                            });

                            pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                            pageBuilderConversionsInfo.PageBuilderConversionLastSendDate = DateTime.Now;
                            pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                            pageBuilderConversionsInfo.PageBuilderConversionNoMatchFound = false;
                            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // No Match
                            conversionNotes.Add(new ConversionNote()
                            {
                                IsError = false,
                                Source = "HandleProcessAndSendDocument",
                                Type = "ReceiverDisabledOrHashMismatch",
                                Description = result
                            });

                            pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                            pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                            pageBuilderConversionsInfo.PageBuilderConversionNoMatchFound = false;
                            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                        }
                        else if (response.StatusCode == HttpStatusCode.Gone)
                        {
                            // No Match
                            conversionNotes.Add(new ConversionNote()
                            {
                                IsError = false,
                                Source = "HandleProcessAndSendDocument",
                                Type = "NoMatchFound",
                                Description = result
                            });

                            pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                            pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                            pageBuilderConversionsInfo.PageBuilderConversionNoMatchFound = true;
                            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                        }
                        else if (response.StatusCode == HttpStatusCode.Conflict)
                        {
                            // No Match
                            conversionNotes.Add(new ConversionNote()
                            {
                                IsError = false,
                                Source = "HandleProcessAndSendDocument",
                                Type = "DocFoundButCantUpdate",
                                Description = result
                            });

                            pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                            pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                            pageBuilderConversionsInfo.PageBuilderConversionNoMatchFound = false;
                            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                        }
                        else
                        {
                            throw new Exception($"Unknown Response code: {response.StatusCode}: {result}");
                        }
                    }
                }
                return true;
            }
            catch (UriFormatException e)
            {
                // Log error
                conversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "HandleProcessAndSendDocument",
                    Type = "SendDocumentError",
                    Description = $"Invalid Uri: {url}, update in Settings to point to your KX13 instance."
                });
                pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                return false;
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
                // Log error
                conversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "HandleProcessAndSendDocument",
                    Type = "SendDocumentError",
                    Description = $"Exception: {e.Message} - {result}"
                });
                pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                return false;
            }
            catch (Exception ex)
            {
                // Log error
                conversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "HandleProcessAndSendDocument",
                    Type = "SendDocumentError",
                    Description = $"Exception: {ex.Message}"
                });
                pageBuilderConversionsInfo.PageBuilderConversionMarkedForSend = false;
                pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                return false;
            }
        }

        #endregion

        #region "Save"

        public bool SaveDocument(PageBuilderConversionsInfo pageBuilderConversionsInfo)
        {
            var document = DocumentHelper.GetDocuments().WhereEquals(nameof(TreeNode.DocumentID), pageBuilderConversionsInfo.PageBuilderConversionDocumentID).Published(true).LatestVersion(false).CombineWithAnyCulture(true).FirstOrDefault();
            if (document == null)
            {
                return false;
            }
            return SaveDocument(document, pageBuilderConversionsInfo);
        }

        private bool SaveDocument(TreeNode document, PageBuilderConversionsInfo pageBuilderConversionsInfo)
        {
            List<ConversionNote> conversionNotes = !string.IsNullOrWhiteSpace(pageBuilderConversionsInfo.PageBuilderConversionNotes) ? JsonConvert.DeserializeObject<List<ConversionNote>>(pageBuilderConversionsInfo.PageBuilderConversionNotes) : new List<ConversionNote>();
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
                document.SetValue("DocumentPageTemplateConfiguration", pageBuilderConversionsInfo.PageBuilderConversionTemplateJSON);
                document.SetValue("DocumentPageBuilderWidgets", pageBuilderConversionsInfo.PageBuilderConversionPageBuilderJSON);

                document.Update();

                if (!document.DocumentCheckedOutAutomatically && !wasCheckedOut)
                {
                    document.CheckIn();
                }
                if (workflow != null && isPublished)
                {
                    document.Publish("Published with Converted Page Builder values");
                }
            }
            catch (Exception ex)
            {
                conversionNotes.Add(new ConversionNote()
                {
                    IsError = true,
                    Source = "HandleProcessAndSaveDocument",
                    Type = "SaveDocumentError",
                    Description = $"Error updating document: {ex.Message}."
                });
                pageBuilderConversionsInfo.PageBuilderConversionNotes = JsonConvert.SerializeObject(conversionNotes, Formatting.Indented);
                PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
                return false;
            }
            return true;
        }

        #endregion

        public static string GetNoonce(KX12To13ConversionRequest request, string privateToken)
        {
            string input = $"{request.lastModified}{request.nodeGuid}{request.nodeAliasPath}{request.siteCodeName}{request.documentCulture}{request.documentGuid}{request.documentPageTemplateConfiguration}{request.documentPageBuilderWidgets}{privateToken}";
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string prior to .NET 5
                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        


    }
}
