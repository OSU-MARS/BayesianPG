using BayesianPG.ThreePG;
using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class TreeSpeciesParameterWorksheet : TreeSpeciesWorksheet
    {
        // parsing tracking flags to check for duplicate and missing parameters
        // biomass partitioning and turnover
        private bool pFS2;
        private bool pFS20;
        private bool aWS;
        private bool nWS;
        private bool pRx;
        private bool pRn;
        private bool gammaF1;
        private bool gammaF0;
        private bool tgammaF;
        private bool gammaR;
        private bool leafgrow;
        private bool leaffall;

        // NPP & conductance modifiers
        private bool Tmin;
        private bool Topt;
        private bool Tmax;
        private bool kF;
        private bool SWconst0;
        private bool SWpower0;
        private bool fCalpha700;
        private bool fCg700;
        private bool m0;
        private bool fN0;
        private bool fNn;
        private bool MaxAge;
        private bool nAge;
        private bool rAge;

        private bool gammaN1;
        private bool gammaN0;
        private bool tgammaN;
        private bool ngammaN;
        private bool wSx1000;
        private bool thinPower;
        private bool mF;
        private bool mR;
        private bool mS;

        // canopy structure and processes
        private bool SLA0;
        private bool SLA1;
        private bool tSLA;
        private bool k;
        private bool fullCanAge;
        private bool MaxIntcptn;
        private bool LAImaxIntcptn;
        private bool cVPD;
        private bool alphaCx;
        private bool y;
        private bool MinCond;
        private bool MaxCond;
        private bool LAIgcx;
        private bool CoeffCond;
        private bool BLcond;
        private bool RGcGw;
        private bool D13CTissueDif;
        private bool aFracDiffu;
        private bool bFracRubi;

        // wood and stand properties
        private bool fracBB0;
        private bool fracBB1;
        private bool tBB;
        private bool rhoMin;
        private bool rhoMax;
        private bool tRho;
        private bool CrownShape;

        // height and volume
        private bool aH;
        private bool nHB;
        private bool nHC;
        private bool aV;
        private bool nVB;
        private bool nVH;
        private bool nVBH;
        private bool aK;
        private bool nKB;
        private bool nKH;
        private bool nKC;
        private bool nKrh;
        private bool aHL;
        private bool nHLB;
        private bool nHLL;
        private bool nHLC;
        private bool nHLrh;

        // δ¹³C
        private bool Qa;
        private bool Qb;
        private bool gDM_mol;
        private bool molPAR_MJ;

        public TreeSpeciesParameters Parameters { get; private init; }

        public TreeSpeciesParameterWorksheet()
        {
            // biomass partitioning and turnover
            this.pFS2 = false;
            this.pFS20 = false;
            this.aWS = false;
            this.nWS = false;
            this.pRx = false;
            this.pRn = false;
            this.gammaF1 = false;
            this.gammaF0 = false;
            this.tgammaF = false;
            this.gammaR = false;
            this.leafgrow = false;
            this.leaffall = false;

            // NPP & conductance modifiers
            this.Tmin = false;
            this.Topt = false;
            this.Tmax = false;
            this.kF = false;
            this.SWconst0 = false;
            this.SWpower0 = false;
            this.fCalpha700 = false;
            this.fCg700 = false;
            this.m0 = false;
            this.fN0 = false;
            this.fNn = false;
            this.MaxAge = false;
            this.nAge = false;
            this.rAge = false;

            this.gammaN1 = false;
            this.gammaN0 = false;
            this.tgammaN = false;
            this.ngammaN = false;
            this.wSx1000 = false;
            this.thinPower = false;
            this.mF = false;
            this.mR = false;
            this.mS = false;

            // canopy structure and processes
            this.SLA0 = false;
            this.SLA1 = false;
            this.tSLA = false;
            this.k = false;
            this.fullCanAge = false;
            this.MaxIntcptn = false;
            this.LAImaxIntcptn = false;
            this.cVPD = false;
            this.alphaCx = false;
            this.y = false;
            this.MinCond = false;
            this.MaxCond = false;
            this.LAIgcx = false;
            this.CoeffCond = false;
            this.BLcond = false;
            this.RGcGw = false;
            this.D13CTissueDif = false;
            this.aFracDiffu = false;
            this.bFracRubi = false;

            // wood and stand properties
            this.fracBB0 = false;
            this.fracBB1 = false;
            this.tBB = false;
            this.rhoMin = false;
            this.rhoMax = false;
            this.tRho = false;
            this.CrownShape = false;

            // height and volume
            this.aH = false;
            this.nHB = false;
            this.nHC = false;
            this.aV = false;
            this.nVB = false;
            this.nVH = false;
            this.nVBH = false;
            this.aK = false;
            this.nKB = false;
            this.nKH = false;
            this.nKC = false;
            this.nKrh = false;
            this.aHL = false;
            this.nHLB = false;
            this.nHLL = false;
            this.nHLC = false;
            this.nHLrh = false;

            // δ¹³C
            this.Qa = false;
            this.Qb = false;
            this.gDM_mol = false;
            this.molPAR_MJ = false;

            this.Parameters = new TreeSpeciesParameters();
        }

        public override void OnEndParsing()
        {
            // check all parameters specified
            if (this.pFS2 == false)
            {
                throw new XmlException("Row for " + nameof(this.pFS2) + " is missing.", null);
            }
            if (this.pFS20 == false)
            {
                throw new XmlException("Row for " + nameof(this.pFS20) + " is missing.", null);
            }
            if (this.aWS == false)
            {
                throw new XmlException("Row for " + nameof(this.aWS) + " is missing.", null);
            }
            if (this.nWS == false)
            {
                throw new XmlException("Row for " + nameof(this.nWS) + " is missing.", null);
            }
            if (this.pRx == false)
            {
                throw new XmlException("Row for " + nameof(this.pRx) + " is missing.", null);
            }
            if (this.pRn == false)
            {
                throw new XmlException("Row for " + nameof(this.pRn) + " is missing.", null);
            }
            if (this.gammaF1 == false)
            {
                throw new XmlException("Row for " + nameof(this.gammaF1) + " is missing.", null);
            }
            if (this.gammaF0 == false)
            {
                throw new XmlException("Row for " + nameof(this.gammaF0) + " is missing.", null);
            }
            if (this.tgammaF == false)
            {
                throw new XmlException("Row for " + nameof(this.tgammaF) + " is missing.", null);
            }
            if (this.gammaR == false)
            {
                throw new XmlException("Row for " + nameof(this.gammaR) + " is missing.", null);
            }
            if (this.leafgrow == false)
            {
                throw new XmlException("Row for " + nameof(this.leafgrow) + " is missing.", null);
            }
            if (this.leaffall == false)
            {
                throw new XmlException("Row for " + nameof(this.leaffall) + " is missing.", null);
            }
            
            // NPP & conductance modifiers
            if (this.Tmin == false)
            {
                throw new XmlException("Row for " + nameof(this.Tmin) + " is missing.", null);
            }
            if (this.Topt == false)
            {
                throw new XmlException("Row for " + nameof(this.Topt) + " is missing.", null);
            }
            if (this.Tmax == false)
            {
                throw new XmlException("Row for " + nameof(this.Tmax) + " is missing.", null);
            }
            if (this.kF == false)
            {
                throw new XmlException("Row for " + nameof(this.kF) + " is missing.", null);
            }
            if (this.SWconst0 == false)
            {
                throw new XmlException("Row for " + nameof(this.SWconst0) + " is missing.", null);
            }
            if (this.SWpower0 == false)
            {
                throw new XmlException("Row for " + nameof(this.SWpower0) + " is missing.", null);
            }
            if (this.fCalpha700 == false)
            {
                throw new XmlException("Row for " + nameof(this.fCalpha700) + " is missing.", null);
            }
            if (this.fCg700 == false)
            {
                throw new XmlException("Row for " + nameof(this.fCg700) + " is missing.", null);
            }
            if (this.m0 == false)
            {
                throw new XmlException("Row for " + nameof(this.m0) + " is missing.", null);
            }
            if (this.fN0 == false)
            {
                throw new XmlException("Row for " + nameof(this.fN0) + " is missing.", null);
            }
            if (this.fNn == false)
            {
                throw new XmlException("Row for " + nameof(this.fNn) + " is missing.", null);
            }
            if (this.MaxAge == false)
            {
                throw new XmlException("Row for " + nameof(this.MaxAge) + " is missing.", null);
            }
            if (this.nAge == false)
            {
                throw new XmlException("Row for " + nameof(this.nAge) + " is missing.", null);
            }
            if (this.rAge == false)
            {
                throw new XmlException("Row for " + nameof(this.rAge) + " is missing.", null);
            }

            if (this.gammaN1 == false)
            {
                throw new XmlException("Row for " + nameof(this.gammaN1) + " is missing.", null);
            }
            if (this.gammaN0 == false)
            {
                throw new XmlException("Row for " + nameof(this.gammaN0) + " is missing.", null);
            }
            if (this.tgammaN == false)
            {
                throw new XmlException("Row for " + nameof(this.tgammaN) + " is missing.", null);
            }
            if (this.ngammaN == false)
            {
                throw new XmlException("Row for " + nameof(this.ngammaN) + " is missing.", null);
            }
            if (this.wSx1000 == false)
            {
                throw new XmlException("Row for " + nameof(this.wSx1000) + " is missing.", null);
            }
            if (this.thinPower == false)
            {
                throw new XmlException("Row for " + nameof(this.thinPower) + " is missing.", null);
            }
            if (this.mF == false)
            {
                throw new XmlException("Row for " + nameof(this.mF) + " is missing.", null);
            }
            if (this.mR == false)
            {
                throw new XmlException("Row for " + nameof(this.mR) + " is missing.", null);
            }
            if (this.mS == false)
            {
                throw new XmlException("Row for " + nameof(this.mS) + " is missing.", null);
            }
            
            // canopy structure and processes
            if (this.SLA0 == false)
            {
                throw new XmlException("Row for " + nameof(this.SLA0) + " is missing.", null);
            }
            if (this.SLA1 == false)
            {
                throw new XmlException("Row for " + nameof(this.SLA1) + " is missing.", null);
            }
            if (this.tSLA == false)
            {
                throw new XmlException("Row for " + nameof(this.tSLA) + " is missing.", null);
            }
            if (this.k == false)
            {
                throw new XmlException("Row for " + nameof(this.k) + " is missing.", null);
            }
            if (this.fullCanAge == false)
            {
                throw new XmlException("Row for " + nameof(this.fullCanAge) + " is missing.", null);
            }
            if (this.MaxIntcptn == false)
            {
                throw new XmlException("Row for " + nameof(this.MaxIntcptn) + " is missing.", null);
            }
            if (this.LAImaxIntcptn == false)
            {
                throw new XmlException("Row for " + nameof(this.LAImaxIntcptn) + " is missing.", null);
            }
            if (this.cVPD == false)
            {
                throw new XmlException("Row for " + nameof(this.cVPD) + " is missing.", null);
            }
            if (this.alphaCx == false)
            {
                throw new XmlException("Row for " + nameof(this.alphaCx) + " is missing.", null);
            }
            if (this.y == false)
            {
                throw new XmlException("Row for " + nameof(this.y) + " is missing.", null);
            }
            if (this.MinCond == false)
            {
                throw new XmlException("Row for " + nameof(this.MinCond) + " is missing.", null);
            }
            if (this.MaxCond == false)
            {
                throw new XmlException("Row for " + nameof(this.MaxCond) + " is missing.", null);
            }
            if (this.LAIgcx == false)
            {
                throw new XmlException("Row for " + nameof(this.LAIgcx) + " is missing.", null);
            }
            if (this.CoeffCond == false)
            {
                throw new XmlException("Row for " + nameof(this.CoeffCond) + " is missing.", null);
            }
            if (this.BLcond == false)
            {
                throw new XmlException("Row for " + nameof(this.BLcond) + " is missing.", null);
            }
            if (this.RGcGw == false)
            {
                throw new XmlException("Row for " + nameof(this.RGcGw) + " is missing.", null);
            }
            if (this.D13CTissueDif == false)
            {
                throw new XmlException("Row for " + nameof(this.D13CTissueDif) + " is missing.", null);
            }
            if (this.aFracDiffu == false)
            {
                throw new XmlException("Row for " + nameof(this.aFracDiffu) + " is missing.", null);
            }
            if (this.bFracRubi == false)
            {
                throw new XmlException("Row for " + nameof(this.bFracRubi) + " is missing.", null);
            }
            
            // wood and stand properties
            if (this.fracBB0 == false)
            {
                throw new XmlException("Row for " + nameof(this.fracBB0) + " is missing.", null);
            }
            if (this.fracBB1 == false)
            {
                throw new XmlException("Row for " + nameof(this.fracBB1) + " is missing.", null);
            }
            if (this.tBB == false)
            {
                throw new XmlException("Row for " + nameof(this.tBB) + " is missing.", null);
            }
            if (this.rhoMin == false)
            {
                throw new XmlException("Row for " + nameof(this.rhoMin) + " is missing.", null);
            }
            if (this.rhoMax == false)
            {
                throw new XmlException("Row for " + nameof(this.rhoMax) + " is missing.", null);
            }
            if (this.tRho == false)
            {
                throw new XmlException("Row for " + nameof(this.tRho) + " is missing.", null);
            }
            if (this.CrownShape == false)
            {
                throw new XmlException("Row for " + nameof(this.CrownShape) + " is missing.", null);
            }
            
            // height and volume
            if (this.aH == false)
            {
                throw new XmlException("Row for " + nameof(this.aH) + " is missing.", null);
            }
            if (this.nHB == false)
            {
                throw new XmlException("Row for " + nameof(this.nHB) + " is missing.", null);
            }
            if (this.nHC == false)
            {
                throw new XmlException("Row for " + nameof(this.nHC) + " is missing.", null);
            }
            if (this.aV == false)
            {
                throw new XmlException("Row for " + nameof(this.aV) + " is missing.", null);
            }
            if (this.nVB == false)
            {
                throw new XmlException("Row for " + nameof(this.nVB) + " is missing.", null);
            }
            if (this.nVH == false)
            {
                throw new XmlException("Row for " + nameof(this.nVH) + " is missing.", null);
            }
            if (this.nVBH == false)
            {
                throw new XmlException("Row for " + nameof(this.nVBH) + " is missing.", null);
            }
            if (this.aK == false)
            {
                throw new XmlException("Row for " + nameof(this.aK) + " is missing.", null);
            }
            if (this.nKB == false)
            {
                throw new XmlException("Row for " + nameof(this.nKB) + " is missing.", null);
            }
            if (this.nKH == false)
            {
                throw new XmlException("Row for " + nameof(this.nKH) + " is missing.", null);
            }
            if (this.nKC == false)
            {
                throw new XmlException("Row for " + nameof(this.nKC) + " is missing.", null);
            }
            if (this.nKrh == false)
            {
                throw new XmlException("Row for " + nameof(this.nKrh) + " is missing.", null);
            }
            if (this.aHL == false)
            {
                throw new XmlException("Row for " + nameof(this.aHL) + " is missing.", null);
            }
            if (this.nHLB == false)
            {
                throw new XmlException("Row for " + nameof(this.nHLB) + " is missing.", null);
            }
            if (this.nHLL == false)
            {
                throw new XmlException("Row for " + nameof(this.nHLL) + " is missing.", null);
            }
            if (this.nHLC == false)
            {
                throw new XmlException("Row for " + nameof(this.nHLC) + " is missing.", null);
            }
            if (this.nHLrh == false)
            {
                throw new XmlException("Row for " + nameof(this.nHLrh) + " is missing.", null);
            }
            
            // δ¹³C
            if (this.Qa == false)
            {
                throw new XmlException("Row for " + nameof(this.Qa) + " is missing.", null);
            }
            if (this.Qb == false)
            {
                throw new XmlException("Row for " + nameof(this.Qb) + " is missing.", null);
            }
            if (this.gDM_mol == false)
            {
                throw new XmlException("Row for " + nameof(this.gDM_mol) + " is missing.", null);
            }
            if (this.molPAR_MJ == false)
            {
                throw new XmlException("Row for " + nameof(this.molPAR_MJ) + " is missing.", null);
            }

            // check parameter relations
            for (int speciesIndex = 0; speciesIndex < this.Parameters.n_sp; ++speciesIndex)
            {
                if (this.Parameters.Tmin[speciesIndex] > this.Parameters.Topt[speciesIndex])
                {
                    throw new XmlException("Minimum temperature for '" + this.Parameters.Name[speciesIndex] + "' is greater than its optimal temperature.");
                }
                if (this.Parameters.Topt[speciesIndex] > this.Parameters.Tmax[speciesIndex])
                {
                    throw new XmlException("Optimal temperature for '" + this.Parameters.Name[speciesIndex] + "' is greater than its maximum temperature.");
                }

                int leafgrow = this.Parameters.leafgrow[speciesIndex];
                int leaffall = this.Parameters.leaffall[speciesIndex];
                if ((leafgrow != 0) && (leafgrow == leaffall))
                {
                    throw new XmlException("Deciduous species '" + this.Parameters.Name[speciesIndex] + "' grows and loses its leaves in the same month.");
                }
            }
        }

        public override void ParseHeader(XlsxRow row)
        {
            TreeSpeciesWorksheet.ValidateHeader(row);
            this.Parameters.AllocateSpecies(row.Row[1..]);
        }

        public override void ParseRow(XlsxRow row)
        {
            string parameter = row.Row[0];
            if (row.Columns != this.Parameters.n_sp + 1)
            {
                throw new XmlException(parameter + " parameter values for " + (this.Parameters.n_sp - row.Columns + 1) + " species are missing.", null, row.Number, 2);
            }

            // for now, sanity range checking
            switch (parameter)
            {
                case "pFS2":
                    this.pFS2 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.pFS2, this.pFS2, 0.0F, 5.0F);
                    break;
                case "pFS20":
                    this.pFS20 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.pFS20, this.pFS20, 0.0F, 5.0F);
                    break;
                case "aWS":
                    this.aWS = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.aWS, this.aWS, 0.0F, 5.0F);
                    break;
                case "nWS":
                    this.nWS = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nWS, this.nWS, 0.0F, 5.0F);
                    break;
                case "pRx":
                    this.pRx = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.pRx, this.pRx, 0.0F, 5.0F);
                    break;
                case "pRn":
                    this.pRn = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.pRn, this.pRn, 0.0F, 5.0F);
                    break;
                case "gammaF1":
                    this.gammaF1 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.gammaF1, this.gammaF1, 0.0F, 5.0F);
                    break;
                case "gammaF0":
                    this.gammaF0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.gammaF0, this.gammaF0, 0.0F, 5.0F);
                    break;
                case "tgammaF":
                    this.tgammaF = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.tgammaF, this.tgammaF, 0.0F, 100.0F);
                    break;
                case "gammaR":
                    this.gammaR = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.gammaR, this.gammaR, 0.0F, 5.0F);
                    break;
                case "leafgrow":
                    this.leafgrow = TreeSpeciesParameterWorksheet.ParseRow(parameter, row, this.Parameters.leafgrow, this.leafgrow, 0, 12); // 0 indicates evergreen
                    break;
                case "leaffall":
                    this.leaffall = TreeSpeciesParameterWorksheet.ParseRow(parameter, row, this.Parameters.leaffall, this.leaffall, 0, 12); // 0 indicates evergreen
                    break;
                case "Tmin":
                    this.Tmin = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.Tmin, this.Tmin, -10.0F, 20.0F);
                    break;
                case "Topt":
                    this.Topt = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.Topt, this.Topt, 0.0F, 40.0F);
                    break;
                case "Tmax":
                    this.Tmax = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.Tmax, this.Tmax, 10.0F, 50.0F);
                    break;
                case "kF":
                    this.kF = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.kF, this.kF, 0.0F, 5.0F);
                    break;
                case "SWconst":
                    this.SWconst0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.SWconst0, this.SWconst0, 0.0F, 5.0F);
                    break;
                case "SWpower":
                    this.SWpower0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.SWpower0, this.SWpower0, 0.0F, 20.0F);
                    break;
                case "fCalpha700":
                    this.fCalpha700 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fCalpha700, this.fCalpha700, 0.0F, 5.0F);
                    break;
                case "fCg700":
                    this.fCg700 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fCg700, this.fCg700, 0.0F, 5.0F);
                    break;
                case "m0":
                    this.m0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.m0, this.m0, 0.0F, 5.0F);
                    break;
                case "fN0":
                    this.fN0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fN0, this.fN0, 0.0F, 5.0F);
                    break;
                case "fNn":
                    this.fNn = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fNn, this.fNn, 0.0F, 5.0F);
                    break;
                case "MaxAge":
                    this.MaxAge = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.MaxAge, this.MaxAge, 10.0F, 2500.0F);
                    break;
                case "nAge":
                    this.nAge = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nAge, this.nAge, 0.0F, 5.0F);
                    break;
                case "rAge":
                    this.rAge = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.rAge, this.rAge, 0.0F, 5.0F);
                    break;
                case "gammaN1":
                    this.gammaN1 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.gammaN1, this.gammaN1, 0.0F, 5.0F);
                    break;
                case "gammaN0":
                    this.gammaN0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.gammaN0, this.gammaN0, 0.0F, 5.0F);
                    break;
                case "tgammaN":
                    this.tgammaN = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.tgammaN, this.tgammaN, 0.0F, 5.0F);
                    break;
                case "ngammaN":
                    this.ngammaN = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.ngammaN, this.ngammaN, 0.0F, 5.0F);
                    break;
                case "wSx1000":
                    this.wSx1000 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.wSx1000, this.wSx1000, 100.0F, 1000.0F);
                    break;
                case "thinPower":
                    this.thinPower = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.thinPower, this.thinPower, 0.0F, 5.0F);
                    break;
                case "mF":
                    this.mF = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.mF, this.mF, 0.0F, 5.0F);
                    break;
                case "mR":
                    this.mR = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.mR, this.mR, 0.0F, 5.0F);
                    break;
                case "mS":
                    this.mS = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.mS, this.mS, 0.0F, 5.0F);
                    break;
                case "SLA0":
                    this.SLA0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.SLA0, this.SLA0, 0.0F, 50.0F);
                    break;
                case "SLA1":
                    this.SLA1 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.SLA1, this.SLA1, 0.0F, 50.0F);
                    break;
                case "tSLA":
                    this.tSLA = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.tSLA, this.tSLA, 0.0F, 50.0F);
                    break;
                case "k":
                    this.k = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.k, this.k, 0.0F, 5.0F);
                    break;
                case "fullCanAge":
                    this.fullCanAge = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fullCanAge, this.fullCanAge, 0.0F, 100.0F);
                    break;
                case "MaxIntcptn":
                    this.MaxIntcptn = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.MaxIntcptn, this.MaxIntcptn, 0.0F, 5.0F);
                    break;
                case "LAImaxIntcptn":
                    this.LAImaxIntcptn = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.LAImaxIntcptn, this.LAImaxIntcptn, 0.0F, 15.0F);
                    break;
                case "cVPD":
                    this.cVPD = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.cVPD, this.cVPD, 0.0F, 15.0F);
                    break;
                case "alphaCx":
                    this.alphaCx = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.alphaCx, this.alphaCx, 0.0F, 5.0F);
                    break;
                case "Y":
                    this.y = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.Y, this.y, 0.0F, 5.0F);
                    break;
                case "MinCond":
                    this.MinCond = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.MinCond, this.MinCond, 0.0F, 5.0F);
                    break;
                case "MaxCond":
                    this.MaxCond = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.MaxCond, this.MaxCond, 0.0F, 5.0F);
                    break;
                case "LAIgcx":
                    this.LAIgcx = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.LAIgcx, this.LAIgcx, 0.0F, 15.0F);
                    break;
                case "CoeffCond":
                    this.CoeffCond = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.CoeffCond, this.CoeffCond, 0.0F, 5.0F);
                    break;
                case "BLcond":
                    this.BLcond = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.BLcond, this.BLcond, 0.0F, 5.0F);
                    break;
                case "RGcGw":
                    this.RGcGw = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.RGcGw, this.RGcGw, 0.0F, 5.0F);
                    break;
                case "D13CTissueDif":
                    this.D13CTissueDif = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.D13CTissueDif, this.D13CTissueDif, 0.0F, 10.0F);
                    break;
                case "aFracDiffu":
                    this.aFracDiffu = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.aFracDiffu, this.aFracDiffu, 0.0F, 10.0F);
                    break;
                case "bFracRubi":
                    this.bFracRubi = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.bFracRubi, this.bFracRubi, 0.0F, 100.0F);
                    break;
                case "fracBB0":
                    this.fracBB0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fracBB0, this.fracBB0, 0.0F, 1.0F);
                    break;
                case "fracBB1":
                    this.fracBB1 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.fracBB1, this.fracBB1, 0.0F, 1.0F);
                    break;
                case "tBB":
                    this.tBB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.tBB, this.tBB, 0.0F, 50.0F);
                    break;
                case "rhoMin":
                    this.rhoMin = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.rhoMin, this.rhoMin, 0.0F, 2.0F);
                    break;
                case "rhoMax":
                    this.rhoMax = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.rhoMax, this.rhoMax, 0.0F, 2.0F);
                    break;
                case "tRho":
                    this.tRho = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.tRho, this.tRho, 0.0F, 50.0F);
                    break;
                case "crownshape":
                    this.CrownShape = TreeSpeciesParameterWorksheet.ParseRow<TreeCrownShape>(parameter, row, this.Parameters.CrownShape, this.CrownShape);
                    break;
                case "aH":
                    this.aH = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.aH, this.aH, 0.0F, 5.0F);
                    break;
                case "nHB":
                    this.nHB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nHB, this.nHB, 0.0F, 5.0F);
                    break;
                case "aV":
                    this.aV = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.aV, this.aV, 0.0F, 5.0F);
                    break;
                case "nHC":
                    this.nHC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nHC, this.nHC, 0.0F, 5.0F);
                    break;
                case "nVB":
                    this.nVB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nVB, this.nVB, 0.0F, 5.0F);
                    break;
                case "nVH":
                    this.nVH = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nVH, this.nVH, 0.0F, 5.0F);
                    break;
                case "nVBH":
                    this.nVBH = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nVBH, this.nVBH, 0.0F, 5.0F);
                    break;
                case "aK":
                    this.aK = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.aK, this.aK, 0.0F, 5.0F);
                    break;
                case "nKB":
                    this.nKB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nKB, this.nKB, 0.0F, 5.0F);
                    break;
                case "nKH":
                    this.nKH = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nKH, this.nKH, 0.0F, 5.0F);
                    break;
                case "nKC":
                    this.nKC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nKC, this.nKC, -1.0F, 0.0F);
                    break;
                case "nKrh":
                    this.nKrh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nKrh, this.nKrh, 0.0F, 5.0F);
                    break;
                case "aHL":
                    this.aHL = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.aHL, this.aHL, 0.0F, 15.0F);
                    break;
                case "nHLB":
                    this.nHLB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nHLB, this.nHLB, 0.0F, 5.0F);
                    break;
                case "nHLL":
                    this.nHLL = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nHLL, this.nHLL, 0.0F, 5.0F);
                    break;
                case "nHLC":
                    this.nHLC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nHLC, this.nHLC, -1.0F, 0.0F);
                    break;
                case "nHLrh":
                    this.nHLrh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.nHLrh, this.nHLrh, 0.0F, 5.0F);
                    break;
                case "Qa":
                    this.Qa = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.Qa, this.Qa, -200.0F, 0.0F);
                    break;
                case "Qb":
                    this.Qb = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.Qb, this.Qb, 0.0F, 5.0F);
                    break;
                case "gDM_mol":
                    this.gDM_mol = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.gDM_mol, this.gDM_mol, 0.0F, 50.0F);
                    break;
                case "molPAR_MJ":
                    this.molPAR_MJ = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Parameters.molPAR_MJ, this.molPAR_MJ, 0.0F, 5.0F);
                    break;
                default:
                    throw new NotSupportedException("Unhandled parameter name " + parameter + ".");
            }
        }

        private static bool ParseRow(string parameterName, XlsxRow row, int[] parameterValues, bool previouslyParsed, int minimumValue, int maximumValue)
        {
            if (previouslyParsed)
            {
                throw new XmlException("Repeated specification of " + parameterName + ".", null, row.Number, 1);
            }

            for (int destinationIndex = 0, sourceIndex = 1; sourceIndex < row.Columns; ++destinationIndex, ++sourceIndex)
            {
                int value = Int32.Parse(row.Row[sourceIndex]);
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

        private static bool ParseRow<T>(string parameterName, XlsxRow row, T[] parameterValues, bool previouslyParsed)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            if (previouslyParsed)
            {
                throw new XmlException("Repeated specification of " + parameterName + ".", null, row.Number, 1);
            }

            for (int destinationIndex = 0, sourceIndex = 1; sourceIndex < row.Columns; ++destinationIndex, ++sourceIndex)
            {
                parameterValues[destinationIndex] = Enum.Parse<T>(row.Row[sourceIndex]);
            }

            return true;
        }
    }
}
