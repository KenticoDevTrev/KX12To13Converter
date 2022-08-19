using CMS.DocumentEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface IRunConversionMethods
    {
        IEnumerable<TreeNode> GetDocumentsByPath(string path);
        TreeNode GetDocumentByPath(string path);
    }
}
