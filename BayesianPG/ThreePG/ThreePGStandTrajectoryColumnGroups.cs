using System;

namespace BayesianPG.ThreePG
{
    [Flags]
    public enum ThreePGStandTrajectoryColumnGroups
    {
        Core = 0x0, // always enabled: ok to be zero because HasFlag(Core) is never tested
        BiasCorrection = 0x1,
        D13C = 0x2,
        Extended = 0x4
    }
}
