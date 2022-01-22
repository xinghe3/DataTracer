using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Recorders
{
    class FileLogSrcCodeRecorder : CodeRecorderBase, ISrcCodeRecorder
    {
        public void Init()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (queue.Count > 0 && queue.TryDequeue(out var record))
                    {
                        string sID = ID++.ToString();
                        Logs.Log(sID, record);
                    }
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
