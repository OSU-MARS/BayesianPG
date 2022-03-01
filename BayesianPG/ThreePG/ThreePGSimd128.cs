using BayesianPG.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.ThreePG
{
    public class ThreePGSimd128 : ThreePGpjsMix<Vector128<float>, Vector128<int>>
    {
        public ThreePGSimd128(Site site, SiteClimate climate, SiteTreeSpecies species, TreeSpeciesParameters parameters, TreeSpeciesManagement management, ThreePGSettings settings)
            : base(site, climate, species, parameters, management, settings)
        {
        }

        private void InitializeParametersWeatherAndFirstMonth()
        {
            // CO₂ modifier
            Span<float> fCalphax = stackalloc float[this.Species.n_sp];
            Span<float> fCg0 = stackalloc float[this.Species.n_sp];
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float fCalpha700 = this.Parameters.fCalpha700[speciesIndex];
                fCalphax[speciesIndex] = fCalpha700 / (2.0F - fCalpha700);

                float fCg700 = this.Parameters.fCg700[speciesIndex];
                fCg0[speciesIndex] = fCg700 / (2.0F * fCg700 - 1.0F);
            }

            // Temperature --------
            DateTime timestepEndDate = this.Site.From;
            this.Trajectory.MonthCount = this.Trajectory.Capacity;
            for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
            {
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    // calculate temperature response function to apply to alphaCx
                    float tmp_ave = this.Climate.MeanDailyTemp[timestepIndex];
                    float Tmin = this.Parameters.Tmin[speciesIndex];
                    float Topt = this.Parameters.Topt[speciesIndex];
                    float Tmax = this.Parameters.Tmax[speciesIndex];
                    float f_tmp;
                    if ((tmp_ave <= Tmin) || (tmp_ave >= Tmax))
                    {
                        f_tmp = 0.0F;
                    }
                    else
                    {
                        f_tmp = (tmp_ave - Tmin) / (Topt - Tmin) * MathF.Pow((Tmax - tmp_ave) / (Tmax - Topt),
                            (Tmax - Topt) / (Topt - Tmin));
                    }
                    this.Trajectory.Species.f_tmp[timestepIndex, speciesIndex] = AvxExtensions.BroadcastScalarToVector128(f_tmp);

                    // calculate temperature response function to apply to gc (uses mean of Tx and Tav instead of Tav, Feikema et al 2010)
                    float tmp_max = this.Climate.MeanDailyTempMax[timestepIndex];
                    float f_tmp_gc;
                    if (((tmp_ave + tmp_max) / 2 <= Tmin) || ((tmp_ave + tmp_max) / 2 >= Tmax))
                    {
                        f_tmp_gc = 0.0F;
                    }
                    else
                    {
                        f_tmp_gc = ((tmp_ave + tmp_max) / 2 - Tmin) / (Topt - Tmin) * MathF.Pow((Tmax - (tmp_ave + tmp_max) / 2) / (Tmax - Topt),
                            (Tmax - Topt) / (Topt - Tmin));
                    }
                    this.Trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex] = AvxExtensions.BroadcastScalarToVector128(f_tmp_gc);

                    // frost modifier
                    float kF = this.Parameters.kF[speciesIndex];
                    float frost_days = this.Climate.FrostDays[timestepIndex];
                    float daysInMonth = timestepEndDate.DaysInMonth();
                    // float f_frost = 1.0F - kF * (frost_days / 30.0F); // https://github.com/trotsiuk/r3PG/issues/68
                    float f_frost = 1.0F - kF * (frost_days / daysInMonth);
                    this.Trajectory.Species.f_frost[timestepIndex, speciesIndex] = AvxExtensions.BroadcastScalarToVector128(f_frost);

                    // CO₂ modifiers
                    float fCalpha = fCalphax[speciesIndex] * this.Climate.AtmosphericCO2[timestepIndex] / (350.0F * (fCalphax[speciesIndex] - 1.0F) + this.Climate.AtmosphericCO2[timestepIndex]);
                    this.Trajectory.Species.f_calpha[timestepIndex, speciesIndex] = AvxExtensions.BroadcastScalarToVector128(fCalpha);
                    float fCg = fCg0[speciesIndex] / (1.0F + (fCg0[speciesIndex] - 1.0F) * this.Climate.AtmosphericCO2[timestepIndex] / 350.0F);
                    this.Trajectory.Species.f_cg[timestepIndex, speciesIndex] = AvxExtensions.BroadcastScalarToVector128(fCg);

                    Debug.Assert((f_tmp >= 0.0F) && (f_tmp <= 1.0F) &&
                                 (f_tmp_gc >= 0.0F) && (f_tmp_gc <= 1.0F) &&
                                 (f_frost >= 0.0F) && (f_frost <= 1.0F) &&
                                 (fCalpha >= 0.0F) && (fCalpha <= 1.0F) &&
                                 (fCg >= 0.0F) && (fCg <= 1.0F));
                }
            }

            // air pressure
            this.State.airPressure = 101.3F * MathF.Exp(-1.0F * this.Site.Altitude / 8200.0F);

            // SOIL WATER --------
            // Assign the SWconst and SWpower parameters for this soil class
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                if (this.Site.SoilClass > 0.0F)
                {
                    // standard soil type
                    this.State.swConst[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(0.8F - 0.10F * this.Site.SoilClass);
                    this.State.swPower[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(11.0F - 2.0F * this.Site.SoilClass);
                }
                else if (this.Site.SoilClass < 0.0F)
                {
                    // use supplied parameters
                    this.State.swConst[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Parameters.SWconst0[speciesIndex]);
                    this.State.swPower[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Parameters.SWpower0[speciesIndex]);
                }
                else
                {
                    // no soil-water effects
                    this.State.swConst[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(999.0F);
                    this.State.swPower[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Parameters.SWpower0[speciesIndex]);
                }
            }

            // initial available soil water must be between min and max ASW
            float initialSoilWater = MathF.Max(MathF.Min(this.Site.AvailableSoilWaterInitial, this.Site.AvailableSoilWaterMax), this.Site.AvailableSoilWaterMin);
            this.State.aSW = AvxExtensions.BroadcastScalarToVector128(initialSoilWater);
            this.Trajectory.AvailableSoilWater[0] = this.State.aSW;

            // NUTRITIONS --------
            // Check fN(FR) for no effect: fNn = 0 ==> fN(FR) = 1 for all FR.
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                if (this.Parameters.fNn[speciesIndex] == 0.0F)
                {
                    this.Parameters.fN0[speciesIndex] = 1.0F; // TODO: move to input validation
                }

                // Partitioning  --------
                float pFS20 = this.Parameters.pFS20[speciesIndex];
                float pFS2 = this.Parameters.pFS2[speciesIndex];
                float pfsPower = MathF.Log(pFS20 / pFS2) / MathF.Log(20.0F / 2.0F);
                this.State.pfsPower[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(pfsPower);

                this.State.pfsConst[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(pFS2 / MathF.Pow(2.0F, pfsPower));
            }

            // INITIALISATION (Age dependent)---------------------
            // Calculate species specific modifiers
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float plantingAgeInMonths = 12.0F * (this.Site.From.Year - this.Species.YearPlanted[speciesIndex]) + this.Site.From.Month - this.Species.MonthPlanted[speciesIndex] - 1.0F;
                float[] age = new float[this.Trajectory.MonthCount];
                float[] age_m = new float[this.Trajectory.MonthCount];
                for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
                {
                    float ageInYears = (plantingAgeInMonths + timestepIndex + 1) / 12.0F;
                    age[timestepIndex] = ageInYears;
                    age_m[timestepIndex] = ageInYears - 1.0F / 12.0F;
                }
                age_m[0] = age[0];

                this.Trajectory.Species.age[speciesIndex] = age;
                this.Trajectory.Species.age_m[speciesIndex] = age_m;

                Vector128<float> sla0 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.SLA0[speciesIndex]);
                Vector128<float> sla1 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.SLA1[speciesIndex]);
                Vector128<float> tsla = AvxExtensions.BroadcastScalarToVector128(this.Parameters.tSLA[speciesIndex]);
                this.Trajectory.Species.SLA[speciesIndex] = ThreePGSimd128.GetAgeDependentParameter(age_m, sla0, sla1, tsla, AvxExtensions.BroadcastScalarToVector128(2.0F));

                Vector128<float> fracBB0 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.fracBB0[speciesIndex]);
                Vector128<float> fracBB1 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.fracBB1[speciesIndex]);
                Vector128<float> tBB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.tBB[speciesIndex]);
                this.Trajectory.Species.fracBB[speciesIndex] = ThreePGSimd128.GetAgeDependentParameter(age_m, fracBB0, fracBB1, tBB, AvxExtensions.BroadcastScalarToVector128(1.0F));

                Vector128<float> rhoMin = AvxExtensions.BroadcastScalarToVector128(this.Parameters.rhoMin[speciesIndex]);
                Vector128<float> rhoMax = AvxExtensions.BroadcastScalarToVector128(this.Parameters.rhoMax[speciesIndex]);
                Vector128<float> tRho = AvxExtensions.BroadcastScalarToVector128(this.Parameters.tRho[speciesIndex]);
                this.Trajectory.Species.wood_density[speciesIndex] = ThreePGSimd128.GetAgeDependentParameter(age_m, rhoMin, rhoMax, tRho, AvxExtensions.BroadcastScalarToVector128(1.0F));

                Vector128<float> gammaN0 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.gammaN0[speciesIndex]);
                Vector128<float> gammaN1 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.gammaN1[speciesIndex]);
                Vector128<float> tgammaN = AvxExtensions.BroadcastScalarToVector128(this.Parameters.tgammaN[speciesIndex]);
                Vector128<float> ngammaN = AvxExtensions.BroadcastScalarToVector128(this.Parameters.ngammaN[speciesIndex]);
                this.Trajectory.Species.gammaN[speciesIndex] = ThreePGSimd128.GetAgeDependentParameter(age, gammaN0, gammaN1, tgammaN, ngammaN); // age instead of age_m (per Fortran)

                Vector128<float> gammaF1 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.gammaF1[speciesIndex]);
                Vector128<float> gammaF0 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.gammaF0[speciesIndex]);
                Vector128<float> tgammaF = AvxExtensions.BroadcastScalarToVector128(this.Parameters.tgammaF[speciesIndex]);
                this.Trajectory.Species.gammaF[speciesIndex] = ThreePGSimd128.GetLitterfallRate(age_m, gammaF1, gammaF0, tgammaF);

                // age modifier
                if (this.Parameters.nAge[speciesIndex] == 0.0F)
                {
                    for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
                    {
                        this.Trajectory.Species.f_age[timestepIndex, speciesIndex] = AvxExtensions.BroadcastScalarToVector128(1.0F);
                    }
                }
                else
                {
                    Vector128<float> MaxAge = AvxExtensions.BroadcastScalarToVector128(this.Parameters.MaxAge[speciesIndex]);
                    Vector128<float> rAge = AvxExtensions.BroadcastScalarToVector128(this.Parameters.rAge[speciesIndex]);
                    Vector128<float> rAgeMaxAge = Avx.Multiply(rAge, MaxAge);
                    Vector128<float> nAge = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nAge[speciesIndex]);
                    Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                    for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
                    {
                        this.Trajectory.Species.f_age[timestepIndex, speciesIndex] = Avx.Divide(one, Avx.Add(one, MathV.Pow(Avx.Divide(AvxExtensions.BroadcastScalarToVector128(age_m[timestepIndex]), rAgeMaxAge), nAge)));
                    }
                }
            }

            // INITIALISATION (Stand)---------------------
            Vector128<float> zero = Vector128<float>.Zero;
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                Vector128<float> initialStemsPerHa;
                Vector128<float> initialStemBiomass;
                Vector128<float> initialFoliageBiomass;
                Vector128<float> initialRootBiomass;
                float age = this.Trajectory.Species.age[speciesIndex][0];
                if (age >= 0.0F)
                {
                    initialStemsPerHa = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialStemsPerHectare[speciesIndex]);
                    initialStemBiomass = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialStemBiomass[speciesIndex]);
                    initialFoliageBiomass = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialFoliageBiomass[speciesIndex]);
                    initialRootBiomass = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialRootBiomass[speciesIndex]);
                }
                else
                {
                    initialStemsPerHa = zero;
                    initialStemBiomass = zero;
                    initialFoliageBiomass = zero;
                    initialRootBiomass = zero;
                }

                this.State.stems_n[speciesIndex] = initialStemsPerHa;
                this.State.biom_stem[speciesIndex] = initialStemBiomass;
                this.State.biom_foliage[speciesIndex] = initialFoliageBiomass;
                this.State.biom_root[speciesIndex] = initialRootBiomass;
            }

            // check if this is the dormant period or previous/following period is dormant
            // to allocate foliage if needed, etc.
            Vector128<float> competition_total = Vector128<float>.Zero;
            int monthOfYear = this.Site.From.Month;
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                // if this is a dormant month
                if (this.IsDormant(monthOfYear, speciesIndex))
                {
                    this.State.biom_foliage_debt[speciesIndex] = this.State.biom_foliage[speciesIndex];
                    this.State.biom_foliage[speciesIndex] = Vector128<float>.Zero;
                }

                // initial stand characteristics
                float age = this.Trajectory.Species.age[speciesIndex][0];

                Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                Vector128<float> meanStemBiomassInKg = zero;
                if (age >= 0.0F)
                {
                    Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                    meanStemBiomassInKg = Avx.Divide(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(1000.0F), biom_stem), stems_n);
                }
                this.State.biom_tree[speciesIndex] = meanStemBiomassInKg;

                Vector128<float> basalAreaPerHa; // m²/ha
                Vector128<float> lai;
                Vector128<float> meanDbh;
                if (age >= 0.0F)
                {
                    Vector128<float> sla = this.Trajectory.Species.SLA[speciesIndex][0];
                    Vector128<float> biom_foliage = this.State.biom_foliage[speciesIndex];
                    lai = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.1F), Avx.Multiply(biom_foliage, sla));

                    Vector128<float> aWS = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aWS[speciesIndex]);
                    Vector128<float> nWSreciprocal = AvxExtensions.BroadcastScalarToVector128(1.0F / this.Parameters.nWS[speciesIndex]);
                    meanDbh = MathV.Pow(Avx.Divide(meanStemBiomassInKg, aWS), nWSreciprocal);

                    basalAreaPerHa = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathF.PI * 0.0001F / 4.0F), Avx.Multiply(Avx.Multiply(meanDbh, meanDbh), stems_n));
                }
                else
                {
                    lai = zero;
                    meanDbh = zero;
                    basalAreaPerHa = zero;
                }
                this.State.lai[speciesIndex] = lai;
                this.State.dbh[speciesIndex] = meanDbh;
                this.State.basal_area[speciesIndex] = basalAreaPerHa;

                DebugV.Assert(Avx.Add(Avx.CompareGreaterThanOrEqual(meanStemBiomassInKg, Vector128<float>.Zero), Avx.And(Avx.CompareGreaterThanOrEqual(meanDbh, Vector128<float>.Zero), Avx.CompareGreaterThanOrEqual(basalAreaPerHa, Vector128<float>.Zero))));

                Vector128<float> wood_density = this.Trajectory.Species.wood_density[speciesIndex][0];
                Vector128<float> basal_area = this.State.basal_area[speciesIndex];
                competition_total = Avx.Add(competition_total, Avx.Multiply(wood_density, basal_area));
            }
            this.State.competition_total = competition_total;

            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                Vector128<float> aH = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aH[speciesIndex]);
                Vector128<float> nHB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHB[speciesIndex]);
                Vector128<float> nHC = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHC[speciesIndex]);
                Vector128<float> dbh = this.State.dbh[speciesIndex];
                Vector128<float> height = this.Settings.height_model switch
                {
                    ThreePGHeightModel.Power => Avx.Multiply(aH, Avx.Multiply(MathV.Pow(dbh, nHB), MathV.Pow(competition_total, nHC))),
                    ThreePGHeightModel.Exponent => Avx.Add(AvxExtensions.BroadcastScalarToVector128(1.3F), Avx.Add(Avx.Multiply(aH, MathV.Exp(Avx.Divide(Avx.Subtract(Vector128<float>.Zero, nHB), dbh))), Avx.Multiply(nHC, Avx.Multiply(competition_total, dbh)))),
                    _ => throw new NotSupportedException("Unhandled height model " + this.Settings.height_model + ".")
                };
                this.State.height[speciesIndex] = height;
            }

            // correct the bias
            this.CorrectSizeDistribution(timestep: 0, Constant.Simd128x4.MaskAllTrue);

            Vector128<float> height_max = AvxExtensions.BroadcastScalarToVector128(Single.MinValue);
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                Vector128<float> lai = this.State.lai[speciesIndex];
                int laiNonzeroMask = Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                Vector128<float> height = this.State.height[speciesIndex];
                int heightMask = Avx.MoveMask(Avx.CompareGreaterThan(height, height_max));
                byte combinedMask = (byte)(laiNonzeroMask & heightMask);
                if (combinedMask != Constant.Simd128x4.MaskAllFalse)
                {
                    height_max = Avx.Blend(height_max, height, combinedMask);
                }
            }

            // volume and volume increment
            // Call main function to get volume and then fix up cumulative volume and MAI.
            this.GetVolumeAndIncrement(timestep: 0);
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                Vector128<float> initialVolume = this.State.volume[speciesIndex];
                this.State.volume_cum[speciesIndex] = initialVolume;

                Vector128<float> volume_mai = zero;
                float age = this.Trajectory.Species.age[speciesIndex][0];
                if (age > 0.0F)
                {
                    volume_mai = Avx.Divide(initialVolume, AvxExtensions.BroadcastScalarToVector128(age));
                }
                else
                {
                    volume_mai = zero;
                }
                this.State.volume_mai[speciesIndex] = volume_mai;
            }

            // capture initial month's state into stand trajectory
            this.Trajectory.Species.SetMonth(0, this.State);
        }

        public override void PredictStandTrajectory()
        {
            // *************************************************************************************
            // INITIALISATION (Age independent)
            this.InitializeParametersWeatherAndFirstMonth();

            // *************************************************************************************
            // monthly timesteps
            DateTime timestepEndDate = this.Site.From;
            for (int timestep = 1; timestep < this.Trajectory.MonthCount; ++timestep) // first month is initial month and set up above
            {
                // move to next month
                timestepEndDate = timestepEndDate.AddMonths(1);

                // add any new cohorts ----------------------------------------------------------------------
                byte correctSizeDistributionMask = Constant.Simd128x4.MaskAllFalse;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float age = this.Trajectory.Species.age[speciesIndex][timestep];
                    if (age == 0.0F)
                    {
                        this.State.stems_n[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialStemsPerHectare[speciesIndex]);
                        this.State.biom_stem[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialStemBiomass[speciesIndex]);
                        this.State.biom_foliage[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialFoliageBiomass[speciesIndex]);
                        this.State.biom_root[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Species.InitialRootBiomass[speciesIndex]);
                        correctSizeDistributionMask = Constant.Simd128x4.MaskAllTrue;
                    }
                }

                // Test for deciduous leaf off ----------------------------------------------------------------------
                // If this is first month after dormancy we need to make potential LAI, so the
                // PAR absorbption can be applied, otherwise it will be zero.
                // In the end of the month we will re-calculate it based on the actual values.
                int monthOfYear = timestepEndDate.Month;
                Vector128<float> zero = Vector128<float>.Zero;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    if (this.IsDormant(monthOfYear, speciesIndex) == false)
                    {
                        if (this.IsDormant(monthOfYear - 1, speciesIndex) == true)
                        {
                            Vector128<float> sla = this.Trajectory.Species.SLA[speciesIndex][timestep];
                            Vector128<float> biom_foliage_debt = this.State.biom_foliage_debt[speciesIndex];
                            this.State.lai[speciesIndex] = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.1F), Avx.Multiply(biom_foliage_debt, sla));
                            correctSizeDistributionMask = Constant.Simd128x4.MaskAllTrue;
                        }
                    }

                    // if this is first dormant month, then set WF to 0 and move everything to the debt
                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                    {
                        if (this.IsDormant(monthOfYear - 1, speciesIndex) == false)
                        {
                            this.State.biom_foliage_debt[speciesIndex] = this.State.biom_foliage[speciesIndex];
                            this.State.biom_foliage[speciesIndex] = zero;
                            this.State.lai[speciesIndex] = zero;
                            correctSizeDistributionMask = Constant.Simd128x4.MaskAllTrue;
                        }
                    }
                }

                if (correctSizeDistributionMask != Constant.Simd128x4.MaskAllFalse)
                {
                    this.CorrectSizeDistribution(timestep, correctSizeDistributionMask);
                    correctSizeDistributionMask = Constant.Simd128x4.MaskAllFalse;
                }

                // Radiation and assimilation ----------------------------------------------------------------------
                if (this.Settings.light_model == ThreePGModel.Pjs27)
                {
                    this.Light3PGpjs(timestep, timestepEndDate);
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        this.State.VPD_sp[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(this.Climate.MeanDailyVpd[timestep]);
                    }
                }
                else if (this.Settings.light_model == ThreePGModel.Mix)
                {
                    // Calculate the absorbed PAR.If this is first month, then it will be only potential
                    this.Light3PGmix(timestep, timestepEndDate);
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        Vector128<float> minusLn2laiAbove = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(-Constant.Ln2), this.State.lai_above[speciesIndex]);
                        Vector128<float> cVPD = AvxExtensions.BroadcastScalarToVector128(this.Parameters.cVPD[speciesIndex]);
                        Vector128<float> meanDailyVpd = AvxExtensions.BroadcastScalarToVector128(this.Climate.MeanDailyVpd[timestep]);
                        this.State.VPD_sp[speciesIndex] = Avx.Multiply(meanDailyVpd, MathV.Exp(Avx.Divide(minusLn2laiAbove, cVPD)));
                    }
                }
                else
                {
                    throw new NotSupportedException("Unhandled light model " + this.Settings.light_model + ".");
                }

                // determine various environmental modifiers which were not calculated before
                // calculate VPD modifier
                // Get within-canopy climatic conditions this is exponential function
                Vector128<float> height_max = AvxExtensions.BroadcastScalarToVector128(Single.MinValue);
                Vector128<float> lai_total = Vector128<float>.Zero;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    Vector128<float> lai = this.State.lai[speciesIndex];
                    byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                    if (laiNonzeroMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> height = this.State.height[speciesIndex];
                        height_max = Avx.Blend(height_max, Avx.Max(height_max, height), laiNonzeroMask);
                        lai_total = Avx.Add(lai_total, lai);

                        DebugV.Assert(Avx.CompareGreaterThanOrEqual(lai, zero)); // a negative LAI would reduce lai_total
                    }
                }

                Vector128<float> availableSoilWaterMax = AvxExtensions.BroadcastScalarToVector128(this.Site.AvailableSoilWaterMax);
                Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    Vector128<float> aero_resist = zero;

                    Vector128<float> lai = this.State.lai[speciesIndex];
                    byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                    if (laiNonzeroMask != Constant.Simd128x4.MaskAllFalse) // check for dormancy
                    {
                        Vector128<float> blCondReciprocal = AvxExtensions.BroadcastScalarToVector128(1.0F / this.Parameters.BLcond[speciesIndex]);
                        Vector128<float> leafOnAeroResist = blCondReciprocal; // if this is the (currently) tallest species

                        Vector128<float> height = this.State.height[speciesIndex];
                        byte maxHeightMask = (byte)Avx.MoveMask(Avx.CompareEqual(height, height_max));
                        if (maxHeightMask != Constant.Simd128x4.MaskAllTrue)
                        {
                            Vector128<float> twiceRelativeHeight = Avx.Divide(height, Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.5F), height_max));
                            Vector128<float> exponent = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(-Constant.Ln2), Avx.Multiply(twiceRelativeHeight, twiceRelativeHeight));
                            leafOnAeroResist = Avx.Add(aero_resist, Avx.Multiply(Avx.Subtract(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(5.0F), lai_total), blCondReciprocal), MathV.Exp(exponent)));
                        }

                        aero_resist = Avx.Blend(aero_resist, leafOnAeroResist, laiNonzeroMask);
                    }
                    this.State.aero_resist[speciesIndex] = aero_resist;

                    Vector128<float> minusCoeffCond = AvxExtensions.BroadcastScalarToVector128( -this.Parameters.CoeffCond[speciesIndex]);
                    Vector128<float> VPD_sp = this.State.VPD_sp[speciesIndex];
                    Vector128<float> f_vpd = MathV.Exp(Avx.Multiply(minusCoeffCond, VPD_sp));
                    this.State.f_vpd[speciesIndex] = f_vpd;

                    // soil water modifier
                    Vector128<float> swConst = this.State.swConst[speciesIndex];
                    Vector128<float> swPower = this.State.swPower[speciesIndex];
                    Vector128<float> f_sw = Avx.Divide(one, Avx.Add(one, MathV.Pow(Avx.Divide(Avx.Subtract(one, Avx.Divide(this.State.aSW, availableSoilWaterMax)), swConst), swPower)));
                    this.State.f_sw[speciesIndex] = f_sw;

                    // soil nutrition modifier
                    Vector128<float> f_nutr;
                    if (this.Parameters.fNn[speciesIndex] == 0.0F)
                    {
                        f_nutr = one;
                    }
                    else
                    {
                        Vector128<float> fN0 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.fN0[speciesIndex]);
                        Vector128<float> fNn = AvxExtensions.BroadcastScalarToVector128(this.Parameters.fNn[speciesIndex]);
                        Vector128<float> fertility = AvxExtensions.BroadcastScalarToVector128(this.Species.SoilFertility[speciesIndex]);
                        f_nutr = Avx.Subtract(one, Avx.Multiply(Avx.Subtract(one, fN0), MathV.Pow(Avx.Subtract(one, fertility), fNn)));
                    }
                    this.State.f_nutr[speciesIndex] = f_nutr;

                    // calculate physiological modifier applied to conductance and alphaCx.
                    Vector128<float> f_age = this.Trajectory.Species.f_age[timestep, speciesIndex];
                    Vector128<float> f_phys;
                    if (this.Settings.phys_model == ThreePGModel.Pjs27)
                    {
                        f_phys = Avx.Multiply(Avx.Min(f_vpd, f_sw), f_age);
                        this.Trajectory.Species.f_tmp_gc[timestep, speciesIndex] = one;
                    }
                    else if (this.Settings.phys_model == ThreePGModel.Mix)
                    {
                        f_phys = Avx.Multiply(f_vpd, Avx.Multiply(f_sw, f_age));
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled model " + this.Settings.phys_model + ".");
                    }
                    this.State.f_phys[speciesIndex] = f_phys;

                    DebugV.Assert(Avx.And(Avx.And(Avx.And(Avx.CompareGreaterThanOrEqual(f_vpd, zero), Avx.CompareLessThanOrEqual(f_vpd, one)),
                                                  Avx.And(Avx.CompareGreaterThanOrEqual(f_sw, zero), Avx.CompareLessThanOrEqual(f_sw, one))),
                                          Avx.And(Avx.And(Avx.CompareGreaterThanOrEqual(f_nutr, zero), Avx.CompareLessThanOrEqual(f_nutr, one)),
                                                  Avx.And(Avx.CompareGreaterThanOrEqual(f_phys, zero), Avx.CompareLessThanOrEqual(f_phys, one)))));

                    // calculate assimilation before the water balance is done
                    Vector128<float> alphaC = zero;
                    lai = this.State.lai[speciesIndex];
                    laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                    if (laiNonzeroMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> alphaCx = AvxExtensions.BroadcastScalarToVector128(this.Parameters.alphaCx[speciesIndex]);
                        Vector128<float> f_tmp = this.Trajectory.Species.f_tmp[timestep, speciesIndex];
                        Vector128<float> f_frost = this.Trajectory.Species.f_frost[timestep, speciesIndex];
                        Vector128<float> f_calpha = this.Trajectory.Species.f_calpha[timestep, speciesIndex];
                        alphaC = Avx.Blend(alphaC, Avx.Multiply(alphaCx, Avx.Multiply(f_nutr, Avx.Multiply(f_tmp, Avx.Multiply(f_frost, Avx.Multiply(f_calpha, f_phys))))), laiNonzeroMask);
                    }
                    this.State.alpha_c[speciesIndex] = alphaC;

                    Vector128<float> gDM_mol = AvxExtensions.BroadcastScalarToVector128(this.Parameters.gDM_mol[speciesIndex]);
                    Vector128<float> molPAR_MJ = AvxExtensions.BroadcastScalarToVector128(this.Parameters.molPAR_MJ[speciesIndex]);
                    Vector128<float> epsilon = Avx.Multiply(Avx.Multiply(gDM_mol, molPAR_MJ), alphaC);
                    this.State.epsilon[speciesIndex] = epsilon;

                    Vector128<float> apar = this.State.apar[speciesIndex];
                    Vector128<float> gpp = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.01F), Avx.Multiply(epsilon, apar)); // tDM / ha(apar is MJ / m ^ 2);
                    this.State.GPP[speciesIndex] = gpp;
                    Vector128<float> Y = AvxExtensions.BroadcastScalarToVector128(this.Parameters.Y[speciesIndex]); // assumes respiratory rate is constant
                    this.State.NPP[speciesIndex] = Avx.Multiply(gpp, Y);

                    // Water Balance ----------------------------------------------------------------------
                    // Calculate each species' proportion.
                    Vector128<float> lai_per = zero;
                    byte laiTotalMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai_total, zero));
                    if (laiTotalMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        lai = this.State.lai[speciesIndex];
                        lai_per = Avx.Blend(lai_per, Avx.Divide(lai, lai_total), laiTotalMask);
                    }
                    this.State.lai_per[speciesIndex] = lai_per;

                    // calculate conductance
                    Vector128<float> gC = AvxExtensions.BroadcastScalarToVector128(this.Parameters.MaxCond[speciesIndex]);
                    Vector128<float> laiGcx = AvxExtensions.BroadcastScalarToVector128(this.Parameters.LAIgcx[speciesIndex]);
                    byte laiGcxMask = (byte)Avx.MoveMask(Avx.CompareLessThanOrEqual(lai_total, laiGcx));
                    if (laiGcxMask != Constant.Simd128x4.MaskAllFalse) // TODO: single species case?
                    {
                        Vector128<float> MinCond = AvxExtensions.BroadcastScalarToVector128(this.Parameters.MinCond[speciesIndex]);
                        Vector128<float> MaxCond = AvxExtensions.BroadcastScalarToVector128(this.Parameters.MaxCond[speciesIndex]);
                        gC = Avx.Add(MinCond, Avx.Multiply(Avx.Subtract(MaxCond, MinCond), Avx.Divide(lai_total, laiGcx)));
                    }
                    this.State.gC[speciesIndex] = gC;

                    //float f_phys = this.state.f_phys[speciesIndex];
                    Vector128<float> f_tmp_gc = this.Trajectory.Species.f_tmp_gc[timestep, speciesIndex];
                    Vector128<float> f_cg = this.Trajectory.Species.f_cg[timestep, speciesIndex];
                    this.State.conduct_canopy[speciesIndex] = Avx.Multiply(gC, Avx.Multiply(lai_per, Avx.Multiply(f_phys, Avx.Multiply(f_tmp_gc, f_cg))));
                }
                Vector128<float> conduct_soil = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(Constant.MaxSoilCond / this.Site.AvailableSoilWaterMax), this.State.aSW);
                this.Trajectory.conduct_soil[timestep] = conduct_soil;

                // calculate transpiration
                Vector128<float> evapotra_soil;
                if (this.Settings.transp_model == ThreePGModel.Pjs27)
                {
                    this.Transpiration3PGpjs(timestep, timestepEndDate);
                    evapotra_soil = zero;
                }
                else if (this.Settings.transp_model == ThreePGModel.Mix)
                {
                    evapotra_soil = this.Transpiration3PGmix(timestep, timestepEndDate, conduct_soil);
                }
                else
                {
                    throw new NotSupportedException("Unhandled model " + this.Settings.transp_model + ".");
                }

                Vector128<float> transp_total = Avx.Add(this.State.transp_veg.Sum(), evapotra_soil);

                this.Trajectory.evapotra_soil[timestep] = evapotra_soil;

                // rainfall interception
                Vector128<float> prcp_interc_total = zero;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    Vector128<float> maxIntcptn = AvxExtensions.BroadcastScalarToVector128(this.Parameters.MaxIntcptn[speciesIndex]);
                    Vector128<float> prcp_interc_fract = maxIntcptn;
                    float laiMaxIntcptn = this.Parameters.LAImaxIntcptn[speciesIndex];
                    if (laiMaxIntcptn > 0.0F)
                    {
                        Vector128<float> lai_per = this.State.lai_per[speciesIndex];
                        prcp_interc_fract = Avx.Multiply(maxIntcptn, Avx.Multiply(Avx.Min(one, Avx.Divide(lai_total, AvxExtensions.BroadcastScalarToVector128(laiMaxIntcptn))), lai_per));
                    }

                    Vector128<float> totalPrecipitation = AvxExtensions.BroadcastScalarToVector128(this.Climate.TotalPrecipitation[timestep]);
                    Vector128<float> prcp_interc = Avx.Multiply(totalPrecipitation, prcp_interc_fract);
                    prcp_interc_total = Avx.Add(prcp_interc_total, prcp_interc);

                    this.Trajectory.Species.prcp_interc[timestep, speciesIndex] = prcp_interc;
                }

                // soil water balance
                float irrigation = 0.0F; // standing monthly irrigation, need to constrain irrigation only to the growing season.
                float water_runoff_pooled = 0.0F; // pooling and ponding not currently supported
                float poolFractn = MathF.Max(0.0F, MathF.Min(1.0F, 0.0F)); // determines fraction of excess water that remains on site
                Vector128<float> aSW = Avx.Add(this.State.aSW, AvxExtensions.BroadcastScalarToVector128(this.Climate.TotalPrecipitation[timestep] + (100.0F * irrigation / 12.0F) + water_runoff_pooled));
                Vector128<float> evapo_transp = Avx.Min(aSW, Avx.Add(transp_total, prcp_interc_total)); // ET can not exceed ASW
                Vector128<float> excessSW = Avx.Max(Avx.AddSubtract(aSW, Avx.Add(evapo_transp, availableSoilWaterMax)), zero);
                aSW = Avx.Subtract(aSW, Avx.Add(evapo_transp, excessSW));
                // water_runoff_pooled = poolFractn * excessSW;

                Vector128<float> availableSoilWaterMin = AvxExtensions.BroadcastScalarToVector128(this.Site.AvailableSoilWaterMin);
                Vector128<float> irrig_supl = zero;
                byte irrigationMask = (byte)Avx.MoveMask(Avx.CompareLessThan(aSW, availableSoilWaterMin));
                if (irrigationMask != Constant.Simd128x4.MaskAllFalse)
                {
                    irrig_supl = Avx.Subtract(availableSoilWaterMin, aSW);
                    aSW = availableSoilWaterMin;
                }
                this.Trajectory.irrig_supl[timestep] = irrig_supl;
                this.Trajectory.prcp_runoff[timestep] = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(1.0F - poolFractn), excessSW);

                DebugV.Assert(Avx.And(Avx.And(Avx.And(Avx.CompareGreaterThanOrEqual(aSW, availableSoilWaterMin), Avx.CompareLessThanOrEqual(aSW, availableSoilWaterMax)),
                                              Avx.And(Avx.CompareGreaterThan(evapo_transp, AvxExtensions.BroadcastScalarToVector128(-1.0F)), Avx.CompareGreaterThanOrEqual(excessSW, zero))),
                                      Avx.And(Avx.And(Avx.CompareGreaterThanOrEqual(prcp_interc_total, zero), Avx.CompareGreaterThan(transp_total, AvxExtensions.BroadcastScalarToVector128(-7.5F))),
                                              Avx.CompareGreaterThanOrEqual(AvxExtensions.BroadcastScalarToVector128(water_runoff_pooled), zero))));
                this.State.aSW = aSW;
                this.Trajectory.AvailableSoilWater[timestep] = aSW;
                this.Trajectory.evapo_transp[timestep] = evapo_transp;

                Vector128<float> transpirationScaleDenominator = Avx.Add(transp_total, prcp_interc_total);
                byte nonzeroWaterDemandMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(transpirationScaleDenominator, zero));
                Vector128<float> f_transp_scale; // scales NPP and GPP
                if (nonzeroWaterDemandMask != Constant.Simd128x4.MaskAllFalse)
                {
                    f_transp_scale = Avx.Blend(one, Avx.Divide(evapo_transp, transpirationScaleDenominator), nonzeroWaterDemandMask);
                }
                else
                {
                    // this might be close to 0 if the only existing species is dormant during this month
                    // (it will include the soil evaporation if Apply3PGpjswaterbalance = no)
                    f_transp_scale = one;
                }
                this.Trajectory.f_transp_scale[timestep] = f_transp_scale;

                // correct for actual ET
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    Vector128<float> gpp = this.State.GPP[speciesIndex];
                    gpp = Avx.Multiply(gpp, f_transp_scale);
                    this.State.GPP[speciesIndex] = gpp;

                    Vector128<float> npp = this.State.NPP[speciesIndex];
                    npp = Avx.Multiply(f_transp_scale, npp);
                    this.State.NPP[speciesIndex] = npp;
                    this.State.NPP_f[speciesIndex] = npp;

                    int transpirationMask = Avx.MoveMask(Avx.CompareGreaterThan(transp_total, zero));
                    int scaleMask = Avx.MoveMask(Avx.CompareGreaterThan(transp_total, zero));
                    byte combinedMask = (byte)(transpirationMask & scaleMask);
                    if (combinedMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        // a different scaler is required for transpiration because all of the scaling needs
                        // to be done to the transpiration and not to the RainIntcpth, which occurs regardless of the growth
                        Vector128<float> multiplier = Avx.Divide(Avx.Subtract(evapo_transp, prcp_interc_total), transp_total);

                        Vector128<float> transpVeg = this.State.transp_veg[speciesIndex];
                        transpVeg = Avx.Multiply(transpVeg, multiplier);
                        this.State.transp_veg[speciesIndex] = transpVeg;

                        evapotra_soil = Avx.Multiply(evapotra_soil, multiplier);
                    }

                    // NEED TO CROSS CHECK THIS PART, DON'T FULLY AGREE WITH IT
                    Vector128<float> wue = zero;
                    if (this.Species.n_sp == 1)
                    {
                        byte evapotranspirationMask = (byte)Avx.MoveMask(Avx.CompareNotEqual(evapo_transp, zero));
                        if (evapotranspirationMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            // in case ET is zero
                            // Also, for mixtures it is not possible to calculate WUE based on ET because the soil
                            // evaporation cannot simply be divided between species.
                            wue = Avx.Blend(wue, Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(100.0F), Avx.Divide(npp, evapo_transp)), evapotranspirationMask);
                        }
                    }
                    this.State.WUE[speciesIndex] = wue;

                    Vector128<float> transp_veg = this.State.transp_veg[speciesIndex];
                    Vector128<float> wue_transp = zero;
                    byte transpVegMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(transp_veg, zero));
                    if (transpVegMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        wue_transp = Avx.Blend(wue_transp, Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(100.0F), Avx.Divide(npp, transp_veg)), transpVegMask);
                    }
                    this.State.WUE_transp[speciesIndex] = wue_transp;
                }

                if (this.Settings.CalculateD13C)
                {
                    // δ¹³C module ----------------------------------------------------------------------
                    // Calculating δ¹³C - This is based on Wei et al. 2014(Plant, Cell and Environment 37, 82 - 100)
                    // and Wei et al. 2014(Forest Ecology and Management 313, 69 - 82).This is simply calculated from
                    // other variables and has no influence on any processes
                    // Since δ¹³C is only supported by 3-PGmix canopy_cover is expected to be null the first time this
                    // block is reached.
                    Debug.Assert(Single.IsNaN(this.Climate.D13Catm[timestep]) == false);

                    float daysInMonth = timestepEndDate.DaysInMonth();
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        // convert GPP(currently in tDM/ ha / month) to GPP in mol / m2 / s.
                        Vector128<float> GPP = this.State.GPP[speciesIndex];
                        float gDM_mol = this.Parameters.gDM_mol[speciesIndex];
                        Vector128<float> GPP_molsec = Avx.Multiply(GPP, AvxExtensions.BroadcastScalarToVector128(100.0F / (daysInMonth * 24.0F * 3600.0F * gDM_mol)));

                        // canopy conductance for water vapour in mol / m2s, unit conversion(CanCond is m / s)
                        Vector128<float> conduct_canopy = this.State.conduct_canopy[speciesIndex];
                        float tmp_ave = this.Climate.MeanDailyTemp[timestep];
                        Vector128<float> Gw_mol = Avx.Multiply(conduct_canopy, AvxExtensions.BroadcastScalarToVector128(44.6F * (273.15F / (273.15F + tmp_ave)) * (this.State.airPressure / 101.3F)));
                        this.State.Gw_mol[speciesIndex] = Gw_mol;

                        // canopy conductance for CO₂ in mol / m2s
                        // This calculation needs to consider the area covered by leaves as opposed to the total ground area of the stand.

                        // The explanation that Wei et al.provided for adding the "/Maximum(0.0000001, CanCover)" is
                        // that 3PG is a big leaf leaf model for conductance and the leaf area is assumed to be evenly distributed
                        // across the land area.So GwMol is divided by Maximum(0.0000001, CanCover) to convert the conductance
                        // to the area covered by the leaves only, which is smaller than the land area if the canopy has not
                        // closed.If the original light model has been selected then a CanCover value has already been calculated
                        // although Wei et al.also warn against using d13C calculations in stands with CanCover< 1.

                        // If the new light model has been selected then CanCover still needs to be calculated.
                        Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                        Vector128<float> crown_width_025 = Avx.Add(this.State.crown_width[speciesIndex], AvxExtensions.BroadcastScalarToVector128(0.25F));
                        Vector128<float> canopy_cover = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(1.0F / 10000.0F), Avx.Multiply(stems_n, Avx.Multiply(crown_width_025, crown_width_025)));
                        canopy_cover = Avx.Max(canopy_cover, one);
                        this.State.canopy_cover[speciesIndex] = canopy_cover;

                        Vector128<float> RGcGw = AvxExtensions.BroadcastScalarToVector128(this.Parameters.RGcGw[speciesIndex]);
                        Vector128<float> Gc_mol = Avx.Divide(Avx.Multiply(Gw_mol, RGcGw), Avx.Max(AvxExtensions.BroadcastScalarToVector128(0.0000001F), canopy_cover));
                        this.State.Gc_mol[speciesIndex] = Gc_mol;

                        // default values for dormancy
                        Vector128<float> interCi = zero;
                        Vector128<float> d13CNewPS = zero;
                        Vector128<float> d13CTissue = zero;
                        byte gcMolMask = (byte)Avx.MoveMask(Avx.CompareEqual(Gc_mol, zero));
                        if (gcMolMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            Vector128<float> co2 = AvxExtensions.BroadcastScalarToVector128(0.000001F * this.Climate.AtmosphericCO2[timestep]);
                            // calculating monthly average intercellular CO₂ concentration.Ci = Ca - A / g
                            interCi = Avx.Blend(interCi, Avx.Subtract(co2, Avx.Divide(GPP_molsec, Gc_mol)), gcMolMask);

                            // calculating monthly d13C of new photosynthate, = d13Catm - a - (b - a)(ci / ca)
                            Vector128<float> d13Catm = AvxExtensions.BroadcastScalarToVector128(this.Climate.D13Catm[timestep]);
                            Vector128<float> aFracDiffu = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aFracDiffu[speciesIndex]);
                            Vector128<float> bFracRubi = AvxExtensions.BroadcastScalarToVector128(this.Parameters.bFracRubi[speciesIndex]);
                            d13CNewPS = Avx.Blend(d13CNewPS, Avx.Subtract(d13Catm, Avx.Add(aFracDiffu, Avx.Multiply(Avx.Subtract(bFracRubi, aFracDiffu), Avx.Divide(interCi, co2)))), gcMolMask);

                            Vector128<float> d13CTissueDif = AvxExtensions.BroadcastScalarToVector128(this.Parameters.D13CTissueDif[speciesIndex]);
                            d13CTissue = Avx.Blend(d13CTissue, Avx.Add(d13CNewPS, d13CTissueDif), gcMolMask);
                        }

                        this.State.InterCi[speciesIndex] = interCi;
                        this.State.D13CNewPS[speciesIndex] = d13CNewPS;
                        this.State.D13CTissue[speciesIndex] = d13CTissue;
                    }
                }

                // Biomass increment and loss module ----------------------------------------------
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    // determine biomass increments and losses
                    float m0 = this.Parameters.m0[speciesIndex];
                    float fertility = this.Species.SoilFertility[speciesIndex];
                    Vector128<float> m = AvxExtensions.BroadcastScalarToVector128(m0 + (1.0F - m0) * fertility);
                    this.State.m[speciesIndex] = m;

                    Vector128<float> pRx = AvxExtensions.BroadcastScalarToVector128(this.Parameters.pRx[speciesIndex]);
                    Vector128<float> pRn = AvxExtensions.BroadcastScalarToVector128(this.Parameters.pRn[speciesIndex]);
                    Vector128<float> f_phys = this.State.f_phys[speciesIndex];
                    Vector128<float> npp_fract_root = Avx.Divide(Avx.Multiply(pRx, pRn), Avx.Add(pRn, Avx.Multiply(Avx.Subtract(pRx, pRn), Avx.Multiply(f_phys, m))));
                    this.State.npp_fract_root[speciesIndex] = npp_fract_root;

                    Vector128<float> pFS = this.State.pFS[speciesIndex];
                    Vector128<float> npp_fract_stem = Avx.Divide(Avx.Subtract(one, npp_fract_root), Avx.Add(one, pFS));
                    this.State.npp_fract_stem[speciesIndex] = npp_fract_stem;

                    this.State.npp_fract_foliage[speciesIndex] = Avx.Subtract(one, Avx.Add(npp_fract_root, npp_fract_stem));

                    // Dormant period -----------
                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                    {
                        // if this is the first dormant period then there is litterfall
                        Vector128<float> biom_loss_foliage = zero;
                        if (this.IsDormant(monthOfYear - 1, speciesIndex))
                        {
                            biom_loss_foliage = this.State.biom_foliage_debt[speciesIndex];
                        }
                        this.State.biom_loss_foliage[speciesIndex] = biom_loss_foliage;

                        this.State.biom_loss_root[speciesIndex] = zero;

                        // no growth during leaf off
                        this.State.biom_incr_foliage[speciesIndex] = zero;
                        this.State.biom_incr_root[speciesIndex] = zero;
                        this.State.biom_incr_stem[speciesIndex] = zero;
                    }
                    else
                    {
                        // if there are some leaves to be grown put NPP first to the leaf growth
                        Vector128<float> biom_foliage = this.State.biom_foliage[speciesIndex];
                        byte leafOutMask = (byte)Avx.MoveMask(Avx.CompareEqual(biom_foliage, zero));
                        if (leafOutMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            biom_foliage = Avx.Blend(biom_foliage, this.State.biom_foliage_debt[speciesIndex], leafOutMask);
                            this.State.biom_foliage[speciesIndex] = biom_foliage;
                        }

                        Vector128<float> biom_foliage_debt = this.State.biom_foliage_debt[speciesIndex];
                        Vector128<float> npp = this.State.NPP[speciesIndex];
                        Vector128<float> biomFoliageDebtPaid = Avx.Min(biom_foliage_debt, npp);
                        npp = Avx.Subtract(npp, biomFoliageDebtPaid);
                        biom_foliage_debt = Avx.Subtract(biom_foliage_debt, biomFoliageDebtPaid);
                        this.State.biom_foliage_debt[speciesIndex] = biom_foliage_debt;
                        this.State.NPP[speciesIndex] = npp;

                        // biomass loss
                        Vector128<float> gammaF = this.Trajectory.Species.gammaF[speciesIndex][timestep];
                        Vector128<float> biom_loss_foliage = Avx.Multiply(gammaF, biom_foliage);
                        Vector128<float> gammaR = AvxExtensions.BroadcastScalarToVector128(this.Parameters.gammaR[speciesIndex]);
                        Vector128<float> biom_root = this.State.biom_root[speciesIndex];
                        Vector128<float> biom_loss_root = Avx.Multiply(gammaR, biom_root);

                        this.State.biom_loss_foliage[speciesIndex] = biom_loss_foliage;
                        this.State.biom_loss_root[speciesIndex] = biom_loss_root;

                        // biomass increments
                        Vector128<float> biom_incr_foliage = Avx.Multiply(npp, this.State.npp_fract_foliage[speciesIndex]);
                        Vector128<float> biom_incr_root = Avx.Multiply(npp, this.State.npp_fract_root[speciesIndex]);
                        Vector128<float> biom_incr_stem = Avx.Multiply(npp, this.State.npp_fract_stem[speciesIndex]);

                        this.State.biom_incr_foliage[speciesIndex] = biom_incr_foliage;
                        this.State.biom_incr_root[speciesIndex] = biom_incr_root;
                        this.State.biom_incr_stem[speciesIndex] = biom_incr_stem;

                        // end-of-month biomass
                        this.State.biom_foliage[speciesIndex] = Avx.Add(biom_foliage, Avx.Subtract(biom_incr_foliage, biom_loss_foliage));

                        this.State.biom_root[speciesIndex] = Avx.Add(biom_root, Avx.Subtract(biom_incr_root, biom_loss_root));

                        Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                        this.State.biom_stem[speciesIndex] = Avx.Add(biom_stem, biom_incr_stem);
                    }
                }

                // correct the bias
                this.GetMeanStemMassAndUpdateLai(timestep);
                this.CorrectSizeDistribution(timestep, Constant.Simd128x4.MaskAllTrue);

                // volume and volume increment
                // This is done before thinning and mortality part.
                this.GetVolumeAndIncrement(timestep);

                // Management------------------------------------------------------------------------ -
                if (this.Management.n_sp > 0)
                {
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        float[] thinningAges = this.Management.age[speciesIndex];
                        int thinningIndex = this.State.t_n[speciesIndex];
                        if (thinningAges.Length <= thinningIndex)
                        {
                            continue;
                        }

                        float age = this.Trajectory.Species.age[speciesIndex][timestep];
                        if (age >= thinningAges[thinningIndex])
                        {
                            Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                            Vector128<float> target_stems_n = AvxExtensions.BroadcastScalarToVector128(this.Management.stems_n[speciesIndex][thinningIndex]);

                            byte managementMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(stems_n, target_stems_n));
                            if (managementMask != Constant.Simd128x4.MaskAllFalse)
                            {
                                Vector128<float> mort_manag = Avx.Blend(zero, Avx.Divide(Avx.Subtract(stems_n, target_stems_n), stems_n), managementMask); // could also use Avx.Max(zero, Avx.Divide(...))
                                stems_n = Avx.Multiply(stems_n, Avx.Subtract(one, mort_manag));
                                this.State.mort_manag[speciesIndex] = mort_manag;
                                this.State.stems_n[speciesIndex] = stems_n;

                                // if the stand is thinned from above, then the ratios(F, R and S) of stem,
                                // foliage and roots to be removed relative to the mean tree in the stand
                                // will be > 1.If the product of this ratio and delN is > 1 then the new
                                // WF, WR or WS will be< 0, which is impossible.Therefore, make sure this is >= 0.
                                float stemFraction = this.Management.stem[speciesIndex][thinningIndex];
                                float rootFraction = this.Management.root[speciesIndex][thinningIndex];
                                float foliageFraction = this.Management.foliage[speciesIndex][thinningIndex];
                                float maxFraction = MathF.Max(MathF.Max(stemFraction, rootFraction), foliageFraction);

                                Vector128<float> maxMort = Avx.Min(Avx.Multiply(mort_manag, AvxExtensions.BroadcastScalarToVector128(maxFraction)), one);
                                byte maxMortMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(maxMort, one));

                                Vector128<float> foliageLoss = Avx.Subtract(one, Avx.Multiply(mort_manag, AvxExtensions.BroadcastScalarToVector128(foliageFraction)));
                                if (this.IsDormant(monthOfYear, speciesIndex) == true)
                                {
                                    Vector128<float> biom_foliage_debt = this.State.biom_foliage_debt[speciesIndex];
                                    biom_foliage_debt = Avx.Blend(Avx.Multiply(biom_foliage_debt, foliageLoss), zero, maxMortMask);
                                    this.State.biom_foliage_debt[speciesIndex] = biom_foliage_debt;
                                }
                                else
                                {
                                    Vector128<float> biom_foliage = this.State.biom_foliage[speciesIndex];
                                    biom_foliage = Avx.Blend(Avx.Multiply(biom_foliage, foliageLoss), zero, maxMortMask);
                                    this.State.biom_foliage[speciesIndex] = biom_foliage;
                                }

                                Vector128<float> rootLoss = Avx.Subtract(one, Avx.Multiply(mort_manag, AvxExtensions.BroadcastScalarToVector128(rootFraction)));
                                Vector128<float> biom_root = this.State.biom_root[speciesIndex];
                                biom_root = Avx.Blend(Avx.Multiply(biom_root, rootLoss), zero, maxMortMask);
                                this.State.biom_root[speciesIndex] = biom_root;

                                Vector128<float> stemLoss = Avx.Subtract(one, Avx.Multiply(mort_manag, AvxExtensions.BroadcastScalarToVector128(stemFraction)));
                                Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                                biom_stem = Avx.Blend(Avx.Multiply(biom_stem, stemLoss), zero, maxMortMask);
                                this.State.biom_stem[speciesIndex] = biom_stem;

                                correctSizeDistributionMask = managementMask;
                            }

                            this.State.t_n[speciesIndex] = thinningIndex + 1;
                        }
                    }
                }

                // correct the bias
                if (correctSizeDistributionMask != Constant.Simd128x4.MaskAllFalse)
                {
                    this.GetMeanStemMassAndUpdateLai(timestep);
                    this.CorrectSizeDistribution(timestep, correctSizeDistributionMask);

                    // update volume for thinning
                    this.GetVolumeAndIncrement(timestep);
                    correctSizeDistributionMask = Constant.Simd128x4.MaskAllFalse;
                }

                // Mortality--------------------------------------------------------------------------
                // Stress related ------------------
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    if (this.IsDormant(monthOfYear, speciesIndex) == false)
                    {
                        Vector128<float> gammaN = this.Trajectory.Species.gammaN[speciesIndex][timestep];
                        byte stressMortMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(gammaN, zero));
                        if (stressMortMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                            Vector128<float> mort_stress = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(1.0F / (12.0F * 100.0F)), Avx.Multiply(gammaN, stems_n));
                            mort_stress = Avx.Blend(Avx.Min(mort_stress, stems_n), zero, stressMortMask); // mortality can't be more than available
                            this.State.mort_stress[speciesIndex] = mort_stress;

                            // following loss calculations do not require masking as multiplication by zero stress mortality results in zero mortality
                            Vector128<float> mF = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mF[speciesIndex]);
                            Vector128<float> biom_foliage = this.State.biom_foliage[speciesIndex];
                            Vector128<float> foliageLoss = Avx.Multiply(mF, Avx.Multiply(mort_stress, Avx.Divide(biom_foliage, stems_n)));
                            this.State.biom_foliage[speciesIndex] = Avx.Subtract(biom_foliage, foliageLoss);

                            Vector128<float> mR = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mR[speciesIndex]);
                            Vector128<float> biom_root = this.State.biom_root[speciesIndex];
                            Vector128<float> rootLoss = Avx.Multiply(mR, Avx.Multiply(mort_stress, Avx.Divide(biom_root, stems_n)));
                            this.State.biom_root[speciesIndex] = Avx.Subtract(biom_root, rootLoss);
                            
                            Vector128<float> mS = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mS[speciesIndex]);
                            Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                            Vector128<float> stemLoss = Avx.Multiply(mS, Avx.Multiply(mort_stress, Avx.Divide(biom_stem, stems_n)));
                            this.State.biom_stem[speciesIndex] = Avx.Subtract(biom_stem, stemLoss);

                            this.State.stems_n[speciesIndex] = Avx.Subtract(stems_n, mort_stress);

                            correctSizeDistributionMask = stressMortMask;
                        }
                    }
                    else
                    {
                        this.State.mort_stress[speciesIndex] = zero;
                    }
                }

                // correct the bias
                if (correctSizeDistributionMask != Constant.Simd128x4.MaskAllFalse)
                {
                    this.GetMeanStemMassAndUpdateLai(timestep);
                    this.CorrectSizeDistribution(timestep, correctSizeDistributionMask);
                    correctSizeDistributionMask = Constant.Simd128x4.MaskAllFalse;
                }

                // self-thinning ------------------
                Vector128<float> totalBasalArea = this.State.basal_area.Sum();
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    Vector128<float> basal_area = this.State.basal_area[speciesIndex];
                    Vector128<float> basal_area_prop = Avx.Divide(basal_area, totalBasalArea);
                    this.State.basal_area_prop[speciesIndex] = basal_area_prop;
                    // basal_area_prop[i] if basal_area_prop[i] > 0 and basal_area_prop[i] < 0.01 put 0.01
                    // where(lai[i] > 0.0F.and.basal_area_prop[i] < 0.01F) basal_area_prop[i] = 0.01F
                    Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                    Vector128<float> stems_n_ha = Avx.Divide(stems_n, basal_area_prop);
                    this.State.stems_n_ha[speciesIndex] = stems_n_ha;

                    Vector128<float> wSx1000 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.wSx1000[speciesIndex]);
                    Vector128<float> thinPower = AvxExtensions.BroadcastScalarToVector128(this.Parameters.thinPower[speciesIndex]);
                    this.State.biom_tree_max[speciesIndex] = Avx.Multiply(wSx1000, MathV.Pow(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.0001F), stems_n_ha), thinPower));

                    if (this.IsDormant(monthOfYear, speciesIndex) == false)
                    {
                        Vector128<float> biom_tree_max = this.State.biom_tree_max[speciesIndex];
                        Vector128<float> biom_tree  = this.State.biom_tree[speciesIndex];
                        byte selfThinningMask = (byte)Avx.MoveMask(Avx.CompareLessThan(biom_tree_max, biom_tree));
                        if (selfThinningMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            Vector128<float> mort_thinn = Avx.Multiply(this.GetMortality(speciesIndex), basal_area_prop);
                            this.State.mort_thinn[speciesIndex] = mort_thinn;

                            Vector128<float> biom_foliage = this.State.biom_foliage[speciesIndex];
                            Vector128<float> mF = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mF[speciesIndex]);
                            biom_foliage = Avx.Subtract(biom_foliage, Avx.Multiply(mF, Avx.Multiply(mort_thinn, Avx.Divide(biom_foliage, stems_n))));
                            this.State.biom_foliage[speciesIndex] = Avx.Max(biom_foliage, zero);

                            Vector128<float> biom_root = this.State.biom_root[speciesIndex];
                            Vector128<float> mR = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mR[speciesIndex]);
                            biom_root = Avx.Subtract(biom_root, Avx.Multiply(mR, Avx.Multiply(mort_thinn, Avx.Divide(biom_root, stems_n))));
                            this.State.biom_root[speciesIndex] = Avx.Max(biom_root, zero);

                            Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                            Vector128<float> mS = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mS[speciesIndex]);
                            biom_stem = Avx.Subtract(biom_stem, Avx.Multiply(mS, Avx.Multiply(mort_thinn, Avx.Divide(biom_stem, stems_n))));
                            this.State.biom_stem[speciesIndex] = Avx.Max(biom_stem, zero);

                            stems_n = Avx.Subtract(stems_n, mort_thinn);
                            this.State.stems_n[speciesIndex] = Avx.Max(stems_n, zero);

                            correctSizeDistributionMask = selfThinningMask;
                        }
                    }
                    else
                    {
                        this.State.mort_thinn[speciesIndex] = zero;
                    }
                }

                // correct the bias
                if (correctSizeDistributionMask != Constant.Simd128x4.MaskAllFalse)
                {
                    this.GetMeanStemMassAndUpdateLai(timestep);
                    this.CorrectSizeDistribution(timestep, correctSizeDistributionMask);
                    // not necessary as there is no subsequent access
                    // correctSizeDistributionMask = Constant.Simd128x4.MaskAllFalse; 
                }

                // Additional calculations ------------------
                totalBasalArea = this.State.basal_area.Sum();
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    this.State.basal_area_prop[speciesIndex] = Avx.Divide(this.State.basal_area[speciesIndex], totalBasalArea);

                    // efficiency
                    Vector128<float> apar = this.State.apar[speciesIndex];
                    Vector128<float> epsilon_gpp;
                    Vector128<float> epsilon_npp;
                    Vector128<float> epsilon_biom_stem;
                    byte aparMask = (byte)Avx.MoveMask(Avx.CompareEqual(apar, zero));
                    if (aparMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> oneHundred = AvxExtensions.BroadcastScalarToVector128(100.0F);
                        epsilon_gpp = Avx.Multiply(oneHundred, Avx.Divide(this.State.GPP[speciesIndex], apar));
                        epsilon_npp = Avx.Multiply(oneHundred, Avx.Divide(this.State.NPP_f[speciesIndex], apar));
                        epsilon_biom_stem = Avx.Multiply(oneHundred, Avx.Divide(this.State.biom_incr_stem[speciesIndex], apar));
                    }
                    else
                    {
                        epsilon_gpp = zero;
                        epsilon_npp = zero;
                        epsilon_biom_stem = zero;
                    }
                    this.State.epsilon_gpp[speciesIndex] = epsilon_gpp;
                    this.State.epsilon_npp[speciesIndex] = epsilon_npp;
                    this.State.epsilon_biom_stem[speciesIndex] = epsilon_biom_stem;
                }

                // copy species-specific state into stand trajectory: capture remaining end of month state
                this.Trajectory.Species.SetMonth(timestep, State);
            }
        }

        private void CorrectSizeDistribution(int timestep, byte correctionMask)
        {
            Debug.Assert((this.Bias != null) && (correctionMask != Constant.Simd128x4.MaskAllFalse));

            // Diameter distributions are used to correct for bias when calculating pFS from mean dbh, and ws distributions are
            // used to correct for bias when calculating mean dbh from mean ws.This bias is caused by Jensen's inequality and is
            // corrected using the approach described by Duursma and Robinson(2003) FEM 186, 373 - 380, which uses the CV of the
            // distributions and the exponent of the relationship between predicted and predictor variables.

            // The default is to ignore the bias. The alternative is to correct for it by using empirically derived weibull distributions
            // from the weibull parameters provided by the user. If the weibull distribution does not vary then just provide scale0 and shape0.
            int n_sp = this.Species.n_sp;
            for (int n = 0; n < this.Settings.BiasCorrectionIterations; ++n)
            {
                // LAI
                Vector128<float> lai_total = this.State.lai.Sum();
                Vector128<float> zero = Vector128<float>.Zero;
                Vector128<float> standHeight = zero;
                Vector128<float> totalStems = zero;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                    standHeight = Avx.Add(standHeight, Avx.Multiply(this.State.height[speciesIndex], stems_n));
                    totalStems = Avx.Add(totalStems, stems_n);
                }
                standHeight = Avx.Divide(standHeight, totalStems);

                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    // calculate the relative height
                    // float height = aH[speciesIndex] * dbh[speciesIndex] * *nHB[speciesIndex] * competition_total[speciesIndex] * *nHC[speciesIndex]
                    Vector128<float> height = this.State.height[speciesIndex];
                    Vector128<float> height_rel = Avx.Divide(height, standHeight);

                    Vector128<float> uncorrectedRelativeHeight = this.State.height_rel[speciesIndex];
                    this.State.height_rel[speciesIndex] = Avx.Blend(uncorrectedRelativeHeight, height_rel, correctionMask);
                }

                if (this.Settings.CorrectSizeDistribution)
                {
                    // Calculate the DW scale -------------------
                    Vector128<float> lnCompetitionTotal = MathV.Ln(this.State.competition_total);
                    for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                    {
                        float age = this.Trajectory.Species.age[speciesIndex][timestep];
                        if (age <= 0.0F)
                        {
                            // log of age is minus infinity so bias correction is not possible
                            // All Weibull values end up being NaN. Zeroing the bias corrections for trees of age
                            // zero is handled below this if() block.
                            continue;
                        }

                        Vector128<float> lnAge = AvxExtensions.BroadcastScalarToVector128(MathF.Log(age));
                        Vector128<float> lnDbh = MathV.Ln(this.State.dbh[speciesIndex]);
                        Vector128<float> lnRelativeHeight = MathV.Ln(this.State.height_rel[speciesIndex]);

                        Vector128<float> Dscale0 = AvxExtensions.BroadcastScalarToVector128(this.Bias.Dscale0[speciesIndex]);
                        Vector128<float> DscaleB = AvxExtensions.BroadcastScalarToVector128(this.Bias.DscaleB[speciesIndex]);
                        Vector128<float> Dscalerh = AvxExtensions.BroadcastScalarToVector128(this.Bias.Dscalerh[speciesIndex]);
                        Vector128<float> exponent = Avx.Add(Avx.Add(Dscale0, Avx.Multiply(DscaleB, lnDbh)), Avx.Multiply(Dscalerh, lnRelativeHeight));
                        Vector128<float> Dscalet = AvxExtensions.BroadcastScalarToVector128(this.Bias.Dscalet[speciesIndex]);
                        Vector128<float> DscaleC = AvxExtensions.BroadcastScalarToVector128(this.Bias.DscaleC[speciesIndex]);
                        exponent = Avx.Add(Avx.Add(exponent, Avx.Multiply(Dscalet, lnAge)), Avx.Multiply(DscaleC, lnCompetitionTotal));
                        Vector128<float> DWeibullScale = Avx.Blend(zero, MathV.Exp(exponent), correctionMask);
                        this.State.DWeibullScale[speciesIndex] = DWeibullScale;

                        Vector128<float> Dshape0 = AvxExtensions.BroadcastScalarToVector128(this.Bias.Dshape0[speciesIndex]);
                        Vector128<float> DshapeB = AvxExtensions.BroadcastScalarToVector128(this.Bias.DshapeB[speciesIndex]);
                        Vector128<float> Dshaperh = AvxExtensions.BroadcastScalarToVector128(this.Bias.Dshaperh[speciesIndex]);
                        exponent = Avx.Add(Dshape0, Avx.Add(Avx.Multiply(DshapeB, lnDbh), Avx.Multiply(Dshaperh, lnRelativeHeight)));
                        Vector128<float> Dshapet = AvxExtensions.BroadcastScalarToVector128(this.Bias.Dshapet[speciesIndex]);
                        Vector128<float> DshapeC = AvxExtensions.BroadcastScalarToVector128(this.Bias.DshapeC[speciesIndex]);
                        exponent = Avx.Add(exponent, Avx.Add(Avx.Multiply(Dshapet, lnAge), Avx.Multiply(DshapeC, lnCompetitionTotal)));
                        Vector128<float> DWeibullShape = Avx.Blend(zero, MathV.Exp(exponent), correctionMask);
                        this.State.DWeibullShape[speciesIndex] = DWeibullShape;

                        Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                        Vector128<float> DWeibullShape_gamma = ThreePGSimd128.GammaDistribution(Avx.Add(one, Avx.Divide(one, DWeibullShape)));

                        float Dlocation0scalar = this.Bias.Dlocation0[speciesIndex];
                        float DlocationBscalar = this.Bias.DlocationB[speciesIndex];
                        float DlocationRHscalar = this.Bias.Dlocationrh[speciesIndex];
                        float DlocationTscalar = this.Bias.Dlocationt[speciesIndex];
                        float DlocationCscalar = this.Bias.DlocationC[speciesIndex];
                        Vector128<float> DWeibullLocation;
                        if ((Dlocation0scalar == 0.0F) && (DlocationBscalar == 0.0F) && (DlocationRHscalar == 0.0F) &&
                            (DlocationTscalar == 0.0F) && (DlocationCscalar == 0.0F))
                        {
                            Vector128<float> dbh = this.State.dbh[speciesIndex];
                            DWeibullLocation = Avx.Subtract(Avx.Subtract(Avx.RoundToNearestInteger(dbh), one),
                                                            Avx.Multiply(DWeibullScale, DWeibullShape_gamma));
                        }
                        else
                        {
                            Vector128<float> Dlocation0 = AvxExtensions.BroadcastScalarToVector128(Dlocation0scalar);
                            Vector128<float> DlocationB = AvxExtensions.BroadcastScalarToVector128(DlocationBscalar);
                            Vector128<float> Dlocationrh = AvxExtensions.BroadcastScalarToVector128(DlocationRHscalar);
                            Vector128<float> Dlocationt = AvxExtensions.BroadcastScalarToVector128(DlocationTscalar);
                            Vector128<float> DlocationC = AvxExtensions.BroadcastScalarToVector128(DlocationCscalar);
                            exponent = Avx.Add(Dlocation0, Avx.Add(Avx.Multiply(DlocationB, lnDbh), Avx.Multiply(Dlocationrh, lnRelativeHeight)));
                            exponent = Avx.Add(exponent, Avx.Add(Avx.Multiply(Dlocationt, lnAge), Avx.Multiply(DlocationC, lnCompetitionTotal)));
                            DWeibullLocation = MathV.Exp(exponent);
                        }
                        DWeibullLocation = Avx.Blend(zero, Avx.Max(DWeibullLocation, AvxExtensions.BroadcastScalarToVector128(0.01F)), correctionMask);
                        this.State.DWeibullLocation[speciesIndex] = DWeibullLocation;

                        // Weibull expected value (3-PGmix user manual 11.10 equation A50)
                        Vector128<float> Ex = Avx.Add(DWeibullLocation, Avx.Multiply(DWeibullScale, DWeibullShape_gamma));
                        // now convert the Ex from weibull scale to actual scale of diameter units in cm
                        one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                        Vector128<float> Varx = Avx.Multiply(Avx.Multiply(DWeibullScale, DWeibullScale), Avx.Subtract(ThreePGSimd128.GammaDistribution(Avx.Add(one, Avx.Divide(AvxExtensions.BroadcastScalarToVector128(2.0F), DWeibullShape))), Avx.Multiply(DWeibullShape_gamma, DWeibullShape_gamma)));
                        // Weibull coefficient of variation
                        Vector128<float> CVdbhDistribution = Avx.Blend(zero, Avx.Divide(Avx.Sqrt(Varx), Ex), correctionMask);
                        this.State.CVdbhDistribution[speciesIndex] = CVdbhDistribution;

                        // calculate the bias (3-PGmix user manual 11.10 equation A49)
                        // prevent unrealistically large biases by restricting to ±50%
                        Vector128<float> CVdbhSquared = Avx.Multiply(CVdbhDistribution, CVdbhDistribution);
                        Vector128<float> half = AvxExtensions.BroadcastScalarToVector128(0.5F);
                        Vector128<float> halfCVdbhSquared = Avx.Multiply(half, CVdbhSquared);
                        Vector128<float> minusHalf = Avx.Subtract(Vector128<float>.Zero, half);
                        one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                        Vector128<float> pfsPower = this.State.pfsPower[speciesIndex];
                        Vector128<float> DrelBiaspFS = Avx.Blend(zero, 
                                                                 Avx.Max(minusHalf, 
                                                                         Avx.Min(Avx.Multiply(pfsPower, Avx.Multiply(Avx.Subtract(pfsPower, one), halfCVdbhSquared)),
                                                                                 half)), 
                                                                 correctionMask);
                        this.State.DrelBiaspFS[speciesIndex] = DrelBiaspFS;

                        Vector128<float> nHB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHB[speciesIndex]);
                        Vector128<float> DrelBiasheight = Avx.Blend(zero, 
                                                                    Avx.Max(minusHalf, 
                                                                            Avx.Min(Avx.Multiply(nHB, Avx.Multiply(Avx.Subtract(nHB, one), halfCVdbhSquared)),
                                                                                    half)), 
                                                                    correctionMask);
                        this.State.DrelBiasheight[speciesIndex] = DrelBiasheight;

                        Vector128<float> DrelBiasBasArea = Avx.Max(minusHalf, Avx.Min(CVdbhSquared, half));
                        this.State.DrelBiasBasArea[speciesIndex] = DrelBiasBasArea;

                        Vector128<float> nHLB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHLB[speciesIndex]);
                        Vector128<float> DrelBiasLCL = Avx.Blend(zero, 
                                                                 Avx.Max(minusHalf, 
                                                                         Avx.Min(Avx.Multiply(nHLB, Avx.Multiply(Avx.Subtract(nHLB, one), halfCVdbhSquared)),
                                                                                 half)),
                                                                 correctionMask);
                        this.State.DrelBiasLCL[speciesIndex] = DrelBiasLCL;

                        Vector128<float> nKB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nKB[speciesIndex]);
                        Vector128<float> DrelBiasCrowndiameter = Avx.Blend(zero, 
                                                                           Avx.Max(minusHalf, 
                                                                                   Avx.Min(Avx.Multiply(nKB, Avx.Multiply(Avx.Subtract(nKB, one), halfCVdbhSquared)),
                                                                                           half)),
                                                                           correctionMask);
                        this.State.DrelBiasCrowndiameter[speciesIndex] = DrelBiasCrowndiameter;

                        // calculate the biom_stem scale -------------------
                        Vector128<float> wsscale0 = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsscale0[speciesIndex]);
                        Vector128<float> wsscaleB = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsscaleB[speciesIndex]);
                        Vector128<float> wsscalerh = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsscalerh[speciesIndex]);
                        exponent = Avx.Add(wsscale0, Avx.Add(Avx.Multiply(wsscaleB, lnDbh), Avx.Multiply(wsscalerh, lnRelativeHeight)));
                        Vector128<float> wsscalet = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsscalet[speciesIndex]);
                        Vector128<float> wsscaleC = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsscaleC[speciesIndex]);
                        exponent = Avx.Add(exponent, Avx.Add(Avx.Multiply(wsscalet, lnAge), Avx.Multiply(wsscaleC, lnCompetitionTotal)));
                        Vector128<float> wsWeibullScale = Avx.Blend(zero, MathV.Exp(exponent), correctionMask);
                        this.State.wsWeibullScale[speciesIndex] = wsWeibullScale;

                        Vector128<float> wsshape0 = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsshape0[speciesIndex]);
                        Vector128<float> wsshapeB = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsshapeB[speciesIndex]);
                        Vector128<float> wsshaperh = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsshaperh[speciesIndex]);
                        exponent = Avx.Add(wsshape0, Avx.Add(Avx.Multiply(wsshapeB, lnDbh), Avx.Multiply(wsshaperh, lnRelativeHeight)));
                        Vector128<float> wsshapet = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsshapet[speciesIndex]);
                        Vector128<float> wsshapeC = AvxExtensions.BroadcastScalarToVector128(this.Bias.wsshapeC[speciesIndex]);
                        exponent = Avx.Add(exponent, Avx.Add(Avx.Multiply(wsshapet, lnAge), Avx.Multiply(wsshapeC, lnCompetitionTotal)));
                        Vector128<float> wsWeibullShape = Avx.Blend(zero, MathV.Exp(exponent), correctionMask);
                        this.State.wsWeibullShape[speciesIndex] = wsWeibullShape;

                        one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                        Vector128<float> wsWeibullShape_gamma = ThreePGSimd128.GammaDistribution(Avx.Add(one, Avx.Divide(one, wsWeibullShape)));

                        float wsLocation0scalar = this.Bias.wslocation0[speciesIndex];
                        float wsLocationBscalar = this.Bias.wslocationB[speciesIndex];
                        float wsLocationRHscalar = this.Bias.wslocationrh[speciesIndex];
                        float wsLocationTscalar = this.Bias.wslocationt[speciesIndex];
                        float wsLocationCscalar = this.Bias.wslocationC[speciesIndex];
                        Vector128<float> wsWeibullLocation;
                        if ((wsLocation0scalar == 0.0F) && (wsLocationBscalar == 0.0F) && (wsLocationRHscalar == 0.0F) &&
                            (wsLocationTscalar == 0.0F) && (wsLocationCscalar == 0.0F))
                        {
                            Vector128<float> biom_tree = this.State.biom_tree[speciesIndex];
                            one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                            wsWeibullLocation = Avx.Subtract(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.1F), Avx.RoundToNearestInteger(biom_tree)), Avx.Subtract(one, Avx.Multiply(wsWeibullScale, wsWeibullShape_gamma)));
                        }
                        else
                        {
                            Vector128<float> wslocation0 = AvxExtensions.BroadcastScalarToVector128(wsLocation0scalar);
                            Vector128<float> wslocationB = AvxExtensions.BroadcastScalarToVector128(wsLocationBscalar);
                            Vector128<float> wslocationrh = AvxExtensions.BroadcastScalarToVector128(wsLocationRHscalar);
                            exponent = Avx.Add(wslocation0, Avx.Add(Avx.Multiply(wslocationB, lnDbh), Avx.Multiply(wslocationrh, lnRelativeHeight)));
                            Vector128<float> wslocationt = AvxExtensions.BroadcastScalarToVector128(wsLocationTscalar);
                            Vector128<float> wslocationC = AvxExtensions.BroadcastScalarToVector128(wsLocationCscalar);
                            exponent = Avx.Add(exponent, Avx.Add(Avx.Multiply(wslocationt, lnAge), Avx.Multiply(wslocationC, lnCompetitionTotal)));
                            wsWeibullLocation = MathV.Exp(exponent);
                        }
                        wsWeibullLocation = Avx.Blend(zero, Avx.Max(wsWeibullLocation, AvxExtensions.BroadcastScalarToVector128(0.01F)), correctionMask);
                        this.State.wsWeibullLocation[speciesIndex] = wsWeibullLocation;

                        Ex = Avx.Add(wsWeibullLocation, Avx.Multiply(wsWeibullScale, wsWeibullShape_gamma));
                        // now convert the Ex from weibull scale to actual scale of diameter units in cm
                        one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                        Varx = Avx.Multiply(Avx.Multiply(wsWeibullScale, wsWeibullScale), Avx.Subtract(ThreePGSimd128.GammaDistribution(Avx.Add(one, Avx.Divide(AvxExtensions.BroadcastScalarToVector128(2.0F), wsWeibullShape))),
                                                                                                       Avx.Multiply(wsWeibullShape_gamma, wsWeibullShape_gamma)));
                        Vector128<float> CVwsDistribution = Avx.Blend(zero, Avx.Divide(Avx.Sqrt(Varx), Ex), correctionMask);
                        this.State.CVwsDistribution[speciesIndex] = CVwsDistribution;

                        // DF the nWS is replaced with 1 / nWs because the equation is inverted to predict dbh from ws, instead of ws from dbh
                        Vector128<float> nWsReciprocal = Avx.Divide(one, AvxExtensions.BroadcastScalarToVector128(this.Parameters.nWS[speciesIndex]));
                        half = AvxExtensions.BroadcastScalarToVector128(0.5F);
                        Vector128<float> halfCVwsSquared = Avx.Multiply(half, Avx.Multiply(CVwsDistribution, CVwsDistribution));
                        minusHalf = Avx.Subtract(Vector128<float>.Zero, half);
                        Vector128<float> wsrelBias = Avx.Blend(zero, 
                                                               Avx.Max(minusHalf,
                                                                       Avx.Min(Avx.Multiply(Avx.Multiply(nWsReciprocal, Avx.Subtract(nWsReciprocal, one)), halfCVwsSquared),
                                                                               half)),
                                                               correctionMask);
                        this.State.wsrelBias[speciesIndex] = wsrelBias;
                    }
                }
                else
                {
                    for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                    {
                        this.State.CVdbhDistribution[speciesIndex] = zero;
                        this.State.CVwsDistribution[speciesIndex] = zero;
                        this.State.DrelBiaspFS[speciesIndex] = zero;
                        this.State.DrelBiasBasArea[speciesIndex] = zero;
                        this.State.DrelBiasheight[speciesIndex] = zero;
                        this.State.DrelBiasLCL[speciesIndex] = zero;
                        this.State.DrelBiasCrowndiameter[speciesIndex] = zero;
                        this.State.wsrelBias[speciesIndex] = zero;
                    }
                }

                // correct for trees that have age 0 or are thinned (e.g. n_trees = 0)
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    float age = this.Trajectory.Species.age[speciesIndex][timestep];
                    Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                    if (age <= 0)
                    {
                        byte zeroStemsMask = (byte)Avx.MoveMask(Avx.CompareEqual(stems_n, zero));
                        if (zeroStemsMask == Constant.Simd128x4.MaskAllTrue)
                        {
                            this.State.CVdbhDistribution[speciesIndex] = zero;
                            this.State.CVwsDistribution[speciesIndex] = zero;
                            this.State.DrelBiaspFS[speciesIndex] = zero;
                            this.State.DrelBiasBasArea[speciesIndex] = zero;
                            this.State.DrelBiasheight[speciesIndex] = zero;
                            this.State.DrelBiasLCL[speciesIndex] = zero;
                            this.State.DrelBiasCrowndiameter[speciesIndex] = zero;
                            this.State.wsrelBias[speciesIndex] = zero;
                        }
                        else if (zeroStemsMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            // for now, assume SIMD lanes contain variations on the same stand with the same planting and simlation dates
                            throw new NotSupportedException("Species '" + this.Species.Species[speciesIndex] + "' has different ages in different SIMD lanes.");
                        }
                    }

                    // Correct for bias------------------
                    Vector128<float> aWs = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aWS[speciesIndex]);
                    Vector128<float> biom_tree = this.State.biom_tree[speciesIndex];
                    Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                    Vector128<float> nWsReciprocal = Avx.Divide(one, AvxExtensions.BroadcastScalarToVector128(this.Parameters.nWS[speciesIndex]));
                    Vector128<float> wsrelBias = this.State.wsrelBias[speciesIndex];
                    Vector128<float> dbh = Avx.Multiply(MathV.Pow(Avx.Divide(biom_tree, aWs), nWsReciprocal), Avx.Add(one, wsrelBias));
                    Vector128<float> uncorrectedDbh = this.State.dbh[speciesIndex];
                    this.State.dbh[speciesIndex] = Avx.Blend(uncorrectedDbh, dbh, correctionMask);

                    Vector128<float> DrelBiasBasArea = this.State.DrelBiasBasArea[speciesIndex];
                    this.State.basal_area[speciesIndex] = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.0001F * 0.25F * MathF.PI), Avx.Multiply(Avx.Multiply(dbh, dbh), Avx.Multiply(stems_n, Avx.Add(one, DrelBiasBasArea))));

                    Vector128<float> aH = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aH[speciesIndex]);
                    Vector128<float> nHB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHB[speciesIndex]);
                    Vector128<float> nHC = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHC[speciesIndex]);
                    Vector128<float> aHL = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aHL[speciesIndex]);
                    Vector128<float> nHLB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHLB[speciesIndex]);
                    Vector128<float> nHLC = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHLC[speciesIndex]);
                    Vector128<float> competition_total = this.State.competition_total;
                    Vector128<float> height_rel = this.State.height_rel[speciesIndex];
                    Vector128<float> height;
                    Vector128<float> crown_length;
                    switch (this.Settings.height_model)
                    {
                        case ThreePGHeightModel.Power:
                            Vector128<float> DrelBiasheight = this.State.DrelBiasheight[speciesIndex];
                            Vector128<float> DrelBiasLCL = this.State.DrelBiasLCL[speciesIndex];
                            one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                            height = Avx.Multiply(aH, Avx.Multiply(MathV.Pow(dbh, nHB), Avx.Multiply(MathV.Pow(competition_total, nHC), Avx.Add(one, DrelBiasheight))));
                            Vector128<float> nHLL = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHLL[speciesIndex]);
                            Vector128<float> nHLrh = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nHLrh[speciesIndex]);
                            crown_length = Avx.Multiply(aHL, Avx.Multiply(MathV.Pow(dbh, nHLB), Avx.Multiply(MathV.Pow(lai_total, nHLL), Avx.Multiply(MathV.Pow(competition_total, nHLC), Avx.Multiply(MathV.Pow(height_rel, nHLrh), Avx.Add(one, DrelBiasLCL))))));
                            break;
                        case ThreePGHeightModel.Exponent:
                            Vector128<float> competitionTotalDbh = Avx.Multiply(competition_total, dbh);
                            Vector128<float> breastHeight = AvxExtensions.BroadcastScalarToVector128(1.3F);
                            height = Avx.Add(breastHeight, Avx.Add(Avx.Multiply(aH, MathV.Exp(Avx.Divide(Avx.Subtract(zero, nHB), dbh))), Avx.Multiply(nHC, competitionTotalDbh)));
                            crown_length = Avx.Add(breastHeight, Avx.Add(Avx.Multiply(aHL, MathV.Exp(Avx.Divide(Avx.Subtract(zero, nHLB), dbh))), Avx.Multiply(nHLC, competitionTotalDbh)));
                            break;
                        default:
                            throw new NotSupportedException("Unhandled height model " + this.Settings.height_model + ".");
                    }
                    Vector128<float> uncorrectedHeight = this.State.height[speciesIndex];
                    this.State.height[speciesIndex] = Avx.Blend(uncorrectedHeight, height, correctionMask);

                    // check that the height and LCL allometric equations have not predicted that height - LCL < 0
                    // and if so reduce LCL so that height - LCL = 0(assumes height allometry is more reliable than LCL allometry)
                    crown_length = Avx.Min(crown_length, height);
                    this.State.crown_length[speciesIndex] = crown_length;

                    Vector128<float> aK = AvxExtensions.BroadcastScalarToVector128(this.Parameters.aK[speciesIndex]);
                    Vector128<float> nKB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nKB[speciesIndex]);
                    Vector128<float> nKH = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nKH[speciesIndex]);
                    Vector128<float> nKC = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nKC[speciesIndex]);
                    Vector128<float> nKrh = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nKrh[speciesIndex]);
                    Vector128<float> DrelBiasCrowndiameter = this.State.DrelBiasCrowndiameter[speciesIndex];
                    Vector128<float> crown_width = Avx.Multiply(aK, Avx.Multiply(MathV.Pow(dbh, nKB), Avx.Multiply(MathV.Pow(height, nKH), Avx.Multiply(MathV.Pow(competition_total, nKC), Avx.Multiply(MathV.Pow(height_rel, nKrh), Avx.Add(AvxExtensions.BroadcastScalarToVector128(1.0F), DrelBiasCrowndiameter))))));
                    
                    Vector128<float> lai = this.State.lai[speciesIndex];
                    byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                    crown_width = Avx.Blend(zero, crown_width, laiNonzeroMask);
                    Vector128<float> uncorrectedCrownWidth = this.State.crown_width[speciesIndex];
                    this.State.crown_width[speciesIndex] = Avx.Blend(uncorrectedCrownWidth, crown_width, correctionMask);

                    Vector128<float> pfsConst = this.State.pfsConst[speciesIndex];
                    Vector128<float> pfsPower = this.State.pfsPower[speciesIndex];
                    Vector128<float> DrelBiaspFS = this.State.DrelBiaspFS[speciesIndex];
                    this.State.pFS[speciesIndex] = Avx.Multiply(pfsConst, Avx.Multiply(MathV.Pow(dbh, pfsPower), Avx.Add(AvxExtensions.BroadcastScalarToVector128(1.0F), DrelBiaspFS)));

                    DebugV.Assert(Avx.CompareGreaterThan(dbh, Vector128<float>.Zero));
                }

                // update competition_total to new basal area
                Vector128<float> updated_competition_total = Vector128<float>.Zero;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    Vector128<float> wood_density = this.Trajectory.Species.wood_density[speciesIndex][0];
                    Vector128<float> basal_area = this.State.basal_area[speciesIndex];
                    updated_competition_total = Avx.Add(updated_competition_total, Avx.Multiply(wood_density, basal_area));
                }

                Vector128<float> uncorrectedCompetitionTotal = this.State.competition_total;
                this.State.competition_total = Avx.Blend(uncorrectedCompetitionTotal, updated_competition_total, correctionMask);
            }
        }

        private static Vector128<float> GammaDistribution(Vector128<float> x)
        {
            Vector128<float> gamma = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(Constant.Sqrt2Pi),
                                                  Avx.Multiply(MathV.Pow(x, Avx.Subtract(x, AvxExtensions.BroadcastScalarToVector128(0.5F))), 
                                                               MathV.Exp(Avx.Subtract(Vector128<float>.Zero, x))));

            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
            Vector128<float> polynomial = Avx.Add(one, Avx.Divide(one, Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(12.0F), x)));
            Vector128<float> xSquared = Avx.Multiply(x, x);
            polynomial = Avx.Add(polynomial, Avx.Divide(one, Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(288.0F), xSquared)));
            Vector128<float> xCubed = Avx.Multiply(xSquared, x);
            polynomial = Avx.Add(polynomial, Avx.Divide(AvxExtensions.BroadcastScalarToVector128(-139.0F), Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(51840.0F), xCubed)));
            Vector128<float> xToTheFourth = Avx.Multiply(xSquared, xSquared);
            polynomial = Avx.Add(polynomial, Avx.Divide(AvxExtensions.BroadcastScalarToVector128(-571.0F), Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(2488320.0F), xToTheFourth)));

            gamma = Avx.Multiply(gamma, polynomial);
            return gamma;
        }

        protected static Vector128<float>[] GetAgeDependentParameter(float[] age, Vector128<float> g0, Vector128<float> gx, Vector128<float> tg, Vector128<float> ng)
        {
            int n_m = age.Length;
            Vector128<float> minusLn2 = AvxExtensions.BroadcastScalarToVector128(-Constant.Ln2);
            Vector128<float>[] output = new Vector128<float>[n_m];
            byte tgZeroMask = (byte)Avx.MoveMask(Avx.CompareEqual(tg, Vector128<float>.Zero));
            for (int timestep = 0; timestep < n_m; ++timestep)
            {
                // could special case for all ng = 1 and 2
                Vector128<float> parameter = Avx.Add(gx, Avx.Multiply(Avx.Subtract(g0, gx), MathV.Exp(Avx.Multiply(minusLn2, MathV.Pow(Avx.Divide(AvxExtensions.BroadcastScalarToVector128(age[timestep]), tg), ng)))));
                if (tgZeroMask != Constant.Simd128x4.MaskAllFalse)
                {
                    // exp(-Inf) = 0 analytically but NaN in code
                    // Can also special case MaskAllTrue if needed.
                    parameter = Avx.Blend(parameter, gx, tgZeroMask);
                }
                output[timestep] = parameter;
            }
            return output;
        }

        // returns the indices that would sort an array in ascending order.
        private static void GetAscendingOrderIndices(ReadOnlySpan<Vector128<float>> values, Span<Vector128<int>> sortIndices)
        {
            // for now, default to a scalar implementation since spans are often short
            // Simple stands often have one or two layers, in which case spans are of length two or four.
            // gather
            Span<float> values0 = stackalloc float[values.Length];
            Span<float> values1 = stackalloc float[values.Length];
            Span<float> values2 = stackalloc float[values.Length];
            Span<float> values3 = stackalloc float[values.Length];
            Span<int> sortIndices0 = stackalloc int[values.Length];
            Span<int> sortIndices1 = stackalloc int[values.Length];
            Span<int> sortIndices2 = stackalloc int[values.Length];
            Span<int> sortIndices3 = stackalloc int[values.Length];
            for (int layerBoundaryIndex = 0; layerBoundaryIndex < values.Length; ++layerBoundaryIndex)
            {
                Vector128<float> value = values[layerBoundaryIndex];
                values0[layerBoundaryIndex] = Avx.Extract(value, Constant.Simd128x4.Extract0);
                values1[layerBoundaryIndex] = Avx.Extract(value, Constant.Simd128x4.Extract0);
                values2[layerBoundaryIndex] = Avx.Extract(value, Constant.Simd128x4.Extract0);
                values3[layerBoundaryIndex] = Avx.Extract(value, Constant.Simd128x4.Extract0);
                sortIndices0[layerBoundaryIndex] = layerBoundaryIndex;
                sortIndices1[layerBoundaryIndex] = layerBoundaryIndex;
                sortIndices2[layerBoundaryIndex] = layerBoundaryIndex;
                sortIndices3[layerBoundaryIndex] = layerBoundaryIndex;
            }

            // sort
            MemoryExtensions.Sort<float, int>(values0, sortIndices0);
            MemoryExtensions.Sort<float, int>(values1, sortIndices1);
            MemoryExtensions.Sort<float, int>(values2, sortIndices2);
            MemoryExtensions.Sort<float, int>(values3, sortIndices3);

            // scatter
            for (int layerBoundaryIndex = 0; layerBoundaryIndex < values.Length; ++layerBoundaryIndex)
            {
                Vector128<int> sortIndex = Avx2Extensions.Set128(sortIndices0[layerBoundaryIndex], sortIndices1[layerBoundaryIndex], sortIndices2[layerBoundaryIndex], sortIndices3[layerBoundaryIndex]);
                sortIndices[layerBoundaryIndex] = sortIndex;
            }
        }

        private void GetLayers(ReadOnlySpan<Vector128<float>> heightCrown)
        {
            // function to allocate each tree to the layer based on height and crown heigh
            // First layer (0) is the highest and layer number then increases as height decreases.
            // Forrester DI, Guisasola R, Tang X, et al. 2014. Using a stand-level model to predict light
            //   absorption in stands with vertically and horizontally heterogeneous canopies. Forest Ecosystems
            //   1:17. https://doi.org/10.1186/s40663-014-0017-0
            // Calculations based on example https://it.mathworks.com/matlabcentral/answers/366626-overlapping-time-intervals
            int n_sp = this.Species.n_sp;
            Span<Vector128<float>> height_all = stackalloc Vector128<float>[2 * n_sp];
            Vector128<int>[] ones = new Vector128<int>[2 * n_sp]; // vector of 1, 0, -1 for calculation
            Vector128<int> one = AvxExtensions.Set128(1);
            Vector128<int> minusOne = AvxExtensions.Set128(-1);
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // put height and crown beginning into vector
                height_all[speciesIndex] = heightCrown[speciesIndex];
                height_all[n_sp + speciesIndex] = this.State.height[speciesIndex];

                // assign index order for further calculations
                ones[speciesIndex] = one;
                ones[n_sp + speciesIndex] = minusOne;
            }

            // sort all height and crown height
            Span<Vector128<int>> height_ind = stackalloc Vector128<int>[2 * n_sp];
            ThreePGSimd128.GetAscendingOrderIndices(height_all, height_ind); // sort order of height_all
            Vector128<int>[] buffer = new Vector128<int>[ones.Length];
            for (int index = 0; index < height_ind.Length; ++index)
            {
                // scalar gather for now
                // Avx2.GatherVector128() could probably be used here by reinterpreting Vector128<int>[] as int[]
                // and manipulating vindex to compensate scale being limited to 8 but doing so may not be faster.
                Vector128<int> layerIndices = height_ind[index];
                Vector128<int> layers = ones[Avx.Extract(layerIndices, Constant.Simd128x4.Extract0)];
                layers = Avx2.Blend(layers, ones[Avx.Extract(layerIndices, Constant.Simd128x4.Extract1)], Constant.Simd128x4.Blend1);
                layers = Avx2.Blend(layers, ones[Avx.Extract(layerIndices, Constant.Simd128x4.Extract2)], Constant.Simd128x4.Blend2);
                layers = Avx2.Blend(layers, ones[Avx.Extract(layerIndices, Constant.Simd128x4.Extract3)], Constant.Simd128x4.Blend3);
                buffer[index] = layers;
            }
            Array.Copy(buffer, ones, ones.Length);

            // cumulative sum
            Vector128<int>[] ones_sum = new Vector128<int>[2 * n_sp];
            ones_sum[0] = ones[0];
            // number of layers: 1 if ones_sum is 0, otherwise 0
            Vector128<int> zero = Vector128<int>.Zero;
            Vector128<int> n_l = Avx.ShiftRightLogical(Avx.CompareEqual(ones_sum[0], zero), 31);
            // if (n_sp > 1) then
            for (int index = 1; index < ones.Length; ++index)
            {
                Vector128<int> sum = Avx2.Add(ones_sum[index - 1], ones[index]);
                ones_sum[index] = sum;

                n_l = Avx2.Add(n_l, Avx.ShiftRightLogical(Avx.CompareEqual(ones_sum[0], zero), 31));
            }
            // end if

            int maxLayerCount = AvxExtensions.HorizontalMax(n_l);

            // max height of each layer
            Span<Vector128<float>> height_layer = stackalloc Vector128<float>[maxLayerCount];
            int maxLayerIndex0 = 0;
            int maxLayerIndex1 = 0;
            int maxLayerIndex2 = 0;
            int maxLayerIndex3 = 0;
            for (int index = 0; index < height_ind.Length; ++index)
            {
                byte isZeroMask = (byte)Avx.MoveMask(Avx.CompareEqual(ones_sum[index], zero).AsSingle());
                if (isZeroMask != Constant.Simd128x4.MaskAllFalse)
                {
                    // inefficient element fiddling but unclear if a scalar data structure would be efficient 
                    if ((isZeroMask & 0x1) != 0)
                    {
                        int heightAllIndex0 = Avx.Extract(height_ind[index], Constant.Simd128x4.Extract0);
                        Vector128<float> height0 = Avx.Permute(height_all[heightAllIndex0], Constant.Simd128x4.Broadcast0toAll);

                        Vector128<float> layerHeights = Avx.Blend(height_layer[maxLayerIndex0], height0, Constant.Simd128x4.Blend0);
                        height_layer[maxLayerIndex0++] = layerHeights;
                    }
                    if ((isZeroMask & 0x2) != 0)
                    {
                        int heightAllIndex1 = Avx.Extract(height_ind[index], Constant.Simd128x4.Extract1);
                        Vector128<float> height1 = Avx.Permute(height_all[heightAllIndex1], Constant.Simd128x4.Broadcast1toAll);

                        Vector128<float> layerHeights = Avx.Blend(height_layer[maxLayerIndex1], height1, Constant.Simd128x4.Blend1);
                        height_layer[maxLayerIndex1++] = layerHeights;
                    }
                    if ((isZeroMask & 0x4) != 0)
                    {
                        int heightAllIndex2 = Avx.Extract(height_ind[index], Constant.Simd128x4.Extract2);
                        Vector128<float> height2 = Avx.Permute(height_all[heightAllIndex2], Constant.Simd128x4.Broadcast2toAll);

                        Vector128<float> layerHeights = Avx.Blend(height_layer[maxLayerIndex2], height2, Constant.Simd128x4.Blend2);
                        height_layer[maxLayerIndex2++] = layerHeights;
                    }
                    if ((isZeroMask & 0x8) != 0)
                    {
                        int heightAllIndex3 = Avx.Extract(height_ind[index], Constant.Simd128x4.Extract3);
                        Vector128<float> height3 = Avx.Permute(height_all[heightAllIndex3], Constant.Simd128x4.Broadcast3toAll);

                        Vector128<float> layerHeights = Avx.Blend(height_layer[maxLayerIndex3], height3, Constant.Simd128x4.Blend3);
                        height_layer[maxLayerIndex3++] = layerHeights;
                    }
                }
            }

            // assign layer to each species
            Array.Clear(this.State.layer_id);
            if (maxLayerCount > 1)
            {
                Vector128<int> maxLayerID = AvxExtensions.Set128(Int32.MinValue);
                for (int layerIndex = 0; layerIndex < maxLayerCount - 1; ++layerIndex)
                {
                    for (int speciesIndex = 0; speciesIndex < this.State.height.Length; ++speciesIndex)
                    {
                        byte isTallerMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(this.State.height[speciesIndex], height_layer[layerIndex]));
                        if (isTallerMask != Constant.Simd128x4.MaskAllFalse)
                        {
                            Vector128<int> layer_id = this.State.layer_id[speciesIndex];
                            Vector128<int> layerIDincrement = Avx.Add(layer_id, one);
                            layer_id = Avx2.Blend(layer_id, layerIDincrement, isTallerMask);
                            this.State.layer_id[speciesIndex] = layer_id;

                            maxLayerID = Avx.Max(maxLayerID, layer_id);
                        }
                    }
                }

                // revert the order, so highest trees are in layer 0 and lowest layer is layer n
                for (int speciesIndex = 0; speciesIndex < this.State.height.Length; ++speciesIndex)
                {
                    this.State.layer_id[speciesIndex] = Avx.Subtract(maxLayerID, this.State.layer_id[speciesIndex]);
                }
            }
        }

        private void GetLayerSum(int nLayers, ReadOnlySpan<Vector128<float>> x, Span<Vector128<float>> y)
        {
            // function to sum any array x, based on the vector of layers id
            int n_sp = this.Species.n_sp;
            Debug.Assert(y.Length == n_sp);
            for (int layerIndex = 0; layerIndex < nLayers; ++layerIndex)
            {
                Vector128<int> layerIndex128 = AvxExtensions.Set128(layerIndex);
                Vector128<float> layerSum = Vector128<float>.Zero;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    byte isEqualMask = (byte)Avx.MoveMask(Avx.CompareEqual(this.State.layer_id[speciesIndex], layerIndex128).AsSingle());
                    if (isEqualMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> layerSumX = Avx.Add(layerSum, x[speciesIndex]);
                        layerSum = Avx.Blend(layerSum, layerSumX, isEqualMask);
                    }
                }
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    byte isEqualMask = (byte)Avx.MoveMask(Avx.CompareEqual(this.State.layer_id[speciesIndex], layerIndex128).AsSingle());
                    if (isEqualMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> ySumBlend = Avx.Blend(y[speciesIndex], layerSum, isEqualMask);
                        y[speciesIndex] = ySumBlend;
                    }
                }
            }
        }

        private static Vector128<float>[] GetLitterfallRate(float[] age, Vector128<float> f1, Vector128<float> f0, Vector128<float> tg)
        {
            Vector128<float> minus_kg = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(-12.0F), Avx.Divide(MathV.Ln(Avx.Add(AvxExtensions.BroadcastScalarToVector128(1.0F), Avx.Divide(f1, f0))), tg));
            byte tgf1ZeroMask = (byte)Avx.MoveMask(Avx.CompareEqual(Avx.Multiply(tg, f1), Vector128<float>.Zero));
            int n_m = age.Length;
            Vector128<float>[] f = new Vector128<float>[n_m];
            for (int timestep = 0; timestep < n_m; ++timestep)
            {
                Vector128<float> litterfallRate = Avx.Divide(Avx.Multiply(f1, f0), Avx.Add(f0, Avx.Multiply(Avx.Subtract(f1, f0), MathV.Exp(Avx.Multiply(minus_kg, AvxExtensions.BroadcastScalarToVector128(age[timestep]))))));
                if (tgf1ZeroMask != Constant.Simd128x4.MaskAllFalse)
                {
                    litterfallRate = Avx.Blend(litterfallRate, f1, tgf1ZeroMask);
                }
                f[timestep] = litterfallRate;
            }
            return f;
        }

        private void GetMeanStemMassAndUpdateLai(int timestep)
        {
            Vector128<float> zero = Vector128<float>.Zero;
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                byte stemsNmask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(stems_n, zero));
                // mean stem mass per tree, kg
                Vector128<float> biom_tree = Avx.Divide(Avx.Multiply(biom_stem, AvxExtensions.BroadcastScalarToVector128(1000.0F)), stems_n);
                biom_tree = Avx.Blend(biom_tree, zero, stemsNmask);
                this.State.biom_tree[speciesIndex] = biom_tree;

                Vector128<float> sla = this.Trajectory.Species.SLA[speciesIndex][timestep];
                Vector128<float> biom_foliage = this.State.biom_foliage[speciesIndex];
                this.State.lai[speciesIndex] = Avx.Multiply(biom_foliage, Avx.Multiply(sla, AvxExtensions.BroadcastScalarToVector128(0.1F)));

                DebugV.Assert(Avx.CompareGreaterThanOrEqual(biom_tree, zero));
            }
        }

        private Vector128<float> GetMortality(int speciesIndex)
        {
            // calculate the mortality
            Vector128<float> oneThousand = AvxExtensions.BroadcastScalarToVector128(1000.0F);
            Vector128<float> mS = AvxExtensions.BroadcastScalarToVector128(this.Parameters.mS[speciesIndex]);
            Vector128<float> WS = Avx.Divide(this.State.biom_stem[speciesIndex], this.State.basal_area_prop[speciesIndex]);

            Vector128<float> stems_n = this.State.stems_n_ha[speciesIndex];
            Vector128<float> x1 = Avx.Multiply(oneThousand, Avx.Multiply(mS, Avx.Divide(WS, stems_n)));

            Vector128<float> wSx1000 = AvxExtensions.BroadcastScalarToVector128(this.Parameters.wSx1000[speciesIndex]);
            Vector128<float> oneMinusThinPower = AvxExtensions.BroadcastScalarToVector128(1.0F - this.Parameters.thinPower[speciesIndex]);
            Vector128<float> n = Avx.Divide(stems_n, oneThousand);

            Vector128<float> accuracy = AvxExtensions.BroadcastScalarToVector128(1.0F / 1000.0F);
            Vector128<float> oneMinus_mS = Avx.Subtract(AvxExtensions.BroadcastScalarToVector128(1.0F), mS);
            Vector128<float> zero = Vector128<float>.Zero;
            for (int iteration = 0; iteration < 6; ++iteration)
            {
                int nZeroMask = Avx.MoveMask(Avx.CompareLessThanOrEqual(n, zero)); // added in 3PG+
                
                Vector128<float> x2 = Avx.Multiply(wSx1000, MathV.Pow(n, oneMinusThinPower));
                Vector128<float> fN = Avx.Subtract(Avx.Subtract(x2, Avx.Multiply(x1, n)), Avx.Multiply(oneMinus_mS, WS));
                Vector128<float> minus_fN = Avx.Subtract(zero, fN);
                Vector128<float> dfN = Avx.Multiply(oneMinusThinPower, Avx.Divide(x2, Avx.Subtract(n, x1)));
                Vector128<float> dN = Avx.Divide(minus_fN, dfN);

                Vector128<float> dNabsoluteValue = AvxExtensions.Abs(dN);
                int accuracyMetMask = Avx.MoveMask(Avx.CompareLessThanOrEqual(dNabsoluteValue, accuracy));
                byte completedMask = (byte)(nZeroMask | accuracyMetMask);
                n = Avx.Blend(Avx.Add(n, dN), n, completedMask);

                if (accuracyMetMask == Constant.Simd128x4.MaskAllTrue)
                {
                    break;
                }
            }

            Vector128<float> mort_n = Avx.Subtract(stems_n, Avx.Multiply(oneThousand, n));
            return mort_n;
        }

        private void GetVolumeAndIncrement(int timestep)
        {
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float aV = this.Parameters.aV[speciesIndex];
                Vector128<float> volume;
                if (aV > 0.0F)
                {
                    Vector128<float> nVB = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nVB[speciesIndex]);
                    Vector128<float> dbh = this.State.dbh[speciesIndex];
                    volume = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(aV), MathV.Pow(dbh, nVB));

                    Vector128<float> height = this.State.height[speciesIndex];
                    Vector128<float> nVH = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nVH[speciesIndex]);
                    volume = Avx.Multiply(volume, MathV.Pow(height, nVH));

                    Vector128<float> dbhSquared = Avx.Multiply(dbh, dbh);
                    Vector128<float> nVBH = AvxExtensions.BroadcastScalarToVector128(this.Parameters.nVBH[speciesIndex]);
                    volume = Avx.Multiply(volume, MathV.Pow(Avx.Multiply(dbhSquared, height), nVBH));

                    Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                    volume = Avx.Multiply(volume, stems_n);
                }
                else
                {
                    Vector128<float> fracBB = this.Trajectory.Species.fracBB[speciesIndex][timestep];
                    Vector128<float> wood_density = this.Trajectory.Species.wood_density[speciesIndex][timestep];
                    Vector128<float> biom_stem = this.State.biom_stem[speciesIndex];
                    volume = Avx.Divide(Avx.Multiply(biom_stem, Avx.Subtract(one, fracBB)), wood_density);
                }
                this.State.volume[speciesIndex] = volume;

                Vector128<float> volume_change = Avx.Subtract(volume, this.State.volume_previous[speciesIndex]);
                // guarantee cumulative volume is nondecreasing, https://github.com/trotsiuk/r3PG/issues/63
                Vector128<float> zero = Vector128<float>.Zero;
                byte laiNegativeMask = (byte)Avx.MoveMask(Avx.And(Avx.CompareGreaterThan(this.State.lai[speciesIndex], zero), Avx.CompareLessThan(volume_change, zero)));
                volume_change = Avx.Blend(volume_change, zero, laiNegativeMask);

                Vector128<float> volume_cum = this.State.volume_cum[speciesIndex];
                volume_cum = Avx.Add(volume_cum, volume_change);
                this.State.volume_change[speciesIndex] = volume_change;
                this.State.volume_cum[speciesIndex] = volume_cum;
                this.State.volume_previous[speciesIndex] = volume;

                Vector128<float> age = AvxExtensions.BroadcastScalarToVector128(this.Trajectory.Species.age[speciesIndex][timestep]);
                this.State.volume_mai[speciesIndex] = Avx.Divide(volume_cum, age);
            }
        }

        private void Light3PGmix(int timestep, DateTime timestepEndDate)
        {
            // subroutine calculating apar for mixed species forest
            // It first allocates each species to a specific layer based on height and crown length
            // and then distributes light between those layers.

            // Calculate the mid crown height, crown surface and volume
            // check if species is dormant
            // where(lai(:) == 0)
            //    height(:) = 0.0F
            //    crown_length(:) = 0.0F
            // end where

            // calculate the crown area and volume
            // We only do it for species that have LAI, otherwise it stays 0 as was initialized above
            // If LAI is equal to 0, this is an indicator that the species is currently in its leaf off period.
            int n_sp = this.Species.n_sp;
            Span<Vector128<float>> crownVolumeBySpecies = stackalloc Vector128<float>[n_sp]; // **DF the crown volume of a given species
            Span<Vector128<float>> heightCrown = stackalloc Vector128<float>[n_sp]; // height of the crown begining
            Span<Vector128<float>> heightMidcrown = stackalloc Vector128<float>[n_sp]; // mean height of the middle of the crown(height - height to crown base) / 2 + height to crown base// * **DF
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> height = this.State.height[speciesIndex];
                Vector128<float> crownLength = this.State.crown_length[speciesIndex];
                heightCrown[speciesIndex] = Avx.Subtract(height, crownLength);
                heightMidcrown[speciesIndex] = Avx.Subtract(height, Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.5F), crownLength));

                Vector128<float> crownWidth = this.State.crown_width[speciesIndex];
                TreeCrownShape crownShape = this.Parameters.CrownShape[speciesIndex];
                Vector128<float> crownSA; // mean crown surface area (m²) of a species
                Vector128<float> crownVolume;
                if (crownShape == TreeCrownShape.Cone)
                {
                    Vector128<float> crownWidthSquared = Avx.Multiply(crownWidth, crownWidth);
                    crownSA = Avx.Add(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.25F * MathF.PI), crownWidthSquared),
                                      Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.5F * MathF.PI), 
                                                   Avx.Multiply(crownWidth, 
                                                                Avx.Sqrt(Avx.Add(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.25F), crownWidthSquared), Avx.Multiply(crownLength, crownLength))))));
                    crownVolume = Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(MathF.PI / 12.0F), Avx.Multiply(crownWidthSquared, crownLength));
                }
                else if (crownShape == TreeCrownShape.Ellipsoid)
                {
                    Vector128<float> half = Avx2Extensions.BroadcastScalarToVector128(0.5F);
                    Vector128<float> power = Avx2Extensions.BroadcastScalarToVector128(1.6075F);
                    Vector128<float> halfCrownLengthPower = MathV.Pow(Avx.Multiply(half, crownLength), power);
                    Vector128<float> halfCrownWidthPower = MathV.Pow(Avx.Multiply(half, crownWidth), power);
                    Vector128<float> halfCrownLengthWidthProduct = Avx.Multiply(halfCrownWidthPower, halfCrownLengthPower);
                    Vector128<float> halfCrownSum = Avx.Add(halfCrownLengthWidthProduct, halfCrownLengthWidthProduct);
                    crownSA = Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(4.0F * MathF.PI),
                                           MathV.Pow(Avx.Add(Avx.Multiply(halfCrownWidthPower, halfCrownWidthPower), 
                                                             Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(1.0F / 3.0F), halfCrownSum)),
                                                     Avx2Extensions.BroadcastScalarToVector128(1.0F / 1.6075F)));
                    crownVolume = Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(MathF.PI * 4.0F / 24.0F), Avx.Multiply(Avx.Multiply(crownWidth, crownWidth), crownLength));
                }
                else if (crownShape == TreeCrownShape.HalfEllipsoid)
                {
                    Vector128<float> power = Avx2Extensions.BroadcastScalarToVector128(1.6075F);
                    Vector128<float> crownLengthPower = MathV.Pow(crownLength, power);
                    Vector128<float> half = Avx2Extensions.BroadcastScalarToVector128(0.5F);
                    Vector128<float> halfCrownWidthPower = MathV.Pow(Avx.Multiply(half, crownWidth), power);
                    crownSA = Avx.Add(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(MathF.PI * 0.25F), 
                                                   Avx.Multiply(crownWidth, crownWidth)),
                                      Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(4.0F / 2.0F * MathF.PI),
                                                   MathV.Pow(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(1.0F / 3.0F),
                                                                          Avx.Add(Avx.Multiply(halfCrownWidthPower, halfCrownWidthPower),
                                                                                  Avx.Add(Avx.Multiply(halfCrownWidthPower, crownLengthPower),
                                                                                          Avx.Multiply(halfCrownWidthPower, crownLengthPower)))),
                                                             Avx2Extensions.BroadcastScalarToVector128(1.0F / 1.6075F))));
                    crownVolume = Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(MathF.PI * 4.0F / 24.0F),
                                               Avx.Multiply(crownWidth, Avx.Multiply(crownWidth, crownLength)));
                }
                else if (crownShape == TreeCrownShape.Rectangular)
                {
                    Vector128<float> crownWidthSquared = Avx.Multiply(crownWidth, crownWidth);
                    crownSA = Avx.Add(Avx.Add(crownWidthSquared, crownWidthSquared),
                                      Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(4.0F),
                                                   Avx.Multiply(crownWidth, crownLength)));
                    crownVolume = Avx.Multiply(crownWidthSquared, crownLength);
                }
                else
                {
                    throw new NotSupportedException("Unhandled crown shape '" + crownShape + "' for species " + speciesIndex + ".");
                }

                Vector128<float> lai = this.State.lai[speciesIndex];
                Vector128<float> zero = Vector128<float>.Zero;
                byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                crownVolume = Avx.Blend(zero, crownVolume, laiNonzeroMask);
                crownVolumeBySpecies[speciesIndex] = crownVolume;

                // calculate the ratio of tree leaf area to crown surface area restrict kLS to 1
                Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                Vector128<float> lai_sa_ratio = Avx.Divide(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(10000.0F), lai), 
                                                           Avx.Multiply(stems_n, crownSA));
                lai_sa_ratio = Avx.Blend(zero, lai_sa_ratio, laiNonzeroMask);

                this.State.lai_sa_ratio[speciesIndex] = lai_sa_ratio;
            }

            // separate trees into layers
            this.GetLayers(heightCrown);
            // if (lai[i] == 0.0F) { layer_id[i] = -1.0F; } // commented out in Fortran

            // number of layers
            Vector128<int> maxLayerID = AvxExtensions.Set128(Int32.MinValue);
            for (int speciesIndex = 0; speciesIndex < this.State.layer_id.Length; ++speciesIndex)
            {
                maxLayerID = Avx.Max(maxLayerID, this.State.layer_id[speciesIndex]);
            }
            Vector128<int> nLayers = Avx.Add(maxLayerID, AvxExtensions.Set128(1));
            DebugV.Assert(Avx.CompareGreaterThan(nLayers, Vector128<int>.Zero));
            int maxLayerCount = AvxExtensions.HorizontalMax(nLayers);

            // Now calculate the proportion of the canopy space that is filled by the crowns. The canopy space is the
            // volume between the top and bottom of a layer that is filled by crowns in that layer.
            // We calculate it only for the trees that have LAI and are present in the current month. Decidious trees
            // are in layer during their leaf off period but have zero LAI.
            Span<Vector128<float>> maxLeafedOutHeightByLayer = stackalloc Vector128<float>[maxLayerCount];
            Span<Vector128<float>> minLeafedOutCrownHeightByLayer = stackalloc Vector128<float>[maxLayerCount];
            for (int layerIndex = 0; layerIndex < maxLayerCount; ++layerIndex)
            {
                byte layerHasLeafedOutSpeciesMask = 0; // all false
                Vector128<int> layerIndex128 = AvxExtensions.Set128(layerIndex);
                Vector128<float> maxLeafedOutHeightInLayer = Avx2Extensions.BroadcastScalarToVector128(Single.MinValue);
                Vector128<float> minLeafedOutCrownHeightInLayer = Avx2Extensions.BroadcastScalarToVector128(Single.MaxValue);
                Vector128<float> zero = Vector128<float>.Zero;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    Vector128<int> layer_id = this.State.layer_id[speciesIndex];
                    int isEqualMask = Avx.MoveMask(Avx.CompareEqual(layer_id, layerIndex128).AsSingle());
                    if (isEqualMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> lai = this.State.lai[speciesIndex];
                        int laiNonzeroMask = Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                        byte combinedMask = (byte)(isEqualMask & laiNonzeroMask);
                        Vector128<float> height = this.State.height[speciesIndex];
                        maxLeafedOutHeightInLayer = Avx.Blend(maxLeafedOutHeightInLayer, Avx.Max(maxLeafedOutHeightInLayer, height), combinedMask);
                        minLeafedOutCrownHeightInLayer = Avx.Blend(minLeafedOutCrownHeightInLayer, Avx.Min(minLeafedOutCrownHeightInLayer, heightCrown[speciesIndex]), combinedMask);
                        layerHasLeafedOutSpeciesMask |= combinedMask;
                    }
                }

                maxLeafedOutHeightByLayer[layerIndex] = Avx.Blend(zero, maxLeafedOutHeightInLayer, layerHasLeafedOutSpeciesMask);
                minLeafedOutCrownHeightByLayer[layerIndex] = Avx.Blend(zero, minLeafedOutCrownHeightInLayer, layerHasLeafedOutSpeciesMask);
                // leave default values of zero if no species in layer have leaves on
            }

            Span<Vector128<float>> height_max_l = stackalloc Vector128<float>[n_sp];
            Span<Vector128<float>> heightCrown_min_l = stackalloc Vector128<float>[n_sp];
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<int> layer_id = this.State.layer_id[speciesIndex];
                
                int layer_id0 = Avx.Extract(layer_id, Constant.Simd128x4.Extract0);
                Vector128<float> maxHeightInLayer = maxLeafedOutHeightByLayer[layer_id0];
                Vector128<float> minLeafedOutCrownHeightInLayer = minLeafedOutCrownHeightByLayer[layer_id0];

                int layer_id1 = Avx.Extract(layer_id, Constant.Simd128x4.Extract1);
                maxHeightInLayer = Avx.Blend(maxHeightInLayer, maxLeafedOutHeightByLayer[layer_id1], Constant.Simd128x4.Blend1);
                minLeafedOutCrownHeightInLayer = Avx.Blend(minLeafedOutCrownHeightInLayer, minLeafedOutCrownHeightByLayer[layer_id1], Constant.Simd128x4.Blend1);

                int layer_id2 = Avx.Extract(layer_id, Constant.Simd128x4.Extract2);
                maxHeightInLayer = Avx.Blend(maxHeightInLayer, maxLeafedOutHeightByLayer[layer_id2], Constant.Simd128x4.Blend2);
                minLeafedOutCrownHeightInLayer = Avx.Blend(minLeafedOutCrownHeightInLayer, minLeafedOutCrownHeightByLayer[layer_id2], Constant.Simd128x4.Blend2);

                int layer_id3 = Avx.Extract(layer_id, Constant.Simd128x4.Extract3);
                maxHeightInLayer = Avx.Blend(maxHeightInLayer, maxLeafedOutHeightByLayer[layer_id3], Constant.Simd128x4.Blend3);
                minLeafedOutCrownHeightInLayer = Avx.Blend(minLeafedOutCrownHeightInLayer, minLeafedOutCrownHeightByLayer[layer_id3], Constant.Simd128x4.Blend3);

                height_max_l[speciesIndex] = maxHeightInLayer;
                heightCrown_min_l[speciesIndex] = minLeafedOutCrownHeightInLayer;
            }

            // sum the canopy volume fraction per layer and save it at each species
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> lai = this.State.lai[speciesIndex];
                Vector128<float> crownVolume = crownVolumeBySpecies[speciesIndex];
                Vector128<float> stems_n = this.State.stems_n[speciesIndex];
                Vector128<float> canopyVolumeFraction = Avx.Divide(Avx.Multiply(crownVolume, stems_n),
                                                                   Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(10000.0F),
                                                                                Avx.Subtract(height_max_l[speciesIndex], heightCrown_min_l[speciesIndex])));

                Vector128<float> zero = Vector128<float>.Zero;
                byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                canopyVolumeFraction = Avx.Blend(zero, canopyVolumeFraction, laiNonzeroMask);
                this.State.canopy_vol_frac[speciesIndex] = canopyVolumeFraction;

                DebugV.Assert(Avx.CompareGreaterThanOrEqual(canopyVolumeFraction, zero));
            }
            Span<Vector128<float>> canopy_vol_frac_buffer = stackalloc Vector128<float>[n_sp];
            this.GetLayerSum(maxLayerCount, this.State.canopy_vol_frac, canopy_vol_frac_buffer);
            canopy_vol_frac_buffer.CopyTo(this.State.canopy_vol_frac);

            Span<Vector128<float>> heightMidcrown_r = stackalloc Vector128<float>[n_sp]; // ratio of the mid height of the crown of a given species to the mid height of a canopy layer
            Span<Vector128<float>> kL_l = stackalloc Vector128<float>[n_sp]; // sum of k x L for all species within the given layer
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // if the canopy volume fraction is < 0.01(very small seedlings) then it is outside the range of the model there is no need for lambda_h so, make canopy_vol_frac = 0.01
                // where(canopy_vol_frac[i] < 0.01F) { canopy_vol_frac[i] = 0.01F } // commented out in Fortran
                // minimum height of layer
                Vector128<float> heightMidcrown_l = Avx.Add(heightCrown_min_l[speciesIndex],
                                                            Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.5F),
                                                                         Avx.Subtract(height_max_l[speciesIndex], heightCrown_min_l[speciesIndex])));

                // determine the ratio between the mid height of the given species and the mid height of the layer.
                Vector128<float> midheightRatio = Avx.Divide(heightMidcrown[speciesIndex], heightMidcrown_l);
                heightMidcrown_r[speciesIndex] = midheightRatio;

                // Calculate the sum of kL for all species in a layer
                Vector128<float> k = Avx2Extensions.BroadcastScalarToVector128(this.Parameters.k[speciesIndex]);
                Vector128<float> lai = this.State.lai[speciesIndex];
                kL_l[speciesIndex] = Avx.Multiply(k, lai);

                DebugV.Assert(Avx.Or(Avx.CompareEqual(this.State.lai[speciesIndex], Vector128<float>.Zero), Avx.And(Avx.CompareGreaterThanOrEqual(midheightRatio, Vector128<float>.Zero), Avx.CompareGreaterThan(heightMidcrown_l, Vector128<float>.Zero))));
            }
            Span<Vector128<float>> kL_l_buffer = stackalloc Vector128<float>[n_sp];
            this.GetLayerSum(maxLayerCount, kL_l, kL_l_buffer);
            kL_l = kL_l_buffer;

            // vertical 
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> k = Avx2Extensions.BroadcastScalarToVector128(this.Parameters.k[speciesIndex]);
                Vector128<float> lai = this.State.lai[speciesIndex];
                // Constant to partition light between species and to account for vertical canopy heterogeneity
                // (see Equations 2 and 3 of Forrester et al., 2014, Forest Ecosystems, 1:17)
                Vector128<float> speciesLambdaV = Avx.Add(Avx2Extensions.BroadcastScalarToVector128(0.012306F),
                                                          Avx.Add(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.2366090F),
                                                                               Avx.Multiply(k, 
                                                                                            Avx.Divide(lai, kL_l[speciesIndex]))),
                                                                  Avx.Add(Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.029118F),
                                                                                       heightMidcrown_r[speciesIndex]),
                                                                          Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.608381F),
                                                                                       Avx.Multiply(k, 
                                                                                                    Avx.Divide(lai, 
                                                                                                               Avx.Multiply(kL_l[speciesIndex], heightMidcrown_r[speciesIndex])))))));

                // check for leaf off
                Vector128<float> zero = Vector128<float>.Zero;
                byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai, zero));
                speciesLambdaV = Avx.Blend(zero, speciesLambdaV, laiNonzeroMask);
                this.State.lambda_v[speciesIndex] = speciesLambdaV;

                DebugV.Assert(Avx.And(Avx.CompareGreaterThanOrEqual(speciesLambdaV, Vector128<float>.Zero), Avx.CompareLessThan(speciesLambdaV, Avx2Extensions.BroadcastScalarToVector128(Single.PositiveInfinity))));
            }

            // make sure the sum of all lambda_v = 1 in each leafed out layer
            Span<Vector128<float>> lambdaV_l = stackalloc Vector128<float>[n_sp]; // sum of lambda_v per layer
            this.GetLayerSum(maxLayerCount, this.State.lambda_v, lambdaV_l);
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> lambda_v = this.State.lambda_v[speciesIndex];
                Vector128<float> lambda_v_normalized = Avx.Divide(lambda_v, lambdaV_l[speciesIndex]);
                byte lambdaV_l_nonzeroMask = (byte)Avx.MoveMask(Avx.CompareNotEqual(lambdaV_l[speciesIndex], Vector128<float>.Zero));
                this.State.lambda_v[speciesIndex] = Avx.Blend(lambda_v, lambda_v_normalized, lambdaV_l_nonzeroMask);
            }
            DebugV.Assert(Avx.And(Avx.CompareGreaterThanOrEqual(this.State.lambda_v.Sum(), Vector128<float>.Zero), Avx.CompareLessThan(this.State.lambda_v.Sum(), Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(1.0001F), nLayers.AsSingle())))); // minimum is zero if no layers are leafed out, should be one otherwise

            // calculate the weighted kLS based on kL / sumkL
            Span<Vector128<float>> kLSweightedave = stackalloc Vector128<float>[n_sp]; // calculates the contribution each species makes to the sum of all kLS products in a given layer(see Equation 6 of Forrester et al., 2014, Forest Ecosystems, 1:17)
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> k = Avx2Extensions.BroadcastScalarToVector128(this.Parameters.k[speciesIndex]);
                Vector128<float> kSquared = Avx.Multiply(k, k);
                Vector128<float> lai = this.State.lai[speciesIndex];
                Vector128<float> lai_sa_ratio = this.State.lai_sa_ratio[speciesIndex];
                kLSweightedave[speciesIndex] = Avx.Multiply(kSquared, Avx.Multiply(lai_sa_ratio, Avx.Divide(lai, kL_l[speciesIndex])));
            }
            Span<Vector128<float>> kLSweightedAverageBuffer = stackalloc Vector128<float>[n_sp];
            this.GetLayerSum(maxLayerCount, kLSweightedave, kLSweightedAverageBuffer);
            kLSweightedave = kLSweightedAverageBuffer;

            // the kLS should not be greater than 1(based on the data used to fit the light model in Forrester et al. 2014)
            // This is because when there is a high k then LS is likely to be small.
            float solarAngle = this.State.GetSolarZenithAngle(timestepEndDate);
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                kLSweightedave[speciesIndex] = Avx.Min(kLSweightedave[speciesIndex], Avx2Extensions.BroadcastScalarToVector128(1.0F));

                // Constant to account for horizontal canopy heterogeneity such as gaps between trees and the change
                // in zenith angle (and shading) with latitude and season.
                //   Forrester DI, Guisasola R, Tang X, et al. 2014. Using a stand-level model to predict light
                //     absorption in stands with vertically and horizontally heterogeneous canopies. Forest
                //     Ecosystems 1:17. https://doi.org/10.1186/s40663-014-0017-0
                // Equations 5a and 5b (used in Equation 2).
                // horizontal heterogeneity, 3-PGmix manual 11.1 equations A21 and A22
                Vector128<float> canopy_vol_frac = this.State.canopy_vol_frac[speciesIndex];
                Vector128<float> powerCanopyVolFrac = MathV.Pow(Avx2Extensions.BroadcastScalarToVector128(0.1F), canopy_vol_frac);
                Vector128<float> speciesLambdaH = Avx.Add(Avx2Extensions.BroadcastScalarToVector128(0.8285F),
                                                          Avx.Subtract(Avx.Multiply(Avx.Subtract(Avx2Extensions.BroadcastScalarToVector128(1.09498F),
                                                                                                 Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.781928F),
                                                                                                              kLSweightedave[speciesIndex])),
                                                                                    powerCanopyVolFrac),
                                                                       Avx.Multiply(Avx2Extensions.BroadcastScalarToVector128(0.6714096F), 
                                                                                    powerCanopyVolFrac)));
                if (solarAngle > 30.0F)
                {
                    speciesLambdaH = Avx.Add(speciesLambdaH, Avx2Extensions.BroadcastScalarToVector128(0.00097F * MathF.Pow(1.08259F, solarAngle)));
                }

                // check for leaf off
                Vector128<float> zero = Vector128<float>.Zero;
                byte laiNonzeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(this.State.lai[speciesIndex], zero));
                speciesLambdaH = Avx.Blend(zero, speciesLambdaH, laiNonzeroMask);
                this.State.lambda_h[speciesIndex] = speciesLambdaH;

                DebugV.Assert(Avx.And(Avx.CompareGreaterThanOrEqual(speciesLambdaH, Vector128<float>.Zero), Avx.CompareLessThanOrEqual(speciesLambdaH, Avx2Extensions.BroadcastScalarToVector128(1.25F))));
            }

            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            Vector128<float> RADt = Avx2Extensions.BroadcastScalarToVector128(solar_rad * days_in_month); // total available radiation, MJ m⁻² month⁻¹
            Vector128<float> one = Avx2Extensions.BroadcastScalarToVector128(1.0F);
            Span<Vector128<float>> aparl = stackalloc Vector128<float>[n_sp]; // the absorbed apar for the given  layer
            for (int layerIndex = 0; layerIndex < maxLayerCount; ++layerIndex)
            {
                Vector128<int> layerIndex128 = AvxExtensions.Set128(layerIndex);
                Vector128<float> zero = Vector128<float>.Zero;
                Vector128<float> maxAParL = zero;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    byte isEqualMask = (byte)Avx.MoveMask(Avx.CompareEqual(this.State.layer_id[speciesIndex], layerIndex128).AsSingle());
                    if (isEqualMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        Vector128<float> aparlInLayer = Avx.Multiply(RADt, Avx.Subtract(one, MathV.Exp(Avx.Subtract(zero, kL_l[speciesIndex]))));
                        Vector128<float> aparlBlend = Avx.Blend(aparlInLayer, aparl[speciesIndex], isEqualMask);
                        aparl[speciesIndex] = aparlBlend;
                        maxAParL = Avx.Max(maxAParL, aparlBlend);
                    }
                }
                RADt = Avx.Subtract(RADt, maxAParL); // subtract the layer RAD from total
                DebugV.Assert(Avx.CompareGreaterThanOrEqual(RADt, zero));
            }

            Vector128<float> RADtReciprocal = Avx2Extensions.BroadcastScalarToVector128(1.0F / (solar_rad * days_in_month));
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // ***DF this used to have month in it but this whole sub is run each month so month is now redundant here.
                Vector128<float> lambda_h = this.State.lambda_h[speciesIndex];
                Vector128<float> lambda_v = this.State.lambda_v[speciesIndex];
                Vector128<float> aparOfSpecies = Avx.Multiply(aparl[speciesIndex], Avx.Multiply(lambda_h, lambda_v));
                this.State.apar[speciesIndex] = aparOfSpecies;

                // The proportion of above canopy apar absorbed by each species. This is used for net radiation calculations in the gettranspiration sub
                Vector128<float> speciesAparFraction = Avx.Multiply(aparOfSpecies, RADtReciprocal);
                this.State.fi[speciesIndex] = speciesAparFraction;

                DebugV.Assert(Avx.And(Avx.CompareGreaterThanOrEqual(aparOfSpecies, Vector128<float>.Zero), Avx.And(Avx.CompareGreaterThanOrEqual(speciesAparFraction, Vector128<float>.Zero), Avx.CompareLessThanOrEqual(speciesAparFraction, Avx2Extensions.BroadcastScalarToVector128(1.08F)))));
            }

            // calculate the LAI above the given species for within canopy VPD calculations
            Span<Vector128<float>> LAI_l = stackalloc Vector128<float>[n_sp]; // Layer LAI
            this.GetLayerSum(maxLayerCount, this.State.lai, LAI_l);

            // now calculate the LAI of all layers above and part of the current layer if the species
            // is in the lower half of the layer then also take the proportion of the LAI above
            // the proportion is based on the Relative height of the mid crown
            Vector128<float> justUnderOne = Avx2Extensions.BroadcastScalarToVector128(0.9999999999999F);
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> zero = Vector128<float>.Zero;
                Vector128<float> lai_above = zero;
                Vector128<float> sameLayerLai = zero;
                for (int otherSpeciesIndex = 0; otherSpeciesIndex < n_sp; ++otherSpeciesIndex)
                {
                    Vector128<int> thisLayerID = this.State.layer_id[speciesIndex];
                    Vector128<int> otherLayerID = this.State.layer_id[otherSpeciesIndex];
                    Vector128<float> otherSpeciesLai = this.State.lai[otherSpeciesIndex];
                    byte isLessThanMask = (byte)Avx.MoveMask(Avx.CompareLessThan(otherLayerID, thisLayerID).AsSingle());
                    if (isLessThanMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        lai_above = Avx.Blend(lai_above, Avx.Add(lai_above, otherSpeciesLai), isLessThanMask);
                    }
                    byte isEqualMask = (byte)Avx.MoveMask(Avx.CompareEqual(otherLayerID, thisLayerID).AsSingle());
                    if (isEqualMask != Constant.Simd128x4.MaskAllFalse)
                    {
                        sameLayerLai = Avx.Blend(sameLayerLai, Avx.Add(sameLayerLai, otherSpeciesLai), isEqualMask);
                    }
                }

                sameLayerLai = Avx.Multiply(sameLayerLai, Avx.Subtract(one, heightMidcrown_r[speciesIndex]));
                byte midcrownMask = (byte)Avx.MoveMask(Avx.CompareLessThan(heightMidcrown_r[speciesIndex], justUnderOne));
                sameLayerLai = Avx.Blend(zero, sameLayerLai, midcrownMask);
                lai_above = Avx.Add(lai_above, sameLayerLai);
                this.State.lai_above[speciesIndex] = lai_above;
            }
        }

        private void Light3PGpjs(int timestep, DateTime timestepEndDate)
        {
            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            Vector128<float> RADt = AvxExtensions.BroadcastScalarToVector128(solar_rad * days_in_month); // MJ m⁻² month⁻¹, total available radiation

            int n_sp = this.Species.n_sp;
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float fullCanAgeScalar = this.Parameters.fullCanAge[speciesIndex];
                Vector128<float> canopy_cover = one;
                if (fullCanAgeScalar > 0.0F)
                {
                    Vector128<float> age = AvxExtensions.BroadcastScalarToVector128(this.Trajectory.Species.age_m[speciesIndex][timestep]);
                    Vector128<float> fullCanAge = AvxExtensions.BroadcastScalarToVector128(fullCanAgeScalar);
                    byte ageMask = (byte)Avx.MoveMask(Avx.CompareLessThan(age, fullCanAge));
                    if (ageMask > 0)
                    {
                        Vector128<float> partialCanopyCover = Avx.Divide(Avx.Add(age, AvxExtensions.BroadcastScalarToVector128(0.01F)), fullCanAge);
                        canopy_cover = Avx.Blend(canopy_cover, partialCanopyCover, ageMask);
                    }
                }
                this.State.canopy_cover[speciesIndex] = canopy_cover;

                Vector128<float> minus_k = AvxExtensions.BroadcastScalarToVector128(-this.Parameters.k[speciesIndex]);
                Vector128<float> lai = this.State.lai[speciesIndex];
                Vector128<float> lightIntcptn = Avx.Subtract(one, MathV.Exp(Avx.Divide(Avx.Multiply(minus_k, lai), canopy_cover)));

                this.State.apar[speciesIndex] = Avx.Multiply(RADt, Avx.Multiply(lightIntcptn, canopy_cover));
            }
        }

        private Vector128<float> Transpiration3PGmix(int timestep, DateTime timestepEndDate, Vector128<float> conduct_soil)
        {
            // Species level calculations ---
            // the within canopy aero_resist and VPDspecies have been calculated using information from the light
            // submodel and from the calculation of the modifiers.The netrad for each species is calculated using
            // the fi (proportion of PAR absorbed by the given species) and is calculated by the light submodel.
            float day_length = this.State.GetDayLength(timestepEndDate);
            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];

            int n_sp = this.Species.n_sp;
            Vector128<float> onePlusE20 = AvxExtensions.BroadcastScalarToVector128(1.0F + Constant.e20);
            Vector128<float> rhoAirLambdaVpdConv = AvxExtensions.BroadcastScalarToVector128(Constant.rhoAir * Constant.lambda * Constant.VPDconv);
            Vector128<float> zero = Vector128<float>.Zero;
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                Vector128<float> transp_veg = zero;
                byte laiZeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(this.State.lai[speciesIndex], zero));
                if (laiZeroMask > 0)
                {
                    // SolarRad in MJ / m2 / day---> * 10 ^ 6 J / m2 / day---> / day_length converts to only daytime period--->W / m2
                    float Qa = this.Parameters.Qa[speciesIndex];
                    float Qb = this.Parameters.Qb[speciesIndex];
                    Vector128<float> netRad = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(Qa + Qb * (solar_rad * 1000.0F * 1000.0F / day_length)), this.State.fi[speciesIndex]);
                    // netRad[speciesIndex] = max(netRad[speciesIndex], 0.0F)// net radiation can't be negative
                    DebugV.Assert(Avx.CompareGreaterThan(netRad, AvxExtensions.BroadcastScalarToVector128(-100.0F)));

                    Vector128<float> aero_resist = this.State.aero_resist[speciesIndex];
                    Vector128<float> defTerm = Avx.Multiply(rhoAirLambdaVpdConv, Avx.Divide(this.State.VPD_sp[speciesIndex], aero_resist));
                    Vector128<float> conduct_canopy = this.State.conduct_canopy[speciesIndex];
                    Vector128<float> div = Avx.Add(Avx.Multiply(conduct_canopy, onePlusE20), Avx.Divide(AvxExtensions.BroadcastScalarToVector128(1.0F), aero_resist));
                    
                    transp_veg = Avx.Multiply(Avx.Divide(Avx.Multiply(conduct_canopy, Avx.Add(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(Constant.e20), netRad), defTerm)), div), AvxExtensions.BroadcastScalarToVector128(days_in_month / Constant.lambda * day_length));
                    // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
                    transp_veg = Avx.Blend(transp_veg, zero, laiZeroMask);
                }
                this.State.transp_veg[speciesIndex] = transp_veg;
            }

            // now get the soil evaporation(soil aero_resist = 5 * lai_total, and VPD of soil = VPD * Exp(lai_total * -Log(2) / 5))
            // ending `so` mean soil
            Vector128<float> lai_total = this.State.lai.Sum();
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
            byte laiTotalZeroMask = (byte)Avx.MoveMask(Avx.CompareGreaterThan(lai_total, zero));
            Vector128<float> fiveLaiTotalReciprocal = Avx.Blend(Avx.Divide(one, Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(5.0F), lai_total)),
                                                                one,
                                                                laiTotalZeroMask);

            float vpd_day = this.Climate.MeanDailyVpd[timestep];
            Vector128<float> defTerm_so = Avx.Multiply(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(Constant.rhoAir * Constant.lambda * Constant.VPDconv * vpd_day),
                                                                    MathV.Exp(Avx.Multiply(lai_total, AvxExtensions.BroadcastScalarToVector128(-Constant.Ln2 / 5.0F)))),
                                                       fiveLaiTotalReciprocal);
            Vector128<float> div_so = Avx.Add(Avx.Multiply(conduct_soil, AvxExtensions.BroadcastScalarToVector128(1.0F + Constant.e20)), fiveLaiTotalReciprocal);

            float soilQa = this.Parameters.Qa[0]; // https://github.com/trotsiuk/r3PG/issues/67
            float soilQb = this.Parameters.Qb[0];
            Vector128<float> netRad_so = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(soilQa + soilQb * (solar_rad * 1000.0F * 1000.0F / day_length)), Avx.Subtract(one, this.State.fi.Sum()));
            // SolarRad in MJ / m2 / day---> * 10 ^ 6 J / m2 / day---> / day_length converts to only daytime period--->W / m2

            Vector128<float> evapotra_soil = Avx.Multiply(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(days_in_month / Constant.lambda * day_length), conduct_soil), Avx.Divide(Avx.Add(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(Constant.e20), netRad_so), defTerm_so), div_so));
            // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
            DebugV.Assert(Avx.And(Avx.CompareGreaterThan(evapotra_soil, AvxExtensions.BroadcastScalarToVector128(-12.0F)), Avx.CompareGreaterThan(netRad_so, AvxExtensions.BroadcastScalarToVector128(-120.0F))));
            return evapotra_soil;
        }

        private void Transpiration3PGpjs(int timestep, DateTime timestepEndDate)
        {
            Vector128<float> vpdSpSum = this.State.VPD_sp.Sum();
            byte zeroVpdMask = (byte)Avx.MoveMask(Avx.CompareEqual(vpdSpSum, Vector128<float>.Zero));
            if (zeroVpdMask == Constant.Simd128x4.MaskAllTrue)
            {
                Array.Clear(this.State.transp_veg);
                return;
            }

            float day_length = this.State.GetDayLength(timestepEndDate);
            Vector128<float> days_in_month = AvxExtensions.BroadcastScalarToVector128((float)timestepEndDate.DaysInMonth());
            Vector128<float> lambdaDayLengthReciprocal = AvxExtensions.BroadcastScalarToVector128(1.0F / (Constant.lambda * day_length));
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            Vector128<float> rhoAirLambdaVpdConv = AvxExtensions.BroadcastScalarToVector128(Constant.rhoAir * Constant.lambda * Constant.VPDconv);
            Vector128<float> onePlusE20 = AvxExtensions.BroadcastScalarToVector128(1.0F + Constant.e20);
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                // SolarRad in MJ / m² / day ---> * 10^6 J / m² / day ---> / day_length converts to only daytime period ---> W / m²
                float Qa = this.Parameters.Qa[speciesIndex];
                float Qb = this.Parameters.Qb[speciesIndex];
                float netRad = Qa + Qb * (solar_rad * 1000.0F * 1000.0F / day_length);
                Debug.Assert(netRad > -100.0F);
                // netRad(:) = max(netRad(:), 0.0F) // net radiation can't be negative

                Vector128<float> BLcond = AvxExtensions.BroadcastScalarToVector128(this.Parameters.BLcond[speciesIndex]);
                Vector128<float> VPD_sp = this.State.VPD_sp[speciesIndex];
                Vector128<float> defTerm = Avx.Multiply(rhoAirLambdaVpdConv, Avx.Multiply(BLcond, VPD_sp));
                Vector128<float> conduct_canopy = this.State.conduct_canopy[speciesIndex];
                Vector128<float> div = Avx.Add(Avx.Multiply(conduct_canopy, onePlusE20), BLcond);

                Vector128<float> transp_veg = Avx.Multiply(Avx.Divide(Avx.Multiply(Avx.Multiply(days_in_month, conduct_canopy), Avx.Add(AvxExtensions.BroadcastScalarToVector128(Constant.e20 * netRad), defTerm)), div), lambdaDayLengthReciprocal);
                // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
                Vector128<float> zero = Vector128<float>.Zero;
                transp_veg = Avx.Max(transp_veg, zero); // transpiration can't be negative
                transp_veg = Avx.Blend(transp_veg, zero, zeroVpdMask); // zero transp_veg where VPD sum is zero
                this.State.transp_veg[speciesIndex] = transp_veg;
            }
        }
    }
}