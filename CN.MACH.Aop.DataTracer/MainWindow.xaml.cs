using CN.MACH.Aop.DataTracer.Views;
using CN.MACH.Aop.DataTracer.Views.Redis;
using CN.MACH.Aop.DataTracer.Views.Tracer;
using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

namespace CN.MACH.Aop.DataTracer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

        }

        private void MenuItemAOPCall_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DataTracerView();
        }
        private void MenuItemRedisSubscrib_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new RedisSubscribView();
        }
        private void MenuItemOptions_Click(object sender, RoutedEventArgs e)
        {
            SettingsView settingsView = new SettingsView();
            settingsView.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ICacheProvider cacheProvider = FodyCacheManager.GetInterface();
            IMQProvider mQProvider = cacheProvider as IMQProvider;
            mQProvider.Init();
            cacheProvider.Dispose();
        }
    }
}
