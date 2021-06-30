using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class ThreePGSettingsWorksheet : XlsxWorksheet<ThreePGSettingsWorksheetHeader>
    {
        public SortedList<string, ThreePGSettings> Settings { get; private init; }

        public ThreePGSettingsWorksheet()
        {
            this.Settings = new();
        }

        public override void ParseRow(XlsxRow row)
        {
            string siteName = row.Row[this.Header.Site];
            if (String.IsNullOrWhiteSpace(siteName))
            {
                throw new XmlException("Site's name is null or whitespace.", null, row.Number, this.Header.Site);
            }

            ThreePGSettings settings = new()
            {
                calculate_d13c = Boolean.Parse(row.Row[this.Header.CalculateD13C]),
                correct_bias = Boolean.Parse(row.Row[this.Header.CorrectSizeDistribution]),
                height_model = Enum.Parse<ThreePGHeightModel>(row.Row[this.Header.HeightModel], ignoreCase: true),
                light_model = Enum.Parse<ThreePGModel>(row.Row[this.Header.LightModel], ignoreCase: true),
                phys_model = Enum.Parse<ThreePGModel>(row.Row[this.Header.PhysiologicalModel], ignoreCase: true),
                management = Boolean.Parse(row.Row[this.Header.Management]),
                transp_model = Enum.Parse<ThreePGModel>(row.Row[this.Header.TranspirationModel], ignoreCase: true)
            };

            // all parameters are boolean or enum and therefore do not require additional validation

            this.Settings.Add(siteName, settings);
        }
    }
}
