using BayesianPG.ThreePG;
using BayesianPG.Xlsx;
using System.Collections.Generic;

namespace BayesianPG.Test.Xlsx
{
    internal class TestThreePGReader : ThreePGReader
    {
        public TestThreePGReader(string xlsxFilePath) :
            base(xlsxFilePath)
        {
        }

        public SortedList<string, ThreePGStandTrajectory<float, int>> ReadR3PGValidationOutput()
        {
            StandTrajectoryWorksheet broadleafPjs = this.ReadWorksheet<StandTrajectoryWorksheet>("broadleaf_pjs"); // _r3PG
            StandTrajectoryWorksheet broadleafMix = this.ReadWorksheet<StandTrajectoryWorksheet>("broadleaf_mix"); // _r3PG
            StandTrajectoryWorksheet evergreenPjs = this.ReadWorksheet<StandTrajectoryWorksheet>("evergreen_pjs"); // _r3PG
            StandTrajectoryWorksheet evergreenMix = this.ReadWorksheet<StandTrajectoryWorksheet>("evergreen_mix"); // _r3PG
            StandTrajectoryWorksheet mixturesEurope = this.ReadWorksheet<StandTrajectoryWorksheet>("mixtures_eu"); // _r3PG
            StandTrajectoryWorksheet mixturesOther = this.ReadWorksheet<StandTrajectoryWorksheet>("mixtures_other"); // _r3PG

            SortedList<string, ThreePGStandTrajectory<float, int>> expectedTrajectories = new()
            {
                { "evergreen_pjs", evergreenPjs.Trajectory },
                { "evergreen_mix", evergreenMix.Trajectory },
                { "broadleaf_pjs", broadleafPjs.Trajectory },
                { "broadleaf_mix", broadleafMix.Trajectory },
                { "mixtures_eu", mixturesEurope.Trajectory },
                { "mixtures_other", mixturesOther.Trajectory }
            };
            return expectedTrajectories;
        }
    }
}
