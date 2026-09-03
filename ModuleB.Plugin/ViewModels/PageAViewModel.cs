using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB.ViewModels
{
    public class PageAViewModel:BindableBase
    {
        private string _helloMessage = "欢迎安装模块B -- On";
        public string HelloMessage
        {
            get => _helloMessage;
            set
            {
                SetProperty(ref _helloMessage, value);
            }
        }
    }
}
