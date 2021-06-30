using System;

namespace BayesianPG.ThreePG
{
    public class TreeSpeciesTrajectory : TreeSpeciesArray
    {
        // [n_sp][n_m]
        // age of species, months since planting (https://github.com/trotsiuk/r3PG/issues/53)
        public float[][] age { get; private set; }
        // age for calculation of modifiers, one month less than age
        public float[][] age_m { get; private set; }
        public float[][] fracBB { get; private set; }
        public float[][] gammaF { get; private set; }
        public float[][] gammaN { get; private set; }
        public float[][] SLA { get; private set; }
        public float[][] wood_density { get; private set; }

        // [n_m, n_sp]
        // modifiers
        public float[,] f_age { get; private set; }
        public float[,] f_tmp { get; private set; }
        public float[,] f_tmp_gc { get; private set; }
        public float[,] f_frost { get; private set; }
        public float[,] f_calpha { get; private set; }
        public float[,] f_cg { get; private set; }
        public float[,] f_vpd { get; private set; }
        public float[,] f_sw { get; private set; }
        public float[,] f_nutr { get; private set; }
        public float[,] f_phys { get; private set; }

        // growth
        public float[,] aero_resist { get; private set; }
        public float[,] alpha_c { get; private set; }
        public float[,] biom_foliage_debt { get; private set; }
        public float[,] biom_incr_foliage { get; private set; }
        public float[,] biom_incr_root { get; private set; }
        public float[,] biom_incr_stem { get; private set; }
        public float[,] biom_loss_foliage { get; private set; }
        public float[,] biom_loss_root { get; private set; }
        public float[,] conduct_canopy { get; private set; }
        public float[,] crown_length { get; private set; }
        public float[,] crown_width{ get; private set; }
        public float[,] epsilon_biom_stem { get; private set; }
        public float[,] epsilon_gpp { get; private set; }
        public float[,] epsilon_npp { get; private set; }
        public float[,] gC { get; private set; }
        public float[,] GPP { get; private set; }
        public float[,] NPP_f { get; private set; }
        public float[,] npp_fract_foliage { get; private set; }
        public float[,] npp_fract_root { get; private set; }
        public float[,] npp_fract_stem { get; private set; }
        public float[,] VPD_sp { get; private set; }

        // mortality
        public float[,] mort_stress { get; private set; }
        public float[,] mort_thinn { get; private set; }

        // stand
        public float[,] basal_area { get; private set; }
        public float[,] basal_area_prop { get; private set; }
        public float[,] biom_foliage { get; private set; }
        public float[,] biom_root { get; private set; }
        public float[,] biom_stem { get; private set; }
        public float[,] biom_tree { get; private set; }
        public float[,] biom_tree_max { get; private set; }
        public float[,] dbh { get; private set; }
        public float[,] height { get; private set; }
        public float[,] lai { get; private set; }
        public float[,] stems_n { get; private set; }
        public float[,] stems_n_ha { get; private set; }
        public float[,] volume { get; private set; } // standing
        public float[,] volume_cum { get; private set; } // cumulative

        // water
        public float[,] prcp_interc { get; private set; }
        public float[,] transp_veg { get; private set; }
        public float[,] WUE { get; private set; }
        public float[,] WUEtransp { get; private set; }

        // 3-PGmix
        public float[,] canopy_vol_frac { get; private set; }
        public float[,] fi { get; private set; }
        public float[,] lai_above { get; private set; }
        public float[,] lai_sa_ratio { get; private set; }
        public float[,] lambda_h { get; private set; }
        public float[,] lambda_v { get; private set; }
        public int[,] layer_id { get; private set; }

        // 3-PGpjs and δ¹³C
        public float[,] canopy_cover { get; private set; }

        // δ¹³C
        public float[,] D13CNewPS { get; private set; }
        public float[,] D13CTissue { get; private set; }
        public float[,] Gc_mol { get; private set; }
        public float[,] Gw_mol { get; private set; }
        public float[,] InterCi { get; private set; }

        // bias correction
        public float[,] CVdbhDistribution { get; private set; }
        public float[,] CVwsDistribution { get; private set; }
        public float[,] height_rel { get; private set; }
        public float[,] DWeibullScale { get; private set; }
        public float[,] DWeibullShape { get; private set; }
        public float[,] DWeibullLocation { get; private set; }
        public float[,] wsWeibullScale { get; private set; }
        public float[,] wsWeibullShape { get; private set; }
        public float[,] wsWeibullLocation { get; private set; }
        public float[,] DrelBiaspFS { get; private set; }
        public float[,] DrelBiasheight { get; private set; }
        public float[,] DrelBiasBasArea { get; private set; }
        public float[,] DrelBiasLCL { get; private set; }
        public float[,] DrelBiasCrowndiameter { get; private set; }
        public float[,] wsrelBias { get; private set; }

        public TreeSpeciesTrajectory(string[] speciesNames, int n_m)
        {
            base.AllocateSpecies(speciesNames);

            this.age = new float[n_sp][];
            this.age_m = new float[n_sp][];
            this.fracBB = new float[n_sp][];
            this.gammaF = new float[n_sp][];
            this.gammaN = new float[n_sp][];
            this.SLA = new float[n_sp][];
            this.wood_density = new float[n_sp][];
            for (int speciesIndex = 0; speciesIndex < speciesNames.Length; ++speciesIndex)
            {
                this.age[speciesIndex] = new float[n_m];
                this.age_m[speciesIndex] = new float[n_m];
                this.fracBB[speciesIndex] = new float[n_m];
                this.gammaF[speciesIndex] = new float[n_m];
                this.gammaN[speciesIndex] = new float[n_m];
                this.SLA[speciesIndex] = new float[n_m];
                this.wood_density[speciesIndex] = new float[n_m];
            }

            // modifiers
            this.f_age = new float[n_m, n_sp];
            this.f_tmp = new float[n_m, n_sp];
            this.f_tmp_gc = new float[n_m, n_sp];
            this.f_frost = new float[n_m, n_sp];
            this.f_calpha = new float[n_m, n_sp];
            this.f_cg = new float[n_m, n_sp];
            this.f_vpd = new float[n_m, n_sp];
            this.f_sw = new float[n_m, n_sp];
            this.f_nutr = new float[n_m, n_sp];
            this.f_phys = new float[n_m, n_sp];

            // growth
            this.aero_resist = new float[n_m, n_sp];
            this.alpha_c = new float[n_m, n_sp];
            this.biom_foliage_debt = new float[n_m, n_sp];
            this.biom_incr_foliage = new float[n_m, n_sp];
            this.biom_incr_root = new float[n_m, n_sp];
            this.biom_incr_stem = new float[n_m, n_sp];
            this.biom_loss_foliage = new float[n_m, n_sp];
            this.biom_loss_root = new float[n_m, n_sp];
            this.conduct_canopy = new float[n_m, n_sp];
            this.crown_length = new float[n_m, n_sp];
            this.crown_width = new float[n_m, n_sp];
            this.epsilon_biom_stem = new float[n_m, n_sp];
            this.epsilon_gpp = new float[n_m, n_sp];
            this.epsilon_npp = new float[n_m, n_sp];
            this.gC = new float[n_m, n_sp];
            this.GPP = new float[n_m, n_sp];
            this.NPP_f = new float[n_m, n_sp];
            this.npp_fract_foliage = new float[n_m, n_sp];
            this.npp_fract_root = new float[n_m, n_sp];
            this.npp_fract_stem = new float[n_m, n_sp];
            this.VPD_sp = new float[n_m, n_sp];

            // mortality
            this.mort_stress = new float[n_m, n_sp];
            this.mort_thinn = new float[n_m, n_sp];

            // stand
            this.basal_area = new float[n_m, n_sp];
            this.basal_area_prop = new float[n_m, n_sp];
            this.biom_foliage = new float[n_m, n_sp];
            this.biom_root = new float[n_m, n_sp];
            this.biom_stem = new float[n_m, n_sp];
            this.biom_tree = new float[n_m, n_sp];
            this.biom_tree_max = new float[n_m, n_sp];
            this.dbh = new float[n_m, n_sp];
            this.height = new float[n_m, n_sp];
            this.lai = new float[n_m, n_sp];
            this.stems_n = new float[n_m, n_sp];
            this.stems_n_ha = new float[n_m, n_sp];
            this.volume = new float[n_m, n_sp];
            this.volume_cum = new float[n_m, n_sp];

            // water
            this.prcp_interc = new float[n_m, n_sp];
            this.transp_veg = new float[n_m, n_sp];
            this.WUE = new float[n_m, n_sp];
            this.WUEtransp = new float[n_m, n_sp];

            // 3-PGmix
            this.canopy_vol_frac = new float[n_m, n_sp];
            this.fi = new float[n_m, n_sp];
            this.lai_above = new float[n_m, n_sp];
            this.lai_sa_ratio = new float[n_m, n_sp];
            this.lambda_h = new float[n_m, n_sp];
            this.lambda_v = new float[n_m, n_sp];
            this.layer_id = new int[n_m, n_sp];

            // 3-PGpjs and δ¹³C
            this.canopy_cover = new float[n_m, n_sp];

            // δ¹³C
            this.D13CNewPS = new float[n_m, n_sp];
            this.D13CTissue = new float[n_m, n_sp];
            this.Gc_mol = new float[n_m, n_sp];
            this.Gw_mol = new float[n_m, n_sp];
            this.InterCi = new float[n_m, n_sp];

            // bias correction
            this.CVdbhDistribution = new float[n_m, n_sp];
            this.CVwsDistribution = new float[n_m, n_sp];
            this.height_rel = new float[n_m, n_sp];
            this.DWeibullScale = new float[n_m, n_sp];
            this.DWeibullShape = new float[n_m, n_sp];
            this.DWeibullLocation = new float[n_m, n_sp];
            this.wsWeibullScale = new float[n_m, n_sp];
            this.wsWeibullShape = new float[n_m, n_sp];
            this.wsWeibullLocation = new float[n_m, n_sp];
            this.DrelBiaspFS = new float[n_m, n_sp];
            this.DrelBiasheight = new float[n_m, n_sp];
            this.DrelBiasBasArea = new float[n_m, n_sp];
            this.DrelBiasLCL = new float[n_m, n_sp];
            this.DrelBiasCrowndiameter = new float[n_m, n_sp];
            this.wsrelBias = new float[n_m, n_sp];
        }

        public void AllocateDecade()
        {
            if (this.n_sp < 1)
            {
                // nothing to do
                return;
            }

            int capacity = this.age[0].Length + 10 * 12;
            for (int speciesIndex = 0; speciesIndex < this.n_sp; ++speciesIndex)
            {
                this.age[speciesIndex] = this.age[speciesIndex].Resize(capacity);
                this.age_m[speciesIndex] = this.age_m[speciesIndex].Resize(capacity);
                this.fracBB[speciesIndex] = this.fracBB[speciesIndex].Resize(capacity);
                this.gammaF[speciesIndex] = this.gammaF[speciesIndex].Resize(capacity);
                this.gammaN[speciesIndex] = this.gammaN[speciesIndex].Resize(capacity);
                this.SLA[speciesIndex] = this.SLA[speciesIndex].Resize(capacity);
                this.wood_density[speciesIndex] = this.wood_density[speciesIndex].Resize(capacity);
            }

            // modifiers
            this.f_age = this.f_age.Resize(capacity, this.n_sp);
            this.f_calpha = this.f_calpha.Resize(capacity, this.n_sp);
            this.f_cg = this.f_cg.Resize(capacity, this.n_sp);
            this.f_frost = this.f_frost.Resize(capacity, this.n_sp);
            this.f_nutr = this.f_nutr.Resize(capacity, this.n_sp);
            this.f_phys = this.f_phys.Resize(capacity, this.n_sp);
            this.f_sw = this.f_sw.Resize(capacity, this.n_sp);
            this.f_tmp = this.f_tmp.Resize(capacity, this.n_sp);
            this.f_tmp_gc = this.f_tmp_gc.Resize(capacity, this.n_sp);
            this.f_vpd = this.f_vpd.Resize(capacity, this.n_sp);

            // growth
            this.aero_resist = this.aero_resist.Resize(capacity, this.n_sp);
            this.alpha_c = this.alpha_c.Resize(capacity, this.n_sp);
            this.biom_foliage_debt = this.biom_foliage_debt.Resize(capacity, this.n_sp);
            this.biom_incr_foliage = this.biom_incr_foliage.Resize(capacity, this.n_sp);
            this.biom_incr_root = this.biom_incr_root.Resize(capacity, this.n_sp);
            this.biom_incr_stem = this.biom_incr_stem.Resize(capacity, this.n_sp);
            this.biom_loss_foliage = this.biom_loss_foliage.Resize(capacity, this.n_sp);
            this.biom_loss_root = this.biom_loss_root.Resize(capacity, this.n_sp);
            this.conduct_canopy = this.conduct_canopy.Resize(capacity, this.n_sp);
            this.crown_length = this.crown_length.Resize(capacity, this.n_sp);
            this.crown_width = this.crown_width.Resize(capacity, this.n_sp);
            this.epsilon_biom_stem = this.epsilon_biom_stem.Resize(capacity, this.n_sp);
            this.epsilon_gpp = this.epsilon_gpp.Resize(capacity, this.n_sp);
            this.epsilon_npp = this.epsilon_npp.Resize(capacity, this.n_sp);
            this.gC = this.gC.Resize(capacity, this.n_sp);
            this.GPP = this.GPP.Resize(capacity, this.n_sp);
            this.NPP_f = this.NPP_f.Resize(capacity, this.n_sp);
            this.npp_fract_foliage = this.npp_fract_foliage.Resize(capacity, this.n_sp);
            this.npp_fract_root = this.npp_fract_root.Resize(capacity, this.n_sp);
            this.npp_fract_stem = this.npp_fract_stem.Resize(capacity, this.n_sp);
            this.VPD_sp = this.VPD_sp.Resize(capacity, this.n_sp);

            // mortality
            this.mort_stress = this.mort_stress.Resize(capacity, this.n_sp);
            this.mort_thinn = this.mort_thinn.Resize(capacity, this.n_sp);

            // stand
            this.basal_area = this.basal_area.Resize(capacity, this.n_sp);
            this.basal_area_prop = this.basal_area_prop.Resize(capacity, this.n_sp);
            this.biom_foliage = this.biom_foliage.Resize(capacity, this.n_sp);
            this.biom_root = this.biom_root.Resize(capacity, this.n_sp);
            this.biom_stem = this.biom_stem.Resize(capacity, this.n_sp);
            this.biom_tree = this.biom_tree.Resize(capacity, this.n_sp);
            this.biom_tree_max = this.biom_tree_max.Resize(capacity, this.n_sp);
            this.dbh = this.dbh.Resize(capacity, this.n_sp);
            this.height = this.height.Resize(capacity, this.n_sp);
            this.lai = this.lai.Resize(capacity, this.n_sp);
            this.stems_n = this.stems_n.Resize(capacity, this.n_sp);
            this.stems_n_ha = this.stems_n_ha.Resize(capacity, this.n_sp);
            this.volume = this.volume.Resize(capacity, this.n_sp); // standing
            this.volume_cum = this.volume_cum.Resize(capacity, this.n_sp); // cumulative

            // water
            this.prcp_interc = this.prcp_interc.Resize(capacity, this.n_sp);
            this.transp_veg = this.transp_veg.Resize(capacity, this.n_sp);
            this.WUE = this.WUE.Resize(capacity, this.n_sp);
            this.WUEtransp = this.WUEtransp.Resize(capacity, this.n_sp);

            // 3-PGmix
            this.canopy_vol_frac = this.canopy_vol_frac.Resize(capacity, this.n_sp);
            this.fi = this.fi.Resize(capacity, this.n_sp);
            this.lai_above = this.lai_above.Resize(capacity, this.n_sp);
            this.lai_sa_ratio = this.lai_sa_ratio.Resize(capacity, this.n_sp);
            this.lambda_h = this.lambda_h.Resize(capacity, this.n_sp);
            this.lambda_v = this.lambda_v.Resize(capacity, this.n_sp);
            this.layer_id = this.layer_id.Resize(capacity, this.n_sp);

            // 3-PGpjs and δ¹³C
            this.canopy_cover = this.canopy_cover.Resize(capacity, this.n_sp);

            // δ¹³C
            this.D13CNewPS = this.D13CNewPS.Resize(capacity, this.n_sp);
            this.D13CTissue = this.D13CTissue.Resize(capacity, this.n_sp);
            this.Gc_mol = this.Gc_mol.Resize(capacity, this.n_sp);
            this.Gw_mol = this.Gw_mol.Resize(capacity, this.n_sp);
            this.InterCi = this.InterCi.Resize(capacity, this.n_sp);

            // bias correction
            this.CVdbhDistribution = this.CVdbhDistribution.Resize(capacity, this.n_sp);
            this.CVwsDistribution = this.CVwsDistribution.Resize(capacity, this.n_sp);
            this.height_rel = this.height_rel.Resize(capacity, this.n_sp);
            this.DWeibullScale = this.DWeibullScale.Resize(capacity, this.n_sp);
            this.DWeibullShape = this.DWeibullShape.Resize(capacity, this.n_sp);
            this.DWeibullLocation = this.DWeibullLocation.Resize(capacity, this.n_sp);
            this.wsWeibullScale = this.wsWeibullScale.Resize(capacity, this.n_sp);
            this.wsWeibullShape = this.wsWeibullShape.Resize(capacity, this.n_sp);
            this.wsWeibullLocation = this.wsWeibullLocation.Resize(capacity, this.n_sp);
            this.DrelBiaspFS = this.DrelBiaspFS.Resize(capacity, this.n_sp);
            this.DrelBiasheight = this.DrelBiasheight.Resize(capacity, this.n_sp);
            this.DrelBiasBasArea = this.DrelBiasBasArea.Resize(capacity, this.n_sp);
            this.DrelBiasLCL = this.DrelBiasLCL.Resize(capacity, this.n_sp);
            this.DrelBiasCrowndiameter = this.DrelBiasCrowndiameter.Resize(capacity, this.n_sp);
            this.wsrelBias = this.wsrelBias.Resize(capacity, this.n_sp);
        }

        public override void AllocateSpecies(string[] names)
        {
            int previouslyAllocatedSpecies = this.n_sp;
            if (previouslyAllocatedSpecies < 1)
            {
                throw new NotSupportedException();
            }
            base.AllocateSpecies(names);

            this.age = this.age.Resize(this.n_sp);
            this.age_m = this.age_m.Resize(this.n_sp);
            this.fracBB = this.fracBB.Resize(this.n_sp);
            this.gammaF = this.gammaF.Resize(this.n_sp);
            this.gammaN = this.gammaN.Resize(this.n_sp);
            this.SLA = this.SLA.Resize(this.n_sp);
            this.wood_density = this.wood_density.Resize(this.n_sp);

            int n_m = this.age[0].Length;
            for (int speciesIndex = previouslyAllocatedSpecies; speciesIndex < this.n_sp; ++speciesIndex)
            {
                this.age[speciesIndex] = new float[n_m];
                this.age_m[speciesIndex] = new float[n_m];
                this.fracBB[speciesIndex] = new float[n_m];
                this.gammaF[speciesIndex] = new float[n_m];
                this.gammaN[speciesIndex] = new float[n_m];
                this.SLA[speciesIndex] = new float[n_m];
                this.wood_density[speciesIndex] = new float[n_m];
            }

            // modifiers
            this.f_age = this.f_age.Resize(this.n_sp);
            this.f_calpha = this.f_calpha.Resize(this.n_sp);
            this.f_cg = this.f_cg.Resize(this.n_sp);
            this.f_frost = this.f_frost.Resize(this.n_sp);
            this.f_nutr = this.f_nutr.Resize(this.n_sp);
            this.f_phys = this.f_phys.Resize(this.n_sp);
            this.f_sw = this.f_sw.Resize(this.n_sp);
            this.f_tmp = this.f_tmp.Resize(this.n_sp);
            this.f_tmp_gc = this.f_tmp_gc.Resize(this.n_sp);
            this.f_vpd = this.f_vpd.Resize(this.n_sp);

            // growth
            this.aero_resist = this.aero_resist.Resize(this.n_sp);
            this.alpha_c = this.alpha_c.Resize(this.n_sp);
            this.biom_foliage_debt = this.biom_foliage_debt.Resize(this.n_sp);
            this.biom_incr_foliage = this.biom_incr_foliage.Resize(this.n_sp);
            this.biom_incr_root = this.biom_incr_root.Resize(this.n_sp);
            this.biom_incr_stem = this.biom_incr_stem.Resize(this.n_sp);
            this.biom_loss_foliage = this.biom_loss_foliage.Resize(this.n_sp);
            this.biom_loss_root = this.biom_loss_root.Resize(this.n_sp);
            this.conduct_canopy = this.conduct_canopy.Resize(this.n_sp);
            this.crown_length = this.crown_length.Resize(this.n_sp);
            this.crown_width = this.crown_width.Resize(this.n_sp);
            this.epsilon_biom_stem = this.epsilon_biom_stem.Resize(this.n_sp);
            this.epsilon_gpp = this.epsilon_gpp.Resize(this.n_sp);
            this.epsilon_npp = this.epsilon_npp.Resize(this.n_sp);
            this.GPP = this.GPP.Resize(this.n_sp);
            this.NPP_f = this.NPP_f.Resize(this.n_sp);
            this.npp_fract_foliage = this.npp_fract_foliage.Resize(this.n_sp);
            this.npp_fract_root = this.npp_fract_root.Resize(this.n_sp);
            this.npp_fract_stem = this.npp_fract_stem.Resize(this.n_sp);
            this.VPD_sp = this.VPD_sp.Resize(this.n_sp);

            // mortality
            this.mort_stress = this.mort_stress.Resize(this.n_sp);
            this.mort_thinn = this.mort_thinn.Resize(this.n_sp);

            // stand
            this.basal_area = this.basal_area.Resize(this.n_sp);
            this.basal_area_prop = this.basal_area_prop.Resize(this.n_sp);
            this.biom_foliage = this.biom_foliage.Resize(this.n_sp);
            this.biom_root = this.biom_root.Resize(this.n_sp);
            this.biom_stem = this.biom_stem.Resize(this.n_sp);
            this.biom_tree = this.biom_tree.Resize(this.n_sp);
            this.biom_tree_max = this.biom_tree_max.Resize(this.n_sp);
            this.dbh = this.dbh.Resize(this.n_sp);
            this.height = this.height.Resize(this.n_sp);
            this.lai = this.lai.Resize(this.n_sp);
            this.stems_n = this.stems_n.Resize(this.n_sp);
            this.stems_n_ha = this.stems_n_ha.Resize(this.n_sp);
            this.volume = this.volume.Resize(this.n_sp); // standing
            this.volume_cum = this.volume_cum.Resize(this.n_sp); // cumulative

            // water
            this.prcp_interc = this.prcp_interc.Resize(this.n_sp);
            this.transp_veg = this.transp_veg.Resize(this.n_sp);
            this.WUE = this.WUE.Resize(this.n_sp);
            this.WUEtransp = this.WUEtransp.Resize(this.n_sp);

            // 3-PGmix
            this.canopy_vol_frac = this.canopy_vol_frac.Resize(this.n_sp);
            this.fi = this.fi.Resize(this.n_sp);
            this.lai_above = this.lai_above.Resize(this.n_sp);
            this.lai_sa_ratio = this.lai_sa_ratio.Resize(this.n_sp);
            this.lambda_h = this.lambda_h.Resize(this.n_sp);
            this.lambda_v = this.lambda_v.Resize(this.n_sp);
            this.layer_id = this.layer_id.Resize(this.n_sp);

            // 3-PGpjs and δ¹³C
            this.canopy_cover = this.canopy_cover.Resize(this.n_sp);

            // δ¹³C
            this.D13CNewPS = this.D13CNewPS.Resize(this.n_sp);
            this.D13CTissue = this.D13CTissue.Resize(this.n_sp);
            this.Gc_mol = this.Gc_mol.Resize(this.n_sp);
            this.Gw_mol = this.Gw_mol.Resize(this.n_sp);
            this.InterCi = this.InterCi.Resize(this.n_sp);

            // bias correction
            this.CVdbhDistribution = this.CVdbhDistribution.Resize(this.n_sp);
            this.CVwsDistribution = this.CVwsDistribution.Resize(this.n_sp);
            this.height_rel = this.height_rel.Resize(this.n_sp);
            this.DWeibullScale = this.DWeibullScale.Resize(this.n_sp);
            this.DWeibullShape = this.DWeibullShape.Resize(this.n_sp);
            this.DWeibullLocation = this.DWeibullLocation.Resize(this.n_sp);
            this.wsWeibullScale = this.wsWeibullScale.Resize(this.n_sp);
            this.wsWeibullShape = this.wsWeibullShape.Resize(this.n_sp);
            this.wsWeibullLocation = this.wsWeibullLocation.Resize(this.n_sp);
            this.DrelBiaspFS = this.DrelBiaspFS.Resize(this.n_sp);
            this.DrelBiasheight = this.DrelBiasheight.Resize(this.n_sp);
            this.DrelBiasBasArea = this.DrelBiasBasArea.Resize(this.n_sp);
            this.DrelBiasLCL = this.DrelBiasLCL.Resize(this.n_sp);
            this.DrelBiasCrowndiameter = this.DrelBiasCrowndiameter.Resize(this.n_sp);
            this.wsrelBias = this.wsrelBias.Resize(this.n_sp);
        }

        public void SetMonth(int timestepIndex, ThreePGState state)
        {
            for (int speciesIndex = 0; speciesIndex < this.n_sp; ++speciesIndex)
            {
                // modifiers
                this.f_vpd[timestepIndex, speciesIndex] = state.f_vpd[speciesIndex];
                this.f_sw[timestepIndex, speciesIndex] = state.f_sw[speciesIndex];
                this.f_nutr[timestepIndex, speciesIndex] = state.f_nutr[speciesIndex];
                this.f_phys[timestepIndex, speciesIndex] = state.f_phys[speciesIndex];

                // growth
                this.aero_resist[timestepIndex, speciesIndex] = state.aero_resist[speciesIndex];
                this.alpha_c[timestepIndex, speciesIndex] = state.alpha_c[speciesIndex];
                this.biom_foliage_debt[timestepIndex, speciesIndex] = state.biom_foliage_debt[speciesIndex];
                this.biom_incr_foliage[timestepIndex, speciesIndex] = state.biom_incr_foliage[speciesIndex];
                this.biom_incr_root[timestepIndex, speciesIndex] = state.biom_incr_root[speciesIndex];
                this.biom_incr_stem[timestepIndex, speciesIndex] = state.biom_incr_stem[speciesIndex];
                this.biom_loss_foliage[timestepIndex, speciesIndex] = state.biom_loss_foliage[speciesIndex];
                this.biom_loss_root[timestepIndex, speciesIndex] = state.biom_loss_root[speciesIndex];
                this.conduct_canopy[timestepIndex, speciesIndex] = state.conduct_canopy[speciesIndex];
                this.crown_length[timestepIndex, speciesIndex] = state.crown_length[speciesIndex];
                this.crown_width[timestepIndex, speciesIndex] = state.crown_width[speciesIndex];
                this.epsilon_biom_stem[timestepIndex, speciesIndex] = state.epsilon_biom_stem[speciesIndex];
                this.epsilon_gpp[timestepIndex, speciesIndex] = state.epsilon_gpp[speciesIndex];
                this.epsilon_npp[timestepIndex, speciesIndex] = state.epsilon_npp[speciesIndex];
                this.gC[timestepIndex, speciesIndex] = state.gC[speciesIndex];
                this.GPP[timestepIndex, speciesIndex] = state.GPP[speciesIndex];
                this.NPP_f[timestepIndex, speciesIndex] = state.NPP_f[speciesIndex];
                // this.m[timestepIndex, speciesIndex] = state.m[speciesIndex]; // not returned as an output from Fortran
                this.npp_fract_root[timestepIndex, speciesIndex] = state.npp_fract_root[speciesIndex];
                this.npp_fract_stem[timestepIndex, speciesIndex] = state.npp_fract_stem[speciesIndex];
                this.npp_fract_foliage[timestepIndex, speciesIndex] = state.npp_fract_foliage[speciesIndex];
                this.VPD_sp[timestepIndex, speciesIndex] = state.VPD_sp[speciesIndex];

                // mortality
                this.mort_stress[timestepIndex, speciesIndex] = state.mort_stress[speciesIndex];
                this.mort_thinn[timestepIndex, speciesIndex] = state.mort_thinn[speciesIndex];

                // stand
                this.basal_area[timestepIndex, speciesIndex] = state.basal_area[speciesIndex];
                this.basal_area_prop[timestepIndex, speciesIndex] = state.basal_area_prop[speciesIndex];
                this.biom_foliage[timestepIndex, speciesIndex] = state.biom_foliage[speciesIndex];
                this.biom_foliage_debt[timestepIndex, speciesIndex] = state.biom_foliage_debt[speciesIndex];
                this.biom_root[timestepIndex, speciesIndex] = state.biom_root[speciesIndex];
                this.biom_stem[timestepIndex, speciesIndex] = state.biom_stem[speciesIndex];
                this.biom_tree[timestepIndex, speciesIndex] = state.biom_tree[speciesIndex];
                this.biom_tree_max[timestepIndex, speciesIndex] = state.biom_tree_max[speciesIndex];
                this.dbh[timestepIndex, speciesIndex] = state.dbh[speciesIndex];
                this.height[timestepIndex, speciesIndex] = state.height[speciesIndex];
                this.lai[timestepIndex, speciesIndex] = state.lai[speciesIndex];
                this.stems_n[timestepIndex, speciesIndex] = state.stems_n[speciesIndex];
                this.stems_n_ha[timestepIndex, speciesIndex] = state.stems_n_ha[speciesIndex];
                this.volume[timestepIndex, speciesIndex] = state.volume[speciesIndex];
                this.volume_cum[timestepIndex, speciesIndex] = state.volume_cum[speciesIndex];

                // water
                // this.prcp_interc[timestepIndex, speciesIndex] is set directly
                this.transp_veg[timestepIndex, speciesIndex] = state.transp_veg[speciesIndex];
                this.WUE[timestepIndex, speciesIndex] = state.WUE[speciesIndex];
                this.WUEtransp[timestepIndex, speciesIndex] = state.WUE_transp[speciesIndex];

                // 3-PGmix
                this.canopy_vol_frac[timestepIndex, speciesIndex] = state.canopy_vol_frac[speciesIndex];
                this.fi[timestepIndex, speciesIndex] = state.fi[speciesIndex];
                this.lai_above[timestepIndex, speciesIndex] = state.lai_above[speciesIndex];
                this.lai_sa_ratio[timestepIndex, speciesIndex] = state.lai_sa_ratio[speciesIndex];
                this.lambda_h[timestepIndex, speciesIndex] = state.lambda_h[speciesIndex];
                this.lambda_v[timestepIndex, speciesIndex] = state.lambda_v[speciesIndex];
                this.layer_id[timestepIndex, speciesIndex] = state.layer_id[speciesIndex];

                // 3-PGpjs and δ¹³C
                this.canopy_cover[timestepIndex, speciesIndex] = state.canopy_cover[speciesIndex];

                // δ¹³C
                this.D13CNewPS[timestepIndex, speciesIndex] = state.D13CNewPS[speciesIndex];
                this.D13CTissue[timestepIndex, speciesIndex] = state.D13CTissue[speciesIndex];
                this.Gc_mol[timestepIndex, speciesIndex] = state.Gc_mol[speciesIndex];
                this.Gw_mol[timestepIndex, speciesIndex] = state.Gw_mol[speciesIndex];
                this.InterCi[timestepIndex, speciesIndex] = state.InterCi[speciesIndex];

                // bias correction
                this.CVdbhDistribution[timestepIndex, speciesIndex] = state.CVdbhDistribution[speciesIndex];
                this.CVwsDistribution[timestepIndex, speciesIndex] = state.CVwsDistribution[speciesIndex];
                this.height_rel[timestepIndex, speciesIndex] = state.height_rel[speciesIndex];
                this.DWeibullScale[timestepIndex, speciesIndex] = state.DWeibullScale[speciesIndex];
                this.DWeibullShape[timestepIndex, speciesIndex] = state.DWeibullShape[speciesIndex];
                this.DWeibullLocation[timestepIndex, speciesIndex] = state.DWeibullLocation[speciesIndex];
                this.wsWeibullScale[timestepIndex, speciesIndex] = state.wsWeibullScale[speciesIndex];
                this.wsWeibullShape[timestepIndex, speciesIndex] = state.wsWeibullShape[speciesIndex];
                this.wsWeibullLocation[timestepIndex, speciesIndex] = state.wsWeibullLocation[speciesIndex];
                this.DrelBiaspFS[timestepIndex, speciesIndex] = state.DrelBiaspFS[speciesIndex];
                this.DrelBiasheight[timestepIndex, speciesIndex] = state.DrelBiasheight[speciesIndex];
                this.DrelBiasBasArea[timestepIndex, speciesIndex] = state.DrelBiasBasArea[speciesIndex];
                this.DrelBiasLCL[timestepIndex, speciesIndex] = state.DrelBiasLCL[speciesIndex];
                this.DrelBiasCrowndiameter[timestepIndex, speciesIndex] = state.DrelBiasCrowndiameter[speciesIndex];
                this.wsrelBias[timestepIndex, speciesIndex] = state.wsrelBias[speciesIndex];
            }
        }
    }
}
