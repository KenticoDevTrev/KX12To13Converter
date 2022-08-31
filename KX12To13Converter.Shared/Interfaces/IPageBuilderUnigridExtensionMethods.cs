using KX12To13Converter.Enums;
using System;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface IPageBuilderUnigridExtensionMethods
    {
        Tuple<CustomMessageType, string> HeaderActions_ActionPerformed(IEnumerable<int> pageConversionIDs, object sender, string commandName);
        object Control_OnExternalDataBound(object sender, string sourceName, object parameter);
        Tuple<CustomMessageType, string> Control_OnAction(string actionName, object actionArgument);
    }
}
