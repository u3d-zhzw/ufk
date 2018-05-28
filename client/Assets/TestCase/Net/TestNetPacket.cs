using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

public class TestNetPacket : MonoBehaviour
{
    private Dictionary<ushort, byte[]> dict = new Dictionary<ushort, byte[]>();

    private static ushort maxOfPack = 65535;
    private static ushort idx = 1;

    private ushort countOfPack = 0;

    private HallConnection conn = null;

    private MemoryStream inputStream = new MemoryStream();
    private MemoryStream outputStream = new MemoryStream();
    private long outputOffset = 0;
    private long inputOffset = 0;

    // Use this for initialization
    System.Collections.IEnumerator Start()
    {
        this.conn = new HallConnection();
        this.conn.NetPacketHander -= this.NetPackProc;
        this.conn.NetPacketHander += this.NetPackProc;

        this.countOfPack = maxOfPack;
        this.inputOffset = 0;
        this.outputOffset = 0;

        StopCoroutine("Producer");
        StartCoroutine("Producer");
        yield return new WaitForSeconds(3f);

        StopCoroutine("Consumer");
        StartCoroutine("Consumer");

        yield return null;
    }

    private static System.Random random = new System.Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private void RandBytes(out ushort id, out byte[] buffer)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memStream);

        id = idx;
        writer.Write(id);
        ++idx;

        int sec = DateTime.UtcNow.Second;
        int randomLen = random.Next(sec);
        string str = RandomString(randomLen);
        writer.Write(str);

        uint bufferSize = (uint)memStream.Length;
        writer.Seek(2, SeekOrigin.Begin);
        writer.Write(bufferSize);

        writer.Seek(0, SeekOrigin.End);


        writer.Close();
        memStream.Dispose();

        buffer = memStream.ToArray();
    }

    System.Collections.IEnumerator Producer()
    {
        while (this.countOfPack > 0)
        {
            ushort id = 0;
            byte[] data = null;
            RandBytes(out id, out data);

            if (id <= 0)
            {
                continue;
            }

            // 缓存内存
            this.Cache(data);

            // 记录发包信息
            if (!this.dict.ContainsKey(id))
            {
                this.dict.Add(id, data);
            }
            else
            {
                Debug.LogError("重复key:" + id);
            }

            --countOfPack;
            
            int a = random.Next() % 20;
            int b = random.Next() % 20;
            yield return new WaitForSeconds((float) a / b);
        }

        yield return null;
    }

    System.Collections.IEnumerator Consumer()
    {
        while (true)
        {
            if (outputStream.Length > 0)
            {
                // 模拟收包随机取一段内存
                long ouputLen = outputStream.Length;
                int randLen = (int)(random.Next() % ouputLen);
                if (randLen <= 0)
                {
                    continue;
                }

                byte[] buffer = new byte[randLen];
                this.outputStream.Seek(this.outputOffset, SeekOrigin.Begin);
                int size = this.outputStream.Read(buffer, 0, randLen);
                this.outputStream.Seek(0, SeekOrigin.End);
                this.outputOffset += size;

                // 将内存块传给网络层解析
                this.inputStream.Seek(0, SeekOrigin.End);
                this.inputStream.Write(buffer, 0, buffer.Length);
                this.inputStream.Seek(this.inputOffset, SeekOrigin.Begin);
                this.inputOffset += buffer.Length;

                //this.conn.Receive(inputStream);
            }

            yield return null;
        }
    }

    private void Cache(byte[] data)
    {
        outputStream.Write(data, 0, data.Length); 
    }

    private void NetPackProc(ushort id, byte[] data)
    {
        if (this.dict.ContainsKey(id))
        {
            byte[] lhs = this.dict[id];
            byte[] rhs = data;
            if (!(lhs.Length == rhs.Length && lhs.SequenceEqual(rhs)))
            {
                Debug.LogError("id:" + id + "的body不相等");
            }
        }
        else
        {
            Debug.LogError("找不到id:" + id + "对应的发出包");
        }
    }
 }
