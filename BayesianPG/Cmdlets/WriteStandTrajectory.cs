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
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? File;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public ThreePGStandTrajectory? Trajectory;

        protected override void ProcessRecord()
        {
            Debug.Assert((this.File != null) && (this.Trajectory != null));
            if ((this.Trajectory.Species.n_sp < 1) || (this.Trajectory.Species.n_sp > 2))
            {
                throw new NotSupportedException("Stand has " + this.Trajectory.Species.n_sp + " species.");
            }

            using FileStream stream = new(this.File!, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
            using StreamWriter writer = new(stream, Encoding.UTF8); // callers assume UTF8, see remarks for StreamLengthSynchronizationInterval

            string header = "date,species,age,stems_n,mort_stress,mort_thinn,basal_area,dbh,height,volume,gpp,npp_f,biom_foliage_debt,lai,biom_foliage,sla,biom_root,biom_stem,biom_tree,biom_tree_max," + 
                "basal_area_prop,npp_fract_foliage,npp_fract_root,npp_fract_stem,alpha_c,f_age,f_calpha,f_cg,f_frost,f_nutr,f_phys,f_sw,f_tmp,f_tmp_gc,f_vpd,vpd_sp,epsilon_biom_stem,epsilon_gpp,epsilon_npp," +
                "aero_resist,canopy_cover,conduct_canopy,crown_length,crown_width,gammaF,prcp_interc,transp_veg,wue,wue_transp,canopy_vol_frac,fi,lai_above,lai_sa_ratio,lambda_h,lambda_v,layer_id," + 
                "asw,conduct_soil,evapotra_soil,evapo_transp,f_transp_scale,prcp_runoff,irrig_supl," +
                "CVdbhDistribution,CVwsDistribution,height_rel,DWeibullScale,DWeibullShape,DWeibullLocation,wsWeibullScale,wsWeibullShape,wsWeibullLocation,DrelBiaspFS,DrelBiasheight,DrelBiasBasArea,DrelBiasLCL,DrelBiasCrowndiameter,wsrelBias";
            writer.WriteLine(header);

            for (int speciesIndex = 0; speciesIndex < this.Trajectory.Species.n_sp; ++speciesIndex)
            {
                string species = this.Trajectory.Species.Name[speciesIndex];
                DateTime yearAndMonth = this.Trajectory.From;
                for (int timestepIndex = 0; timestepIndex < this.Trajectory.n_m; ++timestepIndex)
                {
                    string line = yearAndMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "," +
                                  species + "," +
                                  // core stand trajectory and produtivity variables
                                  this.Trajectory.Species.age[speciesIndex][timestepIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.stems_n[timestepIndex, speciesIndex].ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.mort_stress[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.mort_thinn[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.basal_area[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.dbh[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.height[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.volume[timestepIndex, speciesIndex].ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.GPP[timestepIndex, speciesIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.NPP_f[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," + // net of biomass foliage debt paid
                                  this.Trajectory.Species.biom_foliage_debt[timestepIndex, speciesIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.lai[timestepIndex, speciesIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.biom_foliage[timestepIndex, speciesIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.SLA[speciesIndex][timestepIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.biom_root[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.biom_stem[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.biom_tree[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.biom_tree_max[timestepIndex, speciesIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.basal_area_prop[timestepIndex, speciesIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.npp_fract_foliage[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.npp_fract_root[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.npp_fract_stem[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  // top level calculation chain for output intercomparison:
                                  //   (3-PGmix: lai -> lai_above -> vpd_sp -> f_vpd)
                                  //   f_phys = f_vpd * f_sw * f_age
                                  //   alphaC = alphaCx * f_nutr * f_tmp * f_frost * f_calpha * f_phys
                                  //   epsilon = gDM_mol * molPAR_MJ * alphaC - later overwritten in state, though
                                  //   gpp = epsilon * apar / 100
                                  //   npp = Y * gpp - biom_foliage_debt
                                  this.Trajectory.Species.alpha_c[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_age[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_calpha[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_cg[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_frost[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_nutr[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_phys[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_sw[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_tmp[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_tmp_gc[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.f_vpd[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.VPD_sp[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.epsilon_biom_stem[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.epsilon_gpp[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.epsilon_npp[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.aero_resist[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.canopy_cover[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  // conduct_canopy = gC * lai_per * f_phys * f_tmp_gc * f_cg
                                  this.Trajectory.Species.conduct_canopy[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.crown_length[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.crown_width[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.gammaF[speciesIndex][timestepIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.prcp_interc[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.transp_veg[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.WUE[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.WUEtransp[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.canopy_vol_frac[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.fi[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.lai_above[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.lai_sa_ratio[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.lambda_h[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.lambda_v[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.layer_id[timestepIndex, speciesIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.AvailableSoilWater[timestepIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.conduct_soil[timestepIndex].ToString("0.000000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.evapotra_soil[timestepIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.evapo_transp[timestepIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.f_transp_scale[timestepIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.prcp_runoff[timestepIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.irrig_supl[timestepIndex].ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.CVdbhDistribution[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.CVwsDistribution[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.height_rel[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DWeibullScale[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DWeibullShape[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DWeibullLocation[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.wsWeibullScale[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.wsWeibullShape[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.wsWeibullLocation[timestepIndex, speciesIndex].ToString("0.0000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DrelBiaspFS[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DrelBiasheight[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DrelBiasBasArea[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DrelBiasLCL[timestepIndex, speciesIndex].ToString("0.000000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.DrelBiasCrowndiameter[timestepIndex, speciesIndex].ToString("0.000000", CultureInfo.InvariantCulture) + "," +
                                  this.Trajectory.Species.wsrelBias[timestepIndex, speciesIndex].ToString("0.00000", CultureInfo.InvariantCulture);
                    writer.WriteLine(line);

                    // move to next month
                    yearAndMonth = yearAndMonth.AddMonths(1);
                }
            }
        }
    }
}
