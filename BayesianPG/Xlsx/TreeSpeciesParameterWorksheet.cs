using BayesianPG.ThreePG;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class TreeSpeciesParameterWorksheet : XlsxWorksheet<TreeSpeciesParameteHeader>
    {
        private WideformParameterPresence? wideformPresence;

        public TreeSpeciesParameters Parameters { get; private init; }

        public TreeSpeciesParameterWorksheet()
        {
            this.wideformPresence = null;

            this.Parameters = new TreeSpeciesParameters();
        }

        public override void OnEndParsing()
        {
            if (this.wideformPresence != null)
            {
                this.wideformPresence.OnEndParsing();
            }

            // check parameter relations
            for (int speciesIndex = 0; speciesIndex < this.Parameters.n_sp; ++speciesIndex)
            {
                if (this.Parameters.Tmin[speciesIndex] > this.Parameters.Topt[speciesIndex])
                {
                    throw new XmlException("Minimum temperature for '" + this.Parameters.Species[speciesIndex] + "' is greater than its optimal temperature.");
                }
                if (this.Parameters.Topt[speciesIndex] > this.Parameters.Tmax[speciesIndex])
                {
                    throw new XmlException("Optimal temperature for '" + this.Parameters.Species[speciesIndex] + "' is greater than its maximum temperature.");
                }

                int leafgrow = this.Parameters.leafgrow[speciesIndex];
                int leaffall = this.Parameters.leaffall[speciesIndex];
                if ((leafgrow != 0) && (leafgrow == leaffall))
                {
                    throw new XmlException("Deciduous species '" + this.Parameters.Species[speciesIndex] + "' grows and loses its leaves in the same month.");
                }
            }
        }

        public override void ParseHeader(XlsxRow row)
        {
            if (String.Equals(row.Row[0], "parameter", StringComparison.Ordinal))
            {
                TreeSpeciesWorksheet.ValidateHeader(row);
                this.Parameters.AllocateSpecies(row.Row[1..]);
                this.wideformPresence = new();
            }
            else
            {
                this.Header.Parse(row);
                this.Parameters.AllocateSpecies(row.Rows - 1);
            }
        }

        private static int Parse(string parameterName, XlsxRow row, int columnIndex, int minimumValue, int maximumValue)
        {
            int value = Int32.Parse(row.Row[columnIndex], CultureInfo.InvariantCulture);
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

        public override void ParseRow(XlsxRow row)
        {
            if (this.wideformPresence != null)
            {
                this.ParseRowWideform(row);
            }
            else
            {
                this.ParseRowLongform(row);
            }
        }

        private void ParseRowLongform(XlsxRow row)
        {
            int speciesIndex = row.Index - 1;
            string species = row.Row[this.Header.Species];
            if (String.IsNullOrWhiteSpace(species))
            {
                throw new XmlException();
            }

            this.Parameters.Species[speciesIndex] = species;
            this.Parameters.pFS2[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.pFS2), row, this.Header.pFS2, 0.0F, 5.0F);
            this.Parameters.pFS20[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.pFS20), row, this.Header.pFS20, 0.0F, 5.0F);
            this.Parameters.aWS[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.aWS), row, this.Header.aWS, 0.0F, 5.0F);
            this.Parameters.nWS[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nWS), row, this.Header.nWS, 0.0F, 5.0F);
            this.Parameters.pRx[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.pRx), row, this.Header.pRx, 0.0F, 5.0F);
            this.Parameters.pRn[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.pRn), row, this.Header.pRn, 0.0F, 5.0F);
            this.Parameters.gammaF1[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.gammaF1), row, this.Header.gammaF1, 0.0F, 5.0F);
            this.Parameters.gammaF0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.gammaF0), row, this.Header.gammaF0, 0.0F, 5.0F);
            this.Parameters.tgammaF[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.tgammaF), row, this.Header.tgammaF, 0.0F, 100.0F);
            this.Parameters.gammaR[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.gammaR), row, this.Header.gammaR, 0.0F, 5.0F);
            this.Parameters.leafgrow[speciesIndex] = TreeSpeciesParameterWorksheet.Parse(nameof(this.Parameters.leafgrow), row, this.Header.leafgrow, 0, 12); // 0 indicates evergreen
            this.Parameters.leaffall[speciesIndex] = TreeSpeciesParameterWorksheet.Parse(nameof(this.Parameters.leaffall), row, this.Header.leaffall, 0, 12); // 0 indicates evergreen
            this.Parameters.Tmin[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.Tmin), row, this.Header.Tmin, -10.0F, 20.0F);
            this.Parameters.Topt[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.Topt), row, this.Header.Topt, 0.0F, 40.0F);
            this.Parameters.Tmax[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.Tmax), row, this.Header.Tmax, 10.0F, 50.0F);
            this.Parameters.kF[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.kF), row, this.Header.kF, 0.0F, 5.0F);
            this.Parameters.SWconst0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.SWconst0), row, this.Header.SWconst, 0.0F, 5.0F);
            this.Parameters.SWpower0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.SWpower0), row, this.Header.SWpower, 0.0F, 20.0F);
            this.Parameters.fCalpha700[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fCalpha700), row, this.Header.fCalpha700, 0.0F, 5.0F);
            this.Parameters.fCg700[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fCg700), row, this.Header.fCg700, 0.0F, 5.0F);
            this.Parameters.m0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.m0), row, this.Header.m0, 0.0F, 5.0F);
            this.Parameters.fN0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fN0), row, this.Header.fN0, 0.0F, 5.0F);
            this.Parameters.fNn[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fNn), row, this.Header.fNn, 0.0F, 5.0F);
            this.Parameters.MaxAge[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.MaxAge), row, this.Header.MaxAge, 10.0F, 2500.0F);
            this.Parameters.nAge[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nAge), row, this.Header.nAge, 0.0F, 5.0F);
            this.Parameters.rAge[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.rAge), row, this.Header.rAge, 0.0F, 5.0F);
            this.Parameters.gammaN1[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.gammaN1), row, this.Header.gammaN1, 0.0F, 5.0F);
            this.Parameters.gammaN0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.gammaN0), row, this.Header.gammaN0, 0.0F, 5.0F);
            this.Parameters.tgammaN[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.tgammaN), row, this.Header.tgammaN, 0.0F, 5.0F);
            this.Parameters.ngammaN[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.ngammaN), row, this.Header.ngammaN, 0.0F, 5.0F);
            this.Parameters.wSx1000[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.wSx1000), row, this.Header.wSx1000, 100.0F, 1000.0F);
            this.Parameters.thinPower[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.thinPower), row, this.Header.thinPower, 0.0F, 5.0F);
            this.Parameters.mF[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.mF), row, this.Header.mF, 0.0F, 5.0F);
            this.Parameters.mR[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.mR), row, this.Header.mR, 0.0F, 5.0F);
            this.Parameters.mS[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.mS), row, this.Header.mS, 0.0F, 5.0F);
            this.Parameters.SLA0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.SLA0), row, this.Header.SLA0, 0.0F, 50.0F);
            this.Parameters.SLA1[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.SLA1), row, this.Header.SLA1, 0.0F, 50.0F);
            this.Parameters.tSLA[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.tSLA), row, this.Header.tSLA, 0.0F, 50.0F);
            this.Parameters.k[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.k), row, this.Header.k, 0.0F, 5.0F);
            this.Parameters.fullCanAge[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fullCanAge), row, this.Header.fullCanAge, 0.0F, 100.0F);
            this.Parameters.MaxIntcptn[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.MaxIntcptn), row, this.Header.MaxIntcptn, 0.0F, 5.0F);
            this.Parameters.LAImaxIntcptn[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.LAImaxIntcptn), row, this.Header.LAImaxIntcptn, 0.0F, 15.0F);
            this.Parameters.cVPD[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.cVPD), row, this.Header.cVPD, 0.0F, 15.0F);
            this.Parameters.alphaCx[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.alphaCx), row, this.Header.alphaCx, 0.0F, 5.0F);
            this.Parameters.Y[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.Y), row, this.Header.Y, 0.0F, 5.0F);
            this.Parameters.MinCond[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.MinCond), row, this.Header.MinCond, 0.0F, 5.0F);
            this.Parameters.MaxCond[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.MaxCond), row, this.Header.MaxCond, 0.0F, 5.0F);
            this.Parameters.LAIgcx[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.LAIgcx), row, this.Header.LAIgcx, 0.0F, 15.0F);
            this.Parameters.CoeffCond[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.CoeffCond), row, this.Header.CoeffCond, 0.0F, 5.0F);
            this.Parameters.BLcond[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.BLcond), row, this.Header.BLcond, 0.0F, 5.0F);
            this.Parameters.RGcGw[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.RGcGw), row, this.Header.RGcGw, 0.0F, 5.0F);
            this.Parameters.D13CTissueDif[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.D13CTissueDif), row, this.Header.D13CTissueDif, 0.0F, 10.0F);
            this.Parameters.aFracDiffu[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.aFracDiffu), row, this.Header.aFracDiffu, 0.0F, 10.0F);
            this.Parameters.bFracRubi[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.bFracRubi), row, this.Header.bFracRubi, 0.0F, 100.0F);
            this.Parameters.fracBB0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fracBB0), row, this.Header.fracBB0, 0.0F, 1.0F);
            this.Parameters.fracBB1[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.fracBB1), row, this.Header.fracBB1, 0.0F, 1.0F);
            this.Parameters.tBB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.tBB), row, this.Header.tBB, 0.0F, 50.0F);
            this.Parameters.rhoMin[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.rhoMin), row, this.Header.rhoMin, 0.0F, 2.0F);
            this.Parameters.rhoMax[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.rhoMax), row, this.Header.rhoMax, 0.0F, 2.0F);
            this.Parameters.tRho[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.tRho), row, this.Header.tRho, 0.0F, 50.0F);
            this.Parameters.CrownShape[speciesIndex] = Enum.Parse<TreeCrownShape>(row.Row[this.Header.crownshape]);
            this.Parameters.aH[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.aH), row, this.Header.aH, 0.0F, 5.0F);
            this.Parameters.nHB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nHB), row, this.Header.nHB, 0.0F, 5.0F);
            this.Parameters.aV[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.aV), row, this.Header.aV, 0.0F, 5.0F);
            this.Parameters.nHC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nHC), row, this.Header.nHC, 0.0F, 5.0F);
            this.Parameters.nVB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nVB), row, this.Header.nVB, 0.0F, 5.0F);
            this.Parameters.nVH[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nVH), row, this.Header.nVH, 0.0F, 5.0F);
            this.Parameters.nVBH[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nVBH), row, this.Header.nVBH, 0.0F, 5.0F);
            this.Parameters.aK[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.aK), row, this.Header.aK, 0.0F, 5.0F);
            this.Parameters.nKB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nKB), row, this.Header.nKB, 0.0F, 5.0F);
            this.Parameters.nKH[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nKH), row, this.Header.nKH, 0.0F, 5.0F);
            this.Parameters.nKC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nKC), row, this.Header.nKC, -1.0F, 0.0F);
            this.Parameters.nKrh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nKrh), row, this.Header.nKrh, 0.0F, 5.0F);
            this.Parameters.aHL[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.aHL), row, this.Header.aHL, 0.0F, 15.0F);
            this.Parameters.nHLB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nHLB), row, this.Header.nHLB, 0.0F, 5.0F);
            this.Parameters.nHLL[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nHLL), row, this.Header.nHLL, 0.0F, 5.0F);
            this.Parameters.nHLC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nHLC), row, this.Header.nHLC, -1.0F, 0.0F);
            this.Parameters.nHLrh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.nHLrh), row, this.Header.nHLrh, 0.0F, 5.0F);
            this.Parameters.Qa[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.Qa), row, this.Header.Qa, -200.0F, 0.0F);
            this.Parameters.Qb[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.Qb), row, this.Header.Qb, 0.0F, 5.0F);
            this.Parameters.gDM_mol[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.gDM_mol), row, this.Header.gDM_mol, 0.0F, 50.0F);
            this.Parameters.molPAR_MJ[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Parameters.molPAR_MJ), row, this.Header.molPAR_MJ, 0.0F, 5.0F);
        }

        private void ParseRowWideform(XlsxRow row)
        {
            Debug.Assert(this.wideformPresence != null);

            string parameter = row.Row[0];
            if (row.Columns != this.Parameters.n_sp + 1)
            {
                throw new XmlException(parameter + " parameter values for " + (this.Parameters.n_sp - row.Columns + 1) + " species are missing.", null, row.Number, 2);
            }

            // for now, sanity range checking
            switch (parameter)
            {
                case "pFS2":
                    this.wideformPresence.pFS2 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.pFS2, this.wideformPresence.pFS2, 0.0F, 5.0F);
                    break;
                case "pFS20":
                    this.wideformPresence.pFS20 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.pFS20, this.wideformPresence.pFS20, 0.0F, 5.0F);
                    break;
                case "aWS":
                    this.wideformPresence.aWS = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.aWS, this.wideformPresence.aWS, 0.0F, 5.0F);
                    break;
                case "nWS":
                    this.wideformPresence.nWS = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nWS, this.wideformPresence.nWS, 0.0F, 5.0F);
                    break;
                case "pRx":
                    this.wideformPresence.pRx = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.pRx, this.wideformPresence.pRx, 0.0F, 5.0F);
                    break;
                case "pRn":
                    this.wideformPresence.pRn = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.pRn, this.wideformPresence.pRn, 0.0F, 5.0F);
                    break;
                case "gammaF1":
                    this.wideformPresence.gammaF1 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.gammaF1, this.wideformPresence.gammaF1, 0.0F, 5.0F);
                    break;
                case "gammaF0":
                    this.wideformPresence.gammaF0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.gammaF0, this.wideformPresence.gammaF0, 0.0F, 5.0F);
                    break;
                case "tgammaF":
                    this.wideformPresence.tgammaF = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.tgammaF, this.wideformPresence.tgammaF, 0.0F, 100.0F);
                    break;
                case "gammaR":
                    this.wideformPresence.gammaR = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.gammaR, this.wideformPresence.gammaR, 0.0F, 5.0F);
                    break;
                case "leafgrow":
                    this.wideformPresence.leafgrow = TreeSpeciesParameterWorksheet.ParseRow(parameter, row, this.Parameters.leafgrow, this.wideformPresence.leafgrow, 0, 12); // 0 indicates evergreen
                    break;
                case "leaffall":
                    this.wideformPresence.leaffall = TreeSpeciesParameterWorksheet.ParseRow(parameter, row, this.Parameters.leaffall, this.wideformPresence.leaffall, 0, 12); // 0 indicates evergreen
                    break;
                case "Tmin":
                    this.wideformPresence.Tmin = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.Tmin, this.wideformPresence.Tmin, -10.0F, 20.0F);
                    break;
                case "Topt":
                    this.wideformPresence.Topt = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.Topt, this.wideformPresence.Topt, 0.0F, 40.0F);
                    break;
                case "Tmax":
                    this.wideformPresence.Tmax = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.Tmax, this.wideformPresence.Tmax, 10.0F, 50.0F);
                    break;
                case "kF":
                    this.wideformPresence.kF = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.kF, this.wideformPresence.kF, 0.0F, 5.0F);
                    break;
                case "SWconst":
                    this.wideformPresence.SWconst0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.SWconst0, this.wideformPresence.SWconst0, 0.0F, 5.0F);
                    break;
                case "SWpower":
                    this.wideformPresence.SWpower0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.SWpower0, this.wideformPresence.SWpower0, 0.0F, 20.0F);
                    break;
                case "fCalpha700":
                    this.wideformPresence.fCalpha700 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fCalpha700, this.wideformPresence.fCalpha700, 0.0F, 5.0F);
                    break;
                case "fCg700":
                    this.wideformPresence.fCg700 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fCg700, this.wideformPresence.fCg700, 0.0F, 5.0F);
                    break;
                case "m0":
                    this.wideformPresence.m0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.m0, this.wideformPresence.m0, 0.0F, 5.0F);
                    break;
                case "fN0":
                    this.wideformPresence.fN0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fN0, this.wideformPresence.fN0, 0.0F, 5.0F);
                    break;
                case "fNn":
                    this.wideformPresence.fNn = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fNn, this.wideformPresence.fNn, 0.0F, 5.0F);
                    break;
                case "MaxAge":
                    this.wideformPresence.MaxAge = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.MaxAge, this.wideformPresence.MaxAge, 10.0F, 2500.0F);
                    break;
                case "nAge":
                    this.wideformPresence.nAge = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nAge, this.wideformPresence.nAge, 0.0F, 5.0F);
                    break;
                case "rAge":
                    this.wideformPresence.rAge = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.rAge, this.wideformPresence.rAge, 0.0F, 5.0F);
                    break;
                case "gammaN1":
                    this.wideformPresence.gammaN1 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.gammaN1, this.wideformPresence.gammaN1, 0.0F, 5.0F);
                    break;
                case "gammaN0":
                    this.wideformPresence.gammaN0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.gammaN0, this.wideformPresence.gammaN0, 0.0F, 5.0F);
                    break;
                case "tgammaN":
                    this.wideformPresence.tgammaN = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.tgammaN, this.wideformPresence.tgammaN, 0.0F, 5.0F);
                    break;
                case "ngammaN":
                    this.wideformPresence.ngammaN = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.ngammaN, this.wideformPresence.ngammaN, 0.0F, 5.0F);
                    break;
                case "wSx1000":
                    this.wideformPresence.wSx1000 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.wSx1000, this.wideformPresence.wSx1000, 100.0F, 1000.0F);
                    break;
                case "thinPower":
                    this.wideformPresence.thinPower = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.thinPower, this.wideformPresence.thinPower, 0.0F, 5.0F);
                    break;
                case "mF":
                    this.wideformPresence.mF = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.mF, this.wideformPresence.mF, 0.0F, 5.0F);
                    break;
                case "mR":
                    this.wideformPresence.mR = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.mR, this.wideformPresence.mR, 0.0F, 5.0F);
                    break;
                case "mS":
                    this.wideformPresence.mS = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.mS, this.wideformPresence.mS, 0.0F, 5.0F);
                    break;
                case "SLA0":
                    this.wideformPresence.SLA0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.SLA0, this.wideformPresence.SLA0, 0.0F, 50.0F);
                    break;
                case "SLA1":
                    this.wideformPresence.SLA1 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.SLA1, this.wideformPresence.SLA1, 0.0F, 50.0F);
                    break;
                case "tSLA":
                    this.wideformPresence.tSLA = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.tSLA, this.wideformPresence.tSLA, 0.0F, 50.0F);
                    break;
                case "k":
                    this.wideformPresence.k = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.k, this.wideformPresence.k, 0.0F, 5.0F);
                    break;
                case "fullCanAge":
                    this.wideformPresence.fullCanAge = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fullCanAge, this.wideformPresence.fullCanAge, 0.0F, 100.0F);
                    break;
                case "MaxIntcptn":
                    this.wideformPresence.MaxIntcptn = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.MaxIntcptn, this.wideformPresence.MaxIntcptn, 0.0F, 5.0F);
                    break;
                case "LAImaxIntcptn":
                    this.wideformPresence.LAImaxIntcptn = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.LAImaxIntcptn, this.wideformPresence.LAImaxIntcptn, 0.0F, 15.0F);
                    break;
                case "cVPD":
                    this.wideformPresence.cVPD = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.cVPD, this.wideformPresence.cVPD, 0.0F, 15.0F);
                    break;
                case "alphaCx":
                    this.wideformPresence.alphaCx = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.alphaCx, this.wideformPresence.alphaCx, 0.0F, 5.0F);
                    break;
                case "Y":
                    this.wideformPresence.y = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.Y, this.wideformPresence.y, 0.0F, 5.0F);
                    break;
                case "MinCond":
                    this.wideformPresence.MinCond = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.MinCond, this.wideformPresence.MinCond, 0.0F, 5.0F);
                    break;
                case "MaxCond":
                    this.wideformPresence.MaxCond = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.MaxCond, this.wideformPresence.MaxCond, 0.0F, 5.0F);
                    break;
                case "LAIgcx":
                    this.wideformPresence.LAIgcx = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.LAIgcx, this.wideformPresence.LAIgcx, 0.0F, 15.0F);
                    break;
                case "CoeffCond":
                    this.wideformPresence.CoeffCond = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.CoeffCond, this.wideformPresence.CoeffCond, 0.0F, 5.0F);
                    break;
                case "BLcond":
                    this.wideformPresence.BLcond = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.BLcond, this.wideformPresence.BLcond, 0.0F, 5.0F);
                    break;
                case "RGcGw":
                    this.wideformPresence.RGcGw = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.RGcGw, this.wideformPresence.RGcGw, 0.0F, 5.0F);
                    break;
                case "D13CTissueDif":
                    this.wideformPresence.D13CTissueDif = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.D13CTissueDif, this.wideformPresence.D13CTissueDif, 0.0F, 10.0F);
                    break;
                case "aFracDiffu":
                    this.wideformPresence.aFracDiffu = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.aFracDiffu, this.wideformPresence.aFracDiffu, 0.0F, 10.0F);
                    break;
                case "bFracRubi":
                    this.wideformPresence.bFracRubi = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.bFracRubi, this.wideformPresence.bFracRubi, 0.0F, 100.0F);
                    break;
                case "fracBB0":
                    this.wideformPresence.fracBB0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fracBB0, this.wideformPresence.fracBB0, 0.0F, 1.0F);
                    break;
                case "fracBB1":
                    this.wideformPresence.fracBB1 = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.fracBB1, this.wideformPresence.fracBB1, 0.0F, 1.0F);
                    break;
                case "tBB":
                    this.wideformPresence.tBB = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.tBB, this.wideformPresence.tBB, 0.0F, 50.0F);
                    break;
                case "rhoMin":
                    this.wideformPresence.rhoMin = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.rhoMin, this.wideformPresence.rhoMin, 0.0F, 2.0F);
                    break;
                case "rhoMax":
                    this.wideformPresence.rhoMax = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.rhoMax, this.wideformPresence.rhoMax, 0.0F, 2.0F);
                    break;
                case "tRho":
                    this.wideformPresence.tRho = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.tRho, this.wideformPresence.tRho, 0.0F, 50.0F);
                    break;
                case "crownshape":
                    this.wideformPresence.CrownShape = TreeSpeciesParameterWorksheet.ParseRow<TreeCrownShape>(parameter, row, this.Parameters.CrownShape, this.wideformPresence.CrownShape);
                    break;
                case "aH":
                    this.wideformPresence.aH = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.aH, this.wideformPresence.aH, 0.0F, 5.0F);
                    break;
                case "nHB":
                    this.wideformPresence.nHB = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nHB, this.wideformPresence.nHB, 0.0F, 5.0F);
                    break;
                case "aV":
                    this.wideformPresence.aV = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.aV, this.wideformPresence.aV, 0.0F, 5.0F);
                    break;
                case "nHC":
                    this.wideformPresence.nHC = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nHC, this.wideformPresence.nHC, 0.0F, 5.0F);
                    break;
                case "nVB":
                    this.wideformPresence.nVB = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nVB, this.wideformPresence.nVB, 0.0F, 5.0F);
                    break;
                case "nVH":
                    this.wideformPresence.nVH = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nVH, this.wideformPresence.nVH, 0.0F, 5.0F);
                    break;
                case "nVBH":
                    this.wideformPresence.nVBH = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nVBH, this.wideformPresence.nVBH, 0.0F, 5.0F);
                    break;
                case "aK":
                    this.wideformPresence.aK = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.aK, this.wideformPresence.aK, 0.0F, 5.0F);
                    break;
                case "nKB":
                    this.wideformPresence.nKB = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nKB, this.wideformPresence.nKB, 0.0F, 5.0F);
                    break;
                case "nKH":
                    this.wideformPresence.nKH = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nKH, this.wideformPresence.nKH, 0.0F, 5.0F);
                    break;
                case "nKC":
                    this.wideformPresence.nKC = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nKC, this.wideformPresence.nKC, -1.0F, 0.0F);
                    break;
                case "nKrh":
                    this.wideformPresence.nKrh = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nKrh, this.wideformPresence.nKrh, 0.0F, 5.0F);
                    break;
                case "aHL":
                    this.wideformPresence.aHL = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.aHL, this.wideformPresence.aHL, 0.0F, 15.0F);
                    break;
                case "nHLB":
                    this.wideformPresence.nHLB = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nHLB, this.wideformPresence.nHLB, 0.0F, 5.0F);
                    break;
                case "nHLL":
                    this.wideformPresence.nHLL = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nHLL, this.wideformPresence.nHLL, 0.0F, 5.0F);
                    break;
                case "nHLC":
                    this.wideformPresence.nHLC = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nHLC, this.wideformPresence.nHLC, -1.0F, 0.0F);
                    break;
                case "nHLrh":
                    this.wideformPresence.nHLrh = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.nHLrh, this.wideformPresence.nHLrh, 0.0F, 5.0F);
                    break;
                case "Qa":
                    this.wideformPresence.Qa = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.Qa, this.wideformPresence.Qa, -200.0F, 0.0F);
                    break;
                case "Qb":
                    this.wideformPresence.Qb = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.Qb, this.wideformPresence.Qb, 0.0F, 5.0F);
                    break;
                case "gDM_mol":
                    this.wideformPresence.gDM_mol = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.gDM_mol, this.wideformPresence.gDM_mol, 0.0F, 50.0F);
                    break;
                case "molPAR_MJ":
                    this.wideformPresence.molPAR_MJ = TreeSpeciesWorksheet.Parse(parameter, row, this.Parameters.molPAR_MJ, this.wideformPresence.molPAR_MJ, 0.0F, 5.0F);
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

        // parsing tracking flags to check for duplicate and missing parameters
        private class WideformParameterPresence
        {
            // biomass partitioning and turnover
            public bool pFS2 { get; set; }
            public bool pFS20 { get; set; }
            public bool aWS { get; set; }
            public bool nWS { get; set; }
            public bool pRx { get; set; }
            public bool pRn { get; set; }
            public bool gammaF1 { get; set; }
            public bool gammaF0 { get; set; }
            public bool tgammaF { get; set; }
            public bool gammaR { get; set; }
            public bool leafgrow { get; set; }
            public bool leaffall { get; set; }

            // NPP & conductance modifiers
            public bool Tmin { get; set; }
            public bool Topt { get; set; }
            public bool Tmax { get; set; }
            public bool kF { get; set; }
            public bool SWconst0 { get; set; }
            public bool SWpower0 { get; set; }
            public bool fCalpha700 { get; set; }
            public bool fCg700 { get; set; }
            public bool m0 { get; set; }
            public bool fN0 { get; set; }
            public bool fNn { get; set; }
            public bool MaxAge { get; set; }
            public bool nAge { get; set; }
            public bool rAge { get; set; }

            public bool gammaN1 { get; set; }
            public bool gammaN0 { get; set; }
            public bool tgammaN { get; set; }
            public bool ngammaN { get; set; }
            public bool wSx1000 { get; set; }
            public bool thinPower { get; set; }
            public bool mF { get; set; }
            public bool mR { get; set; }
            public bool mS { get; set; }

            // canopy structure and processes
            public bool SLA0 { get; set; }
            public bool SLA1 { get; set; }
            public bool tSLA { get; set; }
            public bool k { get; set; }
            public bool fullCanAge { get; set; }
            public bool MaxIntcptn { get; set; }
            public bool LAImaxIntcptn { get; set; }
            public bool cVPD { get; set; }
            public bool alphaCx { get; set; }
            public bool y { get; set; }
            public bool MinCond { get; set; }
            public bool MaxCond { get; set; }
            public bool LAIgcx { get; set; }
            public bool CoeffCond { get; set; }
            public bool BLcond { get; set; }
            public bool RGcGw { get; set; }
            public bool D13CTissueDif { get; set; }
            public bool aFracDiffu { get; set; }
            public bool bFracRubi { get; set; }

            // wood and stand properties
            public bool fracBB0 { get; set; }
            public bool fracBB1 { get; set; }
            public bool tBB { get; set; }
            public bool rhoMin { get; set; }
            public bool rhoMax { get; set; }
            public bool tRho { get; set; }
            public bool CrownShape { get; set; }

            // height and volume
            public bool aH { get; set; }
            public bool nHB { get; set; }
            public bool nHC { get; set; }
            public bool aV { get; set; }
            public bool nVB { get; set; }
            public bool nVH { get; set; }
            public bool nVBH { get; set; }
            public bool aK { get; set; }
            public bool nKB { get; set; }
            public bool nKH { get; set; }
            public bool nKC { get; set; }
            public bool nKrh { get; set; }
            public bool aHL { get; set; }
            public bool nHLB { get; set; }
            public bool nHLL { get; set; }
            public bool nHLC { get; set; }
            public bool nHLrh { get; set; }

            // δ¹³C
            public bool Qa { get; set; }
            public bool Qb { get; set; }
            public bool gDM_mol { get; set; }
            public bool molPAR_MJ { get; set; }

            public WideformParameterPresence()
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
            }

            public void OnEndParsing()
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
            }
        }
    }
}
