using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrism_Demo.Events
{
    /// <summary>
    /// 事件耦合器的传输数据模型
    /// Severity 用来演示订阅时的 Predicate 过滤：0=普通，1=提示，2=重要。
    /// </summary>
    public class MessagePayload
    {
        public string Message { get; set; } = string.Empty;
        public int Severity { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

    }
    /// <summary>
    /// PubSubEvent<T> 事件基类, 自定义事件只需要继承它, 不需要写任何实现
    /// 发布方和订阅方互不依赖, 只需要通过 IEventAggregator 这个信使间接通讯
    /// </summary>
    public class MessageSentEvent : PubSubEvent<MessagePayload>
    {
    }
}
