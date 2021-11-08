using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody
{
    public class MgConstants
    {
        public static string Options { get; } = "options";

        public static string SrcCodeConfigsKey { get; } = "src:configs";
        public static string IsRecord { get; } = "is_record";
        public static string SrcCodeRecordKey { get; } = "src:records";
        public static string SrcCodeThreadidKey { get; } = "src:threadid";
        public static string IndexDocListKey { get; } = "index:doces";
    }
}
