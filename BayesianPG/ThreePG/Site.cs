using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// site data: d_site in R, siteInputs in Fortran
    /// </summary>
    public class Site
    {
        /// <summary>
        /// altitude of the site location, m (Fortan siteInputs[1])
        /// </summary>
        public float Altitude { get; init; }

        /// <summary>
        /// soil water available at start of simulation (<see cref="From"/> date), mm (Fortan siteInputs[3])
        /// </summary>
        public float AvailableSoilWaterInitial { get; set; }

        /// <summary>
        /// maximum available soil water, mm (Fortan siteInputs[5])
        /// </summary>
        public float AvailableSoilWaterMax { get; init; }

        /// <summary>
        /// minimum available soil water, mm (Fortan siteInputs[4])
        /// </summary>
        public float AvailableSoilWaterMin { get; init; }

        /// <summary>
        /// name of site's climate
        /// </summary>
        public string Climate { get; init; }

        /// <summary>
        /// initial month and year when simulation starts (Fortan siteInputs[6] and [7]) 
        /// </summary>
        public DateTime From { get; init; }

        /// <summary>
        /// site latitude, ° (Fortan siteInputs[0])
        /// </summary>
        public float Latitude { get; init; }

        /// <summary>
        /// Soil class according to Table 2 of 3PGpjs user manual. 
        /// -1 - use SWconst0 and SWpower0
        ///  0 - no effect of available soil water on production 
        ///  1 - sand
        ///  2 - sandy loam
        ///  3 - clay loam
        ///  4 - clay
        ///  (Fortan siteInputs[2])
        /// </summary>
        public float SoilClass { get; init; }

        /// <summary>
        /// month and year when simulation ends
        /// </summary>
        public DateTime To { get; init; }

        public Site()
        {
            this.Climate = String.Empty;
            this.Latitude = Single.MinValue;
            this.Altitude = Single.MinValue;
            this.SoilClass = -1;
            this.AvailableSoilWaterInitial = Single.MinValue;
            this.AvailableSoilWaterMin = Single.MinValue;
            this.AvailableSoilWaterMax = Single.MinValue;
            this.From = DateTime.MinValue;
            this.To = DateTime.MinValue;
        }

        public float[] GetMeanDayLengthInSecondsByMonth()
        {
            // day length calculations
            float[] dayLengthInSeconds = new float[12];

            float SLAt = MathF.Sin(MathF.PI * Latitude / 180.0F);
            float cLat = MathF.Cos(MathF.PI * Latitude / 180.0F);
            for (int monthIndex = 0; monthIndex < dayLengthInSeconds.Length; ++monthIndex)
            {
                int dayOfYear = Constant.DayOfYear[monthIndex];
                float sinDec = 0.4F * MathF.Sin(0.0172F * (dayOfYear - 80.0F));
                float cosH0 = -sinDec * SLAt / (cLat * MathF.Sqrt(1.0F - sinDec * sinDec));

                float meanDayLengthInDays;
                if (cosH0 > 1.0F)
                {
                    meanDayLengthInDays = 0.0F;
                }
                else if (cosH0 < -1.0F)
                {
                    meanDayLengthInDays = 1.0F;
                }
                else
                {
                    meanDayLengthInDays = MathF.Acos(cosH0) / MathF.PI;
                }

                dayLengthInSeconds[monthIndex] = 86400.0F * meanDayLengthInDays; // convert from days to seconds
            }

            return dayLengthInSeconds;
        }

        public float[] GetSolarAngleByMonth() // latitude in degrees
        {
            float firstXaxisIntercept = -0.0018F * this.Latitude * this.Latitude * this.Latitude + 0.0021F * this.Latitude * this.Latitude - 2.3459F * this.Latitude + 80.097F;
            float secondXaxisIntercept = 0.0018F * this.Latitude * this.Latitude * this.Latitude - 0.0031F * this.Latitude * this.Latitude + 2.3826F * this.Latitude + 266.62F;
            float[] solarAngle = new float[12];
            for (int monthIndex = 0; monthIndex < 12; ++monthIndex)
            {
                int dayOfYear = Constant.DayOfYear[monthIndex];
                float gamma = 2.0F * MathF.PI / 365.0F * (dayOfYear - 1.0F);

                float declinationAngle = 0.006918F - 
                   0.399912F * MathF.Cos(gamma) + 0.070257F * MathF.Sin(gamma) -
                   0.006758F * MathF.Cos(2.0F * gamma) + 0.000907F * MathF.Sin(2.0F * gamma) - 
                   0.002697F * MathF.Cos(3.0F * gamma) + 0.00148F * MathF.Sin(3.0F * gamma);

                float szaPrep = MathF.Sin(MathF.PI / 180.0F * this.Latitude * (-1.0F)) * MathF.Sin(declinationAngle) +
                    MathF.Cos(-MathF.PI / 180.0F * this.Latitude) * MathF.Cos(declinationAngle);

                float solarZenithAngle = 180.0F / MathF.PI * (MathF.Atan(-szaPrep / MathF.Sqrt(-szaPrep * szaPrep + 1.0F)) + 2.0F * MathF.Atan(1.0F));
                // latitude is between equator and Tropic of Cancer
                if ((this.Latitude >= 0.0F) && (this.Latitude <= 23.4F))
                {
                    // the zenith angle only needs to be adjusted if the lat is between about -23.4 and 23.4
                    if ((dayOfYear > secondXaxisIntercept) || (dayOfYear < firstXaxisIntercept))
                    {
                        solarZenithAngle = -1.0F * solarZenithAngle;
                    }
                }
                // latitude is between equator and Tropic of Capricorn
                if ((this.Latitude >= -23.4F) && (this.Latitude < 0.0F))
                {
                    // the zenith angle only needs to be adjusted if the lat is between about -23.4 and 23.4
                    if ((dayOfYear > firstXaxisIntercept) || (dayOfYear < secondXaxisIntercept))
                    {
                        solarZenithAngle = -1.0F * solarZenithAngle;
                    }
                }

                solarAngle[monthIndex] = solarZenithAngle;
            }

            return solarAngle;
        }
    }
}
