using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Test
{
    internal static class AssertV
    {
        public static void IsTrue(Vector128<float> condition, string message)
        {
            byte conditionMask = (byte)Avx.MoveMask(condition);
            Assert.IsTrue(conditionMask == Constant.Simd128x4.MaskAllTrue, message);
        }
    }
}
