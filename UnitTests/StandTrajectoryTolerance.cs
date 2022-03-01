using System;

namespace BayesianPG.Test
{
    internal class StandTrajectoryTolerance
    {
        // ratio and difference tolerances on 3-PG output variables
        public float AvailableSoilWater { get; init; }
        public float Evapotranspiration { get; init; }
        public float IrrigationSupplied { get; init; }
        public float PrecipitationRunoff { get; init; }
        public float SoilConductance { get; init; }
        public float SoilEvaporation { get; init; }
        public float TranspirationScale { get; init; }

        public float AeroResist { get; init; }
        public float Age { get; init; }
        public float AgeM { get; init; }
        public float AlphaC { get; init; }
        public float BasalArea { get; init; }
        public float BiomassFoliage { get; init; }
        public float BiomassFoliageDebt { get; init; }
        public float BiomassIncrementFoliage { get; init; }
        public float BiomassIncrementRoot { get; init; }
        public float BiomassIncrementStem { get; init; }
        public float BiomassRoot { get; init; }
        public float BiomassStem { get; init; }
        public float BiomassLossFoliage { get; init; }
        public float BiomassLossRoot { get; init; }
        public float CanopyConductance { get; init; }
        public float CanopyCover { get; init; }
        public float CanopyVolumeFraction { get; init; }
        public float CrownWidth { get; init; }
        public float CVdbhDistribution { get; init; }
        public float CVwsDistribution { get; init; }
        public float D13CNewPS { get; init; }
        public float D13CTissue { get; init; }
        public float Dbh { get; init; }
        public float DrelBiasCrowndiameter { get; init; }
        public float DrelBiasheight { get; init; }
        public float DrelBiasLCL { get; init; }
        public float DrelBiaspFS { get; init; }
        public float DWeibullLocation { get; init; }
        public float DWeibullShape { get; init; }
        public float DWeibullScale { get; init; }
        public float EpsilonStemBiomass { get; init; }
        public float EpsilonGpp { get; init; }
        public float EpsilonNpp { get; init; }
        public float FracBB { get; init; }
        public float FractionApar { get; init; }
        public float GammaF { get; init; }
        public float GammaN { get; init; }
        public float GcMol { get; init; }
        public float GwMol { get; init; }
        public float Gpp { get; init; }
        public float Height { get; init; }
        public float HeightRelative { get; init; }
        public float InterCi { get; init; }
        public float Lai { get; init; }
        public float LaiAbove { get; init; }
        public float LaiToSurfaceAreaRatio { get; init; }
        public float LambdaH { get; init; }
        public float LambdaV { get; init; }
        public int LayerID { get; init; }
        public float ModifierAge { get; init; }
        public float ModiferCAlpha { get; init; }
        public float ModifierCG { get; init; } // canopy conductance
        public float ModifierFrost { get; init; }
        public float ModifierNutrition { get; init; }
        public float ModifierPhysiological { get; init; }
        public float ModifierSoilWater { get; init; }
        public float ModifierTemperature { get; init; }
        public float ModifierTemperatureGC { get; init; }
        public float ModifierVpd { get; init; }
        public float NppF { get; init; }
        public float PrecipitationInterception { get; init; }
        public float Sla { get; init; }
        public float StemsN { get; init; }
        public float TranspirationVegetation { get; init; }
        public float Volume { get; init; }
        public float VolumeCumulative { get; init; }
        public float VpdSp { get; init; }
        public float WoodDensity { get; init; }
        public float WSRelBias { get; init; }
        public float WSWeibullLocation { get; init; }
        public float WSWeibullScale { get; init; }
        public float WSWeibullShape { get; init; }
        public float Wue { get; init; }
        public float WueTransp { get; init; }

        // verification range
        public int MaxTimestep { get; init; }

        public StandTrajectoryTolerance()
            : this(0.00008F) // 0.008% match = 80 ppm: single precision floating point versus double precision in r3PG
        {
        }

        public StandTrajectoryTolerance(float defaultTolerance)
        {
            this.AvailableSoilWater = defaultTolerance;
            this.Evapotranspiration = defaultTolerance;
            this.IrrigationSupplied = defaultTolerance;
            this.PrecipitationRunoff = defaultTolerance;
            this.SoilConductance = defaultTolerance;
            this.SoilEvaporation = defaultTolerance;
            this.TranspirationScale = defaultTolerance;

            this.AeroResist = defaultTolerance;
            this.Age = defaultTolerance;
            this.AgeM = defaultTolerance;
            this.AlphaC = defaultTolerance;
            this.BasalArea = defaultTolerance;
            this.BiomassFoliage = defaultTolerance;
            this.BiomassFoliageDebt = defaultTolerance;
            this.BiomassIncrementFoliage = defaultTolerance;
            this.BiomassIncrementRoot = defaultTolerance;
            this.BiomassIncrementStem = defaultTolerance;
            this.BiomassLossFoliage = defaultTolerance;
            this.BiomassLossRoot = defaultTolerance;
            this.BiomassRoot = defaultTolerance;
            this.BiomassStem = defaultTolerance;
            this.CanopyConductance = defaultTolerance;
            this.CanopyCover = defaultTolerance;
            this.CanopyVolumeFraction = defaultTolerance;
            this.CrownWidth = defaultTolerance;
            this.CVdbhDistribution = defaultTolerance;
            this.CVwsDistribution = defaultTolerance;
            this.D13CNewPS = defaultTolerance;
            this.D13CTissue = defaultTolerance;
            this.Dbh = defaultTolerance;
            this.DrelBiasCrowndiameter = defaultTolerance;
            this.DrelBiasheight = defaultTolerance;
            this.DrelBiasLCL = defaultTolerance;
            this.DrelBiaspFS = defaultTolerance;
            this.DrelBiasLCL = defaultTolerance;
            this.DWeibullLocation = defaultTolerance;
            this.DWeibullShape = defaultTolerance;
            this.DWeibullScale = defaultTolerance;
            this.EpsilonStemBiomass = defaultTolerance;
            this.EpsilonGpp = defaultTolerance;
            this.EpsilonNpp = defaultTolerance;
            this.FracBB = defaultTolerance;
            this.FractionApar = defaultTolerance;
            this.GammaF = defaultTolerance;
            this.GammaN = defaultTolerance;
            this.GcMol = defaultTolerance;
            this.GwMol = defaultTolerance;
            this.Gpp = defaultTolerance;
            this.Height = defaultTolerance;
            this.HeightRelative = defaultTolerance;
            this.InterCi = defaultTolerance;
            this.Lai = defaultTolerance;
            this.LaiAbove = defaultTolerance;
            this.LaiToSurfaceAreaRatio = defaultTolerance;
            this.LambdaH = defaultTolerance;
            this.LambdaV = defaultTolerance;
            this.LayerID = 0;
            this.ModifierAge = defaultTolerance;
            this.ModiferCAlpha = defaultTolerance;
            this.ModifierCG = defaultTolerance;
            this.ModifierFrost = defaultTolerance;
            this.ModifierNutrition = defaultTolerance;
            this.ModifierPhysiological = defaultTolerance;
            this.ModifierSoilWater = defaultTolerance;
            this.ModifierTemperature = defaultTolerance;
            this.ModifierTemperatureGC = defaultTolerance;
            this.ModifierVpd = defaultTolerance;
            this.NppF = defaultTolerance;
            this.PrecipitationInterception = defaultTolerance;
            this.Sla = defaultTolerance;
            this.StemsN = defaultTolerance;
            this.TranspirationVegetation = defaultTolerance;
            this.Volume = defaultTolerance;
            this.VolumeCumulative = defaultTolerance;
            this.VpdSp = defaultTolerance;
            this.WoodDensity = defaultTolerance;
            this.Wue = defaultTolerance;
            this.WueTransp = defaultTolerance;
            this.WSRelBias = defaultTolerance;
            this.WSWeibullLocation = defaultTolerance;
            this.WSWeibullScale = defaultTolerance;
            this.WSWeibullShape = defaultTolerance;
            this.MaxTimestep = Int32.MaxValue;
        }
    }
}
