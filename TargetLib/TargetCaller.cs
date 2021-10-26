using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FodyAopTool;

namespace TargetLib
{
    public class TestModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    [TraceTarget]
    public class TargetCaller
    {
        public int DoSomeThing(int a, int b)
        {
            b = DoAnotherModel(new TestModel() { Age = b, Name = "Jim" });
            return DoAnother2(DoAnother(a)) + b;
        }
        private int DoAnother2(int a)
        {
            return a * 5;
        }
        private int DoAnother(int a)
        {
            return a + 5;
        }
        private int DoAnotherModel(TestModel a)
        {
            return a.Age + 100;
        }
        public static int StaticDoSThing()
        {
            return 0;
        }
    }
}
