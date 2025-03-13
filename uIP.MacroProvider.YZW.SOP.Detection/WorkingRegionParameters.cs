using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public class WorkingRegionParameters
    {
        public int WaferMinArea { get; set; } = 300;
        public int Wafer2PenMaxDist { get; set; } = 200;
        public int WaferPenIouPercentageThreshold { get; set; } = 11;

        public List<TriggerCondition> TriggerConditions { get; set; } = new List<TriggerCondition>();

        public WorkingRegionParameters() { }
        public static WorkingRegionParameters Clone( WorkingRegionParameters src )
        {
            var ret = new WorkingRegionParameters();
            ret.WaferMinArea = src.WaferMinArea;
            ret.Wafer2PenMaxDist = src.Wafer2PenMaxDist;
            ret.TriggerConditions = new List<TriggerCondition>();
            foreach ( var c in src.TriggerConditions )
            {
                ret.TriggerConditions.Add( TriggerCondition.Clone( c ) );
            }
            return ret;
        }
    }
}
