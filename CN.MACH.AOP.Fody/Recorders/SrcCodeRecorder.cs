using CN.MACH.AOP.Fody.Index;
using CN.MACH.AOP.Fody.Models;
using DC.ETL.Infrastructure.Cache;
using FodyAopTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Recorders
{

    /// <summary>
    /// 分离连接缓存保存数据线程
    /// </summary>
    class SrcCodeRecorder : CodeRecorderBase, ISrcCodeRecorder
    {
        private static readonly ICacheProvider cacheProvider = FodyCacheManager.GetInterface();

        public void Init()
        {
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null)
            {
                mQProvider.Subscribe<IndexOptions>(MgConstants.Options, (opt) =>
                {
                    if (opt == null) return;
                    TraceTargetAttribute.IsRecord = opt.IsRecord;
                });
            }
            Task.Run(() =>
            {
                while (true)
                {
                    if (queue.Count > 0 && queue.TryDequeue(out var record))
                    {
                        string sID = ID++.ToString();
                        cacheProvider.Add(MgConstants.SrcCodeRecordKey, sID, record);
                    }
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
