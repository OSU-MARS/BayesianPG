namespace BayesianPG
{
    internal static class Constant
    {
        public const int DefaultTimestepCapacity = 10 * 12; // default to one decade

        public const float ln2 = 0.693147181F;

        // rate of change of saturated VP with T at 20C
        public const float e20 = 2.2F;
        public const float MaxSoilCond = 0.00250F;

        // latent heat of vapourisation of H₂O, J/kg
        public const float lambda = 2460000.0F;
        // density of air, kg/m³
        public const float rhoAir = 1.2F;
        // convert VPD to saturation deficit = 18/29/1000
        public const float VPDconv = 0.000622F;

        public static readonly int[] dayOfYear = { 15, 46, 74, 105, 135, 166, 196, 227, 258, 288, 319, 349 };
        public static readonly int[] DaysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public static class OpenXml
        {
            public const string Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            public static class Attribute
            {
                public const string CellReference = "r";
                public const string CellType = "t";
                public const string Reference = "ref";
            }

            public static class CellType
            {
                public const string InlineString = "inlineStr";
                public const string SharedString = "s";
            }

            public static class Element
            {
                public const string Cell = "c";
                public const string CellValue = "v";
                public const string Dimension = "dimension";
                public const string InlineString = "is";
                public const string Row = "row";
                public const string SharedString = "si";
                public const string SheetData = "sheetData";
                public const string String = "t";
            }
        }
    }
}
