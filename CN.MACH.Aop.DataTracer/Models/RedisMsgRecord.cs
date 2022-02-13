using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.Aop.DataTracer.Models
{
    public class RedisMsgRecord
    {
        public string Time { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Desc { get; private set; }
        public int Id { get; internal set; }

        public string ChannelName { get; internal set; }
        public RedisMsgRecord(string Time, string Name, string Value, string Desc)
        {
            this.Time = Time;
            this.Name = Name;
            this.Value = Value;
            this.Desc = Desc;
        }
        internal void Clone(RedisMsgRecord record)
        {
            Time = record.Time;
            Name = record.Name;
            Value = record.Value;
            Desc = record.Desc;
        }
    }
}
