using BayesianPG.ThreePG;
using System;
using System.Diagnostics;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class TreeSpeciesSizeWorksheet : XlsxWorksheet<TreeSpeciesSizeHeader>
    {
        private WideformParameterPresence? wideformPresence;

        public TreeSpeciesSizeDistribution Sizes { get; private init; }

        public TreeSpeciesSizeWorksheet()
        {
            this.wideformPresence = null;

            this.Sizes = new TreeSpeciesSizeDistribution();
        }

        public override void OnEndParsing()
        {
            if (this.wideformPresence != null)
            {
                this.wideformPresence.OnEndParsing();
            }
        }

        public override void ParseHeader(XlsxRow row)
        {
            if (String.Equals(row.Row[0], "parameter", StringComparison.Ordinal))
            {
                TreeSpeciesWorksheet.ValidateHeader(row);
                this.Sizes.AllocateSpecies(row.Row[1..]);
                this.wideformPresence = new();
            }
            else
            {
                this.Header.Parse(row);
                this.Sizes.AllocateSpecies(row.Rows - 1);
            }
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

            this.Sizes.Species[speciesIndex] = row.Row[this.Header.Species];
            this.Sizes.Dscale0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dscale0), row, this.Header.Dscale0, 0.0F, 4.0F);
            this.Sizes.DscaleB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.DscaleB), row, this.Header.DscaleB, 0.0F, 4.0F);
            this.Sizes.Dscalerh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dscalerh), row, this.Header.Dscalerh, 0.0F, 4.0F);
            this.Sizes.Dscalet[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dscalet), row, this.Header.Dscalet, 0.0F, 4.0F);
            this.Sizes.DscaleC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.DscaleC), row, this.Header.DscaleC, 0.0F, 4.0F);
            
            this.Sizes.Dshape0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dshape0), row, this.Header.Dshape0, 0.0F, 4.0F);
            this.Sizes.DshapeB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.DshapeB), row, this.Header.DshapeB, 0.0F, 4.0F);
            this.Sizes.Dshaperh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dshaperh), row, this.Header.Dshaperh, 0.0F, 4.0F);
            this.Sizes.Dshapet[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dshapet), row, this.Header.Dshapet, 0.0F, 4.0F);
            this.Sizes.DshapeC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.DshapeC), row, this.Header.DshapeC, -2.0F, 2.0F);

            this.Sizes.Dlocation0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dlocation0), row, this.Header.Dlocation0, 0.0F, 4.0F);
            this.Sizes.DlocationB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.DlocationB), row, this.Header.DlocationB, 0.0F, 4.0F);
            this.Sizes.Dlocationrh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dlocationrh), row, this.Header.Dlocationrh, -2.0F, 2.0F);
            this.Sizes.Dlocationt[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.Dlocationt), row, this.Header.Dlocationt, 0.0F, 4.0F);
            this.Sizes.DlocationC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.DlocationC), row, this.Header.DlocationC, -2.0F, 2.0F);
            
            this.Sizes.wsscale0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsscale0), row, this.Header.wsscale0, 0.0F, 4.0F);
            this.Sizes.wsscaleB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsscaleB), row, this.Header.wsscaleB, 0.0F, 4.0F);
            this.Sizes.wsscalerh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsscalerh), row, this.Header.wsscalerh, -4.0F, 0.0F);
            this.Sizes.wsscalet[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsscalet), row, this.Header.wsscalet, 0.0F, 4.0F);
            this.Sizes.wsscaleC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsscaleC), row, this.Header.wsscaleC, -2.0F, 2.0F);
            
            this.Sizes.wsshape0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsshape0), row, this.Header.wsshape0, 0.0F, 4.0F);
            this.Sizes.wsshapeB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsshapeB), row, this.Header.wsshapeB, 0.0F, 4.0F);
            this.Sizes.wsshaperh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsshaperh), row, this.Header.wsshaperh, 0.0F, 4.0F);
            this.Sizes.wsshapet[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsshapet), row, this.Header.wsshapet, 0.0F, 4.0F);
            this.Sizes.wsshapeC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wsshapeC), row, this.Header.wsshapeC, -2.0F, 2.0F);

            this.Sizes.wslocation0[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wslocation0), row, this.Header.wslocation0, 0.0F, 4.0F);
            this.Sizes.wslocationB[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wslocationB), row, this.Header.wslocationB, 0.0F, 4.0F);
            this.Sizes.wslocationrh[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wslocationrh), row, this.Header.wslocationrh, -4.0F, 0.0F);
            this.Sizes.wslocationt[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wslocationt), row, this.Header.wslocationt, -2.0F, 2.0F);
            this.Sizes.wslocationC[speciesIndex] = TreeSpeciesWorksheet.Parse(nameof(this.Sizes.wslocationC), row, this.Header.wslocationC, 0.0F, 4.0F);
        }

        private void ParseRowWideform(XlsxRow row)
        {
            string parameter = row.Row[0];
            if (row.Columns != this.Sizes.n_sp + 1)
            {
                throw new XmlException(parameter + " parameter values for " + (this.Sizes.n_sp - row.Columns + 1) + " species are missing.", null, row.Number, 2);
            }

            // for now, sanity range checking
            Debug.Assert(this.wideformPresence != null);
            switch (parameter)
            {
                case "Dscale0":
                    this.wideformPresence.Dscale0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dscale0, this.wideformPresence.Dscale0, 0.0F, 4.0F);
                    break;
                case "DscaleB":
                    this.wideformPresence.DscaleB = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.DscaleB, this.wideformPresence.DscaleB, 0.0F, 4.0F);
                    break;
                case "Dscalerh":
                    this.wideformPresence.Dscalerh = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dscalerh, this.wideformPresence.Dscalerh, 0.0F, 4.0F);
                    break;
                case "Dscalet":
                    this.wideformPresence.Dscalet = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dscalet, this.wideformPresence.Dscalet, 0.0F, 4.0F);
                    break;
                case "DscaleC":
                    this.wideformPresence.DscaleC = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.DscaleC, this.wideformPresence.DscaleC, 0.0F, 4.0F);
                    break;
                case "Dshape0":
                    this.wideformPresence.Dshape0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dshape0, this.wideformPresence.DshapeB, 0.0F, 4.0F);
                    break;
                case "DshapeB":
                    this.wideformPresence.DshapeB = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.DshapeB, this.wideformPresence.DshapeB, 0.0F, 4.0F);
                    break;
                case "Dshaperh":
                    this.wideformPresence.Dshaperh = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dshaperh, this.wideformPresence.Dshaperh, 0.0F, 4.0F);
                    break;
                case "Dshapet":
                    this.wideformPresence.Dshapet = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dshapet, this.wideformPresence.Dshapet, 0.0F, 4.0F);
                    break;
                case "DshapeC":
                    this.wideformPresence.DshapeC = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.DshapeC, this.wideformPresence.DshapeC, -2.0F, 2.0F);
                    break;
                case "Dlocation0":
                    this.wideformPresence.Dlocation0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dlocation0, this.wideformPresence.Dlocation0, 0.0F, 4.0F);
                    break;
                case "DlocationB":
                    this.wideformPresence.DlocationB = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.DlocationB, this.wideformPresence.DlocationB, 0.0F, 4.0F);
                    break;
                case "Dlocationrh":
                    this.wideformPresence.Dlocationrh = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dlocationrh, this.wideformPresence.Dlocationrh, -2.0F, 2.0F);
                    break;
                case "Dlocationt":
                    this.wideformPresence.Dlocationt = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.Dlocationt, this.wideformPresence.Dlocationt, 0.0F, 4.0F);
                    break;
                case "DlocationC":
                    this.wideformPresence.DlocationC = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.DlocationC, this.wideformPresence.DlocationC, -2.0F, 2.0F);
                    break;
                case "wsscale0":
                    this.wideformPresence.wsscale0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsscale0, this.wideformPresence.wsscale0, 0.0F, 4.0F);
                    break;
                case "wsscaleB":
                    this.wideformPresence.wsscaleB = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsscaleB, this.wideformPresence.wsscaleB, 0.0F, 4.0F);
                    break;
                case "wsscalerh":
                    this.wideformPresence.wsscalerh = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsscalerh, this.wideformPresence.wsscalerh, -4.0F, 0.0F);
                    break;
                case "wsscalet":
                    this.wideformPresence.wsscalet = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsscalet, this.wideformPresence.wsscalet, 0.0F, 4.0F);
                    break;
                case "wsscaleC":
                    this.wideformPresence.wsscaleC = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsscaleC, this.wideformPresence.wsscaleC, -2.0F, 2.0F);
                    break;
                case "wsshape0":
                    this.wideformPresence.wsshape0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsshape0, this.wideformPresence.wsshape0, 0.0F, 4.0F);
                    break;
                case "wsshapeB":
                    this.wideformPresence.wsshapeB = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsshapeB, this.wideformPresence.wsshapeB, 0.0F, 4.0F);
                    break;
                case "wsshaperh":
                    this.wideformPresence.wsshaperh = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsshaperh, this.wideformPresence.wsshaperh, 0.0F, 4.0F);
                    break;
                case "wsshapet":
                    this.wideformPresence.wsshapet = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsshapet, this.wideformPresence.wsshapet, 0.0F, 4.0F);
                    break;
                case "wsshapeC":
                    this.wideformPresence.wsshapeC = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wsshapeC, this.wideformPresence.wsshapeC, -2.0F, 2.0F);
                    break;
                case "wslocation0":
                    this.wideformPresence.wslocation0 = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wslocation0, this.wideformPresence.wslocation0, 0.0F, 4.0F);
                    break;
                case "wslocationB":
                    this.wideformPresence.wslocationB = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wslocationB, this.wideformPresence.wslocationB, 0.0F, 4.0F);
                    break;
                case "wslocationrh":
                    this.wideformPresence.wslocationrh = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wslocationrh, this.wideformPresence.wslocationrh, -4.0F, 0.0F);
                    break;
                case "wslocationt":
                    this.wideformPresence.wslocationt = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wslocationt, this.wideformPresence.wslocationt, -2.0F, 2.0F);
                    break;
                case "wslocationC":
                    this.wideformPresence.wslocationC = TreeSpeciesWorksheet.Parse(parameter, row, this.Sizes.wslocationC, this.wideformPresence.wslocationC, 0.0F, 4.0F);
                    break;
                default:
                    throw new NotSupportedException("Unhandled parameter name " + parameter + ".");
            }
        }


        private class WideformParameterPresence
        {
            // parsing tracking flags to check for duplicate and missing parameters
            public bool Dscale0 { get; set; }
            public bool DscaleB { get; set; }
            public bool Dscalerh { get; set; }
            public bool Dscalet { get; set; }
            public bool DscaleC { get; set; }

            public bool Dshape0 { get; set; }
            public bool DshapeB { get; set; }
            public bool Dshaperh { get; set; }
            public bool Dshapet { get; set; }
            public bool DshapeC { get; set; }

            public bool Dlocation0 { get; set; }
            public bool DlocationB { get; set; }
            public bool Dlocationrh { get; set; }
            public bool Dlocationt { get; set; }
            public bool DlocationC { get; set; }

            public bool wsscale0 { get; set; }
            public bool wsscaleB { get; set; }
            public bool wsscalerh { get; set; }
            public bool wsscalet { get; set; }
            public bool wsscaleC { get; set; }

            public bool wsshape0 { get; set; }
            public bool wsshapeB { get; set; }
            public bool wsshaperh { get; set; }
            public bool wsshapet { get; set; }
            public bool wsshapeC { get; set; }

            public bool wslocation0 { get; set; }
            public bool wslocationB { get; set; }
            public bool wslocationrh { get; set; }
            public bool wslocationt { get; set; }
            public bool wslocationC { get; set; }

            public WideformParameterPresence()
            {
                this.Dscale0 = false;
                this.DscaleB = false;
                this.Dscalerh = false;
                this.Dscalet = false;
                this.DscaleC = false;

                this.Dshape0 = false;
                this.DshapeB = false;
                this.Dshaperh = false;
                this.Dshapet = false;
                this.DshapeC = false;

                this.Dlocation0 = false;
                this.DlocationB = false;
                this.Dlocationrh = false;
                this.Dlocationt = false;
                this.DlocationC = false;

                this.wsscale0 = false;
                this.wsscaleB = false;
                this.wsscalerh = false;
                this.wsscalet = false;
                this.wsscaleC = false;

                this.wsshape0 = false;
                this.wsshapeB = false;
                this.wsshaperh = false;
                this.wsshapet = false;
                this.wsshapeC = false;

                this.wslocation0 = false;
                this.wslocationB = false;
                this.wslocationrh = false;
                this.wslocationt = false;
                this.wslocationC = false;
            }

            public void OnEndParsing()
            {
                // check all parameters specified
                if (this.Dscale0 == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dscale0) + " is missing.", null);
                }
                if (this.DscaleB == false)
                {
                    throw new XmlException("Row for " + nameof(this.DscaleB) + " is missing.", null);
                }
                if (this.Dscalerh == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dscalerh) + " is missing.", null);
                }
                if (this.Dscalet == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dscalet) + " is missing.", null);
                }
                if (this.DscaleC == false)
                {
                    throw new XmlException("Row for " + nameof(this.DscaleC) + " is missing.", null);
                }

                if (this.Dshape0 == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dshape0) + " is missing.", null);
                }
                if (this.DshapeB == false)
                {
                    throw new XmlException("Row for " + nameof(this.DshapeB) + " is missing.", null);
                }
                if (this.Dshaperh == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dshaperh) + " is missing.", null);
                }
                if (this.Dshapet == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dshapet) + " is missing.", null);
                }
                if (this.DshapeC == false)
                {
                    throw new XmlException("Row for " + nameof(this.DshapeC) + " is missing.", null);
                }

                if (this.Dlocation0 == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dlocation0) + " is missing.", null);
                }
                if (this.DlocationB == false)
                {
                    throw new XmlException("Row for " + nameof(this.DlocationB) + " is missing.", null);
                }
                if (this.Dlocationrh == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dlocationrh) + " is missing.", null);
                }
                if (this.Dlocationt == false)
                {
                    throw new XmlException("Row for " + nameof(this.Dlocationt) + " is missing.", null);
                }
                if (this.DlocationC == false)
                {
                    throw new XmlException("Row for " + nameof(this.DlocationC) + " is missing.", null);
                }

                if (this.wsscale0 == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsscale0) + " is missing.", null);
                }
                if (this.wsscaleB == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsscaleB) + " is missing.", null);
                }
                if (this.wsscalerh == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsscalerh) + " is missing.", null);
                }
                if (this.wsscalet == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsscalet) + " is missing.", null);
                }
                if (this.wsscaleC == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsscaleC) + " is missing.", null);
                }

                if (this.wsshape0 == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsshape0) + " is missing.", null);
                }
                if (this.wsshapeB == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsshapeB) + " is missing.", null);
                }
                if (this.wsshaperh == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsshaperh) + " is missing.", null);
                }
                if (this.wsshapet == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsshapet) + " is missing.", null);
                }
                if (this.wsshapeC == false)
                {
                    throw new XmlException("Row for " + nameof(this.wsshapeC) + " is missing.", null);
                }

                if (this.wslocation0 == false)
                {
                    throw new XmlException("Row for " + nameof(this.wslocation0) + " is missing.", null);
                }
                if (this.wslocationB == false)
                {
                    throw new XmlException("Row for " + nameof(this.wslocationB) + " is missing.", null);
                }
                if (this.wslocationrh == false)
                {
                    throw new XmlException("Row for " + nameof(this.wslocationrh) + " is missing.", null);
                }
                if (this.wslocationt == false)
                {
                    throw new XmlException("Row for " + nameof(this.wslocationt) + " is missing.", null);
                }
                if (this.wslocationC == false)
                {
                    throw new XmlException("Row for " + nameof(this.wslocationC) + " is missing.", null);
                }
            }
        }
    }
}
