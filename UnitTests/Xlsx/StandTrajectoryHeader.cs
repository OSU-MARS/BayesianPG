using BayesianPG.ThreePG;
using BayesianPG.Xlsx;
using System;
using System.Xml;

namespace BayesianPG.Test.Xlsx
{
    public class StandTrajectoryHeader : IXlsxWorksheetHeader
    {
        public ThreePGStandTrajectoryColumnGroups ColumnGroups { get; private set; }

        public int aero_resist { get; private set; }
        public int age { get; private set; }
        public int alpha_c { get; private set; }
        public int asw { get; private set; }
        public int basal_area { get; private set; }
        public int basal_area_prop { get; private set; }
        public int biom_foliage { get; private set; }
        public int biom_foliage_debt { get; private set; }
        public int biom_root { get; private set; }
        public int biom_stem { get; private set; }
        public int biom_tree { get; private set; }
        public int biom_tree_max { get; private set; }
        public int canopy_cover { get; private set; }
        public int canopy_vol_frac { get; private set; }
        public int conduct_canopy { get; private set; }
        public int conduct_soil { get; private set; }
        public int crown_length { get; private set; }
        public int crown_width { get; private set; }
        public int CVdbhDistribution { get; private set; }
        public int CVwsDistribution { get; private set; }
        public int D13CNewPS { get; private set; }
        public int D13CTissue { get; private set; }
        public int date { get; private set; }
        public int dbh { get; private set; }
        public int DrelBiasBasArea { get; private set; }
        public int DrelBiasCrowndiameter { get; private set; }
        public int DrelBiasheight { get; private set; }
        public int DrelBiasLCL { get; private set; }
        public int DrelBiaspFS { get; private set; }
        public int DWeibullLocation { get; private set; }
        public int DWeibullScale { get; private set; }
        public int DWeibullShape { get; private set; }
        public int epsilon_biom_stem { get; private set; }
        public int epsilon_gpp { get; private set; }
        public int epsilon_npp { get; private set; }
        public int evapo_transp { get; private set; }
        public int evapotra_soil { get; private set; }
        public int f_age { get; private set; }
        public int f_calpha { get; private set; }
        public int f_cg { get; private set; }
        public int f_frost { get; private set; }
        public int f_nutr { get; private set; }
        public int f_phys { get; private set; }
        public int f_sw { get; private set; }
        public int f_tmp { get; private set; }
        public int f_tmp_gc { get; private set; }
        public int f_transp_scale { get; private set; }
        public int f_vpd { get; private set; }
        public int fi { get; private set; }
        public int fi1 { get; private set; }
        public int fracBB { get; private set; }
        public int frost_days { get; private set; }
        public int gammaF { get; private set; }
        public int gammaN { get; private set; }
        public int Gc_mol { get; private set; }
        public int gpp { get; private set; }
        public int Gw_mol { get; private set; }
        public int height { get; private set; }
        public int height_rel { get; private set; }
        public int InterCi { get; private set; }
        public int irrig_supl { get; private set; }
        public int lai { get; private set; }
        public int lai_above { get; private set; }
        public int lai_sa_ratio { get; private set; }
        public int lambda_h { get; private set; }
        public int lambda_v { get; private set; }
        public int layer_id { get; private set; }
        public int mort_stress { get; private set; }
        public int mort_thinn { get; private set; }
        public int npp_f { get; private set; }
        public int npp_fract_foliage { get; private set; }
        public int npp_fract_root { get; private set; }
        public int npp_fract_stem { get; private set; }
        public int prcp { get; private set; }
        public int prcp_interc { get; private set; }
        public int prcp_runoff { get; private set; }
        public int sla { get; private set; }
        public int species { get; private set; }
        public int stems_n { get; private set; }
        public int transp_veg { get; private set; }
        public int volume { get; private set; }
        public int vpd_day { get; private set; }
        public int vpd_sp { get; private set; }
        public int wood_density { get; private set; }
        public int wsrelBias { get; private set; }
        public int wsWeibullLocation { get; private set; }
        public int wsWeibullScale { get; private set; }
        public int wsWeibullShape { get; private set; }
        public int wue { get; private set; }
        public int wue_transp { get; private set; }

        // extended columns
        // These are either increments either trivially computed from other columns or repetition of monthly
        // "climate" that's already in the climate tables.
        public int apar { get; private set; }
        public int biom_incr_foliage { get; private set; }
        public int biom_incr_root { get; private set; }
        public int biom_incr_stem { get; private set; }
        public int biom_loss_foliage { get; private set; }
        public int biom_loss_root { get; private set; }
        public int co2 { get; private set; }
        public int day_length { get; private set; }
        public int delta13c { get; private set; }
        public int pFS { get; private set; }
        public int prcp_interc_frac { get; private set; }
        public int solar_rad { get; private set; }
        public int tmp_ave { get; private set; }
        public int tmp_max { get; private set; }
        public int tmp_min { get; private set; }
        public int volume_change { get; private set; }
        public int volume_cum { get; private set; }
        public int volume_extracted { get; private set; }
        public int volume_mai { get; private set; }
        public int water_runoff_pooled { get; private set; }

        public StandTrajectoryHeader()
        {
            this.ColumnGroups = ThreePGStandTrajectoryColumnGroups.Core;

            this.aero_resist = -1;
            this.age = -1;
            this.alpha_c = -1;
            this.apar = -1;
            this.asw = -1;
            this.basal_area = -1;
            this.basal_area_prop = -1;
            this.biom_foliage = -1;
            this.biom_foliage_debt = -1;
            this.biom_incr_foliage = -1;
            this.biom_incr_root = -1;
            this.biom_incr_stem = -1;
            this.biom_loss_foliage = -1;
            this.biom_loss_root = -1;
            this.biom_root = -1;
            this.biom_stem = -1;
            this.biom_tree = -1;
            this.biom_tree_max = -1;
            this.canopy_cover = -1;
            this.canopy_vol_frac = -1;
            this.co2 = -1;
            this.conduct_canopy = -1;
            this.conduct_soil = -1;
            this.crown_length = -1;
            this.crown_width = -1;
            this.CVdbhDistribution = -1;
            this.CVwsDistribution = -1;
            this.D13CNewPS = -1;
            this.D13CTissue = -1;
            this.date = -1;
            this.day_length = -1;
            this.dbh = -1;
            this.delta13c = -1;
            this.DrelBiasBasArea = -1;
            this.DrelBiasCrowndiameter = -1;
            this.DrelBiasheight = -1;
            this.DrelBiasLCL = -1;
            this.DrelBiaspFS = -1;
            this.DWeibullLocation = -1;
            this.DWeibullScale = -1;
            this.DWeibullShape = -1;
            this.epsilon_biom_stem = -1;
            this.epsilon_gpp = -1;
            this.epsilon_npp = -1;
            this.evapo_transp = -1;
            this.evapotra_soil = -1;
            this.f_age = -1;
            this.f_calpha = -1;
            this.f_cg = -1;
            this.f_frost = -1;
            this.f_nutr = -1;
            this.f_phys = -1;
            this.f_sw = -1;
            this.f_tmp = -1;
            this.f_tmp_gc = -1;
            this.f_transp_scale = -1;
            this.f_vpd = -1;
            this.fi = -1;
            this.fi1 = -1;
            this.fracBB = -1;
            this.frost_days = -1;
            this.gammaF = -1;
            this.gammaN = -1;
            this.Gc_mol = -1;
            this.gpp = -1;
            this.Gw_mol = -1;
            this.height = -1;
            this.height_rel = -1;
            this.InterCi = -1;
            this.irrig_supl = -1;
            this.lai = -1;
            this.lai_above = -1;
            this.lai_sa_ratio = -1;
            this.lambda_h = -1;
            this.lambda_v = -1;
            this.layer_id = -1;
            this.mort_stress = -1;
            this.mort_thinn = -1;
            this.npp_f = -1;
            this.npp_fract_foliage = -1;
            this.npp_fract_root = -1;
            this.npp_fract_stem = -1;
            this.pFS = -1;
            this.prcp = -1;
            this.prcp_interc = -1;
            this.prcp_interc_frac = -1;
            this.prcp_runoff = -1;
            this.sla = -1;
            this.solar_rad = -1;
            this.species = -1;
            this.stems_n = -1;
            this.tmp_ave = -1;
            this.tmp_ave = -1;
            this.tmp_max = -1;
            this.tmp_min = -1;
            this.transp_veg = -1;
            this.volume = -1;
            this.volume_change = -1;
            this.volume_cum = -1;
            this.volume_extracted = -1;
            this.volume_mai = -1;
            this.vpd_day = -1;
            this.vpd_sp = -1;
            this.water_runoff_pooled = -1;
            this.wood_density = -1;
            this.wsrelBias = -1;
            this.wsWeibullLocation = -1;
            this.wsWeibullScale = -1;
            this.wsWeibullShape = -1;
            this.wue = -1;
            this.wue_transp = -1;
        }

        public void Parse(XlsxRow header)
        {
            for (int index = 0; index < header.Columns; ++index)
            {
                string column = header.Row[index];
                switch (column)
                {
                    case "date":
                        this.date = index;
                        break;
                    case "species":
                        this.species = index;
                        break;
                    case "tmp_min":
                        this.tmp_min = index;
                        break;
                    case "age":
                        this.age = index;
                        break;
                    case "layer_id":
                        this.layer_id = index;
                        break;
                    case "biom_stem":
                        this.biom_stem = index;
                        break;
                    case "f_age":
                        this.f_age = index;
                        break;
                    case "gpp":
                        this.gpp = index;
                        break;
                    case "conduct_canopy":
                        this.conduct_canopy = index;
                        break;
                    case "biom_tree_max":
                        this.biom_tree_max = index;
                        break;
                    case "Gc_mol":
                        this.Gc_mol = index;
                        break;
                    case "Dweibullscale": // r3PG output (versus internal) casing
                    case "DWeibullScale":
                        this.DWeibullScale = index;
                        break;
                    case "tmp_max":
                        this.tmp_max = index;
                        break;
                    case "stems_n":
                        this.stems_n = index;
                        break;
                    case "sla":
                        this.sla = index;
                        break;
                    case "biom_root":
                        this.biom_root = index;
                        break;
                    case "f_vpd":
                        this.f_vpd = index;
                        break;
                    // for now, handle // https://github.com/trotsiuk/r3PG/issues/69 by assuming npp means npp_f
                    case "npp":
                    case "npp_f":
                        this.npp_f = index;
                        break;
                    case "conduct_soil":
                        this.conduct_soil = index;
                        break;
                    case "gammaN":
                        this.gammaN = index;
                        break;
                    case "Gw_mol":
                        this.Gw_mol = index;
                        break;
                    case "Dweibullshape": // r3PG output (versus internal) casing
                    case "DWeibullShape":
                        this.DWeibullShape = index;
                        break;
                    case "tmp_ave":
                        this.tmp_ave = index;
                        break;
                    case "basal_area":
                        this.basal_area = index;
                        break;
                    case "lai":
                        this.lai = index;
                        break;
                    case "biom_foliage":
                        this.biom_foliage = index;
                        break;
                    case "f_tmp":
                        this.f_tmp = index;
                        break;
                    case "apar":
                        this.apar = index;
                        break;
                    case "evapotra_soil":
                        this.evapotra_soil = index;
                        break;
                    case "mort_thinn":
                        this.mort_thinn = index;
                        break;
                    case "D13CNewPS":
                        this.D13CNewPS = index;
                        break;
                    case "Dweibulllocation": // r3PG output (versus internal) casing
                    case "DWeibullLocation":
                        this.DWeibullLocation = index;
                        break;
                    case "frost_days":
                        this.frost_days = index;
                        break;
                    case "basal_area_prop":
                        this.basal_area_prop = index;
                        break;
                    case "lai_above":
                        this.lai_above = index;
                        break;
                    case "biom_tree":
                        this.biom_tree = index;
                        break;
                    case "f_tmp_gc":
                        this.f_tmp_gc = index;
                        break;
                    case "fi":
                        this.fi = index;
                        break;
                    case "prcp_interc":
                        this.prcp_interc = index;
                        break;
                    case "mort_stress":
                        this.mort_stress = index;
                        break;
                    case "D13CTissue":
                        this.D13CTissue = index;
                        break;
                    case "wsweibullscale": // r3PG output (versus internal) casing
                    case "wsWeibullScale":
                        this.wsWeibullScale = index;
                        break;
                    case "solar_rad":
                        this.solar_rad = index;
                        break;
                    case "dbh":
                        this.dbh = index;
                        break;
                    case "lai_sa_ratio":
                        this.lai_sa_ratio = index;
                        break;
                    case "wood_density":
                        this.wood_density = index;
                        break;
                    case "f_frost":
                        this.f_frost = index;
                        break;
                    case "alpha_c":
                        this.alpha_c = index;
                        break;
                    case "InterCi":
                        this.InterCi = index;
                        break;
                    case "wsweibullshape": // r3PG output (versus Fortran) casing
                    case "wsWeibullShape":
                        this.wsWeibullShape = index;
                        break;
                    case "day_length":
                        this.day_length = index;
                        break;
                    case "height":
                        this.height = index;
                        break;
                    case "canopy_vol_frac":
                        this.canopy_vol_frac = index;
                        break;
                    case "fracBB":
                        this.fracBB = index;
                        break;
                    case "f_sw":
                        this.f_sw = index;
                        break;
                    case "epsilon_gpp":
                        this.epsilon_gpp = index;
                        break;
                    case "prcp_runoff":
                        this.prcp_runoff = index;
                        break;
                    case "volume_extracted":
                        this.volume_extracted = index;
                        break;
                    case "wsweibulllocation": // r3PG output (versus Fortran) casing
                    case "wsWeibullLocation":
                        this.wsWeibullLocation = index;
                        break;
                    case "prcp":
                        this.prcp = index;
                        break;
                    case "height_rel":
                        this.height_rel = index;
                        break;
                    case "canopy_cover":
                        this.canopy_cover = index;
                        break;
                    case "biom_loss_foliage":
                        this.biom_loss_foliage = index;
                        break;
                    case "f_nutr":
                        this.f_nutr = index;
                        break;
                    case "epsilon_npp":
                        this.epsilon_npp = index;
                        break;
                    case "irrig_supl":
                        this.irrig_supl = index;
                        break;
                    case "CVdbhDistribution":
                        this.CVdbhDistribution = index;
                        break;
                    case "vpd_day":
                        this.vpd_day = index;
                        break;
                    case "crown_length":
                        this.crown_length = index;
                        break;
                    case "lambda_v":
                        this.lambda_v = index;
                        break;
                    case "biom_loss_root":
                        this.biom_loss_root = index;
                        break;
                    case "f_calpha":
                        this.f_calpha = index;
                        break;
                    case "epsilon_biom_stem":
                        this.epsilon_biom_stem = index;
                        break;
                    case "wue":
                        this.wue = index;
                        break;
                    case "CVwsDistribution":
                        this.CVwsDistribution = index;
                        break;
                    case "co2":
                        this.co2 = index;
                        break;
                    case "crown_width":
                        this.crown_width = index;
                        break;
                    case "lambda_h":
                        this.lambda_h = index;
                        break;
                    case "biom_incr_foliage":
                        this.biom_incr_foliage = index;
                        break;
                    case "f_cg":
                        this.f_cg = index;
                        break;
                    case "npp_fract_stem":
                        this.npp_fract_stem = index;
                        break;
                    case "wue_transp":
                        this.wue_transp = index;
                        break;
                    case "wsrelBias":
                        this.wsrelBias = index;
                        break;
                    case "delta13c":
                        this.delta13c = index;
                        break;
                    case "volume":
                        this.volume = index;
                        break;
                    case "aero_resist":
                        this.aero_resist = index;
                        break;
                    case "biom_incr_root":
                        this.biom_incr_root = index;
                        break;
                    case "f_phys":
                        this.f_phys = index;
                        break;
                    case "npp_fract_foliage":
                        this.npp_fract_foliage = index;
                        break;
                    case "evapo_transp":
                        this.evapo_transp = index;
                        break;
                    case "DrelBiaspFS":
                        this.DrelBiaspFS = index;
                        break;
                    case "volume_mai":
                        this.volume_mai = index;
                        break;
                    case "vpd_sp":
                        this.vpd_sp = index;
                        break;
                    case "biom_incr_stem":
                        this.biom_incr_stem = index;
                        break;
                    case "gammaF":
                        this.gammaF = index;
                        break;
                    case "npp_fract_root":
                        this.npp_fract_root = index;
                        break;
                    case "transp_veg":
                        this.transp_veg = index;
                        break;
                    case "DrelBiasheight":
                        this.DrelBiasheight = index;
                        break;
                    case "volume_change":
                        this.volume_change = index;
                        break;
                    case "f_transp_scale":
                        this.f_transp_scale = index;
                        break;
                    case "pFS":
                        this.pFS = index;
                        break;
                    case "asw":
                        this.asw = index;
                        break;
                    case "DrelBiasBasArea":
                        this.DrelBiasBasArea = index;
                        break;
                    case "volume_cum":
                        this.volume_cum = index;
                        break;
                    case "biom_foliage_debt":
                        this.biom_foliage_debt = index;
                        break;
                    case "water_runoff_polled": // Fortran typo
                        this.water_runoff_pooled = index;
                        break;
                    case "DrelBiasLCL":
                        this.DrelBiasLCL = index;
                        break;
                    case "DrelBiasCrowndiameter":
                        this.DrelBiasCrowndiameter = index;
                        break;
                    default:
                        throw new NotSupportedException("Unhandled column name '" + column + "'.");
                }
            }

            // verify presence of required columns
            if (this.aero_resist < 0)
            {
                throw new XmlException("Aerodynamic resistance column not found in stand trajectory header.");
            }
            if (this.age < 0)
            {
                throw new XmlException("Age column not found in stand trajectory header.");
            }
            if (this.alpha_c < 0)
            {
                throw new XmlException("alphaC column not found in stand trajectory header.");
            }
            if (this.asw < 0)
            {
                throw new XmlException("Available soil water column not found in stand trajectory header.");
            }
            if (this.basal_area < 0)
            {
                throw new XmlException("Basal area column not found in stand trajectory header.");
            }
            if (this.basal_area_prop < 0)
            {
                throw new XmlException("Basal area proportion column not found in stand trajectory header.");
            }
            if (this.biom_foliage < 0)
            {
                throw new XmlException("Foliage biomass column not found in stand trajectory header.");
            }
            if (this.biom_foliage_debt < 0)
            {
                throw new XmlException("Foliage biomass debt column not found in stand trajectory header.");
            }
            if (this.biom_stem < 0)
            {
                throw new XmlException("Stem biomass column not found in stand trajectory header.");
            }
            if (this.biom_root < 0)
            {
                throw new XmlException("Root biomass column not found in stand trajectory header.");
            }
            if (this.biom_tree < 0)
            {
                throw new XmlException("Mean tree biomass column not found in stand trajectory header.");
            }
            if (this.biom_tree_max < 0)
            {
                throw new XmlException("Maximum tree biomass column not found in stand trajectory header.");
            }
            if (this.conduct_canopy < 0)
            {
                throw new XmlException("Canopy conductance column not found in stand trajectory header.");
            }
            if (this.canopy_vol_frac < 0)
            {
                throw new XmlException("Canopy volume fraction column not found in stand trajectory header.");
            }
            if (this.conduct_soil < 0)
            {
                throw new XmlException("Soil conductivity column not found in stand trajectory header.");
            }
            if (this.crown_length < 0)
            {
                throw new XmlException("Crown length column not found in stand trajectory header.");
            }
            if (this.crown_width < 0)
            {
                throw new XmlException("Crown width column not found in stand trajectory header.");
            }
            if (this.date < 0)
            {
                throw new XmlException("Date column not found in stand trajectory header.");
            }
            if (this.dbh < 0)
            {
                throw new XmlException("DBH column not found in stand trajectory header.");
            }
            if (this.epsilon_biom_stem < 0)
            {
                throw new XmlException("Epsilon column for stem biomass not found in stand trajectory header.");
            }
            if (this.epsilon_gpp < 0)
            {
                throw new XmlException("Epsilon column for GPP (gross primary productivity) not found in stand trajectory header.");
            }
            if (this.epsilon_npp < 0)
            {
                throw new XmlException("Epsilon column for NPP (net primary productivity) not found in stand trajectory header.");
            }
            if (this.evapotra_soil < 0)
            {
                throw new XmlException("Soil evapotranspiration column not found in stand trajectory header.");
            }
            if (this.evapo_transp < 0)
            {
                throw new XmlException("Evapotranspiration column not found in stand trajectory header.");
            }
            if (this.f_age < 0)
            {
                throw new XmlException("Age modifier column not found in stand trajectory header.");
            }
            if (this.f_calpha < 0)
            {
                throw new XmlException("alphaC modifier column not found in stand trajectory header.");
            }
            if (this.f_cg < 0)
            {
                throw new XmlException("Cg modifier column not found in stand trajectory header.");
            }
            if (this.f_frost < 0)
            {
                throw new XmlException("Frost modifier column not found in stand trajectory header.");
            }
            if (this.f_nutr < 0)
            {
                throw new XmlException("Nutrition modifier column not found in stand trajectory header.");
            }
            if (this.f_phys < 0)
            {
                throw new XmlException("Physiology modifier column not found in stand trajectory header.");
            }
            if (this.f_sw < 0)
            {
                throw new XmlException("Soil water modifier column not found in stand trajectory header.");
            }
            if (this.f_tmp < 0)
            {
                throw new XmlException("Temperature modifier column not found in stand trajectory header.");
            }
            if (this.f_tmp_gc < 0)
            {
                throw new XmlException("Temperature Gc modifier column not found in stand trajectory header.");
            }
            if (this.f_vpd < 0)
            {
                throw new XmlException("Vapor pressure deficit modifier column not found in stand trajectory header.");
            }
            if (this.fracBB < 0)
            {
                throw new XmlException("Branch and bark fraction column not found in stand trajectory header.");
            }
            if (this.gammaF < 0)
            {
                throw new XmlException("gammaF column not found in stand trajectory header.");
            }
            if (this.gammaN < 0)
            {
                throw new XmlException("gammaN column not found in stand trajectory header.");
            }
            if (this.gpp < 0)
            {
                throw new XmlException("GPP column not found in stand trajectory header.");
            }
            if (this.height < 0)
            {
                throw new XmlException("Height column not found in stand trajectory header.");
            }
            if (this.irrig_supl < 0)
            {
                throw new XmlException("Irrigation column not found in stand trajectory header.");
            }
            if (this.lai < 0)
            {
                throw new XmlException("Leaf area index column not found in stand trajectory header.");
            }
            if (this.lai_above < 0)
            {
                throw new XmlException("Leaf area above species column not found in stand trajectory header.");
            }
            if (this.lai_sa_ratio < 0)
            {
                throw new XmlException("Leaf area to surface area ratio column not found in stand trajectory header.");
            }
            if (this.lambda_h < 0)
            {
                throw new XmlException("lambdaH column not found in stand trajectory header.");
            }
            if (this.lambda_v < 0)
            {
                throw new XmlException("lambdaV column not found in stand trajectory header.");
            }
            if (this.layer_id < 0)
            {
                throw new XmlException("Layer ID column not found in stand trajectory header.");
            }
            if (this.mort_stress < 0)
            {
                throw new XmlException("Stress mortality column not found in stand trajectory header.");
            }
            if (this.mort_thinn < 0)
            {
                throw new XmlException("Thinning mortality not found in stand trajectory header.");
            }
            if (this.npp_fract_foliage < 0)
            {
                throw new XmlException("Foliage NPP fraction column not found in stand trajectory header.");
            }
            if (this.npp_f < 0)
            {
                throw new XmlException("NPP column not found in stand trajectory header.");
            }
            if (this.npp_fract_foliage < 0)
            {
                throw new XmlException("Foliage NPP fraction column not found in stand trajectory header.");
            }
            if (this.npp_fract_root < 0)
            {
                throw new XmlException("Root NPP fraction column not found in stand trajectory header.");
            }
            if (this.npp_fract_stem < 0)
            {
                throw new XmlException("Stem NPP fraction column not found in stand trajectory header.");
            }
            if (this.prcp_interc < 0)
            {
                throw new XmlException("Precipitation interception column not found in stand trajectory header.");
            }
            if (this.prcp_runoff < 0)
            {
                throw new XmlException("Runoff column not found in stand trajectory header.");
            }
            if (this.sla < 0)
            {
                throw new XmlException("Specific leaf area column not found in stand trajectory header.");
            }
            if (this.stems_n < 0)
            {
                throw new XmlException("Stem count column not found in stand trajectory header.");
            }
            if (this.species < 0)
            {
                throw new XmlException("Species column not found in stand trajectory header.");
            }
            if (this.stems_n < 0)
            {
                throw new XmlException("Stems per hectare column not found in stand trajectory header.");
            }
            if (this.volume < 0)
            {
                throw new XmlException("Volume column not found in stand trajectory header.");
            }
            if (this.vpd_sp < 0)
            {
                throw new XmlException("Species vapor pressure deficit column not found in stand trajectory header.");
            }
            if (this.wood_density < 0)
            {
                throw new XmlException("Wood density column not found in stand trajectory header.");
            }
            if (this.wue < 0)
            {
                throw new XmlException("Water use efficiency column not found in stand trajectory header.");
            }
            if (this.wue_transp < 0)
            {
                throw new XmlException("Water transpiration efficiency column not found in stand trajectory header.");
            }

            // bias correction columns
            bool hasBiasColumns = (this.CVdbhDistribution >= 0) ||
                                  (this.CVwsDistribution >= 0) ||
                                  (this.DrelBiasBasArea >= 0) ||
                                  (this.DrelBiasCrowndiameter >= 0) ||
                                  (this.DrelBiasLCL >= 0) ||
                                  (this.DrelBiaspFS >= 0) ||
                                  (this.DWeibullLocation >= 0) ||
                                  (this.DWeibullScale >= 0) ||
                                  (this.DWeibullShape >= 0) ||
                                  (this.height_rel >= 0) ||
                                  (this.wsrelBias >= 0) ||
                                  (this.wsWeibullLocation >= 0) ||
                                  (this.wsWeibullScale >= 0) ||
                                  (this.wsWeibullShape >= 0);
            if (hasBiasColumns)
            {
                if (this.CVdbhDistribution < 0)
                {
                    throw new XmlException("CVdbhDistribution column not found in stand trajectory header.");
                }
                if (this.CVwsDistribution < 0)
                {
                    throw new XmlException("CVwsDistribution column not found in stand trajectory header.");
                }
                if (this.DrelBiasBasArea < 0)
                {
                    throw new XmlException("DrelBiasBasArea column not found in stand trajectory header.");
                }
                if (this.DrelBiasCrowndiameter < 0)
                {
                    throw new XmlException("DrelBiasCrowndiameter column not found in stand trajectory header.");
                }
                if (this.DrelBiasheight < 0)
                {
                    throw new XmlException("DrelBiasheight column not found in stand trajectory header.");
                }
                if (this.DrelBiasLCL < 0)
                {
                    throw new XmlException("Day length column not found in stand trajectory header.");
                }
                if (this.DrelBiaspFS < 0)
                {
                    throw new XmlException("DrelBiaspFS column not found in stand trajectory header.");
                }
                if (this.DWeibullLocation < 0)
                {
                    throw new XmlException("DWeibullLocation column not found in stand trajectory header.");
                }
                if (this.DWeibullScale < 0)
                {
                    throw new XmlException("DWeibullScale column not found in stand trajectory header.");
                }
                if (this.DWeibullShape < 0)
                {
                    throw new XmlException("DWeibullShape column not found in stand trajectory header.");
                }
                if (this.height_rel < 0)
                {
                    throw new XmlException("Relative height column not found in stand trajectory header.");
                }
                if (this.wsrelBias < 0)
                {
                    throw new XmlException("wsrelBias column not found in stand trajectory header.");
                }
                if (this.wsWeibullLocation < 0)
                {
                    throw new XmlException("WS Weibull location column not found in stand trajectory header.");
                }
                if (this.wsWeibullScale < 0)
                {
                    throw new XmlException("WS Weibull scale column not found in stand trajectory header.");
                }
                if (this.wsWeibullShape < 0)
                {
                    throw new XmlException("WS Weibull shape column not found in stand trajectory header.");
                }

                this.ColumnGroups |= ThreePGStandTrajectoryColumnGroups.BiasCorrection;
            }

            // δ13C columns
            bool hasD13Ccolumns = (this.D13CNewPS >= 0) ||
                                  (this.D13CTissue >= 0) ||
                                  (this.InterCi >= 0);
            if (hasD13Ccolumns)
            {
                if (this.D13CNewPS < 0)
                {
                    throw new XmlException("D13CNewPS column not found in stand trajectory header.");
                }
                if (this.D13CTissue < 0)
                {
                    throw new XmlException("D13CTissue column not found in stand trajectory header.");
                }
                if (this.InterCi < 0)
                {
                    throw new XmlException("InterCi column not found in stand trajectory header.");
                }

                this.ColumnGroups |= ThreePGStandTrajectoryColumnGroups.D13C;
            }

            // extended columns logged from r3PG but not currently logged from C#
            // If any extended column is present require all columns be present.
            bool hasBayesianPGextendedColumns = (this.biom_incr_foliage >= 0) ||
                                                (this.biom_incr_root >= 0) ||
                                                (this.biom_incr_stem >= 0) ||
                                                (this.biom_loss_foliage >= 0) ||
                                                (this.biom_loss_root >= 0) ||
                                                (this.volume_cum >= 0);
            if (hasBayesianPGextendedColumns)
            {
                if (this.biom_incr_foliage < 0)
                {
                    throw new XmlException("Foliage biomass increment column not found in stand trajectory header.");
                }
                if (this.biom_incr_root < 0)
                {
                    throw new XmlException("Root biomass increment column not found in stand trajectory header.");
                }
                if (this.biom_incr_stem < 0)
                {
                    throw new XmlException("Stem biomass increment column not found in stand trajectory header.");
                }
                if (this.biom_loss_foliage < 0)
                {
                    throw new XmlException("Foliage biomass loss column not found in stand trajectory header.");
                }
                if (this.biom_loss_root < 0)
                {
                    throw new XmlException("Root biomass loss column not found in stand trajectory header.");
                }
                if (this.volume_cum < 0)
                {
                    throw new XmlException("Cumulative volume column not found in stand trajectory header.");
                }

                this.ColumnGroups |= ThreePGStandTrajectoryColumnGroups.Extended;
            }

            bool hasR3PGextendedColumns = (this.apar >= 0) ||
                                          (this.co2 >= 0) ||
                                          (this.day_length >= 0) ||
                                          (this.delta13c >= 0) ||
                                          (this.pFS >= 0) ||
                                          (this.prcp_interc_frac >= 0) ||
                                          (this.solar_rad >= 0) ||
                                          (this.tmp_ave >= 0) ||
                                          (this.tmp_max >= 0) ||
                                          (this.tmp_min >= 0) ||
                                          (this.volume_change >= 0) ||
                                          (this.volume_extracted >= 0) ||
                                          (this.volume_mai >= 0) ||
                                          (this.vpd_day >= 0) ||
                                          (this.water_runoff_pooled >= 0);
            if (hasR3PGextendedColumns)
            {
                if (this.apar < 0)
                {
                    throw new XmlException("Absorbed photosynthetically active radiation column not found in stand trajectory header.");
                }
                if (this.co2 < 0)
                {
                    throw new XmlException("CO₂ column not found in stand trajectory header.");
                }
                if (this.day_length < 0)
                {
                    throw new XmlException("Day length column not found in stand trajectory header.");
                }
                if (this.delta13c < 0)
                {
                    throw new XmlException("δ13C column not found in stand trajectory header.");
                }
                if (this.pFS < 0)
                {
                    throw new XmlException("pFS column not found in stand trajectory header.");
                }
                if (this.prcp_interc_frac < 0)
                {
                    throw new XmlException("Precipitation interception frac column not found in stand trajectory header.");
                }
                if (this.solar_rad < 0)
                {
                    throw new XmlException("Solar radiation column not found in stand trajectory header.");
                }
                if (this.tmp_ave < 0)
                {
                    throw new XmlException("Average temperature column not found in stand trajectory header.");
                }
                if (this.tmp_max < 0)
                {
                    throw new XmlException("Maximum temperature column not found in stand trajectory header.");
                }
                if (this.tmp_min < 0)
                {
                    throw new XmlException("Minimum temperature column not found in stand trajectory header.");
                }
                if (this.volume_change < 0)
                {
                    throw new XmlException("Volume change column not found in stand trajectory header.");
                }
                if (this.volume_extracted < 0)
                {
                    throw new XmlException("Extracted volume column not found in stand trajectory header.");
                }
                if (this.volume_mai < 0)
                {
                    throw new XmlException("MAI (mean annual increment) column not found in stand trajectory header.");
                }
                if (this.vpd_day < 0)
                {
                    throw new XmlException("Daily vapor pressure deficit column not found in stand trajectory header.");
                }
                if (this.water_runoff_pooled < 0)
                {
                    throw new XmlException("Pooled runoff column not found in stand trajectory header.");
                }
            }
        }

        // parse Visual Basic reference output in https://github.com/trotsiuk/r3PG/tree/master/pkg/tests/r_vba_compare/r3PG_input.xls
        //private void Parse(string[] header)
        //{
        //    // get column indices
        //    for (int index = 0; index < header.Length; ++index)
        //    {
        //        string column = header[index];
        //        switch (column)
        //        {
        //            case "Year & month":
        //                this.YearAndMonth = index;
        //                break;
        //            case "Stand age":
        //                this.StandAge = index;
        //                break;
        //            case "Age species1":
        //                this.age = index;
        //                break;
        //            case "Age species2":
        //                this.AgeSpecies2 = index;
        //                break;
        //            case "Stems1":
        //                this.stems_n = index;
        //                break;
        //            case "Stems2":
        //                this.Stems2 = index;
        //                break;
        //            case "Foliage DM1":
        //                this.biom_foliage = index;
        //                break;
        //            case "Foliage DM2":
        //                this.FoliageDM2 = index;
        //                break;
        //            case "Root DM1":
        //                this.biom_root = index;
        //                break;
        //            case "Root DM2":
        //                this.RootDM2 = index;
        //                break;
        //            case "Stem DM1":
        //                this.biom_stem = index;
        //                break;
        //            case "Stem DM2":
        //                this.StemDM2 = index;
        //                break;
        //            case "Stand volume1":
        //                this.volume = index;
        //                break;
        //            case "Stand volume2":
        //                this.StandVolume2 = index;
        //                break;
        //            case "LAI1":
        //                this.lai = index;
        //                break;
        //            case "LAI2":
        //                this.LAI2 = index;
        //                break;
        //            case "MAI1":
        //                this.MAI1 = index;
        //                break;
        //            case "MAI2":
        //                this.MAI2 = index;
        //                break;
        //            case "Mean DBH1":
        //                this.dbh = index;
        //                break;
        //            case "Mean DBH2":
        //                this.MeanDbh2 = index;
        //                break;
        //            case "FR1":
        //                this.FR1 = index;
        //                break;
        //            case "FR2":
        //                this.FR2 = index;
        //                break;
        //            case "Daylength":
        //                this.DayLength = index;
        //                break;
        //            case "VPD":
        //                this.VPD = index;
        //                break;
        //            case "Applied irrig.":
        //                this.Irrigation = index;
        //                break;
        //            case "Basal area1":
        //                this.basal_area = index;
        //                break;
        //            case "Basal area2":
        //                this.BasalArea2 = index;
        //                break;
        //            case "Extracted volume1":
        //                this.ExtractedVolume1 = index;
        //                break;
        //            case "Extracted volume2":
        //                this.ExtractedVolume2 = index;
        //                break;
        //            case "Total cumulative volume1":
        //                this.volume_cum = index;
        //                break;
        //            case "Total cumulative volume2":
        //                this.TotalCumulativeVolume2 = index;
        //                break;
        //            case "Latest extracted volume1":
        //                this.LatestExtractedVolume1 = index;
        //                break;
        //            case "Latest extracted volume2":
        //                this.LatestExtractedVolume2 = index;
        //                break;
        //            case "Peak MAI1":
        //                this.PeakMai1 = index;
        //                break;
        //            case "Peak MAI2":
        //                this.PeakMai2 = index;
        //                break;
        //            case "Age at peak MAI1":
        //                this.AgeAtPeakMai1 = index;
        //                break;
        //            case "Age at peak MAI2":
        //                this.AgeAtPeakMai2 = index;
        //                break;
        //            case "stem growth rate1":
        //                this.StemGrowthRate1 = index;
        //                break;
        //            case "stem growth rate2":
        //                this.StemGrowthRate2 = index;
        //                break;
        //            case "Height1":
        //                this.height = index;
        //                break;
        //            case "Height2":
        //                this.Height2 = index;
        //                break;
        //            case "LCL1":
        //                this.LCL1 = index;
        //                break;
        //            case "LCL2":
        //                this.LCL2 = index;
        //                break;
        //            case "crowndiameter1":
        //                this.CrownDiameter1 = index;
        //                break;
        //            case "crowndiameter2":
        //                this.CrownDiameter2 = index;
        //                break;
        //            case "treeLAtoSAratio1":
        //                this.treeLAtoSAratio1 = index;
        //                break;
        //            case "treeLAtoSAratio2":
        //                this.treeLAtoSAratio2 = index;
        //                break;
        //            case "CropTrees per ha1":
        //                this.CropTreesPerHa1 = index;
        //                break;
        //            case "CropTrees per ha2":
        //                this.CropTreesPerHa2 = index;
        //                break;
        //            case "CropTree Mean DBH1":
        //                this.CropTreeMeanDbh1 = index;
        //                break;
        //            case "CropTree Mean DBH2":
        //                this.CropTreeMeanDbh2 = index;
        //                break;
        //            case "CropTree Basal area1":
        //                this.CropTreeBasalArea1 = index;
        //                break;
        //            case "CropTree Basal area2":
        //                this.CropTreeBasalArea2 = index;
        //                break;
        //            case "CropTree volume1":
        //                this.CropTreeVolume1 = index;
        //                break;
        //            case "CropTree volume2":
        //                this.CropTreeVolume2 = index;
        //                break;
        //            case "CropTree Height1":
        //                this.CropTreeHeight1 = index;
        //                break;
        //            case "CropTree Height2":
        //                this.CropTreeHeight2 = index;
        //                break;
        //            case "CropTree Stem DM1":
        //                this.CropTreeStemDM1 = index;
        //                break;
        //            case "CropTree Stem DM2":
        //                this.CropTreeStemDM2 = index;
        //                break;
        //            case "SLA1":
        //                this.SLA = index;
        //                break;
        //            case "SLA2":
        //                this.SLA2 = index;
        //                break;
        //            case "Canopy cover1":
        //                this.CanopyCover1 = index;
        //                break;
        //            case "Canopy cover2":
        //                this.CanopyCover2 = index;
        //                break;
        //            case "Max LAI1":
        //                this.MaxLai1 = index;
        //                break;
        //            case "Max LAI2":
        //                this.MaxLai2 = index;
        //                break;
        //            case "Age at max LAI1":
        //                this.AgeAtMaxLai1 = index;
        //                break;
        //            case "Age at max LAI2":
        //                this.AgeAtMaxLai2 = index;
        //                break;
        //            case "CanopyVolumefractionSpecies1":
        //                this.CanopyVolumeFractionSpecies1 = index;
        //                break;
        //            case "CanopyVolumefractionSpecies2":
        //                this.CanopyVolumeFractionSpecies2 = index;
        //                break;
        //            case "LayerForSpecies1":
        //                this.LayerForSpecies1 = index;
        //                break;
        //            case "LayerForSpecies2":
        //                this.LayerForSpecies2 = index;
        //                break;
        //            case "LAIabove1":
        //                this.LaiAbove1 = index;
        //                break;
        //            case "LAIabove2":
        //                this.LaiAbove2 = index;
        //                break;
        //            case "lambdaV1":
        //                this.lambdaV1 = index;
        //                break;
        //            case "lambdaV2":
        //                this.lambdaV2 = index;
        //                break;
        //            case "lambdaH1":
        //                this.lambdaH1 = index;
        //                break;
        //            case "lambdaH2":
        //                this.lambdaH2 = index;
        //                break;
        //            case "ra1":
        //                this.ra1 = index;
        //                break;
        //            case "ra2":
        //                this.ra2 = index;
        //                break;
        //            case "VPDspecies1":
        //                this.vpd_sp = index;
        //                break;
        //            case "VPDspecies2":
        //                this.VPDspecies2 = index;
        //                break;
        //            case "Total DM1":
        //                this.TotalDM1 = index;
        //                break;
        //            case "Total DM2":
        //                this.TotalDM2 = index;
        //                break;
        //            case "Mean stem mass1":
        //                this.MeanStemMass1 = index;
        //                break;
        //            case "Mean stem mass2":
        //                this.MeanStemMass2 = index;
        //                break;
        //            case "Basic density1":
        //                this.wood_density = index;
        //                break;
        //            case "Basic density2":
        //                this.BasicDensity2 = index;
        //                break;
        //            case "Bark & branch fraction1":
        //                this.fracBB = index;
        //                break;
        //            case "Bark & branch fraction2":
        //                this.BarkAndBranchFraction2 = index;
        //                break;
        //            case "fAge1":
        //                this.f_age = index;
        //                break;
        //            case "fAge2":
        //                this.fAge2 = index;
        //                break;
        //            case "fVPD1":
        //                this.f_vpd = index;
        //                break;
        //            case "fVPD2":
        //                this.fVPD2 = index;
        //                break;
        //            case "fT1":
        //                this.f_tmp = index;
        //                break;
        //            case "fT2":
        //                this.fT2 = index;
        //                break;
        //            case "fTgc1":
        //                this.f_tmp_gc = index;
        //                break;
        //            case "fTgc2":
        //                this.fTgc2 = index;
        //                break;
        //            case "fFrost1":
        //                this.f_frost = index;
        //                break;
        //            case "fFrost2":
        //                this.fFrost2 = index;
        //                break;
        //            case "fSW1":
        //                this.f_sw = index;
        //                break;
        //            case "fSW2":
        //                this.fSW2 = index;
        //                break;
        //            case "fNutr1":
        //                this.f_nutr = index;
        //                break;
        //            case "fNutr2":
        //                this.fNutr2 = index;
        //                break;
        //            case "fCalpha1":
        //                this.f_calpha = index;
        //                break;
        //            case "fCalpha2":
        //                this.fCalpha2 = index;
        //                break;
        //            case "fCg1":
        //                this.f_cg = index;
        //                break;
        //            case "fCg2":
        //                this.fCg2 = index;
        //                break;
        //            case "PhysMod1":
        //                this.f_phys = index;
        //                break;
        //            case "PhysMod2":
        //                this.PhysMod2 = index;
        //                break;
        //            case "GPP1":
        //                this.gpp = index;
        //                break;
        //            case "GPP2":
        //                this.GPP2 = index;
        //                break;
        //            case "NPP1":
        //                this.npp = index;
        //                break;
        //            case "NPP2":
        //                this.NPP2 = index;
        //                break;
        //            case "rGPP1":
        //                this.rGPP1 = index;
        //                break;
        //            case "rGPP2":
        //                this.rGPP2 = index;
        //                break;
        //            case "rNPP1":
        //                this.rNPP1 = index;
        //                break;
        //            case "rNPP2":
        //                this.rNPP2 = index;
        //                break;
        //            case "RADint1":
        //                this.RADint1 = index;
        //                break;
        //            case "RADint2":
        //                this.RADint2 = index;
        //                break;
        //            case "rRADint1":
        //                this.rRADint1 = index;
        //                break;
        //            case "rRADint2":
        //                this.rRADint2 = index;
        //                break;
        //            case "fi1":
        //                this.fi1 = index;
        //                break;
        //            case "fi2":
        //                this.fi2 = index;
        //                break;
        //            case "alphaC1":
        //                this.apha_c = index;
        //                break;
        //            case "alphaC2":
        //                this.alphaC2 = index;
        //                break;
        //            case "Epsilon1":
        //                this.epsilon_gpp = index;
        //                break;
        //            case "Epsilon2":
        //                this.Epsilon2 = index;
        //                break;
        //            case "Stem DM epsilon1":
        //                this.StemDMepsilon1 = index;
        //                break;
        //            case "Stem DM epsilon2":
        //                this.StemDMepsilon2 = index;
        //                break;
        //            case "NPPEpsilon1":
        //                this.epsilon_npp = index;
        //                break;
        //            case "NPPEpsilon2":
        //                this.NPPEpsilon2 = index;
        //                break;
        //            case "CVI1":
        //                this.CVI1 = index;
        //                break;
        //            case "CVI2":
        //                this.CVI2 = index;
        //                break;
        //            case "m1":
        //                this.m1 = index;
        //                break;
        //            case "m2":
        //                this.m2 = index;
        //                break;
        //            case "pR1":
        //                this.pR1 = index;
        //                break;
        //            case "pR2":
        //                this.pR2 = index;
        //                break;
        //            case "pS1":
        //                this.pS1 = index;
        //                break;
        //            case "pS2":
        //                this.pS2 = index;
        //                break;
        //            case "pF1":
        //                this.pF1 = index;
        //                break;
        //            case "pF2":
        //                this.pF2 = index;
        //                break;
        //            case "pFS1":
        //                this.pFS1 = index;
        //                break;
        //            case "pFS2":
        //                this.pFS2 = index;
        //                break;
        //            case "Litter fall rate1":
        //                this.LitterfallRate1 = index;
        //                break;
        //            case "Litter fall rate2":
        //                this.LitterfallRate2 = index;
        //                break;
        //            case "Foliage litter fall of period1":
        //                this.FoliageLitterfallOfPeriod1 = index;
        //                break;
        //            case "Foliage litter fall of period2":
        //                this.FoliageLitterfallOfPeriod2 = index;
        //                break;
        //            case "Root litter fall of period1":
        //                this.RootLitterfallOfPeriod1 = index;
        //                break;
        //            case "Root litter fall of period2":
        //                this.RootLitterfallOfPeriod2 = index;
        //                break;
        //            case "rLitterF1":
        //                this.rLitterF1 = index;
        //                break;
        //            case "rLitterF2":
        //                this.rLitterF2 = index;
        //                break;
        //            case "rLitterR1":
        //                this.rLitterR1 = index;
        //                break;
        //            case "rLitterR2":
        //                this.rLitterR2 = index;
        //                break;
        //            case "NPPdebt1":
        //                this.NPPdebt1 = index;
        //                break;
        //            case "NPPdebt2":
        //                this.NPPdebt2 = index;
        //                break;
        //            case "incrWF1":
        //                this.incrWF1 = index;
        //                break;
        //            case "incrWF2":
        //                this.incrWF2 = index;
        //                break;
        //            case "incrWS1":
        //                this.incrWS1 = index;
        //                break;
        //            case "incrWS2":
        //                this.incrWS2 = index;
        //                break;
        //            case "incrWR1":
        //                this.incrWR1 = index;
        //                break;
        //            case "incrWR2":
        //                this.incrWR2 = index;
        //                break;
        //            case "wSmax1":
        //                this.wSmax1 = index;
        //                break;
        //            case "wSmax2":
        //                this.wSmax2 = index;
        //                break;
        //            case "gammaN1":
        //                this.gammaN = index;
        //                break;
        //            case "gammaN2":
        //                this.gammaN2 = index;
        //                break;
        //            case "Self thinning1":
        //                this.SelfThinning1 = index;
        //                break;
        //            case "Self thinning2":
        //                this.SelfThinning2 = index;
        //                break;
        //            case "Stem mortality1":
        //                this.StemMortality1 = index;
        //                break;
        //            case "Stem mortality2":
        //                this.StemMortality2 = index;
        //                break;
        //            case "Supp. irrig.":
        //                this.SuppliedIrrigation = index;
        //                break;
        //            case "rSupIrrig":
        //                this.rSupIrrig = index;
        //                break;
        //            case "Run off":
        //                this.Runoff = index;
        //                break;
        //            case "rRunOff":
        //                this.rRunOff = index;
        //                break;
        //            case "fraction rain intcptn1":
        //                this.FractionRainIntcptn1 = index;
        //                break;
        //            case "fraction rain intcptn2":
        //                this.FractionRainIntcptn2 = index;
        //                break;
        //            case "Rain intcptn1":
        //                this.RainIntcptn1 = index;
        //                break;
        //            case "Rain intcptn2":
        //                this.RainIntcptn2 = index;
        //                break;
        //            case "rRainInt1":
        //                this.rRainInt1 = index;
        //                break;
        //            case "rRainInt2":
        //                this.rRainInt2 = index;
        //                break;
        //            case "Canopy conductance1":
        //                this.cond_canopy = index;
        //                break;
        //            case "Canopy conductance2":
        //                this.CanopyConductance2 = index;
        //                break;
        //            case "Soil conductance":
        //                this.SoilConductance = index;
        //                break;
        //            case "Soil evaporation":
        //                this.SoilEvaporation = index;
        //                break;
        //            case "rSoilEvap":
        //                this.rSoilEvap = index;
        //                break;
        //            case "WUE1":
        //                this.wue = index;
        //                break;
        //            case "WUE2":
        //                this.WUE2 = index;
        //                break;
        //            case "WUEtransp1":
        //                this.wue_transp = index;
        //                break;
        //            case "WUEtransp2":
        //                this.WUEtransp2 = index;
        //                break;
        //            case "ET":
        //                this.ET = index;
        //                break;
        //            case "Penman transp.1":
        //                this.PenmanTransp1 = index;
        //                break;
        //            case "Penman transp.2":
        //                this.PenmanTransp2 = index;
        //                break;
        //            case "rEvapTransp":
        //                this.rEvapTransp = index;
        //                break;
        //            case "rTransp1":
        //                this.rTransp1 = index;
        //                break;
        //            case "rTransp2":
        //                this.rTransp2 = index;
        //                break;
        //            case "Transp Scale Factor":
        //                this.TranspScaleFactor = index;
        //                break;
        //            case "ASW":
        //                this.ASW = index;
        //                break;
        //            case "pooled SW":
        //                this.PooledSW = index;
        //                break;
        //            case "Max ASW":
        //            case "Min ASW":
        //            case "Frost days":
        //            case "Solar rad.":
        //            case "Max temp.":
        //            case "Min temp.":
        //            case "Mean temp.":
        //            case "Rainfall":
        //            case "CO2":
        //            case "CO2Monthly":
        //            case "atm delta 13C":
        //                // ignored fields repeated from site and climate inputs
        //                break;
        //            default:
        //                throw new NotSupportedException("Unhandled column name '" + column + "'.");
        //        }
        //    }
        //}
    }
}
