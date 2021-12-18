using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.Aop.DataTracer.Models;
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
        public RedisSubscribView()
        {
            InitializeComponent();
            DataContext = this;
            cacheProvider = FodyCacheManager.GetInterface();
            RedisSubscribOptions options = new RedisSubscribOptions()
            {
                 SubscribKeys = ""
            };
            string subscribKeys = File.ReadAllText("redissubkeys.txt");

            string[] linekeys = StringUtils.SplitByLine(subscribKeys, StringUtils.Trim);
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            if (mQProvider != null && linekeys!=null &&  linekeys.Length > 0)
            {
                mQProvider.Init();
                string pfxKey = "zbytest";
                int n = 0;
                foreach (var key in linekeys)
                {
                    n++;
                    int nret = AppendSubscribe(mQProvider, pfxKey + key);
                    if (nret != ErrorCode.Success)
                        break;
                }
                mQProvider.Start();
                MessageBox.Show($"完成{n}个消息订阅");
            }

        }

        private int AppendSubscribe(IMQProvider mQProvider, string key)
        {
            if (string.IsNullOrEmpty(key)) return ErrorCode.EmptyParams;
            return mQProvider.Subscribe(key, (opt) =>
            {
                if (string.IsNullOrEmpty(opt)) return;
                RedisMsgRecord redisMsgRecord = new RedisMsgRecord()
                {
                    Name = key,
                    Value = opt
                };
                Dispatcher.Invoke(() =>
                {
                    Records.Add(redisMsgRecord);
                });
            });
        }
    }
}
