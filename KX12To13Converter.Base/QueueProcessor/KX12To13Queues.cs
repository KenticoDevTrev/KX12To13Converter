using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.PageOperations;
using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace KX12To13Converter.Base.QueueProcessor
{
    public class KX12To13Queues
    {
        /// <summary>
        /// Generates a PageBuilderConversionsINfo object
        /// </summary>
        /// <param name="document"></param>
        /// <param name="results"></param>
        /// <param name="markForSend"></param>
        /// <returns></returns>
        public static PageBuilderConversionsInfo GenerateConversionInfo(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results, bool markForSend)
        {
            PageBuilderConversionsInfo pageBuilderConversionsInfo = new PageBuilderConversionsInfo()
            {
                PageBuilderConversionDocumentID = document.DocumentID,
                PageBuilderConversionDateProcessed = DateTime.Now,
                PageBuilderConversionMarkedForSend = markForSend,
                PageBuilderConversionMarkForConversion = false,
                PageBuilderConversionPageBuilderJSON = JsonConvert.SerializeObject(results.PageBuilderData.ZoneConfiguration, Formatting.None),
                PageBuilderConversionTemplateJSON = JsonConvert.SerializeObject(results.PageBuilderData.TemplateConfiguration, Formatting.None),
                PageBuilderConversionSuccessful = !results.CancelOperation,
                PageBuilderConversionNotes = JsonConvert.SerializeObject(results.ConversionNotes, Formatting.Indented)
            };

            // Delete any existing matches
            ConnectionHelper.ExecuteNonQuery($"delete from KX12To13Converter_PageBuilderConversions where PageBuilderConversionDocumentID = {document.DocumentID}", null, QueryTypeEnum.SQLQuery);

            // Insert
            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);
            return pageBuilderConversionsInfo;
        }

        public static void AddConversionToQueue(int documentID, bool markForSend)
        {
            if (SettingsKeyInfoProvider.GetBoolValue("AutomaticallyConvertAndSetSendQueue", SiteContext.CurrentSiteID))
            {
                PageBuilderConversionsInfo pageBuilderConversionsInfo = new PageBuilderConversionsInfo()
                {
                    PageBuilderConversionDocumentID = documentID,
                    PageBuilderConversionMarkedForSend = markForSend,
                    PageBuilderConversionMarkForConversion = false,
                };

                // Delete any existing matches
                ConnectionHelper.ExecuteNonQuery($"delete from KX12To13Converter_PageBuilderConversions where PageBuilderConversionDocumentID = {documentID}", null, QueryTypeEnum.SQLQuery);

                // Insert
                PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(pageBuilderConversionsInfo);

                RunQueueForConversion();
            }
        }

        public static void RunQueueForSending()
        {
            // Run the task on a thread, unless already running
            // Queue up task asyncly
            if (SettingsKeyInfoProvider.GetBoolValue("ConversionSendQueueEnabled", SiteContext.CurrentSiteID))
            {
                try
                {
                    if (!ThreadDebug.LiveThreadItems.Any(x => x.MethodName == "SendQueue" && x.ThreadFinished == DateTimeHelper.ZERO_TIME))
                    {
                        var newThread = new CMSThread(new ThreadStart(SendQueue), new ThreadSettings()
                        {
                            Mode = ThreadModeEnum.Async,
                            IsBackground = true,
                            Priority = ThreadPriority.AboveNormal,
                            UseEmptyContext = false,
                            CreateLog = true,
                        });
                        newThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("KX12To13ConversionQueue", "ErrorStartingSendingQueue", ex);
                }
            }
        }

        private static void SendQueue()
        {
            IConversionProcessingMethods ConversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
            // Loop
            while (SettingsKeyInfoProvider.GetBoolValue("ConversionSendQueueEnabled", SiteContext.CurrentSiteID))
            {
                var nextConversions = PageBuilderConversionsInfoProvider.GetPageBuilderConversions()
                    .TopN(10)
                    .WhereEquals(nameof(PageBuilderConversionsInfo.PageBuilderConversionMarkedForSend), true)
                    .TypedResult;

                // If nothing, wait a second and try again
                if (!nextConversions.Any())
                {
                    Thread.Sleep(10000);
                    continue;
                }

                // Convert each document then update the conversion item
                foreach (var nextConversion in nextConversions)
                {
                    ConversionProcessingMethods.SendDocument(nextConversion);
                }
            }
        }

        public static void RunQueueForConversion()
        {
            // Run the task on a thread, unless already running
            // Queue up task asyncly
            if (SettingsKeyInfoProvider.GetBoolValue("AutomaticallyConvertAndSetSendQueue", SiteContext.CurrentSiteID))
            {
                try
                {
                    if (!ThreadDebug.LiveThreadItems.Any(x => x.MethodName == "ConversionQueue" && x.ThreadFinished == DateTimeHelper.ZERO_TIME))
                    {
                        var newThread = new CMSThread(new ThreadStart(ConversionQueue), new ThreadSettings()
                        {
                            Mode = ThreadModeEnum.Async,
                            IsBackground = true,
                            Priority = ThreadPriority.AboveNormal,
                            UseEmptyContext = false,
                            CreateLog = true,
                        });
                        newThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("KX12To13ConversionQueue", "ErrorStartingConversionQueue", ex);
                }
            }

        }

        private static void ConversionQueue()
        {
            IPortalEngineToMVCConverterRetriever portalEngineToMVCConverterRetriever = Service.Resolve<IPortalEngineToMVCConverterRetriever>();
            // Loop
            while(SettingsKeyInfoProvider.GetBoolValue("AutomaticallyConvertAndSetSendQueue", SiteContext.CurrentSiteID))
            {
                var nextConversions =
                    ConnectionHelper.ExecuteQuery($"select top 10 * from KX12To13Converter_PageBuilderConversions inner join View_CMS_Tree_Joined on DocumentID = PageBuilderConversionDocumentID with (nolock) where {nameof(PageBuilderConversionsInfo.PageBuilderConversionMarkForConversion)} = 1", null, QueryTypeEnum.SQLQuery)
                    .Tables[0].Rows.Cast<DataRow>()
                    .Select(x => new Tuple<TreeNode, PageBuilderConversionsInfo>(TreeNode.New(x), new PageBuilderConversionsInfo(x))).ToList();

                // If nothing, wait a second and try again
                if (!nextConversions.Any())
                {
                    Thread.Sleep(10000);
                    continue;
                }

                // Convert each document then update the conversion item
                foreach (var nextConversion in nextConversions)
                {
                    int siteID = nextConversion.Item1.NodeSiteID;

                    var converter = CacheHelper.Cache(cs =>
                    {
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


                    var results = converter.ProcessDocument(nextConversion.Item1);
                    var conversion = nextConversion.Item2;
                    conversion.PageBuilderConversionDateProcessed = DateTime.Now;
                    conversion.PageBuilderConversionSuccessful = !results.CancelOperation;
                    conversion.PageBuilderConversionMarkForConversion = false;
                    conversion.PageBuilderConversionMarkedForSend = true;
                    conversion.PageBuilderConversionTemplateJSON = JsonConvert.SerializeObject(results.PageBuilderData.TemplateConfiguration, Formatting.None);
                    conversion.PageBuilderConversionPageBuilderJSON = JsonConvert.SerializeObject(results.PageBuilderData.ZoneConfiguration, Formatting.None);
                    conversion.PageBuilderConversionNotes = JsonConvert.SerializeObject(results.ConversionNotes, Formatting.Indented);
                    PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(conversion);
                }
            }
        }
    }
}
