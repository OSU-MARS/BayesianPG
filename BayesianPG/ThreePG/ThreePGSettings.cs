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
        public int b_n { get; init; }

        /// <summary>
        ///  (settings[0])
        /// </summary>
        public ThreePGModel light_model { get; init; }
        /// <summary>
        ///  (settings[1])
        /// </summary>
        public ThreePGModel transp_model { get; init; }
        /// <summary>
        ///  (settings[2])
        /// </summary>
        public ThreePGModel phys_model { get; init; }
        /// <summary>
        ///  (settings[3])
        /// </summary>
        public ThreePGHeightModel height_model { get; init; }
        /// <summary>
        ///  (settings[4])
        /// </summary>
        public bool correct_bias { get; init; }
        /// <summary>
        ///  (settings[5])
        /// </summary>
        public bool calculate_d13c { get; init; }

        public bool management { get; init; }

        public ThreePGSettings()
        {
            this.b_n = 2;
            this.correct_bias = true;
            this.calculate_d13c = false;
            this.height_model = ThreePGHeightModel.Power;
            this.light_model = ThreePGModel.Mix;
            this.phys_model = ThreePGModel.Mix;
            this.transp_model = ThreePGModel.Mix;

            this.management = true;
        }
    }
}
