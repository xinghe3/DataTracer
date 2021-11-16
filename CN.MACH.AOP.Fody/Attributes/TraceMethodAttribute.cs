using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody;
using CN.MACH.AOP.Fody.Index;
using CN.MACH.AOP.Fody.Models;
using CN.MACH.AOP.Fody.Utils;
using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
using FodyAopTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
[module: TraceTarget]  //相当于注册类
namespace FodyAopTool
{
    [AttributeUsage(AttributeTargets.All)] //可以分为对属性，方法，域等注解，all就是全部都注解
    public class TraceTargetAttribute : Attribute
    {
        private static ICacheProvider cacheProvider = FodyCacheManager.GetInterface();
        public static bool IsRecord = false;
        private static int ID = 0;
        private object _lockID = new object();

        protected object InitInstance;

        protected MethodBase InitMethod;

        protected Object[] Args;

        static TraceTargetAttribute()
        {
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null)
            {
                mQProvider.Subscribe<IndexOptions>(MgConstants.Options, (opt)=>
                {
                    if (opt == null) return;
                    IsRecord = opt.IsRecord;
                });
            }
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
                    record.Params.Add(new SrcCodeObjectModel()
                    {
                        Type = ToParamTypeString(arg),
                        Value = ToParamJsonString(arg)
                    });
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
            string sID;
            lock(_lockID)
            {
                sID = ID++.ToString();
            }
            //cacheProvider.Add(MgConstants.SrcCodeRecordKey, sID, record);
            //cacheProvider.Add(MgConstants.SrcCodeThreadidKey, sID, Thread.CurrentThread.ManagedThreadId);
            // object obj = cacheProvider.Get("src:records", sID);
            // Logs.WriteLogFile("ThreadId:" + Thread.CurrentThread.ManagedThreadId + "\r\n" + txt, "FodyAopTool");
        }

    }
}
