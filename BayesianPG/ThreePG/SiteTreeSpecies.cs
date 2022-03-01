using BayesianPG.Extensions;
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
        /// year when species was planted (Fortran speciesInputs[0])
        /// </summary>
        public int[] YearPlanted { get; private set; }

        /// <summary>
        /// month when species was planted (Fortran speciesInputs[1])
        /// </summary>
        public int[] MonthPlanted { get; private set; }

        /// <summary>
        /// initial foliage biomass for a species (Fortran speciesInputs[6])
        /// </summary>
        public float[] InitialFoliageBiomass { get; private set; }

        /// <summary>
        /// initial root biomass for a species (Fortran speciesInputs[5])
        /// </summary>
        public float[] InitialRootBiomass { get; private set; }

        /// <summary>
        /// initial stem biomass for a species (Fortran speciesInputs[4])
        /// </summary>
        public float[] InitialStemBiomass { get; private set; }

        /// <summary>
        /// initial stand stocking for a species (Fortran speciesInputs[3])
        /// </summary>
        public float[] InitialStemsPerHectare { get; private set; }

        /// <summary>
        /// initial soil fertility rating for species (from 0 to 1, speciesInputs[2])
        /// </summary>
        public float[] SoilFertility { get; private set; }

        public SiteTreeSpecies()
        {
            this.InitialFoliageBiomass = Array.Empty<float>();
            this.InitialRootBiomass = Array.Empty<float>();
            this.InitialStemBiomass = Array.Empty<float>();
            this.InitialStemsPerHectare = Array.Empty<float>();
            this.MonthPlanted = Array.Empty<int>();
            this.SoilFertility = Array.Empty<float>();
            this.YearPlanted = Array.Empty<int>();
        }

        public override void AllocateSpecies(string[] names)
        {
            base.AllocateSpecies(names);

            this.InitialFoliageBiomass = this.InitialFoliageBiomass.Resize(this.n_sp);
            this.InitialRootBiomass = this.InitialRootBiomass.Resize(this.n_sp);
            this.InitialStemBiomass = this.InitialStemBiomass.Resize(this.n_sp);
            this.InitialStemsPerHectare = this.InitialStemsPerHectare.Resize(this.n_sp);
            this.MonthPlanted = this.MonthPlanted.Resize(this.n_sp);
            this.SoilFertility = this.SoilFertility.Resize(this.n_sp);
            this.YearPlanted = this.YearPlanted.Resize(this.n_sp);
        }
    }
}
