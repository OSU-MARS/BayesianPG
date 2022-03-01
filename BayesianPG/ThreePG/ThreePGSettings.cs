namespace BayesianPG.ThreePG
{
    /// <summary>
    /// settings: settings list in R, settings in Fortran
    /// </summary>
    public class ThreePGSettings
    {
        /// <summary>
        /// Number of iterations to run each time bias correction occurs.
        /// </summary>
        public int BiasCorrectionIterations { get; init; }

        /// <summary>
        ///  (Fortran settings[5])
        /// </summary>
        public bool CalculateD13C { get; init; }

        public ThreePGStandTrajectoryColumnGroups ColumnGroups { get; init; }

        /// <summary>
        ///  (Fortran settings[4])
        /// </summary>
        public bool CorrectSizeDistribution { get; init; }

        /// <summary>
        ///  (Fortran settings[0])
        /// </summary>
        public ThreePGModel light_model { get; init; }
        /// <summary>
        ///  (Fortran settings[1])
        /// </summary>
        public ThreePGModel transp_model { get; init; }
        /// <summary>
        ///  (Fortran settings[2])
        /// </summary>
        public ThreePGModel phys_model { get; init; }
        /// <summary>
        ///  (Fortran settings[3])
        /// </summary>
        public ThreePGHeightModel height_model { get; init; }

        public bool management { get; init; }

        public ThreePGSettings()
        {
            this.BiasCorrectionIterations = 2;
            this.ColumnGroups = ThreePGStandTrajectoryColumnGroups.Core;
            this.CorrectSizeDistribution = true;
            this.CalculateD13C = false;
            this.height_model = ThreePGHeightModel.Power;
            this.light_model = ThreePGModel.Mix;
            this.phys_model = ThreePGModel.Mix;
            this.transp_model = ThreePGModel.Mix;

            this.management = true;
        }
    }
}
