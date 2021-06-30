using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteTreeSpeciesWorksheet : XlsxWorksheet<SiteTreeSpeciesWorksheetHeader>
    {
        public SortedList<string, SiteTreeSpecies> SpeciesBySite { get; private init; }

        public SiteTreeSpeciesWorksheet()
        {
            this.SpeciesBySite = new();
        }

        public override void ParseRow(XlsxRow row)
        {
            string siteName = row.Row[this.Header.Site];
            if (String.IsNullOrWhiteSpace(siteName))
            {
                throw new XmlException("Site's name is null or whitespace.", null, row.Number, this.Header.Site);
            }
            string speciesName = row.Row[this.Header.Species];
            if (String.IsNullOrWhiteSpace(speciesName))
            {
                throw new XmlException("Tree species is null or whitespace.", null, row.Number, this.Header.Site);
            }

            if (this.SpeciesBySite.TryGetValue(siteName, out SiteTreeSpecies? treeSpecies) == false)
            {
                treeSpecies = new();
                this.SpeciesBySite.Add(siteName, treeSpecies);
            }

            float initialFoliageBiomass = Single.Parse(row.Row[this.Header.Biom_foliage]);
            float initialRootBiomass = Single.Parse(row.Row[this.Header.Biom_root]);
            float initialStemBiomass = Single.Parse(row.Row[this.Header.Biom_stem]);
            float fertility = Single.Parse(row.Row[this.Header.Fertility]);
            DateTime planted = DateTime.ParseExact(row.Row[this.Header.Planted], "yyyy-MM", CultureInfo.InvariantCulture);
            float stems_n = Single.Parse(row.Row[this.Header.Stems_n]);

            if (initialFoliageBiomass < 0.0F)
            {
                throw new XmlException("biom_foliage", null, row.Number, this.Header.Biom_foliage);
            }
            if (initialRootBiomass < 0.0F)
            {
                throw new XmlException("biom_root", null, row.Number, this.Header.Biom_foliage);
            }
            if (initialStemBiomass < 0.0F)
            {
                throw new XmlException("biom_stem", null, row.Number, this.Header.Biom_foliage);
            }
            if ((fertility < 0.0F) || (fertility > 1.0F))
            {
                throw new XmlException("fertility", null, row.Number, this.Header.Fertility);
            }
            // planted is checked by DateTime.ParseExact()
            if (stems_n < 0.0F)
            {
                throw new XmlException("stems_n", null, row.Number, this.Header.Biom_foliage);
            }

            int newSpeciesIndex = treeSpecies.n_sp;
            treeSpecies.AllocateSpecies(new string[] { speciesName });

            treeSpecies.biom_foliage_i[newSpeciesIndex] = initialFoliageBiomass;
            treeSpecies.biom_root_i[newSpeciesIndex] = initialRootBiomass;
            treeSpecies.biom_stem_i[newSpeciesIndex] = initialStemBiomass;
            treeSpecies.fertility[newSpeciesIndex] = fertility;
            treeSpecies.month_p[newSpeciesIndex] = planted.Month;
            treeSpecies.stems_n_i[newSpeciesIndex] = stems_n;
            treeSpecies.year_p[newSpeciesIndex] = planted.Year;
        }
    }
}
