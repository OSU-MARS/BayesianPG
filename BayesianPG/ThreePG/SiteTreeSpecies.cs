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
        /// Year when a tree species was planted (year, Fortran speciesInputs[0])
        /// </summary>
        public int[] YearPlanted { get; private set; }

        /// <summary>
        /// Month when a tree species was planted (month 1..12, Fortran speciesInputs[1])
        /// </summary>
        public int[] MonthPlanted { get; private set; }

        /// <summary>
        /// Initial foliage biomass for a tree species (dry Mg/ha, Fortran speciesInputs[6])
        /// </summary>
        public float[] InitialFoliageBiomass { get; private set; }

        /// <summary>
        /// Initial total root biomass (structural-coarse and fine) for a tree species (dry Mg/ha, Fortran speciesInputs[5])
        /// </summary>
        public float[] InitialRootBiomass { get; private set; }

        /// <summary>
        /// Initial stem biomass for a tree species (dry Mg/ha, Fortran speciesInputs[4])
        /// </summary>
        public float[] InitialStemBiomass { get; private set; }

        /// <summary>
        /// Initial stand stocking for a tree species (trees per hectare, Fortran speciesInputs[3])
        /// </summary>
        public float[] InitialStemsPerHectare { get; private set; }

        /// <summary>
        /// Initial soil fertility rating for a tree species (0..1, 0 assigns all fertility control to the species' m0, speciesInputs[2])
        /// </summary>
        public float[] SoilFertility { get; private set; }

        public SiteTreeSpecies()
        {
            this.InitialFoliageBiomass = [];
            this.InitialRootBiomass = [];
            this.InitialStemBiomass = [];
            this.InitialStemsPerHectare = [];
            this.MonthPlanted = [];
            this.SoilFertility = [];
            this.YearPlanted = [];
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
