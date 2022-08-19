using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface IPreUpgrade4ConvertForms
    {
        void ConvertForms(Dictionary<string, string> oldToNew, string defaultSectionIdentifier, string defaultSectionType);
        IEnumerable<string> GetAllFormControlNames();
    }
}
