using System.Windows;
using System.Windows.Controls;

namespace WpfBlendTest1.FuncCallView
{
    /// <summary>
    /// JsonView.xaml 的交互逻辑
    /// </summary>
    public partial class JsonView : UserControl
    {


        public string JsonString
        {
            get 
            {
                return (string)GetValue(JsonStringProperty); 
            }
            set 
            {
                // 绑定不会触发set
                SetValue(JsonStringProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for JsonString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JsonStringProperty =
            DependencyProperty.Register("JsonString", typeof(string), typeof(JsonView), new PropertyMetadata(string.Empty));


        public JsonView()
        {
            this.InitializeComponent();
        }
    }
}