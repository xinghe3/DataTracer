using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.Aop.DataTracer.Models;
using CN.MACH.AOP.Fody.Index;
using CN.MACH.AOP.Fody.Utils;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CN.MACH.Aop.DataTracer.Views.Redis
{
    /// <summary>
    /// RedisSubscribView.xaml 的交互逻辑
    /// 待增加-增加一个再次发送消息的按钮方便测试
    /// 增加一个保存按钮 保存消息，便于再次发送 可以加注释 名称
    /// 增加一个加载保存消息 按钮 使用保存问题 发送消息
    /// 还需要一个删除消息的操作按钮
    /// </summary>
    public partial class RedisSubscribView : UserControl
    {
        private readonly ICacheProvider cacheProvider = null;
        private ObservableCollection<RedisMsgRecord> records = new ObservableCollection<RedisMsgRecord>();

        public ObservableCollection<RedisMsgRecord> Records
        {
            get { return records; }
            set { records = value; }
        }


        /// <summary>
        /// 控制暂停在界面更新消息 后台还是在接收消息
        /// </summary>
        public bool Paused
        {
            get { return (bool)GetValue(PausedProperty); }
            set { SetValue(PausedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Paused.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PausedProperty =
            DependencyProperty.Register("Paused", typeof(bool), typeof(RedisSubscribView), new PropertyMetadata(false));



        private List<RedisMsgRecord> allRecords = new List<RedisMsgRecord>();
        private RedisQueryWords query = new RedisQueryWords();
        private SavedMsgs msgs = new SavedMsgs();

        public RedisSubscribView()
        {
            InitializeComponent();
            DataContext = this;
            cacheProvider = FodyCacheManager.GetInterface();
            IndexSettings indexSettings = FodyCacheManager.GetSetting();
            string subscribKeys = File.ReadAllText("redissubkeys.txt");

            string[] linekeys = StringUtils.SplitByLine(subscribKeys, StringUtils.Trim);
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null && linekeys!=null &&  linekeys.Length > 0)
            {
                int nRet = mQProvider.Init();
                string pfxKey = indexSettings.CacheSetting.PefixKey;
                int n = 0;
                foreach (var key in linekeys)
                {
                    string[] keyDataArr = StringUtils.Split(key, "\t");
                    if (keyDataArr == null || keyDataArr.Length <= 0) continue;
                    n++;
                    string keyName = keyDataArr[0];
                    string keyDesc = keyDataArr.Length > 1 ? keyDataArr[1] : string.Empty;
                    int nret = AppendSubscribe(mQProvider, pfxKey + keyName, keyDesc);
                    if (nret != ErrorCode.Success)
                        break;
                }
                nRet = mQProvider.Start();
                if(nRet == ErrorCode.Success)
                    MessageBox.Show($"完成{n}个消息订阅");
                else
                    MessageBox.Show($"连接失败");
            }

        }

        private int AppendSubscribe(IMQProvider mQProvider, string key, string desc)
        {
            if (string.IsNullOrEmpty(key)) return ErrorCode.EmptyParams;
            return mQProvider.Subscribe(key, (opt) =>
            {
                if (string.IsNullOrEmpty(opt)) return;
                RedisMsgRecord redisMsgRecord = new RedisMsgRecord()
                {
                    Time = DateTime.Now.ToString("MM-dd HH:mm:ss"),
                    Name = key,
                    Value = opt,
                    Desc = desc,
                };
                if (!query.Filter(redisMsgRecord))
                    return;
                Dispatcher.Invoke(() =>
                {
                    if (Paused) // 如果暂停就不更新界面
                        return;
                    allRecords.Add(redisMsgRecord);
                    Records.Add(redisMsgRecord);
                    msglist.ScrollIntoView(msglist.Items[msglist.Items.Count - 1]);
                });
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Records.Clear();
            });
        }

        private void searchKeyWords_KeyUp(object sender, KeyEventArgs e)
        {
            string searchWords = searchKeyWords.Text;
            query = RedisQueryWordsBuilder.Build(searchWords);
            Search(searchWords);
        }

        private void Search(string searchWords)
        {
            Dispatcher.Invoke(() =>
            {
                IEnumerable<RedisMsgRecord> list = query != null ? allRecords.Where(r => query.Filter(r)) : allRecords;
                Records.Clear();
                foreach (var item in list)
                {
                    Records.Add(item);
                }
            });
        }

        class RedisQueryWords
        {
            public RedisQueryWords()
            {
                Includes = new List<string>();
                Excludes = new List<string>();

            }
            public ICollection<string> Includes { get; set; }
            public ICollection<string> Excludes { get; set; }

            public bool Filter(RedisMsgRecord record)
            {
                bool b = true;
                if (record == null || string.IsNullOrEmpty(record.Name)) return false;
                if (Includes != null && Includes.Count > 0)
                {
                    foreach (string include in Includes)
                    {
                        if (record.Name.Contains(include))
                            return true;
                    }
                }
                if (Excludes != null && Excludes.Count > 0)
                {
                    foreach (string exclude in Excludes)
                    {
                        if (record.Name.Contains(exclude))
                            return false;
                    }
                }
                return b;
            }
        }
        class RedisQueryWordsBuilder
        {
            public static RedisQueryWords Build(string words)
            {
                RedisQueryWords query = new RedisQueryWords();
                if (string.IsNullOrEmpty(words))
                    return query;
                string[] wordsArr = words.Split(' ');
                foreach (string word in wordsArr)
                {
                    if(word.StartsWith("in:"))
                    {
                        query.Includes.Add(word.Replace("in:", ""));
                    }
                    else
                    {
                        query.Excludes.Add(word);

                    }
                }
                return query;
            }
        }

        class SavedMsgs
        {
            private List<RedisMsgRecord> savedMsgs = new List<RedisMsgRecord>();
            private string savedFilePath = "savedmsgs.json";
            public List<RedisMsgRecord> Load()
            {
                string recordStrs = FileUtils.Read(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath), Encoding.UTF8);
                savedMsgs = JsonUtils.Deserialize<List<RedisMsgRecord>>(recordStrs);
                if (savedMsgs == null) savedMsgs = new List<RedisMsgRecord>();
                return savedMsgs;
            }
            public void Add(RedisMsgRecord record)
            {
                if (savedMsgs == null || savedMsgs.Count <= 0)
                    Load();
                savedMsgs.Add(record);
                string recordStrs = JsonUtils.Serialize(savedMsgs);
                FileUtils.Save(recordStrs,System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath), Encoding.UTF8);
            }
            public void Clear()
            {
                if (savedMsgs != null) savedMsgs.Clear();
                FileUtils.DeleteFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath));
            }
        }

        private void LoadMsgs_Click(object sender, RoutedEventArgs e)
        {
            allRecords = msgs.Load();
            Search(string.Empty);
        }
        private void ClearSavedMsgs_Click(object sender, RoutedEventArgs e)
        {
            msgs.Clear();
        }
        private void SaveMsg_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            RedisMsgRecord record = btn.DataContext as RedisMsgRecord;
            if (record == null)
                return;
            msgs.Add(record);
        }

        private void SendMsg_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            RedisMsgRecord record = btn.DataContext as RedisMsgRecord;
            if (record == null)
                return;
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null)
            {
                // add a string pub func
                mQProvider.Publish(record.Name, record.Value);
            }
        }
    }
}
