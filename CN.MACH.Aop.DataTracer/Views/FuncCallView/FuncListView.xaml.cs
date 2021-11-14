using System.Windows.Controls;
using System.Windows.Input;

namespace WpfBlendTest1
{
    /// <summary>
    /// FuncListView.xaml 的交互逻辑
    /// </summary>
    public partial class FuncListView : UserControl
    {
        public FuncListView()
        {
            InitializeComponent();
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ListBox listbox = sender as ListBox;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            listbox.RaiseEvent(eventArg);
        }
    }
}