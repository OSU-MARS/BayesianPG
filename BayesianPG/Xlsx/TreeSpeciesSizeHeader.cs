using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    public class TreeSpeciesSizeHeader : IXlsxWorksheetHeader
    {
        public int Species { get; private set; }
        public int Dscale0 { get; private set; }
        public int DscaleB  { get; private set; }
        public int Dscalerh { get; private set; }
        public int Dscalet { get; private set; }
        public int DscaleC { get; private set; }
        public int Dshape0 { get; private set; }
        public int DshapeB { get; private set; }
        public int Dshaperh { get; private set; }
        public int Dshapet { get; private set; }
        public int DshapeC { get; private set; }
        public int Dlocation0 { get; private set; }
        public int DlocationB { get; private set; }
        public int Dlocationrh { get; private set; }
        public int Dlocationt { get; private set; }
        public int DlocationC { get; private set; }
        public int wsscale0 { get; private set; }
        public int wsscaleB { get; private set; }
        public int wsscalerh { get; private set; }
        public int wsscalet { get; private set; }
        public int wsscaleC { get; private set; }
        public int wsshape0 { get; private set; }
        public int wsshapeB { get; private set; }
        public int wsshaperh { get; private set; }
        public int wsshapet { get; private set; }
        public int wsshapeC { get; private set; }
        public int wslocation0 { get; private set; }
        public int wslocationB { get; private set; }
        public int wslocationrh { get; private set; }
        public int wslocationt { get; private set; }
        public int wslocationC { get; private set; }

        public TreeSpeciesSizeHeader()
        {
            this.Species = -1;
            this.Dscale0 = -1;
            this.DscaleB = -1;
            this.Dscalerh = -1;
            this.Dscalet = -1;
            this.DscaleC = -1;
            this.Dshape0 = -1;
            this.DshapeB = -1;
            this.Dshaperh = -1;
            this.Dshapet = -1;
            this.DshapeC = -1;
            this.Dlocation0 = -1;
            this.DlocationB = -1;
            this.Dlocationrh = -1;
            this.Dlocationt = -1;
            this.DlocationC = -1;
            this.wsscale0 = -1;
            this.wsscaleB = -1;
            this.wsscalerh = -1;
            this.wsscalet = -1;
            this.wsscaleC = -1;
            this.wsshape0 = -1;
            this.wsshapeB = -1;
            this.wsshaperh = -1;
            this.wsshapet = -1;
            this.wsshapeC = -1;
            this.wslocation0 = -1;
            this.wslocationB = -1;
            this.wslocationrh = -1;
            this.wslocationt = -1;
            this.wslocationC = -1;
        }

        public void Parse(XlsxRow header)
        {
            for (int index = 0; index < header.Columns; ++index)
            {
                string column = header.Row[index];
                switch (column)
                {
                    case "species":
                        this.Species = index;
                        break;
                    case "Dscale0":
                        this.Dscale0 = index;
                        break;
                    case "DscaleB":
                        this.DscaleB = index;
                        break;
                    case "Dscalerh":
                        this.Dscalerh = index;
                        break;
                    case "Dscalet":
                        this.Dscalet = index;
                        break;
                    case "DscaleC":
                        this.DscaleC = index;
                        break;
                    case "Dshape0":
                        this.Dshape0 = index;
                        break;
                    case "DshapeB":
                        this.DshapeB = index;
                        break;
                    case "Dshaperh":
                        this.Dshaperh = index;
                        break;
                    case "Dshapet":
                        this.Dshapet = index;
                        break;
                    case "DshapeC":
                        this.DshapeC = index;
                        break;
                    case "Dlocation0":
                        this.Dlocation0 = index;
                        break;
                    case "DlocationB":
                        this.DlocationB = index;
                        break;
                    case "Dlocationrh":
                        this.Dlocationrh = index;
                        break;
                    case "Dlocationt":
                        this.Dlocationt = index;
                        break;
                    case "DlocationC":
                        this.DlocationC = index;
                        break;
                    case "wsscale0":
                        this.wsscale0 = index;
                        break;
                    case "wsscaleB":
                        this.wsscaleB = index;
                        break;
                    case "wsscalerh":
                        this.wsscalerh = index;
                        break;
                    case "wsscalet":
                        this.wsscalet = index;
                        break;
                    case "wsscaleC":
                        this.wsscaleC = index;
                        break;
                    case "wsshape0":
                        this.wsshape0 = index;
                        break;
                    case "wsshapeB":
                        this.wsshapeB = index;
                        break;
                    case "wsshaperh":
                        this.wsshaperh = index;
                        break;
                    case "wsshapet":
                        this.wsshapet = index;
                        break;
                    case "wsshapeC":
                        this.wsshapeC = index;
                        break;
                    case "wslocation0":
                        this.wslocation0 = index;
                        break;
                    case "wslocationB":
                        this.wslocationB = index;
                        break;
                    case "wslocationrh":
                        this.wslocationrh = index;
                        break;
                    case "wslocationt":
                        this.wslocationt = index;
                        break;
                    case "wslocationC":
                        this.wslocationC = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.Species < 0)
            {
                throw new XmlException("Species column not found in size distribution header.");
            }
            if (this.Dscale0 < 0)
            {
                throw new XmlException("Dscale0 column not found in size distribution header.");
            }
            if (this.DscaleB < 0)
            {
                throw new XmlException("DscaleB column not found in size distribution header.");
            }
            if (this.Dscalerh < 0)
            {
                throw new XmlException("Dscalerh column not found in size distribution header.");
            }
            if (this.Dscalet < 0)
            {
                throw new XmlException("Dscalet column not found in size distribution header.");
            }
            if (this.DscaleC < 0)
            {
                throw new XmlException("DscaleC column not found in size distribution header.");
            }
            if (this.Dshape0 < 0)
            {
                throw new XmlException("Dshape0 column not found in size distribution header.");
            }
            if (this.DshapeB < 0)
            {
                throw new XmlException("DshapeB column not found in size distribution header.");
            }
            if (this.Dshaperh < 0)
            {
                throw new XmlException("Dshaperh column not found in size distribution header.");
            }
            if (this.Dshapet < 0)
            {
                throw new XmlException("Dshapet column not found in size distribution header.");
            }
            if (this.DshapeC < 0)
            {
                throw new XmlException("DshapeC column not found in size distribution header.");
            }
            if (this.Dlocation0 < 0)
            {
                throw new XmlException("Dlocation0 column not found in size distribution header.");
            }
            if (this.DlocationB < 0)
            {
                throw new XmlException("DlocationB column not found in size distribution header.");
            }
            if (this.Dlocationrh < 0)
            {
                throw new XmlException("Dlocationrh column not found in size distribution header.");
            }
            if (this.Dlocationt < 0)
            {
                throw new XmlException("Dlocationt column not found in size distribution header.");
            }
            if (this.DlocationC < 0)
            {
                throw new XmlException("DlocationC column not found in size distribution header.");
            }
            if (this.wsscale0 < 0)
            {
                throw new XmlException("wsscale0 column not found in size distribution header.");
            }
            if (this.wsscaleB < 0)
            {
                throw new XmlException("wsscaleB column not found in size distribution header.");
            }
            if (this.wsscalerh < 0)
            {
                throw new XmlException("wsscalerh column not found in size distribution header.");
            }
            if (this.wsscalet < 0)
            {
                throw new XmlException("wsscalet column not found in size distribution header.");
            }
            if (this.wsscaleC < 0)
            {
                throw new XmlException("wsscaleC column not found in size distribution header.");
            }
            if (this.wsshape0 < 0)
            {
                throw new XmlException("wsshape0 column not found in size distribution header.");
            }
            if (this.wsshapeB < 0)
            {
                throw new XmlException("wsshapeB column not found in size distribution header.");
            }
            if (this.wsshaperh < 0)
            {
                throw new XmlException("wsshaperh column not found in size distribution header.");
            }
            if (this.wsshapet < 0)
            {
                throw new XmlException("wsshapet column not found in size distribution header.");
            }
            if (this.wsshaperh < 0)
            {
                throw new XmlException("wsshaperh column not found in size distribution header.");
            }
            if (this.wsshapeC < 0)
            {
                throw new XmlException("wsshapeC column not found in size distribution header.");
            }
            if (this.wslocation0 < 0)
            {
                throw new XmlException("wslocation0 column not found in size distribution header.");
            }
            if (this.wslocationB < 0)
            {
                throw new XmlException("wslocationB column not found in size distribution header.");
            }
            if (this.wslocationrh < 0)
            {
                throw new XmlException("wslocationrh column not found in size distribution header.");
            }
            if (this.wslocationt < 0)
            {
                throw new XmlException("wslocationt column not found in size distribution header.");
            }
            if (this.wslocationC < 0)
            {
                throw new XmlException("wslocationC column not found in size distribution header.");
            }
        }
    }
}
