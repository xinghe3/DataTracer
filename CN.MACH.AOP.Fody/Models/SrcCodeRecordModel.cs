using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Models
{
    [Serializable]
    public class SrcCodeRecordModel
    {
        public string InstanceName { get; internal set; }
        public bool IsStatic { get; internal set; }
        public string MethodName { get; internal set; }

        public List<SrcCodeObjectModel> Params { get; internal set; }
        public string PropertyName { get; internal set; }
    }
    [Serializable]
    public class SrcCodeObjectModel
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

}
