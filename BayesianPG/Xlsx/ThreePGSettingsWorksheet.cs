using BayesianPG.ThreePG;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class ThreePGSettingsWorksheet : XlsxWorksheet<ThreePGSettingsHeader>
    {
        public SortedList<string, ThreePGSettings> Settings { get; private init; }

        public ThreePGSettingsWorksheet()
        {
            this.Settings = [];
        }

        public override void ParseRow(XlsxRow row)
        {
            string siteName = row.Row[this.Header.Site];
            if (String.IsNullOrWhiteSpace(siteName))
            {
                throw new XmlException("Site's name is null or whitespace.", null, row.Number, this.Header.Site);
            }

            // bias correction iterations is not currently a parseable setting
            ThreePGSettings settings = new()
            {
                CalculateD13C = Boolean.Parse(row.Row[this.Header.CalculateD13C]),
                ColumnGroups = Enum.Parse<ThreePGStandTrajectoryColumnGroups>(row.Row[this.Header.TrajectoryColumns]),
                CorrectSizeDistribution = Boolean.Parse(row.Row[this.Header.CorrectSizeDistribution]),
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
