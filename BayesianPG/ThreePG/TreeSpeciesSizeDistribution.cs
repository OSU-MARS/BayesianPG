using BayesianPG.Extensions;
using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// Weibull parameters for bias correction in diameter (D) and stem mass (ws).
    /// </summary>
    /// <remarks>
    /// 3-PGmix manual section 11.10, equations A54-A59.
    /// </remarks>
    public class TreeSpeciesSizeDistribution : TreeSpeciesArray
    {
        /// <summary>
        /// (pars_b[0])
        /// </summary>
        public float[] Dscale0 { get; private set; }
        /// <summary>
        /// (pars_b[1])
        /// </summary>
        public float[] DscaleB { get; private set; }
        /// <summary>
        /// (pars_b[2])
        /// </summary>
        public float[] Dscalerh { get; private set; }
        /// <summary>
        /// (pars_b[3])
        /// </summary>
        public float[] Dscalet { get; private set; }
        /// <summary>
        /// (pars_b[4])
        /// </summary>
        public float[] DscaleC { get; private set; }

        /// <summary>
        /// (pars_b[5])
        /// </summary>
        public float[] Dshape0 { get; private set; }
        /// <summary>
        /// (pars_b[6])
        /// </summary>
        public float[] DshapeB { get; private set; }
        /// <summary>
        /// (pars_b[7])
        /// </summary>
        public float[] Dshaperh { get; private set; }
        /// <summary>
        /// (pars_b[8])
        /// </summary>
        public float[] Dshapet { get; private set; }
        /// <summary>
        /// (pars_b[9])
        /// </summary>
        public float[] DshapeC { get; private set; }

        /// <summary>
        /// (pars_b[100])
        /// </summary>
        public float[] Dlocation0 { get; private set; }
        /// <summary>
        /// (pars_b[11])
        /// </summary>
        public float[] DlocationB { get; private set; }
        /// <summary>
        /// (pars_b[12])
        /// </summary>
        public float[] Dlocationrh { get; private set; }
        /// <summary>
        /// (pars_b[13])
        /// </summary>
        public float[] Dlocationt { get; private set; }
        /// <summary>
        /// (pars_b[14])
        /// </summary>
        public float[] DlocationC { get; private set; }

        /// <summary>
        /// (pars_b[15])
        /// </summary>
        public float[] wsscale0 { get; private set; }
        /// <summary>
        /// (pars_b[16])
        /// </summary>
        public float[] wsscaleB { get; private set; }
        /// <summary>
        /// (pars_b[17])
        /// </summary>
        public float[] wsscalerh { get; private set; }
        /// <summary>
        /// (pars_b[18])
        /// </summary>
        public float[] wsscalet { get; private set; }
        /// <summary>
        /// (pars_b[19])
        /// </summary>
        public float[] wsscaleC { get; private set; }

        /// <summary>
        /// (pars_b[20])
        /// </summary>
        public float[] wsshape0 { get; private set; }
        /// <summary>
        /// (pars_b[21])
        /// </summary>
        public float[] wsshapeB { get; private set; }
        /// <summary>
        /// (pars_b[22])
        /// </summary>
        public float[] wsshaperh { get; private set; }
        /// <summary>
        /// (pars_b[23])
        /// </summary>
        public float[] wsshapet { get; private set; }
        /// <summary>
        /// (pars_b[24])
        /// </summary>
        public float[] wsshapeC { get; private set; }

        /// <summary>
        /// (pars_b[25])
        /// </summary>
        public float[] wslocation0 { get; private set; }
        /// <summary>
        /// (pars_b[26])
        /// </summary>
        public float[] wslocationB { get; private set; }
        /// <summary>
        /// (pars_b[27])
        /// </summary>
        public float[] wslocationrh { get; private set; }
        /// <summary>
        /// (pars_b[28])
        /// </summary>
        public float[] wslocationt { get; private set; }
        /// <summary>
        /// (pars_b[29])
        /// </summary>
        public float[] wslocationC { get; private set; }

        public TreeSpeciesSizeDistribution()
        {
            this.Dscale0 = Array.Empty<float>();
            this.DscaleB = Array.Empty<float>();
            this.Dscalerh = Array.Empty<float>();
            this.Dscalet = Array.Empty<float>();
            this.DscaleC = Array.Empty<float>();

            this.Dshape0 = Array.Empty<float>();
            this.DshapeB = Array.Empty<float>();
            this.Dshaperh = Array.Empty<float>();
            this.Dshapet = Array.Empty<float>();
            this.DshapeC = Array.Empty<float>();

            this.Dlocation0 = Array.Empty<float>();
            this.DlocationB = Array.Empty<float>();
            this.Dlocationrh = Array.Empty<float>();
            this.Dlocationt = Array.Empty<float>();
            this.DlocationC = Array.Empty<float>();

            this.wsscale0 = Array.Empty<float>();
            this.wsscaleB = Array.Empty<float>();
            this.wsscalerh = Array.Empty<float>();
            this.wsscalet = Array.Empty<float>();
            this.wsscaleC = Array.Empty<float>();

            this.wsshape0 = Array.Empty<float>();
            this.wsshapeB = Array.Empty<float>();
            this.wsshaperh = Array.Empty<float>();
            this.wsshapet = Array.Empty<float>();
            this.wsshapeC = Array.Empty<float>();

            this.wslocation0 = Array.Empty<float>();
            this.wslocationB = Array.Empty<float>();
            this.wslocationrh = Array.Empty<float>();
            this.wslocationt = Array.Empty<float>();
            this.wslocationC = Array.Empty<float>();
        }

        public override void AllocateSpecies(int additionalSpecies)
        {
            base.AllocateSpecies(additionalSpecies);

            this.Dscale0 = this.Dscale0.Resize(this.n_sp);
            this.DscaleB = this.DscaleB.Resize(this.n_sp);
            this.Dscalerh = this.Dscalerh.Resize(this.n_sp);
            this.Dscalet = this.Dscalet.Resize(this.n_sp);
            this.DscaleC = this.DscaleC.Resize(this.n_sp);

            this.Dshape0 = this.Dshape0.Resize(this.n_sp);
            this.DshapeB = this.DshapeB.Resize(this.n_sp);
            this.Dshaperh = this.Dshaperh.Resize(this.n_sp);
            this.Dshapet = this.Dshapet.Resize(this.n_sp);
            this.DshapeC = this.DshapeC.Resize(this.n_sp);

            this.Dlocation0 = this.Dlocation0.Resize(this.n_sp);
            this.DlocationB = this.DlocationB.Resize(this.n_sp);
            this.Dlocationrh = this.Dlocationrh.Resize(this.n_sp);
            this.Dlocationt = this.Dlocationt.Resize(this.n_sp);
            this.DlocationC = this.DlocationC.Resize(this.n_sp);

            this.wsscale0 = this.wsscale0.Resize(this.n_sp);
            this.wsscaleB = this.wsscaleB.Resize(this.n_sp);
            this.wsscalerh = this.wsscalerh.Resize(this.n_sp);
            this.wsscalet = this.wsscalet.Resize(this.n_sp);
            this.wsscaleC = this.wsscaleC.Resize(this.n_sp);

            this.wsshape0 = this.wsshape0.Resize(this.n_sp);
            this.wsshapeB = this.wsshapeB.Resize(this.n_sp);
            this.wsshaperh = this.wsshaperh.Resize(this.n_sp);
            this.wsshapet = this.wsshapet.Resize(this.n_sp);
            this.wsshapeC = this.wsshapeC.Resize(this.n_sp);

            this.wslocation0 = this.wslocation0.Resize(this.n_sp);
            this.wslocationB = this.wslocationB.Resize(this.n_sp);
            this.wslocationrh = this.wslocationrh.Resize(this.n_sp);
            this.wslocationt = this.wslocationt.Resize(this.n_sp);
            this.wslocationC = this.wslocationC.Resize(this.n_sp);
        }

        public override void AllocateSpecies(string[] names)
        {
            int existingSpecies = this.n_sp;
            this.AllocateSpecies(names.Length);
            Array.Copy(names, 0, this.Species, existingSpecies, names.Length);
        }

        public TreeSpeciesSizeDistribution Filter(SiteTreeSpecies treeSpecies)
        {
            TreeSpeciesSizeDistribution filteredDistribution = new();
            filteredDistribution.AllocateSpecies(treeSpecies.Species);

            for (int destinationIndex = 0; destinationIndex < treeSpecies.n_sp; ++destinationIndex)
            {
                int sourceIndex = this.Species.FindIndex(treeSpecies.Species[destinationIndex]);
                if (sourceIndex == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(treeSpecies));
                }

                filteredDistribution.Dscale0[destinationIndex] = this.Dscale0[sourceIndex];
                filteredDistribution.DscaleB[destinationIndex] = this.DscaleB[sourceIndex];
                filteredDistribution.Dscalerh[destinationIndex] = this.Dscalerh[sourceIndex];
                filteredDistribution.Dscalet[destinationIndex] = this.Dscalet[sourceIndex];
                filteredDistribution.DscaleC[destinationIndex] = this.DscaleC[sourceIndex];

                filteredDistribution.Dshape0[destinationIndex] = this.Dshape0[sourceIndex];
                filteredDistribution.DshapeB[destinationIndex] = this.DshapeB[sourceIndex];
                filteredDistribution.Dshaperh[destinationIndex] = this.Dshaperh[sourceIndex];
                filteredDistribution.Dshapet[destinationIndex] = this.Dshapet[sourceIndex];
                filteredDistribution.DshapeC[destinationIndex] = this.DshapeC[sourceIndex];

                filteredDistribution.Dlocation0[destinationIndex] = this.Dlocation0[sourceIndex];
                filteredDistribution.DlocationB[destinationIndex] = this.DlocationB[sourceIndex];
                filteredDistribution.Dlocationrh[destinationIndex] = this.Dlocationrh[sourceIndex];
                filteredDistribution.Dlocationt[destinationIndex] = this.Dlocationt[sourceIndex];
                filteredDistribution.DlocationC[destinationIndex] = this.DlocationC[sourceIndex];

                filteredDistribution.wsscale0[destinationIndex] = this.wsscale0[sourceIndex];
                filteredDistribution.wsscaleB[destinationIndex] = this.wsscaleB[sourceIndex];
                filteredDistribution.wsscalerh[destinationIndex] = this.wsscalerh[sourceIndex];
                filteredDistribution.wsscalet[destinationIndex] = this.wsscalet[sourceIndex];
                filteredDistribution.wsscaleC[destinationIndex] = this.wsscaleC[sourceIndex];

                filteredDistribution.wsshape0[destinationIndex] = this.wsshape0[sourceIndex];
                filteredDistribution.wsshapeB[destinationIndex] = this.wsshapeB[sourceIndex];
                filteredDistribution.wsshaperh[destinationIndex] = this.wsshaperh[sourceIndex];
                filteredDistribution.wsshapet[destinationIndex] = this.wsshapet[sourceIndex];
                filteredDistribution.wsshapeC[destinationIndex] = this.wsshapeC[sourceIndex];

                filteredDistribution.wslocation0[destinationIndex] = this.wslocation0[sourceIndex];
                filteredDistribution.wslocationB[destinationIndex] = this.wslocationB[sourceIndex];
                filteredDistribution.wslocationrh[destinationIndex] = this.wslocationrh[sourceIndex];
                filteredDistribution.wslocationt[destinationIndex] = this.wslocationt[sourceIndex];
                filteredDistribution.wslocationC[destinationIndex] = this.wslocationC[sourceIndex];
            }

            return filteredDistribution;
        }
    }
}