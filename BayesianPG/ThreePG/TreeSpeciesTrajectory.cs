using BayesianPG.Extensions;
using System;

namespace BayesianPG.ThreePG
{
    /// <remarks>
    /// 86 output variables per species (403 kB in memory per species-century if all are enabled).
    /// - 7 age dependent terms
    /// - 9 modifiers
    /// - 17 growth
    /// - 2 mortality
    /// - 13 stand
    /// - 4 water
    /// - 7 3-PGmix
    /// - 1 3-PGpjs
    /// - 5 δ¹³C
    /// - 15 bias correction
    /// - 6 extended
    /// </remarks>
    public class TreeSpeciesTrajectory<TFloat, TInteger> : TreeSpeciesArray 
        where TFloat : struct
        where TInteger : struct
    {
        // [n_sp][n_m]
        // age of species, months since planting (https://github.com/trotsiuk/r3PG/issues/53)
        public float[][] age { get; private set; } // years
        // age for calculation of modifiers, one month less than age
        public float[][] age_m { get; private set; } // displaced years
        public TFloat[][] fracBB { get; private set; } // branch and bark fraction
        public TFloat[][] gammaF { get; private set; } // monthly litterfall rate, fraction
        public TFloat[][] gammaN { get; private set; } // monthly density independent mortality rate, fraction
        public TFloat[][] SLA { get; private set; } // specific leaf area, m²/kg
        public TFloat[][] wood_density { get; private set; } // specific gravity = tons/m³

        // [n_m, n_sp]
        // modifiers
        public TFloat[,] f_age { get; private set; } // age modifier, 3-PGmix Equation A10
        public TFloat[,] f_tmp { get; private set; } // temperature modifier, 3-PGmix Equation A5
        public TFloat[,] f_frost { get; private set; } // frost days modifier, 3-PGmix Equation A6
        public TFloat[,] f_calpha { get; private set; } // CO₂ modifier for PG (assimilation enhancement), 3-PGmix Equation A11
        public TFloat[,] f_cg { get; private set; } // CO₂ modifier for canopy conductance, 3-PGmix Equation A12
        public TFloat[,] f_vpd { get; private set; } // vapor pressure deficit modifier, 3-PGmix Equation A8
        public TFloat[,] f_sw { get; private set; } // available soil water modifier, 3-PGmix Equation A9
        public TFloat[,] f_nutr { get; private set; } // nutrition (fertility) modifier, 3-PGmix Equation A7
        public TFloat[,] f_phys { get; private set; } // physiological modifier φ, 3-PGmix Equation A3

        // growth
        public TFloat[,] aero_resist { get; private set; }
        public TFloat[,] alpha_c { get; private set; } // canopy quantumn efficiency after modifiers, mol/mol, 3-PGmix Equation A2
        public TFloat[,] biom_foliage_debt { get; private set; } // dry tons/ha, deciduous species
        public TFloat[,] conduct_canopy { get; private set; } // species canopy conductance after modifiers, gC * lai_per * f_phys * f_tmp_gc * f_cg, m/s
        public TFloat[,] crown_length { get; private set; } // mean crown length, m
        public TFloat[,] crown_width { get; private set; } // mean crown diameter, m
        public TFloat[,] epsilon_biom_stem { get; private set; } // light utilization efficiency in growth of stem biomass, dry g/MJ
        public TFloat[,] epsilon_gpp { get; private set; } // light utilization efficiency, dry g/MJ
        public TFloat[,] epsilon_npp { get; private set; } // light utilization efficiency, dry g/MJ
        public TFloat[,] f_tmp_gc { get; private set; } // temperature response multiplier for canopy conductance
        public TFloat[,] gC { get; private set; } // canopy conductance, m/s
        public TFloat[,] GPP { get; private set; } // gross primary production Pg, dry tons/ha
        public TFloat[,] NPP_f { get; private set; } // net primary production Pn, dry tons/ha
        public TFloat[,] npp_fract_foliage { get; private set; } // fraction of NPP allocated to foliage
        public TFloat[,] npp_fract_root { get; private set; } // fraction of NPP allocated to coarse and fine roots
        public TFloat[,] npp_fract_stem { get; private set; } // fraction of NPP allocated to stem, branches, and bark
        public TFloat[,] VPD_sp { get; private set; }

        // mortality
        public TFloat[,] mort_stress { get; private set; } // density independent mortality from gammaN
        public TFloat[,] mort_thinn { get; private set; } // density dependent mortality (self thinning)

        // stand
        public TFloat[,] basal_area { get; private set; } // m²/ha
        public TFloat[,] basal_area_prop { get; private set; } // species fraction of total basal area
        public TFloat[,] biom_foliage { get; private set; } // dry tons/ha
        public TFloat[,] biom_root { get; private set; } // dry tons/ha
        public TFloat[,] biom_stem { get; private set; } // dry tons/ha
        public TFloat[,] biom_tree { get; private set; } // mean stem biomass, dry tons/tree
        public TFloat[,] biom_tree_max { get; private set; }
        public TFloat[,] dbh { get; private set; } // [quadratic] mean DBH, cm
        public TFloat[,] height { get; private set; } // top height, m
        public TFloat[,] lai { get; private set; } // m²/m²
        public TFloat[,] stems_n { get; private set; } // actual trees per hectare
        public TFloat[,] stems_n_ha { get; private set; } // maximum trees per hectare at biomass limit
        public TFloat[,] volume { get; private set; } // standing volume, merchantable m³/ha

        // water
        public TFloat[,] prcp_interc { get; private set; }
        public TFloat[,] transp_veg { get; private set; }
        public TFloat[,] WUE { get; private set; }
        public TFloat[,] WUEtransp { get; private set; }

        // 3-PGmix
        public TFloat[,] canopy_vol_frac { get; private set; }
        public TFloat[,] fi { get; private set; }
        public TFloat[,] lai_above { get; private set; }
        public TFloat[,] lai_sa_ratio { get; private set; }
        public TFloat[,] lambda_h { get; private set; }
        public TFloat[,] lambda_v { get; private set; }
        public TInteger[,] layer_id { get; private set; }

        // 3-PGpjs and δ¹³C
        public TFloat[,] canopy_cover { get; private set; }

        // δ¹³C
        public TFloat[,] D13CNewPS { get; private set; }
        public TFloat[,] D13CTissue { get; private set; }
        public TFloat[,] Gc_mol { get; private set; }
        public TFloat[,] Gw_mol { get; private set; }
        public TFloat[,] InterCi { get; private set; }

        // bias correction
        public TFloat[,] CVdbhDistribution { get; private set; }
        public TFloat[,] CVwsDistribution { get; private set; }
        public TFloat[,] height_rel { get; private set; }
        public TFloat[,] DWeibullScale { get; private set; }
        public TFloat[,] DWeibullShape { get; private set; }
        public TFloat[,] DWeibullLocation { get; private set; }
        public TFloat[,] wsWeibullScale { get; private set; }
        public TFloat[,] wsWeibullShape { get; private set; }
        public TFloat[,] wsWeibullLocation { get; private set; }
        public TFloat[,] DrelBiaspFS { get; private set; }
        public TFloat[,] DrelBiasheight { get; private set; }
        public TFloat[,] DrelBiasBasArea { get; private set; }
        public TFloat[,] DrelBiasLCL { get; private set; }
        public TFloat[,] DrelBiasCrowndiameter { get; private set; }
        public TFloat[,] wsrelBias { get; private set; }

        // extended columns
        public TFloat[,] biom_incr_foliage { get; private set; }
        public TFloat[,] biom_incr_root { get; private set; }
        public TFloat[,] biom_incr_stem { get; private set; }
        public TFloat[,] biom_loss_foliage { get; private set; }
        public TFloat[,] biom_loss_root { get; private set; }
        public TFloat[,] volume_cum { get; private set; } // cumulative

        public TreeSpeciesTrajectory(string[] speciesNames, int monthCount, ThreePGStandTrajectoryColumnGroups columns)
        {
            base.AllocateSpecies(speciesNames);

            this.age = new float[n_sp][];
            this.age_m = new float[this.n_sp][];
            this.fracBB = new TFloat[this.n_sp][];
            this.gammaF = new TFloat[this.n_sp][];
            this.gammaN = new TFloat[this.n_sp][];
            this.SLA = new TFloat[this.n_sp][];
            this.wood_density = new TFloat[this.n_sp][];
            for (int speciesIndex = 0; speciesIndex < speciesNames.Length; ++speciesIndex)
            {
                this.age[speciesIndex] = new float[monthCount];
                this.age_m[speciesIndex] = new float[monthCount];
                this.fracBB[speciesIndex] = new TFloat[monthCount];
                this.gammaF[speciesIndex] = new TFloat[monthCount];
                this.gammaN[speciesIndex] = new TFloat[monthCount];
                this.SLA[speciesIndex] = new TFloat[monthCount];
                this.wood_density[speciesIndex] = new TFloat[monthCount];
            }

            // modifiers
            this.f_age = new TFloat[monthCount, this.n_sp];
            this.f_tmp = new TFloat[monthCount, this.n_sp];
            this.f_frost = new TFloat[monthCount, this.n_sp];
            this.f_calpha = new TFloat[monthCount, this.n_sp];
            this.f_cg = new TFloat[monthCount, this.n_sp];
            this.f_vpd = new TFloat[monthCount, this.n_sp];
            this.f_sw = new TFloat[monthCount, this.n_sp];
            this.f_nutr = new TFloat[monthCount, this.n_sp];
            this.f_phys = new TFloat[monthCount, this.n_sp];

            // growth
            this.alpha_c = new TFloat[monthCount, this.n_sp];
            this.aero_resist = new TFloat[monthCount, this.n_sp];
            this.biom_foliage_debt = new TFloat[monthCount, this.n_sp];
            this.conduct_canopy = new TFloat[monthCount, this.n_sp];
            this.crown_length = new TFloat[monthCount, this.n_sp];
            this.crown_width = new TFloat[monthCount, this.n_sp];
            this.epsilon_biom_stem = new TFloat[monthCount, this.n_sp];
            this.epsilon_gpp = new TFloat[monthCount, this.n_sp];
            this.epsilon_npp = new TFloat[monthCount, this.n_sp];
            this.f_tmp_gc = new TFloat[monthCount, this.n_sp];
            this.gC = new TFloat[monthCount, this.n_sp];
            this.GPP = new TFloat[monthCount, this.n_sp];
            this.NPP_f = new TFloat[monthCount, this.n_sp];
            this.npp_fract_foliage = new TFloat[monthCount, this.n_sp];
            this.npp_fract_root = new TFloat[monthCount, this.n_sp];
            this.npp_fract_stem = new TFloat[monthCount, this.n_sp];
            this.VPD_sp = new TFloat[monthCount, this.n_sp];

            // mortality
            this.mort_stress = new TFloat[monthCount, this.n_sp];
            this.mort_thinn = new TFloat[monthCount, this.n_sp];

            // stand
            this.basal_area = new TFloat[monthCount, this.n_sp];
            this.basal_area_prop = new TFloat[monthCount, this.n_sp];
            this.biom_foliage = new TFloat[monthCount, this.n_sp];
            this.biom_root = new TFloat[monthCount, this.n_sp];
            this.biom_stem = new TFloat[monthCount, this.n_sp];
            this.biom_tree = new TFloat[monthCount, this.n_sp];
            this.biom_tree_max = new TFloat[monthCount, this.n_sp];
            this.dbh = new TFloat[monthCount, this.n_sp];
            this.height = new TFloat[monthCount, this.n_sp];
            this.lai = new TFloat[monthCount, this.n_sp];
            this.stems_n = new TFloat[monthCount, this.n_sp];
            this.stems_n_ha = new TFloat[monthCount, this.n_sp];
            this.volume = new TFloat[monthCount, this.n_sp];

            // water
            this.prcp_interc = new TFloat[monthCount, this.n_sp];
            this.transp_veg = new TFloat[monthCount, this.n_sp];
            this.WUE = new TFloat[monthCount, this.n_sp];
            this.WUEtransp = new TFloat[monthCount, this.n_sp];

            // 3-PGmix
            this.canopy_vol_frac = new TFloat[monthCount, this.n_sp];
            this.fi = new TFloat[monthCount, this.n_sp];
            this.lai_above = new TFloat[monthCount, this.n_sp];
            this.lai_sa_ratio = new TFloat[monthCount, this.n_sp];
            this.lambda_h = new TFloat[monthCount, this.n_sp];
            this.lambda_v = new TFloat[monthCount, this.n_sp];
            this.layer_id = new TInteger[monthCount, this.n_sp];

            // 3-PGpjs and δ¹³C
            this.canopy_cover = new TFloat[monthCount, this.n_sp];

            // δ¹³C
            this.D13CNewPS = new TFloat[monthCount, this.n_sp];
            this.D13CTissue = new TFloat[monthCount, this.n_sp];
            this.Gc_mol = new TFloat[monthCount, this.n_sp];
            this.Gw_mol = new TFloat[monthCount, this.n_sp];
            this.InterCi = new TFloat[monthCount, this.n_sp];

            // bias correction
            if (columns.HasFlag(ThreePGStandTrajectoryColumnGroups.BiasCorrection))
            {
                this.CVdbhDistribution = new TFloat[monthCount, this.n_sp];
                this.CVwsDistribution = new TFloat[monthCount, this.n_sp];
                this.height_rel = new TFloat[monthCount, this.n_sp];
                this.DWeibullScale = new TFloat[monthCount, this.n_sp];
                this.DWeibullShape = new TFloat[monthCount, this.n_sp];
                this.DWeibullLocation = new TFloat[monthCount, this.n_sp];
                this.wsWeibullScale = new TFloat[monthCount, this.n_sp];
                this.wsWeibullShape = new TFloat[monthCount, this.n_sp];
                this.wsWeibullLocation = new TFloat[monthCount, this.n_sp];
                this.DrelBiaspFS = new TFloat[monthCount, this.n_sp];
                this.DrelBiasheight = new TFloat[monthCount, this.n_sp];
                this.DrelBiasBasArea = new TFloat[monthCount, this.n_sp];
                this.DrelBiasLCL = new TFloat[monthCount, this.n_sp];
                this.DrelBiasCrowndiameter = new TFloat[monthCount, this.n_sp];
                this.wsrelBias = new TFloat[monthCount, this.n_sp];
            }
            else
            {
                this.CVdbhDistribution = new TFloat[0, 0];
                this.CVwsDistribution = new TFloat[0, 0];
                this.height_rel = new TFloat[0, 0];
                this.DWeibullScale = new TFloat[0, 0];
                this.DWeibullShape = new TFloat[0, 0];
                this.DWeibullLocation = new TFloat[0, 0];
                this.wsWeibullScale = new TFloat[0, 0];
                this.wsWeibullShape = new TFloat[0, 0];
                this.wsWeibullLocation = new TFloat[0, 0];
                this.DrelBiaspFS = new TFloat[0, 0];
                this.DrelBiasheight = new TFloat[0, 0];
                this.DrelBiasBasArea = new TFloat[0, 0];
                this.DrelBiasLCL = new TFloat[0, 0];
                this.DrelBiasCrowndiameter = new TFloat[0, 0];
                this.wsrelBias = new TFloat[0, 0];
            }

            // extended columns
            if (columns.HasFlag(ThreePGStandTrajectoryColumnGroups.Extended))
            {
                // not currently supported
                // apar
                // pFS
                // prcp_interc_frac
                // volume_change
                // volume_extracted
                // volume_mai
                this.biom_incr_foliage = new TFloat[monthCount, this.n_sp];
                this.biom_incr_root = new TFloat[monthCount, this.n_sp];
                this.biom_incr_stem = new TFloat[monthCount, this.n_sp];
                this.biom_loss_foliage = new TFloat[monthCount, this.n_sp];
                this.biom_loss_root = new TFloat[monthCount, this.n_sp];
                this.volume_cum = new TFloat[monthCount, this.n_sp];
            }
            else
            {
                this.biom_incr_foliage = new TFloat[0, 0];
                this.biom_incr_root = new TFloat[0, 0];
                this.biom_incr_stem = new TFloat[0, 0];
                this.biom_loss_foliage = new TFloat[0, 0];
                this.biom_loss_root = new TFloat[0, 0];
                this.volume_cum = new TFloat[0, 0];
            }
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
            this.f_vpd = this.f_vpd.Resize(capacity, this.n_sp);

            // growth
            this.aero_resist = this.aero_resist.Resize(capacity, this.n_sp);
            this.alpha_c = this.alpha_c.Resize(capacity, this.n_sp);
            this.biom_foliage_debt = this.biom_foliage_debt.Resize(capacity, this.n_sp);
            this.conduct_canopy = this.conduct_canopy.Resize(capacity, this.n_sp);
            this.crown_length = this.crown_length.Resize(capacity, this.n_sp);
            this.crown_width = this.crown_width.Resize(capacity, this.n_sp);
            this.epsilon_biom_stem = this.epsilon_biom_stem.Resize(capacity, this.n_sp);
            this.epsilon_gpp = this.epsilon_gpp.Resize(capacity, this.n_sp);
            this.epsilon_npp = this.epsilon_npp.Resize(capacity, this.n_sp);
            this.f_tmp_gc = this.f_tmp_gc.Resize(capacity, this.n_sp);
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
            if (this.CVdbhDistribution.Length > 0)
            {
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

            // extended columns
            if (this.biom_incr_foliage.Length > 0)
            {
                this.biom_incr_foliage = this.biom_incr_foliage.Resize(capacity, this.n_sp);
                this.biom_incr_root = this.biom_incr_root.Resize(capacity, this.n_sp);
                this.biom_incr_stem = this.biom_incr_stem.Resize(capacity, this.n_sp);
                this.biom_loss_foliage = this.biom_loss_foliage.Resize(capacity, this.n_sp);
                this.biom_loss_root = this.biom_loss_root.Resize(capacity, this.n_sp);
                this.volume_cum = this.volume_cum.Resize(capacity, this.n_sp);
            }
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
                this.fracBB[speciesIndex] = new TFloat[n_m];
                this.gammaF[speciesIndex] = new TFloat[n_m];
                this.gammaN[speciesIndex] = new TFloat[n_m];
                this.SLA[speciesIndex] = new TFloat[n_m];
                this.wood_density[speciesIndex] = new TFloat[n_m];
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
            this.f_vpd = this.f_vpd.Resize(this.n_sp);

            // growth
            this.aero_resist = this.aero_resist.Resize(this.n_sp);
            this.alpha_c = this.alpha_c.Resize(this.n_sp);
            this.biom_foliage_debt = this.biom_foliage_debt.Resize(this.n_sp);
            this.conduct_canopy = this.conduct_canopy.Resize(this.n_sp);
            this.crown_length = this.crown_length.Resize(this.n_sp);
            this.crown_width = this.crown_width.Resize(this.n_sp);
            this.epsilon_biom_stem = this.epsilon_biom_stem.Resize(this.n_sp);
            this.epsilon_gpp = this.epsilon_gpp.Resize(this.n_sp);
            this.epsilon_npp = this.epsilon_npp.Resize(this.n_sp);
            this.f_tmp_gc = this.f_tmp_gc.Resize(this.n_sp);
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
            if (this.CVdbhDistribution.Length > 0)
            {
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

            // extended columns
            if (this.biom_incr_foliage.Length > 0)
            {
                this.biom_incr_foliage = this.biom_incr_foliage.Resize(this.n_sp);
                this.biom_incr_root = this.biom_incr_root.Resize(this.n_sp);
                this.biom_incr_stem = this.biom_incr_stem.Resize(this.n_sp);
                this.biom_loss_foliage = this.biom_loss_foliage.Resize(this.n_sp);
                this.biom_loss_root = this.biom_loss_root.Resize(this.n_sp);
                this.volume_cum = this.volume_cum.Resize(this.n_sp); // cumulative
            }
        }

        public void SetMonth(int timestepIndex, ThreePGState<TFloat, TInteger> state)
        {
            for (int speciesIndex = 0; speciesIndex < this.n_sp; ++speciesIndex)
            {
                // modifiers not already set from within timestep
                this.f_vpd[timestepIndex, speciesIndex] = state.f_vpd[speciesIndex];
                this.f_sw[timestepIndex, speciesIndex] = state.f_sw[speciesIndex];
                this.f_nutr[timestepIndex, speciesIndex] = state.f_nutr[speciesIndex];
                this.f_phys[timestepIndex, speciesIndex] = state.f_phys[speciesIndex];

                // growth
                this.aero_resist[timestepIndex, speciesIndex] = state.aero_resist[speciesIndex];
                this.alpha_c[timestepIndex, speciesIndex] = state.alpha_c[speciesIndex];
                this.biom_foliage_debt[timestepIndex, speciesIndex] = state.biom_foliage_debt[speciesIndex];
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
                this.npp_fract_foliage[timestepIndex, speciesIndex] = state.npp_fract_foliage[speciesIndex];
                this.npp_fract_root[timestepIndex, speciesIndex] = state.npp_fract_root[speciesIndex];
                this.npp_fract_stem[timestepIndex, speciesIndex] = state.npp_fract_stem[speciesIndex];
                this.VPD_sp[timestepIndex, speciesIndex] = state.VPD_sp[speciesIndex];

                // mortality
                this.mort_stress[timestepIndex, speciesIndex] = state.mort_stress[speciesIndex];
                this.mort_thinn[timestepIndex, speciesIndex] = state.mort_thinn[speciesIndex];

                // stand
                this.basal_area[timestepIndex, speciesIndex] = state.basal_area[speciesIndex];
                this.basal_area_prop[timestepIndex, speciesIndex] = state.basal_area_prop[speciesIndex];
                this.biom_foliage[timestepIndex, speciesIndex] = state.biom_foliage[speciesIndex];
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
                if (this.CVdbhDistribution.Length > 0)
                {
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

                // extended coluns
                if (this.biom_incr_foliage.Length > 0)
                {
                    this.biom_incr_foliage[timestepIndex, speciesIndex] = state.biom_incr_foliage[speciesIndex];
                    this.biom_incr_root[timestepIndex, speciesIndex] = state.biom_incr_root[speciesIndex];
                    this.biom_incr_stem[timestepIndex, speciesIndex] = state.biom_incr_stem[speciesIndex];
                    this.biom_loss_foliage[timestepIndex, speciesIndex] = state.biom_loss_foliage[speciesIndex];
                    this.biom_loss_root[timestepIndex, speciesIndex] = state.biom_loss_root[speciesIndex];
                    this.volume_cum[timestepIndex, speciesIndex] = state.volume_cum[speciesIndex];
                }
            }
        }
    }
}
