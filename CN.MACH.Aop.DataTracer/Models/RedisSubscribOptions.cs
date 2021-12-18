using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.Aop.DataTracer.Models
{
    public class RedisSubscribOptions
    {
        /// <summary>
        /// 换行分割的所有需获取的KEY
        /// </summary>
        public string SubscribKeys { get; set; }
    }
}
