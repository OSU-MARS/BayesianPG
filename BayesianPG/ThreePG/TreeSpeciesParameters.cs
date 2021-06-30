using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// tree species parameters: d_parameters in R, pars_i in Fortran
    /// Vectors of length n_sp.
    /// </summary>
    public class TreeSpeciesParameters : TreeSpeciesArray
    {
        // biomass partitioning and turnover
        /// <summary>
        /// Foliage:stem partitioning ratio at D=2 cm (pars_i[0])
        /// </summary>
        public float[] pFS2 { get; private set; }
        /// <summary>
        /// Foliage:stem partitioning ratio at D=20 cm (pars_i[1])
        /// </summary>
        public float[] pFS20 { get; private set; } // pars_i[1]
        /// <summary>
        /// Constant in the stem mass v.diam.relationship (pars_i[2])
        /// </summary>
        public float[] aWS { get; private set; }
        /// <summary>
        /// Power in the stem mass v.diam.relationship (pars_i[3])
        /// </summary>
        public float[] nWS { get; private set; }
        /// <summary>
        /// Maximum fraction of NPP to roots (pars_i[4])
        /// </summary>
        public float[] pRx { get; private set; }
        /// <summary>
        /// Minimum fraction of NPP to roots (pars_i[5])
        /// </summary>
        public float[] pRn { get; private set; }
        /// <summary>
        /// Coefficients in monthly litterfall rate (pars_i[6])
        /// </summary>
        public float[] gammaF1 { get; private set; }
        /// <summary>
        /// Coefficients in monthly litterfall rate (pars_i[7])
        /// </summary>
        public float[] gammaF0 { get; private set; }
        /// <summary>
        /// Coefficients in monthly litterfall rate (pars_i[8])
        /// </summary>
        public float[] tgammaF { get; private set; }
        /// <summary>
        /// Average monthly root turnover rate (pars_i[9])
        /// </summary>
        public float[] gammaR { get; private set; }
        /// <summary>
        /// If deciduous, leaves are produced at end of this month (pars_i[10])
        /// </summary>
        public int[] leafgrow { get; private set; }
        /// <summary>
        /// If deciduous, leaves all fall at start of this month (pars_i[11])
        /// </summary>
        public int[] leaffall { get; private set; }

        // NPP & conductance modifiers
        /// <summary>
        /// Minimum temperature for growth (pars_i[12])
        /// </summary>
        public float[] Tmin { get; private set; }
        /// <summary>
        /// Optimum temperature for growth (pars_i[13])
        /// </summary>
        public float[] Topt { get; private set; }
        /// <summary>
        /// Maximum temperature for growth (pars_i[14])
        /// </summary>
        public float[] Tmax { get; private set; }
        /// <summary>
        /// Days production lost per frost day (pars_i[15])
        /// </summary>
        public float[] kF { get; private set; }
        /// <summary>
        /// Moisture ratio deficit for fq = 0.5 (pars_i[16])
        /// </summary>
        public float[] SWconst0 { get; private set; }
        /// <summary>
        /// Power of moisture ratio deficit (pars_i[17])
        /// </summary>
        public float[] SWpower0 { get; private set; }
        /// <summary>
        /// Assimilation enhancement factor at 700 ppm (pars_i[18])
        /// </summary>
        public float[] fCalpha700 { get; private set; }
        /// <summary>
        /// Canopy conductance enhancement factor at 700 ppm (pars_i[19])
        /// </summary>
        public float[] fCg700 { get; private set; }
        /// <summary>
        /// Value of 'm' when FR = 0 (pars_i[20])
        /// </summary>
        public float[] m0 { get; private set; }
        /// <summary>
        /// Value of 'fNutr' when FR = 0 (pars_i[21])
        /// </summary>
        public float[] fN0 { get; private set; }
        /// <summary>
        /// Power of(1-FR) in 'fNutr' (pars_i[22])
        /// </summary>
        public float[] fNn { get; private set; }
        /// <summary>
        /// Maximum stand age used in age modifier (pars_i[23])
        /// </summary>
        public float[] MaxAge { get; private set; }
        /// <summary>
        /// Power of relative age in function for f_age (pars_i[24])
        /// </summary>
        public float[] nAge { get; private set; }
        /// <summary>
        /// Relative age to give f_age = 0.5 (pars_i[25])
        /// </summary>
        public float[] rAge { get; private set; }

        // Stem mortality & self-thinning
        /// <summary>
        /// Mortality rate for large t (pars_i[26])
        /// </summary>
        public float[] gammaN1 { get; private set; }
        /// <summary>
        /// Seedling mortality rate(t = 0) (pars_i[27])
        /// </summary>
        public float[] gammaN0 { get; private set; }
        /// <summary>
        /// Age at which mortality rate has median value (pars_i[28])
        /// </summary>
        public float[] tgammaN { get; private set; }
        /// <summary>
        /// Shape of mortality response (pars_i[29])
        /// </summary>
        public float[] ngammaN { get; private set; }
        /// <summary>
        /// Max.stem mass per tree @ 1000 trees/hectare (pars_i[30])
        /// </summary>
        public float[] wSx1000 { get; private set; }
        /// <summary>
        /// Power in self-thinning rule (pars_i[31])
        /// </summary>
        public float[] thinPower { get; private set; }
        /// <summary>
        /// Fraction mean single-tree foliage biomass lost per dead tree (pars_i[32])
        /// </summary>
        public float[] mF { get; private set; }
        /// <summary>
        /// Fraction mean single-tree root biomass lost per dead tree (pars_i[33])
        /// </summary>
        public float[] mR { get; private set; }
        /// <summary>
        /// Fraction mean single-tree stem biomass lost per dead tree (pars_i[34])
        /// </summary>
        public float[] mS { get; private set; }

        // canopy structure and processes
        /// <summary>
        /// Specific leaf area at age 0 (pars_i[35])
        /// </summary>
        public float[] SLA0 { get; private set; }
        /// <summary>
        /// Specific leaf area for mature leaves (pars_i[36])
        /// </summary>
        public float[] SLA1 { get; private set; }
        /// <summary>
        /// Age at which specific leaf area = (SLA0 + SLA1) / 2 (pars_i[37])
        /// </summary>
        public float[] tSLA { get; private set; }
        /// <summary>
        /// Extinction coefficient for absorption of PAR by canopy (pars_i[38])
        /// </summary>
        public float[] k { get; private set; }
        /// <summary>
        /// Age at canopy closure (pars_i[39])
        /// </summary>
        public float[] fullCanAge { get; private set; }
        /// <summary>
        /// Maximum proportion of rainfall evaporated from canopy (pars_i[40])
        /// </summary>
        public float[] MaxIntcptn { get; private set; }
        /// <summary>
        /// LAI for maximum rainfall interception (pars_i[41])
        /// </summary>
        public float[] LAImaxIntcptn { get; private set; }
        /// <summary>
        /// 'DF LAI for 50% reduction of VPD in canopy (pars_i[42])
        /// </summary>
        public float[] cVPD { get; private set; }
        /// <summary>
        /// Canopy quantum efficiency (pars_i[43])
        /// </summary>
        public float[] alphaCx { get; private set; }
        /// <summary>
        /// Ratio NPP/GPP (pars_i[44])
        /// </summary>
        public float[] Y { get; private set; }
        /// <summary>
        /// Minimum canopy conductance (pars_i[45])
        /// </summary>
        public float[] MinCond { get; private set; }
        /// <summary>
        /// Maximum canopy conductance (pars_i[46])
        /// </summary>
        public float[] MaxCond { get; private set; }
        /// <summary>
        /// LAI for maximum canopy conductance (pars_i[47])
        /// </summary>
        public float[] LAIgcx { get; private set; }
        /// <summary>
        /// Defines stomatal response to VPD (pars_i[48])
        /// </summary>
        public float[] CoeffCond { get; private set; }
        /// <summary>
        /// Canopy boundary layer conductance (pars_i[49])
        /// </summary>
        public float[] BLcond { get; private set; }
        /// <summary>
        /// The ratio of diffusivities of CO2 and water vapour in air (pars_i[50])
        /// </summary>
        public float[] RGcGw { get; private set; }
        /// <summary>
        /// δ¹³C difference of modelled tissue and new photosynthate (pars_i[51])
        /// </summary>
        public float[] D13CTissueDif { get; private set; }
        /// <summary>
        /// Fractionation against ¹³C in diffusion (pars_i[52])
        /// </summary>
        public float[] aFracDiffu { get; private set; }
        /// <summary>
        /// Enzymatic fractionation by Rubisco (pars_i[53])
        /// </summary>
        public float[] bFracRubi { get; private set; }

        // wood and stand properties
        /// <summary>
        /// Branch and bark fraction at age 0 (pars_i[54])
        /// </summary>
        public float[] fracBB0 { get; private set; }
        /// <summary>
        /// Branch and bark fraction for mature stands (pars_i[55])
        /// </summary>
        public float[] fracBB1 { get; private set; }
        /// <summary>
        /// Age at which fracBB = (fracBB0 + fracBB1) / 2 (pars_i[56])
        /// </summary>
        public float[] tBB { get; private set; }
        /// <summary>
        /// Minimum basic density - for young trees (pars_i[57])
        /// </summary>
        public float[] rhoMin { get; private set; }
        /// <summary>
        /// Maximum basic density - for older trees (pars_i[58])
        /// </summary>
        public float[] rhoMax { get; private set; }
        /// <summary>
        /// Age at which rho = (rhoMin + rhoMax) / 2 (pars_i[59])
        /// </summary>
        public float[] tRho { get; private set; }
        /// <summary>
        /// 3-PGmix: crown shape of species (pars_i[60])
        /// </summary>
        public TreeCrownShape[] CrownShape { get; private set; }

        // height and volume
        /// <summary>
        ///  (pars_i[61])
        /// </summary>
        public float[] aH { get; private set; }
        /// <summary>
        ///  (pars_i[62])
        /// </summary>
        public float[] nHB { get; private set; }
        /// <summary>
        ///  (pars_i[63])
        /// </summary>
        public float[] nHC { get; private set; }
        /// <summary>
        ///  (pars_i[64])
        /// </summary>
        public float[] aV { get; private set; }
        /// <summary>
        ///  (pars_i[65])
        /// </summary>
        public float[] nVB { get; private set; }
        /// <summary>
        ///  (pars_i[66])
        /// </summary>
        public float[] nVH { get; private set; }
        /// <summary>
        ///  (pars_i[67])
        /// </summary>
        public float[] nVBH { get; private set; }
        /// <summary>
        ///  (pars_i[68])
        /// </summary>
        public float[] aK { get; private set; }
        /// <summary>
        ///  (pars_i[69])
        /// </summary>
        public float[] nKB { get; private set; }
        /// <summary>
        ///  (pars_i[70])
        /// </summary>
        public float[] nKH { get; private set; }
        /// <summary>
        ///  (pars_i[71])
        /// </summary>
        public float[] nKC { get; private set; }
        /// <summary>
        ///  (pars_i[72])
        /// </summary>
        public float[] nKrh { get; private set; }
        /// <summary>
        ///  (pars_i[73])
        /// </summary>
        public float[] aHL { get; private set; }
        /// <summary>
        ///  (pars_i[74])
        /// </summary>
        public float[] nHLB { get; private set; }
        /// <summary>
        ///  (pars_i[75])
        /// </summary>
        public float[] nHLL { get; private set; }
        /// <summary>
        ///  (pars_i[76])
        /// </summary>
        public float[] nHLC { get; private set; }
        /// <summary>
        ///  (pars_i[77])
        /// </summary>
        public float[] nHLrh { get; private set; }

        // δ¹³C
        /// <summary>
        ///  (pars_i[78])
        /// </summary>
        public float[] Qa { get; private set; }
        /// <summary>
        ///  (pars_i[79])
        /// </summary>
        public float[] Qb { get; private set; }
        /// <summary>
        ///  (pars_i[80])
        /// </summary>
        public float[] gDM_mol { get; private set; }
        /// <summary>
        ///  (pars_i[81])
        /// </summary>
        public float[] molPAR_MJ { get; private set; }

        public TreeSpeciesParameters()
        {
            // biomass partitioning and turnover
            this.pFS2 = Array.Empty<float>();
            this.pFS20 = Array.Empty<float>(); // pars_i[1]
            this.aWS = Array.Empty<float>();
            this.nWS = Array.Empty<float>();
            this.pRx = Array.Empty<float>();
            this.pRn = Array.Empty<float>();
            this.gammaF1 = Array.Empty<float>();
            this.gammaF0 = Array.Empty<float>();
            this.tgammaF = Array.Empty<float>();
            this.gammaR = Array.Empty<float>();
            this.leafgrow = Array.Empty<int>();
            this.leaffall = Array.Empty<int>();

            // NPP & conductance modifiers
            this.Tmin = Array.Empty<float>();
            this.Topt = Array.Empty<float>();
            this.Tmax = Array.Empty<float>();
            this.kF = Array.Empty<float>();
            this.SWconst0 = Array.Empty<float>();
            this.SWpower0 = Array.Empty<float>();
            this.fCalpha700 = Array.Empty<float>();
            this.fCg700 = Array.Empty<float>();
            this.m0 = Array.Empty<float>();
            this.fN0 = Array.Empty<float>();
            this.fNn = Array.Empty<float>();
            this.MaxAge = Array.Empty<float>();
            this.nAge = Array.Empty<float>();
            this.rAge = Array.Empty<float>();

            // stem mortality & self-thinning
            this.gammaN1 = Array.Empty<float>();
            this.gammaN0 = Array.Empty<float>();
            this.tgammaN = Array.Empty<float>();
            this.ngammaN = Array.Empty<float>();
            this.wSx1000 = Array.Empty<float>();
            this.thinPower = Array.Empty<float>();
            this.mF = Array.Empty<float>();
            this.mR = Array.Empty<float>();
            this.mS = Array.Empty<float>();

            // canopy structure and processes
            this.SLA0 = Array.Empty<float>();
            this.SLA1 = Array.Empty<float>();
            this.tSLA = Array.Empty<float>();
            this.k = Array.Empty<float>();
            this.fullCanAge = Array.Empty<float>();
            this.MaxIntcptn = Array.Empty<float>();
            this.LAImaxIntcptn = Array.Empty<float>();
            this.cVPD = Array.Empty<float>();
            this.alphaCx = Array.Empty<float>();
            this.Y = Array.Empty<float>();
            this.MinCond = Array.Empty<float>();
            this.MaxCond = Array.Empty<float>();
            this.LAIgcx = Array.Empty<float>();
            this.CoeffCond = Array.Empty<float>();
            this.BLcond = Array.Empty<float>();
            this.RGcGw = Array.Empty<float>();
            this.D13CTissueDif = Array.Empty<float>();
            this.aFracDiffu = Array.Empty<float>();
            this.bFracRubi = Array.Empty<float>();

            // wood and stand properties
            this.fracBB0 = Array.Empty<float>();
            this.fracBB1 = Array.Empty<float>();
            this.tBB = Array.Empty<float>();
            this.rhoMin = Array.Empty<float>();
            this.rhoMax = Array.Empty<float>();
            this.tRho = Array.Empty<float>();
            this.CrownShape = new TreeCrownShape[n_sp];

            // height and volume
            this.aH = Array.Empty<float>();
            this.nHB = Array.Empty<float>();
            this.nHC = Array.Empty<float>();
            this.aV = Array.Empty<float>();
            this.nVB = Array.Empty<float>();
            this.nVH = Array.Empty<float>();
            this.nVBH = Array.Empty<float>();
            this.aK = Array.Empty<float>();
            this.nKB = Array.Empty<float>();
            this.nKH = Array.Empty<float>();
            this.nKC = Array.Empty<float>();
            this.nKrh = Array.Empty<float>();
            this.aHL = Array.Empty<float>();
            this.nHLB = Array.Empty<float>();
            this.nHLL = Array.Empty<float>();
            this.nHLC = Array.Empty<float>();
            this.nHLrh = Array.Empty<float>();

            // δ¹³C
            this.Qa = Array.Empty<float>();
            this.Qb = Array.Empty<float>();
            this.gDM_mol = Array.Empty<float>();
            this.molPAR_MJ = Array.Empty<float>();
        }

        public override void AllocateSpecies(string[] names)
        {
            base.AllocateSpecies(names);

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

            // Wood and stand properties
            this.fracBB0 = this.fracBB0.Resize(this.n_sp);
            this.fracBB1 = this.fracBB1.Resize(this.n_sp);
            this.tBB = this.tBB.Resize(this.n_sp);
            this.rhoMin = this.rhoMin.Resize(this.n_sp);
            this.rhoMax = this.rhoMax.Resize(this.n_sp);
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

        public TreeSpeciesParameters Filter(SiteTreeSpecies treeSpecies)
        {
            TreeSpeciesParameters filteredParameters = new();
            filteredParameters.AllocateSpecies(treeSpecies.Name);

            for (int destinationIndex = 0; destinationIndex < treeSpecies.n_sp; ++destinationIndex)
            {
                int sourceIndex = this.Name.FindIndex(treeSpecies.Name[destinationIndex]);
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
                filteredParameters.rhoMin[destinationIndex] = this.rhoMin[sourceIndex];
                filteredParameters.rhoMax[destinationIndex] = this.rhoMax[sourceIndex];
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
