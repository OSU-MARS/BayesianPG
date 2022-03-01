using BayesianPG.ThreePG;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace BayesianPG.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "StandTrajectory")]
    public class WriteStandTrajectory : Cmdlet
    {
        [Parameter]
        public SwitchParameter ReferencePrecision;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? File;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public ThreePGStandTrajectory? Trajectory;

        public WriteStandTrajectory()
        {
            this.ReferencePrecision = false;
        }

        protected override void ProcessRecord()
        {
            Debug.Assert((this.File != null) && (this.Trajectory != null));
            if (this.Trajectory is not ThreePGStandTrajectory<float, int> trajectory)
            {
                throw new NotSupportedException("Data in stand trajectory is not of a recognized type.");
            }

            using FileStream stream = new(this.File, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
            using StreamWriter writer = new(stream, Encoding.UTF8); // callers assume UTF8, see remarks for StreamLengthSynchronizationInterval

            // write header
            string header = "date,species,age,stems_n,mort_stress,mort_thinn,basal_area,dbh,height,volume,gpp,npp_f,biom_foliage_debt,lai,biom_foliage,sla,biom_root,biom_stem,biom_tree,biom_tree_max," +
                "basal_area_prop,npp_fract_foliage,npp_fract_root,npp_fract_stem,alpha_c,f_age,f_calpha,f_cg,f_frost,f_nutr,f_phys,f_sw,f_tmp,f_tmp_gc,f_vpd,vpd_sp,epsilon_biom_stem,epsilon_gpp,epsilon_npp," +
                "aero_resist,canopy_cover,conduct_canopy,crown_length,crown_width,fracBB,gammaF,gammaN,prcp_interc,transp_veg,wood_density,wue,wue_transp,canopy_vol_frac,fi,lai_above,lai_sa_ratio,lambda_h,lambda_v,layer_id," +
                "asw,conduct_soil,evapotra_soil,evapo_transp,f_transp_scale,prcp_runoff,irrig_supl";
            if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                header += ",CVdbhDistribution,CVwsDistribution,height_rel,DWeibullScale,DWeibullShape,DWeibullLocation,wsWeibullScale,wsWeibullShape,wsWeibullLocation,DrelBiaspFS,DrelBiasheight,DrelBiasBasArea,DrelBiasLCL,DrelBiasCrowndiameter,wsrelBias";
            }
            if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
            {
                header += ",D13CNewPS,D13CTissue,InterCi";
            }
            if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                header += ",biom_incr_foliage,biom_incr_root,biom_incr_stem,biom_loss_foliage,biom_loss_root,volume_cum";
            }
            writer.WriteLine(header);


            // write trajectory
            this.WriteTrajectory(writer, trajectory);
        }

        private void WriteTrajectory(StreamWriter writer, ThreePGStandTrajectory<float, int> trajectory)
        {
            string precision1 = "0.0";
            string precision2 = "0.00";
            string precision3 = "0.000";
            string precision4 = "0.0000";
            string precision5 = "0.00000";
            string precision6 = "0.000000";
            if (this.ReferencePrecision)
            {
                precision1 = "0.00000";
                precision2 = "0.000000";
                precision3 = "0.0000000";
                precision4 = "0.00000000";
                precision5 = "0.000000000";
                precision6 = "0.0000000000";
            }

            for (int speciesIndex = 0; speciesIndex < trajectory.Species.n_sp; ++speciesIndex)
            {
                string species = trajectory.Species.Species[speciesIndex];
                DateTime yearAndMonth = trajectory.From;
                for (int timestepIndex = 0; timestepIndex < trajectory.MonthCount; ++timestepIndex)
                {
                    string line = yearAndMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "," +
                                  species + "," +
                                  // core stand trajectory and produtivity variables
                                  trajectory.Species.age[speciesIndex][timestepIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.stems_n[timestepIndex, speciesIndex].ToString(precision1, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.mort_stress[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.mort_thinn[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.basal_area[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.dbh[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.height[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.volume[timestepIndex, speciesIndex].ToString(precision1, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.GPP[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.NPP_f[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," + // net of biomass foliage debt paid
                                  trajectory.Species.biom_foliage_debt[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lai[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_foliage[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.SLA[speciesIndex][timestepIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_root[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_stem[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_tree[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_tree_max[timestepIndex, speciesIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.basal_area_prop[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.npp_fract_foliage[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.npp_fract_root[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.npp_fract_stem[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  // top level calculation chain for output intercomparison:
                                  //   (3-PGmix: lai -> lai_above -> vpd_sp -> f_vpd)
                                  //   f_phys = f_vpd * f_sw * f_age
                                  //   alphaC = alphaCx * f_nutr * f_tmp * f_frost * f_calpha * f_phys
                                  //   epsilon = gDM_mol * molPAR_MJ * alphaC - later overwritten in state, though
                                  //   gpp = epsilon * apar / 100
                                  //   npp = Y * gpp - biom_foliage_debt
                                  trajectory.Species.alpha_c[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_age[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_calpha[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_cg[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_frost[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_nutr[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_phys[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_sw[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_tmp[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_vpd[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.VPD_sp[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.epsilon_biom_stem[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.epsilon_gpp[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.epsilon_npp[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.aero_resist[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.canopy_cover[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  // conduct_canopy = gC * lai_per * f_phys * f_tmp_gc * f_cg
                                  trajectory.Species.conduct_canopy[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.crown_length[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.crown_width[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.fracBB[speciesIndex][timestepIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.gammaF[speciesIndex][timestepIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.gammaN[speciesIndex][timestepIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  // trajectory.Species.pFS[speciesIndex][timestepIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.prcp_interc[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.transp_veg[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.wood_density[speciesIndex][timestepIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.WUE[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.WUEtransp[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.canopy_vol_frac[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.fi[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lai_above[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lai_sa_ratio[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lambda_h[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lambda_v[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.layer_id[timestepIndex, speciesIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                  trajectory.AvailableSoilWater[timestepIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.conduct_soil[timestepIndex].ToString(precision6, CultureInfo.InvariantCulture) + "," +
                                  trajectory.evapotra_soil[timestepIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.evapo_transp[timestepIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.f_transp_scale[timestepIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.prcp_runoff[timestepIndex].ToString(precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.irrig_supl[timestepIndex].ToString(precision1, CultureInfo.InvariantCulture);
                    writer.Write(line);

                    if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
                    {
                        string bias = "," +
                            trajectory.Species.CVdbhDistribution[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.CVwsDistribution[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.height_rel[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DWeibullScale[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DWeibullShape[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DWeibullLocation[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsWeibullScale[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsWeibullShape[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsWeibullLocation[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiaspFS[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasheight[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasBasArea[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasLCL[timestepIndex, speciesIndex].ToString(precision6, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasCrowndiameter[timestepIndex, speciesIndex].ToString(precision6, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsrelBias[timestepIndex, speciesIndex].ToString(precision6, CultureInfo.InvariantCulture);
                        writer.Write(bias);
                    }

                    if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
                    {
                        string d13c = "," +
                            trajectory.Species.D13CNewPS[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.D13CTissue[timestepIndex, speciesIndex].ToString(precision3, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.InterCi[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture);
                        writer.Write(d13c);
                    }

                    if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
                    {
                        string extended = "," +
                            trajectory.Species.biom_incr_foliage[timestepIndex, speciesIndex].ToString(precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_incr_root[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_incr_stem[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_loss_foliage[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_loss_root[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.volume_cum[timestepIndex, speciesIndex].ToString(precision4, CultureInfo.InvariantCulture);
                        writer.Write(extended);
                    }

                    writer.Write(writer.NewLine);

                    // move to next month
                    yearAndMonth = yearAndMonth.AddMonths(1);
                }
            }
        }
    }
}
