目前的研究，必须要用Skype for bussiness账户，不能使用匿名。

调用服务端生成的Meeting Url，再调用浏览器打开。

1.如果Skype for bussiness已经配置好了Skype for bussiness账户，Skype for bussiness会处理Meeting,
我的程序能够正常运行，接受到conversation创建的事件，以及添加Participant等事件。


2.如果Skype for bussiness没有配置好Skype for bussiness账户，需要安装Skype Meetings App ，用它加入会议。
Skype Meetings App可以用匿名登录，但是只有视频和语音通信功能。
也可以使用Skype for bussiness账户登录，支持共享桌面和PPT的功能。  
这时我的程序不会收到任何Skype for bussiness的事件。 


匿名登录的用户也是能使用共享共享桌面和PPT的功能，只是权限的问题。
设置为Presenter就可以了。