using BayesianPG.Extensions;
using System;
using System.Runtime.Intrinsics;

namespace BayesianPG.ThreePG
{
    public static class TreeSpeciesParameters
    {
        public static TreeSpeciesParameters<Vector128<float>> BroadcastScalarToVector128(TreeSpeciesParameters<float> parameters)
        {
            TreeSpeciesParameters<Vector128<float>> parameters128 = new();
            parameters128.AllocateSpecies(parameters.Species);

            for (int speciesIndex = 0; speciesIndex < parameters.Species.Length; ++speciesIndex)
            {
                // biomass partitioning and turnover
                parameters128.pFS2[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.pFS2[speciesIndex]);
                parameters128.pFS20[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.pFS20[speciesIndex]);
                parameters128.aWS[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.aWS[speciesIndex]);
                parameters128.nWS[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nWS[speciesIndex]);
                parameters128.pRx[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.pRx[speciesIndex]);
                parameters128.pRn[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.pRn[speciesIndex]);
                parameters128.gammaF1[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.gammaF1[speciesIndex]);
                parameters128.gammaF0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.gammaF0[speciesIndex]);
                parameters128.tgammaF[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.tgammaF[speciesIndex]);
                parameters128.gammaR[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.gammaR[speciesIndex]);
                parameters128.leafgrow[speciesIndex] = parameters.leafgrow[speciesIndex];
                parameters128.leaffall[speciesIndex] = parameters.leaffall[speciesIndex];

                // NPP & conductance modifiers
                parameters128.Tmin[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.Tmin[speciesIndex]);
                parameters128.Topt[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.Topt[speciesIndex]);
                parameters128.Tmax[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.Tmax[speciesIndex]);
                parameters128.kF[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.kF[speciesIndex]);
                parameters128.SWconst0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.SWconst0[speciesIndex]);
                parameters128.SWpower0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.SWpower0[speciesIndex]);
                parameters128.fCalpha700[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fCalpha700[speciesIndex]);
                parameters128.fCg700[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fCg700[speciesIndex]);
                parameters128.m0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.m0[speciesIndex]);
                parameters128.fN0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fN0[speciesIndex]);
                parameters128.fNn[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fNn[speciesIndex]);
                parameters128.MaxAge[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.MaxAge[speciesIndex]);
                parameters128.nAge[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nAge[speciesIndex]);
                parameters128.rAge[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.rAge[speciesIndex]);

                // stem mortality & self-thinning
                parameters128.gammaN1[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.gammaN1[speciesIndex]);
                parameters128.gammaN0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.gammaN0[speciesIndex]);
                parameters128.tgammaN[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.tgammaN[speciesIndex]);
                parameters128.ngammaN[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.ngammaN[speciesIndex]);
                parameters128.wSx1000[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.wSx1000[speciesIndex]);
                parameters128.thinPower[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.thinPower[speciesIndex]);
                parameters128.mF[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.mF[speciesIndex]);
                parameters128.mR[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.mR[speciesIndex]);
                parameters128.mS[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.mS[speciesIndex]);

                // canopy structure and processes
                parameters128.SLA0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.SLA0[speciesIndex]);
                parameters128.SLA1[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.SLA1[speciesIndex]);
                parameters128.tSLA[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.tSLA[speciesIndex]);
                parameters128.k[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.k[speciesIndex]);
                parameters128.fullCanAge[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fullCanAge[speciesIndex]);
                parameters128.MaxIntcptn[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.MaxIntcptn[speciesIndex]);
                parameters128.LAImaxIntcptn[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.LAImaxIntcptn[speciesIndex]);
                parameters128.cVPD[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.cVPD[speciesIndex]);
                parameters128.alphaCx[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.alphaCx[speciesIndex]);
                parameters128.Y[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.Y[speciesIndex]);
                parameters128.MinCond[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.MinCond[speciesIndex]);
                parameters128.MaxCond[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.MaxCond[speciesIndex]);
                parameters128.LAIgcx[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.LAIgcx[speciesIndex]);
                parameters128.CoeffCond[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.CoeffCond[speciesIndex]);
                parameters128.BLcond[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.BLcond[speciesIndex]);
                parameters128.RGcGw[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.RGcGw[speciesIndex]);
                parameters128.D13CTissueDif[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.D13CTissueDif[speciesIndex]);
                parameters128.aFracDiffu[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.aFracDiffu[speciesIndex]);
                parameters128.bFracRubi[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.bFracRubi[speciesIndex]);

                // wood and stand properties
                parameters128.fracBB0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fracBB0[speciesIndex]);
                parameters128.fracBB1[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.fracBB1[speciesIndex]);
                parameters128.tBB[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.tBB[speciesIndex]);
                parameters128.rho0[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.rho0[speciesIndex]);
                parameters128.rho1[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.rho1[speciesIndex]);
                parameters128.tRho[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.tRho[speciesIndex]);
                parameters128.CrownShape[speciesIndex] = parameters.CrownShape[speciesIndex];

                // height and volume
                parameters128.aH[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.aH[speciesIndex]);
                parameters128.nHB[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nHB[speciesIndex]);
                parameters128.nHC[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nHC[speciesIndex]);
                parameters128.aV[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.aV[speciesIndex]);
                parameters128.nVB[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nVB[speciesIndex]);
                parameters128.nVH[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nVH[speciesIndex]);
                parameters128.nVBH[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nVBH[speciesIndex]);
                parameters128.aK[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.aK[speciesIndex]);
                parameters128.nKB[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nKB[speciesIndex]);
                parameters128.nKH[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nKH[speciesIndex]);
                parameters128.nKC[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nKC[speciesIndex]);
                parameters128.nKrh[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nKrh[speciesIndex]);
                parameters128.aHL[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.aHL[speciesIndex]);
                parameters128.nHLB[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nHLB[speciesIndex]);
                parameters128.nHLL[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nHLL[speciesIndex]);
                parameters128.nHLC[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nHLC[speciesIndex]);
                parameters128.nHLrh[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.nHLrh[speciesIndex]);

                // δ¹³C
                parameters128.Qa[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.Qa[speciesIndex]);
                parameters128.Qb[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.Qb[speciesIndex]);
                parameters128.gDM_mol[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.gDM_mol[speciesIndex]);
                parameters128.molPAR_MJ[speciesIndex] = AvxExtensions.BroadcastScalarToVector128(parameters.molPAR_MJ[speciesIndex]);
            }

            return parameters128;
        }
    }

    /// <summary>
    /// Tree species parameters: d_parameters in R, pars_i in Fortran, vectors of length n_sp.
    /// </summary>
    /// <remarks>
    /// 74 required + 9 optional parameters per species:
    /// - 12 biomass partitioning and turnover
    /// - 14 NPP and conductance modifiers
    /// - 9 mortality and self thinning
    /// - 15 canopy structure and processes
    /// - [4 δ¹³C]
    /// - 8 wood and stand properties
    /// - 12 requried + 5 optional height and volume
    /// - 4 radiation
    /// Maximum Markov chain size is 3.1 GB per species for 10 million iterations.
    /// </remarks>
    public class TreeSpeciesParameters<TFloat> : TreeSpeciesArray
        where TFloat: struct
    {
        // biomass partitioning and turnover
        /// <summary>
        /// Foliage: stem partitioning ratio at D=2 cm (ratio, Fortran pars_i[0])
        /// </summary>
        public TFloat[] pFS2 { get; private set; }
        /// <summary>
        /// Foliage: stem partitioning ratio at D=20 cm (ratio, Fortran pars_i[1])
        /// </summary>
        public TFloat[] pFS20 { get; private set; } // pars_i[1]
        /// <summary>
        /// Mean diameter: stem mass divider (regression coefficient, Fortran pars_i[2])
        /// </summary>
        public TFloat[] aWS { get; private set; }
        /// <summary>
        /// Mean diameter: reciprocal of divided stem mapss power (regression coefficient, Fortran pars_i[3])
        /// </summary>
        public TFloat[] nWS { get; private set; }
        /// <summary>
        /// Maximum fraction of NPP to roots (fraction, Fortran pars_i[4])
        /// </summary>
        public TFloat[] pRx { get; private set; }
        /// <summary>
        /// Minimum fraction of NPP to roots (fraction, Fortran pars_i[5])
        /// </summary>
        public TFloat[] pRn { get; private set; }
        /// <summary>
        /// Monthly litterfall rate in mature stands (fraction/month, Fortran pars_i[6])
        /// </summary>
        public TFloat[] gammaF1 { get; private set; }
        /// <summary>
        /// Monthly litterfall rate at age 0 (fraction/month, Fortran pars_i[7])
        /// </summary>
        public TFloat[] gammaF0 { get; private set; }
        /// <summary>
        /// Age at which litterfall rate has median value (years, Fortran pars_i[8])
        /// </summary>
        public TFloat[] tgammaF { get; private set; }
        /// <summary>
        /// Average monthly root turnover rate (fraction, Fortran pars_i[9])
        /// </summary>
        public TFloat[] gammaR { get; private set; }
        /// <summary>
        /// If deciduous, leaves are produced at end of this month (month number 1..2, 0 for evergreen, Fortran pars_i[10])
        /// </summary>
        public int[] leafgrow { get; private set; }
        /// <summary>
        /// If deciduous, leaves all fall at start of this month (month number 1..2, 0 for evergreen, Fortran pars_i[11])
        /// </summary>
        public int[] leaffall { get; private set; }

        // NPP and conductance modifiers
        /// <summary>
        /// Minimum temperature for growth (°C, Fortran pars_i[12])
        /// </summary>
        public TFloat[] Tmin { get; private set; }
        /// <summary>
        /// Optimum temperature for growth (°C, Fortran pars_i[13])
        /// </summary>
        public TFloat[] Topt { get; private set; }
        /// <summary>
        /// Maximum temperature for growth (°C, Fortran pars_i[14])
        /// </summary>
        public TFloat[] Tmax { get; private set; }
        /// <summary>
        /// Days production lost per frost day (multiplier, Fortran pars_i[15])
        /// </summary>
        public TFloat[] kF { get; private set; }
        /// <summary>
        /// Relative soil water availability at which the soil water modifier fq = 0.5 (Landsberg Fig. 9.3, 3-PGmix Eq. A9, Fortran pars_i[16])
        /// </summary>
        /// <remarks>Lower in sandier soils and higher in clay, likely lower on sites with greater soil water availability (mm/m).</remarks>
        public TFloat[] SWconst0 { get; private set; }
        /// <summary>
        /// Power of moisture ratio deficit (3-PGmix Eq. A9, Fortran pars_i[17])
        /// </summary>
        public TFloat[] SWpower0 { get; private set; }
        /// <summary>
        /// Assimilation enhancement (canopy quantumn efficiency) modifier at 700 ppm atmospheric CO₂ (Landsberg Fig. 9.3, 3-PGmix Eq. A11, Fortran pars_i[18])
        /// </summary>
        public TFloat[] fCalpha700 { get; private set; }
        /// <summary>
        /// Canopy conductance enhancement factor at 700 ppm atmospheric CO₂ (Fortran pars_i[19])
        /// </summary>
        public TFloat[] fCg700 { get; private set; }
        /// <summary>
        /// Value of root biomass partitioning factor m when fertility rating = 0 (3-PGmix root biomass allocation Eq. A26-27, Fortran pars_i[20])
        /// </summary>
        public TFloat[] m0 { get; private set; }
        /// <summary>
        /// Value of 'fNutr' when fertility rating = 0 (Fortran pars_i[21])
        /// </summary>
        public TFloat[] fN0 { get; private set; }
        /// <summary>
        /// Power of (1 - fertility) in 'fNutr' (has no effect when site fertility for a tree species = 0, Fortran pars_i[22])
        /// </summary>
        public TFloat[] fNn { get; private set; }
        /// <summary>
        /// Maximum stand age used in age modifier (Fortran pars_i[23])
        /// </summary>
        public TFloat[] MaxAge { get; private set; }
        /// <summary>
        /// Power of relative age in function for f_age (Fortran pars_i[24])
        /// </summary>
        public TFloat[] nAge { get; private set; }
        /// <summary>
        /// Relative age to give f_age = 0.5 (Fortran pars_i[25])
        /// </summary>
        public TFloat[] rAge { get; private set; }

        // stem mortality and self-thinning
        /// <summary>
        /// Density independent mortality rate for ages well above tgammaN (%/year, Fortran pars_i[26])
        /// </summary>
        public TFloat[] gammaN1 { get; private set; }
        /// <summary>
        /// Seedling mortality rate (age = 0) (%/year, Fortran pars_i[27])
        /// </summary>
        public TFloat[] gammaN0 { get; private set; }
        /// <summary>
        /// Age at which mortality rate has median value (Fortran pars_i[28])
        /// </summary>
        public TFloat[] tgammaN { get; private set; }
        /// <summary>
        /// Shape power of mortality response (Fortran pars_i[29])
        /// </summary>
        public TFloat[] ngammaN { get; private set; }
        /// <summary>
        /// Max.stem mass per tree @ 1000 trees/hectare (Fortran pars_i[30])
        /// </summary>
        public TFloat[] wSx1000 { get; private set; }
        /// <summary>
        /// Power in self-thinning rule (Fortran pars_i[31])
        /// </summary>
        public TFloat[] thinPower { get; private set; }
        /// <summary>
        /// Fraction of mean single-tree foliage biomass lost per dead tree (Fortran pars_i[32])
        /// </summary>
        public TFloat[] mF { get; private set; }
        /// <summary>
        /// Fraction of mean single-tree root biomass lost per dead tree (Fortran pars_i[33])
        /// </summary>
        public TFloat[] mR { get; private set; }
        /// <summary>
        /// Fraction of mean single-tree stem biomass lost per dead tree (Fortran pars_i[34])
        /// </summary>
        public TFloat[] mS { get; private set; }

        // canopy structure and processes
        /// <summary>
        /// Specific leaf area at age 0 (Fortran pars_i[35])
        /// </summary>
        public TFloat[] SLA0 { get; private set; }
        /// <summary>
        /// Specific leaf area for mature leaves (Fortran pars_i[36])
        /// </summary>
        public TFloat[] SLA1 { get; private set; }
        /// <summary>
        /// Age at which specific leaf area = (SLA0 + SLA1) / 2 (Fortran pars_i[37])
        /// </summary>
        public TFloat[] tSLA { get; private set; }
        /// <summary>
        /// Extinction coefficient for absorption of PAR by canopy (Fortran pars_i[38])
        /// </summary>
        public TFloat[] k { get; private set; }
        /// <summary>
        /// Age at canopy closure (years, 3-PGpjs only, Fortran pars_i[39])
        /// </summary>
        public TFloat[] fullCanAge { get; private set; }
        /// <summary>
        /// Maximum proportion of rainfall evaporated from canopy (Fortran pars_i[40])
        /// </summary>
        public TFloat[] MaxIntcptn { get; private set; }
        /// <summary>
        /// LAI for maximum rainfall interception (Fortran pars_i[41])
        /// </summary>
        public TFloat[] LAImaxIntcptn { get; private set; }
        /// <summary>
        /// LAI for 50% reduction of VPD in canopy (Fortran pars_i[42])
        /// </summary>
        public TFloat[] cVPD { get; private set; }
        /// <summary>
        /// Canopy quantum efficiency (molC/molPAR before modifiers, Fortran pars_i[43])
        /// </summary>
        public TFloat[] alphaCx { get; private set; }
        /// <summary>
        /// Ratio of NPP to GPP (fraction, Fortran pars_i[44])
        /// </summary>
        public TFloat[] Y { get; private set; }
        /// <summary>
        /// Minimum canopy conductance (m/s, Fortran pars_i[45])
        /// </summary>
        public TFloat[] MinCond { get; private set; }
        /// <summary>
        /// Maximum canopy conductance (m/s, Fortran pars_i[46])
        /// </summary>
        public TFloat[] MaxCond { get; private set; }
        /// <summary>
        /// LAI for maximum canopy conductance (m²/m², Fortran pars_i[47])
        /// </summary>
        public TFloat[] LAIgcx { get; private set; }
        /// <summary>
        /// Defines stomatal response to VPD (mbar⁻¹, Fortran pars_i[48])
        /// </summary>
        public TFloat[] CoeffCond { get; private set; }
        /// <summary>
        /// Canopy boundary layer conductance (m/s, Fortran pars_i[49])
        /// </summary>
        public TFloat[] BLcond { get; private set; }

        // δ¹³C
        /// <summary>
        /// The ratio of diffusivities of CO₂ and water vapour in air (ratio, Fortran pars_i[50])
        /// </summary>
        public TFloat[] RGcGw { get; private set; }
        /// <summary>
        /// δ¹³C difference of modelled tissue and new photosynthate (per mil, Fortran pars_i[51])
        /// </summary>
        public TFloat[] D13CTissueDif { get; private set; }
        /// <summary>
        /// Fractionation against ¹³C in diffusion (per mil, Fortran pars_i[52])
        /// </summary>
        public TFloat[] aFracDiffu { get; private set; }
        /// <summary>
        /// Enzymatic fractionation by rubisco (per mil, Fortran pars_i[53])
        /// </summary>
        public TFloat[] bFracRubi { get; private set; }

        // wood and stand properties
        /// <summary>
        /// Branch and bark fraction at age 0 (fraction, unsued if aV ≠ 0, Fortran pars_i[54])
        /// </summary>
        public TFloat[] fracBB0 { get; private set; }
        /// <summary>
        /// Branch and bark fraction for mature stands (fraction, unsued if aV ≠ 0, Fortran pars_i[55])
        /// </summary>
        public TFloat[] fracBB1 { get; private set; }
        /// <summary>
        /// Age at which fracBB = (fracBB0 + fracBB1) / 2 (years, unsued if aV ≠ 0, Fortran pars_i[56])
        /// </summary>
        public TFloat[] tBB { get; private set; }
        /// <summary>
        /// Wood density at age 0 (specific gravity = tons/m³, Fortran pars_i[57])
        /// </summary>
        public TFloat[] rho0 { get; private set; }
        /// <summary>
        /// Wood density of mature trees (specific gravity = tons/m³, Fortran pars_i[58])
        /// </summary>
        public TFloat[] rho1 { get; private set; }
        /// <summary>
        /// Age at which wood density rho = (rho0 + rho1) / 2 (years, Fortran pars_i[59])
        /// </summary>
        public TFloat[] tRho { get; private set; }
        /// <summary>
        /// 3-PGmix: crown shape of species (Fortran pars_i[60])
        /// </summary>
        public TreeCrownShape[] CrownShape { get; private set; }

        // height and volume
        /// <summary>
        /// Height regression: multiplier coefficient (Fortran pars_i[61])
        /// </summary>
        public TFloat[] aH { get; private set; }
        /// <summary>
        /// Height regression: DBH power (Fortran pars_i[62])
        /// </summary>
        public TFloat[] nHB { get; private set; }
        /// <summary>
        /// Height regression: competition power (Fortran pars_i[63])
        /// </summary>
        public TFloat[] nHC { get; private set; }
        /// <summary>
        /// Merchantable volume regression: multiplier coefficient (Fortran pars_i[64])
        /// </summary>
        public TFloat[] aV { get; private set; }
        /// <summary>
        /// Merchantable volume regression: DBH power (Fortran pars_i[65])
        /// </summary>
        public TFloat[] nVB { get; private set; }
        /// <summary>
        /// Merchantable volume regression: height power (Fortran pars_i[66])
        /// </summary>
        public TFloat[] nVH { get; private set; }
        /// <summary>
        /// Merchantable volume regression: DBH² height power  (Fortran pars_i[67])
        /// </summary>
        public TFloat[] nVBH { get; private set; }
        /// <summary>
        /// Crown width regression: multiplier coefficient (bias correction, Fortran pars_i[68])
        /// </summary>
        public TFloat[] aK { get; private set; }
        /// <summary>
        /// Crown width regression: DBH power (bias correction, Fortran pars_i[69])
        /// </summary>
        public TFloat[] nKB { get; private set; }
        /// <summary>
        /// Crown width regression: height power (bias correction, Fortran pars_i[70])
        /// </summary>
        public TFloat[] nKH { get; private set; }
        /// <summary>
        /// Crown width regression: competition power (bias correction, Fortran pars_i[71])
        /// </summary>
        public TFloat[] nKC { get; private set; }
        /// <summary>
        /// Crown width regression: relative height (bias correction, Fortran pars_i[72])
        /// </summary>
        public TFloat[] nKrh { get; private set; }
        /// <summary>
        /// Crown length regression: multiplier (bias correction, Fortran pars_i[73])
        /// </summary>
        public TFloat[] aHL { get; private set; }
        /// <summary>
        /// Crown length regression: DBH power (Fortran pars_i[74])
        /// </summary>
        public TFloat[] nHLB { get; private set; }
        /// <summary>
        /// Crown length regression: LAI power, <see cref="ThreePGHeightModel.Power"> only (bias correction, Fortran pars_i[75])
        /// </summary>
        public TFloat[] nHLL { get; private set; }
        /// <summary>
        /// Crown length regression: competition power (Fortran pars_i[76])
        /// </summary>
        public TFloat[] nHLC { get; private set; }
        /// <summary>
        /// Crown length regression: relative height power, <see cref="ThreePGHeightModel.Power"> only (bias correction, Fortran pars_i[77])
        /// </summary>
        public TFloat[] nHLrh { get; private set; }

        // radiation
        /// <summary>
        ///  (Fortran pars_i[78])
        /// </summary>
        public TFloat[] Qa { get; private set; }
        /// <summary>
        ///  (Fortran pars_i[79])
        /// </summary>
        public TFloat[] Qb { get; private set; }
        /// <summary>
        ///  (Fortran pars_i[80])
        /// </summary>
        public TFloat[] gDM_mol { get; private set; }
        /// <summary>
        ///  (Fortran pars_i[81])
        /// </summary>
        public TFloat[] molPAR_MJ { get; private set; }

        public TreeSpeciesParameters()
        {
            // biomass partitioning and turnover
            this.pFS2 = Array.Empty<TFloat>();
            this.pFS20 = Array.Empty<TFloat>(); // pars_i[1]
            this.aWS = Array.Empty<TFloat>();
            this.nWS = Array.Empty<TFloat>();
            this.pRx = Array.Empty<TFloat>();
            this.pRn = Array.Empty<TFloat>();
            this.gammaF1 = Array.Empty<TFloat>();
            this.gammaF0 = Array.Empty<TFloat>();
            this.tgammaF = Array.Empty<TFloat>();
            this.gammaR = Array.Empty<TFloat>();
            this.leafgrow = Array.Empty<int>();
            this.leaffall = Array.Empty<int>();

            // NPP & conductance modifiers
            this.Tmin = Array.Empty<TFloat>();
            this.Topt = Array.Empty<TFloat>();
            this.Tmax = Array.Empty<TFloat>();
            this.kF = Array.Empty<TFloat>();
            this.SWconst0 = Array.Empty<TFloat>();
            this.SWpower0 = Array.Empty<TFloat>();
            this.fCalpha700 = Array.Empty<TFloat>();
            this.fCg700 = Array.Empty<TFloat>();
            this.m0 = Array.Empty<TFloat>();
            this.fN0 = Array.Empty<TFloat>();
            this.fNn = Array.Empty<TFloat>();
            this.MaxAge = Array.Empty<TFloat>();
            this.nAge = Array.Empty<TFloat>();
            this.rAge = Array.Empty<TFloat>();

            // stem mortality & self-thinning
            this.gammaN1 = Array.Empty<TFloat>();
            this.gammaN0 = Array.Empty<TFloat>();
            this.tgammaN = Array.Empty<TFloat>();
            this.ngammaN = Array.Empty<TFloat>();
            this.wSx1000 = Array.Empty<TFloat>();
            this.thinPower = Array.Empty<TFloat>();
            this.mF = Array.Empty<TFloat>();
            this.mR = Array.Empty<TFloat>();
            this.mS = Array.Empty<TFloat>();

            // canopy structure and processes
            this.SLA0 = Array.Empty<TFloat>();
            this.SLA1 = Array.Empty<TFloat>();
            this.tSLA = Array.Empty<TFloat>();
            this.k = Array.Empty<TFloat>();
            this.fullCanAge = Array.Empty<TFloat>();
            this.MaxIntcptn = Array.Empty<TFloat>();
            this.LAImaxIntcptn = Array.Empty<TFloat>();
            this.cVPD = Array.Empty<TFloat>();
            this.alphaCx = Array.Empty<TFloat>();
            this.Y = Array.Empty<TFloat>();
            this.MinCond = Array.Empty<TFloat>();
            this.MaxCond = Array.Empty<TFloat>();
            this.LAIgcx = Array.Empty<TFloat>();
            this.CoeffCond = Array.Empty<TFloat>();
            this.BLcond = Array.Empty<TFloat>();
            this.RGcGw = Array.Empty<TFloat>();
            this.D13CTissueDif = Array.Empty<TFloat>();
            this.aFracDiffu = Array.Empty<TFloat>();
            this.bFracRubi = Array.Empty<TFloat>();

            // wood and stand properties
            this.fracBB0 = Array.Empty<TFloat>();
            this.fracBB1 = Array.Empty<TFloat>();
            this.tBB = Array.Empty<TFloat>();
            this.rho0 = Array.Empty<TFloat>();
            this.rho1 = Array.Empty<TFloat>();
            this.tRho = Array.Empty<TFloat>();
            this.CrownShape = new TreeCrownShape[n_sp];

            // height and volume
            this.aH = Array.Empty<TFloat>();
            this.nHB = Array.Empty<TFloat>();
            this.nHC = Array.Empty<TFloat>();
            this.aV = Array.Empty<TFloat>();
            this.nVB = Array.Empty<TFloat>();
            this.nVH = Array.Empty<TFloat>();
            this.nVBH = Array.Empty<TFloat>();
            this.aK = Array.Empty<TFloat>();
            this.nKB = Array.Empty<TFloat>();
            this.nKH = Array.Empty<TFloat>();
            this.nKC = Array.Empty<TFloat>();
            this.nKrh = Array.Empty<TFloat>();
            this.aHL = Array.Empty<TFloat>();
            this.nHLB = Array.Empty<TFloat>();
            this.nHLL = Array.Empty<TFloat>();
            this.nHLC = Array.Empty<TFloat>();
            this.nHLrh = Array.Empty<TFloat>();

            // δ¹³C
            this.Qa = Array.Empty<TFloat>();
            this.Qb = Array.Empty<TFloat>();
            this.gDM_mol = Array.Empty<TFloat>();
            this.molPAR_MJ = Array.Empty<TFloat>();
        }

        public override void AllocateSpecies(int additionalSpecies)
        {
            base.AllocateSpecies(additionalSpecies);

            // biomass partitioning and turnover
            this.pFS2 = this.pFS2.Resize(this.n_sp);
            this.pFS20 = this.pFS20.Resize(this.n_sp); // pars_i[1]
            this.aWS = this.aWS.Resize(this.n_sp);
            this.nWS = this.nWS.Resize(this.n_sp);
            this.pRx = this.pRx.Resize(this.n_sp);
            this.pRn = this.pRn.Resize(this.n_sp);
            this.gammaF1 = this.gammaF1.Resize(this.n_sp);
            this.gammaF0 = this.gammaF0.Resize(this.n_sp);
            this.tgammaF = this.tgammaF.Resize(this.n_sp);
            this.gammaR = this.gammaR.Resize(this.n_sp);
            this.leafgrow = this.leafgrow.Resize(this.n_sp);
            this.leaffall = this.leaffall.Resize(this.n_sp);

            // NPP & conductance modifiers
            this.Tmin = this.Tmin.Resize(this.n_sp);
            this.Topt = this.Topt.Resize(this.n_sp);
            this.Tmax = this.Tmax.Resize(this.n_sp);
            this.kF = this.kF.Resize(this.n_sp);
            this.SWconst0 = this.SWconst0.Resize(this.n_sp);
            this.SWpower0 = this.SWpower0.Resize(this.n_sp);
            this.fCalpha700 = this.fCalpha700.Resize(this.n_sp);
            this.fCg700 = this.fCg700.Resize(this.n_sp);
            this.m0 = this.m0.Resize(this.n_sp);
            this.fN0 = this.fN0.Resize(this.n_sp);
            this.fNn = this.fNn.Resize(this.n_sp);
            this.MaxAge = this.MaxAge.Resize(this.n_sp);
            this.nAge = this.nAge.Resize(this.n_sp);
            this.rAge = this.rAge.Resize(this.n_sp);

            // stem mortality & self-thinning
            this.gammaN1 = this.gammaN1.Resize(this.n_sp);
            this.gammaN0 = this.gammaN0.Resize(this.n_sp);
            this.tgammaN = this.tgammaN.Resize(this.n_sp);
            this.ngammaN = this.ngammaN.Resize(this.n_sp);
            this.wSx1000 = this.wSx1000.Resize(this.n_sp);
            this.thinPower = this.thinPower.Resize(this.n_sp);
            this.mF = this.mF.Resize(this.n_sp);
            this.mR = this.mR.Resize(this.n_sp);
            this.mS = this.mS.Resize(this.n_sp);

            // canopy structure and processes
            this.SLA0 = this.SLA0.Resize(this.n_sp);
            this.SLA1 = this.SLA1.Resize(this.n_sp);
            this.tSLA = this.tSLA.Resize(this.n_sp);
            this.k = this.k.Resize(this.n_sp);
            this.fullCanAge = this.fullCanAge.Resize(this.n_sp);
            this.MaxIntcptn = this.MaxIntcptn.Resize(this.n_sp);
            this.LAImaxIntcptn = this.LAImaxIntcptn.Resize(this.n_sp);
            this.cVPD = this.cVPD.Resize(this.n_sp);
            this.alphaCx = this.alphaCx.Resize(this.n_sp);
            this.Y = this.Y.Resize(this.n_sp);
            this.MinCond = this.MinCond.Resize(this.n_sp);
            this.MaxCond = this.MaxCond.Resize(this.n_sp);
            this.LAIgcx = this.LAIgcx.Resize(this.n_sp);
            this.CoeffCond = this.CoeffCond.Resize(this.n_sp);
            this.BLcond = this.BLcond.Resize(this.n_sp);
            this.RGcGw = this.RGcGw.Resize(this.n_sp);
            this.D13CTissueDif = this.D13CTissueDif.Resize(this.n_sp);
            this.aFracDiffu = this.aFracDiffu.Resize(this.n_sp);
            this.bFracRubi = this.bFracRubi.Resize(this.n_sp);

            // wood and stand properties
            this.fracBB0 = this.fracBB0.Resize(this.n_sp);
            this.fracBB1 = this.fracBB1.Resize(this.n_sp);
            this.tBB = this.tBB.Resize(this.n_sp);
            this.rho0 = this.rho0.Resize(this.n_sp);
            this.rho1 = this.rho1.Resize(this.n_sp);
            this.tRho = this.tRho.Resize(this.n_sp);
            this.CrownShape = this.CrownShape.Resize(this.n_sp);

            // height and volume
            this.aH = this.aH.Resize(this.n_sp);
            this.nHB = this.nHB.Resize(this.n_sp);
            this.nHC = this.nHC.Resize(this.n_sp);
            this.aV = this.aV.Resize(this.n_sp);
            this.nVB = this.nVB.Resize(this.n_sp);
            this.nVH = this.nVH.Resize(this.n_sp);
            this.nVBH = this.nVBH.Resize(this.n_sp);
            this.aK = this.aK.Resize(this.n_sp);
            this.nKB = this.nKB.Resize(this.n_sp);
            this.nKH = this.nKH.Resize(this.n_sp);
            this.nKC = this.nKC.Resize(this.n_sp);
            this.nKrh = this.nKrh.Resize(this.n_sp);
            this.aHL = this.aHL.Resize(this.n_sp);
            this.nHLB = this.nHLB.Resize(this.n_sp);
            this.nHLL = this.nHLL.Resize(this.n_sp);
            this.nHLC = this.nHLC.Resize(this.n_sp);
            this.nHLrh = this.nHLrh.Resize(this.n_sp);

            // δ¹³C
            this.Qa = this.Qa.Resize(this.n_sp);
            this.Qb = this.Qb.Resize(this.n_sp);
            this.gDM_mol = this.gDM_mol.Resize(this.n_sp);
            this.molPAR_MJ = this.molPAR_MJ.Resize(this.n_sp);
        }

        public override void AllocateSpecies(string[] names)
        {
            int existingSpecies = this.n_sp;
            this.AllocateSpecies(names.Length);
            Array.Copy(names, 0, this.Species, existingSpecies, names.Length);
        }

        public TreeSpeciesParameters<TFloat> Filter(SiteTreeSpecies treeSpecies)
        {
            TreeSpeciesParameters<TFloat> filteredParameters = new();
            filteredParameters.AllocateSpecies(treeSpecies.Species);

            for (int destinationIndex = 0; destinationIndex < treeSpecies.n_sp; ++destinationIndex)
            {
                int sourceIndex = this.Species.FindIndex(treeSpecies.Species[destinationIndex]);
                if (sourceIndex == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(treeSpecies));
                }

                // biomass partitioning and turnover
                filteredParameters.pFS2[destinationIndex] = this.pFS2[sourceIndex];
                filteredParameters.pFS20[destinationIndex] = this.pFS20[sourceIndex];
                filteredParameters.aWS[destinationIndex] = this.aWS[sourceIndex];
                filteredParameters.nWS[destinationIndex] = this.nWS[sourceIndex];
                filteredParameters.pRx[destinationIndex] = this.pRx[sourceIndex];
                filteredParameters.pRn[destinationIndex] = this.pRn[sourceIndex];
                filteredParameters.gammaF1[destinationIndex] = this.gammaF1[sourceIndex];
                filteredParameters.gammaF0[destinationIndex] = this.gammaF0[sourceIndex];
                filteredParameters.tgammaF[destinationIndex] = this.tgammaF[sourceIndex];
                filteredParameters.gammaR[destinationIndex] = this.gammaR[sourceIndex];
                filteredParameters.leafgrow[destinationIndex] = this.leafgrow[sourceIndex];
                filteredParameters.leaffall[destinationIndex] = this.leaffall[sourceIndex];

                // NPP & conductance modifiers
                filteredParameters.Tmin[destinationIndex] = this.Tmin[sourceIndex];
                filteredParameters.Topt[destinationIndex] = this.Topt[sourceIndex];
                filteredParameters.Tmax[destinationIndex] = this.Tmax[sourceIndex];
                filteredParameters.kF[destinationIndex] = this.kF[sourceIndex];
                filteredParameters.SWconst0[destinationIndex] = this.SWconst0[sourceIndex];
                filteredParameters.SWpower0[destinationIndex] = this.SWpower0[sourceIndex];
                filteredParameters.fCalpha700[destinationIndex] = this.fCalpha700[sourceIndex];
                filteredParameters.fCg700[destinationIndex] = this.fCg700[sourceIndex];
                filteredParameters.m0[destinationIndex] = this.m0[sourceIndex];
                filteredParameters.fN0[destinationIndex] = this.fN0[sourceIndex];
                filteredParameters.fNn[destinationIndex] = this.fNn[sourceIndex];
                filteredParameters.MaxAge[destinationIndex] = this.MaxAge[sourceIndex];
                filteredParameters.nAge[destinationIndex] = this.nAge[sourceIndex];
                filteredParameters.rAge[destinationIndex] = this.rAge[sourceIndex];

                // stem mortality & self-thinning
                filteredParameters.gammaN1[destinationIndex] = this.gammaN1[sourceIndex];
                filteredParameters.gammaN0[destinationIndex] = this.gammaN0[sourceIndex];
                filteredParameters.tgammaN[destinationIndex] = this.tgammaN[sourceIndex];
                filteredParameters.ngammaN[destinationIndex] = this.ngammaN[sourceIndex];
                filteredParameters.wSx1000[destinationIndex] = this.wSx1000[sourceIndex];
                filteredParameters.thinPower[destinationIndex] = this.thinPower[sourceIndex];
                filteredParameters.mF[destinationIndex] = this.mF[sourceIndex];
                filteredParameters.mR[destinationIndex] = this.mR[sourceIndex];
                filteredParameters.mS[destinationIndex] = this.mS[sourceIndex];

                // canopy structure and processes
                filteredParameters.SLA0[destinationIndex] = this.SLA0[sourceIndex];
                filteredParameters.SLA1[destinationIndex] = this.SLA1[sourceIndex];
                filteredParameters.tSLA[destinationIndex] = this.tSLA[sourceIndex];
                filteredParameters.k[destinationIndex] = this.k[sourceIndex];
                filteredParameters.fullCanAge[destinationIndex] = this.fullCanAge[sourceIndex];
                filteredParameters.MaxIntcptn[destinationIndex] = this.MaxIntcptn[sourceIndex];
                filteredParameters.LAImaxIntcptn[destinationIndex] = this.LAImaxIntcptn[sourceIndex];
                filteredParameters.cVPD[destinationIndex] = this.cVPD[sourceIndex];
                filteredParameters.alphaCx[destinationIndex] = this.alphaCx[sourceIndex];
                filteredParameters.Y[destinationIndex] = this.Y[sourceIndex];
                filteredParameters.MinCond[destinationIndex] = this.MinCond[sourceIndex];
                filteredParameters.MaxCond[destinationIndex] = this.MaxCond[sourceIndex];
                filteredParameters.LAIgcx[destinationIndex] = this.LAIgcx[sourceIndex];
                filteredParameters.CoeffCond[destinationIndex] = this.CoeffCond[sourceIndex];
                filteredParameters.BLcond[destinationIndex] = this.BLcond[sourceIndex];
                filteredParameters.RGcGw[destinationIndex] = this.RGcGw[sourceIndex];
                filteredParameters.D13CTissueDif[destinationIndex] = this.D13CTissueDif[sourceIndex];
                filteredParameters.aFracDiffu[destinationIndex] = this.aFracDiffu[sourceIndex];
                filteredParameters.bFracRubi[destinationIndex] = this.bFracRubi[sourceIndex];

                // wood and stand properties
                filteredParameters.fracBB0[destinationIndex] = this.fracBB0[sourceIndex];
                filteredParameters.fracBB1[destinationIndex] = this.fracBB1[sourceIndex];
                filteredParameters.tBB[destinationIndex] = this.tBB[sourceIndex];
                filteredParameters.rho0[destinationIndex] = this.rho0[sourceIndex];
                filteredParameters.rho1[destinationIndex] = this.rho1[sourceIndex];
                filteredParameters.tRho[destinationIndex] = this.tRho[sourceIndex];
                filteredParameters.CrownShape[destinationIndex] = this.CrownShape[sourceIndex];

                // height and volume
                filteredParameters.aH[destinationIndex] = this.aH[sourceIndex];
                filteredParameters.nHB[destinationIndex] = this.nHB[sourceIndex];
                filteredParameters.nHC[destinationIndex] = this.nHC[sourceIndex];
                filteredParameters.aV[destinationIndex] = this.aV[sourceIndex];
                filteredParameters.nVB[destinationIndex] = this.nVB[sourceIndex];
                filteredParameters.nVH[destinationIndex] = this.nVH[sourceIndex];
                filteredParameters.nVBH[destinationIndex] = this.nVBH[sourceIndex];
                filteredParameters.aK[destinationIndex] = this.aK[sourceIndex];
                filteredParameters.nKB[destinationIndex] = this.nKB[sourceIndex];
                filteredParameters.nKH[destinationIndex] = this.nKH[sourceIndex];
                filteredParameters.nKC[destinationIndex] = this.nKC[sourceIndex];
                filteredParameters.nKrh[destinationIndex] = this.nKrh[sourceIndex];
                filteredParameters.aHL[destinationIndex] = this.aHL[sourceIndex];
                filteredParameters.nHLB[destinationIndex] = this.nHLB[sourceIndex];
                filteredParameters.nHLL[destinationIndex] = this.nHLL[sourceIndex];
                filteredParameters.nHLC[destinationIndex] = this.nHLC[sourceIndex];
                filteredParameters.nHLrh[destinationIndex] = this.nHLrh[sourceIndex];

                // δ¹³C
                filteredParameters.Qa[destinationIndex] = this.Qa[sourceIndex];
                filteredParameters.Qb[destinationIndex] = this.Qb[sourceIndex];
                filteredParameters.gDM_mol[destinationIndex] = this.gDM_mol[sourceIndex];
                filteredParameters.molPAR_MJ[destinationIndex] = this.molPAR_MJ[sourceIndex];
            }

            return filteredParameters;
        }
    }
}
