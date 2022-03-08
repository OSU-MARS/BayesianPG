using BayesianPG.Extensions;
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
                Altitude = Single.Parse(row.Row[this.Header.Altitude], CultureInfo.InvariantCulture),
                AvailableSoilWaterInitial = Single.Parse(row.Row[this.Header.AvailableSoilWaterInitial], CultureInfo.InvariantCulture),
                AvailableSoilWaterMax = Single.Parse(row.Row[this.Header.AvailableSoilWaterMaximum], CultureInfo.InvariantCulture),
                AvailableSoilWaterMin = Single.Parse(row.Row[this.Header.AvailableSoilWaterMinimum], CultureInfo.InvariantCulture),
                Climate = row.Row[this.Header.Climate],
                Latitude = Single.Parse(row.Row[this.Header.Latitude], CultureInfo.InvariantCulture),
                From = DateTimeExtensions.FromExcel(Int32.Parse(row.Row[this.Header.From], CultureInfo.InvariantCulture)),
                Name = siteName,
                SoilClass = Single.Parse(row.Row[this.Header.SoilClass], CultureInfo.InvariantCulture),
                To = DateTimeExtensions.FromExcel(Int32.Parse(row.Row[this.Header.To], CultureInfo.InvariantCulture))
            };

            if ((site.Altitude < -431.0F) || (site.Altitude > 8848.0F))
            {
                throw new XmlException(nameof(site.Altitude), null, row.Number, this.Header.Altitude);
            }
            if ((site.AvailableSoilWaterInitial < site.AvailableSoilWaterMin) || (site.AvailableSoilWaterInitial > site.AvailableSoilWaterMax))
            {
                throw new XmlException(nameof(site.AvailableSoilWaterInitial), null, row.Number, this.Header.AvailableSoilWaterInitial);
            }
            if ((site.AvailableSoilWaterMin < 0.0F) || (site.AvailableSoilWaterMin > site.AvailableSoilWaterMax))
            {
                throw new XmlException(nameof(site.AvailableSoilWaterMin), null, row.Number, this.Header.AvailableSoilWaterMinimum);
            }
            if ((site.AvailableSoilWaterMax < site.AvailableSoilWaterMin) || (site.AvailableSoilWaterMax > 5000.0F))
            {
                throw new XmlException(nameof(site.AvailableSoilWaterMax), null, row.Number, this.Header.AvailableSoilWaterMaximum);
            }
            // site.From is checked by DateTime
            if ((site.Latitude < -90.0F) || (site.Latitude > 90.0F))
            {
                throw new XmlException(nameof(site.Latitude), null, row.Number, this.Header.Latitude);
            }
            if ((site.SoilClass < -1.0F) || (site.SoilClass > 5.0F))
            {
                throw new XmlException(nameof(site.SoilClass), null, row.Number, this.Header.SoilClass);
            }

            this.Sites.Add(siteName, site);
        }
    }
}
