using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteClimateWorksheetHeader : IXlsxWorksheetHeader
    {
        public int ClimateID { get; private set; }
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int TemperatureMin { get; private set; }
        public int TemperatureMax { get; private set; }
        public int TemperatureAverage { get; private set; }
        public int Precipitation { get; private set; }
        public int SolarRadiation { get; private set; }
        public int FrostDays { get; private set; }
        public int CO2 { get; private set; }
        public int D13CAtm { get; private set; }

        public SiteClimateWorksheetHeader()
        {
            this.ClimateID = -1;
            this.Year = -1;
            this.Month = -1;
            this.TemperatureMin = -1;
            this.TemperatureMax = -1;
            this.TemperatureAverage = -1;
            this.Precipitation = -1;
            this.SolarRadiation = -1;
            this.FrostDays = -1;
            this.CO2 = -1;
            this.D13CAtm = -1;
        }

        public void Parse(XlsxRow header)
        {
            for (int index = 0; index < header.Columns; ++index)
            {
                string column = header.Row[index];
                switch (column)
                {
                    case "climate_id":
                        this.ClimateID = index;
                        break;
                    case "year":
                        this.Year = index;
                        break;
                    case "month":
                        this.Month = index;
                        break;
                    case "tmp_min":
                        this.TemperatureMin = index;
                        break;
                    case "tmp_max":
                        this.TemperatureMax = index;
                        break;
                    case "tmp_ave":
                        this.TemperatureAverage = index;
                        break;
                    case "prcp":
                        this.Precipitation = index;
                        break;
                    case "srad":
                        this.SolarRadiation = index;
                        break;
                    case "frost_days":
                        this.FrostDays = index;
                        break;
                    case "co2":
                        this.CO2 = index;
                        break;
                    case "d13catm":
                        this.D13CAtm = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.ClimateID < 0)
            {
                throw new XmlException("Climate name column not found in climate header.");
            }
            if (this.Year < 0)
            {
                throw new XmlException("Year column not found in climate header.");
            }
            if (this.Month < 0)
            {
                throw new XmlException("Month column not found in climate header.");
            }
            if (this.TemperatureMin < 0)
            {
                throw new XmlException("Minimum temperature column not found in climate header.");
            }
            if (this.TemperatureMax < 0)
            {
                throw new XmlException("Maxiumum temperature column not found in climate header.");
            }
            if (this.TemperatureAverage < 0)
            {
                throw new XmlException("Mean temperature column not found in climate header.");
            }
            if (this.Precipitation < 0)
            {
                throw new XmlException("Precipitation column not found in climate header.");
            }
            if (this.SolarRadiation < 0)
            {
                throw new XmlException("Solar radiation column not found in climate header.");
            }
            if (this.FrostDays < 0)
            {
                throw new XmlException("Frost days column not found in climate header.");
            }
            if (this.CO2 < 0)
            {
                throw new XmlException("CO₂ column not found in climate header.");
            }
            if (this.D13CAtm < 0)
            {
                throw new XmlException("δ¹³C column not found in climate header.");
            }
        }
    }
}
