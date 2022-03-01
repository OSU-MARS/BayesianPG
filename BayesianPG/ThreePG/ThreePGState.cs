using System;

namespace BayesianPG.ThreePG
{
    public class ThreePGState<TFloat, TInteger> 
        where TFloat : struct
        where TInteger : struct
    {
        // initialization
        public float[] adjSolarZenithAngle { get; private init; }
        public float[] day_length { get; private init; }
        public TFloat[] pfsConst { get; private init; }
        public TFloat[] pfsPower { get; private init; }
        public TFloat[] swConst { get; private init; }
        public TFloat[] swPower { get; private init; }

        // management
        public int[] t_n { get; private init; }

        // modifiers
        public TFloat[] f_vpd { get; private init; }
        public TFloat[] f_sw { get; private init; }
        public TFloat[] f_nutr { get; private init; }
        public TFloat[] f_phys { get; private init; }

        // mortality
        public TFloat[] mort_manag { get; private init; }
        public TFloat[] mort_stress { get; private init; }
        public TFloat[] mort_thinn { get; private init; }

        // growth
        public TFloat[] aero_resist { get; private init; }
        public float airPressure { get; set; }
        public TFloat[] apar { get; private init; }
        public TFloat[] alpha_c { get; private init; }
        public TFloat[] biom_incr_foliage { get; private init; }
        public TFloat[] biom_incr_root { get; private init; }
        public TFloat[] biom_incr_stem { get; private init; }
        public TFloat[] biom_loss_foliage { get; private init; }
        public TFloat[] biom_loss_root { get; private init; }
        public TFloat[] conduct_canopy { get; private init; }
        public TFloat[] epsilon { get; private init; }
        public TFloat[] epsilon_gpp { get; private init; }
        public TFloat[] epsilon_npp { get; private init; }
        public TFloat[] epsilon_biom_stem { get; private init; }
        public TFloat[] GPP { get; private init; }
        public TFloat[] NPP { get; private init; }
        public TFloat[] NPP_f { get; private init; } // duplicate of NPP present in Fortran, TODO: remove
        public TFloat[] npp_fract_root { get; private init; }
        public TFloat[] npp_fract_stem { get; private init; }
        public TFloat[] npp_fract_foliage { get; private init; }
        public TFloat[] gC { get; private init; }
        public TFloat[] m { get; private init; }
        public TFloat[] VPD_sp { get; private init; }

        // stand
        public TFloat[] basal_area { get; private init; }
        public TFloat[] basal_area_prop { get; private init; }

        public TFloat[] biom_foliage { get; private init; }
        public TFloat[] biom_foliage_debt { get; private init; }
        public TFloat[] biom_root { get; private init; }
        public TFloat[] biom_stem { get; private init; }
        public TFloat[] biom_tree { get; private init; } // mean stem mass, kg
        public TFloat[] biom_tree_max { get; private init; }

        public TFloat aSW { get; set; }
        public TFloat competition_total { get; set; }

        public TFloat[] dbh { get; private init; }
        public TFloat[] lai { get; private init; }
        public TFloat[] lai_per { get; private init; }
        public TFloat[] height { get; private init; }
        public TFloat[] stems_n { get; private init; }
        public TFloat[] stems_n_ha { get; private init; }

        public TFloat[] volume { get; private init; }
        public TFloat[] volume_change { get; private init; } // probably can be removed
        public TFloat[] volume_cum { get; private init; } // probably can be removed
        public TFloat[] volume_previous { get; private init; } // probably can be removed
        public TFloat[] volume_mai { get; private init; } // probably can be removed

        // water
        public TFloat[] transp_veg { get; private init; }
        public TFloat[] WUE { get; private init; }
        public TFloat[] WUE_transp { get; private init; }

        // 3-PGmix
        public TFloat[] canopy_vol_frac { get; private init; }
        public TFloat[] crown_length { get; private init; }
        public TFloat[] crown_width { get; private init; }
        public TFloat[] fi { get; private init; } // fraction of apar by species
        public TFloat[] lai_above { get; private init; }
        public TFloat[] lai_sa_ratio { get; private init; } // the ratio of mean tree leaf area (m²) to crownSA (m²)
        public TFloat[] lambda_v { get; private init; } 
        public TFloat[] lambda_h { get; private init; } 
        public TInteger[] layer_id { get; private init; }
        public TFloat[] pFS { get; private init; }

        // 3-PGpjs and δ¹³C
        public TFloat[] canopy_cover { get; private init; }

        // δ¹³C
        public TFloat[] D13CNewPS { get; private init; }
        public TFloat[] D13CTissue { get; private init; }
        public TFloat[] Gc_mol { get; private init; }
        public TFloat[] Gw_mol { get; private init; }
        public TFloat[] InterCi { get; private init; }

        // bias correction
        public TFloat[] CVdbhDistribution { get; private init; }
        public TFloat[] CVwsDistribution { get; private init; }
        public TFloat[] height_rel { get; private init; } // probably can be made local to bias correction
        public TFloat[] DWeibullScale { get; private init; }
        public TFloat[] DWeibullShape { get; private init; }
        public TFloat[] DWeibullLocation { get; private init; }
        public TFloat[] wsWeibullScale { get; private init; }
        public TFloat[] wsWeibullShape { get; private init; }
        public TFloat[] wsWeibullLocation { get; private init; }
        public TFloat[] DrelBiaspFS { get; private init; }
        public TFloat[] DrelBiasheight { get; private init; }
        public TFloat[] DrelBiasBasArea { get; private init; }
        public TFloat[] DrelBiasLCL { get; private init; }
        public TFloat[] DrelBiasCrowndiameter { get; private init; }
        public TFloat[] wsrelBias { get; private init; }

        public ThreePGState(int n_sp, Site site)
        {
            // initialization
            this.adjSolarZenithAngle = site.GetSolarAngleByMonth();
            this.day_length = site.GetMeanDayLengthInSecondsByMonth();
            this.pfsConst = new TFloat[n_sp];
            this.pfsPower = new TFloat[n_sp];
            this.swConst = new TFloat[n_sp];
            this.swPower = new TFloat[n_sp];

            // mangement
            this.t_n = new int[n_sp];

            // modifiers
            this.f_vpd = new TFloat[n_sp];
            this.f_sw = new TFloat[n_sp];
            this.f_nutr = new TFloat[n_sp];
            this.f_phys = new TFloat[n_sp];

            // mortality
            this.mort_manag = new TFloat[n_sp];
            this.mort_stress = new TFloat[n_sp];
            this.mort_thinn = new TFloat[n_sp];

            // growth
            this.aero_resist = new TFloat[n_sp];
            this.airPressure = Single.NaN;
            this.apar = new TFloat[n_sp];
            this.alpha_c = new TFloat[n_sp];
            this.biom_incr_foliage = new TFloat[n_sp];
            this.biom_incr_root = new TFloat[n_sp];
            this.biom_incr_stem = new TFloat[n_sp];
            this.biom_loss_foliage = new TFloat[n_sp];
            this.biom_loss_root = new TFloat[n_sp];
            this.epsilon = new TFloat[n_sp];
            this.epsilon_gpp = new TFloat[n_sp];
            this.epsilon_npp = new TFloat[n_sp];
            this.epsilon_biom_stem = new TFloat[n_sp];
            this.GPP = new TFloat[n_sp];
            this.NPP = new TFloat[n_sp];
            this.NPP_f = new TFloat[n_sp];
            this.npp_fract_root = new TFloat[n_sp];
            this.npp_fract_stem = new TFloat[n_sp];
            this.npp_fract_foliage = new TFloat[n_sp];
            this.gC = new TFloat[n_sp];
            this.conduct_canopy = new TFloat[n_sp];
            this.m = new TFloat[n_sp];
            this.VPD_sp = new TFloat[n_sp];

            // stand
            this.aSW = new();
            this.basal_area = new TFloat[n_sp];
            this.biom_foliage = new TFloat[n_sp];
            this.biom_foliage_debt = new TFloat[n_sp];
            this.biom_root = new TFloat[n_sp];
            this.biom_stem = new TFloat[n_sp];
            this.biom_tree_max = new TFloat[n_sp];
            this.biom_tree = new TFloat[n_sp];

            this.competition_total = new();
            this.dbh = new TFloat[n_sp];
            this.height = new TFloat[n_sp];
            this.lai = new TFloat[n_sp];
            this.lai_per = new TFloat[n_sp];
            this.stems_n = new TFloat[n_sp];
            this.stems_n_ha = new TFloat[n_sp];

            this.basal_area_prop = new TFloat[n_sp];
            this.volume = new TFloat[n_sp];
            this.volume_change = new TFloat[n_sp];
            this.volume_cum = new TFloat[n_sp];
            this.volume_previous = new TFloat[n_sp];
            this.volume_mai = new TFloat[n_sp];

            // water
            this.transp_veg = new TFloat[n_sp];
            this.WUE = new TFloat[n_sp];
            this.WUE_transp = new TFloat[n_sp];

            // 3-PGmix
            this.canopy_vol_frac = new TFloat[n_sp];
            this.crown_length = new TFloat[n_sp];
            this.crown_width = new TFloat[n_sp];
            this.fi = new TFloat[n_sp];
            this.lai_above = new TFloat[n_sp];
            this.lai_sa_ratio = new TFloat[n_sp];
            this.lambda_v =  new TFloat[n_sp];
            this.lambda_h =  new TFloat[n_sp];
            this.layer_id =  new TInteger[n_sp];
            this.pFS = new TFloat[n_sp];

            // 3-PGpjs and δ¹³C
            this.canopy_cover = new TFloat[n_sp];

            // δ¹³C
            this.D13CNewPS = new TFloat[n_sp];
            this.D13CTissue = new TFloat[n_sp];
            this.Gc_mol = new TFloat[n_sp];
            this.Gw_mol = new TFloat[n_sp];
            this.InterCi = new TFloat[n_sp];

            // bias correction
            this.CVdbhDistribution = new TFloat[n_sp];
            this.CVwsDistribution = new TFloat[n_sp];
            this.height_rel = new TFloat[n_sp];
            this.DWeibullScale = new TFloat[n_sp];
            this.DWeibullShape = new TFloat[n_sp];
            this.DWeibullLocation = new TFloat[n_sp];
            this.wsWeibullScale = new TFloat[n_sp];
            this.wsWeibullShape = new TFloat[n_sp];
            this.wsWeibullLocation = new TFloat[n_sp];
            this.DrelBiaspFS = new TFloat[n_sp];
            this.DrelBiasheight = new TFloat[n_sp];
            this.DrelBiasBasArea = new TFloat[n_sp];
            this.DrelBiasLCL = new TFloat[n_sp];
            this.DrelBiasCrowndiameter = new TFloat[n_sp];
            this.wsrelBias = new TFloat[n_sp];
        }

        public float GetDayLength(DateTime timestepEndDate)
        {
            return this.day_length[timestepEndDate.Month - 1];
        }

        public float GetSolarZenithAngle(DateTime timestepEndDate)
        {
            return this.adjSolarZenithAngle[timestepEndDate.Month - 1];
        }
    }
}
