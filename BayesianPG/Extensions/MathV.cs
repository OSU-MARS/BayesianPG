using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Extensions
{
    internal static class MathV
    {
        private const float ExpC1 = 0.007972914726F;
        private const float ExpC2 = 0.1385283768F;
        private const float ExpC3 = 2.885390043F;
        private const float ExpToExp2 = 1.442695040888963F; // log2(e) for converting base 2 IEEE 754 exponent manipulation to base e

        private const float FloatExp2MaximumPower = 127.0F; // range limiting decompositions using 8 bit signed exponent
        private const int FloatExponentMask = 0x7F800000;
        private const int FloatMantissaBits = 23;
        private const int FloatMantissaZero = 127;
        private const int FloatMantissaMask = 0x007FFFFF;

        private const float Log2Beta1 = 1.441814292091611F;
        private const float Log2Beta2 = -0.708440969761796F;
        private const float Log2Beta3 = 0.414281442395441F;
        private const float Log2Beta4 = -0.192544768195605F;
        private const float Log2Beta5 = 0.044890234549254F;
        private const float Log2ToNaturalLog = 0.693147180559945F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Exp(Vector128<float> power)
        {
            return MathV.Exp2(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathV.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Exp2(Vector128<float> power)
        {
            Debug.Assert(Avx.MoveMask(Avx.And(Avx.CompareGreaterThan(power, AvxExtensions.BroadcastScalarToVector128(MathV.FloatExp2MaximumPower)), Avx.CompareOrdered(power, power))) == 0);

            Vector128<float> integerPowerAsFloat = Avx.RoundToZero(power);
            Vector128<float> fractionalPower = Avx.Subtract(power, integerPowerAsFloat);
            Vector128<float> fractionSquared = Avx.Multiply(fractionalPower, fractionalPower);

            Vector128<float> c1 = AvxExtensions.BroadcastScalarToVector128(MathV.ExpC1);
            Vector128<float> a = Avx.Add(fractionalPower, Avx.Multiply(c1, Avx.Multiply(fractionSquared, fractionalPower)));
            Vector128<float> c2 = AvxExtensions.BroadcastScalarToVector128(MathV.ExpC2);
            Vector128<float> c3 = AvxExtensions.BroadcastScalarToVector128(MathV.ExpC3);
            Vector128<float> b = Avx.Add(c3, Avx.Multiply(c2, fractionSquared));
            Vector128<float> fractionalInterpolant = Avx.Divide(Avx.Add(b, a), Avx.Subtract(b, a));

            Vector128<int> integerPower = Avx.ConvertToVector128Int32(integerPowerAsFloat); // res = 2^intPart
            Vector128<int> integerExponent = Avx.ShiftLeftLogical(integerPower, 23);
            Vector128<float> exponent = Avx.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            byte zeroMask = (byte)Avx.MoveMask(Avx.CompareLessThan(power, AvxExtensions.BroadcastScalarToVector128(-MathV.FloatExp2MaximumPower)));
            if (zeroMask != 0)
            {
                exponent = Avx.Blend(exponent, Vector128<float>.Zero, zeroMask);
            }
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Ln(Vector128<float> value)
        {
            return Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathV.Log2ToNaturalLog), MathV.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Log2(Vector128<float> value)
        {
            // split value into exponent and mantissa parts
            Vector128<int> integerValue = value.AsInt32();
            Vector128<float> exponent = Avx.ConvertToVector128Single(Avx.Subtract(Avx.ShiftRightLogical(Avx.And(integerValue, AvxExtensions.Set128(MathV.FloatExponentMask)),
                                                                                                        MathV.FloatMantissaBits),
                                                                                  AvxExtensions.Set128(MathV.FloatMantissaZero)));
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
            Vector128<float> mantissa = Avx.Or(Avx.And(integerValue, AvxExtensions.Set128(MathV.FloatMantissaMask)).AsSingle(), one);

            // evaluate mantissa polynomial
            Vector128<float> beta1 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta1);
            Vector128<float> x = Avx.Subtract(mantissa, one);
            Vector128<float> polynomial = Avx.Multiply(beta1, x);

            Vector128<float> beta2 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta2);
            Vector128<float> x2 = Avx.Multiply(x, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta2, x2));

            Vector128<float> beta3 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta3);
            Vector128<float> x3 = Avx.Multiply(x2, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta3, x3));

            Vector128<float> beta4 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta4);
            Vector128<float> x4 = Avx.Multiply(x3, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta4, x4));

            Vector128<float> beta5 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta5);
            Vector128<float> x5 = Avx.Multiply(x4, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta5, x5));

            // form logarithm
            return Avx.Add(exponent, polynomial);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Pow(Vector128<float> x, Vector128<float> y)
        {
            return MathV.Exp2(Avx.Multiply(MathV.Log2(x), y));
        }
    }
}
