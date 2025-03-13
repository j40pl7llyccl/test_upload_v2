using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;

namespace uIP.MacroProvider.Commons.CronJob
{

    public class KeepingScheduleInfo
    {
        public RenewType RenewWay { get; set; } = RenewType.NA;
        public int ExceptCount { get; set; } = -1;
        public int NumberScales { get; set; } = 1;
        public DateTime ScheduleAt { get; set; } = DateTime.Now;
        public string WhichPlugin { get; set; } = "";
        public string WhichPluginClassFunc { get; set; } = "";
        public object[] FuncArgs { get; set; } = new object[0];
        public KeepingScheduleInfo() { }
    }
}
