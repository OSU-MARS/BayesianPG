using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// tree species inputs for site (stocking, planting, ...): d_species in R, speciesInputs in Fortran
    /// Vectors of length n_sp.
    /// </summary>
    public class SiteTreeSpecies : TreeSpeciesArray
    {
        /// <summary>
        /// year when species was planted (speciesInputs[0])
        /// </summary>
        public int[] year_p { get; private set; }

        /// <summary>
        /// month when species was planted (speciesInputs[1])
        /// </summary>
        public int[] month_p { get; private set; }

        /// <summary>
        /// initial soil fertility rating for species (from 0 to 1, speciesInputs[2])
        /// </summary>
        public float[] fertility { get; private set; }

        /// <summary>
        /// initial stand stocking for a species (speciesInputs[3])
        /// </summary>
        public float[] stems_n_i { get; private set; }

        /// <summary>
        /// initial stem biomass for a species (speciesInputs[4])
        /// </summary>
        public float[] biom_stem_i { get; private set; }

        /// <summary>
        /// initial root biomass for a species (speciesInputs[5])
        /// </summary>
        public float[] biom_root_i { get; private set; }

        /// <summary>
        /// initial foliage biomass for a species (speciesInputs[6])
        /// </summary>
        public float[] biom_foliage_i { get; private set; }

        public SiteTreeSpecies()
        {
            this.biom_foliage_i = Array.Empty<float>();
            this.biom_root_i = Array.Empty<float>();
            this.biom_stem_i = Array.Empty<float>();
            this.fertility = Array.Empty<float>();
            this.month_p = Array.Empty<int>();
            this.stems_n_i = Array.Empty<float>();
            this.year_p = Array.Empty<int>();
        }

        public override void AllocateSpecies(string[] names)
        {
            base.AllocateSpecies(names);

            this.biom_foliage_i = this.biom_foliage_i.Resize(this.n_sp);
            this.biom_root_i = this.biom_root_i.Resize(this.n_sp);
            this.biom_stem_i = this.biom_stem_i.Resize(this.n_sp);
            this.fertility = this.fertility.Resize(this.n_sp);
            this.month_p = this.month_p.Resize(this.n_sp);
            this.stems_n_i = this.stems_n_i.Resize(this.n_sp);
            this.year_p = this.year_p.Resize(this.n_sp);
        }
    }
}
