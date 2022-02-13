using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.Aop.DataTracer.Models;
using CN.MACH.Aop.DataTracer.Services;
using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CN.MACH.Aop.DataTracer.Views.Redis
{
    /// <summary>
    /// RedisSubscribView.xaml 的交互逻辑
    /// 消息能修改能复制 能批量生产 能保存一个系列的消息 批量重发 保持原有时间间隔
    /// 考虑消息 消息内容 的复制粘贴及修改 删除
    /// 消息来源记录
    /// </summary>
    public partial class RedisSubscribView : UserControl
    {
        private RedisMsgRecorder recorder = new RedisMsgRecorder();
        public ObservableCollection<RedisMsgRecord> Records { get; private set; }
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

        public RedisSubscribView()
        {
            InitializeComponent();
            DataContext = this;
            recorder.Init(RedisMsgRecordReceived);
            Records = recorder.GetRecords("chanels");
            msglist.Visibility = Visibility.Visible;
        }
        private void RedisMsgRecordReceived(object sender, RedisMsgRecord record)
        {
            Dispatcher.Invoke(() =>
            {
                Records.Add(record);
                msglist.ScrollIntoView(msglist.Items[msglist.Items.Count - 1]);
            });
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Records.Clear();
            });
            recorder.ClearAllRecords();
        }

        private void Search()
        {
            recorder.Search("channels", new RedisSearchKeyWords() { Contents = searchContent.Text, KeyWords = searchKeyWords.Text }, SearchRecordsReceived);
        }
        private void SearchMsgs_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void SearchRecordsReceived(object sender, IEnumerable<RedisMsgRecord> records)
        {
            Dispatcher.Invoke(() =>
            {
                Records.Clear();
                foreach (var item in records)
                {
                    Records.Add(item);
                }
            });
        }
        private void LoadMsgs_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void ClearSavedMsgs_Click(object sender, RoutedEventArgs e)
        {
            recorder.ClearSavedMsgs();
        }
        private void SaveMsg_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            recorder.SaveMsg(btn.DataContext as RedisMsgRecord);
        }
        private void DelMsg_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            recorder.DelMsg(btn.DataContext as RedisMsgRecord);
        }
        private void SendMsg_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            recorder.SendMsg(btn.DataContext as RedisMsgRecord);
        }

        private void MsgName_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock t = sender as TextBlock;
            Clipboard.SetText(t.Text);
        }
    }
}
