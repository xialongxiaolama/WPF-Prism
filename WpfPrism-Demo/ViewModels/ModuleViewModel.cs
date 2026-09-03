using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using WpfPrism_Demo.Events;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 模块化管理页面的 ViewModel。
    /// 
    /// 职责：
    /// 1. 从 IModuleCatalog 读取所有已注册模块，包装成 ModuleItemViewModel 卡片列表
    /// 2. 调用 IModuleManager.LoadModule 实现按需加载
    /// 3. 实现"逻辑卸载"——Prism 不支持真正卸载模块（DI 注册无法撤销），
    ///    因此卸载 = 从 ContentRegion 移除该模块的视图 + 发布卸载事件让导航栏隐藏入口 + 标记状态
    /// 4. 通过 IEventAggregator 发布 ModuleLoadedEvent / ModuleUnloadedEvent，
    ///    与 MainWindowViewModel 解耦通信，实现侧边栏动态添加/移除导航项
    /// </summary>
    public class ModuleViewModel : BindableBase
    {
        private readonly IModuleCatalog _moduleCatalog;
        private readonly IModuleManager _moduleManager;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 模块卡片集合。使用 ObservableCollection 而非 List，
        /// 因为集合的 Add/Remove/Clear 会自动通知 UI 刷新。
        /// </summary>
        public ObservableCollection<ModuleItemViewModel> Modules { get; }
            = new ObservableCollection<ModuleItemViewModel>();

        public ModuleViewModel(
            IModuleCatalog moduleCatalog,
            IModuleManager moduleManager,
            IRegionManager regionManager,
            IEventAggregator eventAggregator)
        {
            _moduleCatalog = moduleCatalog;
            _moduleManager = moduleManager;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            // 构造时立即加载模块列表（原代码的 bug：构造函数没有调用 RefrashModules）
            RefreshModules();
        }

        /// <summary>
        /// 从模块目录读取所有模块，包装成卡片 ViewModel。
        /// 对于已经被 Prism 加载的模块（State == Initialized），同步标记为已加载。
        /// </summary>
        private void RefreshModules()
        {
            Modules.Clear();

            foreach (var info in _moduleCatalog.Modules)
            {
                var item = new ModuleItemViewModel(info);

                // 为每个卡片注入加载/卸载命令，回调到当前 ViewModel
                // 注意：不能在对象初始化器中引用 item 自身，因此在外部赋值
                item.LoadCommand = new DelegateCommand(
                    () => LoadModule(item),
                    () => item.State == ModuleState.NotLoaded);

                item.UnloadCommand = new DelegateCommand(
                    () => UnloadModule(item),
                    () => item.State == ModuleState.Loaded);

                // Prism 的 ModuleState 枚举：NotStarted -> LoadingTypes -> Initializing -> Initialized
                // 如果模块已经是 Initialized 状态，说明已加载
                if (info.State == Prism.Modularity.ModuleState.Initialized)
                {
                    item.State = ModuleState.Loaded;
                }

                Modules.Add(item);
            }
        }

        /// <summary>
        /// 加载模块。
        /// 调用链：IModuleManager.LoadModule -> 模块程序集加载 -> RegisterTypes 注册视图/服务
        /// -> OnInitialized 初始化 -> 发布 ModuleLoadedEvent -> MainWindow 侧边栏添加导航项
        /// </summary>
        private void LoadModule(ModuleItemViewModel item)
        {
            if (item.State != ModuleState.NotLoaded)
                return;

            item.State = ModuleState.Loading;

            try
            {
                // Prism 核心：按模块名加载。如果模块已加载过，此方法会直接返回不重复加载。
                _moduleManager.LoadModule(item.ModuleName);

                item.State = ModuleState.Loaded;

                // 发布事件，通知 MainWindowViewModel 在侧边栏添加导航入口
                _eventAggregator.GetEvent<ModuleLoadedEvent>().Publish(
                    new ModuleLoadedPayload
                    {
                        ModuleName = item.ModuleName,
                        NavigationViewName = item.NavigationViewName,
                        DisplayName = item.DisplayName
                    });
            }
            catch (System.Exception)
            {
                item.State = ModuleState.NotLoaded;
                throw;
            }
        }

        /// <summary>
        /// 逻辑卸载模块。
        /// 
        /// 重要原理：Prism 的 IModuleManager 没有 UnloadModule 方法。
        /// 原因是模块加载时会向 DI 容器注册类型（RegisterTypes），而 DryIoc 等容器
        /// 不支持在运行时移除已注册的类型。因此真正的"卸载程序集"在 .NET 中
        /// （除非使用 AssemblyLoadContext 隔离）也无法做到。
        /// 
        /// 这里实现的是逻辑卸载：
        /// 1. 发布 ModuleUnloadedEvent，让 MainWindow 侧边栏移除导航入口
        /// 2. 将卡片状态重置为 NotLoaded
        /// 
        /// 注意：卸载按钮只存在于 Module 管理页面，点击时 ContentRegion 显示的
        /// 就是 Module 页面本身，因此不需要处理"当前正显示模块视图"的情况。
        /// </summary>
        private void UnloadModule(ModuleItemViewModel item)
        {
            if (item.State != ModuleState.Loaded)
                return;

            // 发布卸载事件，通知 MainViewModel 从侧边栏移除导航项
            _eventAggregator.GetEvent<ModuleUnloadedEvent>().Publish(item.ModuleName);

            // 重置卡片状态
            item.State = ModuleState.NotLoaded;
        }
    }
}
