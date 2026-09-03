using Prism.Commands;
using Prism.Modularity;
using Prism.Mvvm;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 单个模块卡片的 ViewModel。
    /// 对应 Module 页面中每一张模块卡片，封装模块元数据、加载状态和加载/卸载命令。
    /// </summary>
    public class ModuleItemViewModel : BindableBase
    {
        private readonly IModuleInfo _moduleInfo;

        public ModuleItemViewModel(IModuleInfo moduleInfo)
        {
            _moduleInfo = moduleInfo;
            ModuleName = moduleInfo.ModuleName;
            // IModuleInfo.ModuleType 是 string 类型（模块类的程序集限定名），直接使用
            ModuleType = string.IsNullOrEmpty(moduleInfo.ModuleType) ? "未知类型" : moduleInfo.ModuleType;
            InitializationMode = moduleInfo.InitializationMode.ToString();

            // 根据模块名映射到其注册的导航视图名。
            // 模块的视图名是在模块内部 RegisterTypes 中通过 RegisterForNavigation 注册的，
            // 主程序无法直接获知，因此在这里维护一个显式映射。
            NavigationViewName = ModuleName switch
            {
                "ModuleAModule" => "ModuleA_PageA",
                "ModuleBModule" => "ModuleB_PageA",
                _ => string.Empty
            };

            DisplayName = ModuleName switch
            {
                "ModuleAModule" => "模块A (AddModule)",
                "ModuleBModule" => "模块B (目录扫描)",
                _ => ModuleName
            };
        }

        /// <summary>模块在 Prism 目录中的名称</summary>
        public string ModuleName { get; }

        /// <summary>模块的 .NET 类型名</summary>
        public string ModuleType { get; }

        /// <summary>加载时机：WhenAvailable / OnDemand</summary>
        public string InitializationMode { get; }

        /// <summary>模块注册的可导航视图名</summary>
        public string NavigationViewName { get; }

        /// <summary>卡片上显示的友好名称</summary>
        public string DisplayName { get; }

        private ModuleState _state = ModuleState.NotLoaded;
        /// <summary>模块当前状态（驱动卡片上按钮的可用状态和文字）</summary>
        public ModuleState State
        {
            get => _state;
            set
            {
                if (SetProperty(ref _state, value))
                {
                    // StateText 是计算属性，依赖 State。
                    // WPF 不会因为 State 变化而自动重新读取 StateText，
                    // 必须手动触发 PropertyChanged("StateText") 通知 UI 刷新文字。
                    RaisePropertyChanged(nameof(StateText));
                    // 状态变化时刷新命令的可执行状态
                    LoadCommand?.RaiseCanExecuteChanged();
                    UnloadCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>状态文本（用于 UI 显示）</summary>
        public string StateText => State switch
        {
            ModuleState.NotLoaded => "未加载",
            ModuleState.Loading => "加载中...",
            ModuleState.Loaded => "已加载",
            _ => "未知"
        };

        private DelegateCommand? _loadCommand;
        /// <summary>加载模块命令。仅在未加载状态下可执行。</summary>
        public DelegateCommand LoadCommand
        {
            get => _loadCommand!;
            set => SetProperty(ref _loadCommand, value);
        }

        private DelegateCommand? _unloadCommand;
        /// <summary>卸载模块命令。仅在已加载状态下可执行。</summary>
        public DelegateCommand UnloadCommand
        {
            get => _unloadCommand!;
            set => SetProperty(ref _unloadCommand, value);
        }
    }

    /// <summary>模块加载状态枚举</summary>
    public enum ModuleState
    {
        NotLoaded,
        Loading,
        Loaded
    }
}
