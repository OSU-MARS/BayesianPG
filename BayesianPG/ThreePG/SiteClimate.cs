using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// climate: d_climate in R, forcingInputs in Fortran
    /// Vectors of monthly input of length n_m.
    /// </summary>
    public class SiteClimate
    {
        public int Capacity { get; private set; }
        public int n_m { get; set; }

        /// <summary>
        /// minimum daily temperature (forcingInputs[0])
        /// TODO: figure out why tmp_min is unused in Fortran
        /// </summary>
        // public float[] tmp_min { get; private set; }

        /// <summary>
        /// maximum daily temperature (forcingInputs[1])
        /// </summary>
        public float[] tmp_max { get; private set; }

        /// <summary>
        /// forcingInputs[2]
        /// </summary>
        public float[] tmp_ave { get; private set; }

        /// <summary>
        /// monthly precipitation sum (forcingInputs[3])
        /// </summary>
        public float[] prcp { get; private set; }

        /// <summary>
        /// mean daily incident solar radiation (forcingInputs[4])
        /// </summary>
        public float[] solar_rad { get; private set; }

        /// <summary>
        /// number of frost days per month (forcingInputs[5])
        /// </summary>
        public float[] frost_days { get; private set; }

        /// <summary>
        /// forcingInputs[6]
        /// </summary>
        public float[] vpd_day { get; private set; }

        /// <summary>
        /// atmospheric CO₂ (forcingInputs[7])
        /// </summary>
        public float[] co2 { get; private set; }

        /// <summary>
        /// added δ¹³C of atmospheric CO₂ (per mil) (forcingInputs[8])
        /// </summary>
        public float[] d13Catm { get; private set; }

        public SiteClimate()
        {
            this.Capacity = Constant.DefaultTimestepCapacity;
            this.n_m = 0;

            this.co2 = new float[this.Capacity];
            this.d13Catm = new float[this.Capacity];
            this.frost_days = new float[this.Capacity];
            this.prcp = new float[this.Capacity];
            this.solar_rad = new float[this.Capacity];
            this.tmp_ave = new float[this.Capacity];
            this.tmp_max = new float[this.Capacity];
            this.vpd_day = new float[this.Capacity];
        }

        public void AllocateDecade()
        {
            this.Capacity += 10 * 12;
            this.co2 = this.co2.Resize(this.Capacity);
            this.d13Catm = this.d13Catm.Resize(this.Capacity);
            this.frost_days = this.frost_days.Resize(this.Capacity);
            this.prcp = this.prcp.Resize(this.Capacity);
            this.solar_rad = this.solar_rad.Resize(this.Capacity);
            this.tmp_ave = this.tmp_ave.Resize(this.Capacity);
            this.tmp_max = this.tmp_max.Resize(this.Capacity);
            this.vpd_day = this.vpd_day.Resize(this.Capacity);
        }
    }
}
