using BayesianPG.Test.Xlsx;
using BayesianPG.ThreePG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BayesianPG.Test
{
    [TestClass]
    [DeploymentItem("r3PG.xlsx")]
    public class Test3PG
    {
        /// <summary>
        /// Verifies C# implementations of 3-PGpjs 2.7 and 3-PGmix against r3PG Fortran implementations.
        /// </summary>
        /// <remarks>
        /// Ported from https://github.com/trotsiuk/r3PG/blob/master/pkg/tests/r_vba_compare/r3PG_VBA_compare.R.
        /// </remarks>
        [TestMethod]
        public void CompareWithR3PG()
        {
            using TestThreePGReader reader = new("r3PG.xlsx");
            SortedList<string, ThreePGStandTrajectory> expectedTrajectoriesBySiteName = reader.ReadExpectations();
            SortedList<string, ThreePGpjsMix> sitesByName = reader.ReadSites();
            SortedList<string, StandTrajectoryTolerance> tolerancesBySiteName = new()
            {
                // see Test3PG.R for analysis
                { "broadleaf_mix", new() },
                { "broadleaf_pjs", new() },
                { "evergreen_mix", new() },
                { "evergreen_pjs", new() },
                { "mixtures_eu", new()
                                 {
                                     volume_cum = 0.015F // differs because r3PG.xlsx is based on r3PG 0.1.3, which does not have the fix for https://github.com/trotsiuk/r3PG/issues/63
                                 }
                },
                { "mixtures_other", new()
                                    {        
                                        MaxTimestep = 10 * 12 // verify against only first 10 years due to https://github.com/trotsiuk/r3PG/issues/70
                                    }
                },
            };

            for (int index = 0; index < sitesByName.Count; ++index)
            {
                // filter to specific site for debugging
                //if (sitesByName.Keys[index] != "mixtures_other")
                //{
                //    continue;
                //}
                ThreePGpjsMix threePG = sitesByName.Values[index];
                threePG.PredictStandTrajectory();

                Test3PG.VerifySpeciesParameters(threePG);
                Test3PG.VerifySizeDistribution(threePG);
                Test3PG.VerifyStandTrajectory(threePG, expectedTrajectoriesBySiteName.Values[index], tolerancesBySiteName.Values[index]);
            }

            for (int index = 0; index < expectedTrajectoriesBySiteName.Count; ++index)
            {
                string siteName = sitesByName.Keys[index];
                string expectedSiteName = expectedTrajectoriesBySiteName.Keys[index];

                Assert.IsTrue(String.Equals(siteName, expectedSiteName));
            }
        }

        private static void VerifyArray(string name, float[,] actual, int expectedTimesteps, float[,] expectedValues, float tolerance, int maxTimestep)
        {
            Debug.Assert((tolerance > 0.0F) && (tolerance <= 1.0F));
            int actualMonths = actual.GetLength(0);
            Assert.IsTrue(actualMonths == expectedTimesteps);

            // for now, skip first month as not all values are computed
            float differenceThreshold = 0.1F * tolerance;
            float maxRatio = 1.0F + tolerance;
            float minRatio = 1.0F - tolerance;
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
            {
                int expectedSpecies = expectedValues.GetLength(1);
                Assert.IsTrue(actual.GetLength(1) == expectedSpecies);
                for (int speciesIndex = 0; speciesIndex < expectedSpecies; ++speciesIndex)
                {
                    float actualValue = actual[timestepIndex, speciesIndex];
                    float expectedValue = expectedValues[timestepIndex, speciesIndex];
                    if (MathF.Abs(expectedValue) < differenceThreshold)
                    {
                        float difference = actualValue - expectedValue;
                        Assert.IsTrue(MathF.Abs(difference) <= differenceThreshold, name + "[" + timestepIndex + ", " + speciesIndex + "]: difference = " + difference + ".");
                    }
                    else
                    {
                        float ratio = actualValue / expectedValue;
                        Assert.IsTrue((ratio >= minRatio) && (ratio <= maxRatio), name + "[" + timestepIndex + ", " + speciesIndex + "]: ratio = " + ratio + ".");
                    }
                }
            }
        }

        private static void VerifyArray(string name, float[] actual, int expectedTimesteps, float[] expectedValues, float tolerance, int maxTimestep)
        {
            if (Single.IsNaN(tolerance))
            {
                // checking is disabled
                return;
            }
            Debug.Assert((tolerance > 0.0F) && (tolerance <= 1.0F));
            Assert.IsTrue(actual.Length == expectedTimesteps);

            // for now, skip first month as not all values are computed
            float differenceThreshold = 0.1F * tolerance;
            float maxRatio = 1.0F + tolerance;
            float minRatio = 1.0F - tolerance;
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
            {
                float actualValue = actual[timestepIndex];
                float expectedValue = expectedValues[timestepIndex];
                if (MathF.Abs(expectedValue) < differenceThreshold)
                {
                    float difference = actualValue - expectedValue;
                    Assert.IsTrue(MathF.Abs(difference) <= differenceThreshold, name + "[" + timestepIndex + "]: difference = " + difference + ".");
                }
                else
                {
                    float ratio = actualValue / expectedValue;
                    Assert.IsTrue((ratio >= minRatio) && (ratio <= maxRatio), name + "[" + timestepIndex + "]: ratio = " + ratio + ".");
                }
            }
        }

        private static void VerifyArray(string name, float[][] actual, int expectedTimesteps, float[][] expectedValues, float tolerance, int maxTimestep)
        {
            Debug.Assert((tolerance > 0.0F) && (tolerance <= 1.0F));
            int expectedSpecies = expectedValues.Length;
            Assert.IsTrue(actual.Length == expectedSpecies);

            float differenceThreshold = 0.1F * tolerance;
            float maxRatio = 1.0F + tolerance;
            float minRatio = 1.0F - tolerance;
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int speciesIndex = 0; speciesIndex < expectedSpecies; ++speciesIndex)
            {
                float[] actualValuesForSpecies = actual[speciesIndex];
                int actualMonths = actualValuesForSpecies.Length;
                Assert.IsTrue(actualMonths == expectedTimesteps);

                float[] expectedValuesForSpecies = expectedValues[speciesIndex];
                // for now, skip first month as not all values are computed
                for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
                {
                    float actualValue = actualValuesForSpecies[timestepIndex];
                    float expectedValue = expectedValuesForSpecies[timestepIndex];
                    if (MathF.Abs(expectedValue) < differenceThreshold)
                    {
                        float difference = actualValue - expectedValue;
                        Assert.IsTrue(MathF.Abs(difference) <= differenceThreshold, name + "[" + speciesIndex + "][" + timestepIndex + "]");
                    }
                    else
                    {
                        float ratio = actualValue / expectedValue;
                        Assert.IsTrue((ratio >= minRatio) && (ratio <= maxRatio), name + "[" + speciesIndex + "][" + timestepIndex + "]");
                    }
                }
            }
        }

        private static void VerifyArray(string name, string[] actual, string[] expectedValues)
        {
            Assert.IsTrue(actual.Length == expectedValues.Length);

            // for now, skip first month as not all values are computed
            // also skip last expected month as it's duplicate or incomplete
            int maxTimestep = expectedValues.Length - 1;
            for (int timestepIndex = 1; timestepIndex < maxTimestep; ++timestepIndex)
            {
                string actualValue = actual[timestepIndex];
                string expectedValue = expectedValues[timestepIndex];
                Assert.IsTrue(String.Equals(actualValue, expectedValue, StringComparison.Ordinal), name + "[" + timestepIndex + "]: '" + actualValue + "' instead of '" + expectedValue + "'.");
            }
        }

        private static void VerifySpeciesParameters(ThreePGpjsMix threePG)
        {
            TreeSpeciesParameters expectedParameters = TestConstant.TreeParameters; // shorthand for readability
            for (int speciesIndex = 0; speciesIndex < threePG.Parameters.n_sp; ++speciesIndex)
            {
                string speciesName = threePG.Parameters.Name[speciesIndex];
                int verificationIndex = expectedParameters.Name.FindIndex(speciesName);
                if (verificationIndex != -1)
                {
                    Assert.IsTrue(threePG.Parameters.pFS2[speciesIndex] == expectedParameters.pFS2[verificationIndex], nameof(expectedParameters.pFS2));
                    Assert.IsTrue(threePG.Parameters.pFS20[speciesIndex] == expectedParameters.pFS20[verificationIndex], nameof(expectedParameters.pFS20));
                    Assert.IsTrue(threePG.Parameters.aWS[speciesIndex] == expectedParameters.aWS[verificationIndex], nameof(expectedParameters.aWS));
                    Assert.IsTrue(threePG.Parameters.nWS[speciesIndex] == expectedParameters.nWS[verificationIndex], nameof(expectedParameters.nWS));
                    Assert.IsTrue(threePG.Parameters.pRx[speciesIndex] == expectedParameters.pRx[verificationIndex], nameof(expectedParameters.pRx));
                    Assert.IsTrue(threePG.Parameters.pRn[speciesIndex] == expectedParameters.pRn[verificationIndex], nameof(expectedParameters.pRn));
                    Assert.IsTrue(threePG.Parameters.gammaF1[speciesIndex] == expectedParameters.gammaF1[verificationIndex], nameof(expectedParameters.gammaF1));
                    Assert.IsTrue(threePG.Parameters.gammaN0[speciesIndex] == expectedParameters.gammaN0[verificationIndex], nameof(expectedParameters.gammaN0));
                    Assert.IsTrue(threePG.Parameters.tgammaF[speciesIndex] == expectedParameters.tgammaF[verificationIndex], nameof(expectedParameters.tgammaF));
                    Assert.IsTrue(threePG.Parameters.gammaR[speciesIndex] == expectedParameters.gammaR[verificationIndex], nameof(expectedParameters.gammaR));
                    Assert.IsTrue(threePG.Parameters.leafgrow[speciesIndex] == expectedParameters.leafgrow[verificationIndex], nameof(expectedParameters.leafgrow));
                    Assert.IsTrue(threePG.Parameters.leaffall[speciesIndex] == expectedParameters.leaffall[verificationIndex], nameof(expectedParameters.leaffall));
                    Assert.IsTrue(threePG.Parameters.Tmin[speciesIndex] == expectedParameters.Tmin[verificationIndex], nameof(expectedParameters.Tmin));
                    Assert.IsTrue(threePG.Parameters.Topt[speciesIndex] == expectedParameters.Topt[verificationIndex], nameof(expectedParameters.Topt));
                    Assert.IsTrue(threePG.Parameters.Tmax[speciesIndex] == expectedParameters.Tmax[verificationIndex], nameof(expectedParameters.Tmax));
                    Assert.IsTrue(threePG.Parameters.kF[speciesIndex] == expectedParameters.kF[verificationIndex], nameof(expectedParameters.kF));
                    Assert.IsTrue(threePG.Parameters.SWconst0[speciesIndex] == expectedParameters.SWconst0[verificationIndex], nameof(expectedParameters.SWconst0));
                    Assert.IsTrue(threePG.Parameters.SWpower0[speciesIndex] == expectedParameters.SWpower0[verificationIndex], nameof(expectedParameters.SWpower0));
                    Assert.IsTrue(threePG.Parameters.fCalpha700[speciesIndex] == expectedParameters.fCalpha700[verificationIndex], nameof(expectedParameters.fCalpha700));
                    Assert.IsTrue(threePG.Parameters.fCg700[speciesIndex] == expectedParameters.fCg700[verificationIndex], nameof(expectedParameters.fCg700));
                    Assert.IsTrue(threePG.Parameters.m0[speciesIndex] == expectedParameters.m0[verificationIndex], nameof(expectedParameters.m0));
                    Assert.IsTrue(threePG.Parameters.fN0[speciesIndex] == expectedParameters.fN0[verificationIndex], nameof(expectedParameters.fN0));
                    Assert.IsTrue(threePG.Parameters.MaxAge[speciesIndex] == expectedParameters.MaxAge[verificationIndex], nameof(expectedParameters.MaxAge));
                    Assert.IsTrue(threePG.Parameters.nAge[speciesIndex] == expectedParameters.nAge[verificationIndex], nameof(expectedParameters.nAge));
                    Assert.IsTrue(threePG.Parameters.rAge[speciesIndex] == expectedParameters.rAge[verificationIndex], nameof(expectedParameters.rAge));
                    Assert.IsTrue(threePG.Parameters.gammaN1[speciesIndex] == expectedParameters.gammaN1[verificationIndex], nameof(expectedParameters.gammaN1));
                    Assert.IsTrue(threePG.Parameters.gammaN0[speciesIndex] == expectedParameters.gammaN0[verificationIndex], nameof(expectedParameters.gammaN0));
                    Assert.IsTrue(threePG.Parameters.tgammaN[speciesIndex] == expectedParameters.tgammaN[verificationIndex], nameof(expectedParameters.tgammaN));
                    Assert.IsTrue(threePG.Parameters.ngammaN[speciesIndex] == expectedParameters.ngammaN[verificationIndex], nameof(expectedParameters.ngammaN));
                    Assert.IsTrue(threePG.Parameters.thinPower[speciesIndex] == expectedParameters.thinPower[verificationIndex], nameof(expectedParameters.thinPower));
                    Assert.IsTrue(threePG.Parameters.mF[speciesIndex] == expectedParameters.mF[verificationIndex], nameof(expectedParameters.mF));
                    Assert.IsTrue(threePG.Parameters.mR[speciesIndex] == expectedParameters.mR[verificationIndex], nameof(expectedParameters.mR));
                    Assert.IsTrue(threePG.Parameters.mS[speciesIndex] == expectedParameters.mS[verificationIndex], nameof(expectedParameters.mS));
                    Assert.IsTrue(threePG.Parameters.SLA0[speciesIndex] == expectedParameters.SLA0[verificationIndex], nameof(expectedParameters.SLA0));
                    Assert.IsTrue(threePG.Parameters.SLA1[speciesIndex] == expectedParameters.SLA1[verificationIndex], nameof(expectedParameters.SLA1));
                    Assert.IsTrue(threePG.Parameters.tSLA[speciesIndex] == expectedParameters.tSLA[verificationIndex], nameof(expectedParameters.tSLA));
                    Assert.IsTrue(threePG.Parameters.k[speciesIndex] == expectedParameters.k[verificationIndex], nameof(expectedParameters.k));
                    Assert.IsTrue(threePG.Parameters.fullCanAge[speciesIndex] == expectedParameters.fullCanAge[verificationIndex], nameof(expectedParameters.fullCanAge));
                    Assert.IsTrue(threePG.Parameters.MaxIntcptn[speciesIndex] == expectedParameters.MaxIntcptn[verificationIndex], nameof(expectedParameters.MaxIntcptn));
                    Assert.IsTrue(threePG.Parameters.LAImaxIntcptn[speciesIndex] == expectedParameters.LAImaxIntcptn[verificationIndex], nameof(expectedParameters.LAImaxIntcptn));
                    Assert.IsTrue(threePG.Parameters.cVPD[speciesIndex] == expectedParameters.cVPD[verificationIndex], nameof(expectedParameters.cVPD));
                    Assert.IsTrue(threePG.Parameters.alphaCx[speciesIndex] == expectedParameters.alphaCx[verificationIndex], nameof(expectedParameters.alphaCx));
                    Assert.IsTrue(threePG.Parameters.Y[speciesIndex] == expectedParameters.Y[verificationIndex], nameof(expectedParameters.Y));
                    Assert.IsTrue(threePG.Parameters.MinCond[speciesIndex] == expectedParameters.MinCond[verificationIndex], nameof(expectedParameters.MinCond));
                    Assert.IsTrue(threePG.Parameters.MaxCond[speciesIndex] == expectedParameters.MaxCond[verificationIndex], nameof(expectedParameters.MaxCond));
                    Assert.IsTrue(threePG.Parameters.LAIgcx[speciesIndex] == expectedParameters.LAIgcx[verificationIndex], nameof(expectedParameters.LAIgcx));
                    Assert.IsTrue(threePG.Parameters.CoeffCond[speciesIndex] == expectedParameters.CoeffCond[verificationIndex], nameof(expectedParameters.CoeffCond));
                    Assert.IsTrue(threePG.Parameters.BLcond[speciesIndex] == expectedParameters.BLcond[verificationIndex], nameof(expectedParameters.BLcond));
                    Assert.IsTrue(threePG.Parameters.RGcGw[speciesIndex] == expectedParameters.RGcGw[verificationIndex], nameof(expectedParameters.RGcGw));
                    Assert.IsTrue(threePG.Parameters.D13CTissueDif[speciesIndex] == expectedParameters.D13CTissueDif[verificationIndex], nameof(expectedParameters.D13CTissueDif));
                    Assert.IsTrue(threePG.Parameters.aFracDiffu[speciesIndex] == expectedParameters.aFracDiffu[verificationIndex], nameof(expectedParameters.aFracDiffu));
                    Assert.IsTrue(threePG.Parameters.bFracRubi[speciesIndex] == expectedParameters.bFracRubi[verificationIndex], nameof(expectedParameters.bFracRubi));
                    Assert.IsTrue(threePG.Parameters.fracBB0[speciesIndex] == expectedParameters.fracBB0[verificationIndex], nameof(expectedParameters.fracBB0));
                    Assert.IsTrue(threePG.Parameters.fracBB1[speciesIndex] == expectedParameters.fracBB1[verificationIndex], nameof(expectedParameters.fracBB1));
                    Assert.IsTrue(threePG.Parameters.tBB[speciesIndex] == expectedParameters.tBB[verificationIndex], nameof(expectedParameters.tBB));
                    Assert.IsTrue(threePG.Parameters.rhoMin[speciesIndex] == expectedParameters.rhoMin[verificationIndex], nameof(expectedParameters.rhoMin));
                    Assert.IsTrue(threePG.Parameters.rhoMax[speciesIndex] == expectedParameters.rhoMax[verificationIndex], nameof(expectedParameters.rhoMax));
                    Assert.IsTrue(threePG.Parameters.tRho[speciesIndex] == expectedParameters.tRho[verificationIndex], nameof(expectedParameters.tRho));
                    Assert.IsTrue(threePG.Parameters.CrownShape[speciesIndex] == expectedParameters.CrownShape[verificationIndex], nameof(expectedParameters.CrownShape));
                    Assert.IsTrue(threePG.Parameters.aH[speciesIndex] == expectedParameters.aH[verificationIndex], nameof(expectedParameters.aH));
                    Assert.IsTrue(threePG.Parameters.nHB[speciesIndex] == expectedParameters.nHB[verificationIndex], nameof(expectedParameters.nHB));
                    Assert.IsTrue(threePG.Parameters.nHC[speciesIndex] == expectedParameters.nHC[verificationIndex], nameof(expectedParameters.nHC));
                    Assert.IsTrue(threePG.Parameters.aV[speciesIndex] == expectedParameters.aV[verificationIndex], nameof(expectedParameters.aV));
                    Assert.IsTrue(threePG.Parameters.nVB[speciesIndex] == expectedParameters.nVB[verificationIndex], nameof(expectedParameters.nVB));
                    Assert.IsTrue(threePG.Parameters.nVH[speciesIndex] == expectedParameters.nVH[verificationIndex], nameof(expectedParameters.nVH));
                    Assert.IsTrue(threePG.Parameters.nVBH[speciesIndex] == expectedParameters.nVBH[verificationIndex], nameof(expectedParameters.nVBH));
                    Assert.IsTrue(threePG.Parameters.aK[speciesIndex] == expectedParameters.aK[verificationIndex], nameof(expectedParameters.aK));
                    Assert.IsTrue(threePG.Parameters.nKB[speciesIndex] == expectedParameters.nKB[verificationIndex], nameof(expectedParameters.nKB));
                    Assert.IsTrue(threePG.Parameters.nKH[speciesIndex] == expectedParameters.nKH[verificationIndex], nameof(expectedParameters.nKH));
                    Assert.IsTrue(threePG.Parameters.nKC[speciesIndex] == expectedParameters.nKC[verificationIndex], nameof(expectedParameters.nKC));
                    Assert.IsTrue(threePG.Parameters.nKrh[speciesIndex] == expectedParameters.nKrh[verificationIndex], nameof(expectedParameters.nKrh));
                    Assert.IsTrue(threePG.Parameters.aHL[speciesIndex] == expectedParameters.aHL[verificationIndex], nameof(expectedParameters.aHL));
                    Assert.IsTrue(threePG.Parameters.nHLB[speciesIndex] == expectedParameters.nHLB[verificationIndex], nameof(expectedParameters.nHLB));
                    Assert.IsTrue(threePG.Parameters.nHLL[speciesIndex] == expectedParameters.nHLL[verificationIndex], nameof(expectedParameters.nHLL));
                    Assert.IsTrue(threePG.Parameters.nHLC[speciesIndex] == expectedParameters.nHLC[verificationIndex], nameof(expectedParameters.nHLC));
                    Assert.IsTrue(threePG.Parameters.nHLrh[speciesIndex] == expectedParameters.nHLrh[verificationIndex], nameof(expectedParameters.nHLrh));
                    Assert.IsTrue(threePG.Parameters.Qa[speciesIndex] == expectedParameters.Qa[verificationIndex], nameof(expectedParameters.Qa));
                    Assert.IsTrue(threePG.Parameters.Qb[speciesIndex] == expectedParameters.Qb[verificationIndex], nameof(expectedParameters.Qb));
                    Assert.IsTrue(threePG.Parameters.gDM_mol[speciesIndex] == expectedParameters.gDM_mol[verificationIndex], nameof(expectedParameters.gDM_mol));
                    Assert.IsTrue(threePG.Parameters.molPAR_MJ[speciesIndex] == expectedParameters.molPAR_MJ[verificationIndex], nameof(expectedParameters.molPAR_MJ));
                }
            }
        }

        private static void VerifySizeDistribution(ThreePGpjsMix threePG)
        {
            if (threePG.Settings.correct_bias)
            {
                Assert.IsTrue(threePG.Bias != null);

                TreeSpeciesSizeDistribution expectedBias = TestConstant.TreeSizeDistributions; // shorthand for readability
                for (int speciesIndex = 0; speciesIndex < threePG.Bias.n_sp; ++speciesIndex)
                {
                    string speciesName = threePG.Parameters.Name[speciesIndex];
                    int verificationIndex = expectedBias.Name.FindIndex(speciesName);
                    if (verificationIndex != -1)
                    {
                        Assert.IsTrue(threePG.Bias.Dscale0[speciesIndex] == expectedBias.Dscale0[verificationIndex], nameof(expectedBias.Dscale0));
                        Assert.IsTrue(threePG.Bias.DscaleB[speciesIndex] == expectedBias.DscaleB[verificationIndex], nameof(expectedBias.DscaleB));
                        Assert.IsTrue(threePG.Bias.Dscalerh[speciesIndex] == expectedBias.Dscalerh[verificationIndex], nameof(expectedBias.Dscalerh));
                        Assert.IsTrue(threePG.Bias.Dscalet[speciesIndex] == expectedBias.Dscalet[verificationIndex], nameof(expectedBias.Dscalet));
                        Assert.IsTrue(threePG.Bias.DscaleC[speciesIndex] == expectedBias.DscaleC[verificationIndex], nameof(expectedBias.DscaleC));
                        Assert.IsTrue(threePG.Bias.Dshape0[speciesIndex] == expectedBias.Dshape0[verificationIndex], nameof(expectedBias.Dshape0));
                        Assert.IsTrue(threePG.Bias.DshapeB[speciesIndex] == expectedBias.DshapeB[verificationIndex], nameof(expectedBias.DshapeB));
                        Assert.IsTrue(threePG.Bias.Dshaperh[speciesIndex] == expectedBias.Dshaperh[verificationIndex], nameof(expectedBias.Dshaperh));
                        Assert.IsTrue(threePG.Bias.Dshapet[speciesIndex] == expectedBias.Dshapet[verificationIndex], nameof(expectedBias.Dshapet));
                        Assert.IsTrue(threePG.Bias.DshapeC[speciesIndex] == expectedBias.DshapeC[verificationIndex], nameof(expectedBias.DshapeC));
                        Assert.IsTrue(threePG.Bias.Dlocation0[speciesIndex] == expectedBias.Dlocation0[verificationIndex], nameof(expectedBias.Dlocation0));
                        Assert.IsTrue(threePG.Bias.DlocationB[speciesIndex] == expectedBias.DlocationB[verificationIndex], nameof(expectedBias.DlocationB));
                        Assert.IsTrue(threePG.Bias.Dlocationrh[speciesIndex] == expectedBias.Dlocationrh[verificationIndex], nameof(expectedBias.Dlocationrh));
                        Assert.IsTrue(threePG.Bias.Dlocationt[speciesIndex] == expectedBias.Dlocationt[verificationIndex], nameof(expectedBias.Dlocationt));
                        Assert.IsTrue(threePG.Bias.DlocationC[speciesIndex] == expectedBias.DlocationC[verificationIndex], nameof(expectedBias.DlocationC));
                        Assert.IsTrue(threePG.Bias.wsscale0[speciesIndex] == expectedBias.wsscale0[verificationIndex], nameof(expectedBias.wsscale0));
                        Assert.IsTrue(threePG.Bias.wsscaleB[speciesIndex] == expectedBias.wsscaleB[verificationIndex], nameof(expectedBias.wsscaleB));
                        Assert.IsTrue(threePG.Bias.wsscalerh[speciesIndex] == expectedBias.wsscalerh[verificationIndex], nameof(expectedBias.wsscalerh));
                        Assert.IsTrue(threePG.Bias.wsscalet[speciesIndex] == expectedBias.wsscalet[verificationIndex], nameof(expectedBias.wsscalet));
                        Assert.IsTrue(threePG.Bias.wsscaleC[speciesIndex] == expectedBias.wsscaleC[verificationIndex], nameof(expectedBias.wsscaleC));
                        Assert.IsTrue(threePG.Bias.wsshape0[speciesIndex] == expectedBias.wsshape0[verificationIndex], nameof(expectedBias.wsshape0));
                        Assert.IsTrue(threePG.Bias.wsshapeB[speciesIndex] == expectedBias.wsshapeB[verificationIndex], nameof(expectedBias.wsshapeB));
                        Assert.IsTrue(threePG.Bias.wsshaperh[speciesIndex] == expectedBias.wsshaperh[verificationIndex], nameof(expectedBias.wsshaperh));
                        Assert.IsTrue(threePG.Bias.wsshapet[speciesIndex] == expectedBias.wsshapet[verificationIndex], nameof(expectedBias.wsshapet));
                        Assert.IsTrue(threePG.Bias.wsshapeC[speciesIndex] == expectedBias.wsshapeC[verificationIndex], nameof(expectedBias.wsshapeC));
                        Assert.IsTrue(threePG.Bias.wslocation0[speciesIndex] == expectedBias.wslocation0[verificationIndex], nameof(expectedBias.wslocation0));
                        Assert.IsTrue(threePG.Bias.wslocationB[speciesIndex] == expectedBias.wslocationB[verificationIndex], nameof(expectedBias.wslocationB));
                        Assert.IsTrue(threePG.Bias.wslocationrh[speciesIndex] == expectedBias.wslocationrh[verificationIndex], nameof(expectedBias.wslocationrh));
                        Assert.IsTrue(threePG.Bias.wslocationt[speciesIndex] == expectedBias.wslocationt[verificationIndex], nameof(expectedBias.wslocationt));
                        Assert.IsTrue(threePG.Bias.wslocationC[speciesIndex] == expectedBias.wslocationC[verificationIndex], nameof(expectedBias.wslocationC));
                    }
                }
            }
            // bias correction is disabled so, for now, ignore size distribution
        }

        private static void VerifyStandTrajectory(ThreePGpjsMix threePG, ThreePGStandTrajectory expectedTrajectory, StandTrajectoryTolerance tolerances)
        {
            ThreePGStandTrajectory actualTrajectory = threePG.Trajectory;

            // verify array sizes and scalar properties
            // Reference trajectories in spreadsheet stop in January of end year rather than extend to February.
            // Also, January predictions are duplicated into February.
            Assert.IsTrue(actualTrajectory.n_m >= expectedTrajectory.n_m);
            Assert.IsTrue(actualTrajectory.Species.n_sp == expectedTrajectory.Species.n_sp);

            // verify species order matches
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.Name), actualTrajectory.Species.Name, expectedTrajectory.Species.Name);

            // verify stand trajectory
            Test3PG.VerifyArray(nameof(actualTrajectory.AvailableSoilWater), actualTrajectory.AvailableSoilWater, actualTrajectory.n_m, expectedTrajectory.AvailableSoilWater, tolerances.AvailableSoilWater, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.DayLength), actualTrajectory.DayLength, actualTrajectory.n_m, expectedTrajectory.DayLength, tolerances.DayLength, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Evapotranspiration), actualTrajectory.Evapotranspiration, actualTrajectory.n_m, expectedTrajectory.Evapotranspiration, tolerances.Evapotranspiration, tolerances.MaxTimestep);
            Assert.IsTrue((actualTrajectory.From.Year == expectedTrajectory.From.Year) && (actualTrajectory.From.Month == expectedTrajectory.From.Month), nameof(actualTrajectory.From));
            Test3PG.VerifyArray(nameof(actualTrajectory.irrig_supl), actualTrajectory.irrig_supl, actualTrajectory.n_m, expectedTrajectory.irrig_supl, tolerances.Irrigation, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.prcp_runoff), actualTrajectory.prcp_runoff, actualTrajectory.n_m, expectedTrajectory.prcp_runoff, tolerances.Runoff, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.conduct_soil), actualTrajectory.conduct_soil, actualTrajectory.n_m, expectedTrajectory.conduct_soil, tolerances.SoilConductance, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.evapotra_soil), actualTrajectory.evapotra_soil, actualTrajectory.n_m, expectedTrajectory.evapotra_soil, tolerances.SoilEvaporation, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.age), actualTrajectory.Species.age, actualTrajectory.n_m, expectedTrajectory.Species.age, tolerances.age, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.age_m), actualTrajectory.Species.age_m, actualTrajectory.n_m, expectedTrajectory.Species.age_m, tolerances.age_m, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.alpha_c), actualTrajectory.Species.alpha_c, actualTrajectory.n_m, expectedTrajectory.Species.alpha_c, tolerances.alpha_c, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.basal_area), actualTrajectory.Species.basal_area, actualTrajectory.n_m, expectedTrajectory.Species.basal_area, tolerances.basal_area, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_foliage), actualTrajectory.Species.biom_foliage, actualTrajectory.n_m, expectedTrajectory.Species.biom_foliage, tolerances.biom_foliage, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_root), actualTrajectory.Species.biom_root, actualTrajectory.n_m, expectedTrajectory.Species.biom_root, tolerances.biom_root, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_stem), actualTrajectory.Species.biom_stem, actualTrajectory.n_m, expectedTrajectory.Species.biom_stem, tolerances.biom_stem, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.CanopyCover), actualTrajectory.Species.CanopyCover, actualTrajectory.n_m, expectedTrajectory.Species.CanopyCover, tolerances.CanopyCover, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.CanopyVolumeFraction), actualTrajectory.Species.CanopyVolumeFraction, actualTrajectory.n_m, expectedTrajectory.Species.CanopyVolumeFraction, tolerances.CanopyVolumeFraction, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.conduct_canopy), actualTrajectory.Species.conduct_canopy, actualTrajectory.n_m, expectedTrajectory.Species.conduct_canopy, tolerances.conduct_canopy, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.CrownDiameter), actualTrajectory.Species.CrownDiameter, actualTrajectory.n_m, expectedTrajectory.Species.CrownDiameter, tolerances.CrownDiameter, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.dbh), actualTrajectory.Species.dbh, actualTrajectory.n_m, expectedTrajectory.Species.dbh, tolerances.dbh, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.epsilon_gpp), actualTrajectory.Species.epsilon_gpp, actualTrajectory.n_m, expectedTrajectory.Species.epsilon_gpp, tolerances.epsilon_gpp, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.epsilon_npp), actualTrajectory.Species.epsilon_npp, actualTrajectory.n_m, expectedTrajectory.Species.epsilon_npp, tolerances.epsilon_npp, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.ExtractedVolume), actualTrajectory.Species.ExtractedVolume, actualTrajectory.n_m, expectedTrajectory.Species.ExtractedVolume, tolerances.ExtractedVolume, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.fi), actualTrajectory.Species.fi, actualTrajectory.n_m, expectedTrajectory.Species.fi, tolerances.fi, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.FoliageLitterFallRateOfPeriod), actualTrajectory.Species.FoliageLitterFallRateOfPeriod, actualTrajectory.n_m, expectedTrajectory.Species.FoliageLitterFallRateOfPeriod, tolerances.FoliageLitterFallRateOfPeriod, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.FR), actualTrajectory.Species.FR, actualTrajectory.n_m, expectedTrajectory.Species.FR, tolerances.FR, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.fracBB), actualTrajectory.Species.fracBB, actualTrajectory.n_m, expectedTrajectory.Species.fracBB, tolerances.fracBB, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_age), actualTrajectory.Species.f_age, actualTrajectory.n_m, expectedTrajectory.Species.f_age, tolerances.f_age, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_calpha), actualTrajectory.Species.f_calpha, actualTrajectory.n_m, expectedTrajectory.Species.f_calpha, tolerances.f_calpha, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_cg), actualTrajectory.Species.f_cg, actualTrajectory.n_m, expectedTrajectory.Species.f_cg, tolerances.f_cg, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_frost), actualTrajectory.Species.f_frost, actualTrajectory.n_m, expectedTrajectory.Species.f_frost, tolerances.f_frost, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_nutr), actualTrajectory.Species.f_nutr, actualTrajectory.n_m, expectedTrajectory.Species.f_nutr, tolerances.f_nutr, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_phys), actualTrajectory.Species.f_phys, actualTrajectory.n_m, expectedTrajectory.Species.f_phys, tolerances.f_phys, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_sw), actualTrajectory.Species.f_sw, actualTrajectory.n_m, expectedTrajectory.Species.f_sw, tolerances.f_sw, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_tmp), actualTrajectory.Species.f_tmp, actualTrajectory.n_m, expectedTrajectory.Species.f_tmp, tolerances.f_tmp, tolerances.MaxTimestep);
            if (threePG.Settings.phys_model == ThreePGModel.Mix)
            {
                // f_tmp_gc is hard coded to 1 in 3-PGpjs but the reference worksheets contain the values calculated prior to pjs forcing it to 1
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_tmp_gc), actualTrajectory.Species.f_tmp_gc, actualTrajectory.n_m, expectedTrajectory.Species.f_tmp_gc, tolerances.f_tmp_gc, tolerances.MaxTimestep);
            }
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_vpd), actualTrajectory.Species.f_vpd, actualTrajectory.n_m, expectedTrajectory.Species.f_vpd, tolerances.f_vpd, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.gammaF), actualTrajectory.Species.gammaF, actualTrajectory.n_m, expectedTrajectory.Species.gammaF, tolerances.gammaF, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.gammaN), actualTrajectory.Species.gammaN, actualTrajectory.n_m, expectedTrajectory.Species.gammaN, tolerances.gammaN, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.GPP), actualTrajectory.Species.GPP, actualTrajectory.n_m, expectedTrajectory.Species.GPP, tolerances.GPP, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.height), actualTrajectory.Species.height, actualTrajectory.n_m, expectedTrajectory.Species.height, tolerances.height, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.lai), actualTrajectory.Species.lai, actualTrajectory.n_m, expectedTrajectory.Species.lai, tolerances.lai, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.LAIabove), actualTrajectory.Species.LAIabove, actualTrajectory.n_m, expectedTrajectory.Species.LAIabove, tolerances.LAIabove, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.LambdaH1), actualTrajectory.Species.LambdaH1, actualTrajectory.n_m, expectedTrajectory.Species.LambdaH1, tolerances.LambdaH1, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.LambdaV1), actualTrajectory.Species.LambdaV1, actualTrajectory.n_m, expectedTrajectory.Species.LambdaV1, tolerances.LambdaV1, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.Layer), actualTrajectory.Species.Layer, actualTrajectory.n_m, expectedTrajectory.Species.Layer, tolerances.Layer, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.LCL), actualTrajectory.Species.LCL, actualTrajectory.n_m, expectedTrajectory.Species.LCL, tolerances.LCL, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.m1), actualTrajectory.Species.m1, actualTrajectory.n_m, expectedTrajectory.Species.m1, tolerances.m1, tolerances.MaxTimestep);
            // TODO: investigation of biomass foliage debt, NPP, and NPPdebt in deciduous cases
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.NPP), actualTrajectory.Species.NPP, actualTrajectory.n_m, expectedTrajectory.Species.NPP, tolerances.NPP, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.NPPdebt), actualTrajectory.Species.NPPdebt, actualTrajectory.n_m, expectedTrajectory.Species.NPPdebt, tolerances.NPPdebt, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.PenmanTranspiration), actualTrajectory.Species.PenmanTranspiration, actualTrajectory.n_m, expectedTrajectory.Species.evapotra_soil, tolerances.evapotra_soil, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.pF), actualTrajectory.Species.pF, actualTrajectory.n_m, expectedTrajectory.Species.pF, tolerances.pF, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.pFS), actualTrajectory.Species.pFS, actualTrajectory.n_m, expectedTrajectory.Species.pFS, tolerances.pFS, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.pR), actualTrajectory.Species.pR, actualTrajectory.n_m, expectedTrajectory.Species.pR, tolerances.pR, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.pS), actualTrajectory.Species.pS, actualTrajectory.n_m, expectedTrajectory.Species.pS, tolerances.pS, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.RA), actualTrajectory.Species.RA, actualTrajectory.n_m, expectedTrajectory.Species.RA, tolerances.RA, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.RADint), actualTrajectory.Species.RADint, actualTrajectory.n_m, expectedTrajectory.Species.RADint, tolerances.RADint, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.RainInterception), actualTrajectory.Species.RainInterception, actualTrajectory.n_m, expectedTrajectory.Species.RainInterception, tolerances.RainInterception, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.SLA), actualTrajectory.Species.SLA, actualTrajectory.n_m, expectedTrajectory.Species.SLA, tolerances.SLA, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.stems_n), actualTrajectory.Species.stems_n, actualTrajectory.n_m, expectedTrajectory.Species.stems_n, tolerances.stems_n, tolerances.MaxTimestep);
            // stems_n_ha is not in reference worksheets (but could potentially be calculated)
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.stems_n_ha), actualTrajectory.Species.stems_n_ha, actualTrajectory.n_m, expectedTrajectory.Species.stems_n_ha, tolerances.stems_n_ha, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.volume), actualTrajectory.Species.volume, actualTrajectory.n_m, expectedTrajectory.Species.volume, tolerances.volume, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.volume_cum), actualTrajectory.Species.volume_cum, actualTrajectory.n_m, expectedTrajectory.Species.volume_cum, tolerances.volume_cum, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.VPD_sp), actualTrajectory.Species.VPD_sp, actualTrajectory.n_m, expectedTrajectory.Species.VPD_sp, tolerances.VPD_sp, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.wood_density), actualTrajectory.Species.wood_density, actualTrajectory.n_m, expectedTrajectory.Species.wood_density, tolerances.wood_density, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.WUE), actualTrajectory.Species.WUE, actualTrajectory.n_m, expectedTrajectory.Species.WUE, tolerances.WUE, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.WUEtransp), actualTrajectory.Species.WUEtransp, actualTrajectory.n_m, expectedTrajectory.Species.WUEtransp, tolerances.WUEtransp, tolerances.MaxTimestep);
        }
    }
}