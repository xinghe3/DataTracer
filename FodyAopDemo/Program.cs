using FodyAopTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TargetLib;

namespace FodyAopDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TraceTargetAttribute.IsRecord = true;
            Console.WriteLine("TargetCaller in target lib will excute aop.");
            TargetCaller targetCaller = new TargetCaller();
            int n = targetCaller.DoSomeThing(1,3);
            Console.WriteLine("TargetCaller static in target lib Assembily will excute aop.");
            TargetCaller.StaticDoSThing();
            Console.ReadLine();
        }
    }

}
