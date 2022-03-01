using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteTreeSpeciesHeader : IXlsxWorksheetHeader
    {
        public int Site { get; private set; }
        public int Species { get; private set; }
        public int Planted { get; private set; }
        public int Fertility { get; private set; }
        public int Stems_n { get; private set; }
        public int Biom_stem { get; private set; }
        public int Biom_root { get; private set; }
        public int Biom_foliage { get; private set; }

        public SiteTreeSpeciesHeader()
        {
            this.Biom_foliage = -1;
            this.Biom_root = -1;
            this.Biom_stem = -1;
            this.Fertility = -1;
            this.Planted = -1;
            this.Site = -1;
            this.Species = -1;
        }

        public void Parse(XlsxRow header)
        {
            for (int index = 0; index < header.Columns; ++index)
            {
                string column = header.Row[index];
                switch (column)
                {
                    case "site":
                        this.Site = index;
                        break;
                    case "species":
                        this.Species = index;
                        break;
                    case "planted":
                        this.Planted = index;
                        break;
                    case "fertility":
                        this.Fertility = index;
                        break;
                    case "stems_n":
                        this.Stems_n = index;
                        break;
                    case "biom_stem":
                        this.Biom_stem = index;
                        break;
                    case "biom_root":
                        this.Biom_root = index;
                        break;
                    case "biom_foliage":
                        this.Biom_foliage = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.Biom_foliage < 0)
            {
                throw new XmlException("Foliage biomass column not found in tree species header.");
            }
            if (this.Biom_root < 0)
            {
                throw new XmlException("Root biomass column not found in tree species header.");
            }
            if (this.Biom_stem < 0)
            {
                throw new XmlException("Stem biomass column not found in tree species header.");
            }
            if (this.Fertility < 0)
            {
                throw new XmlException("Soil fertility column not found in tree species header.");
            }
            if (this.Planted < 0)
            {
                throw new XmlException("Planting date column not found in tree species header.");
            }
            if (this.Site < 0)
            {
                throw new XmlException("Site name column not found in tree species header.");
            }
            if (this.Species < 0)
            {
                throw new XmlException("Species name column not found in tree species header.");
            }
            if (this.Stems_n < 0)
            {
                throw new XmlException("Stem count column not found in tree species header.");
            }
        }
    }
}
