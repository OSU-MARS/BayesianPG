using BayesianPG.ThreePG;
using System.Collections.Generic;

namespace BayesianPG.Xlsx
{
    public class ThreePGReader : XlsxReader
    {
        public ThreePGReader(string xlsxFilePath)
            : base(xlsxFilePath)
        {
        }

        public SortedList<string, ThreePGScalar> ReadSites()
        {
            SiteWorksheet sites = this.ReadWorksheet<SiteWorksheet>("site");
            SiteClimateWorksheet climates = this.ReadWorksheet<SiteClimateWorksheet>("climate");
            SiteTreeSpeciesWorksheet treeSpecies = this.ReadWorksheet<SiteTreeSpeciesWorksheet>("species");
            TreeSpeciesParameterWorksheet treeSpeciesParameters = this.ReadWorksheet<TreeSpeciesParameterWorksheet>("parameters");
            TreeSpeciesSizeWorksheet treeSizes = this.ReadWorksheet<TreeSpeciesSizeWorksheet>("sizeDist");
            SiteManagementWorksheet siteManagement = this.ReadWorksheet<SiteManagementWorksheet>("thinning");
            ThreePGSettingsWorksheet threePGsettings = this.ReadWorksheet<ThreePGSettingsWorksheet>("settings");

            SortedList<string, ThreePGScalar> threePGbySiteName = new(sites.Sites.Count);
            for (int index = 0; index < sites.Sites.Count; ++index)
            {
                string siteName = sites.Sites.Keys[index];
                Site site = sites.Sites.Values[index];
                SiteClimate climate = climates.Sites[site.Climate];
                SiteTreeSpecies trees = treeSpecies.SpeciesBySite[siteName];
                TreeSpeciesParameters treeParametersForSite = treeSpeciesParameters.Parameters.Filter(trees);
                TreeSpeciesSizeDistribution treeSizesOnSite = treeSizes.Sizes.Filter(trees);
                ThreePGSettings settings = threePGsettings.Settings[siteName];
                TreeSpeciesManagement management = TreeSpeciesManagement.None;
                if (settings.management)
                {
                    management = siteManagement.Management[siteName];
                }

                ThreePGScalar threePG = new(site, climate, trees, treeParametersForSite, management, settings)
                {
                    Bias = treeSizesOnSite
                };
                threePGbySiteName.Add(siteName, threePG);
            }

            return threePGbySiteName;
        }
    }
}
