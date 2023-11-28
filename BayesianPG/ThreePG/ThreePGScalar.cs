using BayesianPG.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace BayesianPG.ThreePG
{
    public class ThreePGScalar(Site site, SiteClimate climate, SiteTreeSpecies species, TreeSpeciesParameters<float> parameters, TreeSpeciesManagement management, ThreePGSettings settings) 
        : ThreePGpjsMix<float, int>(site, climate, species, parameters, management, settings)
    {
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
                        f_tmp = (tmp_ave - Tmin) / (Topt - Tmin) * MathF.Pow((Tmax - tmp_ave) / (Tmax - Topt), (Tmax - Topt) / (Topt - Tmin));
                    }
                    this.Trajectory.Species.f_tmp[timestepIndex, speciesIndex] = f_tmp;

                    // calculate temperature response function to apply to gc
                    // Uses mean of Tx and Tav instead of Tav.
                    //   Feikema PM, et al. 2010. Validation of plantation transpiration in south-eastern Australia estimated using
                    //   the 3PG+ forest growth model. Forest Ecology And Management 260:663-678. https://doi.org/10.1016/j.foreco.2010.05.022
                    float tmp_max = this.Climate.MeanDailyTempMax[timestepIndex];
                    float f_tmp_gc;
                    if (((tmp_ave + tmp_max) / 2 <= Tmin) || ((tmp_ave + tmp_max) / 2 >= Tmax))
                    {
                        f_tmp_gc = 0.0F;
                    }
                    else
                    {
                        f_tmp_gc = ((tmp_ave + tmp_max) / 2 - Tmin) / (Topt - Tmin) * MathF.Pow((Tmax - (tmp_ave + tmp_max) / 2) / (Tmax - Topt), (Tmax - Topt) / (Topt - Tmin));
                    }
                    this.Trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex] = f_tmp_gc;

                    // frost modifier
                    float kF = this.Parameters.kF[speciesIndex];
                    float frost_days = this.Climate.FrostDays[timestepIndex];
                    float daysInMonth = timestepEndDate.DaysInMonth();
                    // float f_frost = 1.0F - kF * (frost_days / 30.0F); // https://github.com/trotsiuk/r3PG/issues/68
                    float f_frost = MathF.Max(0.0F, MathF.Min(1.0F - kF * (frost_days / daysInMonth), 1.0F));
                    this.Trajectory.Species.f_frost[timestepIndex, speciesIndex] = f_frost;

                    // CO₂ modifiers
                    float atmosphericCO2 = this.Climate.AtmosphericCO2[timestepIndex];
                    float fCalpha = fCalphax[speciesIndex] * atmosphericCO2 / (350.0F * (fCalphax[speciesIndex] - 1.0F) + atmosphericCO2);
                    this.Trajectory.Species.f_calpha[timestepIndex, speciesIndex] = fCalpha;
                    float fCg = fCg0[speciesIndex] / (1.0F + (fCg0[speciesIndex] - 1.0F) * atmosphericCO2 / 350.0F);
                    this.Trajectory.Species.f_cg[timestepIndex, speciesIndex] = fCg;

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
                    this.State.swConst[speciesIndex] = 0.8F - 0.10F * this.Site.SoilClass;
                    this.State.swPower[speciesIndex] = 11.0F - 2.0F * this.Site.SoilClass;
                }
                else if (this.Site.SoilClass < 0.0F)
                {
                    // use supplied parameters
                    this.State.swConst[speciesIndex] = this.Parameters.SWconst0[speciesIndex];
                    this.State.swPower[speciesIndex] = this.Parameters.SWpower0[speciesIndex];
                }
                else
                {
                    // no soil-water effects
                    this.State.swConst[speciesIndex] = 999.0F;
                    this.State.swPower[speciesIndex] = this.Parameters.SWpower0[speciesIndex];
                }
            }

            // initial available soil water must be between min and max ASW
            this.State.aSW = MathF.Max(MathF.Min(this.Site.AvailableSoilWaterInitial, this.Site.AvailableSoilWaterMax), this.Site.AvailableSoilWaterMin);
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
                float pfsPower = 0.434294481903252F * MathF.Log(pFS20 / pFS2); // 1 / MathF.Log(20.0F / 2.0F) = 0.434294481903252;
                this.State.pfsPower[speciesIndex] = pfsPower;

                this.State.pfsConst[speciesIndex] = pFS2 / MathF.Pow(2.0F, pfsPower);
            }

            // INITIALISATION (Age dependent)---------------------
            // Calculate species specific modifiers
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float[] age = new float[this.Trajectory.MonthCount];
                float[] age_m = new float[this.Trajectory.MonthCount];
                for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
                {
                    float ageInMonths = 12.0F * (this.Site.From.Year - this.Species.YearPlanted[speciesIndex]) + this.Site.From.Month - this.Species.MonthPlanted[speciesIndex] - 1.0F;
                    float ageInYears = (ageInMonths + timestepIndex + 1) / 12.0F;

                    age[timestepIndex] = 12.0F * (this.Site.From.Year - this.Species.YearPlanted[speciesIndex]) + this.Site.From.Month - this.Species.MonthPlanted[speciesIndex] - 1.0F; // age of this tree species in months
                    age[timestepIndex] = (age[timestepIndex] + timestepIndex + 1) / 12.0F; // translate to years
                    age_m[timestepIndex] = age[timestepIndex] - 1.0F / 12.0F;
                }
                age_m[0] = age[0];

                this.Trajectory.Species.age[speciesIndex] = age;
                this.Trajectory.Species.age_m[speciesIndex] = age_m;

                float sla0 = this.Parameters.SLA0[speciesIndex];
                float sla1 = this.Parameters.SLA1[speciesIndex];
                float tsla = this.Parameters.tSLA[speciesIndex];
                this.Trajectory.Species.SLA[speciesIndex] = ThreePGScalar.GetAgeDependentParameter(age_m, sla0, sla1, tsla, 2.0F);

                float fracBB0 = this.Parameters.fracBB0[speciesIndex];
                float fracBB1 = this.Parameters.fracBB1[speciesIndex];
                float tBB = this.Parameters.tBB[speciesIndex];
                this.Trajectory.Species.fracBB[speciesIndex] = ThreePGScalar.GetAgeDependentParameter(age_m, fracBB0, fracBB1, tBB, 1.0F);

                float rho0 = this.Parameters.rho0[speciesIndex];
                float rho1 = this.Parameters.rho1[speciesIndex];
                float tRho = this.Parameters.tRho[speciesIndex];
                this.Trajectory.Species.wood_density[speciesIndex] = ThreePGScalar.GetAgeDependentParameter(age_m, rho0, rho1, tRho, 1.0F);

                float gammaN0 = this.Parameters.gammaN0[speciesIndex];
                float gammaN1 = this.Parameters.gammaN1[speciesIndex];
                float tgammaN = this.Parameters.tgammaN[speciesIndex];
                float ngammaN = this.Parameters.ngammaN[speciesIndex];
                this.Trajectory.Species.gammaN[speciesIndex] = ThreePGScalar.GetAgeDependentParameter(age, gammaN0, gammaN1, tgammaN, ngammaN); // age instead of age_m (per Fortran)

                float gammaF1 = this.Parameters.gammaF1[speciesIndex];
                float gammaF0 = this.Parameters.gammaF0[speciesIndex];
                float tgammaF = this.Parameters.tgammaF[speciesIndex];
                this.Trajectory.Species.gammaF[speciesIndex] = ThreePGScalar.GetLitterfallRate(age_m, gammaF1, gammaF0, tgammaF);

                // age modifier
                if (this.Parameters.nAge[speciesIndex] == 0.0F)
                {
                    for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
                    {
                        this.Trajectory.Species.f_age[timestepIndex, speciesIndex] = 1.0F;
                    }
                }
                else
                {
                    float MaxAge = this.Parameters.MaxAge[speciesIndex];
                    float rAge = this.Parameters.rAge[speciesIndex];
                    float nAge = this.Parameters.nAge[speciesIndex];
                    for (int timestepIndex = 0; timestepIndex < this.Trajectory.MonthCount; ++timestepIndex)
                    {
                        this.Trajectory.Species.f_age[timestepIndex, speciesIndex] = 1.0F / (1.0F + MathF.Pow(age_m[timestepIndex] / (MaxAge * rAge), nAge));
                    }
                }
            }

            // INITIALISATION (Stand)---------------------
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float age = this.Trajectory.Species.age[speciesIndex][0];
                if (age >= 0.0F)
                {
                    this.State.stems_n[speciesIndex] = this.Species.InitialStemsPerHectare[speciesIndex];
                    this.State.biom_stem[speciesIndex] = this.Species.InitialStemBiomass[speciesIndex];
                    this.State.biom_foliage[speciesIndex] = this.Species.InitialFoliageBiomass[speciesIndex];
                    this.State.biom_root[speciesIndex] = this.Species.InitialRootBiomass[speciesIndex];
                }
                else
                {
                    this.State.stems_n[speciesIndex] = 0.0F;
                    this.State.biom_stem[speciesIndex] = 0.0F;
                    this.State.biom_foliage[speciesIndex] = 0.0F;
                    this.State.biom_root[speciesIndex] = 0.0F;
                }
            }

            // reset any applied treatments
            // This is redundant the first time a stand is simulated as t_n defaults to zero but is necessary on
            // subsequent calls.
            Array.Clear(this.State.t_n);

            // check if this is the dormant period or previous/following period is dormant
            // to allocate foliage if needed, etc.
            float competition_total = 0.0F;
            int monthOfYear = this.Site.From.Month;
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                // if this is a dormant month
                if (this.IsDormant(monthOfYear, speciesIndex))
                {
                    this.State.biom_foliage_debt[speciesIndex] = this.State.biom_foliage[speciesIndex];
                    this.State.biom_foliage[speciesIndex] = 0.0F;
                }

                // initial stand characteristics
                float age = this.Trajectory.Species.age[speciesIndex][0];
                if (age >= 0.0F)
                {
                    float stems_n = this.State.stems_n[speciesIndex];
                    float biom_stem = this.State.biom_stem[speciesIndex];
                    float meanStemBiomassInKg = biom_stem * 1000.0F / stems_n;
                    this.State.biom_tree[speciesIndex] = meanStemBiomassInKg;

                    float aWS = this.Parameters.aWS[speciesIndex];
                    float nWS = this.Parameters.nWS[speciesIndex];
                    float meanDbh = MathF.Pow(meanStemBiomassInKg / aWS, 1.0F / nWS);
                    this.State.dbh[speciesIndex] = meanDbh;

                    float basalAreaPerHa = MathF.PI * 0.0001F * meanDbh * meanDbh / 4.0F * stems_n; // m²/ha
                    this.State.basal_area[speciesIndex] = basalAreaPerHa;

                    float sla = this.Trajectory.Species.SLA[speciesIndex][0];
                    float biom_foliage = this.State.biom_foliage[speciesIndex];
                    this.State.lai[speciesIndex] = biom_foliage * sla * 0.1F;

                    Debug.Assert((meanStemBiomassInKg >= 0.0F) && (meanDbh >= 0.0F) && (basalAreaPerHa >= 0.0F));
                }
                
                float wood_density = this.Trajectory.Species.wood_density[speciesIndex][0];
                competition_total += wood_density * this.State.basal_area[speciesIndex];
            }
            this.State.competition_total = competition_total;

            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float aH = this.Parameters.aH[speciesIndex];
                float nHB = this.Parameters.nHB[speciesIndex];
                float nHC = this.Parameters.nHC[speciesIndex];
                float dbh = this.State.dbh[speciesIndex];
                float height = this.Settings.height_model switch
                {
                    // ThreePGHeightModel.Power => aH * MathF.Pow(dbh, nHB) * MathF.Pow(competition_total, nHC),
                    ThreePGHeightModel.Power => aH * MathF.Exp(MathF.Log(dbh) * nHB + MathF.Log(competition_total) * nHC),
                    ThreePGHeightModel.Exponent => 1.3F + aH * MathF.Exp(-nHB / dbh) + nHC * competition_total * dbh,
                    _ => throw new NotSupportedException("Unhandled height model " + this.Settings.height_model + ".")
                };
                this.State.height[speciesIndex] = height;
            }

            // correct the bias
            this.CorrectSizeDistribution(timestep: 0);

            // present in fortran but has no effect since height_max is not used
            // float height_max = Single.MinValue;
            // for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            // {
            //     float lai = this.State.lai[speciesIndex];
            //     float height = this.State.height[speciesIndex];
            //     if ((lai > 0.0F) && (height > height_max))
            //     {
            //         height_max = height;
            //     }
            // }

            // volume and volume increment
            // Call main function to get volume and then fix up cumulative volume and MAI.
            this.GetVolumeAndIncrement(timestep: 0);
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float initialVolume = this.State.volume[speciesIndex];
                this.State.volume_cum[speciesIndex] = initialVolume;

                float age = this.Trajectory.Species.age[speciesIndex][0];
                float volume_mai = 0.0F;
                if (age > 0.0F)
                {
                    volume_mai = initialVolume / age;
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
            bool correctSizeDistribution = false;
            DateTime timestepEndDate = this.Site.From;
            for (int timestep = 1; timestep < this.Trajectory.MonthCount; ++timestep) // first month is initial month and set up above
            {
                // move to next month
                timestepEndDate = timestepEndDate.AddMonths(1);

                // add any new cohorts ----------------------------------------------------------------------
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float age = this.Trajectory.Species.age[speciesIndex][timestep];
                    if (age == 0.0F)
                    {
                        this.State.stems_n[speciesIndex] = this.Species.InitialStemsPerHectare[speciesIndex];
                        this.State.biom_stem[speciesIndex] = this.Species.InitialStemBiomass[speciesIndex];
                        this.State.biom_foliage[speciesIndex] = this.Species.InitialFoliageBiomass[speciesIndex];
                        this.State.biom_root[speciesIndex] = this.Species.InitialRootBiomass[speciesIndex];
                        correctSizeDistribution = true;
                    }
                }

                // Test for deciduous leaf off ----------------------------------------------------------------------
                // If this is first month after dormancy we need to make potential LAI, so the
                // PAR absorbption can be applied, otherwise it will be zero.
                // In the end of the month we will re-calculate it based on the actual values.
                int monthOfYear = timestepEndDate.Month;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    if (this.IsDormant(monthOfYear, speciesIndex) == false)
                    {
                        if (this.IsDormant(monthOfYear - 1, speciesIndex) == true)
                        {
                            float sla = this.Trajectory.Species.SLA[speciesIndex][timestep];
                            float biom_foliage_debt = this.State.biom_foliage_debt[speciesIndex];
                            this.State.lai[speciesIndex] = biom_foliage_debt * sla * 0.1F;
                            correctSizeDistribution = true;
                        }
                    }

                    // if this is first dormant month, then set WF to 0 and move everything to the debt
                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                    {
                        if (this.IsDormant(monthOfYear - 1, speciesIndex) == false)
                        {
                            this.State.biom_foliage_debt[speciesIndex] = this.State.biom_foliage[speciesIndex];
                            this.State.biom_foliage[speciesIndex] = 0.0F;
                            this.State.lai[speciesIndex] = 0.0F;
                            correctSizeDistribution = true;
                        }
                    }
                }

                if (correctSizeDistribution == true)
                {
                    this.CorrectSizeDistribution(timestep);
                    correctSizeDistribution = false;
                }

                // Radiation and assimilation ----------------------------------------------------------------------
                if (this.Settings.light_model == ThreePGModel.Pjs27)
                {
                    this.Light3PGpjs(timestep, timestepEndDate);
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        this.State.VPD_sp[speciesIndex] = this.Climate.MeanDailyVpd[timestep];
                    }
                }
                else if (this.Settings.light_model == ThreePGModel.Mix)
                {
                    // calculate the absorbed PAR
                    this.Light3PGmix(timestep, timestepEndDate);
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        float lai_above = this.State.lai_above[speciesIndex];
                        float cVPD = this.Parameters.cVPD[speciesIndex];
                        this.State.VPD_sp[speciesIndex] = this.Climate.MeanDailyVpd[timestep] * MathF.Exp(lai_above * (-Constant.Ln2) / cVPD);
                    }
                }
                else
                {
                    throw new NotSupportedException("Unhandled light model " + this.Settings.light_model + ".");
                }

                // determine various environmental modifiers which were not calculated before
                // calculate VPD modifier
                // Get within-canopy climatic conditions this is exponential function
                float height_max = Single.MinValue;
                float lai_total = 0.0F;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float lai = this.State.lai[speciesIndex];
                    if (lai > 0.0F)
                    {
                        float height = this.State.height[speciesIndex];
                        height_max = MathF.Max(height_max, height);
                        lai_total += lai;
                    }
                }

                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float aero_resist = 0.0F;
                    float lai = this.State.lai[speciesIndex];
                    if (lai > 0.0F) // check for leaf off
                    {
                        float blCondReciprocal = 1.0F / this.Parameters.BLcond[speciesIndex];
                        float height = this.State.height[speciesIndex];
                        aero_resist = blCondReciprocal; // if this is the (currently) tallest species
                        if (height < height_max)
                        {
                            float twiceRelativeHeight = height / (height_max / 2.0F);
                            aero_resist += (5.0F * lai_total - blCondReciprocal) *
                                MathF.Exp(-Constant.Ln2 * twiceRelativeHeight * twiceRelativeHeight);
                        }
                    }
                    this.State.aero_resist[speciesIndex] = aero_resist;

                    float coeffCond = this.Parameters.CoeffCond[speciesIndex];
                    float VPD_sp = this.State.VPD_sp[speciesIndex];
                    float f_vpd = MathF.Exp(-coeffCond * VPD_sp);
                    this.State.f_vpd[speciesIndex] = f_vpd;

                    // soil water modifier
                    float swConst = this.State.swConst[speciesIndex];
                    float swPower = this.State.swPower[speciesIndex];
                    float availableSoilWaterMax = this.Site.AvailableSoilWaterMax;
                    float f_sw = 1.0F / (1.0F + MathF.Pow((1.0F - this.State.aSW / availableSoilWaterMax) / swConst, swPower));
                    this.State.f_sw[speciesIndex] = f_sw;

                    // soil nutrition modifier
                    float f_nutr;
                    if (this.Parameters.fNn[speciesIndex] == 0.0F)
                    {
                        f_nutr = 1.0F;
                    }
                    else
                    {
                        float fN0 = this.Parameters.fN0[speciesIndex];
                        float fNn = this.Parameters.fNn[speciesIndex];
                        float fertility = this.Species.SoilFertility[speciesIndex];
                        f_nutr = 1.0F - (1.0F - fN0) * MathF.Pow(1.0F - fertility, fNn);
                    }
                    this.State.f_nutr[speciesIndex] = f_nutr;

                    // calculate physiological modifier applied to conductance and alphaCx.
                    float f_age = this.Trajectory.Species.f_age[timestep, speciesIndex];
                    float f_phys;
                    if (this.Settings.phys_model == ThreePGModel.Pjs27)
                    {
                        f_phys = MathF.Min(f_vpd, f_sw) * f_age;
                        this.Trajectory.Species.f_tmp_gc[timestep, speciesIndex] = 1.0F;
                    }
                    else if (this.Settings.phys_model == ThreePGModel.Mix)
                    {
                        f_phys = f_vpd * f_sw * f_age;
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled model " + this.Settings.phys_model + ".");
                    }
                    this.State.f_phys[speciesIndex] = f_phys;

                    Debug.Assert((f_vpd >= 0.0F) && (f_vpd <= 1.0F) &&
                                 (f_sw >= 0.0F) && (f_sw <= 1.0F) &&
                                 (f_nutr >= 0.0F) && (f_nutr <= 1.0F) &&
                                 (f_phys >= 0.0F) && (f_phys <= 1.0F));

                    // calculate assimilation before the water balance is done
                    float alphaC = 0.0F;
                    if (this.State.lai[speciesIndex] > 0.0F)
                    {
                        float alphaCx = this.Parameters.alphaCx[speciesIndex];
                        float f_tmp = this.Trajectory.Species.f_tmp[timestep, speciesIndex];
                        float f_frost = this.Trajectory.Species.f_frost[timestep, speciesIndex];
                        float f_calpha = this.Trajectory.Species.f_calpha[timestep, speciesIndex];
                        alphaC = alphaCx * f_nutr * f_tmp * f_frost * f_calpha * f_phys;
                    }
                    this.State.alpha_c[speciesIndex] = alphaC;

                    float gDM_mol = this.Parameters.gDM_mol[speciesIndex];
                    float molPAR_MJ = this.Parameters.molPAR_MJ[speciesIndex];
                    float epsilon = gDM_mol * molPAR_MJ * alphaC;
                    this.State.epsilon[speciesIndex] = epsilon;

                    float gpp = epsilon * this.State.apar[speciesIndex] / 100.0F; // tDM / ha(apar is MJ / m ^ 2);
                    this.State.GPP[speciesIndex] = gpp;
                    float Y = this.Parameters.Y[speciesIndex]; // assumes respiratory rate is constant
                    this.State.NPP[speciesIndex] = gpp * Y;

                    // Water Balance ----------------------------------------------------------------------
                    // Calculate each species' proportion.
                    float lai_per = 0.0F;
                    if (lai_total > 0.0F)
                    {
                        lai_per = this.State.lai[speciesIndex] / lai_total;
                    }
                    this.State.lai_per[speciesIndex] = lai_per;

                    // calculate conductance
                    float LAIgcx = this.Parameters.LAIgcx[speciesIndex];
                    float gC = this.Parameters.MaxCond[speciesIndex];
                    if (lai_total <= LAIgcx) // TODO: single species case?
                    {
                        float MinCond = this.Parameters.MinCond[speciesIndex];
                        float MaxCond = this.Parameters.MaxCond[speciesIndex];
                        gC = MinCond + (MaxCond - MinCond) * lai_total / LAIgcx;
                    }
                    this.State.gC[speciesIndex] = gC;

                    //float f_phys = this.state.f_phys[speciesIndex];
                    float f_tmp_gc = this.Trajectory.Species.f_tmp_gc[timestep, speciesIndex];
                    float f_cg = this.Trajectory.Species.f_cg[timestep, speciesIndex];
                    float conduct_canopy = gC * lai_per * f_phys * f_tmp_gc * f_cg;
                    this.State.conduct_canopy[speciesIndex] = conduct_canopy;
                }
                float conduct_soil = Constant.MaxSoilCond * this.State.aSW / this.Site.AvailableSoilWaterMax;
                this.Trajectory.conduct_soil[timestep] = conduct_soil;

                // calculate transpiration
                float evapotra_soil = 0.0F;
                if (this.Settings.transp_model == ThreePGModel.Pjs27)
                {
                    this.Transpiration3PGpjs(timestep, timestepEndDate);
                }
                else if (this.Settings.transp_model == ThreePGModel.Mix)
                {
                    evapotra_soil = this.Transpiration3PGmix(timestep, timestepEndDate, conduct_soil);
                }
                else
                {
                    throw new NotSupportedException("Unhandled model " + this.Settings.transp_model + ".");
                }

                float transp_total = this.State.transp_veg.Sum() + evapotra_soil;

                this.Trajectory.evapotra_soil[timestep] = evapotra_soil;

                // rainfall interception
                float prcp_interc_total = 0.0F;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float maxIntcptn = this.Parameters.MaxIntcptn[speciesIndex];
                    float prcp_interc_fract = maxIntcptn;
                    float laiMaxIntcptn = this.Parameters.LAImaxIntcptn[speciesIndex];
                    if (laiMaxIntcptn > 0.0F)
                    {
                        float lai_per = this.State.lai_per[speciesIndex];
                        prcp_interc_fract = maxIntcptn * MathF.Min(1.0F, lai_total / laiMaxIntcptn) * lai_per;
                    }

                    float prcp_interc = this.Climate.TotalPrecipitation[timestep] * prcp_interc_fract;
                    prcp_interc_total += prcp_interc;

                    this.Trajectory.Species.prcp_interc[timestep, speciesIndex] = prcp_interc;
                }

                // soil water balance
                float irrigation = 0.0F; // standing monthly irrigation, need to constrain irrigation only to the growing season.
                float water_runoff_pooled = 0.0F; // pooling and ponding not currently supported
                float poolFractn = MathF.Max(0.0F, MathF.Min(1.0F, 0.0F)); // determines fraction of excess water that remains on site
                float aSW = this.State.aSW + this.Climate.TotalPrecipitation[timestep] + (100.0F * irrigation / 12.0F) + water_runoff_pooled;
                float evapo_transp = MathF.Min(aSW, transp_total + prcp_interc_total); // ET can not exceed ASW
                float excessSW = MathF.Max(aSW - evapo_transp - this.Site.AvailableSoilWaterMax, 0.0F);
                aSW = aSW - evapo_transp - excessSW;
                water_runoff_pooled = poolFractn * excessSW;

                float irrig_supl = 0.0F;
                if (aSW < this.Site.AvailableSoilWaterMin)
                {
                    irrig_supl = this.Site.AvailableSoilWaterMin - aSW;
                    aSW = this.Site.AvailableSoilWaterMin;
                }
                this.Trajectory.irrig_supl[timestep] = irrig_supl;
                this.Trajectory.prcp_runoff[timestep] = (1.0F - poolFractn) * excessSW;

                Debug.Assert((aSW >= this.Site.AvailableSoilWaterMin) && (aSW <= this.Site.AvailableSoilWaterMax) && (evapo_transp > -1.0F) && (excessSW >= 0.0F) && (prcp_interc_total >= 0.0F) && (transp_total > -7.5F) && (water_runoff_pooled >= 0.0F));
                this.State.aSW = aSW;
                this.Trajectory.AvailableSoilWater[timestep] = aSW;
                this.Trajectory.evapo_transp[timestep] = evapo_transp;

                float f_transp_scale;
                if ((transp_total + prcp_interc_total) == 0)
                {
                    // this might be close to 0 if the only existing species is dormant during this month
                    // (it will include the soil evaporation if Apply3PGpjswaterbalance = no)
                    f_transp_scale = 1.0F;
                }
                else
                {
                    f_transp_scale = evapo_transp / (transp_total + prcp_interc_total); // scales NPP and GPP
                }
                this.Trajectory.f_transp_scale[timestep] = f_transp_scale;

                // correct for actual ET
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    this.State.GPP[speciesIndex] *= f_transp_scale;
                    float npp = this.State.NPP[speciesIndex];
                    npp *= f_transp_scale;
                    this.State.NPP[speciesIndex] = npp;
                    this.State.NPP_f[speciesIndex] = npp;

                    if ((transp_total > 0.0F) && (f_transp_scale < 1.0F))
                    {
                        // a different scaler is required for transpiration because all of the scaling needs
                        // to be done to the transpiration and not to the RainIntcpth, which occurs regardless of the growth
                        float transpVeg = this.State.transp_veg[speciesIndex];
                        transpVeg *= (evapo_transp - prcp_interc_total) / transp_total;
                        this.State.transp_veg[speciesIndex] = transpVeg;

                        evapotra_soil = (evapo_transp - prcp_interc_total) / transp_total * evapotra_soil;
                    }

                    // NEED TO CROSS CHECK THIS PART, DON'T FULLY AGREE WITH IT
                    float wue;
                    if ((evapo_transp != 0.0F) && (this.Species.n_sp == 1))
                    {
                        // in case ET is zero
                        // Also, for mixtures it is not possible to calculate WUE based on ET because the soil
                        // evaporation cannot simply be divided between species.
                        wue = 100.0F * npp / evapo_transp;
                    }
                    else
                    {
                        wue = 0.0F;
                    }
                    this.State.WUE[speciesIndex] = wue;

                    float transp_veg = this.State.transp_veg[speciesIndex];
                    float wue_transp = 0.0F;
                    if (transp_veg > 0.0F)
                    {
                        wue_transp = 100.0F * npp / transp_veg;
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
                        float GPP = this.State.GPP[speciesIndex];
                        float gDM_mol = this.Parameters.gDM_mol[speciesIndex];
                        float GPP_molsec = GPP * 100.0F / (daysInMonth * 24.0F * 3600.0F * gDM_mol);

                        // canopy conductance for water vapour in mol / m2s, unit conversion(CanCond is m / s)
                        float conduct_canopy = this.State.conduct_canopy[speciesIndex];
                        float tmp_ave = this.Climate.MeanDailyTemp[timestep];
                        float Gw_mol = conduct_canopy * 44.6F * (273.15F / (273.15F + tmp_ave)) * (this.State.airPressure / 101.3F);
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
                        float stems_n = this.State.stems_n[speciesIndex];
                        float crown_width_025 = this.State.crown_width[speciesIndex] + 0.25F;
                        float canopy_cover = stems_n * crown_width_025 * crown_width_025 / 10000.0F;
                        if (canopy_cover > 1.0F)
                        {
                            canopy_cover = 1.0F;
                        }
                        this.State.canopy_cover[speciesIndex] = canopy_cover;

                        float RGcGw = this.Parameters.RGcGw[speciesIndex];
                        float Gc_mol = Gw_mol * RGcGw / MathF.Max(0.0000001F, canopy_cover);
                        this.State.Gc_mol[speciesIndex] = Gc_mol;

                        // default values for dormancy
                        float InterCi = 0.0F;
                        float D13CNewPS = 0.0F;
                        float D13CTissue = 0.0F;
                        if (Gc_mol != 0.0F)
                        {
                            float co2 = 0.000001F * this.Climate.AtmosphericCO2[timestep];
                            // calculating monthly average intercellular CO₂ concentration.Ci = Ca - A / g
                            InterCi = co2 - GPP_molsec / Gc_mol;
                            
                            // calculating monthly d13C of new photosynthate, = d13Catm - a - (b - a)(ci / ca)
                            float d13Catm = this.Climate.D13Catm[timestep];
                            float aFracDiffu = this.Parameters.aFracDiffu[speciesIndex];
                            float bFracRubi = this.Parameters.bFracRubi[speciesIndex];
                            D13CNewPS = d13Catm - aFracDiffu - (bFracRubi - aFracDiffu) * (InterCi / co2);

                            float d13CTissueDif = this.Parameters.D13CTissueDif[speciesIndex];
                            D13CTissue = D13CNewPS + d13CTissueDif;
                        }

                        this.State.InterCi[speciesIndex] = InterCi;
                        this.State.D13CNewPS[speciesIndex] = D13CNewPS;
                        this.State.D13CTissue[speciesIndex] = D13CTissue;
                    }
                }

                // Biomass increment and loss module ----------------------------------------------
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    // determine biomass increments and losses
                    float m0 = this.Parameters.m0[speciesIndex];
                    float fertility = this.Species.SoilFertility[speciesIndex];
                    float m = m0 + (1.0F - m0) * fertility;
                    this.State.m[speciesIndex] = m;

                    float pRx = this.Parameters.pRx[speciesIndex];
                    float pRn = this.Parameters.pRn[speciesIndex];
                    float f_phys = this.State.f_phys[speciesIndex];
                    float npp_fract_root = pRx * pRn / (pRn + (pRx - pRn) * f_phys * m);
                    this.State.npp_fract_root[speciesIndex] = npp_fract_root;

                    float pFS = this.State.pFS[speciesIndex];
                    float npp_fract_stem = (1.0F - npp_fract_root) / (1.0F + pFS);
                    this.State.npp_fract_stem[speciesIndex] = npp_fract_stem;

                    this.State.npp_fract_foliage[speciesIndex] = 1.0F - npp_fract_root - npp_fract_stem;

                    // Dormant period -----------
                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                    {
                        // if this is the first dormant period then there is litterfall
                        float biom_loss_foliage = 0.0F;
                        if (this.IsDormant(monthOfYear - 1, speciesIndex))
                        {
                            biom_loss_foliage = this.State.biom_foliage_debt[speciesIndex];
                        }
                        this.State.biom_loss_foliage[speciesIndex] = biom_loss_foliage;

                        this.State.biom_loss_root[speciesIndex] = 0.0F;

                        // no growth during leaf off
                        this.State.biom_incr_foliage[speciesIndex] = 0.0F;
                        this.State.biom_incr_root[speciesIndex] = 0.0F;
                        this.State.biom_incr_stem[speciesIndex] = 0.0F;
                    }
                    else
                    {
                        // if there are some leaves to be grown put NPP first to the leaf growth
                        float biom_foliage = this.State.biom_foliage[speciesIndex];
                        if (biom_foliage == 0.0F)
                        {
                            biom_foliage = this.State.biom_foliage_debt[speciesIndex];
                            this.State.biom_foliage[speciesIndex] = biom_foliage;
                        }

                        float biom_foliage_debt = this.State.biom_foliage_debt[speciesIndex];
                        float npp = this.State.NPP[speciesIndex];
                        if (npp >= biom_foliage_debt)
                        {
                            // there is enough NPP to regrow all of the leaves
                            npp -= biom_foliage_debt;
                            biom_foliage_debt = 0.0F;
                        }
                        else
                        {
                            // if there is not enough NPP to regrow all the leaves regrow what can be regrown and continue leafout in dex month
                            biom_foliage_debt -= npp;
                            npp = 0.0F;
                        }
                        this.State.biom_foliage_debt[speciesIndex] = biom_foliage_debt;
                        this.State.NPP[speciesIndex] = npp;

                        // biomass loss
                        float gammaF = this.Trajectory.Species.gammaF[speciesIndex][timestep];
                        float biom_loss_foliage = gammaF * biom_foliage;

                        float gammaR = this.Parameters.gammaR[speciesIndex];
                        float biom_root = this.State.biom_root[speciesIndex];
                        float biom_loss_root = gammaR * biom_root;

                        this.State.biom_loss_foliage[speciesIndex] = biom_loss_foliage;
                        this.State.biom_loss_root[speciesIndex] = biom_loss_root;

                        // biomass increments
                        float biom_incr_foliage = npp * this.State.npp_fract_foliage[speciesIndex];
                        float biom_incr_root = npp * this.State.npp_fract_root[speciesIndex];
                        float biom_incr_stem = npp * this.State.npp_fract_stem[speciesIndex];

                        this.State.biom_incr_foliage[speciesIndex] = biom_incr_foliage;
                        this.State.biom_incr_root[speciesIndex] = biom_incr_root;
                        this.State.biom_incr_stem[speciesIndex] = biom_incr_stem;

                        // end-of-month biomass
                        this.State.biom_foliage[speciesIndex] = biom_foliage + biom_incr_foliage - biom_loss_foliage;
                        this.State.biom_root[speciesIndex] = biom_root + biom_incr_root - biom_loss_root;
                        float biom_stem = this.State.biom_stem[speciesIndex];
                        this.State.biom_stem[speciesIndex] = biom_stem + biom_incr_stem;
                    }
                }

                // correct the bias
                this.GetMeanStemMassAndUpdateLai(timestep);
                this.CorrectSizeDistribution(timestep);

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
                            float stems_n = this.State.stems_n[speciesIndex];
                            float target_stems_n = this.Management.stems_n[speciesIndex][thinningIndex];
                            if (stems_n > target_stems_n)
                            {
                                float mort_manag = (stems_n - target_stems_n) / stems_n;
                                stems_n *= 1.0F - mort_manag;
                                this.State.mort_manag[speciesIndex] = mort_manag;
                                this.State.stems_n[speciesIndex] = stems_n;

                                // if the stand is thinned from above, then the ratios(F, R and S) of stem,
                                // foliage and roots to be removed relative to the mean tree in the stand
                                // will be > 1.If the product of this ratio and delN is > 1 then the new
                                // WF, WR or WS will be< 0, which is impossible.Therefore, make sure this is >= 0.
                                float stemFraction = this.Management.stem[speciesIndex][thinningIndex];
                                float rootFraction = this.Management.root[speciesIndex][thinningIndex];
                                float foliageFraction = this.Management.foliage[speciesIndex][thinningIndex];
                                float maxMort = mort_manag * MathF.Max(MathF.Max(stemFraction, rootFraction), foliageFraction);
                                if (maxMort > 1.0F)
                                {
                                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                                    {
                                        this.State.biom_foliage_debt[speciesIndex] = 0.0F;
                                    }
                                    else
                                    {
                                        this.State.biom_foliage[speciesIndex] = 0.0F;
                                    }

                                    this.State.biom_root[speciesIndex] = 0.0F;
                                    this.State.biom_stem[speciesIndex] = 0.0F;
                                    this.State.stems_n[speciesIndex] = 0.0F;
                                }
                                else
                                {
                                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                                    {
                                        this.State.biom_foliage_debt[speciesIndex] *= 1.0F - mort_manag * foliageFraction;
                                    }
                                    else
                                    {
                                        this.State.biom_foliage[speciesIndex] *= 1.0F - mort_manag * foliageFraction;
                                    }

                                    this.State.biom_root[speciesIndex] *= 1.0F - mort_manag * rootFraction;
                                    this.State.biom_stem[speciesIndex] *= 1.0F - mort_manag * stemFraction;
                                }

                                correctSizeDistribution = true;
                            }

                            this.State.t_n[speciesIndex] = thinningIndex + 1;
                        }
                    }
                }

                // correct the bias
                if (correctSizeDistribution == true)
                {
                    this.GetMeanStemMassAndUpdateLai(timestep);
                    this.CorrectSizeDistribution(timestep);

                    // update volume for thinning
                    this.GetVolumeAndIncrement(timestep);
                    correctSizeDistribution = false;
                }

                // Mortality--------------------------------------------------------------------------
                // Stress related ------------------
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    if (this.IsDormant(monthOfYear, speciesIndex) == false)
                    {
                        float gammaN = this.Trajectory.Species.gammaN[speciesIndex][timestep];
                        if (gammaN > 0.0F)
                        {
                            float stems_n = this.State.stems_n[speciesIndex];
                            float mort_stress = gammaN * stems_n / 12.0F / 100.0F;
                            // mort_stress[i] = ceiling(mort_stress[i]); // commented in Fortran
                            mort_stress = MathF.Min(mort_stress, stems_n); // mortality can't be more than available
                            this.State.mort_stress[speciesIndex] = mort_stress;

                            float mF = this.Parameters.mF[speciesIndex];
                            this.State.biom_foliage[speciesIndex] -= mF * mort_stress * (this.State.biom_foliage[speciesIndex] / stems_n);
                            float mR = this.Parameters.mR[speciesIndex];
                            this.State.biom_root[speciesIndex] -= mR * mort_stress * (this.State.biom_root[speciesIndex] / stems_n);
                            float mS = this.Parameters.mS[speciesIndex];
                            this.State.biom_stem[speciesIndex] -= mS * mort_stress * (this.State.biom_stem[speciesIndex] / stems_n);

                            this.State.stems_n[speciesIndex] -= mort_stress;

                            correctSizeDistribution = true;
                        }
                    }
                    else
                    {
                        this.State.mort_stress[speciesIndex] = 0.0F;
                    }
                }

                // correct the bias
                if (correctSizeDistribution == true)
                {
                    this.GetMeanStemMassAndUpdateLai(timestep);
                    this.CorrectSizeDistribution(timestep);
                    correctSizeDistribution = false;
                }

                // self-thinning ------------------
                float totalBasalArea = this.State.basal_area.Sum();
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float basal_area_prop = this.State.basal_area[speciesIndex] / totalBasalArea;
                    this.State.basal_area_prop[speciesIndex] = basal_area_prop;
                    // basal_area_prop[i] if basal_area_prop[i] > 0 and basal_area_prop[i] < 0.01 put 0.01
                    // where(lai[i] > 0.0F.and.basal_area_prop[i] < 0.01F) basal_area_prop[i] = 0.01F
                    float stems_n = this.State.stems_n[speciesIndex];
                    float stems_n_ha = stems_n / basal_area_prop;
                    this.State.stems_n_ha[speciesIndex] = stems_n_ha;

                    float wSx1000 = this.Parameters.wSx1000[speciesIndex];
                    float thinPower = this.Parameters.thinPower[speciesIndex];
                    this.State.biom_tree_max[speciesIndex] = wSx1000 * MathF.Pow(1000.0F / stems_n_ha, thinPower);

                    if (this.IsDormant(monthOfYear, speciesIndex) == false)
                    {
                        if (this.State.biom_tree_max[speciesIndex] < this.State.biom_tree[speciesIndex])
                        {
                            float mort_thinn = this.GetMortality(speciesIndex) * basal_area_prop;
                            this.State.mort_thinn[speciesIndex] = mort_thinn;

                            // if (stems_n[i] < 1.0F) mort_thinn[i] = stems_n[i]
                            // mort_thinn[i] = ceiling(mort_thinn[i])

                            if (mort_thinn < stems_n)
                            {
                                float biom_foliage = this.State.biom_foliage[speciesIndex];
                                float mF = this.Parameters.mF[speciesIndex];
                                this.State.biom_foliage[speciesIndex] = biom_foliage - mF * mort_thinn * (biom_foliage / stems_n);

                                float biom_root = this.State.biom_root[speciesIndex];
                                float mR = this.Parameters.mR[speciesIndex];
                                this.State.biom_root[speciesIndex] = biom_root - mR * mort_thinn * (biom_root / stems_n);

                                float biom_stem = this.State.biom_stem[speciesIndex];
                                float mS = this.Parameters.mS[speciesIndex];
                                this.State.biom_stem[speciesIndex] = biom_stem - mS * mort_thinn * (biom_stem / stems_n);
                                this.State.stems_n[speciesIndex] -= mort_thinn;
                            }
                            else
                            {
                                this.State.biom_foliage[speciesIndex] = 0.0F;
                                this.State.biom_root[speciesIndex] = 0.0F;
                                this.State.biom_stem[speciesIndex] = 0.0F;
                                this.State.stems_n[speciesIndex] = 0.0F;
                            }

                            correctSizeDistribution = true;
                        }
                    }
                    else
                    {
                        this.State.mort_thinn[speciesIndex] = 0.0F;
                    }
                }

                // correct the bias
                if (correctSizeDistribution == true)
                {
                    this.GetMeanStemMassAndUpdateLai(timestep);
                    this.CorrectSizeDistribution(timestep);
                    correctSizeDistribution = false;
                }

                // Additional calculations ------------------
                totalBasalArea = this.State.basal_area.Sum();
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    this.State.basal_area_prop[speciesIndex] = this.State.basal_area[speciesIndex] / totalBasalArea;

                    // efficiency
                    float apar = this.State.apar[speciesIndex];
                    float epsilon_gpp = 0.0F;
                    float epsilon_npp = 0.0F;
                    float epsilon_biom_stem = 0.0F;
                    if (apar != 0.0F)
                    {
                        epsilon_gpp = 100.0F * this.State.GPP[speciesIndex] / apar;
                        epsilon_npp = 100.0F * this.State.NPP_f[speciesIndex] / apar;
                        epsilon_biom_stem = 100.0F * this.State.biom_incr_stem[speciesIndex] / apar;
                    }
                    this.State.epsilon_gpp[speciesIndex] = epsilon_gpp;
                    this.State.epsilon_npp[speciesIndex] = epsilon_npp;
                    this.State.epsilon_biom_stem[speciesIndex] = epsilon_biom_stem;
                }

                // copy species-specific state into stand trajectory: capture remaining end of month state
                this.Trajectory.Species.SetMonth(timestep, State);
            }
        }

        private void CorrectSizeDistribution(int timestep)
        {
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
                float lai_total = this.State.lai.Sum();
                float standHeight = 0.0F;
                float totalStems = 0.0F;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    float stems_n = this.State.stems_n[speciesIndex];
                    standHeight += this.State.height[speciesIndex] * stems_n;
                    totalStems += stems_n;
                }
                standHeight /= totalStems;

                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    // calculate the relative height
                    // float height = aH[speciesIndex] * dbh[speciesIndex] * *nHB[speciesIndex] * competition_total[speciesIndex] * *nHC[speciesIndex]
                    float height_rel = this.State.height[speciesIndex] / standHeight;
                    this.State.height_rel[speciesIndex] = height_rel;
                }

                if (this.Settings.CorrectSizeDistribution)
                {
                    if (this.Bias == null)
                    {
                        throw new InvalidOperationException("Size distribution corrections are enabled in settings but size distributions are not specified.");
                    }

                    // Calculate the DW scale -------------------
                    float lnCompetitionTotal = MathF.Log(this.State.competition_total);
                    for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                    {
                        float age = this.Trajectory.Species.age[speciesIndex][timestep];
                        if (age == 0.0F)
                        {
                            // log of age is minus infinity so bias correction is not possible
                            // All Weibull values end up being NaN. Zeroing the bias corrections for trees of age
                            // zero is handled below this if() block.
                            continue;
                        }

                        float lnAge = MathF.Log(age);
                        float lnDbh = MathF.Log(this.State.dbh[speciesIndex]);
                        float lnRelativeHeight = MathF.Log(this.State.height_rel[speciesIndex]);

                        float Dscale0 = this.Bias.Dscale0[speciesIndex];
                        float DscaleB = this.Bias.DscaleB[speciesIndex];
                        float Dscalerh = this.Bias.Dscalerh[speciesIndex];
                        float Dscalet = this.Bias.Dscalet[speciesIndex];
                        float DscaleC = this.Bias.DscaleC[speciesIndex];
                        float DWeibullScale = MathF.Exp(Dscale0 + DscaleB * lnDbh + Dscalerh * lnRelativeHeight + Dscalet * lnAge + DscaleC * lnCompetitionTotal);
                        this.State.DWeibullScale[speciesIndex] = DWeibullScale;

                        float Dshape0 = this.Bias.Dshape0[speciesIndex];
                        float DshapeB = this.Bias.DshapeB[speciesIndex];
                        float Dshaperh = this.Bias.Dshaperh[speciesIndex];
                        float Dshapet = this.Bias.Dshapet[speciesIndex];
                        float DshapeC = this.Bias.DshapeC[speciesIndex];
                        float DWeibullShape = MathF.Exp(Dshape0 + DshapeB * lnDbh + Dshaperh * lnRelativeHeight + Dshapet * lnAge + DshapeC * lnCompetitionTotal);
                        this.State.DWeibullShape[speciesIndex] = DWeibullShape;

                        float DWeibullShape_gamma = ThreePGScalar.GammaDistribution(1.0F + 1.0F / DWeibullShape);

                        float Dlocation0 = this.Bias.Dlocation0[speciesIndex];
                        float DlocationB = this.Bias.DlocationB[speciesIndex];
                        float Dlocationrh = this.Bias.Dlocationrh[speciesIndex];
                        float Dlocationt = this.Bias.Dlocationt[speciesIndex];
                        float DlocationC = this.Bias.DlocationC[speciesIndex];
                        float DWeibullLocation;
                        if ((Dlocation0 == 0.0F) && (DlocationB == 0.0F) && (Dlocationrh == 0.0F) &&
                            (Dlocationt == 0.0F) && (DlocationC == 0.0F))
                        {
                            float dbh = this.State.dbh[speciesIndex];
                            DWeibullLocation = MathF.Round(dbh) - 1.0F - DWeibullScale * DWeibullShape_gamma;
                        }
                        else
                        {
                            DWeibullLocation = MathF.Exp(Dlocation0 + DlocationB * lnDbh + Dlocationrh * lnRelativeHeight + Dlocationt * lnAge + DlocationC * lnCompetitionTotal);
                        }
                        if (DWeibullLocation < 0.01F)
                        {
                            DWeibullLocation = 0.01F;
                        }
                        this.State.DWeibullLocation[speciesIndex] = DWeibullLocation;

                        // Weibull expected value (3-PGmix user manual 11.10 equation A50)
                        float Ex = DWeibullLocation + DWeibullScale * DWeibullShape_gamma;
                        // now convert the Ex from weibull scale to actual scale of diameter units in cm
                        float Varx = DWeibullScale * DWeibullScale * (ThreePGScalar.GammaDistribution(1.0F + 2.0F / DWeibullShape) - DWeibullShape_gamma * DWeibullShape_gamma);
                        // Weibull coefficient of variation
                        float CVdbhDistribution = MathF.Sqrt(Varx) / Ex;
                        this.State.CVdbhDistribution[speciesIndex] = CVdbhDistribution;

                        // calculate the bias (3-PGmix user manual 11.10 equation A49)
                        // prevent unrealistically large biases by restricting to ±50%
                        float pfsPower = this.State.pfsPower[speciesIndex];
                        float DrelBiaspFS = 0.5F * (pfsPower * (pfsPower - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiaspFS = ThreePGScalar.Limit(DrelBiaspFS, -0.5F, 0.5F);
                        this.State.DrelBiaspFS[speciesIndex] = DrelBiaspFS;

                        float nHB = this.Parameters.nHB[speciesIndex];
                        float DrelBiasheight = 0.5F * (nHB * (nHB - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasheight = ThreePGScalar.Limit(DrelBiasheight, -0.5F, 0.5F);
                        this.State.DrelBiasheight[speciesIndex] = DrelBiasheight;

                        float DrelBiasBasArea = 0.5F * (2.0F * (2.0F - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasBasArea = ThreePGScalar.Limit(DrelBiasBasArea, -0.5F, 0.5F);
                        this.State.DrelBiasBasArea[speciesIndex] = DrelBiasBasArea;

                        float nHLB = this.Parameters.nHLB[speciesIndex];
                        float DrelBiasLCL = 0.5F * (nHLB * (nHLB - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasLCL = ThreePGScalar.Limit(DrelBiasLCL, -0.5F, 0.5F);
                        this.State.DrelBiasLCL[speciesIndex] = DrelBiasLCL;

                        float nKB = this.Parameters.nKB[speciesIndex];
                        float DrelBiasCrowndiameter = 0.5F * (nKB * (nKB - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasCrowndiameter = ThreePGScalar.Limit(DrelBiasCrowndiameter, -0.5F, 0.5F);
                        this.State.DrelBiasCrowndiameter[speciesIndex] = DrelBiasCrowndiameter;

                        // calculate the biom_stem scale -------------------
                        float wsscale0 = this.Bias.wsscale0[speciesIndex];
                        float wsscaleB = this.Bias.wsscaleB[speciesIndex];
                        float wsscalerh = this.Bias.wsscalerh[speciesIndex];
                        float wsscalet = this.Bias.wsscalet[speciesIndex];
                        float wsscaleC = this.Bias.wsscaleC[speciesIndex];
                        float wsWeibullScale = MathF.Exp(wsscale0 + wsscaleB * lnDbh + wsscalerh * lnRelativeHeight + wsscalet * lnAge + wsscaleC * lnCompetitionTotal);
                        this.State.wsWeibullScale[speciesIndex] = wsWeibullScale;

                        float wsshape0 = this.Bias.wsshape0[speciesIndex];
                        float wsshapeB = this.Bias.wsshapeB[speciesIndex];
                        float wsshaperh = this.Bias.wsshaperh[speciesIndex];
                        float wsshapet = this.Bias.wsshapet[speciesIndex];
                        float wsshapeC = this.Bias.wsshapeC[speciesIndex];
                        float wsWeibullShape = MathF.Exp(wsshape0 + wsshapeB * lnDbh + wsshaperh * lnRelativeHeight + wsshapet * lnAge + wsshapeC * lnCompetitionTotal);
                        this.State.wsWeibullShape[speciesIndex] = wsWeibullShape;

                        float wsWeibullShape_gamma = ThreePGScalar.GammaDistribution(1.0F + 1.0F / wsWeibullShape);

                        float wslocation0 = this.Bias.wslocation0[speciesIndex];
                        float wslocationB = this.Bias.wslocationB[speciesIndex];
                        float wslocationrh = this.Bias.wslocationrh[speciesIndex];
                        float wslocationt = this.Bias.wslocationt[speciesIndex];
                        float wslocationC = this.Bias.wslocationC[speciesIndex];
                        float wsWeibullLocation;
                        if ((wslocation0 == 0.0F) && (wslocationB == 0.0F) && (wslocationrh == 0.0F) &&
                            (wslocationt == 0.0F) && (wslocationC == 0.0F))
                        {
                            float biom_tree = this.State.biom_tree[speciesIndex];
                            wsWeibullLocation = MathF.Round(biom_tree) / 10.0F - 1.0F - wsWeibullScale * wsWeibullShape_gamma;
                        }
                        else
                        {
                            wsWeibullLocation = MathF.Exp(wslocation0 + wslocationB * lnDbh + wslocationrh * lnRelativeHeight + wslocationt * lnAge + wslocationC * lnCompetitionTotal);
                        }
                        if (wsWeibullLocation < 0.01F)
                        {
                            wsWeibullLocation = 0.01F;
                        }
                        this.State.wsWeibullLocation[speciesIndex] = wsWeibullLocation;

                        Ex = wsWeibullLocation + wsWeibullScale * wsWeibullShape_gamma;
                        // now convert the Ex from weibull scale to actual scale of diameter units in cm
                        Varx = wsWeibullScale * wsWeibullScale * (ThreePGScalar.GammaDistribution(1.0F + 2.0F / wsWeibullShape) -
                            wsWeibullShape_gamma * wsWeibullShape_gamma);
                        float CVwsDistribution = MathF.Sqrt(Varx) / Ex;
                        this.State.CVwsDistribution[speciesIndex] = CVwsDistribution;

                        // DF the nWS is replaced with 1 / nWs because the equation is inverted to predict DBH from ws, instead of ws from DBH
                        float nWs = this.Parameters.nWS[speciesIndex];
                        float wsrelBias = 0.5F * (1.0F / nWs * (1.0F / nWs - 1.0F)) * CVwsDistribution * CVwsDistribution;
                        wsrelBias = ThreePGScalar.Limit(wsrelBias, -0.5F, 0.5F);
                        this.State.wsrelBias[speciesIndex] = wsrelBias;
                    }
                }
                else
                {
                    for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                    {
                        this.State.CVdbhDistribution[speciesIndex] = 0.0F;
                        this.State.CVwsDistribution[speciesIndex] = 0.0F;
                        this.State.DrelBiaspFS[speciesIndex] = 0.0F;
                        this.State.DrelBiasBasArea[speciesIndex] = 0.0F;
                        this.State.DrelBiasheight[speciesIndex] = 0.0F;
                        this.State.DrelBiasLCL[speciesIndex] = 0.0F;
                        this.State.DrelBiasCrowndiameter[speciesIndex] = 0.0F;
                        this.State.wsrelBias[speciesIndex] = 0.0F;
                    }
                }

                // correct for trees that have age 0 or are thinned (e.g. n_trees = 0)
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    float age = this.Trajectory.Species.age[speciesIndex][timestep];
                    float stems_n = this.State.stems_n[speciesIndex];
                    if ((age <= 0.0F) || (stems_n == 0.0F))
                    {
                        this.State.CVdbhDistribution[speciesIndex] = 0.0F;
                        this.State.CVwsDistribution[speciesIndex] = 0.0F;
                        this.State.DrelBiaspFS[speciesIndex] = 0.0F;
                        this.State.DrelBiasBasArea[speciesIndex] = 0.0F;
                        this.State.DrelBiasheight[speciesIndex] = 0.0F;
                        this.State.DrelBiasLCL[speciesIndex] = 0.0F;
                        this.State.DrelBiasCrowndiameter[speciesIndex] = 0.0F;
                        this.State.wsrelBias[speciesIndex] = 0.0F;
                    }

                    // Correct for bias------------------
                    float aWS = this.Parameters.aWS[speciesIndex];
                    float biom_tree = this.State.biom_tree[speciesIndex];
                    float nWs = this.Parameters.nWS[speciesIndex];
                    float wsrelBias = this.State.wsrelBias[speciesIndex];
                    float dbh = MathF.Pow(biom_tree / aWS, 1.0F / nWs) * (1.0F + wsrelBias);
                    this.State.dbh[speciesIndex] = dbh;

                    float DrelBiasBasArea = this.State.DrelBiasBasArea[speciesIndex];
                    this.State.basal_area[speciesIndex] = 0.0001F * 0.25F * MathF.PI * dbh * dbh * stems_n * (1.0F + DrelBiasBasArea);

                    float aH = this.Parameters.aH[speciesIndex];
                    float nHB = this.Parameters.nHB[speciesIndex];
                    float nHC = this.Parameters.nHC[speciesIndex];
                    float aHL = this.Parameters.aHL[speciesIndex];
                    float nHLB = this.Parameters.nHLB[speciesIndex];
                    float nHLC = this.Parameters.nHLC[speciesIndex];
                    float competition_total = this.State.competition_total;
                    float height_rel = this.State.height_rel[speciesIndex];
                    float height;
                    float crown_length;
                    switch (this.Settings.height_model)
                    {
                        case ThreePGHeightModel.Power:
                            float DrelBiasheight = this.State.DrelBiasheight[speciesIndex];
                            float DrelBiasLCL = this.State.DrelBiasLCL[speciesIndex];
                            // height = aH * MathF.Pow(dbh, nHB) * MathF.Pow(competition_total, nHC) * (1.0F + DrelBiasheight);
                            height = aH * MathF.Exp(MathF.Log(dbh) * nHB + MathF.Log(competition_total) * nHC) * (1.0F + DrelBiasheight);
                            float nHLL = this.Parameters.nHLL[speciesIndex];
                            float nHLrh = this.Parameters.nHLrh[speciesIndex];
                            // crown_length = aHL * MathF.Pow(dbh, nHLB) * MathF.Pow(lai_total, nHLL) * MathF.Pow(competition_total, nHLC) * MathF.Pow(height_rel, nHLrh) * (1.0F + DrelBiasLCL);
                            crown_length = aHL * MathF.Exp(MathF.Log(dbh) * nHLB + MathF.Log(lai_total) * nHLL + MathF.Log(competition_total) * nHLC + MathF.Log(height_rel) * nHLrh) * (1.0F + DrelBiasLCL);
                            break;
                        case ThreePGHeightModel.Exponent:
                            height = 1.3F + aH * MathF.Exp(-nHB / dbh) + nHC * competition_total * dbh;
                            crown_length = 1.3F + aHL * MathF.Exp(-nHLB / dbh) + nHLC * competition_total * dbh;
                            break;
                        default:
                            throw new NotSupportedException("Unhandled height model " + this.Settings.height_model + ".");
                    }
                    this.State.height[speciesIndex] = height;

                    // check that the height and LCL allometric equations have not predicted that height - LCL < 0
                    // and if so reduce LCL so that height - LCL = 0(assumes height allometry is more reliable than LCL allometry)
                    if (crown_length > height)
                    {
                        crown_length = height;
                    }
                    this.State.crown_length[speciesIndex] = crown_length;

                    float crown_width = 0.0F;
                    if (this.State.lai[speciesIndex] > 0.0F)
                    {
                        float aK = this.Parameters.aK[speciesIndex];
                        float nKB = this.Parameters.nKB[speciesIndex];
                        float nKH = this.Parameters.nKH[speciesIndex];
                        float nKC = this.Parameters.nKC[speciesIndex];
                        float nKrh = this.Parameters.nKrh[speciesIndex];
                        float DrelBiasCrowndiameter = this.State.DrelBiasCrowndiameter[speciesIndex];
                        // crown_width = aK * MathF.Pow(dbh, nKB) * MathF.Pow(height, nKH) * MathF.Pow(competition_total, nKC) * MathF.Pow(height_rel, nKrh) * (1.0F + DrelBiasCrowndiameter);
                        crown_width = aK * MathF.Exp(MathF.Log(dbh) * nKB + MathF.Log(height) * nKH + MathF.Log(competition_total) * nKC + MathF.Log(height_rel) * nKrh) * (1.0F + DrelBiasCrowndiameter);
                    }
                    this.State.crown_width[speciesIndex] = crown_width;

                    float pfsConst = this.State.pfsConst[speciesIndex];
                    float pfsPower = this.State.pfsPower[speciesIndex];
                    float DrelBiaspFS = this.State.DrelBiaspFS[speciesIndex];
                    this.State.pFS[speciesIndex] = pfsConst * MathF.Pow(dbh, pfsPower) * (1.0F + DrelBiaspFS);

                    Debug.Assert(dbh > 0.0F);
                }

                // update competition_total to new basal area
                float updated_competition_total = 0.0F;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float wood_density = this.Trajectory.Species.wood_density[speciesIndex][timestep];
                    updated_competition_total += wood_density * this.State.basal_area[speciesIndex];
                }
                this.State.competition_total = updated_competition_total;
            }
        }

        private static float GammaDistribution(float x)
        {
            float gamma = MathF.Pow(x, x - 0.5F) * MathF.Exp(-x) * Constant.Sqrt2Pi *
                                       (1.0F + 1.0F / (12.0F * x) + 1.0F / (288.0F * x * x) - 139.0F / (51840.0F * x * x * x) -
                                       571.0F / (2488320.0F * x * x * x * x));
            return gamma;
        }

        private static float[] GetAgeDependentParameter(float[] age, float g0, float gx, float tg, float ng)
        {
            int n_m = age.Length;
            float[] output = new float[n_m];
            for (int timestep = 0; timestep < n_m; ++timestep)
            {
                float parameter = gx; // tg = 0 case: exp(-Inf) = 0 analytically but NaN in code
                if (tg != 0.0F)
                {
                    // special case for ng = 1 and 2
                    float power = age[timestep] / tg;
                    if (ng == 2.0F)
                    {
                        power *= power;
                    }
                    else
                    {
                        power = MathF.Pow(power, ng);
                    }
                    // exp(-log(2) * power) = exp(-log(2))^power = 0.5^power = (2^-1)^power = 2^(-power)
                    parameter += (g0 - gx) * MathF.Exp(-Constant.Ln2 * power);
                }

                output[timestep] = parameter;
            }
            return output;
        }

        // returns the indices that would sort an array in ascending order.
        private static void GetAscendingOrderIndices(ReadOnlySpan<float> values, Span<int> sortIndices)
        {
            for (int layerBoundaryIndex = 0; layerBoundaryIndex < values.Length; ++layerBoundaryIndex)
            {
                sortIndices[layerBoundaryIndex] = layerBoundaryIndex;
            }

            Span<float> valuesClone = stackalloc float[values.Length];
            values.CopyTo(valuesClone);
            MemoryExtensions.Sort<float, int>(valuesClone, sortIndices);
        }

        private void GetLayers(ReadOnlySpan<float> heightCrown)
        {
            // function to allocate each tree to the layer based on height and crown heigh
            // First layer (0) is the highest and layer number then increases as height decreases.
            // Forrester DI, Guisasola R, Tang X, et al. 2014. Using a stand-level model to predict light
            //   absorption in stands with vertically and horizontally heterogeneous canopies. Forest Ecosystems
            //   1:17. https://doi.org/10.1186/s40663-014-0017-0
            // Calculations based on example https://it.mathworks.com/matlabcentral/answers/366626-overlapping-time-intervals
            int n_sp = this.Species.n_sp;
            Span<float> height_all = stackalloc float[2 * n_sp];
            Span<int> ones = stackalloc int[2 * n_sp]; // vector of 1, 0, -1 for calculation
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // put height and crown beginning into vector
                height_all[speciesIndex] = heightCrown[speciesIndex];
                height_all[n_sp + speciesIndex] = this.State.height[speciesIndex];

                // assign index order for further calculations
                ones[speciesIndex] = 1;
                ones[n_sp + speciesIndex] = -1;
            }

            // sort all height and crown height
            Span<int> height_ind = stackalloc int[2 * n_sp];
            ThreePGScalar.GetAscendingOrderIndices(height_all, height_ind); // sort order of height_all
            Span<int> buffer = stackalloc int[ones.Length];
            for (int index = 0; index < height_ind.Length; ++index)
            {
                buffer[index] = ones[height_ind[index]];
            }
            buffer.CopyTo(ones);

            // cumulative sum
            Span<int> ones_sum = stackalloc int[2 * n_sp];
            ones_sum[0] = ones[0];
            int n_l = ones_sum[0] == 0 ? 1 : 0; // number of layers
            // if (n_sp > 1) then
            for (int i = 1; i < ones.Length; ++i)
            {
                ones_sum[i] = ones_sum[i - 1] + ones[i];
                if (ones_sum[i] == 0)
                {
                    ++n_l;
                }
            }
            // end if

            // max height of each layer
            Span<float> height_layer = stackalloc float[n_l];
            int layerIndex = 0;
            for (int index = 0; index < height_ind.Length; ++index)
            {
                if (ones_sum[index] == 0)
                {
                    height_layer[layerIndex] = height_all[height_ind[index]];
                    ++layerIndex;
                }
            }
            Debug.Assert(layerIndex == n_l);

            // Assign layer to each species
            Array.Clear(this.State.layer_id);
            if (n_l > 1)
            {
                int maxLayerID = Int32.MinValue;
                for (layerIndex = 0; layerIndex < n_l - 1; ++layerIndex)
                {
                    for (int speciesIndex = 0; speciesIndex < this.State.height.Length; ++speciesIndex)
                    {
                        if (this.State.height[speciesIndex] > height_layer[layerIndex])
                        {
                            int layer_id = layerIndex + 1;
                            this.State.layer_id[speciesIndex] = layer_id;

                            if (maxLayerID < layer_id)
                            {
                                maxLayerID = layer_id;
                            }
                        }
                    }
                }

                // revert the order, so highest trees are in layer 0 and lowest layer is layer n
                for (int speciesIndex = 0; speciesIndex < this.State.height.Length; ++speciesIndex)
                {
                    this.State.layer_id[speciesIndex] = maxLayerID - this.State.layer_id[speciesIndex];
                }
            }
        }

        private void GetLayerSum(int nLayers, ReadOnlySpan<float> x, Span<float> y)
        {
            // function to sum any array x, based on the vector of layers id
            int n_sp = this.Species.n_sp;
            Debug.Assert(y.Length == n_sp);
            for (int layerIndex = 0; layerIndex < nLayers; ++layerIndex)
            {
                float layerSum = 0.0F;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if (this.State.layer_id[speciesIndex] == layerIndex)
                    {
                        layerSum += x[speciesIndex];
                    }
                }
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if (this.State.layer_id[speciesIndex] == layerIndex)
                    {
                        y[speciesIndex] = layerSum;
                    }
                }
            }
        }

        private static float[] GetLitterfallRate(float[] age, float f1, float f0, float tg)
        {
            int n_m = age.Length;
            float[] f = new float[n_m];
            if (tg * f1 == 0.0F)
            {
                Array.Fill(f, f1);
            }
            else
            {
                float kg = 12.0F * MathF.Log(1.0F + f1 / f0) / tg;
                for (int timestep = 0; timestep < n_m; ++timestep)
                {
                    f[timestep] = f1 * f0 / (f0 + (f1 - f0) * MathF.Exp(-kg * age[timestep]));
                }
            }
            return f;
        }

        private void GetMeanStemMassAndUpdateLai(int timestep)
        {
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float stems_n = this.State.stems_n[speciesIndex];
                float biom_tree = 0.0F; // mean stem mass per tree, kg
                if (stems_n > 0.0F)
                {
                    float biom_stem = this.State.biom_stem[speciesIndex];
                    biom_tree = biom_stem * 1000.0F / stems_n;
                }
                this.State.biom_tree[speciesIndex] = biom_tree;

                float biom_foliage = this.State.biom_foliage[speciesIndex];
                float sla = this.Trajectory.Species.SLA[speciesIndex][timestep];
                this.State.lai[speciesIndex] = biom_foliage * sla * 0.1F;

                Debug.Assert(biom_tree >= 0.0F);
            }
        }

        private float GetMortality(int speciesIndex)
        {
            // calculate the mortality
            float stems_n = this.State.stems_n_ha[speciesIndex];
            float WS = this.State.biom_stem[speciesIndex] / this.State.basal_area_prop[speciesIndex];
            float mS = this.Parameters.mS[speciesIndex];
            float wSx1000 = this.Parameters.wSx1000[speciesIndex];
            float thinPower = this.Parameters.thinPower[speciesIndex];

            float accuracy = 1.0F / 1000.0F;
            float n = stems_n / 1000.0F;
            float x1 = 1000.0F * mS * WS / stems_n;
            for (int iteration = 0; iteration < 6; ++iteration)
            {
                if (n <= 0.0F)
                {
                    // added in 3PG+ but risky since negative n results in negative mort_n
                    Debug.Assert(n == 0);
                    break;
                }

                float x2 = wSx1000 * MathF.Pow(n, 1.0F - thinPower);
                float fN = x2 - x1 * n - (1.0F - mS) * WS;
                float dfN = (1.0F - thinPower) * x2 / n - x1;
                float dN = -fN / dfN;
                n += dN;
                if (MathF.Abs(dN) <= accuracy)
                {
                    break;
                }
            }

            float mort_n = stems_n - 1000.0F * n;
            return mort_n;
        }

        private void GetVolumeAndIncrement(int timestep)
        {
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float aV = this.Parameters.aV[speciesIndex];
                float volume;
                if (aV > 0.0F)
                {
                    float nVB = this.Parameters.nVB[speciesIndex];
                    float nVH = this.Parameters.nVH[speciesIndex];
                    float nVBH = this.Parameters.nVBH[speciesIndex];
                    float dbh = this.State.dbh[speciesIndex];
                    float height = this.State.height[speciesIndex];
                    float stems_n = this.State.stems_n[speciesIndex];
                    // volume = aV * MathF.Pow(dbh, nVB) * MathF.Pow(height, nVH) * MathF.Pow(dbh * dbh * height, nVBH) * stems_n;
                    volume = aV * MathF.Exp(MathF.Log(dbh) * nVB + MathF.Log(height) * nVH + MathF.Log(dbh * dbh * height) * nVBH) * stems_n;
                }
                else
                {
                    float fracBB = this.Trajectory.Species.fracBB[speciesIndex][timestep];
                    float wood_density = this.Trajectory.Species.wood_density[speciesIndex][timestep];
                    float biom_stem = this.State.biom_stem[speciesIndex];
                    volume = biom_stem * (1.0F - fracBB) / wood_density;
                }
                this.State.volume[speciesIndex] = volume;

                float volume_change = 0.0F;
                if (this.State.lai[speciesIndex] > 0.0F)
                {
                    volume_change = volume - this.State.volume_previous[speciesIndex];
                    if (volume_change < 0.0F)
                    {
                        // guarantee cumulative volume is nondecreasing, https://github.com/trotsiuk/r3PG/issues/63
                        volume_change = 0.0F;
                    }
                }

                float volume_cum = this.State.volume_cum[speciesIndex];
                volume_cum += volume_change;
                this.State.volume_change[speciesIndex] = volume_change;
                this.State.volume_cum[speciesIndex] = volume_cum;
                this.State.volume_previous[speciesIndex] = volume;

                float age = this.Trajectory.Species.age[speciesIndex][timestep];
                this.State.volume_mai[speciesIndex] = volume_cum / age;
            }
        }

        // lai_above: leaf area above the given species
        // fi: * **DF the proportion of above canopy apar absorbed by each species
        // lambda_v: Constant to partition light between species and to account for vertical canopy heterogeneity(see Equations 2 and 3 of Forrester et al., 2014, Forest Ecosystems, 1:17)
        // lambda_h: Constant to account for horizontal canopy heterogeneity such as gaps between trees and the change in zenith angle(and shading) with latitude and season(see Equations 2 and 5 of Forrester et al., 2014, Forest Ecosystems, 1:17)
        // canopy_vol_frac: Fraction of canopy space (between lowest crown crown height to tallest height) filled by crowns
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
            Span<float> crownVolumeBySpecies = stackalloc float[n_sp]; // **DF the crown volume of a given species
            Span<float> heightCrown = stackalloc float[n_sp]; // height of the crown begining
            Span<float> heightMidcrown = stackalloc float[n_sp]; // mean height of the middle of the crown(height - height to crown base) / 2 + height to crown base// * **DF
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float height = this.State.height[speciesIndex];
                float crownLength = this.State.crown_length[speciesIndex];
                heightCrown[speciesIndex] = height - crownLength;
                heightMidcrown[speciesIndex] = height - crownLength / 2.0F;

                float crownWidth = this.State.crown_width[speciesIndex];
                float crownSA = 0.0F; // mean crown surface area (m²) of a species
                float crownVolume = 0.0F;
                float lai = this.State.lai[speciesIndex];
                if (lai > 0.0F)
                {
                    TreeCrownShape crownShape = this.Parameters.CrownShape[speciesIndex];
                    if (crownShape == TreeCrownShape.Cone)
                    {
                        float crownWidthSquared = crownWidth * crownWidth;
                        crownSA = MathF.PI * 0.25F * crownWidthSquared +
                            0.5F * MathF.PI * crownWidth *
                            MathF.Sqrt(0.25F * crownWidthSquared + crownLength * crownLength);
                        crownVolume = MathF.PI * crownWidthSquared * crownLength / 12.0F;
                    }
                    else if (crownShape == TreeCrownShape.Ellipsoid)
                    {
                        float halfCrownLengthPower = MathF.Pow(crownLength / 2.0F, 1.6075F);
                        float halfCrownWidthPower = MathF.Pow(crownWidth / 2.0F, 1.6075F);
                        crownSA = 4.0F * MathF.PI * MathF.Pow(halfCrownWidthPower * halfCrownWidthPower + 2.0F / 3.0F * halfCrownWidthPower * halfCrownLengthPower,
                                                              1.0F / 1.6075F);
                        crownVolume = MathF.PI * crownWidth * crownWidth * crownLength * 4.0F / 24.0F;
                    }
                    else if (crownShape == TreeCrownShape.HalfEllipsoid)
                    {
                        float crownLengthPower = MathF.Pow(crownLength, 1.6075F);
                        float halfCrownWidthPower = MathF.Pow(crownWidth / 2.0F, 1.6075F);
                        crownSA = MathF.PI * 0.25F * crownWidth * crownWidth +
                                  4.0F / 2.0F * MathF.PI * MathF.Pow((halfCrownWidthPower * halfCrownWidthPower + 
                                                                      halfCrownWidthPower * crownLengthPower + 
                                                                      halfCrownWidthPower * crownLengthPower) / 3.0F, 1.0F / 1.6075F);
                        crownVolume = MathF.PI * crownWidth * crownWidth * crownLength * 4.0F / 24.0F;
                    }
                    else if (crownShape == TreeCrownShape.Rectangular)
                    {
                        float crownWidthSquared = crownWidth * crownWidth;
                        crownSA = 2.0F * crownWidthSquared + 4.0F * crownWidth * crownLength;
                        crownVolume = crownWidthSquared * crownLength;
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled crown shape '" + crownShape + "' for species " + speciesIndex + ".");
                    }
                }
                crownVolumeBySpecies[speciesIndex] = crownVolume;

                // calculate the ratio of tree leaf area to crown surface area restrict kLS to 1
                float lai_sa_ratio = 0.0F;
                if (lai > 0.0F)
                {
                    float stems_n = this.State.stems_n[speciesIndex];
                    lai_sa_ratio = lai * 10000.0F / (stems_n * crownSA);
                }
                this.State.lai_sa_ratio[speciesIndex] = lai_sa_ratio;
            }

            // separate trees into layers
            this.GetLayers(heightCrown);
            // if (lai[i] == 0.0F) { layer_id[i] = -1.0F; } // commented out in Fortran

            // number of layers
            int maxLayerID = Int32.MinValue;
            for (int speciesIndex = 0; speciesIndex < this.State.layer_id.Length; ++speciesIndex)
            {
                maxLayerID = Math.Max(maxLayerID, this.State.layer_id[speciesIndex]);
            }
            int nLayers = maxLayerID + 1;
            Debug.Assert(nLayers > 0);

            // Now calculate the proportion of the canopy space that is filled by the crowns. The canopy space is the
            // volume between the top and bottom of a layer that is filled by crowns in that layer.
            // We calculate it only for the trees that have LAI and are present in the current month. Decidious trees
            // are in layer during their leaf off period but have zero LAI.
            Span<float> maxLeafedOutHeightByLayer = stackalloc float[nLayers];
            Span<float> minLeafedOutCrownHeightByLayer = stackalloc float[nLayers];
            for (int layerIndex = 0; layerIndex < nLayers; ++layerIndex)
            {
                bool layerHasLeafedOutSpecies = false;
                float maxLeafedOutHeightInLayer = Single.MinValue;
                float minLeafedOutCrownHeightInLayer = Single.MaxValue;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    int layer_id = this.State.layer_id[speciesIndex];
                    float lai = this.State.lai[speciesIndex];
                    if ((layer_id == layerIndex) && (lai > 0.0F))
                    {
                        float height = this.State.height[speciesIndex];
                        maxLeafedOutHeightInLayer = MathF.Max(maxLeafedOutHeightInLayer, height);
                        minLeafedOutCrownHeightInLayer = MathF.Min(minLeafedOutCrownHeightInLayer, heightCrown[speciesIndex]);
                        layerHasLeafedOutSpecies = true;
                    }
                }

                if (layerHasLeafedOutSpecies)
                {
                    maxLeafedOutHeightByLayer[layerIndex] = maxLeafedOutHeightInLayer;
                    minLeafedOutCrownHeightByLayer[layerIndex] = minLeafedOutCrownHeightInLayer;
                }
                // leave default values of zero if no species in layer have leaves on
            }

            Span<float> height_max_l = stackalloc float[n_sp];
            Span<float> heightCrown_min_l = stackalloc float[n_sp];
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                int layer_id = this.State.layer_id[speciesIndex];
                height_max_l[speciesIndex] = maxLeafedOutHeightByLayer[layer_id];
                heightCrown_min_l[speciesIndex] = minLeafedOutCrownHeightByLayer[layer_id];
            }

            // sum the canopy volume fraction per layer and save it at each species
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float lai = this.State.lai[speciesIndex];
                float canopyVolumeFraction = 0.0F;
                if (lai > 0.0F)
                {
                    float crownVolume = crownVolumeBySpecies[speciesIndex];
                    float stems_n = this.State.stems_n[speciesIndex];
                    canopyVolumeFraction = crownVolume * stems_n / ((height_max_l[speciesIndex] - heightCrown_min_l[speciesIndex]) * 10000.0F);
                }
                this.State.canopy_vol_frac[speciesIndex] = canopyVolumeFraction;

                Debug.Assert(canopyVolumeFraction >= 0.0F);
            }
            Span<float> canopy_vol_frac_temp = stackalloc float[n_sp];
            this.GetLayerSum(nLayers, this.State.canopy_vol_frac, canopy_vol_frac_temp);
            canopy_vol_frac_temp.CopyTo(this.State.canopy_vol_frac);

            Span<float> heightMidcrown_r = stackalloc float[n_sp]; // ratio of the mid height of the crown of a given species to the mid height of a canopy layer
            Span<float> kL_l = stackalloc float[n_sp]; // sum of k x L for all species within the given layer
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // if the canopy volume fraction is < 0.01(very small seedlings) then it is outside the range of the model there is no need for lambda_h so, make canopy_vol_frac = 0.01
                // where(canopy_vol_frac[i] < 0.01F) { canopy_vol_frac[i] = 0.01F } // commented out in Fortran
                // minimum height of layer
                float heightMidcrown_l = heightCrown_min_l[speciesIndex] + (height_max_l[speciesIndex] - heightCrown_min_l[speciesIndex]) / 2.0F;

                // determine the ratio between the mid height of the given species and the mid height of the layer.
                float midheightRatio = heightMidcrown[speciesIndex] / heightMidcrown_l;
                heightMidcrown_r[speciesIndex] = midheightRatio;

                // Calculate the sum of kL for all species in a layer
                kL_l[speciesIndex] = this.Parameters.k[speciesIndex] * this.State.lai[speciesIndex];

                Debug.Assert((this.State.lai[speciesIndex] == 0.0F) || ((midheightRatio >= 0.0F) && (heightMidcrown_l > 0.0F)));
            }
            Span<float> kL_l_buffer = stackalloc float[n_sp];
            this.GetLayerSum(nLayers, kL_l, kL_l_buffer);
            kL_l = kL_l_buffer;

            // vertical 
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float speciesLambdaV = 0.0F;
                // check for leaf off
                float lai = this.State.lai[speciesIndex];
                if (lai > 0.0F)
                {
                    // Constant to partition light between species and to account for vertical canopy heterogeneity
                    // (see Equations 2 and 3 of Forrester et al., 2014, Forest Ecosystems, 1:17)
                    float k = this.Parameters.k[speciesIndex];
                    speciesLambdaV = 0.012306F + 0.2366090F * k * lai / kL_l[speciesIndex] + 0.029118F * heightMidcrown_r[speciesIndex] +
                         0.608381F * k * lai / kL_l[speciesIndex] * heightMidcrown_r[speciesIndex];
                }
                this.State.lambda_v[speciesIndex] = speciesLambdaV;

                Debug.Assert((speciesLambdaV >= 0.0F) && (speciesLambdaV < Single.PositiveInfinity));
            }

            // make sure the sum of all lambda_v = 1 in each leafed out layer
            Span<float> lambdaV_l = stackalloc float[n_sp];
            this.GetLayerSum(nLayers, this.State.lambda_v, lambdaV_l); // sum of lambda_v per layer
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                if (lambdaV_l[speciesIndex] != 0.0F)
                {
                    this.State.lambda_v[speciesIndex] /= lambdaV_l[speciesIndex];
                }
            }
            Debug.Assert((this.State.lambda_v.Sum() >= 0.0F) && (this.State.lambda_v.Sum() < 1.0001F * nLayers)); // minimum is zero if no layers are leafed out, should be one otherwise

            // calculate the weighted kLS based on kL / sumkL
            Span<float> kLSweightedave = stackalloc float[n_sp]; // calculates the contribution each species makes to the sum of all kLS products in a given layer(see Equation 6 of Forrester et al., 2014, Forest Ecosystems, 1:17)
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float k = this.Parameters.k[speciesIndex];
                float lai = this.State.lai[speciesIndex];
                float lai_sa_ratio = this.State.lai_sa_ratio[speciesIndex];
                kLSweightedave[speciesIndex] = k * lai_sa_ratio * k * lai / kL_l[speciesIndex];
            }
            Span<float> kLSweightedave_buffer = stackalloc float[n_sp];
            this.GetLayerSum(nLayers, kLSweightedave, kLSweightedave_buffer);
            kLSweightedave = kLSweightedave_buffer;

            // the kLS should not be greater than 1(based on the data used to fit the light model in Forrester et al. 2014)
            // This is because when there is a high k then LS is likely to be small.
            float solarAngle = this.State.GetSolarZenithAngle(timestepEndDate);
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                if (kLSweightedave[speciesIndex] > 1.0F)
                {
                    kLSweightedave[speciesIndex] = 1.0F;
                }

                // Constant to account for horizontal canopy heterogeneity such as gaps between trees and the change
                // in zenith angle (and shading) with latitude and season.
                //   Forrester DI, Guisasola R, Tang X, et al. 2014. Using a stand-level model to predict light
                //     absorption in stands with vertically and horizontally heterogeneous canopies. Forest
                //     Ecosystems 1:17. https://doi.org/10.1186/s40663-014-0017-0
                // Equations 5a and 5b (used in Equation 2).
                float speciesLambdaH = 0.0F;
                if (this.State.lai[speciesIndex] > 0.0F) // check for leaf off
                {
                    // horizontal heterogeneity, 3-PGmix manual 11.1 equations A21 and A22
                    float canopy_vol_frac = this.State.canopy_vol_frac[speciesIndex];
                    float powerCanopyVolFrac = MathF.Pow(0.1F, canopy_vol_frac);
                    speciesLambdaH = 0.8285F + ((1.09498F - 0.781928F * kLSweightedave[speciesIndex]) * powerCanopyVolFrac) -
                        0.6714096F * powerCanopyVolFrac;
                    if (solarAngle > 30.0F)
                    {
                        speciesLambdaH += 0.00097F * MathF.Pow(1.08259F, solarAngle); // this could be hoisted
                    }
                }
                this.State.lambda_h[speciesIndex] = speciesLambdaH;

                Debug.Assert((speciesLambdaH >= 0.0F) && (speciesLambdaH <= 1.250F));
            }

            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            float RADt = solar_rad * days_in_month; // total available radiation, MJ m⁻² month⁻¹
            Span<float> aparl = stackalloc float[n_sp]; // the absorbed apar for the given layer
            for (int layerIndex = 0; layerIndex < nLayers; ++layerIndex)
            {
                float maxAParL = 0.0F;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if (this.State.layer_id[speciesIndex] == layerIndex)
                    {
                        float aparlInLayer = RADt * (1.0F - MathF.Exp(-kL_l[speciesIndex]));
                        aparl[speciesIndex] = aparlInLayer;
                        maxAParL = MathF.Max(maxAParL, aparlInLayer);
                    }
                }
                RADt -= maxAParL; // subtract the layer RAD from total
                Debug.Assert(RADt >= 0.0F);
            }

            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // ***DF this used to have month in it but this whole sub is run each month so month is now redundant here.
                float lambda_h = this.State.lambda_h[speciesIndex];
                float lambda_v = this.State.lambda_v[speciesIndex];
                float aparOfSpecies = aparl[speciesIndex] * lambda_h * lambda_v;
                this.State.apar[speciesIndex] = aparOfSpecies;

                // The proportion of above canopy apar absorbed by each species. This is used for net radiation calculations in the gettranspiration sub
                float speciesAparFraction = aparOfSpecies / (solar_rad * days_in_month);
                this.State.fi[speciesIndex] = speciesAparFraction;

                Debug.Assert((aparOfSpecies >= 0.0F) && (speciesAparFraction >= 0.0F) && (speciesAparFraction <= 1.0F));
            }

            // calculate the LAI above the given species for within canopy VPD calculations
            Span<float> LAI_l = stackalloc float[n_sp];
            this.GetLayerSum(nLayers, this.State.lai, LAI_l); // Layer LAI

            // now calculate the LAI of all layers above and part of the current layer if the species
            // is in the lower half of the layer then also take the proportion of the LAI above
            // the proportion is based on the Relative height of the mid crown
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float lai_above = 0.0F;
                float sameLayerLai = 0.0F;
                for (int otherSpeciesIndex = 0; otherSpeciesIndex < n_sp; ++otherSpeciesIndex)
                {
                    int thisLayerID = this.State.layer_id[speciesIndex];
                    int otherLayerID = this.State.layer_id[otherSpeciesIndex];
                    float otherSpeciesLai = this.State.lai[otherSpeciesIndex];
                    if (otherLayerID < thisLayerID)
                    {
                        lai_above += otherSpeciesLai;
                    }
                    else if (otherLayerID == thisLayerID)
                    {
                        sameLayerLai += otherSpeciesLai;
                    }
                }

                if (heightMidcrown_r[speciesIndex] < 0.9999999999999F)
                {
                    lai_above += sameLayerLai * (1.0F - heightMidcrown_r[speciesIndex]);
                }
                this.State.lai_above[speciesIndex] = lai_above;
            }
        }

        private void Light3PGpjs(int timestep, DateTime timestepEndDate)
        {
            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            float RADt = solar_rad * days_in_month; // MJ m⁻² month⁻¹, total available radiation

            int n_sp = this.Species.n_sp;
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float fullCanAge = this.Parameters.fullCanAge[speciesIndex];
                float canopy_cover = 1.0F;
                if (fullCanAge > 0.0F)
                {
                    float age = this.Trajectory.Species.age_m[speciesIndex][timestep];
                    if (age < fullCanAge)
                    {
                        canopy_cover = (age + 0.01F) / fullCanAge;
                    }
                }
                this.State.canopy_cover[speciesIndex] = canopy_cover;

                float k = this.Parameters.k[speciesIndex];
                float lai = this.State.lai[speciesIndex];
                float lightIntcptn = 1.0F - MathF.Exp(-k * lai / canopy_cover);

                this.State.apar[speciesIndex] = RADt * lightIntcptn * canopy_cover;
            }
        }

        private static float Limit(float value, float minValue, float maxValue)
        {
            // clamp the value to be within the minimum and maximum range
            if (value > maxValue)
            {
                return maxValue;
            }
            else if (value < minValue)
            {
                return minValue;
            }
            return value;
        }

        private float Transpiration3PGmix(int timestep, DateTime timestepEndDate, float conduct_soil)
        {
            // Species level calculations ---
            // the within canopy aero_resist and VPDspecies have been calculated using information from the light
            // submodel and from the calculation of the modifiers.The netrad for each species is calculated using
            // the fi (proportion of PAR absorbed by the given species) and is calculated by the light submodel.
            float day_length = this.State.GetDayLength(timestepEndDate);
            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            int n_sp = this.Species.n_sp;
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float transp_veg = 0.0F;
                if (this.State.lai[speciesIndex] > 0.0F)
                {
                    // SolarRad in MJ / m² / day---> * 10⁶ J / m² / day---> / day_length converts to only daytime period--->W / m2
                    float Qa = this.Parameters.Qa[speciesIndex];
                    float Qb = this.Parameters.Qb[speciesIndex];
                    float netRad = (Qa + Qb * (solar_rad * 1000.0F * 1000.0F / day_length)) * this.State.fi[speciesIndex];
                    // netRad = max(netRad, 0.0F)// net radiation can't be negative
                    Debug.Assert(netRad > -100.0F);

                    float aero_resist = this.State.aero_resist[speciesIndex];
                    float defTerm = Constant.rhoAir * Constant.lambda * (Constant.VPDconv * this.State.VPD_sp[speciesIndex]) / aero_resist;
                    float conduct_canopy = this.State.conduct_canopy[speciesIndex];
                    float div = conduct_canopy * (1.0F + Constant.e20) + 1.0F / aero_resist;

                    transp_veg = days_in_month * conduct_canopy * (Constant.e20 * netRad + defTerm) / div / Constant.lambda * day_length;
                    // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
                }
                this.State.transp_veg[speciesIndex] = transp_veg;
            }

            // now get the soil evaporation(soil aero_resist = 5 * lai_total, and VPD of soil = VPD * Exp(lai_total * -Log(2) / 5))
            // ending `so` mean soil
            float vpd_day = this.Climate.MeanDailyVpd[timestep];
            float lai_total = this.State.lai.Sum();
            float defTerm_so = Constant.rhoAir * Constant.lambda * (Constant.VPDconv * (vpd_day * MathF.Exp(lai_total * (-Constant.Ln2) / 5.0F)));
            float div_so;
            if (lai_total > 0)
            {
                defTerm_so /= (5.0F * lai_total);
                div_so = conduct_soil * (1.0F + Constant.e20) + 1.0F / (5.0F * lai_total);
            }
            else
            {
                div_so = conduct_soil * (1.0F + Constant.e20) + 1.0F;
            }

            float soilQa = this.Parameters.Qa[0]; // https://github.com/trotsiuk/r3PG/issues/67
            float soilQb = this.Parameters.Qb[0];
            float netRad_so = (soilQa + soilQb * (solar_rad * 1000.0F * 1000.0F / day_length)) * (1.0F - this.State.fi.Sum());
            // SolarRad in MJ / m2 / day---> * 10 ^ 6 J / m2 / day---> / day_length converts to only daytime period--->W / m2

            float evapotra_soil = days_in_month * conduct_soil * (Constant.e20 * netRad_so + defTerm_so) / div_so / Constant.lambda * day_length;
            // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
            Debug.Assert((evapotra_soil > -12F) && (netRad_so > -120.0F));
            return evapotra_soil;
        }

        private void Transpiration3PGpjs(int timestep, DateTime timestepEndDate)
        {
            if (this.State.VPD_sp.Sum() == 0.0F)
            {
                Array.Clear(this.State.transp_veg);
                return;
            }

            float day_length = this.State.GetDayLength(timestepEndDate);
            float days_in_month = timestepEndDate.DaysInMonth();
            float solar_rad = this.Climate.MeanDailySolarRadiation[timestep];
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                // SolarRad in MJ / m² / day ---> * 10^6 J / m² / day ---> / day_length converts to only daytime period ---> W / m²
                float Qa = this.Parameters.Qa[speciesIndex];
                float Qb = this.Parameters.Qb[speciesIndex];
                float netRad = Qa + Qb * (solar_rad * 1000.0F * 1000.0F / day_length);
                Debug.Assert(netRad > -100.0F);
                // netRad(:) = max(netRad(:), 0.0F) // net radiation can't be negative

                float BLcond = this.Parameters.BLcond[speciesIndex];
                float defTerm = Constant.rhoAir * Constant.lambda * Constant.VPDconv * this.State.VPD_sp[speciesIndex] * BLcond;
                float conduct_canopy = this.State.conduct_canopy[speciesIndex];
                float div = conduct_canopy * (1.0F + Constant.e20) + BLcond;

                float transp_veg = days_in_month * conduct_canopy * (Constant.e20 * netRad + defTerm) / div / Constant.lambda * day_length;
                // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
                transp_veg = MathF.Max(0.0F, transp_veg); // transpiration can't be negative
                this.State.transp_veg[speciesIndex] = transp_veg;
            }
        }
    }
}
