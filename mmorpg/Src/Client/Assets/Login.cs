using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Login : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //127.0.0.1 代表的是本机地址，如果只有一台电脑的话 服务端和客户端的IP都是这个
        //但如果不是一台电脑的话 这个地址只代表服务端
        Network.NetClient.Instance.Init("127.0.0.1", 8000); //初始化
        Network.NetClient.Instance.Connect();//链接

        //发送消息

        //创建主消息
        SkillBridge.Message.NetMessage message = new SkillBridge.Message.NetMessage();
        ////创建请求消息
        message.Request = new SkillBridge.Message.NetMessageRequest();
        //创建自定义消息
        message.Request.firstRequest = new SkillBridge.Message.FirstTestRequest();
        //给自己定义的消息填充数据
        message.Request.firstRequest.Helloworld = "Hello World";
        //调用SendMessage 发送出去
        Network.NetClient.Instance.SendMessage(message);
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
