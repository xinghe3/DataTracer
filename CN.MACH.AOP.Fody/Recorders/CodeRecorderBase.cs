using CN.MACH.AOP.Fody.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Recorders
{
    abstract class CodeRecorderBase
    {
        protected static int ID = 0;

        protected static ConcurrentQueue<SrcCodeRecordModel> queue = new ConcurrentQueue<SrcCodeRecordModel>();
        public virtual void Push(SrcCodeRecordModel record)
        {
            queue.Enqueue(record);
        }
    }
}
