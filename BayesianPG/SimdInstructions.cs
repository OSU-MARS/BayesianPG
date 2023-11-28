namespace BayesianPG
{
    public enum SimdInstructions
    {
        /// <summary>
        /// Use only scalar (non-SIMD) code paths.
        /// </summary>
        Scalar = 32,

        /// <summary>
        /// SSE, AVX, AVX2, or (not currently used) FMA VEX instructions at 128 bit width.
        /// </summary>
        Vex128 = 128
    }
}
