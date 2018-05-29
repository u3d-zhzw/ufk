using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using UnityEngine;
using System.Linq;
using System;
using Net.TestNet;

public class TestNetPacket : MonoBehaviour
{
    private HallConnection conn = null;
    private static System.Random rand = new System.Random();

    // Use this for initialization
    void Start()
    {
        this.conn = new HallConnection();
        this.conn.Connect("47.106.66.32", 56789, ()=> 
        {
            Debug.Log("connected success");
            StopCoroutine("Test");
            StartCoroutine("Test");
        });
    }

    void OnDestroy()
    {
        if (this.conn != null)
        {
            this.conn.Disconnect();
        }
    }

    void Update()
    {
        if (this.conn != null)
        {
            this.conn.Update();
        }
    }


    System.Collections.IEnumerator Test()
    {
        ushort reqId = 1;
        ushort respId = 2;

        this.conn.Listen(respId, this.NetPackProc);

        while (true)
        {
            var person = new Person
            {
                Id = 12345,
                Name = RandomString(),
                Address = new Address
                {
                    Line1 = RandomString(),
                    Line2 = RandomString()
                }
            };

            this.conn.Send<Person>(reqId, person);
            yield return new WaitForSeconds(1f);
        }
    }

    public static string RandomString()
    {
        int len = rand.Next(1, 20);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, len)
          .Select(s => s[rand.Next(s.Length)]).ToArray());
    }

    private void NetPackProc(ushort id, byte[] data)
    {
        Debug.Log("id:" + id);
        Debug.Log("data.lenght:" + data.Length);
    }
 }
