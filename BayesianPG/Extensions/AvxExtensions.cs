using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Extensions
{
    internal static class AvxExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Abs(Vector128<float> values)
        {
            Vector128<int> minus1 = AvxExtensions.Set128(-1);
            Vector128<int> signMask = Avx.ShiftRightLogical(minus1, 1);
            Vector128<int> absoluteValues = Avx.And(signMask, values.AsInt32());
            return absoluteValues.AsSingle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> BroadcastScalarToVector128(float value)
        {
            return Avx.BroadcastScalarToVector128(&value);
        }

        public static int HorizontalMax(Vector128<int> value)
        {
            Vector128<int> maxIn01 = Avx.Max(value, Avx.Shuffle(value, 0xee)); // max(0, 2) in 0, max(1, 3) in 1, upper 64 unchanged: (3 << 6) | (2 << 4) | (3 << 2) | 2 = 0xc0 | 0x20 | 0x0c | 0x02 = 11101110 = 0xee
            Vector128<int> maxIn0 = Avx.Max(maxIn01, Avx.Shuffle(maxIn01, 0xe5)); // max(0|2, 1|3) in 0, upper 96 unchanged: (3 << 6) | (2 << 4) | (1 << 2) | 1 = 0xc0 | 0x20 | 0x04 | 0x01 = 11100101 = 0xe5
            return Avx.Extract(maxIn0, Constant.Simd128x4.Extract0);
        }

        public static byte IsNaN(Vector128<float> value)
        {
            // compare as integers since Single.NaN ≠ Single.NaN and then cast back to float for masking
            Vector128<int> nanAsInt = AvxExtensions.BroadcastScalarToVector128(Single.NaN).AsInt32();
            byte isNaN = (byte)Avx.MoveMask(Avx.CompareEqual(value.AsInt32(), nanAsInt).AsSingle());
            return isNaN;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> Set128(int value)
        {
            // AVX version of Avx2.BroadcastScalarToVector128(&value);
            Vector128<int> value128 = Vector128.CreateScalarUnsafe(value);
            return Avx.Shuffle(value128, Constant.Simd128x4.Broadcast0toAll);
        }
    }
}