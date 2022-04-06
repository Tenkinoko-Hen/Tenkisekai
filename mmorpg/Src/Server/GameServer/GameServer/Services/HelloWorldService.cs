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
        public void Init()
        {

        }

        public void Start()
        {

            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FirstTestRequest>(this.OnFirstTestRequest);
        }

        private void OnFirstTestRequest(NetConnection<NetSession> sender, FirstTestRequest request)
        {
          
            Log.InfoFormat("这是一个测试:Hello:{0}", request.Helloworld);
        }

        public void Stop()
        {

        }
    }
}
