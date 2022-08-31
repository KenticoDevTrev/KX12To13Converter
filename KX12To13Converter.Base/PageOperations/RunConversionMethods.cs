using CMS.DocumentEngine;
using KX12To13Converter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.PageOperations
{
    public class RunConversionMethods : IRunConversionMethods
    {
        public IEnumerable<TreeNode> GetDocumentsByPath(string path, int siteID, string culture = null)
        {
            if(siteID > 0)
            {
                var siteQuery = DocumentHelper.GetDocuments()
                    .WhereNull(nameof(TreeNode.NodeLinkedNodeID))
                    .WhereEqualsOrNull(nameof(TreeNode.NodeIsContentOnly), false)
                    .Published(true)
                    .LatestVersion(false)
                    .OnSite(siteID)
                    .Path(path, PathTypeEnum.Explicit);
                if(!string.IsNullOrWhiteSpace(culture))
                {
                    siteQuery.Culture(culture);
                } else
                {
                    siteQuery.AllCultures();
                }
                return siteQuery.TypedResult;
            } else
            {
                var nonSiteQuery = DocumentHelper.GetDocuments()
                    .WhereNull(nameof(TreeNode.NodeLinkedNodeID))
                    .WhereEqualsOrNull(nameof(TreeNode.NodeIsContentOnly), false)
                    .Published(true)
                    .LatestVersion(false)
                    .Path(path, PathTypeEnum.Explicit);
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    nonSiteQuery.Culture(culture);
                }
                else
                {
                    nonSiteQuery.AllCultures();
                }
                return nonSiteQuery.TypedResult;
            }
            
        }

        public TreeNode GetDocumentByPath(string path, int siteID, string culture)
        {
            if(siteID > 0)
            {
                var siteQuery = DocumentHelper.GetDocuments()
                    .OnSite(siteID)
                    .Path(path, PathTypeEnum.Single)
                    .WhereNull(nameof(TreeNode.NodeLinkedNodeID))
                    .WhereEqualsOrNull(nameof(TreeNode.NodeIsContentOnly), false)
                    .Where("DocumentCulture in (select C.CultureCode from CMS_SiteCulture SC inner join CMS_Culture C on SC.CultureID = C.CultureID where SC.SiteID = NodeSiteID)")
                    .Published(true)
                    .LatestVersion(false);
                if (!string.IsNullOrWhiteSpace(culture)) {
                    siteQuery.Culture(culture);
                } else
                {
                    siteQuery.CombineWithDefaultCulture();
                }
                return siteQuery.TypedResult.FirstOrDefault();
            } else {
                var nonSiteQuery = DocumentHelper.GetDocuments()
                    .Path(path, PathTypeEnum.Single)
                    .WhereNull(nameof(TreeNode.NodeLinkedNodeID))
                    .WhereEqualsOrNull(nameof(TreeNode.NodeIsContentOnly), false)
                    .Where("DocumentCulture in (select C.CultureCode from CMS_SiteCulture SC inner join CMS_Culture C on SC.CultureID = C.CultureID where SC.SiteID = NodeSiteID)")
                    .Published(true)
                    .LatestVersion(false);
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    nonSiteQuery.Culture(culture);
                }
                else
                {
                    nonSiteQuery.CombineWithDefaultCulture();
                }
                return nonSiteQuery.TypedResult.FirstOrDefault();
            }
        }

    }
}
