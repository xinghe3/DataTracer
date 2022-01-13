using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.ETL.Infrastructure.Cache
{
    public interface IMQProvider
    {
        /// <summary>
        /// 消息订阅接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="action"></param>
        void Subscribe<T>(string topic, Action<T> action) where T : class;

        /// <summary>
        /// 消息订阅接口 直接返回json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="action"></param>
        int Subscribe(string topic, Action<string> action);

        /// <summary>
        /// 启动 订阅服务器
        /// </summary>
        /// <returns></returns>
        int Start();
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        int Init();

        /// <summary>
        /// 消息发布接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        void Publish<T>(string topic, T msg) where T : class;

        /// <summary>
        /// 消息发布
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        void Publish(string topic, string msg);
    }
}
