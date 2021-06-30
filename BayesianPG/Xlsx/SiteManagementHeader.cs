using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteManagementHeader : IXlsxWorksheetHeader
    {
        public int Age { get; set; }
        public int Foliage { get; set; }
        public int Root { get; set; }
        public int Site { get; set; }
        public int Species { get; set; }
        public int Stem { get; set; }
        public int Stems_n { get; set; }

        public SiteManagementHeader()
        {
            this.Age = -1;
            this.Foliage = -1;
            this.Root = -1;
            this.Site = -1;
            this.Species = -1;
            this.Stem = -1;
            this.Stems_n = -1;
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
                    case "age":
                        this.Age = index;
                        break;
                    case "stems_n":
                        this.Stems_n = index;
                        break;
                    case "stem":
                        this.Stem = index;
                        break;
                    case "root":
                        this.Root = index;
                        break;
                    case "foliage":
                        this.Foliage = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.Age < 0)
            {
                throw new XmlException("Age column not found in management header.");
            }
            if (this.Foliage < 0)
            {
                throw new XmlException("Foliage column not found in management header.");
            }
            if (this.Root < 0)
            {
                throw new XmlException("Root removal intensity not found in management header.");
            }
            if (this.Site < 0)
            {
                throw new XmlException("Site name column not found in management header.");
            }
            if (this.Species < 0)
            {
                throw new XmlException("Species column not found in management header.");
            }
            if (this.Stem < 0)
            {
                throw new XmlException("Stem removal intensity column not found in management header.");
            }
            if (this.Stems_n < 0)
            {
                throw new XmlException("Stems per hectare column not found in management header.");
            }
        }
    }
}
