using CMS.DataEngine;
using CMS.Helpers;
using CMS.UIControls;
using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Core;
using KX12To13Converter.Interfaces;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts
{
    public partial class ConvertPageTypesForMVC : CMSPage
    {
        public IPreUpgrade3ConvertPageTypesForMVC PreUpgrade3ConvertPageTypesForMVC { get; }

        public ConvertPageTypesForMVC()
        {
            PreUpgrade3ConvertPageTypesForMVC = Service.Resolve<IPreUpgrade3ConvertPageTypesForMVC>();
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var pageBuilderPageTypes = PreUpgrade3ConvertPageTypesForMVC.GetPageBuidlerPageTypes();

            var pageBuilderContentOnly = PreUpgrade3ConvertPageTypesForMVC.GetPageBuilderContentOnly();

            // Get any page types that are parents of any URL enabled one.
            var classWithUrlPages = PreUpgrade3ConvertPageTypesForMVC.GetClassWithUrlPages();

            bool unConvertedTypesFound = (pageBuilderPageTypes.Any() || pageBuilderContentOnly.Any());
            if (unConvertedTypesFound)
            {
                pnlFirstRun.Visible = true;
                pnlSecondRun.Visible = false;
                pnlEnablePageBuilder.Controls.Add(new Literal()
                {
                    Text = "<h3>Page Types with Widgets/Content Zones possible</h3><ul>"
                });
                foreach (var pageType in pageBuilderPageTypes)
                {
                    // Check to see if any Database references found
                    WriteClassEntry(pageType);
                }
                pnlEnablePageBuilder.Controls.Add(new Literal()
                {
                    Text = "</ul><hr/><h3>Page Types without Widgets/Content Zones possible</h3><ul>"
                });
                foreach (var pageType in pageBuilderContentOnly)
                {
                    // Check to see if any Database references found
                    WriteClassEntryContentOnly(pageType);
                }
                pnlEnablePageBuilder.Controls.Add(new Literal()
                {
                    Text = "</ul>"
                });
            }
            else if (classWithUrlPages.Any())
            {
                pnlFirstRun.Visible = false;
                pnlSecondRun.Visible = true;


                pnlEnableUrlForParents.Controls.Add(new Literal()
                {
                    Text = "<ul>"
                });

                foreach (var pageType in classWithUrlPages)
                {
                    // Check to see if any Database references found
                    WriteClassEntryForUrl(pageType);
                }

                pnlEnableUrlForParents.Controls.Add(new Literal()
                {
                    Text = "</ul>"
                });

            }
            else
            {
                pnlFirstRun.Visible = false;
                pnlSecondRun.Visible = false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }



        public void WriteClassEntry(DataClassInfo classObj)
        {
            int totalWidgetContentPages = PreUpgrade3ConvertPageTypesForMVC.GetTotalWidgetContentPages(classObj);
            int totalPages = PreUpgrade3ConvertPageTypesForMVC.GetTotalPages(classObj);

            Panel classEntry = new Panel()
            {
                ID = $"pnlConversion1_{classObj.ClassID}",
                ClientIDMode = ClientIDMode.Static
            };
            classEntry.Controls.Add(new Literal()
            {
                Text = $"<li><strong>[{classObj.ClassName}] {classObj.ClassDisplayName}</strong> [{totalWidgetContentPages}/{totalPages} with Widget Content]<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Conversion Mode: "
            });
            classEntry.Controls.Add(GetSelectControl(classObj, totalWidgetContentPages > 0, false));
            classEntry.Controls.Add(new Literal()
            {
                Text = $"<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;MVC Template ID: "
            });
            classEntry.Controls.Add(GetDefaultTemplateControl(classObj, totalWidgetContentPages > 0, false));
            classEntry.Controls.Add(new Literal()
            {
                Text = $"</li>"
            });
            pnlEnablePageBuilder.Controls.Add(classEntry);
        }

        public void WriteClassEntryContentOnly(DataClassInfo classObj)
        {
            int totalPages = PreUpgrade3ConvertPageTypesForMVC.GetTotalPages(classObj);

            Panel classEntry = new Panel()
            {
                ID = $"pnlConversion1_{classObj.ClassID}",
                ClientIDMode = ClientIDMode.Static
            };
            classEntry.Controls.Add(new Literal()
            {
                Text = $"<li><strong>[{classObj.ClassName}] {classObj.ClassDisplayName}</strong> [{totalPages} pages]<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Conversion Mode: "
            });
            classEntry.Controls.Add(GetSelectControl(classObj, false, true));
            classEntry.Controls.Add(new Literal()
            {
                Text = $"</li>"
            });
            pnlEnablePageBuilder.Controls.Add(classEntry);
        }

        public void WriteClassEntryForUrl(DataClassInfo classObj)
        {
            Panel classEntry = new Panel()
            {
                ID = $"pnlConversion2_{classObj.ClassID}",
                ClientIDMode = ClientIDMode.Static
            };
            classEntry.Controls.Add(new Literal()
            {
                Text = $"<li><strong>[{classObj.ClassName}] {classObj.ClassDisplayName}</strong><br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Conversion Mode: "
            });
            classEntry.Controls.Add(GetSelectControl(classObj, false, true));
            classEntry.Controls.Add(new Literal()
            {
                Text = $"</li>"
            });
            pnlEnableUrlForParents.Controls.Add(classEntry);
        }

        private TextBox GetDefaultTemplateControl(DataClassInfo classObj, bool hasWidgetPages, bool contentOnly)
        {
            return new TextBox()
            {
                ID = $"tbxConversion1_{classObj.ClassID}",
                ClientIDMode = ClientIDMode.Static,
                CssClass = "form-control",
                Text = (hasWidgetPages ? $"{classObj.ClassName}_Default" : "")
            };
        }

        private DropDownList GetSelectControl(DataClassInfo classObj, bool hasWidgetPages, bool contentOnly)
        {
            DropDownList dropDownList = new DropDownList()
            {
                ID = $"ddlConversion1_{classObj.ClassID}",
                ClientIDMode = ClientIDMode.Static,
                CssClass = "form-control"
            };
            if (!contentOnly)
            {
                dropDownList.Items.AddRange(new ListItem[] {
            new ListItem("Page Builder Enabled", "pagebuilder")
            {
                Selected = hasWidgetPages
            },
            new ListItem("Url Enabled Only", "urlonly")
            {
                Selected = !hasWidgetPages
            },
            new ListItem("Neither Url nor Page Builder", "neither")
            {
                Selected = false
            }
            });
            }
            else
            {
                dropDownList.Items.AddRange(new ListItem[] {
            new ListItem("Url Enabled", "urlonly")
            {
                Selected = false
            },
            new ListItem("Neither Url nor Page Builder", "neither")
            {
                Selected = true
            }
            });
            }
            return dropDownList;
        }


        protected void btnAdjustClasses_Click(object sender, EventArgs e)
        {
            // loop through panel
            foreach (var pnl in pnlEnablePageBuilder.Controls.Cast<Control>().Where(x => x is Panel).Select(x => (Panel)x))
            {
                if (pnl.ClientID == null || !pnl.ClientID.StartsWith("pnlConversion1"))
                {
                    continue;
                }
                int classID = int.Parse(pnl.ID.Replace("pnlConversion1_", ""));
                DropDownList ddlControl = (DropDownList)pnl.FindControl($"ddlConversion1_{classID}");
                TextBox txtControl = (TextBox)pnl.FindControl($"tbxConversion1_{classID}");

                PreUpgrade3ConvertPageTypesForMVC.AdjustClass(ddlControl.SelectedValue, txtControl?.Text, classID);
            }

            CacheHelper.ClearCache();
            SystemHelper.RestartApplication();
            URLHelper.RefreshCurrentPage();
        }

        protected void btnEnableUrlFeature_Click(object sender, EventArgs e)
        {
            // loop through panel
            foreach (var pnl in pnlEnableUrlForParents.Controls.Cast<Control>().Where(x => x is Panel).Select(x => (Panel)x))
            {
                if (pnl.ClientID == null || !pnl.ClientID.StartsWith("pnlConversion2"))
                {
                    continue;
                }
                int classID = int.Parse(pnl.ID.Replace("pnlConversion2_", ""));
                DropDownList ddlControl = (DropDownList)pnl.FindControl($"ddlConversion1_{classID}");
                var classObj = DataClassInfoProvider.GetDataClassInfo(classID);

                PreUpgrade3ConvertPageTypesForMVC.EnableUrlFeature(ddlControl.SelectedValue, classObj);
            }

            CacheHelper.ClearCache();
            SystemHelper.RestartApplication();
            URLHelper.RefreshCurrentPage();
        }

    }

}