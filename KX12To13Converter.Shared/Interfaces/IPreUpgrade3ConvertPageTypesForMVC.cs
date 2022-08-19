using CMS.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Interfaces
{
    public interface IPreUpgrade3ConvertPageTypesForMVC
    {
        void AdjustClass(string adjustToValue, string defaultTemplate, int classID);
        void EnableUrlFeature(string adjustToValue, DataClassInfo classObj);
        IEnumerable<DataClassInfo> GetClassWithUrlPages();
        IEnumerable<DataClassInfo> GetPageBuidlerPageTypes();
        IEnumerable<DataClassInfo> GetPageBuilderContentOnly();
        int GetTotalPages(DataClassInfo classObj);
        int GetTotalWidgetContentPages(DataClassInfo classObj);
    }
}
