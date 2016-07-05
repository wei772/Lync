1. 每一个参与人都需要账号吧(需要)

2. 怎么打开现有的会议

3. 怎么生成uri


sip:tonyxia@o365ms.com;gruu;opaque=app:conf:focus:id:8BBGHSMP

https://meet.lync.com/shlinker-o365ms/tonyxia/8BBGHSMP


https://social.msdn.microsoft.com/Forums/lync/en-US/8252794d-ea59-46ae-a051-b1685269ccaa/joining-the-conference-problem-in-lync-sdk?forum=communicatorsdk

I'm afraid, you can't use Lync API to Join conference without successfully signing into Lync,
 and there is no way to sign-in to Lync as anonymous user. Th
e functionality that Lync Attendee is using to Join anonymously is not exposed through the Lync API.


To join a conf anonymously you need to either dial into the server directly or try to join using Lync API while signing into a different server 
that has no federation relationship with the actual conference server.

4. UI Suppression Mode  not  supprot  contentsharing

5. Video是否支持多个participate? 目前看是不支持的。

6. 越看越无力





@黎为，像昨天中午咱俩所说，你有研究成果，就在这里更新一下：
1. 先知道我们在集成时，有几个大的概念是要集成的：
1）语音会议
    
2）视频会议（在PC端是可以看到多路视频的）
    只能显示两个视频

3）PC端是有主持人的

4）PC和PC端共享桌面和PPT文档

    只能共享桌面，不能PPT

5）PC和Mobile会议互通（Mobile是Guest模式，PC创建会议是不是也要同样的模式）

    不支持Guest模式
6）Skype的文本消息和人员我们不需要。即时消息用我们微会议，人员用我们同事录。

2. 有UI的SDK和无UI的SDK分别能做成什么样子（我已经说服客户，UI不嵌在随办里面没问题）

    有UI的无法自定义界面，能够发送文本信息。

3. 我们要继承的功能，哪些是SDK封装好的，哪些需要我们自己写代码逻辑


4. Mobile端Gavin已经研究清楚，可以和Gavin交流。

