using CMS.DocumentEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface IRunConversionMethods
    {
        IEnumerable<TreeNode> GetDocumentsByPath(string path, int siteID, string culture = null);
        TreeNode GetDocumentByPath(string path, int siteID, string culture);
    }
}
