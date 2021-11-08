using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody;
using CN.MACH.AOP.Fody.Index;
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
        private static ICacheProvider cacheProvider = new RedisCacheProvider(
                new CN.MACH.AI.Cache.CacheSetting()
                {
                     Connection = "127.0.0.1", Port = 6379, PefixKey = "zbytest:"
                }
            );
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
            InitInstance = instance;
            Args = args;
            
            if (InitMethod.Name == ".ctor" ||InitMethod.Name == ".cctor")// 构造函数
            {
                return;
            }
            Type typeOfInstance = instance?.GetType()??null;
            string instanceName = typeOfInstance?.FullName?? method.ReflectedType.Name;
            StringBuilder sArgs = new StringBuilder();
            if (args != null && args.Length > 0)
            {
                foreach (object arg in args)
                {
                    if (sArgs.Length > 0) sArgs.Append(", ");
                    sArgs.Append( ToParamJsonString(arg));
                }
            }
            if(InitMethod.Name.Contains("set_"))// 属性
            {
                string propertyName = InitMethod.Name.Replace("set_", "");
                Log(instanceName + "." + propertyName + " = " + sArgs + ";");

            }
            else if (InitMethod.Name.Contains("get_"))// 属性
            {
                // 暂时不关心读取属性
            }
            else
            {
                Log(instanceName + "." + InitMethod.Name + "(" + sArgs + ");");

            }
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

        private void Log(string txt)
        {
            string sID;
            lock(_lockID)
            {
                sID = ID++.ToString();
            }
            cacheProvider.Add(MgConstants.SrcCodeRecordKey, sID, txt);
            cacheProvider.Add(MgConstants.SrcCodeThreadidKey, sID,
                Thread.CurrentThread.ManagedThreadId);
            // object obj = cacheProvider.Get("src:records", sID);
            // Logs.WriteLogFile("ThreadId:" + Thread.CurrentThread.ManagedThreadId + "\r\n" + txt, "FodyAopTool");
        }

    }
}
