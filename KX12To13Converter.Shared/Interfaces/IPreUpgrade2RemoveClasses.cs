using CMS.DataEngine;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface IPreUpgrade2RemoveClasses
    {
        void DeleteClasses(IEnumerable<int> classIds);
        void DeletePagesClasses(IEnumerable<int> classIds);
        string GetEntryValue(DataClassInfo dataClassInfo);
        string GetEntryValueForUsed(DataClassInfo classObj);
        IEnumerable<DataClassInfo> GetPageTypes();
        IEnumerable<DataClassInfo> GetUsedPageTypes();
        int References(string lookup, string table, string column);
    }
}
