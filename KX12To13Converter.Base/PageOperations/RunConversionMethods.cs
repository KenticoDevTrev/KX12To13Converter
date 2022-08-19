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
        public IEnumerable<TreeNode> GetDocumentsByPath(string path)
        {
            return DocumentHelper.GetDocuments()
                    .Published(false)
                    .LatestVersion(true)
                    .Path(path, PathTypeEnum.Explicit)
                    .TypedResult;
        }

        public TreeNode GetDocumentByPath(string path)
        {
            return DocumentHelper.GetDocuments().Path(path, PathTypeEnum.Single).TypedResult.FirstOrDefault();
        }
    }
}
