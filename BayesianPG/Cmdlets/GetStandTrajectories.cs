using BayesianPG.ThreePG;
using BayesianPG.Xlsx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;

namespace BayesianPG.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandTrajectories")]
    public class GetStandTrajectories : Cmdlet
    {
        [Parameter]
        public Simd Simd { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? Xlsx { get; set; }

        public GetStandTrajectories()
        {
            this.Simd = Simd.Vex128;
        }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Xlsx != null);

            using ThreePGReader reader = new(this.Xlsx);
            SortedList<string, ThreePGScalar> sitesByName = reader.ReadSites();

            if (this.Simd == Simd.Scalar)
            {
                for (int index = 0; index < sitesByName.Count; ++index)
                {
                    ThreePGScalar threePGscalar = sitesByName.Values[index];
                    threePGscalar.PredictStandTrajectory();
                }
                this.WriteObject(sitesByName);
            }
            else if (this.Simd == Simd.Vex128)
            {
                SortedList<string, ThreePGSimd128> sitesByName128 = new(sitesByName.Count);
                for (int index = 0; index < sitesByName.Count; ++index)
                {
                    ThreePGScalar threePGscalar = sitesByName.Values[index];
                    
                    ThreePGSimd128 threePG128 = new(threePGscalar)
                    {
                        Bias = threePGscalar.Bias
                    };
                    threePG128.PredictStandTrajectory();
                    
                    sitesByName128.Add(threePG128.Site.Name, threePG128);
                }
                this.WriteObject(sitesByName128);
            }
            else
            {
                throw new NotSupportedException("Unhandled SIMD width " + this.Simd + ".");
            }
        }
    }
}
