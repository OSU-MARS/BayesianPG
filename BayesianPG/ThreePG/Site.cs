using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// site data: d_site in R, siteInputs in Fortran
    /// </summary>
    public class Site
    {
        /// <summary>
        /// name of site's climate
        /// </summary>
        public string Climate { get; init; }

        /// <summary>
        /// site latitude, ° (siteInputs[0])
        /// </summary>
        public float Lat { get; init; }

        /// <summary>
        /// altitude of the site location, m (siteInputs[1])
        /// </summary>
        public float altitude { get; init; }

        /// <summary>
        /// Soil class according to Table 2 of 3PGpjs user manual. 
        /// -1 - use SWconst0 and SWpower0
        ///  0 - no effect of available soil water on production 
        ///  1 - sand
        ///  2 - sandy loam
        ///  3 - clay loam
        ///  4 - clay
        ///         /// (siteInputs[2])
        /// </summary>
        public int soil_class { get; init; }

        /// <summary>
        /// available soil water, mm (siteInputs[3])
        /// </summary>
        public float aSW { get; set; }

        /// <summary>
        /// minimum available soil water, mm (siteInputs[4])
        /// </summary>
        public float asw_min { get; init; }

        /// <summary>
        /// maximum available soil water, mm (siteInputs[5])
        /// </summary>
        public float asw_max { get; init; }

        /// <summary>
        /// initial month and year when simulation starts (siteInputs[6] and [7]) 
        /// </summary>
        public DateTime From { get; init; }

        /// <summary>
        /// month and year when simulation ends
        /// </summary>
        public DateTime To { get; init; }

        public Site()
        {
            this.Climate = String.Empty;
            this.Lat = Single.MinValue;
            this.altitude = Single.MinValue;
            this.soil_class = -1;
            this.aSW = Single.MinValue;
            this.asw_min = Single.MinValue;
            this.asw_max = Single.MinValue;
            this.From = DateTime.MinValue;
            this.To = DateTime.MinValue;
        }

        public float[] GetMeanDayLengthInSecondsByMonth()
        {
            // Day - length calculations
            // local
            Span<float> sinDec = stackalloc float[12];
            Span<float> cosH0 = stackalloc float[12];
            float[] dayLength = new float[12];

            float SLAt = MathF.Sin(MathF.PI * Lat / 180.0F);
            float cLat = MathF.Cos(MathF.PI * Lat / 180.0F);
            for (int monthIndex = 0; monthIndex < sinDec.Length; ++monthIndex)
            {
                sinDec[monthIndex] = 0.4F * MathF.Sin(0.0172F * (Constant.dayOfYear[monthIndex] - 80.0F));
                cosH0[monthIndex] = -sinDec[monthIndex] * SLAt / (cLat * MathF.Sqrt(1.0F - sinDec[monthIndex] * sinDec[monthIndex]));

                dayLength[monthIndex] = MathF.Acos(cosH0[monthIndex]) / MathF.PI;

                if (cosH0[monthIndex] > 1.0F)
                {
                    dayLength[monthIndex] = 0.0F;
                }
                if (cosH0[monthIndex] < -1.0F)
                {
                    dayLength[monthIndex] = 1.0F;
                }

                dayLength[monthIndex] *= 86400.0F; // convert from days to seconds
            }

            return dayLength;
        }

        public float[] GetSolarAngleByMonth() // latitude in degrees
        {
            float firstXaxisIntercept = -0.0018F * Lat * Lat * Lat + 0.0021F * Lat * Lat - 2.3459F * Lat + 80.097F;
            float secondXaxisIntercept = 0.0018F * Lat * Lat * Lat - 0.0031F * Lat * Lat + 2.3826F * Lat + 266.62F;
            Span<float> gamma = stackalloc float[12];
            Span<float> declinationAngle = stackalloc float[12];
            Span<float> szaPrep = stackalloc float[12];
            Span<float> solarZenithAngle = stackalloc float[12];
            float[] solarangle = new float[12];
            for (int monthIndex = 0; monthIndex < 12; ++monthIndex)
            {
                gamma[monthIndex] = 2.0F * MathF.PI / 365.0F * (Constant.dayOfYear[monthIndex] - 1.0F);

                declinationAngle[monthIndex] = 0.006918F - (0.399912F * MathF.Cos(gamma[monthIndex])) + 0.070257F * MathF.Sin(gamma[monthIndex]) -
                   0.006758F * MathF.Cos(2.0F * gamma[monthIndex]) + 0.000907F * MathF.Sin(2.0F * gamma[monthIndex]) - 0.002697F * MathF.Cos(3.0F * gamma[monthIndex]) +
                   0.00148F * MathF.Sin(3.0F * gamma[monthIndex]);

                szaPrep[monthIndex] = MathF.Sin(MathF.PI / 180.0F * Lat * (-1.0F)) * MathF.Sin(declinationAngle[monthIndex]) +
                    MathF.Cos(-MathF.PI / 180.0F * Lat) * MathF.Cos(declinationAngle[monthIndex]);
                solarZenithAngle[monthIndex] = 180.0F / MathF.PI * (MathF.Atan(-szaPrep[monthIndex] / MathF.Sqrt(-szaPrep[monthIndex] * szaPrep[monthIndex] + 1.0F)) + 2.0F * MathF.Atan(1.0F));

                solarangle[monthIndex] = solarZenithAngle[monthIndex];

                if ((Lat >= 0.0F) && (Lat <= 23.4F))
                {
                    // the zenith angle only needs to be adjusted if the lat is between about - 23.4 and 23.4
                    if ((Constant.dayOfYear[monthIndex] > secondXaxisIntercept) || (Constant.dayOfYear[monthIndex] < firstXaxisIntercept))
                    {
                        solarangle[monthIndex] = -1.0F * solarZenithAngle[monthIndex];
                    }
                }

                if ((Lat >= -23.4F) && (Lat < 0.0F))
                {
                    // the zenith angle only needs to be adjusted if the lat is between about - 23.4 and 23.4
                    if ((Constant.dayOfYear[monthIndex] > firstXaxisIntercept) || (Constant.dayOfYear[monthIndex] < secondXaxisIntercept))
                    {
                        solarangle[monthIndex] = -1.0F * solarZenithAngle[monthIndex];
                    }
                }
            }

            return solarangle;
        }
    }
}
