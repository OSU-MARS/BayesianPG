using System;

namespace BayesianPG.ThreePG
{
    public class ThreePGStandTrajectory
    {
        public int Capacity { get; private set; }
        public DateTime From { get; set; }
        public int n_m { get; set; }

        public float[] AvailableSoilWater { get; private set; }
        // TODO public float[] DayLength { get; private set; }
        public float[] evapo_transp { get; private set; } // total evapotranspiration
        public float[] f_transp_scale { get; private set; }
        public float[] irrig_supl { get; private set; }
        public float[] prcp_runoff { get; private set; }
        public float[] conduct_soil { get; private set; }
        public float[] evapotra_soil { get; private set; }
        public TreeSpeciesTrajectory Species { get; private set; }

        public ThreePGStandTrajectory(string speciesName, DateTime from)
            : this(new string[] { speciesName }, from, Constant.DefaultTimestepCapacity)
        {
        }

        public ThreePGStandTrajectory(string[] speciesNames, DateTime from, DateTime to)
            : this(speciesNames, from, 12 * (to.Year - from.Year) + to.Month - from.Month + 1)
        {
            if (to < from)
            {
                throw new ArgumentException("Trajectory end date " + to.ToString("yyyy-MM") + " is before trajectory start date " + from.ToString("yyyy-MM") + ".");
            }
        }

        protected ThreePGStandTrajectory(string[] speciesNames, DateTime from, int capacity)
        {
            this.Capacity = capacity;
            this.From = from;
            this.n_m = 0;

            this.AvailableSoilWater = new float[capacity];
            // this.DayLength = new float[capacity];
            this.evapo_transp = new float[capacity];
            this.f_transp_scale = new float[capacity];
            this.irrig_supl = new float[capacity];
            this.prcp_runoff = new float[capacity];
            this.conduct_soil = new float[capacity];
            this.evapotra_soil = new float[capacity];
            this.Species = new(speciesNames, capacity);
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
