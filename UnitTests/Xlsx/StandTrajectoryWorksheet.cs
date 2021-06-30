using BayesianPG.ThreePG;
using BayesianPG.Xlsx;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Test.Xlsx
{
    public class StandTrajectoryWorksheet : XlsxWorksheet<StandTrajectoryWorksheetHeader>
    {
        private ThreePGStandTrajectory? trajectory;
        
        public StandTrajectoryWorksheet()
        {
            this.trajectory = null;
        }

        public ThreePGStandTrajectory Trajectory
        {
            get
            {
                Debug.Assert(this.trajectory != null);
                return this.trajectory;
            }
        }

        // for now, values are assumed correct and not validated
        public override void ParseRow(XlsxRow row)
        {
            // convert from Excel day number to DateTime
            DateTime date = new DateTime(1900, 1, 1).AddDays(Int32.Parse(row.Row[this.Header.date]) - 2);

            // ensure this row fits in trajectory
            if (this.trajectory == null)
            {
                // allocate trajectory on the assumption of a single tree species
                this.trajectory = new ThreePGStandTrajectory(row.Row[this.Header.species], date);
            }
            else if (date < trajectory.From)
            {
                throw new XmlException("Date " + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " is before trajectory start month " + trajectory.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".", null, row.Number, this.Header.date + 1);
            }

            // find or allocate species
            string species = row.Row[this.Header.species];
            int speciesIndex = this.Trajectory.Species.Name.FindIndex(species);
            if (speciesIndex == -1)
            {
                // reshape trajectory to multiple species
                // For now it's assumed all species have a row for each timestep.
                this.Trajectory.Species.AllocateSpecies(new string[] { species });
                speciesIndex = this.Trajectory.Species.n_sp - 1;
            }

            // parse row
            int timestepIndex = 12 * (date.Year - trajectory.From.Year) + date.Month - trajectory.From.Month;
            if (timestepIndex >= this.Trajectory.Capacity)
            {
                this.Trajectory.AllocateDecade();
            }
            if (speciesIndex == 0)
            {
                // for now, assume stand variables are repeated identically for all timesteps with all species
                // Therefore, they only need to be parsed for the first species encountered.
                this.Trajectory.AvailableSoilWater[timestepIndex] = Single.Parse(row.Row[this.Header.asw]);
                // this.Trajectory.DayLength[timestepIndex] = Single.Parse(row.Row[this.Header.DayLength]);
                this.Trajectory.evapo_transp[timestepIndex] = Single.Parse(row.Row[this.Header.evapo_transp]);
                this.Trajectory.irrig_supl[timestepIndex] = Single.Parse(row.Row[this.Header.irrig_supl]);
                this.Trajectory.prcp_runoff[timestepIndex] = Single.Parse(row.Row[this.Header.prcp_runoff]);
                this.Trajectory.conduct_soil[timestepIndex] = Single.Parse(row.Row[this.Header.conduct_soil]);
                this.Trajectory.evapotra_soil[timestepIndex] = Single.Parse(row.Row[this.Header.evapotra_soil]);
            }

            this.Trajectory.Species.age[speciesIndex][timestepIndex] = Single.Parse(row.Row[this.Header.age]);
            this.Trajectory.Species.age_m[speciesIndex][timestepIndex] = this.Trajectory.Species.age[speciesIndex][timestepIndex] - 1.0F / 12.0F;
            this.Trajectory.Species.fracBB[speciesIndex][timestepIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.fracBB]);
            this.Trajectory.Species.gammaF[speciesIndex][timestepIndex] = Single.Parse(row.Row[this.Header.gammaF]);
            this.Trajectory.Species.gammaN[speciesIndex][timestepIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.gammaN]);
            this.Trajectory.Species.SLA[speciesIndex][timestepIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.sla]);
            this.Trajectory.Species.wood_density[speciesIndex][timestepIndex] = Single.Parse(row.Row[this.Header.wood_density]);

            // modifiers
            this.Trajectory.Species.f_age[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_age]);
            this.Trajectory.Species.f_calpha[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_calpha]);
            this.Trajectory.Species.f_cg[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_cg]);
            this.Trajectory.Species.f_frost[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_frost]);
            this.Trajectory.Species.f_nutr[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_nutr]);
            this.Trajectory.Species.f_phys[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_phys]);
            this.Trajectory.Species.f_sw[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_sw]);
            this.Trajectory.Species.f_tmp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_tmp]);
            this.Trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_tmp_gc]);
            this.Trajectory.Species.f_vpd[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.f_vpd]);

            // growth
            this.Trajectory.Species.alpha_c[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.alpha_c]);
            this.Trajectory.Species.epsilon_gpp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.epsilon_gpp]);
            this.Trajectory.Species.epsilon_npp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.epsilon_npp]);
            this.Trajectory.Species.GPP[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.gpp]);
            this.Trajectory.Species.NPP_f[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.npp]);
            this.Trajectory.Species.conduct_canopy[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.conduct_canopy]);
            this.Trajectory.Species.WUE[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wue]);
            this.Trajectory.Species.WUEtransp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wue_transp]);
            this.Trajectory.Species.VPD_sp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.vpd_sp]);
            
            // stand
            this.Trajectory.Species.basal_area[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.basal_area]);
            this.Trajectory.Species.biom_foliage[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_foliage]);
            this.Trajectory.Species.biom_root[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_root]);
            this.Trajectory.Species.biom_stem[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_stem]);
            this.Trajectory.Species.dbh[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.dbh]);
            this.Trajectory.Species.height[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.height]);
            this.Trajectory.Species.lai[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.lai]);
            this.Trajectory.Species.stems_n[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.stems_n]);
            this.Trajectory.Species.volume[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.volume]);
            this.Trajectory.Species.volume_cum[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.volume_cum]);
        }

        protected static float ParseSingle(string floatAsString)
        {
            if (String.IsNullOrEmpty(floatAsString))
            {
                return Single.NaN;
            }

            return Single.Parse(floatAsString);
        }
    }
}
