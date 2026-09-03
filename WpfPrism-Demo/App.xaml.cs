using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WpfPrism_Demo.Services;
using WpfPrism_Demo.ViewModels;
using WpfPrism_Demo.Views;

namespace WpfPrism_Demo
{
    /// <summary>
    /// 应用程序入口
    /// 
    /// 知识点1: ProsmApplication 引导 Bootstrapping
    /// App 继承 PrismApplication 后, Prism 接管整个项目启动流程
    /// Prism启动调用链:
    /// 
    /// 主程序启动, PrismApplication, OnStartUp -> Initialize
    ///     1.创建DI容器 Container, 包含 IContainerRegistry 容积注册器(写), containerProvider 容器提供者(读取)
    ///     2.调用 CreateModuleCatalog  创建 modulelCatalog 模块目录, 创建空的 或者 指定加载哪个目录下的模块
    ///     3.RegisterRequiredTypes  内部注册核心服务
    ///     4.RegisterTypes(containerRegistry)  必选, 在容器中注册自己的服务/视图/对话框 等
    ///     5.ConfigureModuleCatalog  可选, 像moduleCatalog 中添加模块
    ///     6.CreateShell 必选, 创建显示的壳外壳
    ///     7.InitializeShell + InitializedModules 内部完成
    /// OnInitialized 启动完成, 可进行初始化
    /// </summary>
    public partial class App : PrismApplication
    {
        // 创建模块目录, 返回值就是 moduleCatalog , 可以加载指定目录下的模块
        protected override IModuleCatalog CreateModuleCatalog()
        {
            //var moduleCatalog = new ModuleCatalog();

            // @.\Modules 加载调试输出目录的相对路径 Modules bin\Debug\net8-window\Modules 目录
            // 调试时, 需要将模块的输出的文件设置到这个目录中
            var moduleCatalog = new DirectoryModuleCatalog() { ModulePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules") };

            //moduleCatalog.Initialize();

            // 调试：看看到底找到了几个模块
            //foreach (var module in moduleCatalog.Modules)
            //{
            //    Debug.WriteLine($"Found module: {module.ModuleName}");
            //}
            //Console.WriteLine($"主程序创建模块目录~~ moduleCatalog(模块目录) hashCode: {moduleCatalog.GetHashCode()}");
            return moduleCatalog;
        }

        // 应用级依赖注册. 所有的服务、可导航视图、对话框都可以在这里注册到容器中
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Console.WriteLine($"主程序注册~~, containerRegistry(容器注册表) hashCode:{ containerRegistry.GetHashCode() }");
            containerRegistry.RegisterSingleton<IMessageService,MessageService>();

            // 注册本地存储服务
            containerRegistry.RegisterSingleton<AppLocalStorageService>(() =>
            {
                return new AppLocalStorageService("app.storage.json");
            });
            containerRegistry.RegisterSingleton<MainWindow>();

            // 注册页面
            containerRegistry.RegisterForNavigation<HomeView>();
            containerRegistry.RegisterForNavigation<ViewA>();
            containerRegistry.RegisterForNavigation<ViewB>();
            containerRegistry.RegisterForNavigation<ViewC>();
            containerRegistry.RegisterForNavigation<Module>();

            // 注册对话框
            containerRegistry.RegisterDialog<ViewCDialog,ViewCDialogViewModel>();

        }

        // 可选, 将. moduleCatalog 模块目录, 保存模块的元数据清单: 模块列表, 加载时机, 模块之间的依赖关系
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            Console.WriteLine($"主程序模块配置~~, moduleCatalog(模块目录) hashCode: {moduleCatalog.GetHashCode()}");
            // 手动将项目添加到模块
            moduleCatalog.AddModule<ModuleA.Plugin.ModuleAModule>(mode:InitializationMode.OnDemand);
            base.ConfigureModuleCatalog(moduleCatalog);
        }

        /// <summary>
        /// 创建外壳Shell, 外壳是承载各个 Region 的区域
        /// DryIoc 能自动解析并未注册的具体类型
        /// </summary>
        protected override Window CreateShell()
        {
            Console.WriteLine($"主程序创建Shell壳~~, Container hashCode: {Container.GetHashCode()}");
            return Container.Resolve<MainWindow>();
        }
        // 程序初始化完成
        protected override void OnInitialized()

        {
            Console.WriteLine("主程序初始化完成~~");
            base.OnInitialized(); // 显示外壳窗口

            // 知识点3+4 初始化导航, 把MainView 导航进"ContentRegion"区域

            var regionManager = Container.Resolve<IRegionManager>();

            // RequestNavigate(string regionName, string source) 
            // 导航到指定的资源: ContentRegion 展示Home页面内容
            // regionName: 要调用导航功能的区域名称
            // source: 需要展示内容的统一标识符
            // 回调函数内可以调试是否导航成功
            regionManager.RequestNavigate("ContentRegion", "HomeView", res =>
            {
                Console.WriteLine($"导航到HomeView {res.Result}");
            });
        }
    }

}
