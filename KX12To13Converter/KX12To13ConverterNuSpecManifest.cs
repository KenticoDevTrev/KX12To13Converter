using CMS;
using CMS.DataEngine;
using CMS.Modules;
using KX12To13Converter.NuGet;
using NuGet;
using System.Collections.Generic;

[assembly: RegisterModule(typeof(KX12To13ConverterNuSpecManifestInitializationModule))]

namespace KX12To13Converter.NuGet
{
    public class KX12To13ConverterNuSpecManifestInitializationModule : Module
    {
        public KX12To13ConverterNuSpecManifestInitializationModule()
            : base("KX12To13ConverterNuSpecManifestInitializationModule")
        {
        }

        // Contains initialization code that is executed when the application starts
        protected override void OnInit()
        {
            base.OnInit();

            ModulePackagingEvents.Instance.BuildNuSpecManifest.After += BuildNuSpecManifest_After;
        }

        private void BuildNuSpecManifest_After(object sender, BuildNuSpecManifestEventArgs e)
        {
            if (e.ResourceName.Equals("KX12To13Converter", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // Change the name
                e.Manifest.Metadata.Title = "Kentico Xperience 12 Portal to KX13 Converter";
                e.Manifest.Metadata.ProjectUrl = "https://github.com/KenticoDevTrev/KX12To13Converter";
                e.Manifest.Metadata.IconUrl = "https://www.hbs.net/HBS/media/Favicon/favicon-96x96.png";
                e.Manifest.Metadata.Tags = "Kentico Xperience 12 13 Converter Migrater";
                e.Manifest.Metadata.Id = "KX12To13Converter.Admin";
                e.Manifest.Metadata.ReleaseNotes = "Improper Dependency Version for Kentico.Libraries";
                // Add nuget dependencies

                // Add dependencies
                e.Manifest.Metadata.DependencySets = new List<ManifestDependencySet>()
                {
                    new ManifestDependencySet()
                    {
                        Dependencies = new List<ManifestDependency>() {
                            new ManifestDependency()
                            {
                                Id="Kentico.Libraries",
                                Include = "12.0.29",
                                Version = "12.0.29",
                                Exclude = "13.0.0"
                            },
                             new ManifestDependency()
                            {
                                Id="KX12To13Converer.Shared",
                                Include = "12.29.0",
                                Version = "12.29.0",
                                Exclude = "13.0.0"
                            }
                        }
                    }
                };

            }
        }
    }
}