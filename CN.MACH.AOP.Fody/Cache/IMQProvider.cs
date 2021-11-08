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
        /// 消息发布接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        void Publish<T>(string topic, T msg) where T : class;
    }
}
