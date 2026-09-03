
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ModuleB.Views;
namespace ModuleB
{
    // tongguo1new DirectoryModuleCatalog() { ModulePath = @".\Modules" }; 插件扫描装上
    // 在模块的IModule实现类上加特性
    [Module(ModuleName = "ModuleBModule", OnDemand = true)]
    //[ModuleDependency("ModuleA")] 依赖其他插件
    public class ModuleBModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            Console.WriteLine($"ModuleB初始化完成~~, containerProvider(容器提供者) hashCode:{containerProvider.GetHashCode()}");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Console.WriteLine($"ModuleB注册~~, containerRegistry(容器注册者) hashCode:{containerRegistry.GetHashCode()}");
            containerRegistry.RegisterForNavigation<PageA>("ModuleB_PageA");
        }
    }

}
