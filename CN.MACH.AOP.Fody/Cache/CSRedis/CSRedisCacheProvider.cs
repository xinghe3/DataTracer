using CN.MACH.AI.Cache;
using CN.MACH.AI.UnitTest.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DC.ETL.Infrastructure.Cache.Redis
{
    /// <summary>
    /// redis 缓存 接口方法实现
    /// 目前调用 RedisHelper封装类实现功能
    /// 2018-04-18 14:15:27
    /// </summary>
    public class CSRedisCacheProvider : ICacheProvider, IDisposable
    {
        private static bool IsCache = true;

        private CSRedisUtils _ru = null;

        public CSRedisCacheProvider(CacheSetting cacheSetting)
        {
            try
            {
                _ru = new CSRedisUtils(cacheSetting);
            }
            catch (Exception ex)
            {
                if (IsCache == true)
                {
                    Logs.WriteExLog( ex, "缓存连接失败");               
                }
                IsCache = false;
                _ru = null;
            }
        }
        /// <summary>
        /// 显式释放资源 避免程序退出出现线程中止问题
        /// 2018-04-19 10:40:01
        /// </summary>
        ~CSRedisCacheProvider()
        {
            if (_ru != null) _ru.Dispose();
        }
        public void Dispose()
        {
            if (_ru != null) _ru.Dispose();
        }

        public void Add<T>(string key, string valueKey, T value)
        {
            _ru.StringSet<T>(key, value);
            //bool b = _ru.HashSet<object>(key, valueKey, value);
        }

        public void Update(string key, string valueKey, object value)
        {
            //bool b = _ru.HashSet<object>(key, valueKey, value);
        }

        public T Get<T>(string key, string valueKey)
        {
            return _ru.StringGet<T>(key);
            //return _ru.HashGet<object>(key, valueKey);
        }

        public void Remove(string key)
        {
            //bool b = _ru.KeyDelete(key);
        }

        public bool Exists(string key)
        {
            return true;
            //return _ru.KeyExists(key);
        }

        public bool Exists(string key, string valueKey)
        {
            return true;
            //return _ru.HashExists(key, valueKey);
        }

        public bool SetKeyExpire(string Key, TimeSpan? timeSpan)
        {
            return true;
            //return _ru.KeyExpire(Key, timeSpan);
        }

        public bool GetIsCache()
        {
            return IsCache;
        }

        public long Count(string key)
        {
            throw new NotImplementedException();
        }
    }
}
