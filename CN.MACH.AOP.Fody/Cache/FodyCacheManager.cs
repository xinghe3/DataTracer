using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FodyCacheManager
{
    private static ICacheProvider cacheProvider = new CSRedisCacheProvider(
            new CN.MACH.AI.Cache.CacheSetting()
            {
                Connection = "127.0.0.1",
                Port = 6379,
                PefixKey = "zbytest:"
            }
        );

    public static ICacheProvider GetInterface()
    {
        return cacheProvider;
    }
}

