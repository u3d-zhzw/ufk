using System;

using UnityEngine;

namespace SyncTransfrom
{
    public class SyncTransUtils
    {
        private static uint s_tmpValue = 0;

        public static float Now
        {
            get
            {
                return UnityEngine.Time.timeSinceLevelLoad;
            }
        }

        public unsafe static void ToBytes(int v, byte[] buffer, ref uint begin)
        {
            s_tmpValue = *(uint*)&(v);

            buffer[begin++] = (byte)s_tmpValue;
            buffer[begin++] = (byte)(s_tmpValue >> 8);
            buffer[begin++] = (byte)(s_tmpValue >> 16);
            buffer[begin++] = (byte)(s_tmpValue >> 24);
        }


        public unsafe static void ToBytes(Vector2 v, byte[] buffer, ref uint begin)
        {
            // x坐标
            s_tmpValue = *(uint*)&(v.x);
            buffer[begin++] = (byte)s_tmpValue;
            buffer[begin++] = (byte)(s_tmpValue >> 8);
            buffer[begin++] = (byte)(s_tmpValue >> 16);
            buffer[begin++] = (byte)(s_tmpValue >> 24);

            // y坐标
            s_tmpValue = *(uint*)&(v.y);
            buffer[begin++] = (byte)s_tmpValue;
            buffer[begin++] = (byte)(s_tmpValue >> 8);
            buffer[begin++] = (byte)(s_tmpValue >> 16);
            buffer[begin++] = (byte)(s_tmpValue >> 24);
        }

        public unsafe static void ToBytes(float v, byte[] buffer, ref uint begin)
        {
            s_tmpValue = *(uint*)&(v);

            buffer[begin++] = (byte)s_tmpValue;
            buffer[begin++] = (byte)(s_tmpValue >> 8);
            buffer[begin++] = (byte)(s_tmpValue >> 16);
            buffer[begin++] = (byte)(s_tmpValue >> 24);
        }

        public unsafe static Vector2 ToVector2(byte[] buffer, ref uint begin)
        {
            Vector2 v;
            fixed (byte* pbyte = &buffer[begin])
            {
                v.x = *((float*)pbyte);
                v.y = *((float*)(pbyte + 4));

                begin += 8;
                return v;
            }
        }

        public unsafe static float ToFloat(byte[] buffer, ref uint begin)
        {
            fixed (byte* pbyte = &buffer[begin])
            {
                float v = *((float*)(pbyte));
                begin += 4;

                return v;
            }
        }

        public static bool IsZeroVelocity(Vector2 v)
        {
            return v.magnitude <= 0.0001f;
        }

    }
}