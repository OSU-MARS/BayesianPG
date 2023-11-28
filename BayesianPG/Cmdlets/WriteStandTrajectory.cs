using BayesianPG.Extensions;
using BayesianPG.ThreePG;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace BayesianPG.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "StandTrajectory")]
    public class WriteStandTrajectory : Cmdlet
    {
        private string precision1;
        private string precision2;
        private string precision3;
        private string precision4;
        private string precision5;
        private string precision6;

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
            this.precision1 = "0.0";
            this.precision2 = "0.00";
            this.precision3 = "0.000";
            this.precision4 = "0.0000";
            this.precision5 = "0.00000";
            this.precision6 = "0.000000";

            this.ReferencePrecision = false;
        }

        protected override void ProcessRecord()
        {
            Debug.Assert((this.File != null) && (this.Trajectory != null));

            using FileStream stream = new(this.File, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
            using StreamWriter writer = new(stream, Encoding.UTF8); // callers assume UTF8, see remarks for StreamLengthSynchronizationInterval

            // write header
            string header = "date,species,trajectory,age,stems_n,mort_stress,mort_thinn,basal_area,dbh,height,volume,gpp,npp_f,biom_foliage_debt,lai,biom_foliage,sla,biom_root,biom_stem,biom_tree,biom_tree_max," +
                "basal_area_prop,npp_fract_foliage,npp_fract_root,npp_fract_stem,alpha_c,f_age,f_calpha,f_cg,f_frost,f_nutr,f_phys,f_sw,f_tmp,f_tmp_gc,f_vpd,vpd_sp,epsilon_biom_stem,epsilon_gpp,epsilon_npp," +
                "aero_resist,canopy_cover,conduct_canopy,crown_length,crown_width,fracBB,gammaF,gammaN,prcp_interc,transp_veg,wood_density,wue,wue_transp,canopy_vol_frac,fi,lai_above,lai_sa_ratio,lambda_h,lambda_v,layer_id," +
                "asw,conduct_soil,evapotra_soil,evapo_transp,f_transp_scale,prcp_runoff,irrig_supl";
            if (this.Trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                header += ",CVdbhDistribution,CVwsDistribution,height_rel,DWeibullScale,DWeibullShape,DWeibullLocation,wsWeibullScale,wsWeibullShape,wsWeibullLocation,DrelBiaspFS,DrelBiasheight,DrelBiasBasArea,DrelBiasLCL,DrelBiasCrowndiameter,wsrelBias";
            }
            if (this.Trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
            {
                header += ",D13CNewPS,D13CTissue,InterCi";
            }
            if (this.Trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                header += ",biom_incr_foliage,biom_incr_root,biom_incr_stem,biom_loss_foliage,biom_loss_root,volume_cum";
            }
            writer.WriteLine(header);

            // write trajectory
            if (this.ReferencePrecision)
            {
                this.precision1 = "0.00000";
                this.precision2 = "0.000000";
                this.precision3 = "0.0000000";
                this.precision4 = "0.00000000";
                this.precision5 = "0.000000000";
                this.precision6 = "0.0000000000";
            }

            if (this.Trajectory is ThreePGStandTrajectory<float, int> trajectory)
            {
                this.WriteTrajectory(writer, trajectory);
            }
            else if (this.Trajectory is ThreePGStandTrajectory<Vector128<float>, Vector128<int>> trajectory128)
            {
                this.WriteTrajectory(writer, trajectory128);
            }
            else
            {
                throw new NotSupportedException("Data in stand trajectory is not of a recognized type.");
            }
        }

        private void WriteTrajectory(StreamWriter writer, ThreePGStandTrajectory<float, int> trajectory)
        {
            DateTime yearAndMonth = trajectory.From;
            for (int timestepIndex = 0; timestepIndex < trajectory.MonthCount; ++timestepIndex)
            {
                for (int speciesIndex = 0; speciesIndex < trajectory.Species.n_sp; ++speciesIndex)
                {
                    string line = yearAndMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.Species[speciesIndex] + "," +
                                  "0," +
                                  // core stand trajectory and produtivity variables
                                  trajectory.Species.age[speciesIndex][timestepIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.stems_n[timestepIndex, speciesIndex].ToString(this.precision1, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.mort_stress[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.mort_thinn[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.basal_area[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.dbh[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.height[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.volume[timestepIndex, speciesIndex].ToString(this.precision1, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.GPP[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.NPP_f[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," + // net of biomass foliage debt paid
                                  trajectory.Species.biom_foliage_debt[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lai[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_foliage[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.SLA[speciesIndex][timestepIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_root[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_stem[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_tree[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.biom_tree_max[timestepIndex, speciesIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.basal_area_prop[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.npp_fract_foliage[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.npp_fract_root[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.npp_fract_stem[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  // top level calculation chain for output intercomparison:
                                  //   (3-PGmix: lai -> lai_above -> vpd_sp -> f_vpd)
                                  //   f_phys = f_vpd * f_sw * f_age
                                  //   alphaC = alphaCx * f_nutr * f_tmp * f_frost * f_calpha * f_phys
                                  //   epsilon = gDM_mol * molPAR_MJ * alphaC - later overwritten in state, though
                                  //   gpp = epsilon * apar / 100
                                  //   npp = Y * gpp - biom_foliage_debt
                                  trajectory.Species.alpha_c[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_age[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_calpha[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_cg[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_frost[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_nutr[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_phys[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_sw[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_tmp[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.f_vpd[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.VPD_sp[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.epsilon_biom_stem[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.epsilon_gpp[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.epsilon_npp[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.aero_resist[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.canopy_cover[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  // conduct_canopy = gC * lai_per * f_phys * f_tmp_gc * f_cg
                                  trajectory.Species.conduct_canopy[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.crown_length[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.crown_width[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.fracBB[speciesIndex][timestepIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.gammaF[speciesIndex][timestepIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.gammaN[speciesIndex][timestepIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  // trajectory.Species.pFS[speciesIndex][timestepIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.prcp_interc[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.transp_veg[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.wood_density[speciesIndex][timestepIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.WUE[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.WUEtransp[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.canopy_vol_frac[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.fi[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lai_above[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lai_sa_ratio[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lambda_h[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.lambda_v[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.Species.layer_id[timestepIndex, speciesIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                  trajectory.AvailableSoilWater[timestepIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.conduct_soil[timestepIndex].ToString(this.precision6, CultureInfo.InvariantCulture) + "," +
                                  trajectory.evapotra_soil[timestepIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                                  trajectory.evapo_transp[timestepIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.f_transp_scale[timestepIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.prcp_runoff[timestepIndex].ToString(this.precision2, CultureInfo.InvariantCulture) + "," +
                                  trajectory.irrig_supl[timestepIndex].ToString(this.precision1, CultureInfo.InvariantCulture);
                    writer.Write(line);

                    if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
                    {
                        string bias = "," +
                            trajectory.Species.CVdbhDistribution[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.CVwsDistribution[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.height_rel[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DWeibullScale[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DWeibullShape[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DWeibullLocation[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsWeibullScale[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsWeibullShape[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsWeibullLocation[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiaspFS[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasheight[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasBasArea[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasLCL[timestepIndex, speciesIndex].ToString(this.precision6, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.DrelBiasCrowndiameter[timestepIndex, speciesIndex].ToString(this.precision6, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.wsrelBias[timestepIndex, speciesIndex].ToString(this.precision6, CultureInfo.InvariantCulture);
                        writer.Write(bias);
                    }

                    if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
                    {
                        string d13c = "," +
                            trajectory.Species.D13CNewPS[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.D13CTissue[timestepIndex, speciesIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.InterCi[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture);
                        writer.Write(d13c);
                    }

                    if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
                    {
                        string extended = "," +
                            trajectory.Species.biom_incr_foliage[timestepIndex, speciesIndex].ToString(this.precision5, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_incr_root[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_incr_stem[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_loss_foliage[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.biom_loss_root[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture) + "," +
                            trajectory.Species.volume_cum[timestepIndex, speciesIndex].ToString(this.precision4, CultureInfo.InvariantCulture);
                        writer.Write(extended);
                    }

                    writer.Write(writer.NewLine);
                }

                // move to next month
                yearAndMonth = yearAndMonth.AddMonths(1);
            }
        }

        private void WriteTrajectory(StreamWriter writer, ThreePGStandTrajectory<Vector128<float>, Vector128<int>> trajectory)
        {
            DateTime yearAndMonth = trajectory.From;
            for (int timestepIndex = 0; timestepIndex < trajectory.MonthCount; ++timestepIndex)
            {
                for (int speciesIndex = 0; speciesIndex < trajectory.Species.n_sp; ++speciesIndex)
                {
                    string linePrefix = yearAndMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "," +
                                        trajectory.Species.Species[speciesIndex];
                    for (byte element = 0; element < 4; ++element)
                    {
                        string line = linePrefix + "," +
                                      element.ToString(CultureInfo.InvariantCulture) + "," +
                                      // core stand trajectory and produtivity variables
                                      trajectory.Species.age[speciesIndex][timestepIndex].ToString(this.precision3, CultureInfo.InvariantCulture) + "," +
                                      trajectory.Species.stems_n[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision1) + "," +
                                      trajectory.Species.mort_stress[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.mort_thinn[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.basal_area[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.dbh[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.height[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.volume[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision1) + "," +
                                      trajectory.Species.GPP[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                      trajectory.Species.NPP_f[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," + // net of biomass foliage debt paid
                                      trajectory.Species.biom_foliage_debt[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                      trajectory.Species.lai[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                      trajectory.Species.biom_foliage[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                      trajectory.Species.SLA[speciesIndex][timestepIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                      trajectory.Species.biom_root[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.biom_stem[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.biom_tree[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.biom_tree_max[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.Species.basal_area_prop[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                      trajectory.Species.npp_fract_foliage[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      trajectory.Species.npp_fract_root[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.npp_fract_stem[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      // top level calculation chain for output intercomparison:
                                      //   (3-PGmix: lai -> lai_above -> vpd_sp -> f_vpd)
                                      //   f_phys = f_vpd * f_sw * f_age
                                      //   alphaC = alphaCx * f_nutr * f_tmp * f_frost * f_calpha * f_phys
                                      //   epsilon = gDM_mol * molPAR_MJ * alphaC - later overwritten in state, though
                                      //   gpp = epsilon * apar / 100
                                      //   npp = Y * gpp - biom_foliage_debt
                                      trajectory.Species.alpha_c[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      trajectory.Species.f_age[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_calpha[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_cg[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_frost[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_nutr[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_phys[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_sw[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_tmp[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.f_vpd[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.VPD_sp[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.epsilon_biom_stem[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.epsilon_gpp[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.epsilon_npp[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.aero_resist[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      trajectory.Species.canopy_cover[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      // conduct_canopy = gC * lai_per * f_phys * f_tmp_gc * f_cg
                                      trajectory.Species.conduct_canopy[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      trajectory.Species.crown_length[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      trajectory.Species.crown_width[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                      trajectory.Species.fracBB[speciesIndex][timestepIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.gammaF[speciesIndex][timestepIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.gammaN[speciesIndex][timestepIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      // trajectory.Species.pFS[speciesIndex][timestepIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.prcp_interc[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.transp_veg[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.wood_density[speciesIndex][timestepIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.WUE[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.WUEtransp[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.canopy_vol_frac[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.fi[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.lai_above[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.lai_sa_ratio[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.lambda_h[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.lambda_v[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.Species.layer_id[timestepIndex, speciesIndex].ExtractToStringInvariant(element) + "," +
                                      trajectory.AvailableSoilWater[timestepIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.conduct_soil[timestepIndex].ExtractToStringInvariant(element, this.precision6) + "," +
                                      trajectory.evapotra_soil[timestepIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                      trajectory.evapo_transp[timestepIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.f_transp_scale[timestepIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.prcp_runoff[timestepIndex].ExtractToStringInvariant(element, this.precision2) + "," +
                                      trajectory.irrig_supl[timestepIndex].ExtractToStringInvariant(element, this.precision1);
                        writer.Write(line);

                        if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
                        {
                            string bias = "," +
                                trajectory.Species.CVdbhDistribution[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.CVwsDistribution[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.height_rel[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.DWeibullScale[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.DWeibullShape[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.DWeibullLocation[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.wsWeibullScale[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.wsWeibullShape[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.wsWeibullLocation[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.DrelBiaspFS[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                trajectory.Species.DrelBiasheight[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                trajectory.Species.DrelBiasBasArea[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                trajectory.Species.DrelBiasLCL[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision6) + "," +
                                trajectory.Species.DrelBiasCrowndiameter[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision6) + "," +
                                trajectory.Species.wsrelBias[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision6);
                            writer.Write(bias);
                        }

                        if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.D13C))
                        {
                            string d13c = "," +
                                trajectory.Species.D13CNewPS[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                trajectory.Species.D13CTissue[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision3) + "," +
                                trajectory.Species.InterCi[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5);
                            writer.Write(d13c);
                        }

                        if (trajectory.ColumnGroups.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
                        {
                            string extended = "," +
                                trajectory.Species.biom_incr_foliage[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision5) + "," +
                                trajectory.Species.biom_incr_root[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.biom_incr_stem[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.biom_loss_foliage[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.biom_loss_root[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4) + "," +
                                trajectory.Species.volume_cum[timestepIndex, speciesIndex].ExtractToStringInvariant(element, this.precision4);
                            writer.Write(extended);
                        }

                        writer.Write(writer.NewLine);
                    }
                }

                // move to next month
                yearAndMonth = yearAndMonth.AddMonths(1);
            }
        }
    }
}
