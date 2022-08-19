using CMS;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Core;
using CMS.Helpers;
using CMS.UIControls;
using KX12To13Converter.Extensions;
using KX12To13Converter.Interfaces;
using System;
using System.Collections;
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
                CommandName = "MassSend",
                Text = "Send Selected",
                CssClass = "btn btn-primary"
            });
            Control.AddHeaderAction(new HeaderAction()
            {
                CommandName = "MassRetry",
                Text = "Retry Selected",
                CssClass = "btn btn-primary"
            });
            Control.HeaderActions.ActionPerformed += HeaderActions_ActionPerformed;
        }

        private void HeaderActions_ActionPerformed(object sender, System.Web.UI.WebControls.CommandEventArgs e)
        {
            var items = this.Control.SelectedItems.Select(x => ValidationHelper.GetInteger(x, 0)).ToArray();
            IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
            int itemsSuccess = 0;
            int itemsFailure = 0;

            foreach (var pageBuilderConversion in PageBuilderConversionsInfoProvider.GetPageBuilderConversions().WhereIn(nameof(PageBuilderConversionsInfo.PageBuilderConversionsID), items).TypedResult)
            {
                try
                {
                    switch (e.CommandName.ToLower())
                    {
                        case "masssend":
                            if (conversionProcessingMethods.SendDocument(pageBuilderConversion))
                            {
                                itemsSuccess++;
                            }
                            else
                            {
                                itemsFailure++;
                            }
                            break;
                        case "massretry":
                            if (conversionProcessingMethods.ReProcesses(pageBuilderConversion))
                            {
                                itemsSuccess++;
                            }
                            else
                            {
                                itemsFailure++;
                            }
                            break;
                    }
                } catch(Exception ex)
                {
                    itemsFailure++;
                }
            }

            this.Control.AddMessage(MessageTypeEnum.Information, $"{itemsSuccess} Successfully processed, {itemsFailure} Failed");

        }

        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (sourceName.Equals("HasErrors"))
            {
                string notes = ValidationHelper.GetString(parameter, string.Empty);
                return $"{(notes.Contains("IsError: true") ? "Yes" : "No")}";
            }
            if (sourceName.Equals("Notes"))
            {
                string notes = ValidationHelper.GetString(parameter, string.Empty);
                if (!notes.Equals("[]"))
                {
                    return $"<span style=\"pointer: cursor; text-decoration: underline; color: blue;\" title=\"{HTMLHelper.EncodeForHtmlAttribute(notes)}\">Hover</span>";
                }
                else
                {
                    return "";
                }
            }

            return parameter;

        }

        private void Control_OnAction(string actionName, object actionArgument)
        {
            if (actionName.Equals("#send"))
            {
                int id = Convert.ToInt32(actionArgument);
                IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
                bool result = conversionProcessingMethods.SendDocument(PageBuilderConversionsInfoProvider.GetPageBuilderConversionsInfo(id));
                if (result)
                {
                    Control.AddMessage(MessageTypeEnum.Confirmation, "Send Successful");
                }
                else
                {
                    Control.AddMessage(MessageTypeEnum.Error, "Send Error, see conversion notes for more details");
                }
            }
            else if (actionName.Equals("#retry"))
            {
                int id = Convert.ToInt32(actionArgument);

                IConversionProcessingMethods conversionProcessingMethods = Service.Resolve<IConversionProcessingMethods>();
                bool conversionResults = conversionProcessingMethods.ReProcesses(PageBuilderConversionsInfoProvider.GetPageBuilderConversionsInfo(id));
                if (conversionResults)
                {
                    Control.AddMessage(MessageTypeEnum.Confirmation, "Conversion Successful");
                }
                else
                {
                    Control.AddMessage(MessageTypeEnum.Error, "Conversion failed, see conversion notes for more details");
                }
            }
            else
            {
                return;
            }
        }
    }
}
