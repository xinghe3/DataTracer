using CN.MACH.AI.UnitTest.Core.Utils;
using PanGu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Utils
{
    public class WordSplitUtils
    {
        public static List<string> Split(string str)
        {
            Segment.Init();//初始化
            Segment segment = new Segment();
            ICollection<WordInfo> words = segment.DoSegment(str);
            List<string> list = new List<string>();
            if (CollectionUtils.IsNullOrEmpty<WordInfo>(words))
                return list;
            foreach (var item in words)
            {
                list.Add(item.Word);
            }
            return list;
        }

    }
}
