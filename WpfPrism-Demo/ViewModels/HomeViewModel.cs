using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfPrism_Demo.Events;
using WpfPrism_Demo.Services;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 首页: 实现DI注入, 事件聚合器发布
    /// </summary>
    public class HomeViewModel: BindableBase
    {
        private readonly IMessageService _messageService;
        private readonly IEventAggregator _eventAggregator;

        private string _homeSendMessage = "";
        private string _homeReceiveMessage;

        public string HomeReceivedMessage
        {
            get { return _homeReceiveMessage; }
            set { SetProperty(ref _homeReceiveMessage, value); }
        }

        public string HomeSendMessage
        {
            get => _homeSendMessage;
            set {
                SetProperty(ref _homeSendMessage, value);
                PublishImportantCommand.RaiseCanExecuteChanged();
                PublishNormalCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand PublishImportantCommand { get; }
        public DelegateCommand PublishNormalCommand { get; }

        public HomeViewModel(IMessageService messageService,IEventAggregator eventAggregator)
        {
            this._messageService = messageService;
            this._eventAggregator = eventAggregator;

            // 知识点: 构造注入的服务是抽象/接口, 不依赖具体实现, 实现可以随时更换
            this._homeReceiveMessage = messageService.Message();

            // 知识点: 第二个参数是 CanExcute 委托,用来判断 Command 命令是否可以执行
            // CanExcute 函数只会在初始化时自动调用一次来判断状态, 后续需要手动去  xxxCommand.RaiseCanExcuteChanged() 通知更新状态
            this.PublishImportantCommand = new DelegateCommand(()=>PublishMessage(2),CanPublishMessage);

            // 也可以通过Prism实现的ObservesProperty 让属性变化时自动通知 RaiseCanExcuteChanged
            this.PublishNormalCommand = new DelegateCommand(() => PublishMessage(1), CanPublishMessage).ObservesProperty(()=> HomeSendMessage);
        }

        private void PublishMessage(int severity)
        {
            _eventAggregator.GetEvent<MessageSentEvent>().Publish(new MessagePayload { Message = HomeSendMessage, Severity = severity });
        }

        /// <summary>
        /// 用来判断是否可以发布消息, 作为 DelegateCommand 的 CanExecute 方法
        /// 依赖的值更新时, 需要手动去调用通知方法 xxxCommand.RaiseCanExcuteChanged()
        /// RaiseCanExcuteChanged 内部会重新调用 CanPublishMessage 判断状态
        /// </summary>
        private bool CanPublishMessage()
        {
            return !string.IsNullOrWhiteSpace(HomeSendMessage);
        }
    }
}
