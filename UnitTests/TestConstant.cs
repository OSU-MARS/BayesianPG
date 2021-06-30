using BayesianPG.ThreePG;

namespace BayesianPG.Test
{
    internal static class TestConstant
    {
        public static TreeSpeciesParameters TreeParameters { get; private set; }
        public static TreeSpeciesSizeDistribution TreeSizeDistributions { get; private set; }

        static TestConstant()
        {
            TestConstant.TreeParameters = new();
            TestConstant.TreeParameters.AllocateSpecies(new string[] { "Fagus sylvatica" });
            // Fagus sylvatica
            TestConstant.TreeParameters.pFS2[0] = 0.7F;
            TestConstant.TreeParameters.pFS20[0] = 0.06F;
            TestConstant.TreeParameters.aWS[0] = 0.183388481F;
            TestConstant.TreeParameters.nWS[0] = 2.3895F;
            TestConstant.TreeParameters.pRx[0] = 0.7F;
            TestConstant.TreeParameters.pRn[0] = 0.3F;
            TestConstant.TreeParameters.gammaF1[0] = 0.02F;
            TestConstant.TreeParameters.gammaF0[0] = 0.001F;
            TestConstant.TreeParameters.tgammaF[0] = 60.0F;
            TestConstant.TreeParameters.gammaR[0] = 0.015F;
            TestConstant.TreeParameters.leafgrow[0] = 5;
            TestConstant.TreeParameters.leaffall[0] = 11;
            TestConstant.TreeParameters.Tmin[0] = -5.0F;
            TestConstant.TreeParameters.Topt[0] = 20.0F;
            TestConstant.TreeParameters.Tmax[0] = 25.0F;
            TestConstant.TreeParameters.kF[0] = 1.0F;
            TestConstant.TreeParameters.SWconst0[0] = 0.7F;
            TestConstant.TreeParameters.SWpower0[0] = 9.0F;
            TestConstant.TreeParameters.fCalpha700[0] = 1.0F;
            TestConstant.TreeParameters.fCg700[0] = 1.0F;
            TestConstant.TreeParameters.m0[0] = 0.0F;
            TestConstant.TreeParameters.fN0[0] = 0.5F;
            TestConstant.TreeParameters.fNn[0] = 1.0F;
            TestConstant.TreeParameters.MaxAge[0] = 300.0F;
            TestConstant.TreeParameters.nAge[0] = 4.0F;
            TestConstant.TreeParameters.rAge[0] = 0.95F;
            TestConstant.TreeParameters.gammaN1[0] = 0.0F;
            TestConstant.TreeParameters.gammaN0[0] = 0.0F;
            TestConstant.TreeParameters.tgammaN[0] = 0.0F;
            TestConstant.TreeParameters.ngammaN[0] = 1.0F;
            TestConstant.TreeParameters.wSx1000[0] = 400.0F;
            TestConstant.TreeParameters.thinPower[0] = 1.5F;
            TestConstant.TreeParameters.mF[0] = 0.0F;
            TestConstant.TreeParameters.mR[0] = 0.2F;
            TestConstant.TreeParameters.mS[0] = 0.4F;
            TestConstant.TreeParameters.SLA0[0] = 24.71899941F;
            TestConstant.TreeParameters.SLA1[0] = 19.4020502F;
            TestConstant.TreeParameters.tSLA[0] = 35.0F;
            TestConstant.TreeParameters.k[0] = 0.417818256F;
            TestConstant.TreeParameters.fullCanAge[0] = 10.0F;
            TestConstant.TreeParameters.MaxIntcptn[0] = 0.237333333F;
            TestConstant.TreeParameters.LAImaxIntcptn[0] = 3.0F;
            TestConstant.TreeParameters.cVPD[0] = 5.0F;
            TestConstant.TreeParameters.alphaCx[0] = 0.049810073F;
            TestConstant.TreeParameters.Y[0] = 0.47F;
            TestConstant.TreeParameters.MinCond[0] = 0.0F;
            TestConstant.TreeParameters.MaxCond[0] = 0.02F;
            TestConstant.TreeParameters.LAIgcx[0] = 3.33F;
            TestConstant.TreeParameters.CoeffCond[0] = 0.057F;
            TestConstant.TreeParameters.BLcond[0] = 0.2F;
            TestConstant.TreeParameters.RGcGw[0] = 0.66F;
            TestConstant.TreeParameters.D13CTissueDif[0] = 2.0F;
            TestConstant.TreeParameters.aFracDiffu[0] = 4.4F;
            TestConstant.TreeParameters.bFracRubi[0] = 27.0F;
            TestConstant.TreeParameters.fracBB0[0] = 0.75F;
            TestConstant.TreeParameters.fracBB1[0] = 0.15F;
            TestConstant.TreeParameters.tBB[0] = 2.0F;
            TestConstant.TreeParameters.rhoMin[0] = 0.567F;
            TestConstant.TreeParameters.rhoMax[0] = 0.567F;
            TestConstant.TreeParameters.tRho[0] = 1.0F;
            TestConstant.TreeParameters.CrownShape[0] = TreeCrownShape.HalfEllipsoid;
            TestConstant.TreeParameters.aH[0] = 1.007926944F;
            TestConstant.TreeParameters.nHB[0] = 0.5375352F;
            TestConstant.TreeParameters.nHC[0] = 0.4498478F;
            TestConstant.TreeParameters.aV[0] = 0.0F;
            TestConstant.TreeParameters.nVB[0] = 0.0F;
            TestConstant.TreeParameters.nVH[0] = 0.0F;
            TestConstant.TreeParameters.nVBH[0] = 0.0F;
            TestConstant.TreeParameters.aK[0] = 0.938952425F;
            TestConstant.TreeParameters.nKB[0] = 0.5812155F;
            TestConstant.TreeParameters.nKH[0] = 0.0F;
            TestConstant.TreeParameters.nKC[0] = 0.0F;
            TestConstant.TreeParameters.nKrh[0] = 0.0F;
            TestConstant.TreeParameters.aHL[0] = 6.269003734F;
            TestConstant.TreeParameters.nHLB[0] = 0.1891636F;
            TestConstant.TreeParameters.nHLL[0] = 0.0F;
            TestConstant.TreeParameters.nHLC[0] = 0.0F;
            TestConstant.TreeParameters.nHLrh[0] = 0.6551283F;
            TestConstant.TreeParameters.Qa[0] = -90.0F;
            TestConstant.TreeParameters.Qb[0] = 0.8F;
            TestConstant.TreeParameters.gDM_mol[0] = 24.0F;
            TestConstant.TreeParameters.molPAR_MJ[0] = 2.3F;

            TestConstant.TreeSizeDistributions = new();
            TestConstant.TreeSizeDistributions.AllocateSpecies(new string[] { "Fagus sylvatica" });
            // Fagus sylvatica
            TestConstant.TreeSizeDistributions.Dscale0[0] = 0.194387984F;
            TestConstant.TreeSizeDistributions.DscaleB[0] = 1.2246192F;
            TestConstant.TreeSizeDistributions.Dscalerh[0] = 0.0F;
            TestConstant.TreeSizeDistributions.Dscalet[0] = 0.1277694F;
            TestConstant.TreeSizeDistributions.DscaleC[0] = 0.0F;
            TestConstant.TreeSizeDistributions.Dshape0[0] = 0.78810102F;
            TestConstant.TreeSizeDistributions.DshapeB[0] = 0.3161924F;
            TestConstant.TreeSizeDistributions.Dshaperh[0] = 1.6142477F;
            TestConstant.TreeSizeDistributions.Dshapet[0] = 0.0F;
            TestConstant.TreeSizeDistributions.DshapeC[0] = -0.1170241F;
            TestConstant.TreeSizeDistributions.Dlocation0[0] = 1.636091883F;
            TestConstant.TreeSizeDistributions.DlocationB[0] = 0.5046618F;
            TestConstant.TreeSizeDistributions.Dlocationrh[0] = -0.6263416F;
            TestConstant.TreeSizeDistributions.Dlocationt[0] = 0.0F;
            TestConstant.TreeSizeDistributions.DlocationC[0] = 0.0457864F;
            TestConstant.TreeSizeDistributions.wsscale0[0] = 0.022094402F;
            TestConstant.TreeSizeDistributions.wsscaleB[0] = 2.604811F;
            TestConstant.TreeSizeDistributions.wsscalerh[0] = -1.814874F;
            TestConstant.TreeSizeDistributions.wsscalet[0] = 0.320978F;
            TestConstant.TreeSizeDistributions.wsscaleC[0] = 0.089823F;
            TestConstant.TreeSizeDistributions.wsshape0[0] = 0.645865695F;
            TestConstant.TreeSizeDistributions.wsshapeB[0] = 0.2687072F;
            TestConstant.TreeSizeDistributions.wsshaperh[0] = 1.7403653F;
            TestConstant.TreeSizeDistributions.wsshapet[0] = 0.0F;
            TestConstant.TreeSizeDistributions.wsshapeC[0] = -0.1429322F;
            TestConstant.TreeSizeDistributions.wslocation0[0] = 0.142726795F;
            TestConstant.TreeSizeDistributions.wslocationB[0] = 1.028025F;
            TestConstant.TreeSizeDistributions.wslocationrh[0] = -3.499751F;
            TestConstant.TreeSizeDistributions.wslocationt[0] = 0.453181F;
            TestConstant.TreeSizeDistributions.wslocationC[0] = 0.207966F;
        }
    }
}
