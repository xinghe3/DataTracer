using FodyAopTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Filters
{
    class CodeFilterByConfigFile : CodeFilterBase, ICodeFilter
    {
        public void Init()
        {
            TraceTargetAttribute.IsRecord = false;
            Task.Run(() =>
            {
                while (true)
                {
                    
                    Thread.Sleep(10000);
                }
            });
        }
    }
}
