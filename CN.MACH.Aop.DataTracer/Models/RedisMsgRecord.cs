﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.Aop.DataTracer.Models
{
    public class RedisMsgRecord
    {
        public string Time { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Desc { get; internal set; }
    }
}
