using System;

namespace BayesianPG.Test
{
    internal class StandTrajectoryTolerance
    {
        // ratio and difference tolerances on 3-PG output variables
        public float AvailableSoilWater { get; init; }
        public float Irrigation { get; init; }
        public float Runoff { get; init; }
        public float SoilConductance { get; init; }
        public float SoilEvaporation { get; init; }
        public float age { get; init; }
        public float age_m { get; init; }
        public float alpha_c { get; init; }
        public float basal_area { get; init; }
        public float biom_foliage { get; init; }
        public float biom_root { get; init; }
        public float biom_stem { get; init; }
        public float conduct_canopy { get; init; }
        public float dbh { get; init; }
        public float epsilon_gpp { get; init; }
        public float epsilon_npp { get; init; }
        public float fracBB { get; init; }
        public float f_age { get; init; }
        public float f_calpha { get; init; }
        public float f_cg { get; init; }
        public float f_frost { get; init; }
        public float f_nutr { get; init; }
        public float f_phys { get; init; }
        public float f_sw { get; init; }
        public float f_tmp { get; init; }
        public float f_tmp_gc { get; init; }
        public float f_vpd { get; init; }
        public float gammaN { get; init; }
        public float GPP { get; init; }
        public float height { get; init; }
        public float lai { get; init; }
        public float SLA { get; init; }
        public float stems_n { get; init; }
        public float volume { get; init; }
        public float volume_cum { get; init; }
        public float VPD_sp { get; init; }
        public float wood_density { get; init; }
        public float WUE { get; init; }
        public float WUEtransp { get; init; }

        // verification range
        public int MaxTimestep { get; init; }

        public StandTrajectoryTolerance()
            : this(0.00008F) // 0.008% match = 80 ppm: single precision floating point versus double precision in r3PG
        {
        }

        public StandTrajectoryTolerance(float defaultTolerance)
        {
            this.AvailableSoilWater = defaultTolerance;
            // this.float DayLength = defaultTolerance;
            // this.Evapotranspiration = defaultTolerance;
            this.Irrigation = defaultTolerance;
            this.Runoff = defaultTolerance;
            this.SoilConductance = defaultTolerance;
            this.SoilEvaporation = defaultTolerance;
            this.age = defaultTolerance;
            this.age_m = defaultTolerance;
            this.alpha_c = defaultTolerance;
            this.basal_area = defaultTolerance;
            this.biom_foliage = defaultTolerance;
            this.biom_root = defaultTolerance;
            this.biom_stem = defaultTolerance;
            this.conduct_canopy = defaultTolerance;
            this.dbh = defaultTolerance;
            this.epsilon_gpp = defaultTolerance;
            this.epsilon_npp = defaultTolerance;
            this.fracBB = defaultTolerance;
            this.f_age = defaultTolerance;
            this.f_calpha = defaultTolerance;
            this.f_cg = defaultTolerance;
            this.f_frost = defaultTolerance;
            this.f_nutr = defaultTolerance;
            this.f_phys = defaultTolerance;
            this.f_sw = defaultTolerance;
            this.f_tmp = defaultTolerance;
            this.f_tmp_gc = defaultTolerance;
            this.f_vpd = defaultTolerance;
            this.gammaN = defaultTolerance;
            this.GPP = defaultTolerance;
            this.height = defaultTolerance;
            this.lai = defaultTolerance;
            this.SLA = defaultTolerance;
            this.stems_n = defaultTolerance;
            this.volume = defaultTolerance;
            this.volume_cum = defaultTolerance;
            this.VPD_sp = defaultTolerance;
            this.wood_density = defaultTolerance;
            this.WUE = defaultTolerance;
            this.WUEtransp = defaultTolerance;

            this.MaxTimestep = Int32.MaxValue;
        }
    }
}
