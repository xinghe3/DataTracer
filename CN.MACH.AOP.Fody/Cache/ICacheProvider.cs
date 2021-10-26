﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.ETL.Infrastructure.Cache
{
    public interface ICacheProvider : IDisposable
    {
        /// <summary>
        /// 向缓存中添加一个对象
        /// </summary>
        /// <param name="key">缓存的键值</param>
        /// <param name="valueKey">缓存值的键值</param>
        /// <param name="value">缓存的对象</param>
        void Add<T>(string key, string valueKey, T value);
        void Update(string key, string valueKey, object value);
        T Get<T>(string key, string valueKey);
        void Remove(string key);
        bool Exists(string key);
        bool Exists(string key, string valueKey);
        bool SetKeyExpire(string Key, TimeSpan? timeSpan);
        bool GetIsCache();
    }
}
