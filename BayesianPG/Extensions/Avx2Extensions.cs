using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Extensions
{
    public static class Avx2Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> BroadcastScalarToVector128(float value)
        {
            Vector128<float> value128 = Vector128.CreateScalarUnsafe(value);
            return Avx2.BroadcastScalarToVector128(value128);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> BroadcastScalarToVector128(int value)
        {
            Vector128<int> value128 = Vector128.CreateScalarUnsafe(value);
            return Avx2.BroadcastScalarToVector128(value128);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> Set128(int value0, int value1, int value2, int value3)
        {
            // AVX version of Avx2.BroadcastScalarToVector128(&value);
            Vector128<int> value128_0 = Vector128.CreateScalarUnsafe(value0);
            Vector128<int> value128_1 = Avx.Shuffle(Vector128.CreateScalarUnsafe(value1), Constant.Simd128x4.Shuffle0to1);
            Vector128<int> value128_01 = Avx2.Blend(value128_0, value128_1, Constant.Simd128x4.Blend1);
            Vector128<int> value128_2 = Avx.Shuffle(Vector128.CreateScalarUnsafe(value2), Constant.Simd128x4.Shuffle0to2);
            Vector128<int> value128_3 = Avx.Shuffle(Vector128.CreateScalarUnsafe(value3), Constant.Simd128x4.Shuffle0to3);
            Vector128<int> value128_23 = Avx2.Blend(value128_2, value128_3, Constant.Simd128x4.Blend3);
            return Avx2.Blend(value128_01, value128_23, Constant.Simd128x4.Blend23);
        }
    }
}