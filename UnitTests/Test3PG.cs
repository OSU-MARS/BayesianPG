using BayesianPG.Extensions;
using BayesianPG.Test.Xlsx;
using BayesianPG.ThreePG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
            // setup
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
                {
                    "mixtures_eu",
                    new()
                    {
                        // if testing against r3PG
                        // VolumeCumulative = 0.015F // differs because r3PG tabs in r3PG validation scenarios.xlsx are based on r3PG 0.1.3, which does not have the fix for https://github.com/trotsiuk/r3PG/issues/63
                    }
                },
                {
                    "mixtures_other",
                    new()
                    {
                        // if testing against r3PG
                        // MaxTimestep = 10 * 12 // verify against only first 10 years due to https://github.com/trotsiuk/r3PG/issues/70
                    }
                },
            };
            SortedList<string, StandTrajectoryTolerance> tolerancesBySiteName128 = new()
            {
                { "broadleaf_mix", new(0.00041F) },
                { "broadleaf_pjs", new(0.00017F) },
                { "evergreen_mix", new(0.00076F) },
                { "evergreen_pjs", new(0.00018F) },
                { "mixtures_eu", new(0.0056F) },
                { "mixtures_other", new(0.00062F) },
            };

            // verify actual and expected sites match
            for (int siteIndex = 0; siteIndex < expectedTrajectoriesBySiteName.Count; ++siteIndex)
            {
                string siteName = sitesByName.Keys[siteIndex];
                string expectedSiteName = expectedTrajectoriesBySiteName.Keys[siteIndex];

                Assert.IsTrue(String.Equals(siteName, expectedSiteName));
            }

            // scalar simulations
            for (int siteIndex = 0; siteIndex < sitesByName.Count; ++siteIndex)
            {
                // filter to specific site for debugging
                //if (sitesByName.Keys[siteIndex] != "mixtures_eu")
                //{
                //    continue;
                //}

                // repeated runs on the same site as a basic test of state initialization
                for (int iteration = 0; iteration < 2; ++iteration)
                {
                    ThreePGScalar threePG = sitesByName.Values[siteIndex];
                    threePG.PredictStandTrajectory();

                    Test3PG.VerifySpeciesParameters(threePG);
                    Test3PG.VerifySizeDistribution(threePG);
                    Test3PG.VerifyStandTrajectory(threePG, expectedTrajectoriesBySiteName.Values[siteIndex], tolerancesBySiteName.Values[siteIndex], iteration);
                }
            }

            // 128 bit SIMD simulations
            for (int siteIndex = 0; siteIndex < sitesByName.Count; ++siteIndex)
            {
                // filter to specific site for debugging
                //if (sitesByName.Keys[siteIndex] != "mixtures_eu")
                //{
                //    continue;
                //}

                ThreePGScalar threePGScalar = sitesByName.Values[siteIndex];
                ThreePGSimd128 threePG128 = new(threePGScalar)
                {
                    Bias = threePGScalar.Bias
                };
                threePG128.PredictStandTrajectory();

                Test3PG.VerifySpeciesParameters(threePG128);
                Test3PG.VerifySizeDistribution(threePG128);
                Test3PG.VerifyStandTrajectory(threePG128, expectedTrajectoriesBySiteName.Values[siteIndex], tolerancesBySiteName128.Values[siteIndex], iteration: 0);
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, float[,] actual, int expectedTimesteps, float[,] expectedValues, float tolerance, int maxTimestep, int iteration)
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
                        Assert.IsTrue(MathF.Abs(difference) <= differenceThreshold, threePG + ": " + variable + "[" + timestepIndex + ", " + speciesIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                    }
                    else
                    {
                        float ratio = actualValue / expectedValue;
                        Assert.IsTrue((ratio >= minRatio) && (ratio <= maxRatio), threePG + ": " + variable + "[" + timestepIndex + ", " + speciesIndex + "]: ratio = " + ratio + " at iteration " + iteration + ".");
                    }
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, int[,] actual, int expectedTimesteps, int[,] expectedValues, int tolerance, int maxTimestep, int iteration)
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
                    Assert.IsTrue(MathF.Abs(difference) <= tolerance, threePG.Site.Name + ": " + variable + "[" + timestepIndex + ", " + speciesIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, float[] actual, int expectedTimesteps, float[] expectedValues, float tolerance, int maxTimestep, int iteration)
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
                    Assert.IsTrue(MathF.Abs(difference) <= differenceThreshold, threePG.Site.Name + ": " + variable + "[" + timestepIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                }
                else
                {
                    float ratio = actualValue / expectedValue;
                    Assert.IsTrue((ratio >= minRatio) && (ratio <= maxRatio), threePG.Site.Name + ": " + variable + "[" + timestepIndex + "]: ratio = " + ratio + " at iteration " + iteration + ".");
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, float[][] actual, int expectedTimesteps, float[][] expectedValues, float tolerance, int maxTimestep, int iteration)
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
                        Assert.IsTrue(MathF.Abs(difference) <= differenceThreshold, threePG.Site.Name + ": " + variable + "[" + speciesIndex + "][" + timestepIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                    }
                    else
                    {
                        float ratio = actualValue / expectedValue;
                        Assert.IsTrue((ratio >= minRatio) && (ratio <= maxRatio), threePG.Site.Name + ": " + variable + "[" + speciesIndex + "][" + timestepIndex + "]: ratio = " + ratio + " at iteration " + iteration + ".");
                    }
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, string[] actual, string[] expectedValues, int iteration)
        {
            Assert.IsTrue(actual.Length == expectedValues.Length);

            // for now, skip first month as not all values are computed
            // also skip last expected month as it's duplicate or incomplete
            int maxTimestep = expectedValues.Length - 1;
            for (int timestepIndex = 1; timestepIndex < maxTimestep; ++timestepIndex)
            {
                string actualValue = actual[timestepIndex];
                string expectedValue = expectedValues[timestepIndex];
                Assert.IsTrue(String.Equals(actualValue, expectedValue, StringComparison.Ordinal), threePG.Site.Name + ": " + variable + "[" + timestepIndex + "]: '" + actualValue + "' instead of '" + expectedValue + "' at " + iteration + ".");
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, Vector128<float>[,] actual, int expectedTimesteps, float[,] expectedValues, float tolerance, int maxTimestep, int iteration)
        {
            Debug.Assert((tolerance > 0.0F) && (tolerance <= 1.0F));
            int actualMonths = actual.GetLength(0);
            Assert.IsTrue(actualMonths == expectedTimesteps);

            // for now, skip first month as not all values are computed
            float differenceThreshold = 0.1F * tolerance;
            Vector128<float> differenceThreshold128 = AvxExtensions.BroadcastScalarToVector128(differenceThreshold);
            Vector128<float> maxRatio = AvxExtensions.BroadcastScalarToVector128(1.0F + tolerance);
            Vector128<float> minRatio = AvxExtensions.BroadcastScalarToVector128(1.0F - tolerance);
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int timestep = 1; timestep < maxTimestepVerified; ++timestep)
            {
                int expectedSpecies = expectedValues.GetLength(1);
                Assert.IsTrue(actual.GetLength(1) == expectedSpecies);
                for (int speciesIndex = 0; speciesIndex < expectedSpecies; ++speciesIndex)
                {
                    Vector128<float> actualValue = actual[timestep, speciesIndex];
                    float expectedValue = expectedValues[timestep, speciesIndex];
                    Vector128<float> expectedValue128 = AvxExtensions.BroadcastScalarToVector128(expectedValue);
                    if (MathF.Abs(expectedValue) < differenceThreshold)
                    {
                        Vector128<float> difference = Avx.Subtract(actualValue, expectedValue128);
                        AssertV.IsTrue(Avx.CompareLessThanOrEqual(AvxExtensions.Abs(difference), differenceThreshold128), threePG + ": " + variable + "[" + timestep + ", " + speciesIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                    }
                    else
                    {
                        Vector128<float> ratio = Avx.Divide(actualValue, expectedValue128);
                        AssertV.IsTrue(Avx.And(Avx.CompareGreaterThanOrEqual(ratio, minRatio), Avx.CompareLessThanOrEqual(ratio, maxRatio)), threePG + ": " + variable + "[" + timestep + ", " + speciesIndex + "]: ratio = " + ratio + " at iteration " + iteration + ".");
                    }
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, Vector128<int>[,] actual, int expectedTimesteps, int[,] expectedValues, int tolerance, int maxTimestep, int iteration)
        {
            Debug.Assert((tolerance >= 0) && (tolerance <= 1));
            int actualMonths = actual.GetLength(0);
            Assert.IsTrue(actualMonths == expectedTimesteps);

            // for now, skip first month as not all values are computed
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            Vector128<float> tolerance128 = Avx2Extensions.BroadcastScalarToVector128((float)tolerance);
            for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
            {
                int expectedSpecies = expectedValues.GetLength(1);
                Assert.IsTrue(actual.GetLength(1) == expectedSpecies);
                for (int speciesIndex = 0; speciesIndex < expectedSpecies; ++speciesIndex)
                {
                    Vector128<int> actualValue = actual[timestepIndex, speciesIndex];                    
                    Vector128<int> expectedValue = Avx2Extensions.BroadcastScalarToVector128(expectedValues[timestepIndex, speciesIndex]);
                    Vector128<int> difference = Avx.Subtract(actualValue, expectedValue);
                    AssertV.IsTrue(Avx.CompareLessThanOrEqual(AvxExtensions.Abs(difference.AsSingle()), tolerance128), threePG.Site.Name + ": " + variable + "[" + timestepIndex + ", " + speciesIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, Vector128<float>[] actual, int expectedTimesteps, float[] expectedValues, float tolerance, int maxTimestep, int iteration)
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
            Vector128<float> differenceThreshold128 = AvxExtensions.BroadcastScalarToVector128(differenceThreshold);
            Vector128<float> maxRatio = AvxExtensions.BroadcastScalarToVector128(1.0F + tolerance);
            Vector128<float> minRatio = AvxExtensions.BroadcastScalarToVector128(1.0F - tolerance);
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
            {
                Vector128<float> actualValue = actual[timestepIndex];
                float expectedValue = expectedValues[timestepIndex];
                Vector128<float> expectedValue128 = AvxExtensions.BroadcastScalarToVector128(expectedValue);
                if (MathF.Abs(expectedValue) < differenceThreshold)
                {
                    Vector128<float> difference = Avx.Subtract(actualValue, expectedValue128);
                    AssertV.IsTrue(Avx.CompareLessThanOrEqual(AvxExtensions.Abs(difference), differenceThreshold128), threePG.Site.Name + ": " + variable + "[" + timestepIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                }
                else
                {
                    Vector128<float> ratio = Avx.Divide(actualValue, expectedValue128);
                    AssertV.IsTrue(Avx.And(Avx.CompareGreaterThanOrEqual(ratio, minRatio), Avx.CompareLessThanOrEqual(ratio, maxRatio)), threePG.Site.Name + ": " + variable + "[" + timestepIndex + "]: ratio = " + ratio + " at iteration " + iteration + ".");
                }
            }
        }

        private static void VerifyArray(ThreePGpjsMix threePG, string variable, Vector128<float>[][] actual, int expectedTimesteps, float[][] expectedValues, float tolerance, int maxTimestep, int iteration)
        {
            Debug.Assert((tolerance > 0.0F) && (tolerance <= 1.0F));
            int expectedSpecies = expectedValues.Length;
            Assert.IsTrue(actual.Length == expectedSpecies);

            float differenceThreshold = 0.1F * tolerance;
            Vector128<float> differenceThreshold128 = AvxExtensions.BroadcastScalarToVector128(differenceThreshold);
            Vector128<float> maxRatio = AvxExtensions.BroadcastScalarToVector128(1.0F + tolerance);
            Vector128<float> minRatio = AvxExtensions.BroadcastScalarToVector128(1.0F - tolerance);
            int maxTimestepVerified = Math.Min(expectedTimesteps, maxTimestep);
            for (int speciesIndex = 0; speciesIndex < expectedSpecies; ++speciesIndex)
            {
                Vector128<float>[] actualValuesForSpecies = actual[speciesIndex];
                int actualMonths = actualValuesForSpecies.Length;
                Assert.IsTrue(actualMonths == expectedTimesteps);

                float[] expectedValuesForSpecies = expectedValues[speciesIndex];
                // for now, skip first month as not all values are computed
                for (int timestepIndex = 1; timestepIndex < maxTimestepVerified; ++timestepIndex)
                {
                    Vector128<float> actualValue = actualValuesForSpecies[timestepIndex];
                    float expectedValue = expectedValuesForSpecies[timestepIndex];
                    Vector128<float> expectedValue128 = AvxExtensions.BroadcastScalarToVector128(expectedValue);
                    if (MathF.Abs(expectedValue) < differenceThreshold)
                    {
                        Vector128<float> difference = Avx.Subtract(actualValue, expectedValue128);
                        AssertV.IsTrue(Avx.CompareLessThanOrEqual(AvxExtensions.Abs(difference), differenceThreshold128), threePG.Site.Name + ": " + variable + "[" + speciesIndex + "][" + timestepIndex + "]: difference = " + difference + " at iteration " + iteration + ".");
                    }
                    else
                    {
                        Vector128<float> ratio = Avx.Divide(actualValue, expectedValue128);
                        AssertV.IsTrue(Avx.And(Avx.CompareGreaterThanOrEqual(ratio, minRatio), Avx.CompareLessThanOrEqual(ratio, maxRatio)), threePG.Site.Name + ": " + variable + "[" + speciesIndex + "][" + timestepIndex + "]: ratio = " + ratio + " at iteration " + iteration + ".");
                    }
                }
            }
        }

        private static void VerifySpeciesParameters(ThreePGScalar threePG)
        {
            TreeSpeciesParameters<float> expectedParameters = TestConstant.TreeParameters; // shorthand for readability
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
                    Assert.IsTrue(threePG.Parameters.rho0[speciesIndex] == expectedParameters.rho0[verificationIndex], nameof(expectedParameters.rho0));
                    Assert.IsTrue(threePG.Parameters.rho1[speciesIndex] == expectedParameters.rho1[verificationIndex], nameof(expectedParameters.rho1));
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

        private static void VerifySpeciesParameters(ThreePGSimd128 threePG)
        {
            TreeSpeciesParameters<float> expectedParameters = TestConstant.TreeParameters; // shorthand for readability
            for (int speciesIndex = 0; speciesIndex < threePG.Parameters.n_sp; ++speciesIndex)
            {
                string speciesName = threePG.Parameters.Species[speciesIndex];
                int verificationIndex = expectedParameters.Species.FindIndex(speciesName);
                if (verificationIndex != -1)
                {
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.pFS2[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.pFS2[verificationIndex])), nameof(expectedParameters.pFS2));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.pFS20[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.pFS20[verificationIndex])), nameof(expectedParameters.pFS20));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.aWS[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.aWS[verificationIndex])), nameof(expectedParameters.aWS));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nWS[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nWS[verificationIndex])), nameof(expectedParameters.nWS));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.pRx[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.pRx[verificationIndex])), nameof(expectedParameters.pRx));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.pRn[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.pRn[verificationIndex])), nameof(expectedParameters.pRn));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.gammaF1[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.gammaF1[verificationIndex])), nameof(expectedParameters.gammaF1));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.gammaN0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.gammaN0[verificationIndex])), nameof(expectedParameters.gammaN0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.tgammaF[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.tgammaF[verificationIndex])), nameof(expectedParameters.tgammaF));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.gammaR[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.gammaR[verificationIndex])), nameof(expectedParameters.gammaR));
                    Assert.IsTrue(threePG.Parameters.leafgrow[speciesIndex] == expectedParameters.leafgrow[verificationIndex], nameof(expectedParameters.leafgrow));
                    Assert.IsTrue(threePG.Parameters.leaffall[speciesIndex] == expectedParameters.leaffall[verificationIndex], nameof(expectedParameters.leaffall));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.Tmin[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.Tmin[verificationIndex])), nameof(expectedParameters.Tmin));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.Topt[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.Topt[verificationIndex])), nameof(expectedParameters.Topt));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.Tmax[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.Tmax[verificationIndex])), nameof(expectedParameters.Tmax));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.kF[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.kF[verificationIndex])), nameof(expectedParameters.kF));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.SWconst0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.SWconst0[verificationIndex])), nameof(expectedParameters.SWconst0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.SWpower0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.SWpower0[verificationIndex])), nameof(expectedParameters.SWpower0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.fCalpha700[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.fCalpha700[verificationIndex])), nameof(expectedParameters.fCalpha700));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.fCg700[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.fCg700[verificationIndex])), nameof(expectedParameters.fCg700));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.m0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.m0[verificationIndex])), nameof(expectedParameters.m0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.fN0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.fN0[verificationIndex])), nameof(expectedParameters.fN0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.MaxAge[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.MaxAge[verificationIndex])), nameof(expectedParameters.MaxAge));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nAge[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nAge[verificationIndex])), nameof(expectedParameters.nAge));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.rAge[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.rAge[verificationIndex])), nameof(expectedParameters.rAge));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.gammaN1[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.gammaN1[verificationIndex])), nameof(expectedParameters.gammaN1));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.gammaN0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.gammaN0[verificationIndex])), nameof(expectedParameters.gammaN0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.tgammaN[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.tgammaN[verificationIndex])), nameof(expectedParameters.tgammaN));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.ngammaN[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.ngammaN[verificationIndex])), nameof(expectedParameters.ngammaN));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.thinPower[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.thinPower[verificationIndex])), nameof(expectedParameters.thinPower));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.mF[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.mF[verificationIndex])), nameof(expectedParameters.mF));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.mR[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.mR[verificationIndex])), nameof(expectedParameters.mR));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.mS[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.mS[verificationIndex])), nameof(expectedParameters.mS));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.SLA0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.SLA0[verificationIndex])), nameof(expectedParameters.SLA0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.SLA1[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.SLA1[verificationIndex])), nameof(expectedParameters.SLA1));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.tSLA[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.tSLA[verificationIndex])), nameof(expectedParameters.tSLA));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.k[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.k[verificationIndex])), nameof(expectedParameters.k));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.fullCanAge[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.fullCanAge[verificationIndex])), nameof(expectedParameters.fullCanAge));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.MaxIntcptn[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.MaxIntcptn[verificationIndex])), nameof(expectedParameters.MaxIntcptn));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.LAImaxIntcptn[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.LAImaxIntcptn[verificationIndex])), nameof(expectedParameters.LAImaxIntcptn));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.cVPD[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.cVPD[verificationIndex])), nameof(expectedParameters.cVPD));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.alphaCx[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.alphaCx[verificationIndex])), nameof(expectedParameters.alphaCx));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.Y[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.Y[verificationIndex])), nameof(expectedParameters.Y));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.MinCond[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.MinCond[verificationIndex])), nameof(expectedParameters.MinCond));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.MaxCond[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.MaxCond[verificationIndex])), nameof(expectedParameters.MaxCond));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.LAIgcx[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.LAIgcx[verificationIndex])), nameof(expectedParameters.LAIgcx));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.CoeffCond[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.CoeffCond[verificationIndex])), nameof(expectedParameters.CoeffCond));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.BLcond[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.BLcond[verificationIndex])), nameof(expectedParameters.BLcond));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.RGcGw[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.RGcGw[verificationIndex])), nameof(expectedParameters.RGcGw));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.D13CTissueDif[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.D13CTissueDif[verificationIndex])), nameof(expectedParameters.D13CTissueDif));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.aFracDiffu[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.aFracDiffu[verificationIndex])), nameof(expectedParameters.aFracDiffu));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.bFracRubi[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.bFracRubi[verificationIndex])), nameof(expectedParameters.bFracRubi));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.fracBB0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.fracBB0[verificationIndex])), nameof(expectedParameters.fracBB0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.fracBB1[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.fracBB1[verificationIndex])), nameof(expectedParameters.fracBB1));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.tBB[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.tBB[verificationIndex])), nameof(expectedParameters.tBB));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.rho0[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.rho0[verificationIndex])), nameof(expectedParameters.rho0));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.rho1[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.rho1[verificationIndex])), nameof(expectedParameters.rho1));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.tRho[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.tRho[verificationIndex])), nameof(expectedParameters.tRho));
                    Assert.IsTrue(threePG.Parameters.CrownShape[speciesIndex] == expectedParameters.CrownShape[verificationIndex], nameof(expectedParameters.CrownShape));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.aH[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.aH[verificationIndex])), nameof(expectedParameters.aH));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nHB[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nHB[verificationIndex])), nameof(expectedParameters.nHB));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nHC[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nHC[verificationIndex])), nameof(expectedParameters.nHC));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.aV[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.aV[verificationIndex])), nameof(expectedParameters.aV));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nVB[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nVB[verificationIndex])), nameof(expectedParameters.nVB));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nVH[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nVH[verificationIndex])), nameof(expectedParameters.nVH));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nVBH[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nVBH[verificationIndex])), nameof(expectedParameters.nVBH));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.aK[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.aK[verificationIndex])), nameof(expectedParameters.aK));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nKB[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nKB[verificationIndex])), nameof(expectedParameters.nKB));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nKH[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nKH[verificationIndex])), nameof(expectedParameters.nKH));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nKC[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nKC[verificationIndex])), nameof(expectedParameters.nKC));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nKrh[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nKrh[verificationIndex])), nameof(expectedParameters.nKrh));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.aHL[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.aHL[verificationIndex])), nameof(expectedParameters.aHL));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nHLB[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nHLB[verificationIndex])), nameof(expectedParameters.nHLB));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nHLL[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nHLL[verificationIndex])), nameof(expectedParameters.nHLL));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nHLC[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nHLC[verificationIndex])), nameof(expectedParameters.nHLC));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.nHLrh[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.nHLrh[verificationIndex])), nameof(expectedParameters.nHLrh));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.Qa[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.Qa[verificationIndex])), nameof(expectedParameters.Qa));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.Qb[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.Qb[verificationIndex])), nameof(expectedParameters.Qb));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.gDM_mol[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.gDM_mol[verificationIndex])), nameof(expectedParameters.gDM_mol));
                    AssertV.IsTrue(Avx.CompareEqual(threePG.Parameters.molPAR_MJ[speciesIndex], AvxExtensions.BroadcastScalarToVector128(expectedParameters.molPAR_MJ[verificationIndex])), nameof(expectedParameters.molPAR_MJ));
                }
            }
        }

        private static void VerifySizeDistribution<TFloat, TInteger>(ThreePGpjsMix<TFloat, TInteger> threePG)
            where TFloat : struct
            where TInteger : struct
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

        private static void VerifyStandTrajectory(ThreePGScalar threePG, ThreePGStandTrajectory<float, int> expectedTrajectory, StandTrajectoryTolerance tolerances, int iteration)
        {
            ThreePGStandTrajectory<float, int> actualTrajectory = threePG.Trajectory;

            // verify array sizes and scalar properties
            // Reference trajectories in spreadsheet stop in January of end year rather than extend to February.
            // Also, January predictions are duplicated into February.
            Assert.IsTrue(actualTrajectory.MonthCount >= expectedTrajectory.MonthCount);
            Assert.IsTrue(actualTrajectory.Species.n_sp == expectedTrajectory.Species.n_sp);

            // verify species order matches
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.Species), actualTrajectory.Species.Species, expectedTrajectory.Species.Species, iteration);

            // verify stand trajectory
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.AvailableSoilWater), actualTrajectory.AvailableSoilWater, actualTrajectory.MonthCount, expectedTrajectory.AvailableSoilWater, tolerances.AvailableSoilWater, tolerances.MaxTimestep, iteration);
            // Test3PG.VerifyArray(threePG, nameof(actualTrajectory.DayLength), actualTrajectory.DayLength, actualTrajectory.MonthCount, expectedTrajectory.DayLength, tolerances.DayLength, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.evapo_transp), actualTrajectory.evapo_transp, actualTrajectory.MonthCount, expectedTrajectory.evapo_transp, tolerances.Evapotranspiration, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.f_transp_scale), actualTrajectory.f_transp_scale, actualTrajectory.MonthCount, expectedTrajectory.f_transp_scale, tolerances.TranspirationScale, tolerances.MaxTimestep, iteration);
            Assert.IsTrue((actualTrajectory.From.Year == expectedTrajectory.From.Year) && (actualTrajectory.From.Month == expectedTrajectory.From.Month), nameof(actualTrajectory.From) + " at iteration " + iteration + ".");
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.irrig_supl), actualTrajectory.irrig_supl, actualTrajectory.MonthCount, expectedTrajectory.irrig_supl, tolerances.IrrigationSupplied, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.prcp_runoff), actualTrajectory.prcp_runoff, actualTrajectory.MonthCount, expectedTrajectory.prcp_runoff, tolerances.PrecipitationRunoff, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.conduct_soil), actualTrajectory.conduct_soil, actualTrajectory.MonthCount, expectedTrajectory.conduct_soil, tolerances.SoilConductance, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.evapotra_soil), actualTrajectory.evapotra_soil, actualTrajectory.MonthCount, expectedTrajectory.evapotra_soil, tolerances.SoilEvaporation, tolerances.MaxTimestep, iteration);

            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.aero_resist), actualTrajectory.Species.aero_resist, actualTrajectory.MonthCount, expectedTrajectory.Species.aero_resist, tolerances.AeroResist, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.age), actualTrajectory.Species.age, actualTrajectory.MonthCount, expectedTrajectory.Species.age, tolerances.Age, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.age_m), actualTrajectory.Species.age_m, actualTrajectory.MonthCount, expectedTrajectory.Species.age_m, tolerances.AgeM, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.alpha_c), actualTrajectory.Species.alpha_c, actualTrajectory.MonthCount, expectedTrajectory.Species.alpha_c, tolerances.AlphaC, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.basal_area), actualTrajectory.Species.basal_area, actualTrajectory.MonthCount, expectedTrajectory.Species.basal_area, tolerances.BasalArea, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_foliage), actualTrajectory.Species.biom_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_foliage, tolerances.BiomassFoliage, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_foliage_debt), actualTrajectory.Species.biom_foliage_debt, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_foliage_debt, tolerances.BiomassFoliageDebt, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_root), actualTrajectory.Species.biom_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_root, tolerances.BiomassRoot, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_stem), actualTrajectory.Species.biom_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_stem, tolerances.BiomassStem, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.canopy_cover), actualTrajectory.Species.canopy_cover, actualTrajectory.MonthCount, expectedTrajectory.Species.canopy_cover, tolerances.CanopyCover, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.canopy_vol_frac), actualTrajectory.Species.canopy_vol_frac, actualTrajectory.MonthCount, expectedTrajectory.Species.canopy_vol_frac, tolerances.CanopyVolumeFraction, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.conduct_canopy), actualTrajectory.Species.conduct_canopy, actualTrajectory.MonthCount, expectedTrajectory.Species.conduct_canopy, tolerances.CanopyConductance, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.crown_width), actualTrajectory.Species.crown_width, actualTrajectory.MonthCount, expectedTrajectory.Species.crown_width, tolerances.CrownWidth, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.dbh), actualTrajectory.Species.dbh, actualTrajectory.MonthCount, expectedTrajectory.Species.dbh, tolerances.Dbh, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.epsilon_biom_stem), actualTrajectory.Species.epsilon_biom_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_biom_stem, tolerances.EpsilonStemBiomass, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.epsilon_gpp), actualTrajectory.Species.epsilon_gpp, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_gpp, tolerances.EpsilonGpp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.epsilon_npp), actualTrajectory.Species.epsilon_npp, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_npp, tolerances.EpsilonNpp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.fi), actualTrajectory.Species.fi, actualTrajectory.MonthCount, expectedTrajectory.Species.fi, tolerances.FractionApar, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.fracBB), actualTrajectory.Species.fracBB, actualTrajectory.MonthCount, expectedTrajectory.Species.fracBB, tolerances.FracBB, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_age), actualTrajectory.Species.f_age, actualTrajectory.MonthCount, expectedTrajectory.Species.f_age, tolerances.ModifierAge, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_calpha), actualTrajectory.Species.f_calpha, actualTrajectory.MonthCount, expectedTrajectory.Species.f_calpha, tolerances.ModiferCAlpha, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_cg), actualTrajectory.Species.f_cg, actualTrajectory.MonthCount, expectedTrajectory.Species.f_cg, tolerances.ModifierCG, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_frost), actualTrajectory.Species.f_frost, actualTrajectory.MonthCount, expectedTrajectory.Species.f_frost, tolerances.ModifierFrost, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_nutr), actualTrajectory.Species.f_nutr, actualTrajectory.MonthCount, expectedTrajectory.Species.f_nutr, tolerances.ModifierNutrition, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_phys), actualTrajectory.Species.f_phys, actualTrajectory.MonthCount, expectedTrajectory.Species.f_phys, tolerances.ModifierPhysiological, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_sw), actualTrajectory.Species.f_sw, actualTrajectory.MonthCount, expectedTrajectory.Species.f_sw, tolerances.ModifierSoilWater, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_tmp), actualTrajectory.Species.f_tmp, actualTrajectory.MonthCount, expectedTrajectory.Species.f_tmp, tolerances.ModifierTemperature, tolerances.MaxTimestep, iteration);
            if (threePG.Settings.phys_model == ThreePGModel.Mix)
            {
                // f_tmp_gc is hard coded to 1 in 3-PGpjs but the reference worksheets contain the values calculated prior to pjs forcing it to 1
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_tmp_gc), actualTrajectory.Species.f_tmp_gc, actualTrajectory.MonthCount, expectedTrajectory.Species.f_tmp_gc, tolerances.ModifierTemperatureGC, tolerances.MaxTimestep, iteration);
            }
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.f_vpd), actualTrajectory.Species.f_vpd, actualTrajectory.MonthCount, expectedTrajectory.Species.f_vpd, tolerances.ModifierVpd, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.gammaF), actualTrajectory.Species.gammaF, actualTrajectory.MonthCount, expectedTrajectory.Species.gammaF, tolerances.GammaF, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.gammaN), actualTrajectory.Species.gammaN, actualTrajectory.MonthCount, expectedTrajectory.Species.gammaN, tolerances.GammaN, tolerances.MaxTimestep, iteration);
            // not currently logged from C# Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.Gc_mol), actualTrajectory.Species.Gc_mol, actualTrajectory.MonthCount, expectedTrajectory.Species.Gc_mol, tolerances.GcMol, tolerances.MaxTimestep, iteration);
            // not currently logged from C# Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.Gw_mol), actualTrajectory.Species.Gw_mol, actualTrajectory.MonthCount, expectedTrajectory.Species.Gw_mol, tolerances.GwMol, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.GPP), actualTrajectory.Species.GPP, actualTrajectory.MonthCount, expectedTrajectory.Species.GPP, tolerances.Gpp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.height), actualTrajectory.Species.height, actualTrajectory.MonthCount, expectedTrajectory.Species.height, tolerances.Height, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.lai), actualTrajectory.Species.lai, actualTrajectory.MonthCount, expectedTrajectory.Species.lai, tolerances.Lai, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.lai_above), actualTrajectory.Species.lai_above, actualTrajectory.MonthCount, expectedTrajectory.Species.lai_above, tolerances.LaiAbove, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.lai_sa_ratio), actualTrajectory.Species.lai_sa_ratio, actualTrajectory.MonthCount, expectedTrajectory.Species.lai_sa_ratio, tolerances.LaiToSurfaceAreaRatio, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.lambda_h), actualTrajectory.Species.lambda_h, actualTrajectory.MonthCount, expectedTrajectory.Species.lambda_h, tolerances.LambdaH, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.lambda_v), actualTrajectory.Species.lambda_v, actualTrajectory.MonthCount, expectedTrajectory.Species.lambda_v, tolerances.LambdaV, tolerances.MaxTimestep, iteration);
            // TODO: handle Fortran using ones based layer numbering instead of zero based
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.layer_id), actualTrajectory.Species.layer_id, actualTrajectory.MonthCount, expectedTrajectory.Species.layer_id, tolerances.LayerID, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.NPP_f), actualTrajectory.Species.NPP_f, actualTrajectory.MonthCount, expectedTrajectory.Species.NPP_f, tolerances.NppF, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.prcp_interc), actualTrajectory.Species.prcp_interc, actualTrajectory.MonthCount, expectedTrajectory.Species.prcp_interc, tolerances.PrecipitationInterception, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.SLA), actualTrajectory.Species.SLA, actualTrajectory.MonthCount, expectedTrajectory.Species.SLA, tolerances.Sla, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.stems_n), actualTrajectory.Species.stems_n, actualTrajectory.MonthCount, expectedTrajectory.Species.stems_n, tolerances.StemsN, tolerances.MaxTimestep, iteration);
            // stems_n_ha is not in reference worksheets (but could potentially be calculated)
            // Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.stems_n_ha), actualTrajectory.Species.stems_n_ha, actualTrajectory.MonthCount, expectedTrajectory.Species.stems_n_ha, tolerances.stems_n_ha, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.transp_veg), actualTrajectory.Species.transp_veg, actualTrajectory.MonthCount, expectedTrajectory.Species.transp_veg, tolerances.TranspirationVegetation, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.volume), actualTrajectory.Species.volume, actualTrajectory.MonthCount, expectedTrajectory.Species.volume, tolerances.Volume, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.VPD_sp), actualTrajectory.Species.VPD_sp, actualTrajectory.MonthCount, expectedTrajectory.Species.VPD_sp, tolerances.VpdSp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.wood_density), actualTrajectory.Species.wood_density, actualTrajectory.MonthCount, expectedTrajectory.Species.wood_density, tolerances.WoodDensity, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.WUE), actualTrajectory.Species.WUE, actualTrajectory.MonthCount, expectedTrajectory.Species.WUE, tolerances.Wue, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.WUEtransp), actualTrajectory.Species.WUEtransp, actualTrajectory.MonthCount, expectedTrajectory.Species.WUEtransp, tolerances.WueTransp, tolerances.MaxTimestep, iteration);

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.CVdbhDistribution), actualTrajectory.Species.CVdbhDistribution, actualTrajectory.MonthCount, expectedTrajectory.Species.CVdbhDistribution, tolerances.CVdbhDistribution, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.CVwsDistribution), actualTrajectory.Species.CVwsDistribution, actualTrajectory.MonthCount, expectedTrajectory.Species.CVwsDistribution, tolerances.CVwsDistribution, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DWeibullScale), actualTrajectory.Species.DWeibullScale, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullScale, tolerances.DWeibullScale, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DWeibullShape), actualTrajectory.Species.DWeibullShape, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullShape, tolerances.DWeibullShape, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DWeibullLocation), actualTrajectory.Species.DWeibullLocation, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullLocation, tolerances.DWeibullLocation, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DrelBiaspFS), actualTrajectory.Species.DrelBiaspFS, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiaspFS, tolerances.DrelBiaspFS, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DrelBiasheight), actualTrajectory.Species.DrelBiasheight, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasheight, tolerances.DrelBiasheight, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DrelBiasCrowndiameter), actualTrajectory.Species.DrelBiasCrowndiameter, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasCrowndiameter, tolerances.DrelBiasCrowndiameter, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.DrelBiasLCL), actualTrajectory.Species.DrelBiasLCL, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasLCL, tolerances.DrelBiasLCL, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.height_rel), actualTrajectory.Species.height_rel, actualTrajectory.MonthCount, expectedTrajectory.Species.height_rel, tolerances.HeightRelative, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.wsrelBias), actualTrajectory.Species.wsrelBias, actualTrajectory.MonthCount, expectedTrajectory.Species.wsrelBias, tolerances.WSRelBias, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.wsWeibullScale), actualTrajectory.Species.wsWeibullScale, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullScale, tolerances.WSWeibullScale, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.wsWeibullShape), actualTrajectory.Species.wsWeibullShape, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullShape, tolerances.WSWeibullShape, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.wsWeibullLocation), actualTrajectory.Species.wsWeibullLocation, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullLocation, tolerances.WSWeibullLocation, tolerances.MaxTimestep, iteration);
            }

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
            {
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.D13CNewPS), actualTrajectory.Species.D13CNewPS, actualTrajectory.MonthCount, expectedTrajectory.Species.D13CNewPS, tolerances.D13CNewPS, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.D13CTissue), actualTrajectory.Species.D13CTissue, actualTrajectory.MonthCount, expectedTrajectory.Species.D13CTissue, tolerances.D13CTissue, tolerances.MaxTimestep, iteration);
                // TODO: handle Fortran multiplying InterCi by 1 million in i_write_out.h
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.InterCi), actualTrajectory.Species.InterCi, actualTrajectory.MonthCount, expectedTrajectory.Species.InterCi, tolerances.InterCi, tolerances.MaxTimestep, iteration);
            }

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_incr_foliage), actualTrajectory.Species.biom_incr_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_foliage, tolerances.BiomassIncrementFoliage, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_incr_root), actualTrajectory.Species.biom_incr_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_root, tolerances.BiomassIncrementRoot, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_incr_stem), actualTrajectory.Species.biom_incr_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_stem, tolerances.BiomassIncrementStem, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_loss_foliage), actualTrajectory.Species.biom_loss_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_loss_foliage, tolerances.BiomassLossFoliage, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.biom_loss_root), actualTrajectory.Species.biom_loss_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_loss_root, tolerances.BiomassLossRoot, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.volume_cum), actualTrajectory.Species.volume_cum, actualTrajectory.MonthCount, expectedTrajectory.Species.volume_cum, tolerances.VolumeCumulative, tolerances.MaxTimestep, iteration);
            }
        }

        private static void VerifyStandTrajectory(ThreePGSimd128 threePG128, ThreePGStandTrajectory<float, int> expectedTrajectory, StandTrajectoryTolerance tolerances, int iteration)
        {
            ThreePGStandTrajectory<Vector128<float>, Vector128<int>> actualTrajectory = threePG128.Trajectory;

            // verify array sizes and scalar properties
            // Reference trajectories in spreadsheet stop in January of end year rather than extend to February.
            // Also, January predictions are duplicated into February.
            Assert.IsTrue(actualTrajectory.MonthCount >= expectedTrajectory.MonthCount);
            Assert.IsTrue(actualTrajectory.Species.n_sp == expectedTrajectory.Species.n_sp);

            // verify species order matches
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.Species), actualTrajectory.Species.Species, expectedTrajectory.Species.Species, iteration);

            // verify stand trajectory
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.AvailableSoilWater), actualTrajectory.AvailableSoilWater, actualTrajectory.MonthCount, expectedTrajectory.AvailableSoilWater, tolerances.AvailableSoilWater, tolerances.MaxTimestep, iteration);
            // Test3PG.VerifyArray(threePG, nameof(actualTrajectory.DayLength), actualTrajectory.DayLength, actualTrajectory.MonthCount, expectedTrajectory.DayLength, tolerances.DayLength, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.evapo_transp), actualTrajectory.evapo_transp, actualTrajectory.MonthCount, expectedTrajectory.evapo_transp, tolerances.Evapotranspiration, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.f_transp_scale), actualTrajectory.f_transp_scale, actualTrajectory.MonthCount, expectedTrajectory.f_transp_scale, tolerances.TranspirationScale, tolerances.MaxTimestep, iteration);
            Assert.IsTrue((actualTrajectory.From.Year == expectedTrajectory.From.Year) && (actualTrajectory.From.Month == expectedTrajectory.From.Month), nameof(actualTrajectory.From) + " at iteration " + iteration + ".");
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.irrig_supl), actualTrajectory.irrig_supl, actualTrajectory.MonthCount, expectedTrajectory.irrig_supl, tolerances.IrrigationSupplied, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.prcp_runoff), actualTrajectory.prcp_runoff, actualTrajectory.MonthCount, expectedTrajectory.prcp_runoff, tolerances.PrecipitationRunoff, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.conduct_soil), actualTrajectory.conduct_soil, actualTrajectory.MonthCount, expectedTrajectory.conduct_soil, tolerances.SoilConductance, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.evapotra_soil), actualTrajectory.evapotra_soil, actualTrajectory.MonthCount, expectedTrajectory.evapotra_soil, tolerances.SoilEvaporation, tolerances.MaxTimestep, iteration);

            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.aero_resist), actualTrajectory.Species.aero_resist, actualTrajectory.MonthCount, expectedTrajectory.Species.aero_resist, tolerances.AeroResist, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.age), actualTrajectory.Species.age, actualTrajectory.MonthCount, expectedTrajectory.Species.age, tolerances.Age, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.age_m), actualTrajectory.Species.age_m, actualTrajectory.MonthCount, expectedTrajectory.Species.age_m, tolerances.AgeM, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.alpha_c), actualTrajectory.Species.alpha_c, actualTrajectory.MonthCount, expectedTrajectory.Species.alpha_c, tolerances.AlphaC, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.basal_area), actualTrajectory.Species.basal_area, actualTrajectory.MonthCount, expectedTrajectory.Species.basal_area, tolerances.BasalArea, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_foliage), actualTrajectory.Species.biom_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_foliage, tolerances.BiomassFoliage, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_foliage_debt), actualTrajectory.Species.biom_foliage_debt, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_foliage_debt, tolerances.BiomassFoliageDebt, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_root), actualTrajectory.Species.biom_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_root, tolerances.BiomassRoot, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_stem), actualTrajectory.Species.biom_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_stem, tolerances.BiomassStem, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.canopy_cover), actualTrajectory.Species.canopy_cover, actualTrajectory.MonthCount, expectedTrajectory.Species.canopy_cover, tolerances.CanopyCover, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.canopy_vol_frac), actualTrajectory.Species.canopy_vol_frac, actualTrajectory.MonthCount, expectedTrajectory.Species.canopy_vol_frac, tolerances.CanopyVolumeFraction, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.conduct_canopy), actualTrajectory.Species.conduct_canopy, actualTrajectory.MonthCount, expectedTrajectory.Species.conduct_canopy, tolerances.CanopyConductance, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.crown_width), actualTrajectory.Species.crown_width, actualTrajectory.MonthCount, expectedTrajectory.Species.crown_width, tolerances.CrownWidth, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.dbh), actualTrajectory.Species.dbh, actualTrajectory.MonthCount, expectedTrajectory.Species.dbh, tolerances.Dbh, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.epsilon_biom_stem), actualTrajectory.Species.epsilon_biom_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_biom_stem, tolerances.EpsilonStemBiomass, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.epsilon_gpp), actualTrajectory.Species.epsilon_gpp, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_gpp, tolerances.EpsilonGpp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.epsilon_npp), actualTrajectory.Species.epsilon_npp, actualTrajectory.MonthCount, expectedTrajectory.Species.epsilon_npp, tolerances.EpsilonNpp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.fi), actualTrajectory.Species.fi, actualTrajectory.MonthCount, expectedTrajectory.Species.fi, tolerances.FractionApar, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.fracBB), actualTrajectory.Species.fracBB, actualTrajectory.MonthCount, expectedTrajectory.Species.fracBB, tolerances.FracBB, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_age), actualTrajectory.Species.f_age, actualTrajectory.MonthCount, expectedTrajectory.Species.f_age, tolerances.ModifierAge, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_calpha), actualTrajectory.Species.f_calpha, actualTrajectory.MonthCount, expectedTrajectory.Species.f_calpha, tolerances.ModiferCAlpha, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_cg), actualTrajectory.Species.f_cg, actualTrajectory.MonthCount, expectedTrajectory.Species.f_cg, tolerances.ModifierCG, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_frost), actualTrajectory.Species.f_frost, actualTrajectory.MonthCount, expectedTrajectory.Species.f_frost, tolerances.ModifierFrost, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_nutr), actualTrajectory.Species.f_nutr, actualTrajectory.MonthCount, expectedTrajectory.Species.f_nutr, tolerances.ModifierNutrition, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_phys), actualTrajectory.Species.f_phys, actualTrajectory.MonthCount, expectedTrajectory.Species.f_phys, tolerances.ModifierPhysiological, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_sw), actualTrajectory.Species.f_sw, actualTrajectory.MonthCount, expectedTrajectory.Species.f_sw, tolerances.ModifierSoilWater, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_tmp), actualTrajectory.Species.f_tmp, actualTrajectory.MonthCount, expectedTrajectory.Species.f_tmp, tolerances.ModifierTemperature, tolerances.MaxTimestep, iteration);
            if (threePG128.Settings.phys_model == ThreePGModel.Mix)
            {
                // f_tmp_gc is hard coded to 1 in 3-PGpjs but the reference worksheets contain the values calculated prior to pjs forcing it to 1
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_tmp_gc), actualTrajectory.Species.f_tmp_gc, actualTrajectory.MonthCount, expectedTrajectory.Species.f_tmp_gc, tolerances.ModifierTemperatureGC, tolerances.MaxTimestep, iteration);
            }
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.f_vpd), actualTrajectory.Species.f_vpd, actualTrajectory.MonthCount, expectedTrajectory.Species.f_vpd, tolerances.ModifierVpd, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.gammaF), actualTrajectory.Species.gammaF, actualTrajectory.MonthCount, expectedTrajectory.Species.gammaF, tolerances.GammaF, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.gammaN), actualTrajectory.Species.gammaN, actualTrajectory.MonthCount, expectedTrajectory.Species.gammaN, tolerances.GammaN, tolerances.MaxTimestep, iteration);
            // not currently logged from C# Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.Gc_mol), actualTrajectory.Species.Gc_mol, actualTrajectory.MonthCount, expectedTrajectory.Species.Gc_mol, tolerances.GcMol, tolerances.MaxTimestep, iteration);
            // not currently logged from C# Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.Gw_mol), actualTrajectory.Species.Gw_mol, actualTrajectory.MonthCount, expectedTrajectory.Species.Gw_mol, tolerances.GwMol, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.GPP), actualTrajectory.Species.GPP, actualTrajectory.MonthCount, expectedTrajectory.Species.GPP, tolerances.Gpp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.height), actualTrajectory.Species.height, actualTrajectory.MonthCount, expectedTrajectory.Species.height, tolerances.Height, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.lai), actualTrajectory.Species.lai, actualTrajectory.MonthCount, expectedTrajectory.Species.lai, tolerances.Lai, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.lai_above), actualTrajectory.Species.lai_above, actualTrajectory.MonthCount, expectedTrajectory.Species.lai_above, tolerances.LaiAbove, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.lai_sa_ratio), actualTrajectory.Species.lai_sa_ratio, actualTrajectory.MonthCount, expectedTrajectory.Species.lai_sa_ratio, tolerances.LaiToSurfaceAreaRatio, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.lambda_h), actualTrajectory.Species.lambda_h, actualTrajectory.MonthCount, expectedTrajectory.Species.lambda_h, tolerances.LambdaH, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.lambda_v), actualTrajectory.Species.lambda_v, actualTrajectory.MonthCount, expectedTrajectory.Species.lambda_v, tolerances.LambdaV, tolerances.MaxTimestep, iteration);
            // TODO: handle Fortran using ones based layer numbering instead of zero based
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.layer_id), actualTrajectory.Species.layer_id, actualTrajectory.MonthCount, expectedTrajectory.Species.layer_id, tolerances.LayerID, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.NPP_f), actualTrajectory.Species.NPP_f, actualTrajectory.MonthCount, expectedTrajectory.Species.NPP_f, tolerances.NppF, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.prcp_interc), actualTrajectory.Species.prcp_interc, actualTrajectory.MonthCount, expectedTrajectory.Species.prcp_interc, tolerances.PrecipitationInterception, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.SLA), actualTrajectory.Species.SLA, actualTrajectory.MonthCount, expectedTrajectory.Species.SLA, tolerances.Sla, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.stems_n), actualTrajectory.Species.stems_n, actualTrajectory.MonthCount, expectedTrajectory.Species.stems_n, tolerances.StemsN, tolerances.MaxTimestep, iteration);
            // stems_n_ha is not in reference worksheets (but could potentially be calculated)
            // Test3PG.VerifyArray(threePG, nameof(actualTrajectory.Species.stems_n_ha), actualTrajectory.Species.stems_n_ha, actualTrajectory.MonthCount, expectedTrajectory.Species.stems_n_ha, tolerances.stems_n_ha, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.transp_veg), actualTrajectory.Species.transp_veg, actualTrajectory.MonthCount, expectedTrajectory.Species.transp_veg, tolerances.TranspirationVegetation, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.volume), actualTrajectory.Species.volume, actualTrajectory.MonthCount, expectedTrajectory.Species.volume, tolerances.Volume, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.VPD_sp), actualTrajectory.Species.VPD_sp, actualTrajectory.MonthCount, expectedTrajectory.Species.VPD_sp, tolerances.VpdSp, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.wood_density), actualTrajectory.Species.wood_density, actualTrajectory.MonthCount, expectedTrajectory.Species.wood_density, tolerances.WoodDensity, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.WUE), actualTrajectory.Species.WUE, actualTrajectory.MonthCount, expectedTrajectory.Species.WUE, tolerances.Wue, tolerances.MaxTimestep, iteration);
            Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.WUEtransp), actualTrajectory.Species.WUEtransp, actualTrajectory.MonthCount, expectedTrajectory.Species.WUEtransp, tolerances.WueTransp, tolerances.MaxTimestep, iteration);

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.CVdbhDistribution), actualTrajectory.Species.CVdbhDistribution, actualTrajectory.MonthCount, expectedTrajectory.Species.CVdbhDistribution, tolerances.CVdbhDistribution, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.CVwsDistribution), actualTrajectory.Species.CVwsDistribution, actualTrajectory.MonthCount, expectedTrajectory.Species.CVwsDistribution, tolerances.CVwsDistribution, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DWeibullScale), actualTrajectory.Species.DWeibullScale, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullScale, tolerances.DWeibullScale, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DWeibullShape), actualTrajectory.Species.DWeibullShape, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullShape, tolerances.DWeibullShape, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DWeibullLocation), actualTrajectory.Species.DWeibullLocation, actualTrajectory.MonthCount, expectedTrajectory.Species.DWeibullLocation, tolerances.DWeibullLocation, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DrelBiaspFS), actualTrajectory.Species.DrelBiaspFS, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiaspFS, tolerances.DrelBiaspFS, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DrelBiasheight), actualTrajectory.Species.DrelBiasheight, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasheight, tolerances.DrelBiasheight, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DrelBiasCrowndiameter), actualTrajectory.Species.DrelBiasCrowndiameter, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasCrowndiameter, tolerances.DrelBiasCrowndiameter, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.DrelBiasLCL), actualTrajectory.Species.DrelBiasLCL, actualTrajectory.MonthCount, expectedTrajectory.Species.DrelBiasLCL, tolerances.DrelBiasLCL, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.height_rel), actualTrajectory.Species.height_rel, actualTrajectory.MonthCount, expectedTrajectory.Species.height_rel, tolerances.HeightRelative, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.wsrelBias), actualTrajectory.Species.wsrelBias, actualTrajectory.MonthCount, expectedTrajectory.Species.wsrelBias, tolerances.WSRelBias, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.wsWeibullScale), actualTrajectory.Species.wsWeibullScale, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullScale, tolerances.WSWeibullScale, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.wsWeibullShape), actualTrajectory.Species.wsWeibullShape, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullShape, tolerances.WSWeibullShape, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.wsWeibullLocation), actualTrajectory.Species.wsWeibullLocation, actualTrajectory.MonthCount, expectedTrajectory.Species.wsWeibullLocation, tolerances.WSWeibullLocation, tolerances.MaxTimestep, iteration);
            }

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
            {
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.D13CNewPS), actualTrajectory.Species.D13CNewPS, actualTrajectory.MonthCount, expectedTrajectory.Species.D13CNewPS, tolerances.D13CNewPS, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.D13CTissue), actualTrajectory.Species.D13CTissue, actualTrajectory.MonthCount, expectedTrajectory.Species.D13CTissue, tolerances.D13CTissue, tolerances.MaxTimestep, iteration);
                // TODO: handle Fortran multiplying InterCi by 1 million in i_write_out.h
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.InterCi), actualTrajectory.Species.InterCi, actualTrajectory.MonthCount, expectedTrajectory.Species.InterCi, tolerances.InterCi, tolerances.MaxTimestep, iteration);
            }

            if (expectedTrajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_incr_foliage), actualTrajectory.Species.biom_incr_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_foliage, tolerances.BiomassIncrementFoliage, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_incr_root), actualTrajectory.Species.biom_incr_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_root, tolerances.BiomassIncrementRoot, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_incr_stem), actualTrajectory.Species.biom_incr_stem, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_incr_stem, tolerances.BiomassIncrementStem, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_loss_foliage), actualTrajectory.Species.biom_loss_foliage, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_loss_foliage, tolerances.BiomassLossFoliage, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.biom_loss_root), actualTrajectory.Species.biom_loss_root, actualTrajectory.MonthCount, expectedTrajectory.Species.biom_loss_root, tolerances.BiomassLossRoot, tolerances.MaxTimestep, iteration);
                Test3PG.VerifyArray(threePG128, nameof(actualTrajectory.Species.volume_cum), actualTrajectory.Species.volume_cum, actualTrajectory.MonthCount, expectedTrajectory.Species.volume_cum, tolerances.VolumeCumulative, tolerances.MaxTimestep, iteration);
            }
        }
    }
}