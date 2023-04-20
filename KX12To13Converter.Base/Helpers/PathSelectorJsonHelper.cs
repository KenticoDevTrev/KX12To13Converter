using CMS.DocumentEngine;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KX12To13Converter
{
    public static class PathSelectorJsonHelper
    {
        /// <summary>
        /// Takes the given json (Template or Widgets) and replaces NodeAliasPath of the given property with the proper format for the KX13 PathSelector.  
        /// 
        /// NOTE: This will change ANY value of the matching given propertyName, please make sure you do not have other widgets/template properties that share the same property name but are not going to be a Path Selector
        /// NOTE: Child paths are not supported in the KX13 Path Selector, so these are trimmed.  You should account for sub pages in your actual code.
        /// </summary>
        /// <param name="json">The Template or Widget json (From ProcessTemplateWidgetJson event args)</param>
        /// <param name="propertyName">The property name (Case sensitive) that will have node alias paths converted to the PathSelector format: [{"NodeAliasPath": "/The/Path"}]</param>
        /// <returns>The Json string with transformed property</returns>
        public static string TransformNodeAliasPathToPathSelector(string json, string propertyName)
        {
            var regex = RegexHelper.GetRegex($"\"{propertyName}\":\"([~\\/_a-zA-Z0-9-.\\%]*)\",");
            var matches = regex.Matches(json);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        var group = match.Groups[1];
                        var urlJson = $"[{{\"nodeAliasPath\":\"{group.Value.Trim('%').TrimEnd('/')}\"}}]";
                        json = json.Replace($"\"{propertyName}\":\"{group.Value}\",", $"\"{propertyName}\":{urlJson},");
                    }
                }
            }
            return json;
        }
    }
}
