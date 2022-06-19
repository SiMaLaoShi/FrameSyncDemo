# x·FrameSync  

欢迎交流和补充优化。  
--
这只是个帧同步的demo，所以断线处理什么的都没有做，不在这个demo要呈现的范围内  
本demo适合2d的逻辑计算

FrameServer是服务端的demo，用unity做的  
FrameServer/Assets/Scripts/MarsNet/ServerConfig.cs里面的battleUserNum参数是配置一场战斗需要几个人参与

FrameClient是客户端的demo  
有做的:  
1.一致性随机数  
2.自己写的一些简单碰撞  
3.一个简单的游戏demo，只是可以发射子弹破坏障碍，如果需要攻击角色的可以自己扩展下  

没有做的(功能比较简单，需要的自己实现就好了)  
抗udp丢包：使用冗余包，具体冗余包几个，需要检测网络状况来进行动态调整，比较省流量  
抗网络抖动：使用后模拟，不建议使用帧缓存，帧缓存会加大操作延迟  

[原Git链接](https://github.com/OpenYourEye/FrameSync)

### 修改

1. 修复了同一个IP下登陆两个用户的bug，现在走给每个房间的用户走uid
2. 临时拼凑了一个回放系统

### 回放

先走正常模式点击游戏结束，就完成了一局游戏的录制，最终在在FrameClient的replay目录下生成1个文件夹3个replay文件，

![image-20220619163837217](pic/image-20220619163837217.png)

- replay_xx.byte 是每个操作的二进制文件
- replay_xx.json 是记录每个操作的消息id
- replay_xx_role.json 是记录当局的角色

使用回放功能的话，就直接打开ReplayInit场景就行了。

![](pic/回放演示.gif)

![多人回放演示](pic/多人回放演示.gif)
