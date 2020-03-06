using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SyncTransfrom
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SyncObject : MonoBehaviour
    {

        public bool IsSyncInterpolation = false;

        public Vector2 Velocity
        {
            get
            {
                return this.rd2d.velocity;
            }

            set
            {
                this.rd2d.velocity = value;
            }
        }

        private const float MAX_POS_THRESHOLD = 1f;

        private Rigidbody2D rd2d = null;

        private Vector2 lastPos;
        private Vector2 lastVelocity;

        // 同步参数
        private bool targetHolding = false;
        private float targetTime = 0f;
        private Vector2 targetPos = Vector2.zero;
        private Vector2 targetVelocity = Vector2.zero;

        // 两帧速度差求出的预判加速度
        private Vector2 targetAcc;
        private bool canStartInterpolate = false;

        public void Start()
        {
            if (this.rd2d == null)
            {
                this.rd2d = this.GetComponent<Rigidbody2D>();
                if (this.rd2d == null)
                {
                    Debug.LogError("找不到Rigidbody2D");
                }
            }
        }

        public void Update()
        {
        }

        private void FixedUpdate()
        {
            if (IsSyncInterpolation && canStartInterpolate)
            {
                Interpolate();
            }
        }

        private void Interpolate()
        {
            Vector2 curPos = this.transform.localPosition;
            Vector2 realPos = this.targetPos;

            Vector2 curVelocity = this.rd2d.velocity;
            Vector2 realVelocity = this.targetVelocity;

            if (SyncTransUtils.IsZeroVelocity(realVelocity) && SyncTransUtils.IsZeroVelocity(curVelocity))
            {
                this.transform.localPosition = this.targetPos;
                this.rd2d.velocity = this.targetVelocity;
                return;
            }

            if (this.targetHolding)
            {
                this.transform.localPosition = this.targetPos;
                this.rd2d.velocity = this.targetVelocity;
            }
            else
            {
                bool isSameDir = Vector2.Dot(this.targetVelocity.normalized, this.rd2d.velocity.normalized) > 0;
                float distance = Vector2.Distance(curPos, realPos);
                if (distance >= MAX_POS_THRESHOLD && isSameDir)
                {
                    // 施加一个从curPos指向realPos的力，位移差越大变加速度越大
                    float acc = (curVelocity.y - realVelocity.y) / Time.fixedDeltaTime * Math.Abs(this.targetPos.y - curPos.y);
                    this.rd2d.AddForce(this.rd2d.mass * (this.targetPos - curPos).normalized * Math.Abs(acc));
                }
            }

            // todo:碰撞到上下档板或者爱外力减速运动，前后两帧速度方向会不一致

            this.lastPos = curPos;
            this.lastVelocity = curVelocity;
        }

        public void AddForce(float force)
        {
            Vector2 v = this.rd2d.velocity.normalized;
            if (System.Math.Abs(v.y) <= float.Epsilon)
            {
                v.y = 1;
            }

            this.rd2d.AddForce(-v * force);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
        }

        public void RecvPkg(NetPkg pkg)
        {
            EPkgType type = pkg.PkgType;
            switch (type)
            {
                case EPkgType.TRANS:
                    this.SyncTrans((TransPkg)pkg);
                    break;
                default:
                    Debug.LogWarningFormat("无法处理的pkg类型: {0} ", type);
                    break;
            }
        }

        public void SyncTrans(TransPkg pkg)
        {
            this.targetAcc = (pkg.velocity - this.targetVelocity) / (pkg.time - this.targetTime);
            this.targetTime = pkg.time;
            this.targetPos = pkg.pos;
            this.targetVelocity = pkg.velocity;
            this.canStartInterpolate = true;
            this.targetHolding = pkg.isHolding;
        }
    }
}