using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrism_Demo.ViewModels
{
    /// <summary>
    /// 对话框调用 
    /// 通过IDialogService.ShowDialog 打开对话框
    ///     参数1: 需要打开的弹框注册名称
    ///     参数2: 传入弹框的参数
    ///     参数3 IDialogResult : 弹框关闭的回调方法, 接收弹框参数
    /// </summary>
    public class ViewCViewModel : BindableBase
    {
        private string _dialogTitle = "自定义对话框标题";
        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetProperty(ref _dialogTitle, value);
        }
        private string _dialogInput = "默认内容";
        public string DialogInput
        {
            get => _dialogInput;
            set => SetProperty(ref _dialogInput, value);
        }

        private string _dialogResult = "(尚未打开对话框)";

        public string DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        private readonly IDialogService _dialogService;

        public DelegateCommand OpenDialogCommand { get;}

        // 注入dialogService
        public ViewCViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            OpenDialogCommand = new DelegateCommand(OpenDialog);
        }

        private void OpenDialog()
        {
            // 定义传递给弹框的参数
            var parameters = new DialogParameters{
                { "Prompt", DialogInput },
                {"Title", DialogTitle }
            };

            // 1. ViewCDialog 和 ViewCDialogViewModel 需要在 App.xaml.cs 中注册
            // 2. ViewCDialog 是用户控件, ViewCDialogViewModel 必须实现 IDialogAware 接口, 以便接收参数和返回结果
            // 2. result 是对话框关闭的回调函数, 订阅ViewCDialog 的 RequestClose 事件
            _dialogService.ShowDialog("ViewCDialog", parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var res = result.Parameters.GetValue<string>("result");
                    DialogResult = $"点击了[确定]按钮, 弹框返回参数：{res}";
                }
                else
                {
                    DialogResult = $"点击了[取消]按钮, {result.Result} ";
                }
            });
        }
    }
}
