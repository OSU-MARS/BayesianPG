namespace BayesianPG
{
    internal static class Constant
    {
        public const int DefaultTimestepCapacity = 10 * 12; // default to one decade
        public const float Ln2 = 0.693147181F;
        public const float Sqrt2Pi = 2.506628274631000F;

        // rate of change of saturated VP with T at 20C
        public const float e20 = 2.2F;
        public const float MaxSoilCond = 0.00250F;

        // latent heat of vapourisation of H₂O, J/kg
        public const float lambda = 2460000.0F;
        // density of air, kg/m³
        public const float rhoAir = 1.2F;
        // convert VPD to saturation deficit = 18/29/1000
        public const float VPDconv = 0.000622F;

        public static readonly int[] DayOfYear = { 15, 46, 74, 105, 135, 166, 196, 227, 258, 288, 319, 349 };

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

        public static class Simd128x4
        {
            public const byte Broadcast0toAll = 0x00; // 0 << 6  | 0 << 4 | 0 << 2 | 0 = 0x00
            public const byte Broadcast1toAll = 0x55; // 1 << 6  | 1 << 4 | 1 << 2 | 1 = 0101 0101 = 0x55
            public const byte Broadcast2toAll = 0xaa; // 2 << 6  | 2 << 4 | 2 << 2 | 2 = 1010 1010 = 0xaa
            public const byte Broadcast3toAll = 0xff; // 3 << 6  | 3 << 4 | 3 << 2 | 2 = 1111 1111 = 0xff
            public const byte Blend0 = 0x1;
            public const byte Blend1 = 0x2;
            public const byte Blend2 = 0x4;
            public const byte Blend23 = 0xc; // 0x4 | 0x8
            public const byte Blend3 = 0x8;
            public const byte Extract0 = 0;
            public const byte Extract1 = 1;
            public const byte Extract2 = 2;
            public const byte Extract3 = 3;
            public const byte MaskAllFalse = 0x0;
            public const byte MaskAllTrue = 0xf;
            public const byte Shuffle0to1 = 0xe0; // 3 << 6  | 2 << 4 | 0 << 1 | 0 = 1110 0000 = 0xe0
            public const byte Shuffle0to2 = 0xc4; // 3 << 6  | 0 << 4 | 1 << 2 | 0 = 1100 0100 = 0xc4
            public const byte Shuffle0to3 = 0x24; // 0 << 6  | 2 << 4 | 1 << 2 | 0 = 0010 0100 = 0x24
            public const int Width = 4;
        }
    }
}
