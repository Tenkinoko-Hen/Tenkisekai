using Common;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class HelloWorldService :Singleton<HelloWorldService>
    {
        //初始化
        public void Init()
        {

        }

        //实例化方法
        public void Start()
        {
            //订阅我要处理的消息 “FirstTestRequest"
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FirstTestRequest>(this.OnFirstTestRequest);
        }

        //消息处理函数 处理Start传下来的消息
        private void OnFirstTestRequest(NetConnection<NetSession> sender, FirstTestRequest request)
        {
            //浅打下日志
            Log.InfoFormat("这是一个测试:Hello:{0}", request.Helloworld);
        }

        public void Stop()
        {

        }
    }
}
