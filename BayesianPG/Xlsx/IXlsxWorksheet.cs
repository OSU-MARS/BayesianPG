namespace BayesianPG.Xlsx
{
    public interface IXlsxWorksheet
    {
        void OnEndParsing();
        void ParseHeader(XlsxRow row);
        void ParseRow(XlsxRow row);
    }
}
