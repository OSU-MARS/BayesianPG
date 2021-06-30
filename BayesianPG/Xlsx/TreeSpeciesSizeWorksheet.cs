using BayesianPG.ThreePG;
using System;
using System.Xml;

namespace BayesianPG.Xlsx
{
    internal class TreeSpeciesSizeWorksheet : TreeSpeciesWorksheet
    {
        // parsing tracking flags to check for duplicate and missing parameters
        private bool Dscale0;
        private bool DscaleB;
        private bool Dscalerh;
        private bool Dscalet;
        private bool DscaleC;

        private bool Dshape0;
        private bool DshapeB;
        private bool Dshaperh;
        private bool Dshapet;
        private bool DshapeC;

        private bool Dlocation0;
        private bool DlocationB;
        private bool Dlocationrh;
        private bool Dlocationt;
        private bool DlocationC;

        private bool wsscale0;
        private bool wsscaleB;
        private bool wsscalerh;
        private bool wsscalet;
        private bool wsscaleC;

        private bool wsshape0;
        private bool wsshapeB;
        private bool wsshaperh;
        private bool wsshapet;
        private bool wsshapeC;

        private bool wslocation0;
        private bool wslocationB;
        private bool wslocationrh;
        private bool wslocationt;
        private bool wslocationC;

        public TreeSpeciesSizeDistribution Sizes { get; private init; }

        public TreeSpeciesSizeWorksheet()
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

            this.Sizes = new TreeSpeciesSizeDistribution();
        }

        public override void OnEndParsing()
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

        public override void ParseHeader(XlsxRow row)
        {
            TreeSpeciesWorksheet.ValidateHeader(row);
            this.Sizes.AllocateSpecies(row.Row[1..]);
        }

        public override void ParseRow(XlsxRow row)
        {
            string parameter = row.Row[0];
            if (row.Columns != this.Sizes.n_sp + 1)
            {
                throw new XmlException(parameter + " parameter values for " + (this.Sizes.n_sp - row.Columns + 1) + " species are missing.", null, row.Number, 2);
            }

            // for now, sanity range checking
            switch (parameter)
            {
                case "Dscale0":
                    this.Dscale0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dscale0, this.Dscale0, 0.0F, 4.0F);
                    break;
                case "DscaleB":
                    this.DscaleB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.DscaleB, this.DscaleB, 0.0F, 4.0F);
                    break;
                case "Dscalerh":
                    this.Dscalerh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dscalerh, this.Dscalerh, 0.0F, 4.0F);
                    break;
                case "Dscalet":
                    this.Dscalet = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dscalet, this.Dscalet, 0.0F, 4.0F);
                    break;
                case "DscaleC":
                    this.DscaleC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.DscaleC, this.DscaleC, 0.0F, 4.0F);
                    break;
                case "Dshape0":
                    this.Dshape0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dshape0, this.DshapeB, 0.0F, 4.0F);
                    break;
                case "DshapeB":
                    this.DshapeB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.DshapeB, this.DshapeB, 0.0F, 4.0F);
                    break;
                case "Dshaperh":
                    this.Dshaperh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dshaperh, this.Dshaperh, 0.0F, 4.0F);
                    break;
                case "Dshapet":
                    this.Dshapet = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dshapet, this.Dshapet, 0.0F, 4.0F);
                    break;
                case "DshapeC":
                    this.DshapeC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.DshapeC, this.DshapeC, -2.0F, 2.0F);
                    break;
                case "Dlocation0":
                    this.Dlocation0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dlocation0, this.Dlocation0, 0.0F, 4.0F);
                    break;
                case "DlocationB":
                    this.DlocationB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.DlocationB, this.DlocationB, 0.0F, 4.0F);
                    break;
                case "Dlocationrh":
                    this.Dlocationrh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dlocationrh, this.Dlocationrh, -2.0F, 2.0F);
                    break;
                case "Dlocationt":
                    this.Dlocationt = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.Dlocationt, this.Dlocationt, 0.0F, 4.0F);
                    break;
                case "DlocationC":
                    this.DlocationC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.DlocationC, this.DlocationC, -2.0F, 2.0F);
                    break;
                case "wsscale0":
                    this.wsscale0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsscale0, this.wsscale0, 0.0F, 4.0F);
                    break;
                case "wsscaleB":
                    this.wsscaleB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsscaleB, this.wsscaleB, 0.0F, 4.0F);
                    break;
                case "wsscalerh":
                    this.wsscalerh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsscalerh, this.wsscalerh, -4.0F, 0.0F);
                    break;
                case "wsscalet":
                    this.wsscalet = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsscalet, this.wsscalet, 0.0F, 4.0F);
                    break;
                case "wsscaleC":
                    this.wsscaleC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsscaleC, this.wsscaleC, -2.0F, 2.0F);
                    break;
                case "wsshape0":
                    this.wsshape0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsshape0, this.wsshape0, 0.0F, 4.0F);
                    break;
                case "wsshapeB":
                    this.wsshapeB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsshapeB, this.wsshapeB, 0.0F, 4.0F);
                    break;
                case "wsshaperh":
                    this.wsshaperh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsshaperh, this.wsshaperh, 0.0F, 4.0F);
                    break;
                case "wsshapet":
                    this.wsshapet = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsshapet, this.wsshapet, 0.0F, 4.0F);
                    break;
                case "wsshapeC":
                    this.wsshapeC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wsshapeC, this.wsshapeC, -2.0F, 2.0F);
                    break;
                case "wslocation0":
                    this.wslocation0 = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wslocation0, this.wslocation0, 0.0F, 4.0F);
                    break;
                case "wslocationB":
                    this.wslocationB = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wslocationB, this.wslocationB, 0.0F, 4.0F);
                    break;
                case "wslocationrh":
                    this.wslocationrh = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wslocationrh, this.wslocationrh, -4.0F, 0.0F);
                    break;
                case "wslocationt":
                    this.wslocationt = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wslocationt, this.wslocationt, -2.0F, 2.0F);
                    break;
                case "wslocationC":
                    this.wslocationC = TreeSpeciesWorksheet.ParseRow(parameter, row, this.Sizes.wslocationC, this.wslocationC, 0.0F, 4.0F);
                    break;
                default:
                    throw new NotSupportedException("Unhandled parameter name " + parameter + ".");
            }
        }
    }
}
