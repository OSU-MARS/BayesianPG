using BayesianPG.Extensions;
using System;

namespace BayesianPG.ThreePG
{
    // base class for typing WriteStandTrajectory's -Trajectory parameter
    public class ThreePGStandTrajectory
    {
        public int Capacity { get; protected set; }
        public DateTime From { get; set; }
        public ThreePGStandTrajectoryColumnGroups ColumnGroups { get; protected set; }
        public int MonthCount { get; set; }

        protected ThreePGStandTrajectory(DateTime from, int capacity)
        {
            this.Capacity = capacity;
            this.ColumnGroups = ThreePGStandTrajectoryColumnGroups.Core;
            this.From = from;
            this.MonthCount = 0;
        }
    }

    public class ThreePGStandTrajectory<TFloat, TInteger> : ThreePGStandTrajectory 
        where TFloat : struct
        where TInteger : struct
    {
        public TFloat[] AvailableSoilWater { get; private set; }
        public TFloat[] evapo_transp { get; private set; } // total evapotranspiration
        public TFloat[] f_transp_scale { get; private set; }
        public TFloat[] irrig_supl { get; private set; }
        public TFloat[] prcp_runoff { get; private set; }
        public TFloat[] conduct_soil { get; private set; }
        public TFloat[] evapotra_soil { get; private set; }

        public TreeSpeciesTrajectory<TFloat, TInteger> Species { get; private set; }

        public ThreePGStandTrajectory(string speciesName, DateTime from, ThreePGStandTrajectoryColumnGroups columns)
            : this(new string[] { speciesName }, from, Constant.DefaultTimestepCapacity, columns)
        {
        }

        public ThreePGStandTrajectory(string[] speciesNames, DateTime from, DateTime to, ThreePGStandTrajectoryColumnGroups columns)
            : this(speciesNames, from, 12 * (to.Year - from.Year) + to.Month - from.Month + 1, columns)
        {
            if (to < from)
            {
                throw new ArgumentException("Trajectory end date " + to.ToString("yyyy-MM") + " is before trajectory start date " + from.ToString("yyyy-MM") + ".");
            }
        }

        protected ThreePGStandTrajectory(string[] speciesNames, DateTime from, int capacity, ThreePGStandTrajectoryColumnGroups columns)
            : base(from, capacity)
        {
            // core columns
            // (Species level columns are handled by this.Species).
            this.AvailableSoilWater = new TFloat[capacity];
            this.evapo_transp = new TFloat[capacity];
            this.f_transp_scale = new TFloat[capacity];
            this.ColumnGroups = columns;
            this.irrig_supl = new TFloat[capacity];
            this.prcp_runoff = new TFloat[capacity];
            this.conduct_soil = new TFloat[capacity];
            this.evapotra_soil = new TFloat[capacity];
            this.Species = new(speciesNames, capacity, columns);

            // all bias correction columns are species level

            // extended columns not currently supported in C#
            // co2
            // day_length
            // delta13c
            // solar_rad
            // tmp_ave
            // tmp_max
            // tmp_min
            // vpd_day
            // water_runoff_pooled
        }

        public void AllocateDecade()
        {
            this.Capacity += 10 * 12;

            this.AvailableSoilWater = this.AvailableSoilWater.Resize(this.Capacity);
            this.evapo_transp = this.evapo_transp.Resize(this.Capacity);
            this.f_transp_scale = this.f_transp_scale.Resize(this.Capacity);
            this.irrig_supl = this.irrig_supl.Resize(this.Capacity);
            this.prcp_runoff = this.prcp_runoff.Resize(this.Capacity);
            this.conduct_soil = this.conduct_soil.Resize(this.Capacity);
            this.evapotra_soil = this.evapotra_soil.Resize(this.Capacity);

            this.Species.AllocateDecade();
        }
    }
}
