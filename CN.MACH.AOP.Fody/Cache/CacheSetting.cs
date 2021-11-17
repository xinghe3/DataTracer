using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AI.Cache
{
    public class CacheSetting
    {
        public string Connection { get; set; }
        public int Port { get; set; }
        public string PefixKey { get; set; }

        public string GetConnectionString()
        {
            return Connection+":"+ Port.ToString() + ",connectTimeout=1000,connectRetry=5,syncTimeout=10000";
        }
    }
}
