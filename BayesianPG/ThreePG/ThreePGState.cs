using System;

namespace BayesianPG.ThreePG
{
    public class ThreePGState
    {
        // initialization
        public float[] adjSolarZenithAngle { get; private init; }
        public float[] day_length { get; private init; }
        public float[] pfsConst { get; private init; }
        public float[] pfsPower { get; private init; }
        public float[] swConst { get; private init; }
        public float[] swPower { get; private init; }

        // management
        public int[] t_n { get; private init; }

        // modifiers
        public float[] f_vpd { get; private init; }
        public float[] f_sw { get; private init; }
        public float[] f_nutr { get; private init; }
        public float[] f_phys { get; private init; }

        // mortality
        public float[] mort_manag { get; private init; }
        public float[] mort_stress { get; private init; }
        public float[] mort_thinn { get; private init; }

        // growth
        public float[] aero_resist { get; private init; }
        public float air_pressure { get; set; }
        public float[] apar { get; private init; }
        public float[] alpha_c { get; private init; }
        public float[] biom_incr_foliage { get; private init; }
        public float[] biom_incr_root { get; private init; }
        public float[] biom_incr_stem { get; private init; }
        public float[] biom_loss_foliage { get; private init; }
        public float[] biom_loss_root { get; private init; }
        public float[] conduct_canopy { get; private init; }
        public float[] epsilon { get; private init; }
        public float[] epsilon_gpp { get; private init; }
        public float[] epsilon_npp { get; private init; }
        public float[] epsilon_biom_stem { get; private init; }
        public float[] GPP { get; private init; }
        public float[] NPP { get; private init; }
        public float[] NPP_f { get; private init; } // duplicate of NPP present in Fortran, TODO: remove
        public float[] npp_fract_root { get; private init; }
        public float[] npp_fract_stem { get; private init; }
        public float[] npp_fract_foliage { get; private init; }
        public float[] gC { get; private init; }
        public float[] m { get; private init; }
        public float[] VPD_sp { get; private init; }

        // stand
        public float[] basal_area { get; private init; }
        public float[] basal_area_prop { get; private init; }

        public float[] biom_foliage { get; private init; }
        public float[] biom_foliage_debt { get; private init; }
        public float[] biom_root { get; private init; }
        public float[] biom_stem { get; private init; }
        public float[] biom_tree { get; private init; } // mean stem mass, kg
        public float[] biom_tree_max { get; private init; }

        public float competition_total { get; set; }

        public float[] dbh { get; private init; }
        public float[] lai { get; private init; }
        public float[] lai_per { get; private init; }
        public float[] height { get; private init; }
        public float[] stems_n { get; private init; }
        public float[] stems_n_ha { get; private init; }

        public float[] volume { get; private init; }
        public float[] volume_change { get; private init; } // probably can be removed
        public float[] volume_cum { get; private init; } // probably can be removed
        public float[] volume_previous { get; private init; } // probably can be removed
        public float[] volume_mai { get; private init; } // probably can be removed

        // water
        public float[] transp_veg { get; private init; }
        public float[] WUE { get; private init; }
        public float[] WUE_transp { get; private init; }

        // 3-PGmix
        public float[] canopy_vol_frac { get; private init; }
        public float[] crown_length { get; private init; }
        public float[] crown_width { get; private init; }
        public float[] fi { get; private init; } // fraction of apar by species
        public float[] lai_above { get; private init; }
        public float[] lai_sa_ratio { get; private init; } // the ratio of mean tree leaf area (m²) to crownSA (m²)
        public float[] lambda_v { get; private init; } 
        public float[] lambda_h { get; private init; } 
        public int[] layer_id { get; private init; }
        public float[] pFS { get; private init; }

        // 3-PGpjs and δ¹³C
        public float[] canopy_cover { get; private init; }

        // δ¹³C
        public float[] D13CNewPS { get; private init; }
        public float[] D13CTissue { get; private init; }
        public float[] Gc_mol { get; private init; }
        public float[] Gw_mol { get; private init; }
        public float[] InterCi { get; private init; }

        // bias correction
        public float[] CVdbhDistribution { get; private init; }
        public float[] CVwsDistribution { get; private init; }
        public float[] height_rel { get; private init; } // probably can be made local to bias correction
        public float[] DWeibullScale { get; private init; }
        public float[] DWeibullShape { get; private init; }
        public float[] DWeibullLocation { get; private init; }
        public float[] wsWeibullScale { get; private init; }
        public float[] wsWeibullShape { get; private init; }
        public float[] wsWeibullLocation { get; private init; }
        public float[] DrelBiaspFS { get; private init; }
        public float[] DrelBiasheight { get; private init; }
        public float[] DrelBiasBasArea { get; private init; }
        public float[] DrelBiasLCL { get; private init; }
        public float[] DrelBiasCrowndiameter { get; private init; }
        public float[] wsrelBias { get; private init; }

        public ThreePGState(int n_sp, Site site)
        {
            // initialization
            this.adjSolarZenithAngle = site.GetSolarAngleByMonth();
            this.day_length = site.GetMeanDayLengthInSecondsByMonth();
            this.pfsConst = new float[n_sp];
            this.pfsPower = new float[n_sp];
            this.swConst = new float[n_sp];
            this.swPower = new float[n_sp];

            // mangement
            this.t_n = new int[n_sp];

            // modifiers
            this.f_vpd = new float[n_sp];
            this.f_sw = new float[n_sp];
            this.f_nutr = new float[n_sp];
            this.f_phys = new float[n_sp];

            // mortality
            this.mort_manag = new float[n_sp];
            this.mort_stress = new float[n_sp];
            this.mort_thinn = new float[n_sp];

            // growth
            this.aero_resist = new float[n_sp];
            this.air_pressure = Single.NaN;
            this.apar = new float[n_sp];
            this.alpha_c = new float[n_sp];
            this.biom_incr_foliage = new float[n_sp];
            this.biom_incr_root = new float[n_sp];
            this.biom_incr_stem = new float[n_sp];
            this.biom_loss_foliage = new float[n_sp];
            this.biom_loss_root = new float[n_sp];
            this.epsilon = new float[n_sp];
            this.epsilon_gpp = new float[n_sp];
            this.epsilon_npp = new float[n_sp];
            this.epsilon_biom_stem = new float[n_sp];
            this.GPP = new float[n_sp];
            this.NPP = new float[n_sp];
            this.NPP_f = new float[n_sp];
            this.npp_fract_root = new float[n_sp];
            this.npp_fract_stem = new float[n_sp];
            this.npp_fract_foliage = new float[n_sp];
            this.gC = new float[n_sp];
            this.conduct_canopy = new float[n_sp];
            this.m = new float[n_sp];
            this.VPD_sp = new float[n_sp];

            // stand
            this.basal_area = new float[n_sp];
            this.biom_foliage = new float[n_sp];
            this.biom_foliage_debt = new float[n_sp];
            this.biom_root = new float[n_sp];
            this.biom_stem = new float[n_sp];
            this.biom_tree_max = new float[n_sp];
            this.biom_tree = new float[n_sp];
            this.competition_total = 0;

            this.dbh = new float[n_sp];
            this.height = new float[n_sp];
            this.lai = new float[n_sp];
            this.lai_per = new float[n_sp];
            this.stems_n = new float[n_sp];
            this.stems_n_ha = new float[n_sp];

            this.basal_area_prop = new float[n_sp];
            this.volume = new float[n_sp];
            this.volume_change = new float[n_sp];
            this.volume_cum = new float[n_sp];
            this.volume_previous = new float[n_sp];
            this.volume_mai = new float[n_sp];

            // water
            this.transp_veg = new float[n_sp];
            this.WUE = new float[n_sp];
            this.WUE_transp = new float[n_sp];

            // 3-PGmix
            this.canopy_vol_frac = new float[n_sp];
            this.crown_length = new float[n_sp];
            this.crown_width = new float[n_sp];
            this.fi = new float[n_sp];
            this.lai_above = new float[n_sp];
            this.lai_sa_ratio = new float[n_sp];
            this.lambda_v =  new float[n_sp];
            this.lambda_h =  new float[n_sp];
            this.layer_id =  new int[n_sp];
            this.pFS = new float[n_sp];

            // 3-PGpjs and δ¹³C
            this.canopy_cover = new float[n_sp];

            // δ¹³C
            this.D13CNewPS = new float[n_sp];
            this.D13CTissue = new float[n_sp];
            this.Gc_mol = new float[n_sp];
            this.Gw_mol = new float[n_sp];
            this.InterCi = new float[n_sp];

            // bias correction
            this.CVdbhDistribution = new float[n_sp];
            this.CVwsDistribution = new float[n_sp];
            this.height_rel = new float[n_sp];
            this.DWeibullScale = new float[n_sp];
            this.DWeibullShape = new float[n_sp];
            this.DWeibullLocation = new float[n_sp];
            this.wsWeibullScale = new float[n_sp];
            this.wsWeibullShape = new float[n_sp];
            this.wsWeibullLocation = new float[n_sp];
            this.DrelBiaspFS = new float[n_sp];
            this.DrelBiasheight = new float[n_sp];
            this.DrelBiasBasArea = new float[n_sp];
            this.DrelBiasLCL = new float[n_sp];
            this.DrelBiasCrowndiameter = new float[n_sp];
            this.wsrelBias = new float[n_sp];
        }
    }
}
