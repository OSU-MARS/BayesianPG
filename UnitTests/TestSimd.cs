using BayesianPG.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Test
{
    [TestClass]
    public class TestSimd
    {
        [TestMethod]
        public void AvxFunctions()
        {
            // abs
            Vector128<float> values0 = Vector128.Create(0.0F, -1.0F, 1.0F, -0.0001F);
            Vector128<float> absoluteValues0 = AvxExtensions.Abs(values0);
            Assert.IsTrue(absoluteValues0.GetElement(0) == 0.0F);
            Assert.IsTrue(absoluteValues0.GetElement(1) == 1.0F);
            Assert.IsTrue(absoluteValues0.GetElement(2) == 1.0F);
            Assert.IsTrue(absoluteValues0.GetElement(3) == 0.0001F);

            Vector128<float> values1 = Vector128.Create(-889.787776605F, -3.74165E-14F, -Single.Epsilon, Single.NaN);
            Vector128<float> absoluteValues1 = AvxExtensions.Abs(values1);
            Assert.IsTrue(absoluteValues1.GetElement(0) == 889.787776605F);
            Assert.IsTrue(absoluteValues1.GetElement(1) == 3.74165E-14F);
            Assert.IsTrue(absoluteValues1.GetElement(2) == Single.Epsilon);
            Assert.IsTrue(Single.IsNaN(absoluteValues1.GetElement(3))); // NaN != NaN

            Vector128<float> values2 = Vector128.Create(Single.MinValue, Single.MaxValue, Single.NegativeInfinity, Single.PositiveInfinity);
            Vector128<float> absoluteValues2 = AvxExtensions.Abs(values2);
            Assert.IsTrue(absoluteValues2.GetElement(0) == Single.MaxValue);
            Assert.IsTrue(absoluteValues2.GetElement(1) == Single.MaxValue);
            Assert.IsTrue(absoluteValues2.GetElement(2) == Single.PositiveInfinity);
            Assert.IsTrue(absoluteValues2.GetElement(3) == Single.PositiveInfinity);
        }

        [TestMethod]
        public void Avx2Functions()
        {
            Vector128<int> values = AvxExtensions.Set128(0, 1, 2, 3);
            int value0 = Avx.Extract(values, Constant.Simd128x4.Extract0);
            int value1 = Avx.Extract(values, Constant.Simd128x4.Extract1);
            int value2 = Avx.Extract(values, Constant.Simd128x4.Extract2);
            int value3 = Avx.Extract(values, Constant.Simd128x4.Extract3);

            Assert.IsTrue(value0 == 0, "value0");
            Assert.IsTrue(value1 == 1, "value1");
            Assert.IsTrue(value2 == 2, "value2");
            Assert.IsTrue(value3 == 3, "value3");
        }
    }
}