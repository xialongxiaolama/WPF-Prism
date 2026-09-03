using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfPrism_Demo.Services;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 演示1: 获取页面传递参数
    /// 
    /// 演示2: journal 导航, 负责前进后退
    /// 
    /// 
    /// 演示3: 控制页面是否被销毁 IRegionMemberLifetime
    ///     KeepAlive = true , 页面离开不会被销毁
    ///     KeepAlice = false, 页面离开被销毁
    /// KeepAlive 和 IsNavigationTarget 的区别:
    ///     1. KeepAlive 控制实例对象在Region中是否会销毁 , IsNavigationTarget 控制导航请求过来是否复用当前页面在Region 中的实例
    ///     2. 如果KeepAlive = false, IsNavigationTarget = true. 下次进来复用实例, 但是Region 中不存在, 还是创建新的
    ///     3. 如果KeepAlive = true, IsNavigationTarget = false. 下次进来不复用实例, Region 中存在, 依旧创建新的, 内存越堆越多
    ///     4. 如果KeepAlive = false, IsNavigationTarget = false. 每次进来创建新的，离开销毁
    ///     5. 如果KeepAlive = true, IsNavigationTarget = true. 每次进来复用实例，离开不销毁
    ///  </summary>
    public class ViewBViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private readonly AppLocalStorageService _appLocalStorageService; // 本地存储服务

        private string _liftcycleLog = string.Empty; // 生命周期日志, 用来记录页面的生命周期方法调用顺序

        public string LifeCycleLog
        {
            get => _liftcycleLog;
            set => SetProperty(ref _liftcycleLog, value);
        }

        private string _receiveMessage = string.Empty;

        public string ReceiveMessage
        {
            get { return _receiveMessage; }
            set { SetProperty(ref _receiveMessage, value); }
        }



        private bool _keepAliveStatus = true;
        public bool KeepAliveStatus
        {
            get => _keepAliveStatus;
            set {
                SetProperty(ref _keepAliveStatus, value);
                _appLocalStorageService.SetItem("ViewB_KeepAliveStatus", value.ToString());
            }
        }
        public bool KeepAlive => KeepAliveStatus; // 控制页面离开时, 是否销毁

        private bool _isNavigationStatus = true;
        public bool IsNavigationStatus // 控制导航请求过来, 是否复用当前已经存在过的实例
        {
            get => _isNavigationStatus;
            set
            {
                SetProperty(ref _isNavigationStatus, value);
                _appLocalStorageService.SetItem("ViewB_IsNavigationStatus", value.ToString());  
            }
        }


        private IRegionNavigationJournal? _journal; // 导航历史记录, 用来实现前进后退功能
        private DispatcherTimer? _timer;




        #region INavigationAware 三个生命周期方法

        /// <summary>
        /// 控制页面是否被复用, 如果返回true, 则不会重新创建新的ViewAViewModel实例, 直接复用当前实例
        /// 如果返回false, 则页面跳转过来每次都会创建新的ViewAViewModel实例, 重新执行构造函数
        /// 用途:
        ///     1. 可以通过传递的参数, 用来判断是否需要新建页面, 比如不同的用户可以创建新的实例
        /// </summary>
        /// <param name="navigationContext"></param>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return IsNavigationStatus;
        }

        /// <summary>
        /// 从本页面离开, 切换到别的页面, 本视图还没有被销毁时执行
        /// 用途:
        ///     1. 取消订阅事件, 取消timer , 取消通讯订阅等, 防止内存泄漏
        ///     2. 保存页面状态, 比如滚动位置, 输入框内容等
        ///     3. 停止轮训, 关闭串口/设备监听
        /// </summary>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            LifeCycleLog += $"{DateTime.Now:HH:mm:ss}-OnNavigatedFrom - 从页面B离开, IsNavigationTarget={IsNavigationStatus}, KeepAlive={KeepAlive}\n";
            _timer?.Stop();
        }

        /// <summary>
        /// 导航到这个页面实例创建完成之后触发, 可以获取导航携带的参数
        /// 用途: 
        ///     1. 获取导航传递过来的参数
        ///     2. 页面初始化数据, 根据传递的参数去请求数据, 或者刷新UI
        ///     3. 开启设备订阅 事件订阅等
        /// </summary>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 读取本地存储的状态
            IsNavigationStatus = _appLocalStorageService.GetItem("ViewB_IsNavigationStatus") == "True";
            KeepAliveStatus = _appLocalStorageService.GetItem("ViewB_KeepAliveStatus") == "True";

            // 获取当前Region区域的导航历史记录, 用来实现前进后退功能
            // 不同的region区域的导航历史记录是独立的, 互不干扰
            _journal = navigationContext.NavigationService.Journal;
            LifeCycleLog += $"{DateTime.Now:HH:mm:ss}-OnNavigatedTo - 进入页面B, IsNavigationTarget={IsNavigationStatus}, KeepAlive={KeepAlive}\n";

            // 获取导航传递过来的参数, 通过 navigationContext.Parameters["key"] 获取
            if (navigationContext.Parameters.ContainsKey("message"))
            {
                ReceiveMessage = navigationContext.Parameters.GetValue<string>("message");
            }
            else
            {
                ReceiveMessage = "（没有收到 message 参数 —— 试试在 ViewB 点「跳转 ViewA 并携带参数」）";
            }

            // 创建定时器, 如果离开时不销毁, 定时器一直运行, 内存泄漏
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                Console.WriteLine("Timer Tick");
            };
            _timer.Start();
        }
        #endregion

        #region 委托方法
        public DelegateCommand GoBackCommand { get; } // 返回上一页
        public DelegateCommand GoForwardCommand { get; } // 前进下一页
        //public DelegateCommand ToggleReuseCommand { get; } // 切换组件复用状态
        //public DelegateCommand ToggleKeepAliveCommand { get; } // 切换KeepAlive状态


        #endregion
        public ViewBViewModel(AppLocalStorageService appLocalStorageService)
        {
            _appLocalStorageService = appLocalStorageService;

            LifeCycleLog += $"{DateTime.Now:HH:mm:ss}-创建ViewB页面实例\n";
            // 绑定返回命令
            this.GoBackCommand = new DelegateCommand(() =>
            {
                    _journal?.GoBack();
            });
            // 向前
            this.GoForwardCommand = new DelegateCommand(() =>
            {
                _journal?.GoForward();
            });
            // 切换页面复用状态
            // 关闭后, 下次导航到这个页面, 会创建新的实例
            //this.ToggleReuseCommand = new DelegateCommand(()=> IsNavigationStatus = !IsNavigationStatus);

            //// 切换页面离开时是否销毁状态
            //this.ToggleKeepAliveCommand = new DelegateCommand(() => KeepAliveStatus = !KeepAliveStatus);
        }
    }
}
