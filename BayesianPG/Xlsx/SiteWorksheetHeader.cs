using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteWorksheetHeader : IXlsxWorksheetHeader
    {
        public int Altitude { get; set; }
        public int AvailableSoilWaterInitial { get; set; }
        public int AvailableSoilWaterMinimum { get; set; }
        public int AvailableSoilWaterMaximum { get; set; }
        public int Climate { get; set; }
        public int From { get; set; }
        public int Latitude { get; set; }
        public int Site { get; set; }
        public int SoilClass { get; set; }
        public int To { get; set; }

        public SiteWorksheetHeader()
        {
            this.Altitude = -1;
            this.AvailableSoilWaterInitial = -1;
            this.AvailableSoilWaterMaximum = -1;
            this.AvailableSoilWaterMinimum = -1;
            this.Climate = -1;
            this.From = -1;
            this.Latitude = -1;
            this.Site = -1;
            this.SoilClass = -1;
            this.To = -1;
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
                    case "latitude":
                        this.Latitude = index;
                        break;
                    case "altitude":
                        this.Altitude = index;
                        break;
                    case "soil_class":
                        this.SoilClass = index;
                        break;
                    case "asw_i":
                        this.AvailableSoilWaterInitial = index;
                        break;
                    case "asw_min":
                        this.AvailableSoilWaterMinimum = index;
                        break;
                    case "asw_max":
                        this.AvailableSoilWaterMaximum = index;
                        break;
                    case "from":
                        this.From = index;
                        break;
                    case "to":
                        this.To = index;
                        break;
                    case "climate":
                        this.Climate = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.Altitude < 0)
            {
                throw new XmlException("Altitude column not found in site header.");
            }
            if (this.AvailableSoilWaterInitial < 0)
            {
                throw new XmlException("Initially available soil water column not found in site header.");
            }
            if (this.AvailableSoilWaterMaximum < 0)
            {
                throw new XmlException("Maximum available soil water column not found in site header.");
            }
            if (this.AvailableSoilWaterMinimum < 0)
            {
                throw new XmlException("Minimum available soil water column not found in site header.");
            }
            if (this.Climate < 0)
            {
                throw new XmlException("Climate column not found in site header.");
            }
            if (this.From < 0)
            {
                throw new XmlException("Altitude column not found in site header.");
            }
            if (this.Latitude < 0)
            {
                throw new XmlException("Latitude column not found in site header.");
            }
            if (this.Site < 0)
            {
                throw new XmlException("Site name column not found in site header.");
            }
            if (this.SoilClass < 0)
            {
                throw new XmlException("Soil class column not found in site header.");
            }
            if (this.To < 0)
            {
                throw new XmlException("To column not found in site header.");
            }
        }
    }
}
