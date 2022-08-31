using CMS;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Core;
using CMS.Helpers;
using CMS.UIControls;
using KX12To13Converter.Enums;
using KX12To13Converter.Extensions;
using KX12To13Converter.Interfaces;
using System.Linq;

[assembly: RegisterCustomClass("PageBuilderConversionUnigridExtender", typeof(PageBuilderConversionUnigridExtender))]

namespace KX12To13Converter.Extensions
{
    public class PageBuilderConversionUnigridExtender : ControlExtender<UniGrid>
    {
        public override void OnInit()
        {
            Control.OnAction += Control_OnAction;
            Control.OnExternalDataBound += Control_OnExternalDataBound;
            Control.AddHeaderAction(new HeaderAction()
            {
                CommandName = "MassRetry",
                Text = "Retry Selected",
                CssClass = "btn btn-primary"
            });
            Control.AddHeaderAction(new HeaderAction()
            {
                CommandName = "MassSend",
                Text = "Send Selected",
                CssClass = "btn btn-primary"
            });
            Control.AddHeaderAction(new HeaderAction()
            {
                CommandName = "MassSave",
                Text = "Save Selected to CMS_Document",
                CssClass = "btn btn-primary"
            });

            Control.HeaderActions.ActionPerformed += HeaderActions_ActionPerformed;
        }

        private void HeaderActions_ActionPerformed(object sender, System.Web.UI.WebControls.CommandEventArgs e)
        {
            var items = this.Control.SelectedItems.Select(x => ValidationHelper.GetInteger(x, 0)).ToArray();

            IPageBuilderUnigridExtensionMethods pageBuilderUnigridExtensionMethods = Service.Resolve<IPageBuilderUnigridExtensionMethods>();
            var results = pageBuilderUnigridExtensionMethods.HeaderActions_ActionPerformed(items, sender, e.CommandName);

            switch (results.Item1)
            {
                case CustomMessageType.Nothing:
                    return;
                case CustomMessageType.Information:
                    Control.AddMessage(MessageTypeEnum.Information, results.Item2);
                    break;
                case CustomMessageType.Confirmation:
                    Control.AddMessage(MessageTypeEnum.Confirmation, results.Item2);
                    break;
                case CustomMessageType.Warning:
                    Control.AddMessage(MessageTypeEnum.Warning, results.Item2);
                    break;
                case CustomMessageType.Error:
                    Control.AddMessage(MessageTypeEnum.Error, results.Item2);
                    break;
            }


        }

        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            IPageBuilderUnigridExtensionMethods pageBuilderUnigridExtensionMethods = Service.Resolve<IPageBuilderUnigridExtensionMethods>();
            return pageBuilderUnigridExtensionMethods.Control_OnExternalDataBound(sender, sourceName, parameter);


        }


        private void Control_OnAction(string actionName, object actionArgument)
        {
            IPageBuilderUnigridExtensionMethods pageBuilderUnigridExtensionMethods = Service.Resolve<IPageBuilderUnigridExtensionMethods>();
            var results = pageBuilderUnigridExtensionMethods.Control_OnAction(actionName, actionArgument);

            switch (results.Item1)
            {
                case CustomMessageType.Nothing:
                    return;
                case CustomMessageType.Information:
                    Control.AddMessage(MessageTypeEnum.Information, results.Item2);
                    break;
                case CustomMessageType.Confirmation:
                    Control.AddMessage(MessageTypeEnum.Confirmation, results.Item2);
                    break;
                case CustomMessageType.Warning:
                    Control.AddMessage(MessageTypeEnum.Warning, results.Item2);
                    break;
                case CustomMessageType.Error:
                    Control.AddMessage(MessageTypeEnum.Error, results.Item2);
                    break;
            }


            
        }
    }

}
