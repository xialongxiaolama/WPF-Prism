using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrism_Demo.Services
{
    /// <summary>
    /// 依赖注入示范, 类只依赖接口/抽象, 不依赖具体实现, 实现解耦
    /// </summary>
    public interface IMessageService
    {
        string Message();
    }
}
