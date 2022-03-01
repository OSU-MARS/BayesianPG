using BayesianPG.Extensions;
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
        public DateTime From { get; set; }
        public int MonthCount { get; set; }

        /// <summary>
        /// atmospheric CO₂ in ppm (Fortan forcingInputs[7])
        /// </summary>
        public float[] AtmosphericCO2 { get; private set; }

        /// <summary>
        /// added δ¹³C of atmospheric CO₂ per mil (Fortan forcingInputs[8])
        /// </summary>
        public float[] D13Catm { get; private set; }

        /// <summary>
        /// number of frost days per month (Fortan forcingInputs[5])
        /// </summary>
        public float[] FrostDays { get; private set; }

        /// <summary>
        /// mean daily incident solar radiation (Fortan forcingInputs[4])
        /// </summary>
        public float[] MeanDailySolarRadiation { get; private set; }

        /// <summary>
        /// monthly mean temperature (Fortran forcingInputs[2])
        /// </summary>
        public float[] MeanDailyTemp { get; private set; }

        /// <summary>
        /// monthly mean maximum daily temperature (Fortan forcingInputs[1])
        /// </summary>
        public float[] MeanDailyTempMax { get; private set; }

        /// <summary>
        /// monthly mean minimum daily temperature (Fortan forcingInputs[0])
        /// </summary>
        // public float[] MeanDailyTempMin { get; private set; }

        /// <summary>
        /// monthly mean vapor pressure deficit (Fortan forcingInputs[6])
        /// </summary>
        public float[] MeanDailyVpd { get; private set; }

        /// <summary>
        /// monthly precipitation sum (Fortan forcingInputs[3])
        /// </summary>
        public float[] TotalPrecipitation { get; private set; }

        public SiteClimate()
        {
            this.Capacity = Constant.DefaultTimestepCapacity;
            this.From = DateTime.MinValue;
            this.MonthCount = 0;

            this.AtmosphericCO2 = new float[this.Capacity];
            this.D13Catm = new float[this.Capacity];
            this.FrostDays = new float[this.Capacity];
            this.MeanDailySolarRadiation = new float[this.Capacity];
            this.MeanDailyTemp = new float[this.Capacity];
            this.MeanDailyTempMax = new float[this.Capacity];
            this.MeanDailyVpd = new float[this.Capacity];
            this.TotalPrecipitation = new float[this.Capacity];
        }

        public void AllocateDecade()
        {
            this.Capacity += 10 * 12;
            this.AtmosphericCO2 = this.AtmosphericCO2.Resize(this.Capacity);
            this.D13Catm = this.D13Catm.Resize(this.Capacity);
            this.FrostDays = this.FrostDays.Resize(this.Capacity);
            this.MeanDailySolarRadiation = this.MeanDailySolarRadiation.Resize(this.Capacity);
            this.MeanDailyTemp = this.MeanDailyTemp.Resize(this.Capacity);
            this.MeanDailyTempMax = this.MeanDailyTempMax.Resize(this.Capacity);
            this.MeanDailyVpd = this.MeanDailyVpd.Resize(this.Capacity);
            this.TotalPrecipitation = this.TotalPrecipitation.Resize(this.Capacity);
        }
    }
}
