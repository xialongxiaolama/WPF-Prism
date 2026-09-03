using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrism_Demo.ViewModels
{
    class ViewCDialogViewModel : BindableBase, IDialogAware
    {
        private string _title = "默认弹框标题";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _prompt = string.Empty;
        public string Prompt
        {
            get => _prompt;
            set => SetProperty(ref _prompt, value);
        }

        private string _inputText = "这是弹框默认返回的文本";
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        // 关闭弹框事件: 接收 DialogSerive.ShowDialog 第三个参数的回调方法
        public event Action<IDialogResult> RequestClose;

        public ViewCDialogViewModel()
        {
            OkCommand = new DelegateCommand(OkCallBack);
            CancelCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel)));
        }
        private void OkCallBack()
        {
            var p = new DialogParameters
            {
                {"result", InputText }
            };
            var result = new DialogResult(ButtonResult.OK, p);
            RequestClose?.Invoke(result);
        }



        /// <summary>允许关闭吗？返回 false 可以阻止（比如有未保存内容时）</summary>
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            Console.WriteLine("弹框关闭回调方法");
        }

        /// <summary>
        /// 打开弹框触发, 接收调用者传递过来的参数
        /// </summary>
        /// <param name="parameters"></param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            Console.WriteLine("弹框打开回调方法");

            if (parameters.ContainsKey("Prompt"))
            {
                Prompt = parameters.GetValue<string>("Prompt");
            }
            if (parameters.ContainsKey("Title"))
            {
                Title = parameters.GetValue<string>("Title");
            }
        }
    }
}
