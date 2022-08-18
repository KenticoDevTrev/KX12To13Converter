using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Base.PageOperations
{
    public class PreUpgrade2RemoveClasses
    {

        public static IEnumerable<DataClassInfo> GetPageTypes()
        {
            return DataClassInfoProvider.GetClasses()
                .WhereTrue("ClassIsDocumentType")
                .Where("ClassID not in (Select distinct NodeClassID from View_CMS_Tree_Joined) and ClassID not in (select Sub.ClassInheritsFromClassID from CMS_Class Sub where Sub.ClassInheritsFromClassID is not null and Sub.ClassInheritsFromClassID <> 0)")
                .TypedResult;
        }

        public static IEnumerable<DataClassInfo> GetUsedPageTypes()
        {
            return DataClassInfoProvider.GetClasses()
                .WhereTrue("ClassIsDocumentType")
                .Where("ClassID in (Select distinct NodeClassID from View_CMS_Tree_Joined) and ClassID not in (select Sub.ClassInheritsFromClassID from CMS_Class Sub where Sub.ClassInheritsFromClassID is not null and Sub.ClassInheritsFromClassID <> 0)")
                .OrderBy($"(select count(*) from View_CMS_Tree_Joined subQuery where subQuery.NodeCLassID = ClassID)")
                .TypedResult;
        }

        public static string GetEntryValue(DataClassInfo dataClassInfo)
        {
            var tableFieldOnlyQueryLookups = new List<Tuple<string, string, bool>>
        {
            new Tuple<string, string, bool>("CMS_Class", "ClassFormDefinition", false),
            new Tuple<string, string, bool>("CMS_AlternativeForm", "FormDefinition", false),
            new Tuple<string, string, bool>("CMS_Document", "DocumentContent", true),
            new Tuple<string, string, bool>("CMS_Document", "DocumentWebParts", true),
            new Tuple<string, string, bool>("CMS_EmailTemplate", "EmailTemplateText", false),
            new Tuple<string, string, bool>("CMS_EmailTemplate", "EmailTemplatePlainText", false),
            new Tuple<string, string, bool>("CMS_SearchIndex", "IndexQueryKey", false),
            new Tuple<string, string, bool>("CMS_SearchIndex", "IndexCustomAnalyzerClassName", false),
            new Tuple<string, string, bool>("CMS_SettingsKey", "KeyFormControlSettings", false),
            new Tuple<string, string, bool>("CMS_SettingsKey", "KeyValue", false),
            new Tuple<string, string, bool>("CMS_UIElement", "ElementProperties", false),
            new Tuple<string, string, bool>("CMS_UIElement", "ElementAccessCondition", false),
            new Tuple<string, string, bool>("CMS_UIElement", "ElementVisibilityCondition", false),
            new Tuple<string, string, bool>("CMS_WebPart", "WebPartProperties", true),
            new Tuple<string, string, bool>("CMS_Widget", "WidgetProperties", true),
            new Tuple<string, string, bool>("Newsletter_EmailTemplate", "TemplateCode", false),
            new Tuple<string, string, bool>("Newsletter_EmailWidget", "EmailWidgetProperties", false)
        };

            List<string> referencesFound = new List<string>();
            // Loop through queries and look for references
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                    $"CMS.Class|all",
                    $"CMS.AlternativeForm|all",
                    $"CMS.Document|all",
                    $"CMS.EmailTemplate|all",
                    $"CMS.SearchIndex|all",
                    $"CMS.SettingsKey|all",
                    $"CMS.UIElement|all",
                    $"CMS.WebPart|all",
                    $"CMS.Widget|all",
                    $"Newsletter.EmailTemplate|all",
                    $"Newsletter.EmailWidget|all"
                    });
                }
                var entry = $"[{dataClassInfo.ClassName}] {dataClassInfo.ClassDisplayName} [";

                foreach (var lookup in tableFieldOnlyQueryLookups.Where(x => !x.Item3))
                {
                    int refs = References(dataClassInfo.ClassName, lookup.Item1, lookup.Item2);
                    if (refs > 0)
                    {
                        entry += $"{lookup.Item1}.{lookup.Item2}x{refs}, ";
                    }
                }
                foreach (var lookup in tableFieldOnlyQueryLookups.Where(x => x.Item3))
                {
                    bool itemsFound = false;
                    foreach (var queryName in QueryInfoProvider.GetQueries().WhereEquals("ClassID", dataClassInfo.ClassID).TypedResult)
                    {
                        var lookupItem = $"{dataClassInfo.ClassName}.{queryName}";
                        int refs = References(lookupItem, lookup.Item1, lookup.Item2);

                        if (refs > 0 && itemsFound)
                        {
                            itemsFound = true;
                            entry += $"{lookup.Item1}.{lookup.Item2}-{lookupItem}x{refs}";
                        }
                        else if (refs > 0)
                        {
                            entry += $"|{lookupItem}x{refs}";
                        }
                    }
                    if (itemsFound)
                    {
                        entry += ", ";
                    }
                }
                if (entry[entry.Length - 1] == '[')
                {
                    entry = entry.Substring(0, entry.Length - 1);
                }
                else
                {
                    entry = entry.Substring(0, entry.Length - 2) + "]";
                }
                return entry;
            }, new CacheSettings(60, "refLookupForClasses", dataClassInfo.ClassName));
        }

        public static string GetEntryValueForUsed(DataClassInfo classObj)
        {
            int totalDocs = Convert.ToInt32(ConnectionHelper.ExecuteQuery($"select count(*) as totalCount from VIew_CMS_Tree_Joined where NodeClassID = {classObj.ClassID}", null, QueryTypeEnum.SQLQuery).Tables[0].Rows[0]["totalCount"]);
            return $"[{classObj.ClassName}] {classObj.ClassDisplayName} [{totalDocs} Documents]";

        }

        public static int References(string lookup, string table, string column)
        {
            return ValidationHelper.GetInteger(ConnectionHelper.ExecuteQuery($"Select Count(*) from {table} where {column} like '%{SqlHelper.EscapeQuotes(lookup)}%'", null, QueryTypeEnum.SQLQuery).Tables[0].Rows[0][0], 0);
        }

        public static void DeleteClasses(IEnumerable<int> classIds)
        {
            foreach (var classObj in DataClassInfoProvider.GetClasses().WhereIn("ClassID", classIds.ToArray()).TypedResult)
            {
                classObj.Delete();
            }
        }

        public static void DeletePagesClasses(IEnumerable<int> classIds)
        {
            foreach (var classObj in DataClassInfoProvider.GetClasses().WhereIn("ClassID", classIds.ToArray()).TypedResult)
            {
                // Delete pages first
                foreach (var page in DocumentHelper.GetDocuments().WhereEquals("NodeClassID", classObj.ClassID).Published(false).LatestVersion().AllCultures().TypedResult)
                {
                    try
                    {
                        page.Destroy();
                    }
                    catch { }
                }
                classObj.Delete();
            }
        }
    }
}
