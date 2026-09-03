using ModuleA.Plugin.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleA.Plugin
{
    /// <summary>
    /// 依赖于 Prism 的模块加载
    /// </summary>
    public class ModuleAModule : IModule
    {
        // 真实 DI 容器:
        // IContainerProvider 提供容器解析的能力
        // IContainerRegistry 提供容器注册的能力
        public void OnInitialized(IContainerProvider containerProvider)
        {
            Console.WriteLine($"ModuleA初始化完成~~, containerProvider(容器提供者) hashCode:{containerProvider.GetHashCode()}");
        }
        // 主程序在 PrismApplication中创建的根DI容器, 注册 服务/视图/对话框
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Console.WriteLine($"ModuleA注册~~, containerRegistry(容器注册者) hashCode:{containerRegistry.GetHashCode()}");
            containerRegistry.RegisterForNavigation<PageA>("ModuleA_PageA");
        }
    }
}
