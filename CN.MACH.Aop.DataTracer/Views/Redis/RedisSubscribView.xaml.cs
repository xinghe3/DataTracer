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
    /// 待增加-增加一个再次发送消息的按钮方便测试
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
                int nRet = mQProvider.Init();
                string pfxKey = "zbytest";
                int n = 0;
                foreach (var key in linekeys)
                {
                    n++;
                    int nret = AppendSubscribe(mQProvider, pfxKey + key);
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

        private int AppendSubscribe(IMQProvider mQProvider, string key)
        {
            if (string.IsNullOrEmpty(key)) return ErrorCode.EmptyParams;
            return mQProvider.Subscribe(key, (opt) =>
            {
                if (string.IsNullOrEmpty(opt)) return;
                RedisMsgRecord redisMsgRecord = new RedisMsgRecord()
                {
                    Time = DateTime.Now.ToString("MM-dd HH:mm:ss"),
                    Name = key,
                    Value = opt
                };

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
            Dispatcher.Invoke(() =>
            {
                var list = allRecords.Where(r => r.Name.Contains(searchWords)).ToList();
                Records.Clear();
                foreach (var item in list)
                {
                    Records.Add(item);
                }
            });
        }
    }
}
