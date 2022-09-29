using CMS;
using CMS.DataEngine;
using CMS.SiteProvider;
using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.WebHost;
using CMS.Core;
using CMS.Helpers;
using CMS.DocumentEngine;
using System.Linq;
using CMS.Membership;
using System.Net;

namespace KX12To13Converter.KX13Receiver
{

    public class KX13ConverterReceiverController : ApiController
    {

        [HttpPost]
        [ActionName("updatedoc")]
        public HttpResponseMessage UpdateDoc(KX12To13ConversionRequest request)
        {
            try
            {
                var enabled = SettingsKeyInfoProvider.GetBoolValue("KX12To13ConverterReceiverEnabled", SiteContext.CurrentSiteID);
                if (!enabled)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    response.Content = new StringContent("Converter Receiver Enabled setting is false on the KX13 site.");
                    return response;
                }

                string hashValue = SettingsKeyInfoProvider.GetValue("ConverterHashCode", SiteContext.CurrentSiteID);
                string noonce = GetNoonce(request, hashValue);
                if (!noonce.Equals(request.noonce))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    response.Content = new StringContent("Invalid Noonce, ensure Converter Security Hash Value match between KX12 and KX13 settings");
                    return response;
                }

                DateTime upgradeDate = ValidationHelper.GetDateTime(SettingsKeyInfoProvider.GetValue("KX12To13ConverterReceiverUpgradedDate", SiteContext.CurrentSiteID), DateTime.Now).AddDays(1).Date;

                // Get the matching document
                TreeNode doc;
                doc = DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.DocumentGUID), request.documentGuid)
                    .Culture(request.documentCulture)
                    .Published(false)
                    .LatestVersion(true)
                    .TypedResult
                    .FirstOrDefault();
                if (doc == null)
                {
                    doc = DocumentHelper.GetDocuments()
                        .WhereEquals(nameof(TreeNode.NodeGUID), request.nodeGuid)
                        .Culture(request.documentCulture)
                        .Published(false)
                        .LatestVersion(true)
                        .TypedResult
                        .FirstOrDefault();
                }
                if (doc == null)
                {
                    doc = DocumentHelper.GetDocuments()
                        .WhereEquals(nameof(TreeNode.NodeAliasPath), request.nodeAliasPath)
                        .OnSite(request.siteCodeName)
                        .Culture(request.documentCulture)
                        .Published(false)
                        .LatestVersion(true)
                        .TypedResult
                        .FirstOrDefault();
                }

                if (doc == null)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Gone);
                    response.Content = new StringContent($"Could not find a document with DocumentGuid {request.documentGuid} or  NodeGuid {request.nodeGuid} / NodeAliasPath {request.nodeAliasPath} with culture {request.documentCulture} on site {request.siteCodeName}.");
                    return response;
                }


                var overwriteEvenIfNewer = SettingsKeyInfoProvider.GetBoolValue("KX12To13ConverterReceiverOverwriteEvenIfNewer", SiteContext.CurrentSiteID);

                if (!overwriteEvenIfNewer && (doc.DocumentModifiedWhen > upgradeDate && doc.DocumentModifiedWhen > request.lastModified))
                {
                    // Check if was updated from conversion
                    bool lastUpdateWasFromKXConverter = false;
                    if (doc.DocumentCustomData.TryGetValue("LastKX12To13ConversionDate", out object value))
                    {
                        var updatedByConverter = ValidationHelper.GetDateTime(value, upgradeDate);
                        // The last modified wasn't the update converter date
                        if ((doc.DocumentModifiedWhen - updatedByConverter).Minutes > 5)
                        {
                            lastUpdateWasFromKXConverter = true;
                        }
                    }
                    if (!lastUpdateWasFromKXConverter)
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.Conflict);
                        response.Content = new StringContent($"Document found but was updated on KX13 prior to the provided document and 'Overwrite even if newer on KX13' is set to false.");
                        return response;
                    }
                }

                // reget document typed
                doc = DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.DocumentID), doc.DocumentID)
                    .WithCoupledColumns()
                    .Published(false)
                    .LatestVersion(true)
                    .TypedResult
                    .First();

                bool wasPublished = doc.IsPublished;
                bool wasCheckedOut = doc.IsCheckedOut;
                var wi = doc.GetWorkflow();
                var useCheckInCheckOut = wi != null && wi.UseCheckInCheckOut(SiteContext.CurrentSiteName);
                var useWorkflow = wi != null;

                if (useCheckInCheckOut && !wasCheckedOut)
                {
                    doc.CheckOut();
                }

                // Set values
                doc.SetValue("DocumentPageBuilderWidgets", !string.IsNullOrWhiteSpace(request.documentPageBuilderWidgets) ? request.documentPageBuilderWidgets : null);
                doc.SetValue("DocumentPageTemplateConfiguration", !string.IsNullOrWhiteSpace(request.documentPageTemplateConfiguration) ? request.documentPageTemplateConfiguration : null);
                doc.DocumentCustomData.SetValue("LastKX12To13ConversionDate", DateTime.Now);

                DocumentHelper.UpdateDocument(doc, new TreeProvider());
                // Get workflow info

                if (useCheckInCheckOut && !wasCheckedOut)
                {
                    doc.CheckIn();
                }
                if (useWorkflow && (wasPublished || wi.WorkflowAutoPublishChanges))
                {
                    doc.MoveToPublishedStep("Updated through KX12 to 13 Converter");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            } catch(Exception ex)
            {
                var log = Service.Resolve<IEventLogService>();
                log.LogException("KX12To13Converter", "UpdateDoc", ex, SiteContext.CurrentSiteID, additionalMessage: $"For DocumentGuid {request.documentGuid} or  NodeGuid {request.nodeGuid} / NodeAliasPath {request.nodeAliasPath} with culture {request.documentCulture} on site {request.siteCodeName}.");
            }
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

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

    /// <summary>
    /// Request sent from KX12 to update a document on KX13
    /// </summary>
    public class KX12To13ConversionRequest
    {
        public DateTime lastModified { get; set; }
        public Guid nodeGuid { get; set; }
        public string nodeAliasPath { get; set; }
        public string siteCodeName { get; set; }
        public string documentCulture { get; set; }
        public Guid documentGuid { get; set; }
        public string documentPageTemplateConfiguration { get; set; }
        public string documentPageBuilderWidgets { get; set; }
        public string noonce { get; set; }
    }
}
