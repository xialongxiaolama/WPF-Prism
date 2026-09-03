using Prism.Commands;
using Prism.Mvvm;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 主窗口左侧导航栏的单个导航项 ViewModel。
    /// 静态导航项（Home、ViewA 等）和动态导航项（模块加载后添加）共用此类型。
    /// </summary>
    public class NavItemViewModel : BindableBase
    {
        public NavItemViewModel(string displayName, string viewName)
        {
            DisplayName = displayName;
            ViewName = viewName;
        }

        /// <summary>导航按钮上显示的文字</summary>
        public string DisplayName { get; }

        /// <summary>点击后导航到的视图注册名（RegisterForNavigation 的 name 参数）</summary>
        public string ViewName { get; }

        private string _moduleName = string.Empty;
        /// <summary>关联的模块名（仅动态导航项有值，用于卸载时精确匹配）</summary>
        public string ModuleName
        {
            get => _moduleName;
            set => SetProperty(ref _moduleName, value);
        }

        private bool _isVisible = true;
        /// <summary>是否在导航栏可见。模块"卸载"时设为 false 实现隐藏效果。</summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private bool _isDynamic;
        /// <summary>是否为动态添加的导航项（即模块加载后自动添加的）。
        /// 静态项不允许被移除，动态项可以。</summary>
        public bool IsDynamic
        {
            get => _isDynamic;
            set => SetProperty(ref _isDynamic, value);
        }

        private DelegateCommand<string>? _navigateCommand;
        /// <summary>导航命令，由 MainWindowViewModel 统一注入</summary>
        public DelegateCommand<string> NavigateCommand
        {
            get => _navigateCommand!;
            set => SetProperty(ref _navigateCommand, value);
        }
    }
}
