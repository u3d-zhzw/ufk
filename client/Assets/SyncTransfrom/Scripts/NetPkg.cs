using System;

using UnityEngine;

namespace SyncTransfrom
{
    public class NetPkg
    {
        public float time;
        public byte[] Buffer = new byte[128];
        public uint Size = 0;
        public float Time = 0f;

        public virtual EPkgType PkgType { get { return EPkgType.NONE; } }

        public void EnsureCapacity(uint size)
        {
            int bufferSize = this.Buffer.Length;
            if (size > bufferSize)
            {
                this.Buffer = new byte[2 * bufferSize];
            }
        }

        public void CopyFrom(byte[] buffer, uint offset, uint size)
        {
            this.EnsureCapacity(size);
            System.Buffer.BlockCopy(buffer, (int)offset, this.Buffer, 0, (int)size);
        }
    }

    //
    // 摘要:
    //      同步基本信息
    public class TransPkg : NetPkg
    {
        public Vector2 pos;
        public Vector2 velocity;
        public bool isHolding;

        public override EPkgType PkgType { get { return EPkgType.TRANS; } }
    }
}