using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Extensions
{
    internal static class DebugV
    {
        [Conditional("DEBUG")]
        public static void Assert(Vector128<float> condition)
        {
            Debug.Assert(Avx.MoveMask(condition) == Constant.Simd128x4.MaskAllTrue);
        }

        [Conditional("DEBUG")]
        public static void Assert(Vector128<int> condition)
        {
            DebugV.Assert(condition.AsSingle());
        }

        [Conditional("DEBUG")]
        public static void Assert(Vector128<float> condition, string message)
        {
            Debug.Assert(Avx.MoveMask(condition) == Constant.Simd128x4.MaskAllTrue, message);
        }
    }
}
