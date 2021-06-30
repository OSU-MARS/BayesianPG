using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class SiteWorksheet : XlsxWorksheet<SiteWorksheetHeader>
    {
        public SortedList<string, Site> Sites { get; private init; }

        public SiteWorksheet()
        {
            this.Sites = new();
        }

        public override void ParseRow(XlsxRow row)
        {
            string siteName = row.Row[this.Header.Site];
            if (String.IsNullOrWhiteSpace(siteName))
            {
                throw new XmlException("Site's name is null or whitespace.", null, row.Number, this.Header.Site);
            }

            Site site = new()
            {
                altitude = Single.Parse(row.Row[this.Header.Altitude]),
                aSW = Single.Parse(row.Row[this.Header.AvailableSoilWaterInitial]),
                asw_max = Single.Parse(row.Row[this.Header.AvailableSoilWaterMaximum]),
                asw_min = Single.Parse(row.Row[this.Header.AvailableSoilWaterMinimum]),
                Climate = row.Row[this.Header.Climate],
                Lat = Single.Parse(row.Row[this.Header.Latitude]),
                From = DateTime.ParseExact(row.Row[this.Header.From], "yyyy-MM", CultureInfo.InvariantCulture),
                soil_class = Int32.Parse(row.Row[this.Header.SoilClass]),
                To = DateTime.ParseExact(row.Row[this.Header.To], "yyyy-MM", CultureInfo.InvariantCulture)
            };
            // TODO: DateTime to = DateTime.ParseExact(row.Row[this.Header.Latitude], "yyyy-MM", CultureInfo.InvariantCulture);

            if ((site.Lat < -90.0F) || (site.Lat > 90.0F))
            {
                throw new XmlException(nameof(site.Lat), null, row.Number, this.Header.Latitude);
            }
            if ((site.altitude < -431.0F) || (site.altitude > 8848.0F))
            {
                throw new XmlException(nameof(site.altitude), null, row.Number, this.Header.Altitude);
            }
            if ((site.soil_class < -1) || (site.soil_class > 4))
            {
                throw new XmlException(nameof(site.soil_class), null, row.Number, this.Header.SoilClass);
            }
            if ((site.aSW < site.asw_min) || (site.aSW > site.asw_max))
            {
                throw new XmlException(nameof(site.aSW), null, row.Number, this.Header.AvailableSoilWaterInitial);
            }
            if ((site.asw_min < 0.0F) || (site.asw_min > site.asw_max))
            {
                throw new XmlException(nameof(site.asw_min), null, row.Number, this.Header.AvailableSoilWaterMinimum);
            }
            if ((site.asw_max < site.asw_min) || (site.asw_max > 5000.0F))
            {
                throw new XmlException(nameof(site.asw_max), null, row.Number, this.Header.AvailableSoilWaterMaximum);
            }
            // year_i and month_i are checked by DateTime.ParseExact()

            this.Sites.Add(siteName, site);
        }
    }
}
