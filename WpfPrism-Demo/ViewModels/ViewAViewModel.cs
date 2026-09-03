using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using WpfPrism_Demo.Events;
using WpfPrism_Demo.Services;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 演示1: 事件的过滤, 退订, 反向发布
    ///     创建订阅时, 返回一个 SubscriptionToken, 用它可以退订
    ///     GetEvent<T>().Subscribe() 传入订阅方法, 过滤条件, 线程选项等参数
    ///     GetEvent<T>().Publish() 发布消息, 订阅方会收到
    ///     
    /// 演示2: 携带参数跳转到其他页面
    /// 必须实现 INavigationAware 接口, Prism 才会调用它, 用它带的参数跳转到其他界面
    ///     OnNavigatedTo -- 导航到当前页面, 实例创建完成之后执行, 可获取页面携带过来的参数
    ///     OnNavigatedFrom -- 从本页面离开, 切换到别的页面, 本视图还没有销毁时执行
    ///     IsNavigationTarget -- 导航请求过来, 是否复用当前已经存在过的实例
    /// NavigationContext -- 导航上下文, 里面有导航服务, 携带的参数等信息
    ///     NavigationService -- 当前 Region 的服务
    /// </summary>
    public class ViewAViewModel : BindableBase, INavigationAware
    {
        private readonly IEventAggregator _eventAggregator; // 聚合器
        private SubscriptionToken _token; // 订阅令牌, 用来退订
        private IRegionNavigationService _navigateService; // 导航服务, 用来跳转到其他页面
        private bool isSubscribed = true; // 订阅状态
        public bool IsSubscribed
        {
            get => isSubscribed;
            set
            {
                SetProperty(ref isSubscribed, value);
                if (value)
                {
                    SubscribeStatus = "订阅中（仅接收 Severity>0 的消息";
                }
                else
                {
                    SubscribeStatus = "已退订（不再接收任何消息）";
                }
            }
        }

        /// <summary>
        /// 订阅状态
        /// </summary>
        private string _subscribeStatus = "订阅中（仅接收 Severity>0 的消息";
        public string SubscribeStatus
        {
            get => _subscribeStatus;
            set => SetProperty(ref _subscribeStatus, value);
        }

        /// <summary>
        /// ObservableCollection 是 WPF 的集合类型, 实现了INotifyCollectionChanged 接口, 数据内容变时, UI 会自动刷新
        /// </summary>
        public ObservableCollection<string> MessageContent { get; } = new ObservableCollection<string>();


        public DelegateCommand PublishCommand { get; } // 发布消息
        public DelegateCommand ToggleSubscribeCommand { get; } //切换订阅
        public DelegateCommand GoToViewBCommand { get; } //跳转到ViewB

        public ViewAViewModel(IEventAggregator eventAggregator)
        {
            Console.WriteLine("创建窗口实例");
            // 知识点: 从聚合器获取 MessageSentEvent 事件的订阅
            // 
            // 参数1: 订阅的回调函数, 参数是PubSubEvent 携带的 MessagePayload 数据模型
            // 参数2: ThreadOption.UIThread --  回调切回 UI 线程（方便更新界面；主线程发布时两者没区别）
            // 参数3: keepSubscriberReferenceAlive = false, 弱引用, 防止 订阅对象无法回收 导致内存泄漏
            // 参数4: Predicate 过滤条件
            _eventAggregator = eventAggregator;
            _token = _eventAggregator.GetEvent<MessageSentEvent>().Subscribe(
                OnMessageReceived,
                ThreadOption.UIThread,
                false,
                p => p.Severity > 0
            );

            PublishCommand = new DelegateCommand(PublishMessage);

            ToggleSubscribeCommand = new DelegateCommand(ToggleSubscribe);

            GoToViewBCommand = new DelegateCommand(GotoViewB);
        }

        /// <summary>
        /// 跳转页面
        /// </summary>
        private void GotoViewB()
        {
            _navigateService.RequestNavigate("ViewB", new NavigationParameters {
                { "message", "Hello from ViewA" }
            });
        }

        /// <summary>
        /// 切换订阅状态
        /// </summary>
        private void ToggleSubscribe()
        {
            if (IsSubscribed)
            {
                // 取消订阅
                _token.Dispose();
                IsSubscribed = false;
            }
            else
            {
                IsSubscribed = true;
                _token = _eventAggregator.GetEvent<MessageSentEvent>().Subscribe(
                    OnMessageReceived,
                    ThreadOption.UIThread,
                    false,
                    p => p.Severity > 0
                    );
            }
        }

        /// <summary>
        /// 处理订阅消息
        /// </summary>
        /// <param name="payload"></param>
        private void OnMessageReceived(MessagePayload payload)
        {
            MessageContent.Insert(0, $"{payload.Time:HH:mm:ss} - Severity = {payload.Severity} : Message = {payload.Message}");
        }
        private void PublishMessage()
        {
            _eventAggregator.GetEvent<MessageSentEvent>().Publish(new MessagePayload
            {
                Message = "ViewA 发布信息, 等级为2",
                Severity = 2
            });
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true; // 允许复用当前实例, 不会重新创建新的ViewAViewModel
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Console.WriteLine("进入到页面A~");

            // 获取的是当前 ContentRegion 的作用域服务， 有自己独立的 Journal，
            // 通过requestNavigate跳转时， 会在当前作用域的 Journal 中记录历史， 通过导航服务的 GoBack/GoForward 可以实现前进后退
            _navigateService = navigationContext.NavigationService;

        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Console.WriteLine("从页面A离开~");
            //_token.Dispose(); // 离开页面时退订, 防止内存泄漏， 当前业务不适用
        }
    }
}
