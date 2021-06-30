using System;
using System.Diagnostics;
using System.Linq;

namespace BayesianPG.ThreePG
{
    public class ThreePGpjsMix
    {
        private readonly ThreePGState state;

        public TreeSpeciesSizeDistribution? Bias { get; init; }
        public SiteClimate Climate { get; private init; }
        public TreeSpeciesManagement Management { get; private init; }
        public TreeSpeciesParameters Parameters { get; private init; }
        public ThreePGSettings Settings { get; private init; }
        public Site Site { get; private init; }
        public SiteTreeSpecies Species { get; private init; }
        public ThreePGStandTrajectory Trajectory { get; private init; }

        // n_sp: number of species
        // n_m: number of months
        public ThreePGpjsMix(Site site, SiteClimate climate, SiteTreeSpecies species, TreeSpeciesParameters parameters, TreeSpeciesManagement management, ThreePGSettings settings)
        {
            if (species.SpeciesMatch(parameters) == false)
            {
                throw new ArgumentException("Tree species count or ordering is inconsistent between species and parameters.");
            }
            if (settings.management)
            {
                if (species.SpeciesMatch(management) == false)
                {
                    throw new ArgumentException("Tree species count or ordering is inconsistent between species and management.");
                }
            }
            if (site.asw_min > site.asw_max)
            {
                throw new ArgumentOutOfRangeException(nameof(site));
            }

            this.state = new(species.n_sp, site);

            this.Climate = climate;
            this.Management = management;
            this.Parameters = parameters;
            this.Settings = settings;
            this.Site = site;
            this.Species = species;
            this.Trajectory = new(species.Name, site.From, site.To);

            if (climate.n_m < this.Trajectory.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(site), "End month specified in site is beyond the end of the provided climate record.");
            }
        }

        public void PredictStandTrajectory()
        {
            // *************************************************************************************
            // INITIALISATION (Age independent)

            // CO₂ modifier
            Span<float> fCalphax = stackalloc float[this.Species.n_sp];
            Span<float> fCg0 = stackalloc float[this.Species.n_sp];
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                fCalphax[speciesIndex] = this.Parameters.fCalpha700[speciesIndex] / (2.0F - this.Parameters.fCalpha700[speciesIndex]);
                fCg0[speciesIndex] = this.Parameters.fCg700[speciesIndex] / (2.0F * this.Parameters.fCg700[speciesIndex] - 1.0F);
            }

            // Temperature --------
            this.Trajectory.n_m = this.Trajectory.Capacity;
            for (int timestepIndex = 0; timestepIndex < this.Trajectory.n_m; ++timestepIndex)
            {
                for (int i = 0; i < this.Species.n_sp; ++i)
                {
                    // calculate temperature response function to apply to alphaCx
                    float tmp_ave = this.Climate.tmp_ave[timestepIndex];
                    float Tmin = this.Parameters.Tmin[i];
                    float Topt = this.Parameters.Topt[i];
                    float Tmax = this.Parameters.Tmax[i];
                    float f_tmp;
                    if ((tmp_ave <= Tmin) || (tmp_ave >= Tmax))
                    {
                        f_tmp = 0.0F;
                    }
                    else
                    {
                        f_tmp = ((tmp_ave - Tmin) / (Topt - Tmin)) * MathF.Pow((Tmax - tmp_ave) / (Tmax - Topt), 
                            (Tmax - Topt) / (Topt - Tmin));
                    }
                    this.Trajectory.Species.f_tmp[timestepIndex, i] = f_tmp;

                    // calculate temperature response function to apply to gc (uses mean of Tx and Tav instead of Tav, Feikema et al 2010)
                    float tmp_max = this.Climate.tmp_max[timestepIndex];
                    float f_tmp_gc;
                    if (((tmp_ave + tmp_max) / 2 <= Tmin) || ((tmp_ave + tmp_max) / 2 >= Tmax))
                    {
                        f_tmp_gc = 0.0F;
                    }
                    else
                    {
                        f_tmp_gc = (((tmp_ave + tmp_max) / 2 - Tmin) / (Topt - Tmin)) * MathF.Pow((Tmax - (tmp_ave + tmp_max) / 2) / (Tmax - Topt), 
                            (Tmax - Topt) / (Topt - Tmin));
                    }
                    this.Trajectory.Species.f_tmp_gc[timestepIndex, i] = f_tmp_gc;

                    // frost modifier
                    // TODO: fix https://github.com/trotsiuk/r3PG/issues/68
                    float kF = this.Parameters.kF[i];
                    float frost_days = this.Climate.frost_days[timestepIndex];
                    float fFrost = 1.0F - kF * (frost_days / 30.0F);
                    this.Trajectory.Species.f_frost[timestepIndex, i] = fFrost;

                    // CO₂ modifiers
                    float fCalpha = fCalphax[i] * this.Climate.co2[timestepIndex] / (350.0F * (fCalphax[i] - 1.0F) + this.Climate.co2[timestepIndex]);
                    this.Trajectory.Species.f_calpha[timestepIndex, i] = fCalpha;
                    float fCg = fCg0[i] / (1.0F + (fCg0[i] - 1.0F) * this.Climate.co2[timestepIndex] / 350.0F);
                    this.Trajectory.Species.f_cg[timestepIndex, i] = fCg;

                    Debug.Assert((f_tmp >= 0.0F) && (f_tmp <= 1.0F) &&
                                 (f_tmp_gc >= 0.0F) && (f_tmp_gc <= 1.0F) &&
                                 (fFrost >= 0.0F) && (fFrost <= 1.0F) &&
                                 (fCalpha >= 0.0F) && (fCalpha <= 1.0F) &&
                                 (fCg >= 0.0F) && (fCg <= 1.0F));
                }
            }

            // air pressure
            this.state.air_pressure = 101.3F * MathF.Exp(-1.0F * this.Site.altitude / 8200.0F);

            // SOIL WATER --------
            // Assign the SWconst and SWpower parameters for this soil class
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                if (this.Site.soil_class > 0.0F)
                {
                    // standard soil type
                    this.state.swConst[speciesIndex] = 0.8F - 0.10F * this.Site.soil_class;
                    this.state.swPower[speciesIndex] = 11.0F - 2.0F * this.Site.soil_class;
                }
                else if (this.Site.soil_class < 0.0F)
                {
                    // use supplied parameters
                    this.state.swConst[speciesIndex] = this.Parameters.SWconst0[speciesIndex];
                    this.state.swPower[speciesIndex] = this.Parameters.SWpower0[speciesIndex];
                }
                else
                {
                    // no soil-water effects
                    this.state.swConst[speciesIndex] = 999.0F;
                    this.state.swPower[speciesIndex] = this.Parameters.SWpower0[speciesIndex];
                }
            }

            // initial available soil water must be between min and max ASW
            this.Site.aSW = MathF.Max(MathF.Min(this.Site.aSW, this.Site.asw_max), this.Site.asw_min);
            this.Trajectory.AvailableSoilWater[0] = this.Site.aSW;

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
                this.state.pfsPower[speciesIndex] = pfsPower;

                this.state.pfsConst[speciesIndex] = pFS2 / MathF.Pow(2.0F, pfsPower);
            }

            // INITIALISATION (Age dependent)---------------------
            // Calculate species specific modifiers
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float[] age = new float[this.Trajectory.n_m];
                float[] age_m = new float[this.Trajectory.n_m];
                for (int timestepIndex = 0; timestepIndex < this.Trajectory.n_m; ++timestepIndex)
                {
                    age[timestepIndex] = 12.0F * (this.Site.From.Year - this.Species.year_p[speciesIndex]) + this.Site.From.Month - this.Species.month_p[speciesIndex] - 1.0F; // age of this tree species in months
                    age[timestepIndex] = (age[timestepIndex] + timestepIndex + 1) / 12.0F; // translate to years
                    age_m[timestepIndex] = age[timestepIndex] - 1.0F / 12.0F;
                }
                age_m[0] = age[0];

                this.Trajectory.Species.age[speciesIndex] = age;
                this.Trajectory.Species.age_m[speciesIndex] = age_m;
                this.Trajectory.Species.SLA[speciesIndex] = ThreePGpjsMix.GetAgeDependentParameter(age_m, this.Parameters.SLA0[speciesIndex], this.Parameters.SLA1[speciesIndex], this.Parameters.tSLA[speciesIndex], 2.0F);
                this.Trajectory.Species.fracBB[speciesIndex] = ThreePGpjsMix.GetAgeDependentParameter(age_m, this.Parameters.fracBB0[speciesIndex], this.Parameters.fracBB1[speciesIndex], this.Parameters.tBB[speciesIndex], 1.0F);
                this.Trajectory.Species.wood_density[speciesIndex] = ThreePGpjsMix.GetAgeDependentParameter(age_m, this.Parameters.rhoMin[speciesIndex], this.Parameters.rhoMax[speciesIndex], this.Parameters.tRho[speciesIndex], 1.0F);
                this.Trajectory.Species.gammaN[speciesIndex] = ThreePGpjsMix.GetAgeDependentParameter(age, this.Parameters.gammaN0[speciesIndex], this.Parameters.gammaN1[speciesIndex], this.Parameters.tgammaN[speciesIndex], this.Parameters.ngammaN[speciesIndex]); // age instead of age_m (per Fortran)
                this.Trajectory.Species.gammaF[speciesIndex] = ThreePGpjsMix.GetLitterfallRate(age_m, this.Parameters.gammaF1[speciesIndex], this.Parameters.gammaF0[speciesIndex], this.Parameters.tgammaF[speciesIndex]);

                // age modifier
                if (this.Parameters.nAge[speciesIndex] == 0.0F)
                {
                    for (int timestepIndex = 0; timestepIndex < this.Trajectory.n_m; ++timestepIndex)
                    {
                        this.Trajectory.Species.f_age[timestepIndex, speciesIndex] = 1.0F;
                    }
                }
                else
                {
                    for (int timestepIndex = 0; timestepIndex < this.Trajectory.n_m; ++timestepIndex)
                    {
                        float MaxAge = this.Parameters.MaxAge[speciesIndex];
                        float rAge = this.Parameters.rAge[speciesIndex];
                        float nAge = this.Parameters.nAge[speciesIndex];
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
                    this.state.stems_n[speciesIndex] = this.Species.stems_n_i[speciesIndex];
                    this.state.biom_stem[speciesIndex] = this.Species.biom_stem_i[speciesIndex];
                    this.state.biom_foliage[speciesIndex] = this.Species.biom_foliage_i[speciesIndex];
                    this.state.biom_root[speciesIndex] = this.Species.biom_root_i[speciesIndex];
                }
            }

            // check if this is the dormant period or previous/following period is dormant
            // to allocate foliage if needed, etc.
            int monthOfYear = this.Site.From.Month;
            for (int i = 0; i < this.Species.n_sp; ++i)
            {
                // if this is a dormant month
                if (this.IsDormant(monthOfYear, i))
                {
                    this.state.biom_foliage_debt[i] = this.state.biom_foliage[i];
                    this.state.biom_foliage[i] = 0.0F;
                }
            }

            // initial stand characteristics
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float age = this.Trajectory.Species.age[speciesIndex][0];
                if (age >= 0.0F)
                {
                    float stems_n = this.state.stems_n[speciesIndex];
                    float meanStemBiomassInKg = this.state.biom_stem[speciesIndex] * 1000.0F / stems_n;
                    this.state.biom_tree[speciesIndex] = meanStemBiomassInKg;

                    float aWS = this.Parameters.aWS[speciesIndex];
                    float nWS = this.Parameters.nWS[speciesIndex];
                    float meanDbh = MathF.Pow(meanStemBiomassInKg / aWS, 1.0F / nWS);
                    this.state.dbh[speciesIndex] = meanDbh;

                    float basalAreaPerHa = MathF.PI * 0.0001F * meanDbh * meanDbh / 4.0F * stems_n; // m²/ha
                    this.state.basal_area[speciesIndex] = basalAreaPerHa;

                    float sla = this.Trajectory.Species.SLA[speciesIndex][0];
                    this.state.lai[speciesIndex] = this.state.biom_foliage[speciesIndex] * sla * 0.1F;

                    Debug.Assert((meanStemBiomassInKg >= 0.0F) && (meanDbh >= 0.0F) && (basalAreaPerHa >= 0.0F));
                }
            }

            float competition_total = 0.0F;
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float wood_density = this.Trajectory.Species.wood_density[speciesIndex][0];
                competition_total += wood_density * this.state.basal_area[speciesIndex];
            }
            this.state.competition_total = competition_total;

            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float aH = this.Parameters.aH[speciesIndex];
                float nHB = this.Parameters.nHB[speciesIndex];
                float nHC = this.Parameters.nHC[speciesIndex];
                float dbh = this.state.dbh[speciesIndex];
                this.state.height[speciesIndex] = this.Settings.height_model switch
                {
                    ThreePGHeightModel.Power => aH * MathF.Pow(dbh, nHB) * MathF.Pow(competition_total, nHC),
                    ThreePGHeightModel.Exponent => 1.3F + aH * MathF.Exp(-nHB / dbh) + nHC * competition_total * dbh,
                    _ => throw new NotSupportedException("Unhandled height model " + this.Settings.height_model + ".")
                };
            }

            this.state.competition_total = competition_total;

            // correct the bias
            this.CorrectSizeDistribution(timestep: 0);

            float height_max = Single.MinValue;
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                if ((state.lai[speciesIndex] > 0.0F) && (state.height[speciesIndex] > height_max))
                {
                    height_max = this.state.height[speciesIndex];
                }
            }

            // volume and volume increment
            // Call main function to get volume and then fix up cumulative volume and MAI.
            this.GetVolumeAndIncrement(timestep: 0);

            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float initialVolume = this.state.volume[speciesIndex];
                this.state.volume_cum[speciesIndex] = initialVolume;

                float age = this.Trajectory.Species.age[speciesIndex][0];
                float volume_mai = 0.0F;
                if (age > 0.0F)
                {
                    volume_mai = initialVolume / age;
                }
                this.state.volume_mai[speciesIndex] = volume_mai;
            }

            this.Trajectory.Species.SetMonth(0, state);

            // *************************************************************************************
            // monthly timesteps
            bool b_cor = false;
            for (int ii = 1; ii < this.Trajectory.n_m; ++ii) // first month is initial month and set up above
            {
                // move to next month
                ++monthOfYear;
                if (monthOfYear > 12)
                {
                    monthOfYear = 1;
                }
                int monthOfYearIndex = monthOfYear - 1;

                // add any new cohorts ----------------------------------------------------------------------
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float age = this.Trajectory.Species.age[speciesIndex][ii];
                    if (age == 0.0F)
                    {
                        this.state.stems_n[speciesIndex] = this.Species.stems_n_i[speciesIndex];
                        this.state.biom_stem[speciesIndex] = this.Species.biom_stem_i[speciesIndex];
                        this.state.biom_foliage[speciesIndex] = this.Species.biom_foliage_i[speciesIndex];
                        this.state.biom_root[speciesIndex] = this.Species.biom_root_i[speciesIndex];
                        b_cor = true;
                    }
                }

                // Test for deciduous leaf off ----------------------------------------------------------------------
                // If this is first month after dormancy we need to make potential LAI, so the
                // PAR absorbption can be applied, otherwise it will be zero.
                // In the end of the month we will re-calculate it based on the actual values.
                for (int i = 0; i < this.Species.n_sp; ++i)
                {
                    if (this.IsDormant(monthOfYear, i) == false)
                    {
                        if (this.IsDormant(monthOfYear - 1, i) == true)
                        {
                            float sla = this.Trajectory.Species.SLA[i][ii];
                            this.state.lai[i] = this.state.biom_foliage_debt[i] * sla * 0.1F;
                            b_cor = true;
                        }
                    }

                    // if this is first dormant month, then set WF to 0 and move everything to the debt
                    if (this.IsDormant(monthOfYear, i) == true)
                    {
                        if (this.IsDormant(monthOfYear - 1, i) == false)
                        {
                            this.state.biom_foliage_debt[i] = this.state.biom_foliage[i];
                            this.state.biom_foliage[i] = 0.0F;
                            this.state.lai[i] = 0.0F;
                            b_cor = true;
                        }
                    }
                }

                if (b_cor == true)
                {
                    this.CorrectSizeDistribution(ii);
                    b_cor = false;
                }

                // Radiation and assimilation ----------------------------------------------------------------------
                if (this.Settings.light_model == ThreePGModel.Pjs27)
                {
                    this.Light3PGpjs(ii, monthOfYearIndex);
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        this.state.VPD_sp[speciesIndex] = this.Climate.vpd_day[ii];
                    }
                }
                else if (this.Settings.light_model == ThreePGModel.Mix)
                {
                    // Calculate the absorbed PAR.If this is first month, then it will be only potential
                    this.Light3PGmix(ii, monthOfYearIndex);
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        this.state.VPD_sp[speciesIndex] = this.Climate.vpd_day[ii] * MathF.Exp(this.state.lai_above[speciesIndex] * (-Constant.ln2) / this.Parameters.cVPD[speciesIndex]);
                    }
                }
                else
                {
                    throw new NotSupportedException("Unhandled light model " + this.Settings.light_model + ".");
                }

                // determine various environmental modifiers which were not calculated before
                // calculate VPD modifier
                // Get within-canopy climatic conditions this is exponential function
                height_max = Single.MinValue;
                float lai_total = 0.0F;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    if (this.state.lai[speciesIndex] > 0.0F)
                    {
                        height_max = MathF.Max(height_max, this.state.height[speciesIndex]);
                        lai_total += this.state.lai[speciesIndex];
                    }
                }

                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    // but since BLcond is a vector we can't use the expF
                    float BLcond = this.Parameters.BLcond[speciesIndex];
                    float height = this.state.height[speciesIndex];
                    float aero_resist;
                    if (height == height_max)
                    {
                        // if this is the (currently) tallest species
                        aero_resist = 1.0F / BLcond;
                    }
                    else
                    {
                        float twiceRelativeHeight = height / (height_max / 2.0F);
                        aero_resist = (1.0F / BLcond) + (5.0F * lai_total - (1.0F / BLcond)) *
                            MathF.Exp(-Constant.ln2 * twiceRelativeHeight * twiceRelativeHeight);
                    }
                    this.state.aero_resist[speciesIndex] = aero_resist;

                    // check for dormancy
                    if (this.state.lai[speciesIndex] == 0.0F)
                    {
                        this.state.aero_resist[speciesIndex] = 0.0F;
                    }

                    float CoeffCond = this.Parameters.CoeffCond[speciesIndex];
                    float VPD_sp = this.state.VPD_sp[speciesIndex];
                    float f_vpd = MathF.Exp(-CoeffCond * VPD_sp);
                    this.state.f_vpd[speciesIndex] = f_vpd;

                    // soil water modifier
                    float swConst = this.state.swConst[speciesIndex];
                    float swPower = this.state.swPower[speciesIndex];
                    float f_sw = 1.0F / (1.0F + MathF.Pow((1.0F - this.Site.aSW / this.Site.asw_max) / swConst, swPower));
                    this.state.f_sw[speciesIndex] = f_sw;

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
                        float fertility = this.Species.fertility[speciesIndex];
                        f_nutr = 1.0F - (1.0F - fN0) * MathF.Pow(1.0F - fertility, fNn);
                    }
                    this.state.f_nutr[speciesIndex] = f_nutr;

                    // calculate physiological modifier applied to conductance and alphaCx.
                    float f_age = this.Trajectory.Species.f_age[ii, speciesIndex];
                    float f_phys;
                    if (this.Settings.phys_model == ThreePGModel.Pjs27)
                    {
                        f_phys = MathF.Min(f_vpd, f_sw) * f_age;
                        this.Trajectory.Species.f_tmp_gc[ii, speciesIndex] = 1.0F;
                    }
                    else if (this.Settings.phys_model == ThreePGModel.Mix)
                    {
                        f_phys = f_vpd * f_sw * f_age;
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled model " + this.Settings.phys_model + ".");
                    }
                    this.state.f_phys[speciesIndex] = f_phys;

                    Debug.Assert((f_vpd >= 0.0F) && (f_vpd <= 1.0F) &&
                                 (f_sw >= 0.0F) && (f_sw <= 1.0F) &&
                                 (f_nutr >= 0.0F) && (f_nutr <= 1.0F) &&
                                 (f_phys >= 0.0F) && (f_phys <= 1.0F));

                    // calculate assimilation before the water balance is done
                    float alphaC = 0.0F;
                    if (this.state.lai[speciesIndex] > 0.0F)
                    {
                        float alphaCx = this.Parameters.alphaCx[speciesIndex];
                        float fnutr = this.state.f_nutr[speciesIndex];
                        float f_tmp = this.Trajectory.Species.f_tmp[ii, speciesIndex];
                        float f_frost = this.Trajectory.Species.f_frost[ii, speciesIndex];
                        float f_calpha = this.Trajectory.Species.f_calpha[ii, speciesIndex];
                        //float f_phys = this.state.f_phys[speciesIndex];
                        alphaC = alphaCx * f_nutr * f_tmp * f_frost * f_calpha * f_phys;
                    }
                    this.state.alpha_c[speciesIndex] = alphaC;

                    float gDM_mol = this.Parameters.gDM_mol[speciesIndex];
                    float molPAR_MJ = this.Parameters.molPAR_MJ[speciesIndex];
                    float epsilon = gDM_mol * molPAR_MJ * alphaC;
                    this.state.epsilon[speciesIndex] = epsilon;

                    float gpp = epsilon * this.state.apar[speciesIndex] / 100; // tDM / ha(apar is MJ / m ^ 2);
                    this.state.GPP[speciesIndex] = gpp;
                    float Y = this.Parameters.Y[speciesIndex]; // assumes respiratory rate is constant
                    this.state.NPP[speciesIndex] = gpp * Y;

                    // Water Balance ----------------------------------------------------------------------
                    // Calculate each species' proportion.
                    float lai_per = 0.0F;
                    if (lai_total > 0.0F)
                    {
                        lai_per = this.state.lai[speciesIndex] / lai_total;
                    }
                    this.state.lai_per[speciesIndex] = lai_per;

                    // calculate conductance
                    float LAIgcx = this.Parameters.LAIgcx[speciesIndex];
                    float gC = this.Parameters.MaxCond[speciesIndex];
                    if (lai_total <= LAIgcx) // TODO: single species case?
                    {
                        float MinCond = this.Parameters.MinCond[speciesIndex];
                        float MaxCond = this.Parameters.MaxCond[speciesIndex];
                        gC = MinCond + (MaxCond - MinCond) * lai_total / LAIgcx;
                    }
                    this.state.gC[speciesIndex] = gC;

                    //float f_phys = this.state.f_phys[speciesIndex];
                    float f_tmp_gc = this.Trajectory.Species.f_tmp_gc[ii, speciesIndex];
                    float f_cg = this.Trajectory.Species.f_cg[ii, speciesIndex];
                    this.state.conduct_canopy[speciesIndex] = gC * lai_per * f_phys * f_tmp_gc * f_cg;
                }
                float conduct_soil = Constant.MaxSoilCond * this.Site.aSW / this.Site.asw_max;
                this.Trajectory.conduct_soil[ii] = conduct_soil;

                // calculate transpiration
                float evapotra_soil = 0.0F;
                if (this.Settings.transp_model == ThreePGModel.Pjs27)
                {
                    this.Transpiration3PGpjs(ii, monthOfYearIndex);
                }
                else if (this.Settings.transp_model == ThreePGModel.Mix)
                {
                    evapotra_soil = this.Transpiration3PGmix(ii, monthOfYearIndex, conduct_soil);
                }
                else
                {
                    throw new NotSupportedException("Unhandled model " + this.Settings.transp_model + ".");
                }

                float transp_total = this.state.transp_veg.Sum() + evapotra_soil;

                this.Trajectory.evapotra_soil[ii] = evapotra_soil;

                // rainfall interception
                float prcp_interc_total = 0.0F;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float laiMaxIntcptn = this.Parameters.LAImaxIntcptn[speciesIndex];
                    float maxIntcptn = this.Parameters.MaxIntcptn[speciesIndex];
                    float prcp_interc_fract = maxIntcptn;
                    if (laiMaxIntcptn > 0.0F)
                    {
                        prcp_interc_fract = maxIntcptn * MathF.Min(1.0F, lai_total / laiMaxIntcptn) * this.state.lai_per[speciesIndex];
                    }

                    float prcp_interc = this.Climate.prcp[ii] * prcp_interc_fract;
                    prcp_interc_total += prcp_interc;

                    this.Trajectory.Species.prcp_interc[ii, speciesIndex] = prcp_interc;
                }

                // soil water balance
                float irrigation = 0.0F; // standing monthly irrigation, need to constrain irrigation only to the growing season.
                float water_runoff_pooled = 0.0F; // pooling and ponding not currently supported
                float poolFractn = MathF.Max(0.0F, MathF.Min(1.0F, 0.0F)); // determines fraction of excess water that remains on site
                float aSW = this.Site.aSW + this.Climate.prcp[ii] + (100.0F * irrigation / 12.0F) + water_runoff_pooled;
                float evapo_transp = MathF.Min(aSW, transp_total + prcp_interc_total); // ET can not exceed ASW
                float excessSW = MathF.Max(aSW - evapo_transp - this.Site.asw_max, 0.0F);
                aSW = aSW - evapo_transp - excessSW;
                water_runoff_pooled = poolFractn * excessSW;

                float irrig_supl = 0.0F;
                if (aSW < this.Site.asw_min)
                {
                    irrig_supl = this.Site.asw_min - aSW;
                    aSW = this.Site.asw_min;
                }
                this.Trajectory.irrig_supl[ii] = irrig_supl;
                this.Trajectory.prcp_runoff[ii] = (1.0F - poolFractn) * excessSW;

                Debug.Assert((aSW >= this.Site.asw_min) && (aSW <= this.Site.asw_max) && (evapo_transp > -1.0F) && (excessSW >= 0.0F) && (prcp_interc_total >= 0.0F) && (transp_total > -7.5F) && (water_runoff_pooled >= 0.0F));
                this.Site.aSW = aSW;
                this.Trajectory.AvailableSoilWater[ii] = this.Site.aSW;
                this.Trajectory.evapo_transp[ii] = evapo_transp;

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
                this.Trajectory.f_transp_scale[ii] = f_transp_scale;

                // correct for actual ET
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    this.state.GPP[speciesIndex] *= f_transp_scale;
                    float npp = this.state.NPP[speciesIndex];
                    npp *= f_transp_scale;
                    this.state.NPP[speciesIndex] = npp;
                    this.state.NPP_f[speciesIndex] = npp;

                    if ((transp_total > 0) && (f_transp_scale < 1))
                    {
                        // a different scaler is required for transpiration because all of the scaling needs
                        // to be done to the transpiration and not to the RainIntcpth, which occurs regardless of the growth
                        this.state.transp_veg[speciesIndex] = (evapo_transp - prcp_interc_total) / transp_total * this.state.transp_veg[speciesIndex];
                        evapotra_soil = (evapo_transp - prcp_interc_total) / transp_total * evapotra_soil;
                    }

                    // NEED TO CROSS CHECK THIS PART, DON'T FULLY AGREE WITH IT
                    float wue;
                    if ((evapo_transp != 0.0F) && (this.Species.n_sp == 1))
                    {
                        // in case ET is zero// Also, for mixtures it is not possible to calculate WUE based on
                        // ET because the soil evaporation cannot simply be divided between species.
                        wue = 100.0F * npp / evapo_transp;
                    }
                    else
                    {
                        wue = 0.0F;
                    }
                    this.state.WUE[speciesIndex] = wue;

                    float transp_veg = this.state.transp_veg[speciesIndex];
                    float wue_transp = 0.0F;
                    if (transp_veg > 0.0F)
                    {
                        wue_transp = 100.0F * npp / transp_veg;
                    }
                    this.state.WUE_transp[speciesIndex] = wue_transp;
                }

                if (this.Settings.calculate_d13c)
                {
                    // δ¹³C module ----------------------------------------------------------------------
                    // Calculating δ¹³C - This is based on Wei et al. 2014(Plant, Cell and Environment 37, 82 - 100)
                    // and Wei et al. 2014(Forest Ecology and Management 313, 69 - 82).This is simply calculated from
                    // other variables and has no influence on any processes
                    // Since δ¹³C is only supported by 3-PGmix canopy_cover is expected to be null the first time this
                    // block is reached.
                    Debug.Assert(Single.IsNaN(this.Climate.d13Catm[ii]) == false);

                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        // convert GPP(currently in tDM/ ha / month) to GPP in mol / m2 / s.
                        float GPP = this.state.GPP[speciesIndex];
                        float daysInMonth = Constant.DaysInMonth[monthOfYearIndex];
                        float gDM_mol = this.Parameters.gDM_mol[speciesIndex];
                        float GPP_molsec = GPP * 100.0F / (daysInMonth * 24.0F * 3600.0F * gDM_mol);

                        // canopy conductance for water vapour in mol / m2s, unit conversion(CanCond is m / s)
                        float conduct_canopy = this.state.conduct_canopy[speciesIndex];
                        float tmp_ave = this.Climate.tmp_ave[ii];
                        float Gw_mol = conduct_canopy * 44.6F * (273.15F / (273.15F + tmp_ave)) * (this.state.air_pressure / 101.3F);
                        this.state.Gw_mol[speciesIndex] = Gw_mol;

                        // canopy conductance for CO₂ in mol / m2s
                        // This calculation needs to consider the area covered by leaves as opposed to the total ground area of the stand.

                        // The explanation that Wei et al.provided for adding the "/Maximum(0.0000001, CanCover)" is
                        // that 3PG is a big leaf leaf model for conductance and the leaf area is assumed to be evenly distributed
                        // across the land area.So GwMol is divided by Maximum(0.0000001, CanCover) to convert the conductance
                        // to the area covered by the leaves only, which is smaller than the land area if the canopy has not
                        // closed.If the original light model has been selected then a CanCover value has already been calculated
                        // although Wei et al.also warn against using d13C calculations in stands with CanCover< 1.

                        // If the new light model has been selected then CanCover still needs to be calculated.
                        float stems_n = this.state.stems_n[speciesIndex];
                        float crown_width_025 = this.state.crown_width[speciesIndex] + 0.25F;
                        float canopy_cover = stems_n * crown_width_025 * crown_width_025 / 10000.0F;
                        if (canopy_cover > 1.0F)
                        {
                            canopy_cover = 1.0F;
                        }
                        this.state.canopy_cover[speciesIndex] = canopy_cover;

                        float RGcGw = this.Parameters.RGcGw[speciesIndex];
                        float Gc_mol = Gw_mol * RGcGw / MathF.Max(0.0000001F, canopy_cover);
                        this.state.Gc_mol[speciesIndex] = Gc_mol;

                        // default values for dormancy
                        float InterCi = 0.0F;
                        float D13CNewPS = 0.0F;
                        float D13CTissue = 0.0F;
                        if (Gc_mol != 0.0F)
                        {
                            // Calculating monthly average intercellular CO₂ concentration.Ci = Ca - A / g
                            InterCi = this.Climate.co2[ii] * 0.000001F - GPP_molsec / Gc_mol;
                            // Calculating monthly d13C of new photosynthate, = d13Catm - a - (b - a)(ci / ca)
                            D13CNewPS = this.Climate.d13Catm[ii] - this.Parameters.aFracDiffu[speciesIndex] - (this.Parameters.bFracRubi[speciesIndex] - this.Parameters.aFracDiffu[speciesIndex]) * (InterCi / (this.Climate.co2[ii] * 0.000001F));
                            D13CTissue = D13CNewPS + this.Parameters.D13CTissueDif[speciesIndex];
                        }

                        this.state.InterCi[speciesIndex] = InterCi;
                        this.state.D13CNewPS[speciesIndex] = D13CNewPS;
                        this.state.D13CTissue[speciesIndex] = D13CTissue;
                    }
                }

                // Biomass increment and loss module ----------------------------------------------
                for (int i = 0; i < this.Species.n_sp; ++i)
                {
                    // determine biomass increments and losses
                    float m0 = this.Parameters.m0[i];
                    float fertility = this.Species.fertility[i];
                    this.state.m[i] = m0 + (1.0F - m0) * fertility;

                    float pRx = this.Parameters.pRx[i];
                    float pRn = this.Parameters.pRn[i];
                    float f_phys = this.state.f_phys[i];
                    float m = this.state.m[i];
                    this.state.npp_fract_root[i] = pRx * pRn / (pRn + (pRx - pRn) * f_phys * m);

                    float npp_fract_root = this.state.npp_fract_root[i];
                    float pFS = this.state.pFS[i];
                    float npp_fract_stem = (1.0F - npp_fract_root) / (1.0F + pFS);
                    this.state.npp_fract_stem[i] = npp_fract_stem; 
                    
                    this.state.npp_fract_foliage[i] = 1.0F - npp_fract_root - npp_fract_stem;

                    // Dormant period -----------
                    if (this.IsDormant(monthOfYear, i) == true)
                    {
                        // There is no increment.But if this is the first dormant period then there is litterfall
                        float biom_loss_foliage = 0.0F;
                        if (this.IsDormant(monthOfYear - 1, i))
                        {
                            biom_loss_foliage = this.state.biom_foliage_debt[i];
                        }
                        this.state.biom_loss_foliage[i] = biom_loss_foliage;

                        this.state.biom_loss_root[i] = 0.0F;

                        // No changes during dormant period
                        this.state.biom_incr_foliage[i] = 0.0F;
                        this.state.biom_incr_root[i] = 0.0F;
                        this.state.biom_incr_stem[i] = 0.0F;
                    }
                    else
                    {
                        // if there is some leaves to be growth put first NPP to the leaf growth
                        // if there is enough NPP then growth all the leaves, otherwise wait for next period
                        if (this.state.biom_foliage[i] == 0.0F)
                        {
                            this.state.biom_foliage[i] = this.state.biom_foliage_debt[i];
                        }

                        if (this.state.NPP[i] >= this.state.biom_foliage_debt[i])
                        {
                            // if there is enough NPP
                            this.state.NPP[i] -= this.state.biom_foliage_debt[i];
                            this.state.biom_foliage_debt[i] = 0.0F;
                        }
                        else
                        {
                            // IF there is not enough NPP to regrow the leaves we regrow part and wait for
                            this.state.biom_foliage_debt[i] -= this.state.NPP[i];
                            this.state.NPP[i] = 0.0F;
                        }

                        // biomass loss
                        float gammaF = this.Trajectory.Species.gammaF[i][ii];
                        float biom_loss_foliage = gammaF * this.state.biom_foliage[i];
                        float biom_loss_root = this.Parameters.gammaR[i] * this.state.biom_root[i];

                        this.state.biom_loss_foliage[i] = biom_loss_foliage;
                        this.state.biom_loss_root[i] = biom_loss_root;

                        // biomass increments
                        float npp = this.state.NPP[i];
                        float biom_incr_foliage = npp * this.state.npp_fract_foliage[i];
                        float biom_incr_root = npp * this.state.npp_fract_root[i];
                        float biom_incr_stem = npp * this.state.npp_fract_stem[i];

                        this.state.biom_incr_foliage[i] = biom_incr_foliage;
                        this.state.biom_incr_root[i] = biom_incr_root;
                        this.state.biom_incr_stem[i] = biom_incr_stem;

                        // end-of-month biomass
                        this.state.biom_foliage[i] = this.state.biom_foliage[i] + biom_incr_foliage - biom_loss_foliage;
                        this.state.biom_root[i] = this.state.biom_root[i] + biom_incr_root - biom_loss_root;
                        this.state.biom_stem[i] = this.state.biom_stem[i] + biom_incr_stem;
                    }
                }

                // correct the bias
                this.GetMeanStemMassAndUpdateLai(ii);
                this.CorrectSizeDistribution(ii);

                // volume and volume increment
                // This is done before thinning and mortality part.
                this.GetVolumeAndIncrement(ii);

                // Management------------------------------------------------------------------------ -
                if (this.Management.n_sp > 0)
                {
                    for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                    {
                        float[] thinningAges = this.Management.age[speciesIndex];
                        int thinningIndex = this.state.t_n[speciesIndex];
                        if (thinningAges.Length <= thinningIndex)
                        {
                            continue;
                        }

                        float age = this.Trajectory.Species.age[speciesIndex][ii];
                        if (age >= thinningAges[thinningIndex])
                        {
                            float stems_n = this.state.stems_n[speciesIndex];
                            float target_stems_n = this.Management.stems_n[speciesIndex][thinningIndex];
                            if (stems_n > target_stems_n)
                            {
                                float mort_manag = (stems_n - target_stems_n) / stems_n;
                                stems_n *= 1.0F - mort_manag;
                                this.state.mort_manag[speciesIndex] = mort_manag;
                                this.state.stems_n[speciesIndex] = stems_n;

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
                                        this.state.biom_foliage_debt[speciesIndex] = 0.0F;
                                    }
                                    else
                                    {
                                        this.state.biom_foliage[speciesIndex] = 0.0F;
                                    }

                                    this.state.biom_root[speciesIndex] = 0.0F;
                                    this.state.biom_stem[speciesIndex] = 0.0F;
                                    this.state.stems_n[speciesIndex] = 0.0F;
                                }
                                else
                                {
                                    if (this.IsDormant(monthOfYear, speciesIndex) == true)
                                    {
                                        this.state.biom_foliage_debt[speciesIndex] *= 1.0F - mort_manag * this.Management.foliage[speciesIndex][thinningIndex];
                                    }
                                    else
                                    {
                                        this.state.biom_foliage[speciesIndex] *= 1.0F - mort_manag * this.Management.foliage[speciesIndex][thinningIndex];
                                    }

                                    this.state.biom_root[speciesIndex] *= 1.0F - mort_manag * this.Management.root[speciesIndex][thinningIndex];
                                    this.state.biom_stem[speciesIndex] *= 1.0F - mort_manag * this.Management.stem[speciesIndex][thinningIndex];
                                }

                                b_cor = true;
                            }

                            this.state.t_n[speciesIndex] = thinningIndex + 1;
                        }
                    }
                }

                // correct the bias
                if (b_cor == true)
                {
                    this.GetMeanStemMassAndUpdateLai(ii);
                    this.CorrectSizeDistribution(ii);

                    // update volume for thinning
                    this.GetVolumeAndIncrement(ii);
                    b_cor = false;
                }

                // Mortality--------------------------------------------------------------------------
                // Stress related ------------------
                for (int i = 0; i < this.Species.n_sp; ++i)
                {
                    if (this.IsDormant(monthOfYear, i) == false)
                    {
                        float gammaN = this.Trajectory.Species.gammaN[i][ii];
                        if (gammaN > 0.0F)
                        {
                            float stems_n = this.state.stems_n[i];
                            float mort_stress = gammaN * stems_n / 12.0F / 100.0F;
                            // mort_stress[i] = ceiling(mort_stress[i]); // commented in Fortran
                            mort_stress = MathF.Min(mort_stress, stems_n); // mortality can't be more than available
                            this.state.mort_stress[i] = mort_stress;

                            float mF = this.Parameters.mF[i];
                            this.state.biom_foliage[i] -= mF * mort_stress * (this.state.biom_foliage[i] / stems_n);
                            float mR = this.Parameters.mR[i];
                            this.state.biom_root[i] -= mR * mort_stress * (this.state.biom_root[i] / stems_n);
                            float mS = this.Parameters.mS[i];
                            this.state.biom_stem[i] -= mS * mort_stress * (this.state.biom_stem[i] / stems_n);

                            this.state.stems_n[i] -= mort_stress;

                            b_cor = true;
                        }
                    }
                    else
                    {
                        this.state.mort_stress[i] = 0.0F;
                    }
                }

                // correct the bias
                if (b_cor == true)
                {
                    this.GetMeanStemMassAndUpdateLai(ii);
                    this.CorrectSizeDistribution(ii);
                    b_cor = false;
                }

                // self-thinning ------------------
                float totalBasalArea = this.state.basal_area.Sum();
                for (int i = 0; i < this.Species.n_sp; ++i)
                {
                    float basal_area_prop = this.state.basal_area[i] / totalBasalArea;
                    this.state.basal_area_prop[i] = basal_area_prop;
                    // basal_area_prop[i] if basal_area_prop[i] > 0 and basal_area_prop[i] < 0.01 put 0.01
                    // where(lai[i] > 0.0F.and.basal_area_prop[i] < 0.01F) basal_area_prop[i] = 0.01F
                    float stems_n = this.state.stems_n[i];
                    this.state.stems_n_ha[i] = stems_n / basal_area_prop;

                    float wSx1000 = this.Parameters.wSx1000[i];
                    float thinPower = this.Parameters.thinPower[i];
                    this.state.biom_tree_max[i] = wSx1000 * MathF.Pow(1000.0F / this.state.stems_n_ha[i], thinPower);

                    if (this.IsDormant(monthOfYear, i) == false)
                    {
                        if (this.state.biom_tree_max[i] < this.state.biom_tree[i])
                        {
                            float mort_thinn = this.GetMortality(i) * basal_area_prop;
                            this.state.mort_thinn[i] = mort_thinn;

                            // if (stems_n[i] < 1.0F) mort_thinn[i] = stems_n[i]
                            // mort_thinn[i] = ceiling(mort_thinn[i])

                            if (mort_thinn < stems_n)
                            {
                                float biom_foliage = this.state.biom_foliage[i];
                                float mF = this.Parameters.mF[i];
                                this.state.biom_foliage[i] = biom_foliage - mF * mort_thinn * (biom_foliage / stems_n);

                                float biom_root = this.state.biom_root[i];
                                float mR = this.Parameters.mR[i];
                                this.state.biom_root[i] = biom_root - mR * mort_thinn * (biom_root / stems_n);

                                float biom_stem = this.state.biom_stem[i];
                                float mS = this.Parameters.mS[i];
                                this.state.biom_stem[i] = biom_stem - mS * mort_thinn * (biom_stem / stems_n);
                                this.state.stems_n[i] -= mort_thinn;
                            }
                            else
                            {
                                this.state.biom_foliage[i] = 0.0F;
                                this.state.biom_root[i] = 0.0F;
                                this.state.biom_stem[i] = 0.0F;
                                this.state.stems_n[i] = 0.0F;
                            }

                            b_cor = true;
                        }
                    }
                    else
                    {
                        this.state.mort_thinn[i] = 0.0F;
                    }
                }

                // correct the bias
                if (b_cor == true)
                {
                    this.GetMeanStemMassAndUpdateLai(ii);
                    this.CorrectSizeDistribution(ii);
                    b_cor = false;
                }

                // Additional calculations------------------
                totalBasalArea = this.state.basal_area.Sum();
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    this.state.basal_area_prop[speciesIndex] = this.state.basal_area[speciesIndex] / totalBasalArea;

                    // efficiency
                    float apar = this.state.apar[speciesIndex];
                    float epsilon_gpp = 0.0F;
                    float epsilon_npp = 0.0F;
                    float epsilon_biom_stem = 0.0F;
                    if (apar != 0.0F)
                    {
                        epsilon_gpp = 100.0F * this.state.GPP[speciesIndex] / apar;
                        epsilon_npp = 100.0F * this.state.NPP_f[speciesIndex] / apar;
                        epsilon_biom_stem = 100.0F * this.state.biom_incr_stem[speciesIndex] / apar;
                    }
                    this.state.epsilon_gpp[speciesIndex] = epsilon_gpp;
                    this.state.epsilon_npp[speciesIndex] = epsilon_npp;
                    this.state.epsilon_biom_stem[speciesIndex] = epsilon_biom_stem;
                }

                // copy species-specific state into stand trajectory: capture remaining end of month state
                this.Trajectory.Species.SetMonth(ii, state);
            }
        }

        private void CorrectSizeDistribution(int timestep)
        {
            Debug.Assert(this.Bias != null);

            // Diameter distributions are used to correct for bias when calculating pFS from mean dbh, and ws distributions are
            // used to correct for bias when calculating mean dbh from mean ws.This bias is caused by Jensen's inequality and is
            // corrected using the approach described by Duursma and Robinson(2003) FEM 186, 373 - 380, which uses the CV of the
            // distributions and the exponent of the relationship between predicted and predictor variables.

            // The default is to ignore the bias. The alternative is to correct for it by using empirically derived weibull distributions
            // from the weibull parameters provided by the user. If the weibull distribution does not vary then just provide scale0 and shape0.
            int n_sp = this.Species.n_sp;
            for (int n = 0; n < this.Settings.b_n; ++n)
            {
                // LAI
                float lai_total = this.state.lai.Sum();
                float standHeight = 0.0F;
                float totalStems = 0.0F;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    float stems_n = this.state.stems_n[speciesIndex];
                    standHeight += this.state.height[speciesIndex] * stems_n;
                    totalStems += stems_n;
                }
                standHeight /= totalStems;

                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    // calculate the relative height
                    // float height = aH[speciesIndex] * dbh[speciesIndex] * *nHB[speciesIndex] * competition_total[speciesIndex] * *nHC[speciesIndex]
                    float height_rel = this.state.height[speciesIndex] / standHeight;
                    this.state.height_rel[speciesIndex] = height_rel;
                }

                if (this.Settings.correct_bias)
                {
                    // Calculate the DW scale -------------------
                    float lnCompetitionTotal = MathF.Log(this.state.competition_total);
                    for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                    {
                        float age = this.Trajectory.Species.age[speciesIndex][timestep];
                        if (age == 0.0F)
                        {
                            // log of age is minus infinity so bias corretion is not possible
                            // All Weibull values end up being NaN.
                            this.state.DrelBiaspFS[speciesIndex] = 0.0F;
                            this.state.DrelBiasBasArea[speciesIndex] = 0.0F;
                            this.state.DrelBiasheight[speciesIndex] = 0.0F;
                            this.state.DrelBiasLCL[speciesIndex] = 0.0F;
                            this.state.DrelBiasCrowndiameter[speciesIndex] = 0.0F;
                            this.state.wsrelBias[speciesIndex] = 0.0F;
                            continue;
                        }

                        float lnAge = MathF.Log(age);
                        float lnDbh = MathF.Log(this.state.dbh[speciesIndex]);
                        float lnRelativeHeight = MathF.Log(this.state.height_rel[speciesIndex]);

                        float Dscale0 = this.Bias.Dscale0[speciesIndex];
                        float DscaleB = this.Bias.DscaleB[speciesIndex];
                        float Dscalerh = this.Bias.Dscalerh[speciesIndex];
                        float Dscalet = this.Bias.Dscalet[speciesIndex];
                        float DscaleC = this.Bias.DscaleC[speciesIndex];
                        float DWeibullScale = MathF.Exp(Dscale0 + DscaleB * lnDbh + Dscalerh * lnRelativeHeight + Dscalet * lnAge + DscaleC * lnCompetitionTotal);
                        this.state.DWeibullScale[speciesIndex] = DWeibullScale;

                        float Dshape0 = this.Bias.Dshape0[speciesIndex];
                        float DshapeB = this.Bias.DshapeB[speciesIndex];
                        float Dshaperh = this.Bias.Dshaperh[speciesIndex];
                        float Dshapet = this.Bias.Dshapet[speciesIndex];
                        float DshapeC = this.Bias.DshapeC[speciesIndex];
                        float DWeibullShape = MathF.Exp(Dshape0 + DshapeB * lnDbh + Dshaperh * lnRelativeHeight + Dshapet * lnAge + DshapeC * lnCompetitionTotal);
                        this.state.DWeibullShape[speciesIndex] = DWeibullShape;

                        float DWeibullShape_gamma = ThreePGpjsMix.GammaDist(1.0F + 1.0F / DWeibullShape);

                        float Dlocation0 = this.Bias.Dlocation0[speciesIndex];
                        float DlocationB = this.Bias.DlocationB[speciesIndex];
                        float Dlocationrh = this.Bias.Dlocationrh[speciesIndex];
                        float Dlocationt = this.Bias.Dlocationt[speciesIndex];
                        float DlocationC = this.Bias.DlocationC[speciesIndex];
                        float DWeibullLocation;
                        if ((Dlocation0 == 0.0F) && (DlocationB == 0.0F) && (Dlocationrh == 0.0F) &&
                            (Dlocationt == 0.0F) && (DlocationC == 0.0F))
                        {
                            float dbh = this.state.dbh[speciesIndex];
                            DWeibullLocation = MathF.Round(dbh) / 1.0F - 1.0F - DWeibullScale * DWeibullShape_gamma;
                        }
                        else
                        {
                            DWeibullLocation = MathF.Exp(Dlocation0 + DlocationB * lnDbh + Dlocationrh * lnRelativeHeight + Dlocationt * lnAge + DlocationC * lnCompetitionTotal);
                        }
                        if (DWeibullLocation < 0.01F)
                        {
                            DWeibullLocation = 0.01F;
                        }
                        this.state.DWeibullLocation[speciesIndex] = DWeibullLocation;

                        // Weibull expected value (3-PGmix user manual 11.10 equation A50)
                        float Ex = DWeibullLocation + DWeibullScale * DWeibullShape_gamma;
                        // now convert the Ex from weibull scale to actual scale of diameter units in cm
                        float Varx = DWeibullScale * DWeibullScale * (GammaDist(1.0F + 2.0F / DWeibullShape) - DWeibullShape_gamma * DWeibullShape_gamma);
                        // Weibull coefficient of variation
                        float CVdbhDistribution = MathF.Sqrt(Varx) / Ex;
                        this.state.CVdbhDistribution[speciesIndex] = CVdbhDistribution;

                        // calculate the bias (3-PGmix user manual 11.10 equation A49)
                        // prevent unrealistically large biases by restricting to ±50%
                        float pfsPower = this.state.pfsPower[speciesIndex];
                        float DrelBiaspFS = 0.5F * (pfsPower * (pfsPower - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiaspFS = ThreePGpjsMix.Limit(DrelBiaspFS, -0.5F, 0.5F);
                        this.state.DrelBiaspFS[speciesIndex] = DrelBiaspFS;

                        float nHB = this.Parameters.nHB[speciesIndex];
                        float DrelBiasheight = 0.5F * (nHB * (nHB - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasheight = ThreePGpjsMix.Limit(DrelBiasheight, -0.5F, 0.5F);
                        this.state.DrelBiasheight[speciesIndex] = DrelBiasheight;

                        float DrelBiasBasArea = 0.5F * (2.0F * (2.0F - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasBasArea = ThreePGpjsMix.Limit(DrelBiasBasArea, -0.5F, 0.5F);
                        this.state.DrelBiasBasArea[speciesIndex] = DrelBiasBasArea;

                        float nHLB = this.Parameters.nHLB[speciesIndex];
                        float DrelBiasLCL = 0.5F * (nHLB * (nHLB - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasLCL = ThreePGpjsMix.Limit(DrelBiasLCL, -0.5F, 0.5F);
                        this.state.DrelBiasLCL[speciesIndex] = DrelBiasLCL;

                        float nKB = this.Parameters.nKB[speciesIndex];
                        float DrelBiasCrowndiameter = 0.5F * (nKB * (nKB - 1.0F)) * CVdbhDistribution * CVdbhDistribution;
                        DrelBiasCrowndiameter = ThreePGpjsMix.Limit(DrelBiasCrowndiameter, -0.5F, 0.5F);
                        this.state.DrelBiasCrowndiameter[speciesIndex] = DrelBiasCrowndiameter;

                        // calculate the biom_stem scale -------------------
                        float wsscale0 = this.Bias.wsscale0[speciesIndex];
                        float wsscaleB = this.Bias.wsscaleB[speciesIndex];
                        float wsscalerh = this.Bias.wsscalerh[speciesIndex];
                        float wsscalet = this.Bias.wsscalet[speciesIndex];
                        float wsscaleC = this.Bias.wsscaleC[speciesIndex];
                        float wsWeibullScale = MathF.Exp(wsscale0 + wsscaleB * lnDbh + wsscalerh * lnRelativeHeight + wsscalet * lnAge + wsscaleC * lnCompetitionTotal);
                        this.state.wsWeibullScale[speciesIndex] = wsWeibullScale;

                        float wsshape0 = this.Bias.wsshape0[speciesIndex];
                        float wsshapeB = this.Bias.wsshapeB[speciesIndex];
                        float wsshaperh = this.Bias.wsshaperh[speciesIndex];
                        float wsshapet = this.Bias.wsshapet[speciesIndex];
                        float wsshapeC = this.Bias.wsshapeC[speciesIndex];
                        float wsWeibullShape = MathF.Exp(wsshape0 + wsshapeB * lnDbh + wsshaperh * lnRelativeHeight + wsshapet * lnAge + wsshapeC * lnCompetitionTotal);
                        this.state.wsWeibullShape[speciesIndex] = wsWeibullShape;

                        float wsWeibullShape_gamma = GammaDist(1.0F + 1.0F / wsWeibullShape);

                        float wslocation0 = this.Bias.wslocation0[speciesIndex];
                        float wslocationB = this.Bias.wslocationB[speciesIndex];
                        float wslocationrh = this.Bias.wslocationrh[speciesIndex];
                        float wslocationt = this.Bias.wslocationt[speciesIndex];
                        float wslocationC = this.Bias.wslocationC[speciesIndex];
                        float wsWeibullLocation;
                        if ((wslocation0 == 0.0F) && (wslocationB == 0.0F) && (wslocationrh == 0.0F) &&
                            (wslocationt == 0.0F) && (wslocationC == 0.0F))
                        {
                            wsWeibullLocation = MathF.Round(state.biom_tree[speciesIndex]) / 10.0F - 1.0F - wsWeibullScale * wsWeibullShape_gamma;
                        }
                        else
                        {
                            wsWeibullLocation = MathF.Exp(wslocation0 + wslocationB * lnDbh + wslocationrh * lnRelativeHeight + wslocationt * lnAge + wslocationC * lnCompetitionTotal);
                        }
                        if (wsWeibullLocation < 0.01F)
                        {
                            wsWeibullLocation = 0.01F;
                        }
                        this.state.wsWeibullLocation[speciesIndex] = wsWeibullLocation;

                        Ex = wsWeibullLocation + wsWeibullScale * wsWeibullShape_gamma;
                        // now convert the Ex from weibull scale to actual scale of diameter units in cm
                        Varx = wsWeibullScale * wsWeibullScale * (ThreePGpjsMix.GammaDist(1.0F + 2.0F / wsWeibullShape) -
                            wsWeibullShape_gamma * wsWeibullShape_gamma);
                        float CVwsDistribution = MathF.Sqrt(Varx) / Ex;
                        this.state.CVwsDistribution[speciesIndex] = CVwsDistribution;

                        // DF the nWS is replaced with 1 / nWs because the equation is inverted to predict dbh from ws, instead of ws from dbh
                        float nWs = this.Parameters.nWS[speciesIndex];
                        float wsrelBias = 0.5F * (1.0F / nWs * (1.0F / nWs - 1.0F)) * CVwsDistribution * CVwsDistribution;
                        wsrelBias = ThreePGpjsMix.Limit(wsrelBias, -0.5F, 0.5F);
                        this.state.wsrelBias[speciesIndex] = wsrelBias;
                    }
                }
                else
                {
                    for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                    {
                        this.state.DrelBiaspFS[speciesIndex] = 0.0F;
                        this.state.DrelBiasBasArea[speciesIndex] = 0.0F;
                        this.state.DrelBiasheight[speciesIndex] = 0.0F;
                        this.state.DrelBiasLCL[speciesIndex] = 0.0F;
                        this.state.DrelBiasCrowndiameter[speciesIndex] = 0.0F;
                        this.state.wsrelBias[speciesIndex] = 0.0F;
                    }
                }

                // Correct for trees that have age 0 or are thinned (e.g. n_trees = 0)
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    float age = this.Trajectory.Species.age[speciesIndex][timestep];
                    float stems_n = this.state.stems_n[speciesIndex];
                    if ((age == 0.0F) || (stems_n == 0.0F))
                    {
                        this.state.DrelBiaspFS[speciesIndex] = 0.0F;
                        this.state.DrelBiasBasArea[speciesIndex] = 0.0F;
                        this.state.DrelBiasheight[speciesIndex] = 0.0F;
                        this.state.DrelBiasLCL[speciesIndex] = 0.0F;
                        this.state.DrelBiasCrowndiameter[speciesIndex] = 0.0F;
                        this.state.wsrelBias[speciesIndex] = 0.0F;
                    }

                    // Correct for bias------------------
                    float aWs = this.Parameters.aWS[speciesIndex];
                    float biom_tree = this.state.biom_tree[speciesIndex];
                    float nWs = this.Parameters.nWS[speciesIndex];
                    float wsrelBias = this.state.wsrelBias[speciesIndex];
                    float dbh = MathF.Pow(biom_tree / aWs, 1.0F / nWs) * (1.0F + wsrelBias);
                    this.state.dbh[speciesIndex] = dbh;

                    float DrelBiasBasArea = this.state.DrelBiasBasArea[speciesIndex];
                    this.state.basal_area[speciesIndex] = 0.0001F * 0.25F * MathF.PI * dbh * dbh * stems_n * (1.0F + DrelBiasBasArea);

                    float aH = this.Parameters.aH[speciesIndex];
                    float nHB = this.Parameters.nHB[speciesIndex];
                    float nHC = this.Parameters.nHC[speciesIndex];
                    float aHL = this.Parameters.aHL[speciesIndex];
                    float nHLB = this.Parameters.nHLB[speciesIndex];
                    float nHLC = this.Parameters.nHLC[speciesIndex];
                    float competition_total = this.state.competition_total;
                    float height_rel = this.state.height_rel[speciesIndex];
                    float height;
                    float crown_length;
                    switch (this.Settings.height_model)
                    {
                        case ThreePGHeightModel.Power:
                            float DrelBiasheight = this.state.DrelBiasheight[speciesIndex];
                            float DrelBiasLCL = this.state.DrelBiasLCL[speciesIndex];
                            height = aH * MathF.Pow(dbh, nHB) * MathF.Pow(competition_total, nHC) * (1.0F + DrelBiasheight);
                            float nHLL = this.Parameters.nHLL[speciesIndex];
                            float nHLrh = this.Parameters.nHLrh[speciesIndex];
                            crown_length = aHL * MathF.Pow(dbh, nHLB) * MathF.Pow(lai_total, nHLL) * MathF.Pow(competition_total, nHLC) *
                                           MathF.Pow(height_rel, nHLrh) * (1.0F + DrelBiasLCL);
                            break;
                        case ThreePGHeightModel.Exponent:
                            height = 1.3F + aH * MathF.Pow(MathF.Exp(1.0F), -nHB / dbh) + nHC * competition_total * dbh;
                            crown_length = 1.3F + aHL * MathF.Pow(MathF.Exp(1.0F), -nHLB / dbh) + nHLC * competition_total * dbh;
                            break;
                        default:
                            throw new NotSupportedException("Unhandled height model " + this.Settings.height_model + ".");
                    }
                    this.state.height[speciesIndex] = height;

                    // check that the height and LCL allometric equations have not predicted that height - LCL < 0
                    // and if so reduce LCL so that height - LCL = 0(assumes height allometry is more reliable than LCL allometry)
                    if (crown_length > height)
                    {
                        crown_length = height;
                    }
                    this.state.crown_length[speciesIndex] = crown_length;

                    float crown_width = 0.0F;
                    if (state.lai[speciesIndex] > 0.0F)
                    {
                        float aK = this.Parameters.aK[speciesIndex];
                        float nKB = this.Parameters.nKB[speciesIndex];
                        float nKH = this.Parameters.nKH[speciesIndex];
                        float nKC = this.Parameters.nKC[speciesIndex];
                        float nKrh = this.Parameters.nKrh[speciesIndex];
                        float DrelBiasCrowndiameter = this.state.DrelBiasCrowndiameter[speciesIndex];
                        crown_width = aK * MathF.Pow(dbh, nKB) * MathF.Pow(height, nKH) * MathF.Pow(competition_total, nKC) *
                                        MathF.Pow(height_rel, nKrh) * (1.0F + DrelBiasCrowndiameter);
                    }
                    this.state.crown_width[speciesIndex] = crown_width;

                    float pfsConst = this.state.pfsConst[speciesIndex];
                    float pfsPower = this.state.pfsPower[speciesIndex];
                    float DrelBiaspFS = this.state.DrelBiaspFS[speciesIndex];
                    this.state.pFS[speciesIndex] = pfsConst * MathF.Pow(dbh, pfsPower) * (1.0F + DrelBiaspFS);

                    Debug.Assert(dbh > 0.0F);
                }

                // update competition_total to new basal area
                float updated_competition_total = 0.0F;
                for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
                {
                    float wood_density = this.Trajectory.Species.wood_density[speciesIndex][0];
                    updated_competition_total += wood_density * this.state.basal_area[speciesIndex];
                }
                this.state.competition_total = updated_competition_total;
            }
        }

        private static float GammaDist(float x)
        {
            float gamma = MathF.Pow(x, x - 0.5F) * MathF.Exp(-x) * MathF.Sqrt(2.0F * MathF.PI) *
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
                if (tg == 0.0F)
                {
                    output[timestep] = gx; // exp(-Inf) = 0 analytically but NaN in code
                }
                else
                {
                    output[timestep] = gx + (g0 - gx) * MathF.Exp(-Constant.ln2 * MathF.Pow(age[timestep] / tg, ng));
                }
            }
            return output;
        }

        // returns the indices that would sort an array in ascending order.
        private static int[] GetAscendingOrderIndices(Span<float> values)
        {
            int[] indices = new int[values.Length]; // indices into the array 'x' that sort it
            int n = values.Length;
            for (int i = 0; i < n; ++i)
            {
                indices[i] = i;
            }

            float[] valuesClone = values.ToArray();
            Array.Sort(valuesClone, indices);
            return indices;
        }

        private void GetLayer(Span<float> heightCrown)
        {
            // function to allocate each tree to the layer based on height and crown heigh
            // First layer(1) is the highest
            // According to Forrester, D.I., Guisasola, R., Tang, X.et al. For.Ecosyst. (2014) 1: 17.

            // Calculations based on example https://it.mathworks.com/matlabcentral/answers/366626-overlapping-time-intervals
            // output

            // local
            int n_sp = this.Species.n_sp;
            Span<float> height_all = stackalloc float[2 * n_sp];
            int[] ones = new int[2 * n_sp]; // vector of 1, 0, -1 for calculation
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // put height and crown beginning into vector
                height_all[speciesIndex] = heightCrown[speciesIndex];
                height_all[n_sp + speciesIndex] = this.state.height[speciesIndex];

                // assign index order for further calculations
                ones[speciesIndex] = 1;
                ones[n_sp + speciesIndex] = -1;
            }

            // sort all height and crown height
            int[] height_ind = ThreePGpjsMix.GetAscendingOrderIndices(height_all); // sort the array
            int[] buffer = new int[ones.Length];
            for (int index = 0; index < height_ind.Length; ++index)
            {
                buffer[index] = ones[height_ind[index]];
            }
            Array.Copy(buffer, ones, ones.Length);

            // cumulative sum
            int[] ones_sum = new int[2 * n_sp];
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
            Array.Clear(this.state.layer_id);
            if (n_l > 1)
            {
                int maxLayerID = Int32.MinValue;
                for (layerIndex = 0; layerIndex < n_l - 1; ++layerIndex)
                {
                    for (int speciesIndex = 0; speciesIndex < this.state.height.Length; ++speciesIndex)
                    {
                        if (this.state.height[speciesIndex] > height_layer[layerIndex])
                        {
                            int layer_id = layerIndex + 1;
                            this.state.layer_id[speciesIndex] = layer_id;

                            if (maxLayerID < layer_id)
                            {
                                maxLayerID = layer_id;
                            }
                        }
                    }
                }

                // revert the order, so highest trees are in layer 0 and lowest layer is layer n
                for (int speciesIndex = 0; speciesIndex < this.state.height.Length; ++speciesIndex)
                {
                    this.state.layer_id[speciesIndex] = maxLayerID - this.state.layer_id[speciesIndex];
                }
            }
        }

        private float[] GetLayerSum(int nLayers, Span<float> x)
        {
            // function to sum any array x, based on the vector of layers id
            int n_sp = this.Species.n_sp;
            float[] y = new float[n_sp];
            for (int i = 0; i < nLayers; ++i)
            {
                float layerSum = 0.0F;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if (this.state.layer_id[speciesIndex] == i)
                    {
                        layerSum += x[speciesIndex];
                    }
                }
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if (this.state.layer_id[speciesIndex] == i)
                    {
                        y[speciesIndex] = layerSum;
                    }
                }
            }

            return y;
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

        private float GetMortality(int speciesIndex)
        {
            // calculate the mortality
            float stems_n = this.state.stems_n_ha[speciesIndex];
            float WS = this.state.biom_stem[speciesIndex] / this.state.basal_area_prop[speciesIndex];
            float mS = this.Parameters.mS[speciesIndex];
            float wSx1000 = this.Parameters.wSx1000[speciesIndex];
            float thinPower = this.Parameters.thinPower[speciesIndex];

            float accuracy = 1.0F / 1000.0F;
            float n = stems_n / 1000.0F;
            float x1 = 1000.0F * mS * WS / stems_n;
            for (int i = 0; i < 6; ++i)
            {
                if (n <= 0.0F)
                {
                    break; // added in 3PG+
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

        private void GetMeanStemMassAndUpdateLai(int timestep)
        {
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                float stems_n = this.state.stems_n[speciesIndex];
                float biom_tree = 0.0F; // mean stem mass per tree, kg
                if (stems_n > 0.0F)
                {
                    biom_tree = this.state.biom_stem[speciesIndex] * 1000.0F / stems_n;
                }
                this.state.biom_tree[speciesIndex] = biom_tree;

                float sla = this.Trajectory.Species.SLA[speciesIndex][timestep];
                this.state.lai[speciesIndex] = this.state.biom_foliage[speciesIndex] * sla * 0.1F;

                Debug.Assert(biom_tree >= 0.0F);
            }
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
                    float dbh = this.state.dbh[speciesIndex];
                    float height = this.state.height[speciesIndex];
                    float stems_n = this.state.stems_n[speciesIndex];
                    volume = aV * MathF.Pow(dbh, nVB) * MathF.Pow(height, nVH) * MathF.Pow(dbh * dbh * height, nVBH) * stems_n;
                }
                else
                {
                    float fracBB = this.Trajectory.Species.fracBB[speciesIndex][timestep];
                    float wood_density = this.Trajectory.Species.wood_density[speciesIndex][timestep];
                    volume = this.state.biom_stem[speciesIndex] * (1.0F - fracBB) / wood_density;
                }
                this.state.volume[speciesIndex] = volume;

                float volume_change = 0.0F;
                if (this.state.lai[speciesIndex] > 0.0F)
                {
                    volume_change = volume - this.state.volume_previous[speciesIndex];
                    if (volume_change < 0.0F)
                    {
                        // guarantee cumulative volume is nondecreasing, https://github.com/trotsiuk/r3PG/issues/63
                        volume_change = 0.0F;
                    }
                }

                this.state.volume_change[speciesIndex] = volume_change;
                this.state.volume_cum[speciesIndex] += volume_change;
                this.state.volume_previous[speciesIndex] = volume;

                float age = this.Trajectory.Species.age[speciesIndex][timestep];
                this.state.volume_mai[speciesIndex] = this.state.volume_cum[speciesIndex] / age;
            }
        }

        private bool IsDormant(int monthIndex, int speciesIndex)
        {
            // This is called if the leafgrow parameter is not 0, and hence the species is Deciduous
            // This is true if "currentmonth" is part of the dormant season
            float leaffall = this.Parameters.leaffall[speciesIndex];
            float leafgrow = this.Parameters.leafgrow[speciesIndex];
            if (leafgrow > leaffall)
            {
                // southern hemisphere
                Debug.Assert((monthIndex >= 0) && (monthIndex < 13) && (leaffall > 0) && (leafgrow < 13));
                if ((monthIndex >= leaffall) && (monthIndex <= leafgrow))
                {
                    return true;
                }
            }
            else if (leafgrow < leaffall)
            {
                // northern hemisphere
                Debug.Assert((monthIndex >= 0) && (monthIndex < 13) && (leafgrow > 0) && (leaffall < 13));
                if ((monthIndex < leafgrow) || (monthIndex >= leaffall))
                {
                    return true;
                }
            }

            // evergreen species: leafgrow = leaffall = 0
            return false;
        }

        // lai_above: leaf area above the given species
        // fi: * **DF the proportion of above canopy apar absorbed by each species
        // lambda_v: Constant to partition light between species and to account for vertical canopy heterogeneity(see Equations 2 and 3 of Forrester et al., 2014, Forest Ecosystems, 1:17)
        // lambda_h: Constant to account for horizontal canopy heterogeneity such as gaps between trees and the change in zenith angle(and shading) with latitude and season(see Equations 2 and 5 of Forrester et al., 2014, Forest Ecosystems, 1:17)
        // canopy_vol_frac: Fraction of canopy space (between lowest crown crown height to tallest height) filled by crowns
        private void Light3PGmix(int timestep, int monthOfYearIndex)
        {
            // Subroutine calculate the apar for the mixed species forest
            // It first allocate each species to a specific layer based on height and crown length
            // and then distribute the light between those layers

            // Calculate the mid crown height, crown surface and volume
            // check if species is dormant
            // where(lai(:) == 0)
            //    height(:) = 0.0F
            //    crown_length(:) = 0.0F
            // end where

            // Calculate the crown area and volume
            // We only do it for species that have LAI, otherwise it stays 0 as was initialized above
            // If LAI is equal to 0, this is an indicator that the species is currently in the dormant period
            // input
            int n_sp = this.Species.n_sp;
            Span<float> crownSA = stackalloc float[n_sp]; // mean crown surface area (m²) of a species
            Span<float> crownVolume = stackalloc float[n_sp]; // * **DF the crown volume of a given species
            Span<float> heightCrown = stackalloc float[n_sp]; // height of the crown begining
            Span<float> heightMidcrown = stackalloc float[n_sp]; // mean height of the middle of the crown(height - height to crown base) / 2 + height to crown base// * **DF
            for (int i = 0; i < n_sp; ++i)
            {
                float height = this.state.height[i];
                float crown_length = this.state.crown_length[i];
                heightCrown[i] = height - crown_length;
                heightMidcrown[i] = height - crown_length / 2.0F;

                float crown_width = this.state.crown_width[i];
                float lai = this.state.lai[i];
                if (lai > 0.0F)
                {
                    TreeCrownShape crownShape = this.Parameters.CrownShape[i];
                    if (crownShape == TreeCrownShape.Cone)
                    {
                        crownSA[i] = MathF.PI * 0.25F * crown_width * crown_width + 
                            0.5F * MathF.PI * crown_width *
                            MathF.Sqrt(0.25F * crown_width * crown_width + crown_length * crown_length);
                        crownVolume[i] = MathF.PI * crown_width * crown_width * crown_length / 12.0F;
                    }
                    else if (crownShape == TreeCrownShape.Ellipsoid)
                    {
                        float halfCrownLengthPower = MathF.Pow(crown_length / 2.0F, 1.6075F);
                        float halfCrownWidthPower = MathF.Pow(crown_width / 2.0F, 1.6075F);
                        crownSA[i] = 4.0F * MathF.PI * MathF.Pow(halfCrownWidthPower * halfCrownWidthPower +
                                      (halfCrownWidthPower * halfCrownLengthPower + halfCrownWidthPower * halfCrownLengthPower) / 3.0F,
                                      1.0F / 1.6075F);
                        crownVolume[i] = MathF.PI * crown_width * crown_width * crown_length * 4.0F / 24.0F;
                    }
                    else if (crownShape == TreeCrownShape.HalfEllipsoid)
                    {
                        float crownLengthPower = MathF.Pow(crown_length, 1.6075F);
                        float halfCrownWidthPower = MathF.Pow(crown_width / 2.0F, 1.6075F);
                        crownSA[i] = MathF.PI * 0.25F * crown_width * crown_width +
                            4.0F * MathF.PI * MathF.Pow((halfCrownWidthPower * halfCrownWidthPower + 
                                halfCrownWidthPower * crownLengthPower + 
                                halfCrownWidthPower * crownLengthPower) / 3.0F, 1.0F / 1.6075F) / 2.0F;
                        crownVolume[i] = MathF.PI * crown_width * crown_width * crown_length * 4.0F / 24.0F;
                    }
                    else if (crownShape == TreeCrownShape.Rectangular)
                    {
                        crownSA[i] = crown_width * crown_width * 2.0F + crown_width * crown_length * 4.0F;
                        crownVolume[i] = crown_width * crown_width * crown_length;
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled crown shape '" + crownShape + "' for species " + i + ".");
                    }
                }

                // calculate the ratio of tree leaf area to crown surface area restrict kLS to 1
                float lai_sa_ratio = lai * 10000.0F / this.state.stems_n[i] / crownSA[i];
                if (lai == 0.0F)
                {
                    lai_sa_ratio = 0.0F;
                }
                this.state.lai_sa_ratio[i] = lai_sa_ratio;
            }

            // separate trees into layers
            this.GetLayer(heightCrown);
            // if (lai[i] == 0.0F) { layer_id[i] = -1.0F; } // commented out in Fortran

            // number of layers
            int maxLayerID = Int32.MinValue;
            for (int speciesIndex = 0; speciesIndex < this.state.layer_id.Length; ++speciesIndex)
            {
                maxLayerID = Math.Max(maxLayerID, this.state.layer_id[speciesIndex]);
            }
            int nLayers = maxLayerID + 1;
            Debug.Assert(nLayers > 0);

            // Now calculate the proportion of the canopy space that is filled by the crowns. The canopy space is the
            // volume between the top and bottom of a layer that is filled by crowns in that layer.
            // We calculate it only for the trees that have LAI and are present in the current month. Decidious trees
            // are in layer during their leaf off period but have zero LAI.
            Span<float> maxLeafedOutHeightByLayer = stackalloc float[nLayers];
            Span<float> minLeafedOutCrownHeightByLayer = stackalloc float[nLayers];
            for (int i = 0; i < nLayers; ++i)
            {
                bool layerHasLeafedOutSpecies = false;
                float maxLeafedOutHeightInLayer = Single.MinValue;
                float minLeafedOutCrownHeightInLayer = Single.MaxValue;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if ((state.layer_id[speciesIndex] == i) && (state.lai[speciesIndex] > 0.0F))
                    {
                        maxLeafedOutHeightInLayer = MathF.Max(maxLeafedOutHeightInLayer, this.state.height[speciesIndex]);
                        minLeafedOutCrownHeightInLayer = MathF.Min(minLeafedOutCrownHeightInLayer, heightCrown[speciesIndex]);
                        layerHasLeafedOutSpecies = true;
                    }
                }

                if (layerHasLeafedOutSpecies)
                {
                    maxLeafedOutHeightByLayer[i] = maxLeafedOutHeightInLayer;
                    minLeafedOutCrownHeightByLayer[i] = minLeafedOutCrownHeightInLayer;
                }
                // leave default values of zero if no species in layer have leaves on
            }

            Span<float> height_max_l = stackalloc float[n_sp];
            Span<float> heightCrown_min_l = stackalloc float[n_sp];
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                int layer_id = this.state.layer_id[speciesIndex];
                height_max_l[speciesIndex] = maxLeafedOutHeightByLayer[layer_id];
                heightCrown_min_l[speciesIndex] = minLeafedOutCrownHeightByLayer[layer_id];
            }

            // sum the canopy volume fraction per layer and save it at each species
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float canopyVolumeFraction = crownVolume[speciesIndex] * this.state.stems_n[speciesIndex] / ((height_max_l[speciesIndex] - heightCrown_min_l[speciesIndex]) * 10000.0F);
                this.state.canopy_vol_frac[speciesIndex] = canopyVolumeFraction;

                Debug.Assert((state.lai[speciesIndex] == 0.0F) || (canopyVolumeFraction >= 0.0F));
            }
            float[] canopy_vol_frac_temp = this.GetLayerSum(nLayers, this.state.canopy_vol_frac);
            Array.Copy(canopy_vol_frac_temp, this.state.canopy_vol_frac, this.state.canopy_vol_frac.Length); // TODO: update in place

            Span<float> heightMidcrown_l = stackalloc float[n_sp]; // maximum and minimum height of layer
            Span<float> heightMidcrown_r = stackalloc float[n_sp]; // ratio of the mid height of the crown of a given species to the mid height of a canopy layer
            Span<float> kL_l = stackalloc float[n_sp]; // sum of k x L for all species within the given layer
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // if the canopy volume fraction is < 0.01(very small seedlings) then it is outside the range of the model there is no need for lambda_h so, make canopy_vol_frac = 0.01
                // where(canopy_vol_frac[i] < 0.01F) { canopy_vol_frac[i] = 0.01F } // commented out in Fortran
                float minimumLayerHeight = heightCrown_min_l[speciesIndex] + (height_max_l[speciesIndex] - heightCrown_min_l[speciesIndex]) / 2.0F;
                heightMidcrown_l[speciesIndex] = minimumLayerHeight;

                // determine the ratio between the mid height of the given species and the mid height of the layer.
                float midheightRatio = heightMidcrown[speciesIndex] / heightMidcrown_l[speciesIndex];
                heightMidcrown_r[speciesIndex] = midheightRatio;

                // Calculate the sum of kL for all species in a layer
                kL_l[speciesIndex] = this.Parameters.k[speciesIndex] * this.state.lai[speciesIndex];

                Debug.Assert((state.lai[speciesIndex] == 0.0F) || ((midheightRatio >= 0.0F) && (minimumLayerHeight > 0.0F)));
            }
            kL_l = this.GetLayerSum(nLayers, kL_l);

            // vertical 
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float speciesLambdaV = 0.0F;
                // check for leaf off
                float lai = this.state.lai[speciesIndex];
                if (lai > 0.0F)
                {
                    // Constant to partition light between species and to account for vertical canopy heterogeneity
                    // (see Equations 2 and 3 of Forrester et al., 2014, Forest Ecosystems, 1:17)
                    float k = this.Parameters.k[speciesIndex];
                    speciesLambdaV = 0.012306F + 0.2366090F * k * lai / kL_l[speciesIndex] + 0.029118F * heightMidcrown_r[speciesIndex] +
                         0.608381F * k * lai / kL_l[speciesIndex] * heightMidcrown_r[speciesIndex];
                }
                this.state.lambda_v[speciesIndex] = speciesLambdaV;

                Debug.Assert((speciesLambdaV >= 0.0F) && (speciesLambdaV < Single.PositiveInfinity));
            }

            // make sure the sum of all lambda_v = 1 in each leafed out layer
            float[] lambdaV_l = this.GetLayerSum(nLayers, this.state.lambda_v); // sum of lambda_v per layer
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                if (lambdaV_l[speciesIndex] != 0.0F)
                {
                    this.state.lambda_v[speciesIndex] /= lambdaV_l[speciesIndex];
                }
            }
            Debug.Assert((state.lambda_v.Sum() >= 0.0F) && (state.lambda_v.Sum() < 1.0001F * nLayers)); // minimum is zero if no layers are leafed out, should be one otherwise

            // calculate the weighted kLS based on kL / sumkL
            Span<float> kLSweightedave = stackalloc float[n_sp]; // calculates the contribution each species makes to the sum of all kLS products in a given layer(see Equation 6 of Forrester et al., 2014, Forest Ecosystems, 1:17)
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float k = this.Parameters.k[speciesIndex];
                float lai = this.state.lai[speciesIndex];
                float lai_sa_ratio = this.state.lai_sa_ratio[speciesIndex];
                kLSweightedave[speciesIndex] = k * lai_sa_ratio * k * lai / kL_l[speciesIndex];
            }
            kLSweightedave = this.GetLayerSum(nLayers, kLSweightedave);

            // the kLS should not be greater than 1(based on the data used to fit the light model in Forrester et al. 2014)
            // This is because when there is a high k then LS is likely to be small.
            float solarAngle = this.state.adjSolarZenithAngle[monthOfYearIndex];
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
                if (state.lai[speciesIndex] > 0.0F) // check for leaf off
                {
                    // horizontal heterogeneity, 3-PGmix manual 11.1 equations A21 and A22
                    float canopy_vol_frac = this.state.canopy_vol_frac[speciesIndex];
                    speciesLambdaH = 0.8285F + ((1.09498F - 0.781928F * kLSweightedave[speciesIndex]) * MathF.Pow(0.1F, canopy_vol_frac)) -
                        0.6714096F * MathF.Pow(0.1F, canopy_vol_frac);
                    if (solarAngle > 30.0F)
                    {
                        speciesLambdaH += 0.00097F * MathF.Pow(1.08259F, solarAngle);
                    }
                }
                this.state.lambda_h[speciesIndex] = speciesLambdaH;

                Debug.Assert((speciesLambdaH >= 0.0F) && (speciesLambdaH <= 1.25F));
            }

            float days_in_month = Constant.DaysInMonth[monthOfYearIndex];
            float solar_rad = this.Climate.solar_rad[timestep];
            float RADt = solar_rad * days_in_month; // total available radiation, MJ m⁻² month⁻¹
            Span<float> aparl = stackalloc float[n_sp]; // The absorbed apar for the given  layer
            for (int i = 0; i < nLayers; ++i)
            {
                float maxAParL = 0.0F;
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    if (state.layer_id[speciesIndex] == i)
                    {
                        aparl[speciesIndex] = RADt * (1.0F - MathF.Exp(-kL_l[speciesIndex]));
                        maxAParL = MathF.Max(maxAParL, aparl[speciesIndex]);
                    }
                }
                RADt -= maxAParL; // subtract the layer RAD from total
                Debug.Assert(RADt >= 0.0F);
            }

            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                // ***DF this used to have month in it but this whole sub is run each month so month is now redundant here.
                float aparOfSpecies = aparl[speciesIndex] * this.state.lambda_h[speciesIndex] * this.state.lambda_v[speciesIndex];
                this.state.apar[speciesIndex] = aparOfSpecies;

                // The proportion of above canopy apar absorbed by each species. This is used for net radiation calculations in the gettranspiration sub
                float speciesAparFraction = aparOfSpecies / (solar_rad * days_in_month);
                this.state.fi[speciesIndex] = speciesAparFraction;

                Debug.Assert((aparOfSpecies >= 0.0F) && (speciesAparFraction >= 0.0F) && (speciesAparFraction <= 1.08F));
            }

            // calculate the LAI above the given species for within canopy VPD calculations
            float[] LAI_l = this.GetLayerSum(nLayers, this.state.lai); // Layer LAI

            // now calculate the LAI of all layers above and part of the current layer if the species
            // is in the lower half of the layer then also take the proportion of the LAI above
            // the proportion is based on the Relative height of the mid crown
            for (int i = 0; i < n_sp; ++i)
            {
                float lai_above = 0.0F;
                float sameHeightLai = 0.0F;
                for (int otherSpeciesIndex = 0; otherSpeciesIndex < n_sp; ++otherSpeciesIndex)
                {
                    int thisLayerID = this.state.layer_id[i];
                    int otherLayerID = this.state.layer_id[otherSpeciesIndex];
                    float otherSpeciesLai = this.state.lai[otherSpeciesIndex];
                    if (otherLayerID < thisLayerID)
                    {
                        lai_above += otherSpeciesLai;
                    }
                    else if (otherLayerID == thisLayerID)
                    {
                        sameHeightLai += otherSpeciesLai;
                    }
                }

                if (heightMidcrown_r[i] < 0.9999999999999F)
                {
                    lai_above += sameHeightLai * (1.0F - heightMidcrown_r[i]);
                }
                this.state.lai_above[i] = lai_above;
            }
        }

        private void Light3PGpjs(int timestep, int monthOfYearIndex)
        {
            float days_in_month = Constant.DaysInMonth[monthOfYearIndex];
            float solar_rad = this.Climate.solar_rad[timestep];
            float RADt = solar_rad * days_in_month; // MJ m-2 month-1, total available radiation

            int n_sp = this.Species.n_sp;
            Span<float> lightIntcptn = stackalloc float[n_sp];
            for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
            {
                float age = this.Trajectory.Species.age_m[speciesIndex][timestep];
                float fullCanAge = this.Parameters.fullCanAge[speciesIndex];
                float canopy_cover = 1.0F;
                if ((fullCanAge > 0.0F) && (age < fullCanAge))
                {
                    canopy_cover = (age + 0.01F) / fullCanAge;
                }
                this.state.canopy_cover[speciesIndex] = canopy_cover;

                float k = this.Parameters.k[speciesIndex];
                lightIntcptn[speciesIndex] = 1.0F - MathF.Exp(-k * this.state.lai[speciesIndex] / canopy_cover);

                this.state.apar[speciesIndex] = RADt * lightIntcptn[speciesIndex] * canopy_cover;
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

        private float Transpiration3PGmix(int timestep, int monthOfYearIndex, float conduct_soil)
        {
            // Species level calculations ---
            // the within canopy aero_resist and VPDspecies have been calculated using information from the light
            // submodel and from the calculation of the modifiers.The netrad for each species is calculated using
            // the fi (proportion of PAR absorbed by the given species) and is calculated by the light submodel.
            float day_length = this.state.day_length[monthOfYearIndex];
            int days_in_month = Constant.DaysInMonth[monthOfYearIndex];
            float lai_total = this.state.lai.Sum();
            float solar_rad = this.Climate.solar_rad[timestep];
            if (lai_total > 0.0F)
            {
                int n_sp = this.Species.n_sp;
                Span<float> netRad = stackalloc float[n_sp];
                Span<float> defTerm = stackalloc float[n_sp];
                Span<float> div = stackalloc float[n_sp];
                for (int speciesIndex = 0; speciesIndex < n_sp; ++speciesIndex)
                {
                    // SolarRad in MJ / m2 / day---> * 10 ^ 6 J / m2 / day---> / day_length converts to only daytime period--->W / m2
                    float Qa = this.Parameters.Qa[speciesIndex];
                    float Qb = this.Parameters.Qb[speciesIndex];
                    netRad[speciesIndex] = (Qa + Qb * (solar_rad * 1000.0F * 1000.0F / day_length)) * this.state.fi[speciesIndex];
                    // netRad[speciesIndex] = max(netRad[speciesIndex], 0.0F)// net radiation can't be negative
                    Debug.Assert(netRad[speciesIndex] > -100.0F);

                    float aero_resist = this.state.aero_resist[speciesIndex];
                    defTerm[speciesIndex] = Constant.rhoAir * Constant.lambda * (Constant.VPDconv * this.state.VPD_sp[speciesIndex]) / aero_resist;
                    float conduct_canopy = this.state.conduct_canopy[speciesIndex];
                    div[speciesIndex] = conduct_canopy * (1.0F + Constant.e20) + 1.0F / aero_resist;

                    float transp_veg = 0.0F;
                    if (state.lai[speciesIndex] > 0.0F)
                    {
                        transp_veg = days_in_month * conduct_canopy * (Constant.e20 * netRad[speciesIndex] + defTerm[speciesIndex]) / div[speciesIndex] / Constant.lambda * day_length;
                        // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
                    }
                    this.state.transp_veg[speciesIndex] = transp_veg;
                }
            }
            else
            {
                Array.Clear(this.state.transp_veg);
            }

            // now get the soil evaporation(soil aero_resist = 5 * lai_total, and VPD of soil = VPD * Exp(lai_total * -Log(2) / 5))
            // ending `so` mean soil
            float vpd_day = this.Climate.vpd_day[timestep];
            float defTerm_so;
            float div_so;
            if (lai_total > 0)
            {
                defTerm_so = Constant.rhoAir * Constant.lambda * (Constant.VPDconv * (vpd_day * MathF.Exp(lai_total * (-Constant.ln2) / 5.0F))) / (5.0F * lai_total);
                div_so = conduct_soil * (1.0F + Constant.e20) + 1.0F / (5.0F * lai_total);
            }
            else
            {
                // defTerm_so = 0.0F
                defTerm_so = Constant.rhoAir * Constant.lambda * (Constant.VPDconv * (vpd_day * MathF.Exp(lai_total * (-Constant.ln2) / 5.0F)));
                div_so = conduct_soil * (1.0F + Constant.e20) + 1.0F;
            }

            float netRad_so = (this.Parameters.Qa[0] + this.Parameters.Qb[0] * (solar_rad * 1000.0F * 1000.0F / day_length)) * (1.0F - this.state.fi.Sum());
            // SolarRad in MJ / m2 / day---> * 10 ^ 6 J / m2 / day---> / day_length converts to only daytime period--->W / m2

            float evapotra_soil = days_in_month * conduct_soil * (Constant.e20 * netRad_so + defTerm_so) / div_so / Constant.lambda * day_length;
            // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
            Debug.Assert((evapotra_soil > -12F) && (netRad_so > -120.0F));
            return evapotra_soil;
        }

        private void Transpiration3PGpjs(int timestep, int monthOfYearIndex)
        {
            if (this.state.VPD_sp.Sum() == 0.0F)
            {
                Array.Clear(this.state.transp_veg);
                return;
            }

            float day_length = this.state.day_length[monthOfYearIndex];
            int days_in_month = Constant.DaysInMonth[monthOfYearIndex];
            float solar_rad = this.Climate.solar_rad[timestep];
            for (int speciesIndex = 0; speciesIndex < this.Species.n_sp; ++speciesIndex)
            {
                // SolarRad in MJ / m² / day ---> * 10^6 J / m² / day ---> / day_length converts to only daytime period ---> W / m²
                float Qa = this.Parameters.Qa[speciesIndex];
                float Qb = this.Parameters.Qb[speciesIndex];
                float netRad = Qa + Qb * (solar_rad * 1000.0F * 1000.0F / day_length);
                Debug.Assert(netRad > -100.0F);
                // netRad(:) = max(netRad(:), 0.0F) // net radiation can't be negative

                float BLcond = this.Parameters.BLcond[speciesIndex];
                float defTerm = Constant.rhoAir * Constant.lambda * Constant.VPDconv * this.state.VPD_sp[speciesIndex] * BLcond;
                float conduct_canopy = this.state.conduct_canopy[speciesIndex];
                float div = conduct_canopy * (1.0F + Constant.e20) + BLcond;

                float transp_veg = days_in_month * conduct_canopy * (Constant.e20 * netRad + defTerm) / div / Constant.lambda * day_length;
                // in J / m2 / s then the "/lambda*h" converts to kg / m2 / day and the days in month then coverts this to kg/ m2 / month
                transp_veg = MathF.Max(0.0F, transp_veg); // transpiration can't be negative
                this.state.transp_veg[speciesIndex] = transp_veg;
            }
        }
    }
}
