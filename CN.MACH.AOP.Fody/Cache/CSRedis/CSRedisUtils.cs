using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRedis;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Data;
using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AI.Cache;
using static CSRedis.CSRedisClient;
using System.Collections.Concurrent;
using System.Threading;

namespace DC.ETL.Infrastructure.Cache.Redis
{

    /// <summary>
    /// 内部使用 Redis 封装 为进行单元测试类功能定义为 public.
    /// 功能调用全部通过<see cref="CSRedisCacheProvider"/>进行
    /// 应用配置ConfigurationManager ConnectionString="RedisConnectionString"连接缓存数据库
    /// 应用配置ConfigurationManager AppSetting="Redis.DefaultKey"设置默认key前缀
    /// 2018-04-18 14:26:58
    /// </summary>
    public class CSRedisUtils : IDisposable
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        private readonly CacheSetting cacheSetting;

        /// <summary>
        /// redis 连接对象
        /// </summary>
        // private static RedisClient _redisClient;
        private CSRedisClient _redisClient;
        /// <summary>
        /// 订阅对象
        /// </summary>
        private SubscribeObject subscribeObject;

        /// <summary>
        /// 默认的 Key 值（用来当作 RedisKey 的前缀）
        /// </summary>
        private string DefaultKey;

        private static int threadStatus = 0;
        /// <summary>
        /// 释放标志
        /// </summary>
        private bool _IsDisposable = true;


        private List<(string, Action<SubscribeMessageEventArgs>)> subScribes;

        private ConcurrentQueue<Action> actionQueus = new ConcurrentQueue<Action>();

        /// <summary>
        /// 获取 Redis 连接对象
        /// </summary>
        /// <returns></returns>
        public CSRedisClient GetConnectionRedis(CacheSetting cacheSetting)
        {
            if (_redisClient == null || cacheSetting.IsChangeToNewServer)
            {
                if (_redisClient != null) _redisClient.Dispose();

                _redisClient = new CSRedisClient(cacheSetting.GetConnectionString());
            }
            return _redisClient;
        }

        #region 构造函数资源释放


        /// <summary>
        /// 构造函数
        /// </summary>
        public CSRedisUtils(CacheSetting cacheSetting)
        {

            DefaultKey = cacheSetting.PefixKey;
            this.cacheSetting = cacheSetting;
            _redisClient = GetConnectionRedis(cacheSetting);
            AddRegisterEvent();
            RedisHelper.Initialization(_redisClient);
            subScribes = new List<(string, Action<SubscribeMessageEventArgs>)>();

            
        }

        internal int Init()
        {
            if (threadStatus == 2) threadStatus = 3;
            if (threadStatus == 0) threadStatus = 1;
            while (threadStatus != 1)
            {
                Thread.Sleep(500);

            }
            subScribes.Clear();
            subscribeObject?.Unsubscribe();
            subscribeObject = null;
            return ErrorCode.Success;
        }

        internal int Start()
        {

            //threadStatus = 2;
            //Task.Run(() =>
            //{
            //    while (threadStatus == 2)
            //    {
            //        if (actionQueus.Count > 0 && actionQueus.TryDequeue(out var action))
            //        {
            //            action.Invoke();
            //        }
            //        Thread.Sleep(1000);
            //    }
            //    threadStatus = 1;
            //});
            try
            {
                subscribeObject = _redisClient.Subscribe(subScribes.ToArray());
            }
            catch (Exception ex)
            {
                // ReSubscribe();
                Logs.WriteExLog(ex, "通信服务启动失败，错误信息：");
                return ErrorCode.ConnFail;
            }
            return ErrorCode.Success;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~CSRedisUtils()
        {
            Dispose();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_IsDisposable)
            {
                Dispose(true);
            }
        }

        private void Dispose(bool value)
        {
            if (_IsDisposable)
            {
                _IsDisposable = false;
                if (_redisClient != null)
                {
                    subscribeObject?.Unsubscribe();
                    _redisClient.Dispose();
                }
                if (value)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }
        #endregion 构造函数资源释放

        #region String 操作
        /// <summary>
        /// 设置 key 并保存字符串（如果 key 已存在，则覆盖值）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet(string redisKey, object redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            bool result = false;
            if (expiry == null)
            {
                result = _redisClient.Set(redisKey, redisValue);
            }
            else
            {
                result = _redisClient.Set(redisKey, redisValue, (TimeSpan)expiry);
            }
            return result;
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public string StringGet(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return _redisClient.Get(redisKey);
        }

        /// <summary>
        /// 存储一个对象（该对象会被序列化保存）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T>(string redisKey, T redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            var json = Serialize(redisValue);
            bool result = false;
            if (expiry == null)
            {
                result = _redisClient.Set(redisKey, json);
            }
            else
            {
                result = _redisClient.Set(redisKey, json, (TimeSpan)expiry);
            }
            return result;
        }

        /// <summary>
        /// 获取一个对象（会进行反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public T StringGet<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Deserialize<T>(_redisClient.Get(redisKey));
        }

        //#region async

        /// <summary>
        /// 保存一个字符串值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync(string redisKey, string redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            if (expiry == null)
            {
                await _redisClient.SetAsync(redisKey, redisValue);
            }
            else
            {
                await _redisClient.SetAsync(redisKey, redisValue, (TimeSpan)expiry);
            }
            return true;
        }

        ///// <summary>
        ///// 保存一组字符串值
        ///// </summary>
        ///// <param name="keyValuePairs"></param>
        ///// <returns></returns>
        //public async Task<bool> StringSetAsync(IEnumerable<KeyValuePair<RedisKey, RedisValue>> keyValuePairs)
        //{
        //    keyValuePairs =
        //        keyValuePairs.Select(x => new KeyValuePair<RedisKey, RedisValue>(AddKeyPrefix(x.Key), x.Value));
        //    return await GetDB().StringSetAsync(keyValuePairs.ToArray());
        //}

        ///// <summary>
        ///// 获取单个值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <param name="expiry"></param>
        ///// <returns></returns>
        //public async Task<string> StringGetAsync(string redisKey, string redisValue, TimeSpan? expiry = null)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().StringGetAsync(redisKey);
        //}

        ///// <summary>
        ///// 存储一个对象（该对象会被序列化保存）
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <param name="expiry"></param>
        ///// <returns></returns>
        //public async Task<bool> StringSetAsync<T>(string redisKey, T redisValue, TimeSpan? expiry = null)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    var json = Serialize(redisValue);
        //    return await GetDB().StringSetAsync(redisKey, json, expiry);
        //}

        ///// <summary>
        ///// 获取一个对象（会进行反序列化）
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="expiry"></param>
        ///// <returns></returns>
        //public async Task<T> StringGetAsync<T>(string redisKey, TimeSpan? expiry = null)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return Deserialize<T>(await GetDB().StringGetAsync(redisKey));
        //}

        //#endregion async

        #endregion String 操作

        #region Hash 操作


        public long HashLength(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            try
            {
                return _redisClient.HLen(redisKey);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex, redisKey);
            }
            return -1;
        }

        ///// <summary>
        ///// 判断该字段是否存在 hash 中
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public bool HashExists(string redisKey, string hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        return GetDB().HashExists(redisKey, hashField);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey, " ", hashField);
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 从 hash 中移除指定字段
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public bool HashDelete(string redisKey, string hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        return GetDB().HashDelete(redisKey, hashField);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey, " ", hashField);
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 从 hash 中移除指定字段
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public long HashDelete(string redisKey, IEnumerable<RedisValue> hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        return GetDB().HashDelete(redisKey, hashField.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey);
        //    }
        //    return ErrorCode.StoreErr;
        //}

        ///// <summary>
        ///// 在 hash 设定值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public bool HashSet(string redisKey, string hashField, string value)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        return GetDB().HashSet(redisKey, hashField, value);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey, " ", hashField, " ", value);
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 在 hash 中设定值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashFields"></param>
        //public void HashSet(string redisKey, IEnumerable<HashEntry> hashFields)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        GetDB().HashSet(redisKey, hashFields.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey);
        //    }
        //}
        /// <summary>
        /// 在 hash 设定值（序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HashSet(string redisKey, string hashField, string value)
        {
            redisKey = AddKeyPrefix(redisKey);
            try
            {
                var json = Serialize(value);
                return _redisClient.HSet(redisKey, hashField, json);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex, redisKey, hashField);
            }
            return false;
        }

        /// <summary>
        /// 在 hash 中获取值（反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public object HashGet(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            try
            {
                var result = _redisClient.HGet(redisKey, hashField);
                object o = Deserialize(result);
                return o;
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex, redisKey, " ", hashField);
            }
            return null;
        }

        ///// <summary>
        ///// 在 hash 中获取值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public RedisValue[] HashGet(string redisKey, RedisValue[] hashField, string value)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().HashGet(redisKey, hashField);
        //}

        ///// <summary>
        ///// 从 hash 返回所有的字段值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public IEnumerable<RedisValue> HashKeys(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        return GetDB().HashKeys(redisKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey);
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// 返回 hash 中的所有值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public RedisValue[] HashValues(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    try
        //    {
        //        return GetDB().HashValues(redisKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.WriteExLog( ex, redisKey);
        //    }
        //    return null;
        //}

        /// <summary>
        /// 在 hash 设定值（序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HashSet<T>(string redisKey, string hashField, T value)
        {
            redisKey = AddKeyPrefix(redisKey);
            try
            {
                return _redisClient.HSet(redisKey, hashField, value);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog( ex, redisKey, hashField);
            }
            return false;
        }

        /// <summary>
        /// 在 hash 中获取值（反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public T HashGet<T>(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            try
            {
                var result = _redisClient.HGet<T>(redisKey, hashField);
                return result;
            }
            catch (Exception ex)
            {
                Logs.WriteExLog( ex, redisKey, " ", hashField);
            }
            return default(T);
        }

        //#region async

        ///// <summary>
        ///// 判断该字段是否存在 hash 中
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public async Task<bool> HashExistsAsync(string redisKey, string hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashExistsAsync(redisKey, hashField);
        //}

        ///// <summary>
        ///// 从 hash 中移除指定字段
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public async Task<bool> HashDeleteAsync(string redisKey, string hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashDeleteAsync(redisKey, hashField);
        //}

        ///// <summary>
        ///// 从 hash 中移除指定字段
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public async Task<long> HashDeleteAsync(string redisKey, IEnumerable<RedisValue> hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashDeleteAsync(redisKey, hashField.ToArray());
        //}

        ///// <summary>
        ///// 在 hash 设定值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public async Task<bool> HashSetAsync(string redisKey, string hashField, string value)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashSetAsync(redisKey, hashField, value);
        //}

        ///// <summary>
        ///// 在 hash 中设定值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashFields"></param>
        //public async Task HashSetAsync(string redisKey, IEnumerable<HashEntry> hashFields)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    await GetDB().HashSetAsync(redisKey, hashFields.ToArray());
        //}

        ///// <summary>
        ///// 在 hash 中获取值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public async Task<RedisValue> HashGetAsync(string redisKey, string hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashGetAsync(redisKey, hashField);
        //}

        ///// <summary>
        ///// 在 hash 中获取值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<RedisValue>> HashGetAsync(string redisKey, RedisValue[] hashField, string value)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashGetAsync(redisKey, hashField);
        //}

        ///// <summary>
        ///// 从 hash 返回所有的字段值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<RedisValue>> HashKeysAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashKeysAsync(redisKey);
        //}

        ///// <summary>
        ///// 返回 hash 中的所有值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<RedisValue>> HashValuesAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().HashValuesAsync(redisKey);
        //}

        ///// <summary>
        ///// 在 hash 设定值（序列化）
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public async Task<bool> HashSetAsync<T>(string redisKey, string hashField, T value)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    var json = Serialize(value);
        //    return await GetDB().HashSetAsync(redisKey, hashField, json);
        //}

        ///// <summary>
        ///// 在 hash 中获取值（反序列化）
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="hashField"></param>
        ///// <returns></returns>
        //public async Task<T> HashGetAsync<T>(string redisKey, string hashField)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return Deserialize<T>(await GetDB().HashGetAsync(redisKey, hashField));
        //}

        //#endregion async

        #endregion Hash 操作

        //#region List 操作

        ///// <summary>
        ///// 移除并返回存储在该键列表的第一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public string ListLeftPop(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListLeftPop(redisKey);
        //}

        ///// <summary>
        ///// 移除并返回存储在该键列表的最后一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public string ListRightPop(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListRightPop(redisKey);
        //}

        ///// <summary>
        ///// 移除列表指定键上与该值相同的元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public long ListRemove(string redisKey, string redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListRemove(redisKey, redisValue);
        //}

        ///// <summary>
        ///// 在列表尾部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public long ListRightPush(string redisKey, string redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListRightPush(redisKey, redisValue);
        //}

        ///// <summary>
        ///// 在列表头部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public long ListLeftPush(string redisKey, string redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListLeftPush(redisKey, redisValue);
        //}

        ///// <summary>
        ///// 返回列表上该键的长度，如果不存在，返回 0
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public long ListLength(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListLength(redisKey);
        //}

        ///// <summary>
        ///// 返回在该列表上键所对应的元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public IEnumerable<RedisValue> ListRange(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListRange(redisKey);
        //}

        ///// <summary>
        ///// 移除并返回存储在该键列表的第一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public T ListLeftPop<T>(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return Deserialize<T>(GetDB().ListLeftPop(redisKey));
        //}

        ///// <summary>
        ///// 移除并返回存储在该键列表的最后一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public T ListRightPop<T>(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return Deserialize<T>(GetDB().ListRightPop(redisKey));
        //}

        ///// <summary>
        ///// 在列表尾部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public long ListRightPush<T>(string redisKey, T redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListRightPush(redisKey, Serialize(redisValue));
        //}

        ///// <summary>
        ///// 在列表头部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public long ListLeftPush<T>(string redisKey, T redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().ListLeftPush(redisKey, Serialize(redisValue));
        //}

        //#region List-async

        ///// <summary>
        ///// 移除并返回存储在该键列表的第一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<string> ListLeftPopAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListLeftPopAsync(redisKey);
        //}

        ///// <summary>
        ///// 移除并返回存储在该键列表的最后一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<string> ListRightPopAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListRightPopAsync(redisKey);
        //}

        ///// <summary>
        ///// 移除列表指定键上与该值相同的元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public async Task<long> ListRemoveAsync(string redisKey, string redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListRemoveAsync(redisKey, redisValue);
        //}

        ///// <summary>
        ///// 在列表尾部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public async Task<long> ListRightPushAsync(string redisKey, string redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListRightPushAsync(redisKey, redisValue);
        //}

        ///// <summary>
        ///// 在列表头部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public async Task<long> ListLeftPushAsync(string redisKey, string redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListLeftPushAsync(redisKey, redisValue);
        //}

        ///// <summary>
        ///// 返回列表上该键的长度，如果不存在，返回 0
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<long> ListLengthAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListLengthAsync(redisKey);
        //}

        ///// <summary>
        ///// 返回在该列表上键所对应的元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<RedisValue>> ListRangeAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListRangeAsync(redisKey);
        //}

        ///// <summary>
        ///// 移除并返回存储在该键列表的第一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<T> ListLeftPopAsync<T>(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return Deserialize<T>(await GetDB().ListLeftPopAsync(redisKey));
        //}

        ///// <summary>
        ///// 移除并返回存储在该键列表的最后一个元素
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<T> ListRightPopAsync<T>(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return Deserialize<T>(await GetDB().ListRightPopAsync(redisKey));
        //}

        ///// <summary>
        ///// 在列表尾部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public async Task<long> ListRightPushAsync<T>(string redisKey, T redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListRightPushAsync(redisKey, Serialize(redisValue));
        //}

        ///// <summary>
        ///// 在列表头部插入值。如果键不存在，先创建再插入值
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisValue"></param>
        ///// <returns></returns>
        //public async Task<long> ListLeftPushAsync<T>(string redisKey, T redisValue)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().ListLeftPushAsync(redisKey, Serialize(redisValue));
        //}

        //#endregion List-async

        //#endregion List 操作

        //#region SortedSet 操作

        ///// <summary>
        ///// SortedSet 新增
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="member"></param>
        ///// <param name="score"></param>
        ///// <returns></returns>
        //public bool SortedSetAdd(string redisKey, string member, double score)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().SortedSetAdd(redisKey, member, score);
        //}

        ///// <summary>
        ///// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public IEnumerable<RedisValue> SortedSetRangeByRank(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().SortedSetRangeByRank(redisKey);
        //}

        ///// <summary>
        ///// 返回有序集合的元素个数
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public long SortedSetLength(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().SortedSetLength(redisKey);
        //}

        ///// <summary>
        ///// 返回有序集合的元素个数
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="memebr"></param>
        ///// <returns></returns>
        //public bool SortedSetLength(string redisKey, string memebr)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().SortedSetRemove(redisKey, memebr);
        //}

        ///// <summary>
        ///// SortedSet 新增
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="member"></param>
        ///// <param name="score"></param>
        ///// <returns></returns>
        //public bool SortedSetAdd<T>(string redisKey, T member, double score)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    var json = Serialize(member);

        //    return GetDB().SortedSetAdd(redisKey, json, score);
        //}

        //#region SortedSet-Async

        ///// <summary>
        ///// SortedSet 新增
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="member"></param>
        ///// <param name="score"></param>
        ///// <returns></returns>
        //public async Task<bool> SortedSetAddAsync(string redisKey, string member, double score)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().SortedSetAddAsync(redisKey, member, score);
        //}

        ///// <summary>
        ///// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<RedisValue>> SortedSetRangeByRankAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().SortedSetRangeByRankAsync(redisKey);
        //}

        ///// <summary>
        ///// 返回有序集合的元素个数
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<long> SortedSetLengthAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().SortedSetLengthAsync(redisKey);
        //}

        ///// <summary>
        ///// 返回有序集合的元素个数
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="memebr"></param>
        ///// <returns></returns>
        //public async Task<bool> SortedSetRemoveAsync(string redisKey, string memebr)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().SortedSetRemoveAsync(redisKey, memebr);
        //}

        ///// <summary>
        ///// SortedSet 新增
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="member"></param>
        ///// <param name="score"></param>
        ///// <returns></returns>
        //public async Task<bool> SortedSetAddAsync<T>(string redisKey, T member, double score)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    var json = Serialize(member);

        //    return await GetDB().SortedSetAddAsync(redisKey, json, score);
        //}

        //#endregion SortedSet-Async

        //#endregion SortedSet 操作

        //#region key 操作

        ///// <summary>
        ///// 移除指定 Key
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public bool KeyDelete(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().KeyDelete(redisKey);
        //}

        ///// <summary>
        ///// 移除指定 Key
        ///// </summary>
        ///// <param name="redisKeys"></param>
        ///// <returns></returns>
        //public long KeyDelete(IEnumerable<string> redisKeys)
        //{
        //    var keys = redisKeys.Select(x => (RedisKey)AddKeyPrefix(x));
        //    return GetDB().KeyDelete(keys.ToArray());
        //}

        ///// <summary>
        ///// 校验 Key 是否存在
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public bool KeyExists(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().KeyExists(redisKey);
        //}

        ///// <summary>
        ///// 重命名 Key
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisNewKey"></param>
        ///// <returns></returns>
        //public bool KeyRename(string redisKey, string redisNewKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().KeyRename(redisKey, redisNewKey);
        //}

        ///// <summary>
        ///// 设置 Key 的时间
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="expiry"></param>
        ///// <returns></returns>
        //public bool KeyExpire(string redisKey, TimeSpan? expiry)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return GetDB().KeyExpire(redisKey, expiry);
        //}

        //#region key-async

        ///// <summary>
        ///// 移除指定 Key
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<bool> KeyDeleteAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().KeyDeleteAsync(redisKey);
        //}

        ///// <summary>
        ///// 移除指定 Key
        ///// </summary>
        ///// <param name="redisKeys"></param>
        ///// <returns></returns>
        //public async Task<long> KeyDeleteAsync(IEnumerable<string> redisKeys)
        //{
        //    var keys = redisKeys.Select(x => (RedisKey)AddKeyPrefix(x));
        //    return await GetDB().KeyDeleteAsync(keys.ToArray());
        //}

        ///// <summary>
        ///// 校验 Key 是否存在
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <returns></returns>
        //public async Task<bool> KeyExistsAsync(string redisKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().KeyExistsAsync(redisKey);
        //}

        ///// <summary>
        ///// 重命名 Key
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="redisNewKey"></param>
        ///// <returns></returns>
        //public async Task<bool> KeyRenameAsync(string redisKey, string redisNewKey)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().KeyRenameAsync(redisKey, redisNewKey);
        //}

        ///// <summary>
        ///// 设置 Key 的时间
        ///// </summary>
        ///// <param name="redisKey"></param>
        ///// <param name="expiry"></param>
        ///// <returns></returns>
        //public async Task<bool> KeyExpireAsync(string redisKey, TimeSpan? expiry)
        //{
        //    redisKey = AddKeyPrefix(redisKey);
        //    return await GetDB().KeyExpireAsync(redisKey, expiry);
        //}

        //#endregion key-async

        //#endregion key 操作

        //#region 发布订阅

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(string channel, Action<T> action)
        {
            var actionObj = new Action<SubscribeMessageEventArgs>(msg =>
            {
                var t = Deserialize<T>(msg.Body);
                action(t);
            });
            subScribes.Add((channel, actionObj));
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handle"></param>
        public int Subscribe(string channel, Action<string> action)
        {
            //int nRet = ErrorCode.Success;
            //try
            //{
            //    var sub6 = _redisClient.Subscribe((channel, msg => action(msg.Body)));

            //}
            //catch (Exception ex)
            //{
            //    Logs.WriteExLog(ex);
            //    nRet = ErrorCode.UnKnowException;
            //}
            //return nRet;
            var actionObj = new Action<SubscribeMessageEventArgs>(msg =>
            {
                // var t = Deserialize(msg.Body);
                action(msg.Body);
            });
            subScribes.Add((channel, actionObj));
            return ErrorCode.Success;
        }

        ///// <summary>
        ///// 发布
        ///// </summary>
        ///// <param name="channel"></param>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public long Publish(RedisChannel channel, RedisValue message)
        //{
        //    var sub = _connMultiplexer.GetSubscriber();
        //    return sub.Publish(channel, message);
        //}
        public long Publish(string channel, string message)
        {
            try
            {
                return _redisClient.Publish(channel, message);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex);
            }
            return -1;
        }
        /// <summary>
        /// 发布（使用序列化）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public long Publish<T>(string channel, T message)
        {
            var msgStr = "";
            try
            {
                msgStr = Serialize(message);
                return _redisClient.Publish(channel, msgStr);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex);
            }
            return -1;
        }

        //#region 发布订阅-async

        ///// <summary>
        ///// 订阅
        ///// </summary>
        ///// <param name="channel"></param>
        ///// <param name="handle"></param>
        //public async Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handle)
        //{
        //    var sub = _connMultiplexer.GetSubscriber();
        //    await sub.SubscribeAsync(channel, handle);
        //}

        ///// <summary>
        ///// 发布
        ///// </summary>
        ///// <param name="channel"></param>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public async Task<long> PublishAsync(RedisChannel channel, RedisValue message)
        //{
        //    var sub = _connMultiplexer.GetSubscriber();
        //    return await sub.PublishAsync(channel, message);
        //}

        ///// <summary>
        ///// 发布（使用序列化）
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="channel"></param>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public async Task<long> PublishAsync<T>(RedisChannel channel, T message)
        //{
        //    var sub = _connMultiplexer.GetSubscriber();
        //    return await sub.PublishAsync(channel, Serialize(message));
        //}

        //#endregion 发布订阅-async

        //#endregion 发布订阅

        #region private method

        /// <summary>
        /// 添加 Key 的前缀
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string AddKeyPrefix(string key)
        {
            // return "{DefaultKey}:{key}"; // 原始代码
            return string.Format("{0}:{1}", DefaultKey, key);
        }

        #region 注册事件
        /// <summary>
        /// 添加注册事件
        /// </summary>
        private void AddRegisterEvent()
        {
            //_redisClient.TransactionQueued += RedisClient_TransactionQueued;
            //_redisClient.SubscriptionChanged += RedisClient_SubscriptionChanged;
            //_redisClient.SubscriptionReceived += RedisClient_SubscriptionReceived;
            //_redisClient.MonitorReceived += RedisClient_MonitorReceived;
        }

        /// <summary>
        /// 重新配置广播时（通常意味着主从同步更改）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RedisClient_TransactionQueued(object sender, RedisTransactionQueuedEventArgs e)
        {
            Console.WriteLine("{nameof(ConnMultiplexer_ConfigurationChangedBroadcast)}: {e.EndPoint}");
        }

        /// <summary>
        /// 发生内部错误时（主要用于调试）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RedisClient_SubscriptionChanged(object sender, RedisSubscriptionChangedEventArgs e)
        {
            Console.WriteLine("{nameof(ConnMultiplexer_InternalError)}: {e.Exception}");
        }

        /// <summary>
        /// 更改集群时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RedisClient_SubscriptionReceived(object sender, RedisSubscriptionReceivedEventArgs e)
        {
            Console.WriteLine("{nameof(ConnMultiplexer_HashSlotMoved)}: {nameof(e.OldEndPoint)}-{e.OldEndPoint} To {nameof(e.NewEndPoint)}-{e.NewEndPoint}, ");
        }

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RedisClient_MonitorReceived(object sender, RedisMonitorEventArgs e)
        {
            Console.WriteLine("{nameof(ConnMultiplexer_ConfigurationChanged)}: {e.EndPoint}");
        }
        #endregion 注册事件

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string Serialize(object obj)
        {
            if (obj == null)
                return null;

            var binaryFormatter = new BinaryFormatter();
            //var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var memoryStream = new MemoryStream())
            {
                //serializer.WriteObject(memoryStream, obj);
                binaryFormatter.Serialize(memoryStream, obj);
                var data = memoryStream.ToArray();
                return Convert.ToBase64String(data);
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        private static T Deserialize<T>(string json)
        {
            if (json == null)
                return default(T);

            var binaryFormatter = new BinaryFormatter();
            //var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream(Convert.FromBase64String(json)))
            {
                //var result = (T)serializer.ReadObject(memoryStream);
                var result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        private static object Deserialize(string json)
        {
            if (json == null)
                return null;

            var binaryFormatter = new BinaryFormatter();
            //var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream(Convert.FromBase64String(json)))
            {
                //var result = (T)serializer.ReadObject(memoryStream);
                return binaryFormatter.Deserialize(memoryStream);
            }
        }
        #endregion private method
    }
}
