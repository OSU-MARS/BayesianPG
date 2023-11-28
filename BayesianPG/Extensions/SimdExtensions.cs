using System.Globalization;
using System.Runtime.Intrinsics;

namespace BayesianPG.Extensions
{
    internal static class SimdExtensions
    {
        public static string ExtractToStringInvariant(this Vector128<float> value, int element, string precision)
        {
            return value.GetElement(element).ToString(precision, CultureInfo.InvariantCulture);
        }

        public static string ExtractToStringInvariant(this Vector128<int> value, int element)
        {
            return value.GetElement(element).ToString(CultureInfo.InvariantCulture);
        }
    }
}
