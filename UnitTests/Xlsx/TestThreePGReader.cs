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

        public SortedList<string, ThreePGStandTrajectory> ReadExpectations()
        {
            StandTrajectoryWorksheet broadleafPjs = this.ReadWorksheet<StandTrajectoryWorksheet>("broadleaf_pjs");
            StandTrajectoryWorksheet broadleafMix = this.ReadWorksheet<StandTrajectoryWorksheet>("broadleaf_mix");
            StandTrajectoryWorksheet evergreenPjs = this.ReadWorksheet<StandTrajectoryWorksheet>("evergreen_pjs");
            StandTrajectoryWorksheet evergreenMix = this.ReadWorksheet<StandTrajectoryWorksheet>("evergreen_mix");
            StandTrajectoryWorksheet mixturesEurope = this.ReadWorksheet<StandTrajectoryWorksheet>("mixtures_eu");
            StandTrajectoryWorksheet mixturesOther = this.ReadWorksheet<StandTrajectoryWorksheet>("mixtures_other");

            SortedList<string, ThreePGStandTrajectory> expectedTrajectories = new()
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
