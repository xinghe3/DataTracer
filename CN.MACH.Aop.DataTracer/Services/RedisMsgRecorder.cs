using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.Aop.DataTracer.Models;
using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.Aop.DataTracer.Services
{
    /// <summary>
    /// 封装 消息记录
    /// 可以组织多终端的消息集合 多终端的识别依赖于自定义的前置消息
    /// </summary>
    public class RedisMsgRecorder
    {
        private ICacheProvider cacheProvider = null;
        private ObservableCollection<RedisMsgRecord> allRecords = new ObservableCollection<RedisMsgRecord>();
        private static string defaultChannelName = "default_channel";
        private string currentChannelName = defaultChannelName;

        public bool Paused { get; set; }

        private RedisQueryWords query = new RedisQueryWords();
        private SavedMsgs msgs = new SavedMsgs();

        private string channelNames = null;
        private RedisSearchKeyWords redisSearchKeyWords = null;

        public int Init(EventHandler<RedisMsgRecord> MessageReceived)
        {
            int nRet = ErrorCode.Success;
            if (MessageReceived == null)
                throw new ArgumentNullException("接收消息事件参数 MessageReceived 未设置");
            IndexSettings indexSettings = FodyCacheManager.GetSetting();
            cacheProvider = FodyCacheManager.GetInterface(indexSettings?.CacheSetting);
            string subscribKeys = File.ReadAllText("redissubkeys.txt");
            string[] linekeys = StringUtils.SplitByLine(subscribKeys, StringUtils.Trim);
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null && linekeys != null && linekeys.Length > 0)
            {
                nRet = mQProvider.Init();
                string pfxKey = indexSettings.CacheSetting.PefixKey;
                int n = 0;
                foreach (var key in linekeys)
                {
                    string[] keyDataArr = StringUtils.Split(key, "\t");
                    if (keyDataArr == null || keyDataArr.Length <= 0) continue;
                    n++;
                    string keyName = keyDataArr[0];
                    string keyDesc = keyDataArr.Length > 1 ? keyDataArr[1] : string.Empty;
                    int nret = AppendSubscribe(mQProvider, pfxKey + keyName, keyDesc, MessageReceived);
                    if (nret != ErrorCode.Success)
                        break;
                }
                nRet = mQProvider.Start();

            }
            return nRet;
        }

        private int AppendSubscribe(IMQProvider mQProvider, string key, string desc, EventHandler<RedisMsgRecord> MessageReceived)
        {
            if (string.IsNullOrEmpty(key)) return ErrorCode.EmptyParams;
            return mQProvider.Subscribe(key, (opt) =>
            {
                if (string.IsNullOrEmpty(opt)) return;
                RedisMsgRecord redisMsgRecord = new RedisMsgRecord(DateTime.Now.ToString("HH:mm:ss"), key, opt, desc);
                AddToCurrentChannel(redisMsgRecord);
                if (!query.Filter(redisMsgRecord))
                    return;
                if (Paused) // 如果暂停就不更新界面
                    return;
                MessageReceived(this, redisMsgRecord);
                //Dispatcher.Invoke(() =>
                //{
                //    if (Paused) // 如果暂停就不更新界面
                //        return;
                //    Records.Add(redisMsgRecord);
                //    msglist.ScrollIntoView(msglist.Items[msglist.Items.Count - 1]);
                //});
            });
        }
        private int SwitchToChannel(string channelName)
        {
            currentChannelName = channelNames;
            return ErrorCode.Success;
        }
        private int AddToCurrentChannel(RedisMsgRecord redisMsgRecord)
        {
            redisMsgRecord.ChannelName = currentChannelName;
            allRecords.Add(redisMsgRecord);
            return ErrorCode.Success;
        }
        private IEnumerable<RedisMsgRecord> GetRecordsFromChannels(string channelNames, RedisQueryWords query)
        {
            var allRecords = GetRecords(channelNames);
            IEnumerable<RedisMsgRecord> list = query != null ? allRecords.Where(r => query.Filter(r)) : allRecords;
            return list;
        }
        public ObservableCollection<RedisMsgRecord> GetRecords(string channelNames)
        {
            return allRecords;
        }

        public int Search(string channelNames, RedisSearchKeyWords redisSearchKeyWords, EventHandler<IEnumerable<RedisMsgRecord>> SearchCallBack)
        {
            this.channelNames = channelNames;
            this.redisSearchKeyWords = redisSearchKeyWords;
            Task.Run(() =>
            {
                query = RedisQueryWords.Build(redisSearchKeyWords?.KeyWords, redisSearchKeyWords?.Contents);
                IEnumerable<RedisMsgRecord> list = GetRecordsFromChannels(channelNames, query);

                SearchCallBack?.Invoke(this, list);
                //Dispatcher.Invoke(() =>
                //{
                //    Records.Clear();
                //    foreach (var item in list)
                //    {
                //        Records.Add(item);
                //    }
                //});

            });
            return ErrorCode.Success;
        }

        public int ClearAllRecords()
        {
            allRecords.Clear();
            return ErrorCode.Success;
        }

        private int Search(EventHandler<IEnumerable<RedisMsgRecord>> SearchCallBack)
        {
            return Search(channelNames, redisSearchKeyWords, SearchCallBack);
        }
        public int ClearSavedMsgs()
        {
            return msgs.Clear();
        }
        public int SaveMsg(RedisMsgRecord record)
        {
            if (record == null)
                return ErrorCode.NULLPOINTER;
            return msgs.InsertUpdate(record);
        }
        public int DelMsg(RedisMsgRecord record)
        {
            if (record == null)
                return ErrorCode.NULLPOINTER;
            bool b = allRecords.Remove(record);
            return msgs.Remove(record);
        }
        public int SendMsg(RedisMsgRecord record)
        {
            if (record == null)
                return ErrorCode.NULLPOINTER;
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null)
            {
                // add a string pub func
                mQProvider.Publish(record.Name, record.Value);
            }
            return ErrorCode.Success;
        }
        public int LoadMsgs(EventHandler<IEnumerable<RedisMsgRecord>> SearchCallBack)
        {
            List<RedisMsgRecord> savedRecords = msgs.Load();
            allRecords.Clear();
            if (savedRecords != null)
                foreach (var savedRecord in savedRecords)
                {
                    allRecords.Add(savedRecord);
                }
            Search(SearchCallBack);
            return ErrorCode.Success;
        }
    }
}
