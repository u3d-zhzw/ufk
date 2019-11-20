using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using UnityEngine;
using System.Linq;
using System;
using Net.Person;
using Net.Hellow;
using ProtoBuf;

public class TestNetPacket : MonoBehaviour
{
    // private HallConnection conn = null;
    // private static System.Random rand = new System.Random();

    // // Use this for initialization
    // void Start()
    // {
    //     this.conn = new HallConnection();
    //     this.conn.Connect("47.106.66.32", 56789, ()=> 
    //     {
    //         Debug.Log("connected success");
    //         StopCoroutine("Test");
    //         StartCoroutine("Test");
    //     });
    // }

    // void OnDestroy()
    // {
    //     if (this.conn != null)
    //     {
    //         this.conn.Disconnect();
    //     }
    // }

    // void Update()
    // {
    //     if (this.conn != null)
    //     {
    //         this.conn.Update();
    //     }
    // }


    // System.Collections.IEnumerator Test()
    // {

    //     this.conn.Listen(2, this.NetPackProc);
    //     this.conn.Listen(4, this.NetPackProc);

    //     while (true)
    //     {
    //         int n = rand.Next(0, 2);
    //         if (n == 0)
    //         {
    //             Person p = new Person
    //             {
    //                 Id = 12345,
    //                 Name = RandomString(),
    //                 Address = new Address
    //                 {
    //                     Line1 = RandomString(),
    //                     Line2 = RandomString()
    //                 }
    //             };

    //             this.conn.Send<Person>(1, p);
    //             Debug.LogFormat("Person.id:{0}", p.Id);
    //             Debug.LogFormat("Person.Name:{0}", p.Name);
    //             Debug.LogFormat("Person.Address.Line1:{0}", p.Address.Line1);
    //             Debug.LogFormat("Person.Address.Line2:{0}", p.Address.Line2);

    //         }
    //         else if (n == 1)
    //         {
    //             Hellow h = new Hellow();
    //             h.msg = RandomString();

    //             Debug.LogFormat("Hellow.msg:{0}", h.msg);
    //             this.conn.Send<Hellow>(3, h);
    //         }

    //         yield return new WaitForSeconds(1f);
    //     }
    // }

    // public static string RandomString()
    // {
    //     int len = rand.Next(1, 20);
    //     const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    //     return new string(Enumerable.Repeat(chars, len)
    //       .Select(s => s[rand.Next(s.Length)]).ToArray());
    // }

    // private void NetPackProc(ushort id, byte[] data)
    // {
    //     Debug.Log("rec pkg");
    //     Debug.Log("id:" + id);
    //     Debug.Log("data.lenght:" + data.Length);

    //     if (id == 2)
    //     {
    //         Person p = Serializer.Deserialize<Person>(new MemoryStream(data));
    //         Debug.LogFormat("Person.id:{0}", p.Id);
    //         Debug.LogFormat("Person.Name:{0}", p.Name);
    //         Debug.LogFormat("Person.Address.Line1:{0}", p.Address.Line1);
    //         Debug.LogFormat("Person.Address.Line2:{0}", p.Address.Line2);
    //     }
    //     else if (id == 4)
    //     {
    //         Hellow h = Serializer.Deserialize<Hellow>(new MemoryStream(data));
    //         Debug.LogFormat("Hellow.msg:{0}", h.msg);
    //     }
    // }
 }
