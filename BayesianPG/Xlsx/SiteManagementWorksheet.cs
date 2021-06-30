using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteManagementWorksheet : XlsxWorksheet<SiteManagementHeader>
    {
        public SortedList<string, TreeSpeciesManagement> Management { get; private init; }

        public SiteManagementWorksheet()
        {
            this.Management = new();
        }

        public override void ParseRow(XlsxRow row)
        {
            string siteName = row.Row[this.Header.Site];
            if (String.IsNullOrWhiteSpace(siteName))
            {
                throw new XmlException("Site's name is null or whitespace.", null, row.Number, this.Header.Site);
            }

            if (this.Management.TryGetValue(siteName, out TreeSpeciesManagement? management) == false)
            {
                management = new();
                this.Management.Add(siteName, management);
            }

            string species = row.Row[this.Header.Species];
            int speciesIndex = management.Name.FindIndex(species);
            if (speciesIndex == -1)
            {
                speciesIndex = management.n_sp;
                management.AllocateSpecies(new string[] { species });
            }

            float age = Single.Parse(row.Row[this.Header.Age]);
            float foliage = Single.Parse(row.Row[this.Header.Foliage]);
            float root = Single.Parse(row.Row[this.Header.Root]);
            float stem = Single.Parse(row.Row[this.Header.Stem]);
            float stems_n = Single.Parse(row.Row[this.Header.Stems_n]);

            if (age < 0.0F)
            {
                throw new XmlException(nameof(age), null, row.Number, this.Header.Age);
            }
            if ((foliage < 0.0F) || (foliage > 1.0F))
            {
                throw new XmlException(nameof(foliage), null, row.Number, this.Header.Age);
            }
            if ((root < 0.0F) || (root > 1.0F))
            {
                throw new XmlException(nameof(root), null, row.Number, this.Header.Root);
            }
            if ((stem < 0.0F) || (stem > 1.0F))
            {
                throw new XmlException(nameof(stem), null, row.Number, this.Header.Stem);
            }
            if (stems_n < 0.0F)
            {
                throw new XmlException(nameof(stems_n), null, row.Number, this.Header.Stems_n);
            }

            int managementIndex = management.AllocateManagement(speciesIndex);
            management.age[speciesIndex][managementIndex] = age;
            management.foliage[speciesIndex][managementIndex] = foliage;
            management.root[speciesIndex][managementIndex] = root;
            management.stem[speciesIndex][managementIndex] = stem;
            management.stems_n[speciesIndex][managementIndex] = stems_n;
        }
    }
}
