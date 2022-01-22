using CN.MACH.AI.Cache;
using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FodyCacheManager
{
    private static ICacheProvider cacheProvider = null;
    private static IndexSettings indexSettings = null;

    public static ICacheProvider GetInterface(CacheSetting cacheSetting = null)
    {
        if (cacheProvider == null || (cacheSetting?.IsChangeToNewServer ?? false))
        {
            if (cacheSetting == null)
            {
                cacheSetting = new CacheSetting()
                {
                    Connection = "192.168.0.203",
                    Port = 6379,
                    PefixKey = "zbytest"
                };
            }

            cacheProvider = new CSRedisCacheProvider(cacheSetting);
        }
        return cacheProvider;
    }


    public static void Setting(IndexSettings settings)
    {
        indexSettings = settings;
    }
    public static IndexSettings GetSetting()
    {
        if (indexSettings == null)
        {
            indexSettings = new IndexSettings(cacheProvider);
            indexSettings.Load();
        }
        return indexSettings;
    }
}

