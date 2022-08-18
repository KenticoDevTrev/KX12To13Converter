using CMS.Base;
using CMS.UIControls;
using KX12To13Converter.Base.PageOperations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts
{
    public partial class ConvertForms : CMSPage
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var formControls = PreUpgrade4ConvertForms.GetAllFormControlNames();

            string config = string.Empty;
            foreach (string control in formControls)
            {
                switch (control)
                {
                    case "checkboxcontrol":
                        config += $"{control}=CheckBoxComponent\n";
                        break;
                    case "dropdownlistcontrol":
                        config += $"{control}=DropDownComponent\n";
                        break;
                    case "emailinput":
                        config += $"{control}=EmailInputComponent\n";
                        break;
                    case "htmlareacontrol":
                    case "textareacontrol":
                        config += $"{control}=TextAreaComponent\n";
                        break;
                    case "radiobuttonscontrol":
                        config += $"{control}=RadioButtonsComponent\n";
                        break;
                    case "multiplechoicecontrol":
                        config += $"{control}=MultipleChoiceComponent\n";
                        break;
                    case "simplecaptcha":
                        config += $"{control}=RecaptchaComponent\n";
                        break;
                    case "textboxcontrol":
                        config += $"{control}=TextInputComponent\n";
                        break;
                    case "uploadcontrol":
                        config += $"{control}=FileUploaderComponent\n";
                        break;
                    case "usphone":
                        config += $"{control}=USPhoneComponent\n";
                        break;
                    case "uszipcode":
                        config += $"{control}=TextInputComponent\n";
                        break;
                    default:
                        config += $"{control}=\n";
                        break;
                }
            }
            txtConfig.Rows = formControls.Count();
            txtConfig.Text += config;
        }


        protected void btnConvert_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> oldToNew = new Dictionary<string, string>();
            foreach (string configLine in txtConfig.Text.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (!configLine.Contains('=') || configLine.Split('=')[1].Length == 0)
                {
                    throw new Exception("Must have every configuration for each control");
                }
                oldToNew.Add(configLine.Split('=')[0], configLine.Split('=')[1]);
            }
            PreUpgrade4ConvertForms.ConvertForms(oldToNew, txtDefaultSectionIdentifier.Text, txtDefaultSectionType.Text);
            
            ltrResult.Text = "<div class='alert alert-info'>Operations successful</div>";
        }
    }

}