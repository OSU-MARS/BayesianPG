namespace BayesianPG.Xlsx
{
    public abstract class XlsxWorksheet : IXlsxWorksheet
    {
        public virtual void OnEndParsing()
        {
            // default to no op
        }

        public virtual void OnStartParsing(int _1, int _2)
        {
            // default to no op
        }

        public abstract void ParseHeader(XlsxRow row);
        public abstract void ParseRow(XlsxRow row);
    }

    public abstract class XlsxWorksheet<THeader> : XlsxWorksheet
        where THeader : IXlsxWorksheetHeader, new()
    {
        protected THeader Header { get; private init; }

        public XlsxWorksheet()
        {
            this.Header = new();
        }

        public override void ParseHeader(XlsxRow row)
        {
            this.Header.Parse(row);
        }
    }
}
