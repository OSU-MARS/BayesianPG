using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteClimateWorksheet : XlsxWorksheet<SiteClimateHeader>
    {
        private string previousClimateName;
        private int previousMonth;
        private int previousYear;

        public SortedList<string, SiteClimate> Sites { get; private init; }

        public SiteClimateWorksheet()
        {
            this.previousClimateName = String.Empty;
            this.previousMonth = Int32.MinValue;
            this.previousYear = Int32.MinValue;

            this.Sites = [];
        }

        public override void ParseRow(XlsxRow row)
        {
            // allocate space for row if needed
            string climateID = row.Row[this.Header.ClimateID];
            if (String.IsNullOrWhiteSpace(climateID))
            {
                throw new XmlException("Climate name is null or whitespace.", null, row.Number, this.Header.ClimateID);
            }
            if (this.Sites.TryGetValue(climateID, out SiteClimate? climate) == false)
            {
                climate = new();
                this.Sites.Add(climateID, climate);
            }

            int timestepIndex = climate.MonthCount;
            if (timestepIndex >= climate.Capacity)
            {
                climate.AllocateDecade();
            }

            // parse row
            string climateName = row.Row[this.Header.ClimateID];
            float co2 = Single.Parse(row.Row[this.Header.CO2], CultureInfo.InvariantCulture);
            float d13Catm = Single.NaN;
            string d13CatmString = row.Row[this.Header.D13CAtm]; // optional
            if (String.IsNullOrEmpty(d13CatmString) == false)
            {
                d13Catm = Single.Parse(d13CatmString, CultureInfo.InvariantCulture);
            }
            float frostDays = Single.Parse(row.Row[this.Header.FrostDays], CultureInfo.InvariantCulture);
            float precip = Single.Parse(row.Row[this.Header.Precipitation], CultureInfo.InvariantCulture);
            float solarRadiation = Single.Parse(row.Row[this.Header.SolarRadiation], CultureInfo.InvariantCulture);
            float maximumTemperature = Single.Parse(row.Row[this.Header.TemperatureMax], CultureInfo.InvariantCulture);
            float minimumTemperature = Single.Parse(row.Row[this.Header.TemperatureMin], CultureInfo.InvariantCulture);
            int month = Int32.Parse(row.Row[this.Header.Month], CultureInfo.InvariantCulture);
            int year = Int32.Parse(row.Row[this.Header.Year], CultureInfo.InvariantCulture);

            float averageTemperature;
            string averageTemperatureString = row.Row[this.Header.TemperatureAverage]; // optional and imputed
            if (String.IsNullOrEmpty(averageTemperatureString))
            {
                averageTemperature = 0.5F * (maximumTemperature + minimumTemperature); // prepare_climate.R:100
            }
            else
            {
                averageTemperature = Single.Parse(averageTemperatureString, CultureInfo.InvariantCulture);
            }

            // check row
            if (row.Index > 1) // this.previousClimateName, previousMonth and previousYear are initialized on first data row
            {
                // each row should increment month
                // This check can be defeated if climates are not presented in continguous blocks. For now, it's assumed that
                // they are.
                if ((month != this.previousMonth + 1) || (year != this.previousYear))
                {
                    // but this row is not a simple month increment...
                    if ((month != 1) || (this.previousMonth != 12) || (year != this.previousYear + 1))
                    {
                        // ...and it's not a transition from December to January...
                        if (String.Equals(climateName, this.previousClimateName, StringComparison.OrdinalIgnoreCase))
                        {
                            // ...and it's not a transition between climates
                            throw new XmlException(row.Row[this.Header.ClimateID] + ": " + nameof(year) + "-" + nameof(month), null, row.Number, this.Header.Year);
                        }
                    }
                }
            }
            if ((co2 < 0.0F) || (co2 > 2000.0F))
            {
                throw new XmlException(nameof(co2), null, row.Number, this.Header.CO2);
            }
            if ((Single.IsNaN(d13Catm) == false) && ((d13Catm < -25.0F) || (d13Catm > 0.0F)))
            {
                throw new XmlException(nameof(d13Catm), null, row.Number, this.Header.D13CAtm);
            }
            if ((frostDays < 0.0F) || (frostDays > 366.0F))
            {
                throw new XmlException(nameof(frostDays), null, row.Number, this.Header.FrostDays);
            }
            if ((precip < 0.0F) || (precip > 30000.0F))
            {
                throw new XmlException(nameof(precip), null, row.Number, this.Header.Precipitation);
            }
            if ((solarRadiation < 0.0F) || (solarRadiation > 100.0F))
            {
                throw new XmlException(nameof(solarRadiation), null, row.Number, this.Header.SolarRadiation);
            }
            if ((maximumTemperature < minimumTemperature) || (maximumTemperature > 60.0F))
            {
                throw new XmlException(nameof(solarRadiation), null, row.Number, this.Header.TemperatureMax);
            }
            if ((averageTemperature < minimumTemperature) || (averageTemperature > maximumTemperature))
            {
                throw new XmlException(nameof(averageTemperature), null, row.Number, this.Header.TemperatureAverage);
            }
            if ((minimumTemperature < -50.0F) || (minimumTemperature > maximumTemperature))
            {
                throw new XmlException(nameof(solarRadiation), null, row.Number, this.Header.TemperatureMax);
            }

            // store row
            climate.AtmosphericCO2[timestepIndex] = co2;
            climate.D13Catm[timestepIndex] = d13Catm;
            if (climate.MonthCount == 0) // first row encountered for climate
            {
                climate.From = new(year, month, 1);
            }
            climate.FrostDays[timestepIndex] = frostDays;
            climate.TotalPrecipitation[timestepIndex] = precip;
            climate.MeanDailySolarRadiation[timestepIndex] = solarRadiation;
            climate.MeanDailyTemp[timestepIndex] = averageTemperature;
            climate.MeanDailyTempMax[timestepIndex] = maximumTemperature;
            ++climate.MonthCount;
            // not currently stored
            //   row.Row[this.Header.Year]
            //   row.Row[this.Header.Month]
            //   climate.tmp_min[newMonthIndex] = Single.Parse(row.Row[this.Header.TemperatureMin]);

            // calculate vapor pressure difference as it's not specified
            // prepare_climate.R:get_vpd()
            float vpd_min = 6.10780F * MathF.Exp(17.2690F * minimumTemperature / (237.30F + minimumTemperature));
            float vpd_max = 6.10780F * MathF.Exp(17.2690F * maximumTemperature / (237.30F + maximumTemperature));
            climate.MeanDailyVpd[timestepIndex] = 0.5F * (vpd_max - vpd_min);

            this.previousClimateName = climateName;
            this.previousMonth = month;
            this.previousYear = year;
        }
    }
}