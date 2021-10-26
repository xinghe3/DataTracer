using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DC.ETL.Infrastructure.Cache.Redis;
using DC.ETL.Infrastructure.Cache;
using CN.MACH.AI.Cache;

namespace DC.ETL.Infrastructure
{
    public class CacheFactory
    {
        public static ICacheProvider CreateInstance(string type, CacheSetting cacheSetting)
        {
            ICacheProvider cacheProvider = null;
            switch( type )
            {
                case "redis":
                    cacheProvider = new RedisCacheProvider(cacheSetting);
                    break;
                case "csredis":
                    cacheProvider = new CSRedisCacheProvider(cacheSetting);
                    break;
            };
            return cacheProvider;


        }
    }
}
