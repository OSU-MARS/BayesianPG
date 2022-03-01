using BayesianPG.Extensions;
using BayesianPG.Test.Xlsx;
using BayesianPG.ThreePG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BayesianPG.Test
{
    [TestClass]
    [DeploymentItem("r3PG validation stands.xlsx")]
    public class Test3PG
    {
        /// <summary>
        /// Verifies C# implementations of 3-PGpjs 2.7 and 3-PGmix against reference data for the six r3PG 
        /// validation stands (either as captured from C# in Test3PG.R or from r3PG Fortran via 
        /// GenerateReferenceSpreadsheet.R).
        /// </summary>
        /// <remarks>
        /// Originally developed from https://github.com/trotsiuk/r3PG/blob/master/pkg/tests/r_vba_compare/.
        /// </remarks>
        [TestMethod]
        public void R3PGReferenceStands()
        {
            using TestThreePGReader reader = new("r3PG validation stands.xlsx");
            SortedList<string, ThreePGStandTrajectory<float, int>> expectedTrajectoriesBySiteName = reader.ReadR3PGValidationOutput();
            SortedList<string, ThreePGScalar> sitesByName = reader.ReadSites();
            SortedList<string, StandTrajectoryTolerance> tolerancesBySiteName = new()
            {
                // see Test3PG.R for analysis
                { "broadleaf_mix", new() },
                { "broadleaf_pjs", new() },
                { "evergreen_mix", new() },
                { "evergreen_pjs", new() },
                { "mixtures_eu", new()
                                 {
                                     // if testing against r3PG
                                     // VolumeCumulative = 0.015F // differs because r3PG tabs in r3PG validation scenarios.xlsx are based on r3PG 0.1.3, which does not have the fix for https://github.com/trotsiuk/r3PG/issues/63
                                 }
                },
                { "mixtures_other", new()
                                    {      
                                        // if testing against r3PG
                                        // MaxTimestep = 10 * 12 // verify against only first 10 years due to https://github.com/trotsiuk/r3PG/issues/70
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
                ThreePGScalar threePG = sitesByName.Values[index];
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

        private static void VerifyArray(string name, int[,] actual, int expectedTimesteps, int[,] expectedValues, int tolerance, int maxTimestep)
        {
            Debug.Assert((tolerance >= 0) && (tolerance <= 1));
            int actualMonths = actual.GetLength(0);
            Assert.IsTrue(actualMonths == expectedTimesteps);

            // for now, skip first month as not all values are computed
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
            {
                int expectedSpecies = expectedValues.GetLength(1);
                Assert.IsTrue(actual.GetLength(1) == expectedSpecies);
                for (int speciesIndex = 0; speciesIndex < expectedSpecies; ++speciesIndex)
                {
                    int actualValue = actual[timestepIndex, speciesIndex];
                    int expectedValue = expectedValues[timestepIndex, speciesIndex];
                    int difference = actualValue - expectedValue;
                    Assert.IsTrue(MathF.Abs(difference) <= tolerance, name + "[" + timestepIndex + ", " + speciesIndex + "]: difference = " + difference + ".");
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

        private static void VerifySpeciesParameters(ThreePGScalar threePG)
        {
            TreeSpeciesParameters expectedParameters = TestConstant.TreeParameters; // shorthand for readability
            for (int speciesIndex = 0; speciesIndex < threePG.Parameters.n_sp; ++speciesIndex)
            {
                string speciesName = threePG.Parameters.Species[speciesIndex];
                int verificationIndex = expectedParameters.Species.FindIndex(speciesName);
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

        private static void VerifySizeDistribution(ThreePGScalar threePG)
        {
            if (threePG.Settings.CorrectSizeDistribution)
            {
                Assert.IsTrue(threePG.Bias != null);

                TreeSpeciesSizeDistribution expectedBias = TestConstant.TreeSizeDistributions; // shorthand for readability
                for (int speciesIndex = 0; speciesIndex < threePG.Bias.n_sp; ++speciesIndex)
                {
                    string speciesName = threePG.Parameters.Species[speciesIndex];
                    int verificationIndex = expectedBias.Species.FindIndex(speciesName);
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

        private static void VerifyStandTrajectory(ThreePGScalar threePG, ThreePGStandTrajectory<float, int> expectedTrajectory, StandTrajectoryTolerance tolerances)
        {
            ThreePGStandTrajectory<float, int> actualTrajectory = threePG.Trajectory;

            // verify array sizes and scalar properties
            // Reference trajectories in spreadsheet stop in January of end year rather than extend to February.
            // Also, January predictions are duplicated into February.
            Assert.IsTrue(actualTrajectory.MonthCount >= expectedTrajectory.MonthCount);
            Assert.IsTrue(actualTrajectory.Species.n_sp == expectedTrajectory.Species.n_sp);

            // verify species order matches
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.Species), actualTrajectory.Species.Species, expectedTrajectory.Species.Species);

            // verify stand trajectory
            Test3PG.VerifyArray(nameof(actualTrajectory.AvailableSoilWater), actualTrajectory.AvailableSoilWater, actualTrajectory.MonthCount, expectedTrajectory.AvailableSoilWater, tolerances.AvailableSoilWater, tolerances.MaxTimestep);
            // Test3PG.VerifyArray(nameof(actualTrajectory.DayLength), actualTrajectory.DayLength, actualTrajectory.MonthCount, expectedTrajectory.DayLength, tolerances.DayLength, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.evapo_transp), actualTrajectory.evapo_transp, actualTrajectory.MonthCount, expectedTrajectory.evapo_transp, tolerances.Evapotranspiration, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.f_transp_scale), actualTrajectory.f_transp_scale, actualTrajectory.MonthCount, expectedTrajectory.f_transp_scale, tolerances.TranspirationScale, tolerances.MaxTimestep);
            Assert.IsTrue((actualTrajectory.From.Year == expectedTrajectory.From.Year) && (actualTrajectory.From.Month == expectedTrajectory.From.Month), nameof(actualTrajectory.From));
            Test3PG.VerifyArray(nameof(actualTrajectory.irrig_supl), actualTrajectory.irrig_supl, actualTrajectory.MonthCount, expectedTrajectory.irrig_supl, tolerances.IrrigationSupplied, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.prcp_runoff), actualTrajectory.prcp_runoff, actualTrajectory.MonthCount, expectedTrajectory.prcp_runoff, tolerances.PrecipitationRunoff, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.conduct_soil), actualTrajectory.conduct_soil, actualTrajectory.MonthCount, expectedTrajectory.conduct_soil, tolerances.SoilConductance, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.evapotra_soil), actualTrajectory.evapotra_soil, actualTrajectory.MonthCount, expectedTrajectory.evapotra_soil, tolerances.SoilEvaporation, tolerances.MaxTimestep);

            Test3PG.VerifyArray(nameof(actualTrajectory.Species.aero_resist), actualTrajectory.Species.aero_resist, actualTrajectory.MonthCount, expectedTrajectory.Species.aero_resist, tolerances.AeroResist, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.age), actualTrajectory.Species.age, actualTrajectory.MonthCount, expectedTrajectory.Species.age, tolerances.Age, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.age_m), actualTrajectory.Species.age_m, actualTrajectory.MonthCount, expectedTrajectory.Species.age_m, tolerances.AgeM, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.alpha_c), actualTrajectory.Species.alpha_c, actualTrajectory.MonthCount, expectedTrajectory.Species.alpha_c, tolerances.AlphaC, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.basal_area), actualTrajectory.Species.basal_area, actualTrajectory.MonthCount, expectedTrajectory.Species.basal_area, tolerances.BasalArea, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_foliage), actualTrajectory.Species.biom_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_foliage, tolerances.BiomassFoliage, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_foliage_debt), actualTrajectory.Species.biom_foliage_debt, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_foliage_debt, tolerances.BiomassFoliageDebt, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_root), actualTrajectory.Species.biom_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_root, tolerances.BiomassRoot, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_stem), actualTrajectory.Species.biom_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_stem, tolerances.BiomassStem, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.canopy_cover), actualTrajectory.Species.canopy_cover, actualTrajectory.MonthCount, expectedTrajectory.Species.canopy_cover, tolerances.CanopyCover, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.canopy_vol_frac), actualTrajectory.Species.canopy_vol_frac, actualTrajectory.MonthCount, expectedTrajectory.Species.canopy_vol_frac, tolerances.CanopyVolumeFraction, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.conduct_canopy), actualTrajectory.Species.conduct_canopy, actualTrajectory.MonthCount, expectedTrajectory.Species.conduct_canopy, tolerances.CanopyConductance, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.crown_width), actualTrajectory.Species.crown_width, actualTrajectory.MonthCount, expectedTrajectory.Species.crown_width, tolerances.CrownWidth, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.dbh), actualTrajectory.Species.dbh, actualTrajectory.MonthCount, expectedTrajectory.Species.dbh, tolerances.Dbh, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.epsilon_biom_stem), actualTrajectory.Species.epsilon_biom_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_biom_stem, tolerances.EpsilonStemBiomass, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.epsilon_gpp), actualTrajectory.Species.epsilon_gpp, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_gpp, tolerances.EpsilonGpp, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.epsilon_npp), actualTrajectory.Species.epsilon_npp, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_npp, tolerances.EpsilonNpp, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.fi), actualTrajectory.Species.fi, actualTrajectory.MonthCount, expectedTrajectory.Species.fi, tolerances.FractionApar, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.fracBB), actualTrajectory.Species.fracBB, actualTrajectory.MonthCount, expectedTrajectory.Species.fracBB, tolerances.FracBB, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_age), actualTrajectory.Species.f_age, actualTrajectory.MonthCount, expectedTrajectory.Species.f_age, tolerances.ModifierAge, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_calpha), actualTrajectory.Species.f_calpha, actualTrajectory.MonthCount, expectedTrajectory.Species.f_calpha, tolerances.ModiferCAlpha, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_cg), actualTrajectory.Species.f_cg, actualTrajectory.MonthCount, expectedTrajectory.Species.f_cg, tolerances.ModifierCG, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_frost), actualTrajectory.Species.f_frost, actualTrajectory.MonthCount, expectedTrajectory.Species.f_frost, tolerances.ModifierFrost, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_nutr), actualTrajectory.Species.f_nutr, actualTrajectory.MonthCount, expectedTrajectory.Species.f_nutr, tolerances.ModifierNutrition, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_phys), actualTrajectory.Species.f_phys, actualTrajectory.MonthCount, expectedTrajectory.Species.f_phys, tolerances.ModifierPhysiological, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_sw), actualTrajectory.Species.f_sw, actualTrajectory.MonthCount, expectedTrajectory.Species.f_sw, tolerances.ModifierSoilWater, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_tmp), actualTrajectory.Species.f_tmp, actualTrajectory.MonthCount, expectedTrajectory.Species.f_tmp, tolerances.ModifierTemperature, tolerances.MaxTimestep);
            if (threePG.Settings.phys_model == ThreePGModel.Mix)
            {
                // f_tmp_gc is hard coded to 1 in 3-PGpjs but the reference worksheets contain the values calculated prior to pjs forcing it to 1
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_tmp_gc), actualTrajectory.Species.f_tmp_gc, actualTrajectory.MonthCount, expectedTrajectory.Species.f_tmp_gc, tolerances.ModifierTemperatureGC, tolerances.MaxTimestep);
            }
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.f_vpd), actualTrajectory.Species.f_vpd, actualTrajectory.MonthCount, expectedTrajectory.Species.f_vpd, tolerances.ModifierVpd, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.gammaF), actualTrajectory.Species.gammaF, actualTrajectory.MonthCount, expectedTrajectory.Species.gammaF, tolerances.GammaF, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.gammaN), actualTrajectory.Species.gammaN, actualTrajectory.MonthCount, expectedTrajectory.Species.gammaN, tolerances.GammaN, tolerances.MaxTimestep);
            // not currently logged from C# Test3PG.VerifyArray(nameof(actualTrajectory.Species.Gc_mol), actualTrajectory.Species.Gc_mol, actualTrajectory.MonthCount, expectedTrajectory.Species.Gc_mol, tolerances.GcMol, tolerances.MaxTimestep);
            // not currently logged from C# Test3PG.VerifyArray(nameof(actualTrajectory.Species.Gw_mol), actualTrajectory.Species.Gw_mol, actualTrajectory.MonthCount, expectedTrajectory.Species.Gw_mol, tolerances.GwMol, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.GPP), actualTrajectory.Species.GPP, actualTrajectory.MonthCount, expectedTrajectory.Species.GPP, tolerances.Gpp, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.height), actualTrajectory.Species.height, actualTrajectory.MonthCount, expectedTrajectory.Species.height, tolerances.Height, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.lai), actualTrajectory.Species.lai, actualTrajectory.MonthCount, expectedTrajectory.Species.lai, tolerances.Lai, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.lai_above), actualTrajectory.Species.lai_above, actualTrajectory.MonthCount, expectedTrajectory.Species.lai_above, tolerances.LaiAbove, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.lai_sa_ratio), actualTrajectory.Species.lai_sa_ratio, actualTrajectory.MonthCount, expectedTrajectory.Species.lai_sa_ratio, tolerances.LaiToSurfaceAreaRatio, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.lambda_h), actualTrajectory.Species.lambda_h, actualTrajectory.MonthCount, expectedTrajectory.Species.lambda_h, tolerances.LambdaH, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.lambda_v), actualTrajectory.Species.lambda_v, actualTrajectory.MonthCount, expectedTrajectory.Species.lambda_v, tolerances.LambdaV, tolerances.MaxTimestep);
            // TODO: handle Fortran using ones based layer numbering instead of zero based
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.layer_id), actualTrajectory.Species.layer_id, actualTrajectory.MonthCount, expectedTrajectory.Species.layer_id, tolerances.LayerID, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.NPP_f), actualTrajectory.Species.NPP_f, actualTrajectory.MonthCount, expectedTrajectory.Species.NPP_f, tolerances.NppF, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.prcp_interc), actualTrajectory.Species.prcp_interc, actualTrajectory.MonthCount, expectedTrajectory.Species.prcp_interc, tolerances.PrecipitationInterception, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.SLA), actualTrajectory.Species.SLA, actualTrajectory.MonthCount, expectedTrajectory.Species.SLA, tolerances.Sla, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.stems_n), actualTrajectory.Species.stems_n, actualTrajectory.MonthCount, expectedTrajectory.Species.stems_n, tolerances.StemsN, tolerances.MaxTimestep);
            // stems_n_ha is not in reference worksheets (but could potentially be calculated)
            // Test3PG.VerifyArray(nameof(actualTrajectory.Species.stems_n_ha), actualTrajectory.Species.stems_n_ha, actualTrajectory.MonthCount, expectedTrajectory.Species.stems_n_ha, tolerances.stems_n_ha, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.transp_veg), actualTrajectory.Species.transp_veg, actualTrajectory.MonthCount, expectedTrajectory.Species.transp_veg, tolerances.TranspirationVegetation, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.volume), actualTrajectory.Species.volume, actualTrajectory.MonthCount, expectedTrajectory.Species.volume, tolerances.Volume, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.VPD_sp), actualTrajectory.Species.VPD_sp, actualTrajectory.MonthCount, expectedTrajectory.Species.VPD_sp, tolerances.VpdSp, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.wood_density), actualTrajectory.Species.wood_density, actualTrajectory.MonthCount, expectedTrajectory.Species.wood_density, tolerances.WoodDensity, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.WUE), actualTrajectory.Species.WUE, actualTrajectory.MonthCount, expectedTrajectory.Species.WUE, tolerances.Wue, tolerances.MaxTimestep);
            Test3PG.VerifyArray(nameof(actualTrajectory.Species.WUEtransp), actualTrajectory.Species.WUEtransp, actualTrajectory.MonthCount, expectedTrajectory.Species.WUEtransp, tolerances.WueTransp, tolerances.MaxTimestep);

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.CVdbhDistribution), actualTrajectory.Species.CVdbhDistribution, actualTrajectory.MonthCount, expectedTrajectory.Species.CVdbhDistribution, tolerances.CVdbhDistribution, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.CVwsDistribution), actualTrajectory.Species.CVwsDistribution, actualTrajectory.MonthCount, expectedTrajectory.Species.CVwsDistribution, tolerances.CVwsDistribution, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DWeibullScale), actualTrajectory.Species.DWeibullScale, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullScale, tolerances.DWeibullScale, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DWeibullShape), actualTrajectory.Species.DWeibullShape, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullShape, tolerances.DWeibullShape, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DWeibullLocation), actualTrajectory.Species.DWeibullLocation, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullLocation, tolerances.DWeibullLocation, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DrelBiaspFS), actualTrajectory.Species.DrelBiaspFS, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiaspFS, tolerances.DrelBiaspFS, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DrelBiasheight), actualTrajectory.Species.DrelBiasheight, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasheight, tolerances.DrelBiasheight, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DrelBiasCrowndiameter), actualTrajectory.Species.DrelBiasCrowndiameter, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasCrowndiameter, tolerances.DrelBiasCrowndiameter, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.DrelBiasLCL), actualTrajectory.Species.DrelBiasLCL, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasLCL, tolerances.DrelBiasLCL, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.height_rel), actualTrajectory.Species.height_rel, actualTrajectory.MonthCount, expectedTrajectory.Species.height_rel, tolerances.HeightRelative, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.wsrelBias), actualTrajectory.Species.wsrelBias, actualTrajectory.MonthCount, expectedTrajectory.Species.wsrelBias, tolerances.WSRelBias, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.wsWeibullScale), actualTrajectory.Species.wsWeibullScale, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullScale, tolerances.WSWeibullScale, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.wsWeibullShape), actualTrajectory.Species.wsWeibullShape, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullShape, tolerances.WSWeibullShape, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.wsWeibullLocation), actualTrajectory.Species.wsWeibullLocation, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullLocation, tolerances.WSWeibullLocation, tolerances.MaxTimestep);
            }

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
            {
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.D13CNewPS), actualTrajectory.Species.D13CNewPS, actualTrajectory.MonthCount, expectedTrajectory.Species.D13CNewPS, tolerances.D13CNewPS, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.D13CTissue), actualTrajectory.Species.D13CTissue, actualTrajectory.MonthCount, expectedTrajectory.Species.D13CTissue, tolerances.D13CTissue, tolerances.MaxTimestep);
                // TODO: handle Fortran multiplying InterCi by 1 million in i_write_out.h
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.InterCi), actualTrajectory.Species.InterCi, actualTrajectory.MonthCount, expectedTrajectory.Species.InterCi, tolerances.InterCi, tolerances.MaxTimestep);
            }

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_incr_foliage), actualTrajectory.Species.biom_incr_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_foliage, tolerances.BiomassIncrementFoliage, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_incr_root), actualTrajectory.Species.biom_incr_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_root, tolerances.BiomassIncrementRoot, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_incr_stem), actualTrajectory.Species.biom_incr_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_stem, tolerances.BiomassIncrementStem, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_loss_foliage), actualTrajectory.Species.biom_loss_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_loss_foliage, tolerances.BiomassLossFoliage, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.biom_loss_root), actualTrajectory.Species.biom_loss_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_loss_root, tolerances.BiomassLossRoot, tolerances.MaxTimestep);
                Test3PG.VerifyArray(nameof(actualTrajectory.Species.volume_cum), actualTrajectory.Species.volume_cum, actualTrajectory.MonthCount, expectedTrajectory.Species.volume_cum, tolerances.VolumeCumulative, tolerances.MaxTimestep);
            }
        }
    }
}