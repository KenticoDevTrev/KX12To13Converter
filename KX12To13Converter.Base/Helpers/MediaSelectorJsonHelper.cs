using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace KX12To13Converter
{
    public static class MediaSelectorJsonHelper
    {
        /// <summary>
        /// Takes the given json (Template or Widgets) and replaces relative media urls (or getmedia) of the given property with the proper format for the KX13 MediaSelector.  
        /// 
        /// NOTE: This will change ANY value of the matching given propertyName, please make sure you do not have other widgets/template properties that share the same property name but are not going to be a Media Selector
        /// NOTE: If the Media Guid is not found, it will put an empty array for the media items, and will 
        /// NOTE: Because the Media Selector only selects the FileGuid, and not any url parameters (like dynamic size adjustments), all this data will be lost.
        /// </summary>
        /// <param name="json">The Template or Widget json (From ProcessTemplateWidgetJson event args)</param>
        /// <param name="propertyName">The property name (Case sensitive) that will have node alias paths converted to the PathSelector format: [{"NodeAliasPath": "/The/Path"}]</param>
        /// <param name="valuesNotFound">List of values not found, these were replaced with empty values</param>
        /// <returns>The Json string with transformed property</returns>
        public static string TransformMediaUrlToMediaSelector(string json, string propertyName, ref List<string> valuesNotFound)
        {
            var regex = RegexHelper.GetRegex($@"\""{propertyName}\"":\""([^\""\?\#]*)((\?([^\""]*))|(\#([^\""]*)))\""");
            var matches = regex.Matches(json);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 0)
                    {
                        var filePath = match.Groups[0].Value; // File
                        string afterPathParams = match.Groups.Count >= 2 ? match.Groups[1].Value : "";
                        var splitPath = filePath.ToLower().Split("/".ToCharArray()).ToList();
                        Guid? fileGuid = null;
                        if (splitPath.Contains("getmedia") && splitPath.Count > splitPath.IndexOf("getmedia"))
                        {
                            if(Guid.TryParse(splitPath[splitPath.IndexOf("getmedia") + 1], out var mediaGuid))
                            {
                                fileGuid = mediaGuid;
                            }
                        } else
                        {
                            fileGuid = GetGuidFromPath(filePath);
                        }

                        if (fileGuid.HasValue)
                        {
                            var urlJson = $"[{{\"fileGuid\":\"{fileGuid}\"}}]";
                            json = json.Replace($"\"{propertyName}\":\"{filePath}{afterPathParams}\",", $"\"{propertyName}\":{urlJson},");
                        } else
                        {
                            var urlJson = $"[]";
                            json = json.Replace($"\"{propertyName}\":\"{filePath}{afterPathParams}\",", $"\"{propertyName}\":{urlJson},");
                            valuesNotFound.Add(filePath);
                        }
                    }
                }
            }

            return json;
        }


        public static Guid? GetGuidFromPath(string path)
        {
            var allPaths = GetImagePathGuidsDictionaries();
            string pathKey = "/" + (path.Trim('~').Trim('/')).ToLowerInvariant();
            if(allPaths.Item1.ContainsKey(pathKey))
            {
                return allPaths.Item1[pathKey];
            } else if(allPaths.Item2.ContainsKey(pathKey))
            {
                return allPaths.Item2[pathKey];
            }
            return (Guid?)null;
        }

        private static Tuple<Dictionary<string, Guid>, Dictionary<string, Guid>> GetImagePathGuidsDictionaries()
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[] {
                    $"{MediaFileInfo.OBJECT_TYPE}|all",
                    $"{MediaLibraryInfo.OBJECT_TYPE}|all"
                    });
                }
                var normalPathDictionary = new Dictionary<string, Guid>();
                var encodedPathDictionary = new Dictionary<string, Guid>();
                string query = @"
SELECT 
case when NULLIF(COALESCE(GlobalSKFolder.KeyValue, SiteSKFolder.KeyValue), '') is null 
	then
		'/'+SiteName+'/media' 
	else
		'/'+COALESCE(GlobalSKFolder.KeyValue, SiteSKFolder.KeyValue) +
		case when COALESCE(GlobalSKSiteFolder.KeyValue, SiteSKSiteFolder.KeyValue) <> 'False' then '/'+Sitename else '' end	
	end +
'/'+Media_Library.LibraryFolder+'/'+FilePath as FileFullPath,
FileGUID, FileName+FileExtension as FileName
FROM [Media_File] 
inner join Media_Library on LibraryID = FileLibraryID 
left join CMS_Site S on LibrarySiteID = S.SiteID 
left join CMS_SettingsKey SiteSKFolder on SiteSKFolder.KeyName = 'CMSMediaLibrariesFolder' and SiteSKFolder.SiteID = S.SiteID
left join CMS_SettingsKey GlobalSKFolder on GlobalSKFolder.KeyName = 'CMSMediaLibrariesFolder' and GlobalSKFolder.SiteID is null
left join CMS_SettingsKey SiteSKSiteFolder on SiteSKSiteFolder.KeyName = 'CMSUseMediaLibrariesSiteFolder' and SiteSKSiteFolder.SiteID = S.SiteID
left join CMS_SettingsKey GlobalSKSiteFolder on GlobalSKSiteFolder.KeyName = 'CMSUseMediaLibrariesSiteFolder' and GlobalSKSiteFolder.SiteID is null
";
                 var rows = ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, false).Tables[0].Rows.Cast<DataRow>();
                 foreach(var row in rows)
                    {
                    string normalPath = ValidationHelper.GetString(row["FileFullPath"], "").ToLowerInvariant();
                    // Encoding is...hard.  Spaces to %20, then UrlEncode to handle foreign characters, then undo the encoding on the / and revert %2520 back to %20
                    string encodedPath = HttpUtility.UrlEncode(normalPath.Replace(" ", "%20")).Replace("%2520", "%20").Replace("%2f", "/").ToLower();
                    var mediaGuid = ValidationHelper.GetGuid(row["FileGuid"], Guid.Empty);
                    if (!normalPathDictionary.ContainsKey(normalPath))
                    {
                        normalPathDictionary.Add(normalPath, mediaGuid);
                    }
                    if (!encodedPathDictionary.ContainsKey(encodedPath))
                    {
                        encodedPathDictionary.Add(encodedPath, mediaGuid);
                    }
                }
                return new Tuple<Dictionary<string, Guid>, Dictionary<string, Guid>>(normalPathDictionary, encodedPathDictionary);

            }, new CacheSettings(30, $"GetImagePathGuidsDictionaries"));
        }

    }
}
