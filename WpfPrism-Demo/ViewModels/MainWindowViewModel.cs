using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using WpfPrism_Demo.Events;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// MainWindow 壳窗口的 ViewModel。
    /// 
    /// 左侧导航栏改为数据驱动：
    /// - 静态导航项（Home、ViewA/B/C、Module 管理页）在构造函数中初始化
    /// - 动态导航项（模块加载后自动添加）通过订阅 ModuleLoadedEvent 添加
    /// - 模块卸载时通过订阅 ModuleUnloadedEvent 移除（或隐藏）
    /// 
    /// 这样 Module 页面和 MainWindow 之间通过 EventAggregator 解耦，
    /// 不需要互相引用，符合 Prism 的松耦合设计理念。
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 左侧导航栏的全部导航项（静态 + 动态）。
        /// ObservableCollection 的 Add/Remove 会自动通知 UI 刷新。
        /// </summary>
        public ObservableCollection<NavItemViewModel> NavItems { get; }
            = new ObservableCollection<NavItemViewModel>();

        /// <summary>统一的导航命令，所有导航按钮共用，参数为视图注册名</summary>
        public DelegateCommand<string> NavigateCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            NavigateCommand = new DelegateCommand<string>(Navigate);

            // 初始化静态导航项
            InitStaticNavItems();

            // 订阅模块加载/卸载事件，实现侧边栏动态添加/移除导航入口
            _eventAggregator.GetEvent<ModuleLoadedEvent>().Subscribe(OnModuleLoaded);
            _eventAggregator.GetEvent<ModuleUnloadedEvent>().Subscribe(OnModuleUnloaded);
        }

        /// <summary>初始化静态导航项（主程序自带的页面）</summary>
        private void InitStaticNavItems()
        {
            NavItems.Add(new NavItemViewModel("Home(DI+发布事件)", "HomeView")
            {
                NavigateCommand = NavigateCommand
            });
            NavItems.Add(new NavItemViewModel("ViewA(事件订阅)", "ViewA")
            {
                NavigateCommand = NavigateCommand
            });
            NavItems.Add(new NavItemViewModel("ViewB(导航参数&生命周期)", "ViewB")
            {
                NavigateCommand = NavigateCommand
            });
            NavItems.Add(new NavItemViewModel("ViewC(对话框)", "ViewC")
            {
                NavigateCommand = NavigateCommand
            });
            NavItems.Add(new NavItemViewModel("模块化管理", "Module")
            {
                NavigateCommand = NavigateCommand
            });
        }

        /// <summary>
        /// 模块加载完成回调。
        /// 在侧边栏动态添加该模块的导航入口。
        /// 如果已存在（重复加载），则只确保可见，不重复添加。
        /// </summary>
        private void OnModuleLoaded(ModuleLoadedPayload payload)
        {
            // 检查是否已存在该模块的导航项
            var existing = NavItems.FirstOrDefault(n => n.ViewName == payload.NavigationViewName);
            if (existing != null)
            {
                existing.IsVisible = true;
                return;
            }

            // 添加动态导航项
            var navItem = new NavItemViewModel(payload.DisplayName, payload.NavigationViewName)
            {
                IsDynamic = true,
                IsVisible = true,
                ModuleName = payload.ModuleName,
                NavigateCommand = NavigateCommand
            };
            NavItems.Add(navItem);
        }

        /// <summary>
        /// 模块卸载回调。
        /// 从侧边栏移除该模块的导航入口（动态项才允许移除）。
        /// </summary>
        private void OnModuleUnloaded(string moduleName)
        {
            // 通过 ModuleName 精确匹配对应的动态导航项并移除
            var toRemove = NavItems
                .FirstOrDefault(n => n.IsDynamic && n.ModuleName == moduleName);

            if (toRemove != null)
            {
                NavItems.Remove(toRemove);
            }
        }

        /// <summary>执行导航到指定视图</summary>
        private void Navigate(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                return;

            _regionManager.RequestNavigate("ContentRegion", viewName);
        }
    }
}
