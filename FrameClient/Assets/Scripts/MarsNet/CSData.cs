using System;
using System.Collections.Generic;
using System.IO;
using PBCommon;
using ProtoBuf;

//包头结构
public struct PackageConstant
{
    public static int PackMessageIdOffset = 0;

    // 消息id (1个字节)
    public static int PacklengthOffset = 1;

    //消息包长度 (2个字节)
    public static int PacketHeadLength = 3;
    //包头长度
}

public class CSData
{
    public static byte[] GetSendMessage<T>(T pb_Body, CSID messageID)
    {
        var packageBody = SerializeData(pb_Body);
        var packMessageId = (byte)messageID; //消息id (1个字节)

        var packlength = PackageConstant.PacketHeadLength + packageBody.Length; //消息包长度 (2个字节)
        var packlengthByte = BitConverter.GetBytes((short)packlength);

        var packageHeadList = new List<byte>();
        //包头信息
        packageHeadList.Add(packMessageId);
        packageHeadList.AddRange(packlengthByte);
        //包体
        packageHeadList.AddRange(packageBody);

        return packageHeadList.ToArray();
    }


    public static byte[] SerializeData<T>(T instance)
    {
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            Serializer.Serialize(ms, instance);
            bytes = new byte[ms.Position];
            var fullBytes = ms.GetBuffer();
            Array.Copy(fullBytes, bytes, bytes.Length);
        }

        return bytes;
    }

    public static T DeserializeData<T>(byte[] bytes)
    {
        using (Stream ms = new MemoryStream(bytes))
        {
            return Serializer.Deserialize<T>(ms);
        }
    }
}