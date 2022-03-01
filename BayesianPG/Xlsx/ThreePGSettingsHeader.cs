using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class ThreePGSettingsHeader : IXlsxWorksheetHeader
    {
        public int TranspirationModel { get; private set; }
        public int CorrectSizeDistribution { get; private set; }
        public int HeightModel { get; private set; }
        public int CalculateD13C { get; private set; }
        public int LightModel { get; private set; }
        public int Site { get; private set; }
        public int PhysiologicalModel { get; private set; }
        public int Management { get; private set; }
        public int TrajectoryColumns { get; private set; }

        public ThreePGSettingsHeader()
        {
            this.TranspirationModel = -1;
            this.CorrectSizeDistribution = -1;
            this.CalculateD13C = -1;
            this.HeightModel = -1;
            this.LightModel = -1;
            this.Site = -1;
            this.PhysiologicalModel = -1;
            this.Management = -1;
            this.TrajectoryColumns = -1;
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
                    case "light_model":
                        this.LightModel = index;
                        break;
                    case "transp_model":
                        this.TranspirationModel = index;
                        break;
                    case "phys_model":
                        this.PhysiologicalModel = index;
                        break;
                    case "correct_sizeDist":
                        this.CorrectSizeDistribution = index;
                        break;
                    case "height_model":
                        this.HeightModel = index;
                        break;
                    case "calculate_d13c":
                        this.CalculateD13C = index;
                        break;
                    case "management":
                        this.Management = index;
                        break;
                    case "trajectory_columns":
                        this.TrajectoryColumns = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.TranspirationModel < 0)
            {
                throw new XmlException("Transpiration model column not found in settings header.");
            }
            if (this.CorrectSizeDistribution < 0)
            {
                throw new XmlException("Size correction column not found in settings header.");
            }
            if (this.CalculateD13C < 0)
            {
                throw new XmlException("d¹³C column not found in settings header.");
            }
            if (this.HeightModel < 0)
            {
                throw new XmlException("Height model column not found in settings header.");
            }
            if (this.LightModel < 0)
            {
                throw new XmlException("Light model column not found in settings header.");
            }
            if (this.Site < 0)
            {
                throw new XmlException("Site name column not found in settings header.");
            }
            if (this.PhysiologicalModel < 0)
            {
                throw new XmlException("Physiological model not found in settings header.");
            }
            if (this.Management < 0)
            {
                throw new XmlException("Management column not found in settings header.");
            }
            if (this.TrajectoryColumns < 0)
            {
                throw new XmlException("Column indicating stand trajectory column groups not found in settings header.");
            }
        }
    }
}
