using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;

namespace uIP.MacroProvider.Commons.CronJob
{
    public enum RenewType : int
    {
        NA,
        ByHour,
        ByDay,
        ByWeek,
        ByMonth
    }

    internal enum SchedulingType : int
    {
        InSeconds,
        InMinute,
        LongPeriod
    }

    internal class RunData
    {
        internal string WhichPlugin { get; set; } = "";
        internal string WhichPluginClassFunc { get; set; } = "";
        internal UDataCarrier[] FuncArgs { get; set; } = new UDataCarrier[ 0 ];
        internal RunData() { }
    }
}
