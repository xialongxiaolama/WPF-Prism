using Prism.Events;

namespace WpfPrism_Demo.Events
{
    /// <summary>
    /// 模块加载完成事件。
    /// 发布方：ModuleViewModel（调用 IModuleManager.LoadModule 成功后）
    /// 订阅方：MainWindowViewModel（收到后在左侧导航栏动态添加模块入口）
    /// </summary>
    public class ModuleLoadedEvent : PubSubEvent<ModuleLoadedPayload>
    {
    }

    /// <summary>
    /// 模块加载事件的传输数据
    /// </summary>
    public class ModuleLoadedPayload
    {
        /// <summary>模块在 Prism 目录中的名称（如 ModuleAModule）</summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>模块注册的可导航视图名称（如 ModuleA_PageA）</summary>
        public string NavigationViewName { get; set; } = string.Empty;

        /// <summary>在导航栏上显示的友好名称</summary>
        public string DisplayName { get; set; } = string.Empty;
    }
}
