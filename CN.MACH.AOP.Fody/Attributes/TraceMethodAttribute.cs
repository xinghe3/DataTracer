using CN.MACH.AOP.Fody;
using CN.MACH.AOP.Fody.Filters;
using CN.MACH.AOP.Fody.Index;
using CN.MACH.AOP.Fody.Models;
using CN.MACH.AOP.Fody.Recorders;
using CN.MACH.AOP.Fody.Utils;
using DC.ETL.Infrastructure.Cache;
using FodyAopTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

[module: TraceTarget]  //相当于注册类
namespace FodyAopTool
{
    /// <summary>
    /// 程序调用
    /// 1.引用 NUGET包 MethodDecorator.Fody
    /// 2.AssemblyInfo.cs 中 加入 [assembly:FodyAopTool.TraceTarget]
    /// 3.需要拦截的类名 方法名 上添加注解 [FodyAopTool.TraceTarget]
    /// </summary>
    [AttributeUsage(AttributeTargets.All)] //可以分为对属性，方法，域等注解，all就是全部都注解
    public class TraceTargetAttribute : Attribute
    {
        public static bool IsRecord = false;

        protected object InitInstance;

        protected MethodBase InitMethod;

        protected Object[] Args;

        private static ISrcCodeRecorder recorder = null;
        private static ICodeFilter codeFilter = null;

        static TraceTargetAttribute()
        {
            recorder = RecorderFactory.CreateInterface();
            codeFilter = CodeFilterFactory.CreateInterface();
            recorder.Init();
            codeFilter.Init();
        }

        public void Init(object instance, MethodBase method, object[] args)
        {
            if (!IsRecord)
            {
                return;
            }

            InitMethod = method;
            
            if (InitMethod.Name == ".ctor" ||InitMethod.Name == ".cctor")// 构造函数
            {
                return;
            }
            else if (InitMethod.Name.StartsWith("get_"))// 获取属性
            {
                // 暂时不关心读取属性
                return;
            }
            InitInstance = instance;
            Args = args;

            Type typeOfInstance = instance?.GetType() ?? null;
            SrcCodeRecordModel record = new SrcCodeRecordModel()
            {
                Params = new List<SrcCodeObjectModel>()
            };
            record.InstanceName = typeOfInstance?.FullName ?? method.ReflectedType.Name;
            record.IsStatic = typeOfInstance?.FullName == null;
            if (args != null && args.Length > 0)
            {
                foreach (object arg in args)
                {
                    var param = new SrcCodeObjectModel()
                    {
                        Type = ToParamTypeString(arg),
                        Value = ToParamJsonString(arg)
                    };
                    record.Params.Add(param);
                }
            }
            if (InitMethod.Name.Contains("set_"))// 设置属性
            {
                record.PropertyName = InitMethod.Name.Replace("set_", "");
                Log(record);
            }

            else
            {
                record.MethodName = InitMethod.Name;
                Log(record);
            }
        }
        private static string ToParamTypeString(object arg)
        {
            return arg?.ToString() ?? "null";
        }
        private static string ToParamJsonString(object arg)
        {
            if (arg == null) return "null";
            Type t = arg.GetType();
            if(!t.IsValueType)
            {
                return JsonUtils.Serialize(arg);
            }
            return arg?.ToString() ?? "null";
        }

        public void OnEntry()
        {
            if (!IsRecord)
            {
                return;
            }
            //Console.WriteLine("Before " + InitMethod.Name);
        }
        public void OnExit()
        {
            if (!IsRecord)
            {
                return;
            }
            //Console.WriteLine("After " + InitMethod.Name);
        }

        public void OnException(Exception exception)
        {
            if (!IsRecord)
            {
                return;
            }
            // Log("Ex " + InitMethod.Name + " " + exception.Message);
        }

        private void Log(SrcCodeRecordModel record)
        {
            record.ThreadID = Thread.CurrentThread.ManagedThreadId;
            recorder.Push(record);
        }

    }

}
