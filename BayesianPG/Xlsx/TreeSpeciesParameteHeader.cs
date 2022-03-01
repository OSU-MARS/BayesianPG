using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    public class TreeSpeciesParameteHeader : IXlsxWorksheetHeader
    {
        public int Species { get; private set; }
        public int pFS2 { get; private set; }
        public int pFS20 { get; private set; }
        public int aWS { get; private set; }
        public int nWS { get; private set; }
        public int pRx { get; private set; }
        public int pRn { get; private set; }
        public int gammaF1 { get; private set; }
        public int gammaF0 { get; private set; }
        public int tgammaF { get; private set; }
        public int gammaR { get; private set; }
        public int leafgrow { get; private set; }
        public int leaffall { get; private set; }
        public int Tmin { get; private set; }
        public int Topt { get; private set; }
        public int Tmax { get; private set; }
        public int kF { get; private set; }
        public int SWconst { get; private set; }
        public int SWpower { get; private set; }
        public int fCalpha700 { get; private set; }
        public int fCg700 { get; private set; }
        public int m0 { get; private set; }
        public int fN0 { get; private set; }
        public int fNn { get; private set; }
        public int MaxAge { get; private set; }
        public int nAge { get; private set; }
        public int rAge { get; private set; }
        public int gammaN1 { get; private set; }
        public int gammaN0 { get; private set; }
        public int tgammaN { get; private set; }
        public int ngammaN { get; private set; }
        public int wSx1000 { get; private set; }
        public int thinPower { get; private set; }
        public int mF { get; private set; }
        public int mR { get; private set; }
        public int mS { get; private set; }
        public int SLA0 { get; private set; }
        public int SLA1 { get; private set; }
        public int tSLA { get; private set; }
        public int k { get; private set; }
        public int fullCanAge { get; private set; }
        public int MaxIntcptn { get; private set; }
        public int LAImaxIntcptn { get; private set; }
        public int cVPD { get; private set; }
        public int alphaCx { get; private set; }
        public int Y { get; private set; }
        public int MinCond { get; private set; }
        public int MaxCond { get; private set; }
        public int LAIgcx { get; private set; }
        public int CoeffCond { get; private set; }
        public int BLcond { get; private set; }
        public int RGcGw { get; private set; }
        public int D13CTissueDif { get; private set; }
        public int aFracDiffu { get; private set; }
        public int bFracRubi { get; private set; }
        public int fracBB0 { get; private set; }
        public int fracBB1 { get; private set; }
        public int tBB { get; private set; }
        public int rhoMin { get; private set; }
        public int rhoMax { get; private set; }
        public int tRho { get; private set; }
        public int crownshape { get; private set; }
        public int aH { get; private set; }
        public int nHB { get; private set; }
        public int nHC { get; private set; }
        public int aV { get; private set; }
        public int nVB { get; private set; }
        public int nVH { get; private set; }
        public int nVBH { get; private set; }
        public int aK { get; private set; }
        public int nKB { get; private set; }
        public int nKH { get; private set; }
        public int nKC { get; private set; }
        public int nKrh { get; private set; }
        public int aHL { get; private set; }
        public int nHLB { get; private set; }
        public int nHLL { get; private set; }
        public int nHLC { get; private set; }
        public int nHLrh { get; private set; }
        public int Qa { get; private set; }
        public int Qb { get; private set; }
        public int gDM_mol { get; private set; }
        public int molPAR_MJ { get; private set; }

        public TreeSpeciesParameteHeader()
        {
            this.Species = -1;
            this.pFS2 = -1;
            this.pFS20 = -1;
            this.aWS = -1;
            this.nWS = -1;
            this.pRx = -1;
            this.pRn = -1;
            this.gammaF1 = -1;
            this.gammaF0 = -1;
            this.tgammaF = -1;
            this.gammaR = -1;
            this.leafgrow = -1;
            this.leaffall = -1;
            this.Tmin = -1;
            this.Topt = -1;
            this.Tmax = -1;
            this.kF = -1;
            this.SWconst = -1;
            this.SWpower = -1;
            this.fCalpha700 = -1;
            this.fCg700 = -1;
            this.m0 = -1;
            this.fN0 = -1;
            this.fNn = -1;
            this.MaxAge = -1;
            this.nAge = -1;
            this.rAge = -1;
            this.gammaN1 = -1;
            this.gammaN0 = -1;
            this.tgammaN = -1;
            this.ngammaN = -1;
            this.wSx1000 = -1;
            this.thinPower = -1;
            this.mF = -1;
            this.mR = -1;
            this.mS = -1;
            this.SLA0 = -1;
            this.SLA1 = -1;
            this.tSLA = -1;
            this.k = -1;
            this.fullCanAge = -1;
            this.MaxIntcptn = -1;
            this.LAImaxIntcptn = -1;
            this.cVPD = -1;
            this.alphaCx = -1;
            this.Y = -1;
            this.MinCond = -1;
            this.MaxCond = -1;
            this.LAIgcx = -1;
            this.CoeffCond = -1;
            this.BLcond = -1;
            this.RGcGw = -1;
            this.D13CTissueDif = -1;
            this.aFracDiffu = -1;
            this.bFracRubi = -1;
            this.fracBB0 = -1;
            this.fracBB1 = -1;
            this.tBB = -1;
            this.rhoMin = -1;
            this.rhoMax = -1;
            this.tRho = -1;
            this.crownshape = -1;
            this.aH = -1;
            this.nHB = -1;
            this.nHC = -1;
            this.aV = -1;
            this.nVB = -1;
            this.nVH = -1;
            this.nVBH = -1;
            this.aK = -1;
            this.nKB = -1;
            this.nKH = -1;
            this.nKC = -1;
            this.nKrh = -1;
            this.aHL = -1;
            this.nHLB = -1;
            this.nHLL = -1;
            this.nHLC = -1;
            this.nHLrh = -1;
            this.Qa = -1;
            this.Qb = -1;
            this.gDM_mol = -1;
            this.molPAR_MJ = -1;
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
                    case "pFS2":
                        this.pFS2 = index;
                        break;
                    case "pFS20":
                        this.pFS20 = index;
                        break;
                    case "aWS":
                        this.aWS = index;
                        break;
                    case "nWS":
                        this.nWS = index;
                        break;
                    case "pRx":
                        this.pRx = index;
                        break;
                    case "pRn":
                        this.pRn = index;
                        break;
                    case "gammaF1":
                        this.gammaF1 = index;
                        break;
                    case "gammaF0":
                        this.gammaF0 = index;
                        break;
                    case "tgammaF":
                        this.tgammaF = index;
                        break;
                    case "gammaR":
                        this.gammaR = index;
                        break;
                    case "leafgrow":
                        this.leafgrow = index;
                        break;
                    case "leaffall":
                        this.leaffall = index;
                        break;
                    case "Tmin":
                        this.Tmin = index;
                        break;
                    case "Topt":
                        this.Topt = index;
                        break;
                    case "Tmax":
                        this.Tmax = index;
                        break;
                    case "kF":
                        this.kF = index;
                        break;
                    case "SWconst":
                        this.SWconst = index;
                        break;
                    case "SWpower":
                        this.SWpower = index;
                        break;
                    case "fCalpha700":
                        this.fCalpha700 = index;
                        break;
                    case "fCg700":
                        this.fCg700 = index;
                        break;
                    case "m0":
                        this.m0 = index;
                        break;
                    case "fN0":
                        this.fN0 = index;
                        break;
                    case "fNn":
                        this.fNn = index;
                        break;
                    case "MaxAge":
                        this.MaxAge = index;
                        break;
                    case "nAge":
                        this.nAge = index;
                        break;
                    case "rAge":
                        this.rAge = index;
                        break;
                    case "gammaN1":
                        this.gammaN1 = index;
                        break;
                    case "gammaN0":
                        this.gammaN0 = index;
                        break;
                    case "tgammaN":
                        this.tgammaN = index;
                        break;
                    case "ngammaN":
                        this.ngammaN = index;
                        break;
                    case "wSx1000":
                        this.wSx1000 = index;
                        break;
                    case "thinPower":
                        this.thinPower = index;
                        break;
                    case "mF":
                        this.mF = index;
                        break;
                    case "mR":
                        this.mR = index;
                        break;
                    case "mS":
                        this.mS = index;
                        break;
                    case "SLA0":
                        this.SLA0 = index;
                        break;
                    case "SLA1":
                        this.SLA1 = index;
                        break;
                    case "tSLA":
                        this.tSLA = index;
                        break;
                    case "k":
                        this.k = index;
                        break;
                    case "fullCanAge":
                        this.fullCanAge = index;
                        break;
                    case "MaxIntcptn":
                        this.MaxIntcptn = index;
                        break;
                    case "LAImaxIntcptn":
                        this.LAImaxIntcptn = index;
                        break;
                    case "cVPD":
                        this.cVPD = index;
                        break;
                    case "alphaCx":
                        this.alphaCx = index;
                        break;
                    case "Y":
                        this.Y = index;
                        break;
                    case "MinCond":
                        this.MinCond = index;
                        break;
                    case "MaxCond":
                        this.MaxCond = index;
                        break;
                    case "LAIgcx":
                        this.LAIgcx = index;
                        break;
                    case "CoeffCond":
                        this.CoeffCond = index;
                        break;
                    case "BLcond":
                        this.BLcond = index;
                        break;
                    case "RGcGw":
                        this.RGcGw = index;
                        break;
                    case "D13CTissueDif":
                        this.D13CTissueDif = index;
                        break;
                    case "aFracDiffu":
                        this.aFracDiffu = index;
                        break;
                    case "bFracRubi":
                        this.bFracRubi = index;
                        break;
                    case "fracBB0":
                        this.fracBB0 = index;
                        break;
                    case "fracBB1":
                        this.fracBB1 = index;
                        break;
                    case "tBB":
                        this.tBB = index;
                        break;
                    case "rhoMin":
                        this.rhoMin = index;
                        break;
                    case "rhoMax":
                        this.rhoMax = index;
                        break;
                    case "tRho":
                        this.tRho = index;
                        break;
                    case "crownshape":
                        this.crownshape = index;
                        break;
                    case "aH":
                        this.aH = index;
                        break;
                    case "nHB":
                        this.nHB = index;
                        break;
                    case "nHC":
                        this.nHC = index;
                        break;
                    case "aV":
                        this.aV = index;
                        break;
                    case "nVB":
                        this.nVB = index;
                        break;
                    case "nVH":
                        this.nVH = index;
                        break;
                    case "nVBH":
                        this.nVBH = index;
                        break;
                    case "aK":
                        this.aK = index;
                        break;
                    case "nKB":
                        this.nKB = index;
                        break;
                    case "nKH":
                        this.nKH = index;
                        break;
                    case "nKC":
                        this.nKC = index;
                        break;
                    case "nKrh":
                        this.nKrh = index;
                        break;
                    case "aHL":
                        this.aHL = index;
                        break;
                    case "nHLB":
                        this.nHLB = index;
                        break;
                    case "nHLL":
                        this.nHLL = index;
                        break;
                    case "nHLC":
                        this.nHLC = index;
                        break;
                    case "nHLrh":
                        this.nHLrh = index;
                        break;
                    case "Qa":
                        this.Qa = index;
                        break;
                    case "Qb":
                        this.Qb = index;
                        break;
                    case "gDM_mol":
                        this.gDM_mol = index;
                        break;
                    case "molPAR_MJ":
                        this.molPAR_MJ = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            if (this.Species < 0)
            {
                throw new XmlException("Species column not found in size distribution header.");
            }
            if (this.pFS2 < 0)
            {
                throw new XmlException("pFS2 column not found in species parameter header.");
            }
            if (this.pFS20 < 0)
            {
                throw new XmlException("pFS20 column not found in species parameter header.");
            }
            if (this.aWS < 0)
            {
                throw new XmlException("aWS column not found in species parameter header.");
            }
            if (this.nWS < 0)
            {
                throw new XmlException("nWS column not found in species parameter header.");
            }
            if (this.pRx < 0)
            {
                throw new XmlException("pRx column not found in species parameter header.");
            }
            if (this.pRn < 0)
            {
                throw new XmlException("pRn column not found in species parameter header.");
            }
            if (this.gammaF1 < 0)
            {
                throw new XmlException("gammaF1 column not found in species parameter header.");
            }
            if (this.gammaF0 < 0)
            {
                throw new XmlException("gammaF0 column not found in species parameter header.");
            }
            if (this.tgammaF < 0)
            {
                throw new XmlException("tgammaF column not found in species parameter header.");
            }
            if (this.gammaR < 0)
            {
                throw new XmlException("gammaR column not found in species parameter header.");
            }
            if (this.leafgrow < 0)
            {
                throw new XmlException("leafgrow column not found in species parameter header.");
            }
            if (this.leaffall < 0)
            {
                throw new XmlException("leaffall column not found in species parameter header.");
            }
            if (this.Tmin < 0)
            {
                throw new XmlException("Tmin column not found in species parameter header.");
            }
            if (this.Topt < 0)
            {
                throw new XmlException("Topt column not found in species parameter header.");
            }
            if (this.Tmax < 0)
            {
                throw new XmlException("Tmax column not found in species parameter header.");
            }
            if (this.kF < 0)
            {
                throw new XmlException("kF column not found in species parameter header.");
            }
            if (this.SWconst < 0)
            {
                throw new XmlException("SWconst column not found in species parameter header.");
            }
            if (this.SWpower < 0)
            {
                throw new XmlException("SWpower column not found in species parameter header.");
            }
            if (this.fCalpha700 < 0)
            {
                throw new XmlException("fCalpha700 column not found in species parameter header.");
            }
            if (this.fCg700 < 0)
            {
                throw new XmlException("fCg700 column not found in species parameter header.");
            }
            if (this.m0 < 0)
            {
                throw new XmlException("m0 column not found in species parameter header.");
            }
            if (this.fN0 < 0)
            {
                throw new XmlException("fN0 column not found in species parameter header.");
            }
            if (this.fNn < 0)
            {
                throw new XmlException("fNn column not found in species parameter header.");
            }
            if (this.MaxAge < 0)
            {
                throw new XmlException("MaxAge column not found in species parameter header.");
            }
            if (this.nAge < 0)
            {
                throw new XmlException("nAge column not found in species parameter header.");
            }
            if (this.rAge < 0)
            {
                throw new XmlException("rAge column not found in species parameter header.");
            }
            if (this.gammaN1 < 0)
            {
                throw new XmlException("gammaN1 column not found in species parameter header.");
            }
            if (this.gammaN0 < 0)
            {
                throw new XmlException("gammaN0 column not found in species parameter header.");
            }
            if (this.tgammaN < 0)
            {
                throw new XmlException("tgammaN column not found in species parameter header.");
            }
            if (this.ngammaN < 0)
            {
                throw new XmlException("ngammaN column not found in species parameter header.");
            }
            if (this.wSx1000 < 0)
            {
                throw new XmlException("wSx1000 column not found in species parameter header.");
            }
            if (this.thinPower < 0)
            {
                throw new XmlException("thinPower column not found in species parameter header.");
            }
            if (this.mF < 0)
            {
                throw new XmlException("mF column not found in species parameter header.");
            }
            if (this.mR < 0)
            {
                throw new XmlException("mR column not found in species parameter header.");
            }
            if (this.mS < 0)
            {
                throw new XmlException("mS column not found in species parameter header.");
            }
            if (this.SLA0 < 0)
            {
                throw new XmlException("SLA0 column not found in species parameter header.");
            }
            if (this.SLA1 < 0)
            {
                throw new XmlException("SLA1 column not found in species parameter header.");
            }
            if (this.tSLA < 0)
            {
                throw new XmlException("tSLA column not found in species parameter header.");
            }
            if (this.k < 0)
            {
                throw new XmlException("k column not found in species parameter header.");
            }
            if (this.fullCanAge < 0)
            {
                throw new XmlException("fullCanAge column not found in species parameter header.");
            }
            if (this.MaxIntcptn < 0)
            {
                throw new XmlException("MaxIntcptn column not found in species parameter header.");
            }
            if (this.LAImaxIntcptn < 0)
            {
                throw new XmlException("LAImaxIntcptn column not found in species parameter header.");
            }
            if (this.cVPD < 0)
            {
                throw new XmlException("cVPD column not found in species parameter header.");
            }
            if (this.alphaCx < 0)
            {
                throw new XmlException("alphaCx column not found in species parameter header.");
            }
            if (this.Y < 0)
            {
                throw new XmlException("Y column not found in species parameter header.");
            }
            if (this.MinCond < 0)
            {
                throw new XmlException("MinCond column not found in species parameter header.");
            }
            if (this.MaxCond < 0)
            {
                throw new XmlException("MaxCond column not found in species parameter header.");
            }
            if (this.LAIgcx < 0)
            {
                throw new XmlException("LAIgcx column not found in species parameter header.");
            }
            if (this.CoeffCond < 0)
            {
                throw new XmlException("CoeffCond column not found in species parameter header.");
            }
            if (this.BLcond < 0)
            {
                throw new XmlException("BLcond column not found in species parameter header.");
            }
            if (this.RGcGw < 0)
            {
                throw new XmlException("RGcGw column not found in species parameter header.");
            }
            if (this.D13CTissueDif < 0)
            {
                throw new XmlException("D13CTissueDif column not found in species parameter header.");
            }
            if (this.aFracDiffu < 0)
            {
                throw new XmlException("aFracDiffu column not found in species parameter header.");
            }
            if (this.bFracRubi < 0)
            {
                throw new XmlException("bFracRubi column not found in species parameter header.");
            }
            if (this.fracBB0 < 0)
            {
                throw new XmlException("fracBB0 column not found in species parameter header.");
            }
            if (this.fracBB1 < 0)
            {
                throw new XmlException("fracBB1 column not found in species parameter header.");
            }
            if (this.bFracRubi < 0)
            {
                throw new XmlException("bFracRubi column not found in species parameter header.");
            }
            if (this.tBB < 0)
            {
                throw new XmlException("tBB column not found in species parameter header.");
            }
            if (this.rhoMin < 0)
            {
                throw new XmlException("rhoMin column not found in species parameter header.");
            }
            if (this.rhoMax < 0)
            {
                throw new XmlException("rhoMax column not found in species parameter header.");
            }
            if (this.tRho < 0)
            {
                throw new XmlException("tRho column not found in species parameter header.");
            }
            if (this.crownshape < 0)
            {
                throw new XmlException("crownshape column not found in species parameter header.");
            }
            if (this.aH < 0)
            {
                throw new XmlException("aH column not found in species parameter header.");
            }
            if (this.nHB < 0)
            {
                throw new XmlException("nHB column not found in species parameter header.");
            }
            if (this.nHC < 0)
            {
                throw new XmlException("nHC column not found in species parameter header.");
            }
            if (this.aV < 0)
            {
                throw new XmlException("aV column not found in species parameter header.");
            }
            if (this.nVB < 0)
            {
                throw new XmlException("nVB column not found in species parameter header.");
            }
            if (this.nVH < 0)
            {
                throw new XmlException("nVH column not found in species parameter header.");
            }
            if (this.nVBH < 0)
            {
                throw new XmlException("nVBH column not found in species parameter header.");
            }
            if (this.aK < 0)
            {
                throw new XmlException("aK column not found in species parameter header.");
            }
            if (this.nKB < 0)
            {
                throw new XmlException("nKB column not found in species parameter header.");
            }
            if (this.nKH < 0)
            {
                throw new XmlException("nKH column not found in species parameter header.");
            }
            if (this.nKC < 0)
            {
                throw new XmlException("nKC column not found in species parameter header.");
            }
            if (this.nKrh < 0)
            {
                throw new XmlException("nKrh column not found in species parameter header.");
            }
            if (this.aHL < 0)
            {
                throw new XmlException("aHL column not found in species parameter header.");
            }
            if (this.nKrh < 0)
            {
                throw new XmlException("nKrh column not found in species parameter header.");
            }
            if (this.nHLB < 0)
            {
                throw new XmlException("nHLB column not found in species parameter header.");
            }
            if (this.nHLL < 0)
            {
                throw new XmlException("nHLL column not found in species parameter header.");
            }
            if (this.nHLC < 0)
            {
                throw new XmlException("nHLC column not found in species parameter header.");
            }
            if (this.nHLrh < 0)
            {
                throw new XmlException("nHLrh column not found in species parameter header.");
            }
            if (this.Qa < 0)
            {
                throw new XmlException("Qa column not found in species parameter header.");
            }
            if (this.Qb < 0)
            {
                throw new XmlException("Qb column not found in species parameter header.");
            }
            if (this.gDM_mol < 0)
            {
                throw new XmlException("gDM_mol column not found in species parameter header.");
            }
            if (this.molPAR_MJ < 0)
            {
                throw new XmlException("molPAR_MJ column not found in species parameter header.");
            }
        }
    }
}
