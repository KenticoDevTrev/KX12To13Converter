using CMS.DocumentEngine;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Interfaces
{
    public interface IPortalEngineToMVCConverter
    {
        void ProcessesDocuments(IEnumerable<TreeNode> documents, Func<TreeNode, PortalToMVCProcessDocumentPrimaryEventArgs, bool> handler);
        PortalToMVCProcessDocumentPrimaryEventArgs ProcessDocument(TreeNode document);
    }
}
