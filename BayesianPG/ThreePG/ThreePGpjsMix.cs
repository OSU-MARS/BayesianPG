using System;
using System.Diagnostics;

namespace BayesianPG.ThreePG
{
    public abstract class ThreePGpjsMix
    {
        public TreeSpeciesSizeDistribution? Bias { get; init; }
        public SiteClimate Climate { get; private init; }
        public TreeSpeciesManagement Management { get; private init; }
        public ThreePGSettings Settings { get; private init; }
        public Site Site { get; private init; }
        public SiteTreeSpecies Species { get; private init; }

        protected ThreePGpjsMix(Site site, SiteClimate climate, SiteTreeSpecies species, TreeSpeciesManagement management, ThreePGSettings settings)
        {
            if ((climate.From.Year != site.From.Year) || (climate.From.Month != site.From.Month))
            {
                throw new ArgumentException("Climate start month " + climate.From.ToString("yyyy-MM") + " does not match site start month " + site.From.ToString("yyyy-MM"));
            }
            if (settings.management)
            {
                if (species.SpeciesMatch(management) == false)
                {
                    throw new ArgumentException("Tree species count or ordering is inconsistent between species and management.");
                }
            }
            // settings.phys_model can be freely chosen
            if ((settings.transp_model == ThreePGModel.Mix) && (settings.light_model == ThreePGModel.Pjs27))
            {
                throw new ArgumentException("Use of the 3-PGmix transpiration model requires the 3-PGmix light model also be used.");
            }
            if (site.AvailableSoilWaterMin > site.AvailableSoilWaterMax)
            {
                throw new ArgumentOutOfRangeException(nameof(site));
            }
            
            this.Climate = climate;
            this.Management = management;
            this.Settings = settings;
            this.Site = site;
            this.Species = species;
        }

        public abstract void PredictStandTrajectory();
    }

    public abstract class ThreePGpjsMix<TFloat, TInteger> : ThreePGpjsMix 
        where TFloat : struct
        where TInteger : struct
    {
        public TreeSpeciesParameters<TFloat> Parameters { get; private init; }
        protected ThreePGState<TFloat, TInteger> State { get; private init; }

        public ThreePGStandTrajectory<TFloat, TInteger> Trajectory { get; private init; }

        public ThreePGpjsMix(Site site, SiteClimate climate, SiteTreeSpecies species, TreeSpeciesParameters<TFloat> parameters, TreeSpeciesManagement management, ThreePGSettings settings)
            : base(site, climate, species, management, settings)
        {
            if (settings.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection) && ((settings.BiasCorrectionIterations <= 0) || (settings.CorrectSizeDistribution == false)))
            {
                throw new ArgumentException("Bias correction is included in stand trajectory but its calculation is disabled.");
            }
            if (settings.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C) && (settings.CalculateD13C == false))
            {
                throw new ArgumentException("Bias correction is included in stand trajectory but its calculation is disabled.");
            }
            if (species.SpeciesMatch(parameters) == false)
            {
                throw new ArgumentException("Tree species count or ordering is inconsistent between species and parameters.");
            }

            this.Parameters = parameters;
            this.State = new(species.n_sp, site);
            this.Trajectory = new(species.Species, site.From, site.To, settings.ColumnGroups);

            if (climate.MonthCount < this.Trajectory.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(site), "End month specified in site is beyond the end of the provided climate record.");
            }
        }

        protected bool IsDormant(int monthIndex, int speciesIndex)
        {
            // This is called if the leafgrow parameter is not 0, and hence the species is Deciduous
            // This is true if "currentmonth" is part of the dormant season
            float leaffall = this.Parameters.leaffall[speciesIndex];
            float leafgrow = this.Parameters.leafgrow[speciesIndex];
            if (leafgrow > leaffall)
            {
                // southern hemisphere
                Debug.Assert((monthIndex >= 0) && (monthIndex < 13) && (leaffall > 0) && (leafgrow < 13));
                if ((monthIndex >= leaffall) && (monthIndex <= leafgrow))
                {
                    return true;
                }
            }
            else if (leafgrow < leaffall)
            {
                // northern hemisphere
                Debug.Assert((monthIndex >= 0) && (monthIndex < 13) && (leafgrow > 0) && (leaffall < 13));
                if ((monthIndex < leafgrow) || (monthIndex >= leaffall))
                {
                    return true;
                }
            }

            // evergreen species: leafgrow = leaffall = 0
            return false;
        }
    }
}
