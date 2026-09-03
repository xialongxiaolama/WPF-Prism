using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrism_Demo.Services
{
    public class MessageService : IMessageService
    {
        public string Message()
        {
            return "这是通过 DryIoc 容器注入的 MessageService 提供的信息, 这就是依赖注入的意义, 类只依赖抽象接口, 不依赖具体实现, 由容器决定给实际的实现";
        }
    }
}
