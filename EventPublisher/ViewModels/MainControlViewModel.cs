using G2Cy.WpfHost.Interfaces;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPublisher.ViewModels
{
    public class MainControlViewModel:BindableBase
    {
        private IEventAggregator _eventAggregator;
        private string subject;

        public MainControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Subject = "666";// 默认值
            Msg = "来自插件EventPublisher中MainControl的消息";// 默认值
            PublishCmd = new DelegateCommand(PublishMethod);
        }

        private void PublishMethod()
        {
            _eventAggregator.Publish(Subject,Msg, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Growl.SuccessGlobal("消息发布成功");
            }
            else
            {
                //Growl.ErrorGlobal("消息发布失败");
            }
        }

        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                if (subject != value)
                {
                    subject = value;
                    RaisePropertyChanged(nameof(Subject));
                }
            }
        }

        private string msg;

        public string Msg
        {
            get { return msg; }
            set {
                if (msg != value)
                {
                    msg = value;
                    RaisePropertyChanged(nameof(Msg));
                }
            }
        }

        public DelegateCommand PublishCmd {  get; set; } 
    }
}
