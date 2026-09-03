using System.Windows;
using WpfPrism_Demo.ViewModels;

namespace WpfPrism_Demo.Views
{
    /// <summary>
    /// 壳窗口页面
    /// 
    /// Prism AutoWireViewModel 工作流程:
    /// 1. XAML 解析到 prism:ViewModelLocator.AutoWireViewModel="True" 时触发回调
    /// 2. 按命名约定推导 ViewModel 类型: WpfPrism_Demo.Views.MainWindow → WpfPrism_Demo.ViewModels.MainWindowViewModel
    /// 3. 从 DI 容器解析 ViewModel 实例
    /// 4. 通过 FrameworkElement.DataContext 设置绑定
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel mainWindowView)
        {
            InitializeComponent();
            // DataContext 由 AutoWireViewModel 自动设置, 无需手动赋值
            this.DataContext = mainWindowView;
        }
    }
}
