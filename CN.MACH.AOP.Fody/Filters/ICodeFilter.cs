using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Filters
{
    interface ICodeFilter
    {
        void Init();
    }

    class CodeFilterFactory
    {
        public static ICodeFilter CreateInterface()
        {
            return new CodeFilterByConfigFile();
        }
    }
}
