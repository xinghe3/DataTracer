using CN.MACH.AI.Cache;
using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody.Utils;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Index
{
    [Serializable]
    public class IndexOptions
    {
        /// <summary>
        /// 是否启动数据采集
        /// </summary>
        public bool IsRecord { get; set; }
    }
    public class IndexSettings
    {
        private readonly ICacheProvider cacheProvider;
        public IndexOptions indexOptions { get; set; } = new IndexOptions();

        public CacheSetting CacheSetting { get; set; }

        public IndexSettings(ICacheProvider cacheProvider)
        {
            if (cacheProvider == null) return;
            this.cacheProvider = cacheProvider;
            cacheProvider.Add<bool>(MgConstants.SrcCodeConfigsKey, MgConstants.IsRecord, false);
            indexOptions.IsRecord = cacheProvider.Get<bool>(MgConstants.SrcCodeConfigsKey, MgConstants.IsRecord);
        }
        public int Clear()
        {

            return ErrorCode.Success;
        }
        public int Update()
        {
            cacheProvider.Update(MgConstants.SrcCodeConfigsKey, MgConstants.IsRecord, indexOptions.IsRecord);
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null)
            {
                mQProvider.Publish<IndexOptions>(MgConstants.Options, indexOptions);
            }
            return ErrorCode.Success;
        }

        private string jsonPath = "Settings.json";
        public int Load()
        {
            if(CacheSetting == null) CacheSetting = new CacheSetting();
            if (!File.Exists(jsonPath))
                return ErrorCode.FileNotFound;
            string json = File.ReadAllText(jsonPath);
            IndexSettings settings = JsonUtils.Deserialize<IndexSettings>(json);
            if (settings == null || settings.CacheSetting == null) return ErrorCode.NULLPOINTER;
            CacheSetting = settings.CacheSetting;
            return ErrorCode.Success;
        }

        public int Save()
        {
            if (File.Exists(jsonPath))
                File.Delete(jsonPath);
            string json = JsonUtils.Serialize(this);
            File.WriteAllText(jsonPath, json);
            return ErrorCode.Success;
        }
    }
}
