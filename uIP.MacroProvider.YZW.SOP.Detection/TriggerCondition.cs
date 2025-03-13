using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public class TriggerCondition
    {
        public string MessageType { get; set; } = ""; // warning or error
        public string Description { get; set; } = ""; // user defined
        public string TriggerType { get; set; } = ""; // AvailableTrgCond
        public double TimeIntervalSec { get; set; } = 0.0;
        public int EventCountThreshold { get; set; } = 0;
        public TriggerCondition() { }

        public static TriggerCondition Clone( TriggerCondition src)
        {
            return src == null ? new TriggerCondition() : new TriggerCondition()
            {
                MessageType = string.IsNullOrEmpty( src.MessageType ) ? "" : string.Copy( src.MessageType ),
                Description = string.IsNullOrEmpty( src.Description ) ? "" : string.Copy( src.Description ),
                TriggerType = string.IsNullOrEmpty( src.TriggerType ) ? "" : string.Copy( src.TriggerType ),
                TimeIntervalSec = src.TimeIntervalSec,
                EventCountThreshold = src.EventCountThreshold,
            };
        }
    }
}
