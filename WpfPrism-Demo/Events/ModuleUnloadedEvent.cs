using Prism.Events;

namespace WpfPrism_Demo.Events
{
    /// <summary>
    /// 模块逻辑卸载事件。
    /// 注意：Prism 的 IModuleManager 不提供真正的 UnloadModule，
    /// 这里的"卸载"是逻辑层面的——从 Region 移除视图、从导航栏移除入口、标记状态为未加载。
    /// 发布方：ModuleViewModel
    /// 订阅方：MainWindowViewModel（收到后从左侧导航栏移除对应入口）
    /// </summary>
    public class ModuleUnloadedEvent : PubSubEvent<string>
    {
    }
}
