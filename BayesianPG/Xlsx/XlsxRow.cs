using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    public class XlsxRow
    {
        public string[] Row { get ; private init; }
        public int Index { get; set; }
        public int Rows { get; private init; }

        public XlsxRow(string worksheetDimension)
        {
            string[] range = worksheetDimension.Split(':');
            if ((range == null) || (range.Length != 2))
            {
                throw new XmlException(String.Format("Worksheet dimension reference '{0}' is malformed.", worksheetDimension));
            }

            this.Index = 0;
            this.Row = new string[XlsxReader.GetExcelColumnIndex(range[1]) + 1];
            this.Rows = XlsxReader.GetExcelRowIndex(range[1]) + 1;
        }

        public int Columns
        {
            get { return this.Row.Length; }
        }

        public int Number
        {
            // convert from zero-based C# indexing to one-based Excel row numbering for error messages
            get { return this.Index + 1; }
        }
    }
}
