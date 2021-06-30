using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteClimateWorksheet : XlsxWorksheet<SiteClimateWorksheetHeader>
    {
        public SortedList<string, SiteClimate> Sites { get; private init; }

        public SiteClimateWorksheet()
        {
            this.Sites = new();
        }

        public override void ParseRow(XlsxRow row)
        {
            // allocate space for row if needed
            string siteName = row.Row[this.Header.ClimateID];
            if (this.Sites.TryGetValue(siteName, out SiteClimate? climate) == false)
            {
                climate = new();
                this.Sites.Add(siteName, climate);
            }

            int timestepIndex = climate.n_m;
            if (timestepIndex >= climate.Capacity)
            {
                climate.AllocateDecade();
            }

            // parse row
            float co2 = Single.Parse(row.Row[this.Header.CO2]);
            float d13Catm = Single.NaN;
            string d13CatmString = row.Row[this.Header.D13CAtm]; // optional
            if (string.IsNullOrEmpty(d13CatmString) == false)
            {
                d13Catm = Single.Parse(d13CatmString);
            }
            float frostDays = Single.Parse(row.Row[this.Header.FrostDays]);
            float precip = Single.Parse(row.Row[this.Header.Precipitation]);
            float solarRadiation = Single.Parse(row.Row[this.Header.SolarRadiation]);
            float maximumTemperature = Single.Parse(row.Row[this.Header.TemperatureMax]);
            float minimumTemperature = Single.Parse(row.Row[this.Header.TemperatureMin]);

            float averageTemperature;
            string averageTemperatureString = row.Row[this.Header.TemperatureAverage]; // optional and imputed
            if (String.IsNullOrEmpty(averageTemperatureString))
            {
                averageTemperature = 0.5F * (maximumTemperature + minimumTemperature); // prepare_climate.R:100
            }
            else
            {
                averageTemperature = Single.Parse(averageTemperatureString);
            }

            // check row
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
            climate.co2[timestepIndex] = co2;
            climate.d13Catm[timestepIndex] = d13Catm;
            climate.frost_days[timestepIndex] = frostDays;
            climate.prcp[timestepIndex] = precip;
            climate.solar_rad[timestepIndex] = solarRadiation;
            climate.tmp_ave[timestepIndex] = averageTemperature;
            climate.tmp_max[timestepIndex] = maximumTemperature;
            ++climate.n_m;
            // not currently stored
            //   row.Row[this.Header.Year]
            //   row.Row[this.Header.Month]
            //   climate.tmp_min[newMonthIndex] = Single.Parse(row.Row[this.Header.TemperatureMin]);

            // calculate vapor pressure difference as it's not specified
            // prepare_climate.R:get_vpd()
            float vpd_min = 6.10780F * MathF.Exp(17.2690F * minimumTemperature / (237.30F + minimumTemperature));
            float vpd_max = 6.10780F * MathF.Exp(17.2690F * maximumTemperature / (237.30F + maximumTemperature));
            climate.vpd_day[timestepIndex] = 0.5F * (vpd_max - vpd_min);
        }
    }
}