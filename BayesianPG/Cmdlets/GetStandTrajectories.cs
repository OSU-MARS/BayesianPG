using BayesianPG.ThreePG;
using BayesianPG.Xlsx;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;

namespace BayesianPG.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectories")]
    public class GetStandTrajectories : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? Xlsx { get; set; }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Xlsx != null);

            using ThreePGReader reader = new(this.Xlsx);
            SortedList<string, ThreePGScalar> sitesByName = reader.ReadSites();

            for (int index = 0; index < sitesByName.Count; ++index)
            {
                ThreePGScalar threePG = sitesByName.Values[index];
                threePG.PredictStandTrajectory();
            }

            this.WriteObject(sitesByName);
        }
    }
}
