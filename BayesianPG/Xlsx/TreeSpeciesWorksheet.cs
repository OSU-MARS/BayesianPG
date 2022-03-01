using System;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal static class TreeSpeciesWorksheet
    {
        public static float Parse(string parameterName, XlsxRow row, int columnIndex, float minimumValue, float maximumValue)
        {
            float value = Single.Parse(row.Row[columnIndex], CultureInfo.InvariantCulture);
            if (value < minimumValue)
            {
                throw new XmlException("Value of " + value + " for " + parameterName + " is below the minimum value of " + minimumValue + ".", null, row.Number, columnIndex);
            }
            if (value > maximumValue)
            {
                throw new XmlException("Value of " + value + " for " + parameterName + " is above the maximum value of " + maximumValue + ".", null, row.Number, columnIndex);
            }

            return value;
        }

        public static bool Parse(string parameterName, XlsxRow row, float[] parameterValues, bool previouslyParsed, float minimumValue, float maximumValue)
        {
            if (previouslyParsed)
            {
                throw new XmlException("Repeated specification of " + parameterName + ".", null, row.Number, 1);
            }

            for (int destinationIndex = 0, sourceIndex = 1; sourceIndex < row.Columns; ++destinationIndex, ++sourceIndex)
            {
                float value = Single.Parse(row.Row[sourceIndex], CultureInfo.InvariantCulture);
                if (value < minimumValue)
                {
                    throw new XmlException("Value of " + value + " for " + parameterName + " is below the minimum value of " + minimumValue + ".", null, row.Number, sourceIndex);
                }
                if (value > maximumValue)
                {
                    throw new XmlException("Value of " + value + " for " + parameterName + " is above the maximum value of " + maximumValue + ".", null, row.Number, sourceIndex);
                }
                parameterValues[destinationIndex] = value;
            }

            return true;
        }

        public static void ValidateHeader(XlsxRow row)
        {
            if (row.Columns < 2)
            {
                throw new XmlException(nameof(row), null, 1, 2);
            }
            if (String.Equals(row.Row[0], "parameter", StringComparison.Ordinal) == false)
            {
                throw new XmlException("parameter", null, 1, 1);
            }
            for (int index = 1; index < row.Columns; ++index)
            {
                if (String.IsNullOrWhiteSpace(row.Row[index]))
                {
                    throw new XmlException("species", null, 1, index + 1);
                }
            }
        }
    }
}
