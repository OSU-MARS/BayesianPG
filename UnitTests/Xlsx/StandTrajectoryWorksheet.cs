using BayesianPG.Extensions;
using BayesianPG.ThreePG;
using BayesianPG.Xlsx;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace BayesianPG.Test.Xlsx
{
    public class StandTrajectoryWorksheet : XlsxWorksheet<StandTrajectoryHeader>
    {
        private ThreePGStandTrajectory<float, int>? trajectory;
        
        public StandTrajectoryWorksheet()
        {
            this.trajectory = null;
        }

        public ThreePGStandTrajectory<float, int> Trajectory
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
            string dateAsString = row.Row[this.Header.date];
            DateTime date;
            if (dateAsString.Contains('-'))
            {
                date = DateTime.ParseExact(dateAsString, "yyyy-MM", CultureInfo.InvariantCulture);
            }
            else
            {
                date = DateTimeExtensions.FromExcel(Int32.Parse(dateAsString));
            }

            // ensure this row fits in trajectory
            if (this.trajectory == null)
            {
                // allocate trajectory on the assumption of a single tree species
                this.trajectory = new(row.Row[this.Header.species], date, this.Header.ColumnGroups);
            }
            else if (date < trajectory.From)
            {
                throw new XmlException("Date " + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " is before trajectory start month " + trajectory.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".", null, row.Number, this.Header.date + 1);
            }

            // find or allocate species
            string species = row.Row[this.Header.species];
            int speciesIndex = this.Trajectory.Species.Species.FindIndex(species);
            if (speciesIndex == -1)
            {
                // reshape trajectory to multiple species
                // For now it's assumed all species have a row for each timestep.
                this.Trajectory.Species.AllocateSpecies([ species ]);
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
                this.Trajectory.AvailableSoilWater[timestepIndex] = Single.Parse(row.Row[this.Header.asw], CultureInfo.InvariantCulture);
                // this.Trajectory.DayLength[timestepIndex] = Single.Parse(row.Row[this.Header.DayLength], CultureInfo.InvariantCulture);
                this.Trajectory.conduct_soil[timestepIndex] = Single.Parse(row.Row[this.Header.conduct_soil], CultureInfo.InvariantCulture);
                this.Trajectory.evapo_transp[timestepIndex] = Single.Parse(row.Row[this.Header.evapo_transp], CultureInfo.InvariantCulture);
                this.Trajectory.evapotra_soil[timestepIndex] = Single.Parse(row.Row[this.Header.evapotra_soil], CultureInfo.InvariantCulture);
                this.Trajectory.f_transp_scale[timestepIndex] = Single.Parse(row.Row[this.Header.f_transp_scale], CultureInfo.InvariantCulture);
                this.Trajectory.irrig_supl[timestepIndex] = Single.Parse(row.Row[this.Header.irrig_supl], CultureInfo.InvariantCulture);
                this.Trajectory.prcp_runoff[timestepIndex] = Single.Parse(row.Row[this.Header.prcp_runoff], CultureInfo.InvariantCulture);
            }

            this.Trajectory.Species.age[speciesIndex][timestepIndex] = Single.Parse(row.Row[this.Header.age], CultureInfo.InvariantCulture);
            this.Trajectory.Species.age_m[speciesIndex][timestepIndex] = this.Trajectory.Species.age[speciesIndex][timestepIndex] - 1.0F / 12.0F;
            this.Trajectory.Species.fracBB[speciesIndex][timestepIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.fracBB]);
            this.Trajectory.Species.gammaF[speciesIndex][timestepIndex] = Single.Parse(row.Row[this.Header.gammaF], CultureInfo.InvariantCulture);
            this.Trajectory.Species.gammaN[speciesIndex][timestepIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.gammaN]);
            this.Trajectory.Species.SLA[speciesIndex][timestepIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.sla]);
            this.Trajectory.Species.wood_density[speciesIndex][timestepIndex] = Single.Parse(row.Row[this.Header.wood_density], CultureInfo.InvariantCulture);

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
            this.Trajectory.Species.aero_resist[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.aero_resist]);
            this.Trajectory.Species.alpha_c[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.alpha_c]);
            this.Trajectory.Species.biom_foliage_debt[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_foliage_debt], CultureInfo.InvariantCulture);
            this.Trajectory.Species.conduct_canopy[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.conduct_canopy]);
            this.Trajectory.Species.crown_length[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.crown_length]);
            this.Trajectory.Species.crown_width[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.crown_width]);
            this.Trajectory.Species.epsilon_biom_stem[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.epsilon_biom_stem]);
            this.Trajectory.Species.epsilon_gpp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.epsilon_gpp]);
            this.Trajectory.Species.epsilon_npp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.epsilon_npp]);
            // this.Trajectory.Species.gC[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.gC]);
            this.Trajectory.Species.GPP[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.gpp]);
            this.Trajectory.Species.NPP_f[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.npp_f]);
            this.Trajectory.Species.npp_fract_foliage[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.npp_fract_foliage]);
            this.Trajectory.Species.npp_fract_root[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.npp_fract_root]);
            this.Trajectory.Species.npp_fract_stem[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.npp_fract_stem]);
            this.Trajectory.Species.VPD_sp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.vpd_sp]);
            
            // stand
            this.Trajectory.Species.basal_area[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.basal_area], CultureInfo.InvariantCulture);
            this.Trajectory.Species.basal_area_prop[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.basal_area_prop], CultureInfo.InvariantCulture);
            this.Trajectory.Species.biom_foliage[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_foliage], CultureInfo.InvariantCulture);
            this.Trajectory.Species.biom_root[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_root], CultureInfo.InvariantCulture);
            this.Trajectory.Species.biom_stem[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_stem], CultureInfo.InvariantCulture);
            this.Trajectory.Species.biom_tree[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_tree], CultureInfo.InvariantCulture);
            this.Trajectory.Species.biom_tree_max[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_tree_max], CultureInfo.InvariantCulture);
            this.Trajectory.Species.dbh[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.dbh], CultureInfo.InvariantCulture);
            this.Trajectory.Species.height[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.height], CultureInfo.InvariantCulture);
            this.Trajectory.Species.lai[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.lai], CultureInfo.InvariantCulture);
            this.Trajectory.Species.stems_n[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.stems_n], CultureInfo.InvariantCulture);
            // this.Trajectory.Species.stems_n_ha[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.stems_n_ha], CultureInfo.InvariantCulture);
            this.Trajectory.Species.volume[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.volume], CultureInfo.InvariantCulture);

            // water
            this.Trajectory.Species.prcp_interc[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.prcp_interc]);
            this.Trajectory.Species.transp_veg[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.transp_veg]);
            this.Trajectory.Species.WUE[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wue]);
            this.Trajectory.Species.WUEtransp[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wue_transp]);

            // 3-PGmix
            this.Trajectory.Species.canopy_vol_frac[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.canopy_vol_frac]);
            this.Trajectory.Species.fi[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.fi]);
            this.Trajectory.Species.lai_above[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.lai_above]);
            this.Trajectory.Species.lai_sa_ratio[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.lai_sa_ratio]);
            this.Trajectory.Species.lambda_h[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.lambda_h]);
            this.Trajectory.Species.lambda_v[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.lambda_v]);
            this.Trajectory.Species.layer_id[timestepIndex, speciesIndex] = Int32.Parse(row.Row[this.Header.layer_id]);

            // 3-PGpjs and δ¹³C
            this.Trajectory.Species.canopy_cover[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.canopy_cover]);

            // bias correction
            if (this.Header.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                this.Trajectory.Species.CVdbhDistribution[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.CVdbhDistribution]);
                this.Trajectory.Species.CVwsDistribution[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.CVwsDistribution]);
                this.Trajectory.Species.height_rel[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.height_rel]);
                this.Trajectory.Species.DWeibullScale[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DWeibullScale]);
                this.Trajectory.Species.DWeibullShape[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DWeibullShape]);
                this.Trajectory.Species.DWeibullLocation[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DWeibullLocation]);
                this.Trajectory.Species.wsWeibullScale[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wsWeibullScale]);
                this.Trajectory.Species.wsWeibullShape[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wsWeibullShape]);
                this.Trajectory.Species.wsWeibullLocation[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wsWeibullLocation]);
                this.Trajectory.Species.DrelBiaspFS[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DrelBiaspFS]);
                this.Trajectory.Species.DrelBiasheight[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DrelBiasheight]);
                this.Trajectory.Species.DrelBiasBasArea[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DrelBiasBasArea]);
                this.Trajectory.Species.DrelBiasLCL[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DrelBiasLCL]);
                this.Trajectory.Species.DrelBiasCrowndiameter[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.DrelBiasCrowndiameter]);
                this.Trajectory.Species.wsrelBias[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.wsrelBias]);
            }

            // δ¹³C
            if (this.Header.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
            {
                this.Trajectory.Species.D13CNewPS[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.D13CNewPS]);
                this.Trajectory.Species.D13CTissue[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.D13CTissue]);
                this.Trajectory.Species.InterCi[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.InterCi]);
            }

            // extended columns
            if (this.Header.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                this.Trajectory.Species.biom_incr_foliage[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_incr_foliage], CultureInfo.InvariantCulture);
                this.Trajectory.Species.biom_incr_root[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_incr_root], CultureInfo.InvariantCulture);
                this.Trajectory.Species.biom_incr_stem[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_incr_stem], CultureInfo.InvariantCulture);
                this.Trajectory.Species.biom_loss_foliage[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_loss_foliage], CultureInfo.InvariantCulture);
                this.Trajectory.Species.biom_loss_root[timestepIndex, speciesIndex] = Single.Parse(row.Row[this.Header.biom_loss_root], CultureInfo.InvariantCulture);
                this.Trajectory.Species.volume_cum[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.volume_cum]);
            }

            // not currently tested
            // apar
            // co2
            // day_length
            // delta13c
            // this.Trajectory.Species.Gc_mol[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.Gc_mol]);
            // this.Trajectory.Species.Gw_mol[timestepIndex, speciesIndex] = StandTrajectoryWorksheet.ParseSingle(row.Row[this.Header.Gw_mol]);
            // pFS
            // prcp_interc_frac
            // solar_rad
            // tmp_ave
            // tmp_max
            // tmp_min
            // volume_change
            // volume_extracted
            // volume_mai
            // vpd_day
            // water_runoff_pooled
        }

        protected static float ParseSingle(string floatAsString)
        {
            if (String.IsNullOrEmpty(floatAsString))
            {
                return Single.NaN;
            }

            return Single.Parse(floatAsString, CultureInfo.InvariantCulture);
        }
    }
}
