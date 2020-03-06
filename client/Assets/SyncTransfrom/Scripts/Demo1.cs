using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SyncTransfrom
{
    public class Demo1 : MonoBehaviour
    {
        private SyncObject m_ball = null;
        private SyncObject m_ballSyncTarget = null;

        LinkedList<TransPkg> m_syncQue = new LinkedList<TransPkg>();
        
        ObjectPool<TransPkg> m_TransPkgPool = new ObjectPool<TransPkg>();

        private const float SYNC_INTERAL = 0.05f;
        private float m_time = 0f;

        // Start is called before the first frame update
        void Start()
        {
            GameObject world1 = GameObject.Find("World1");
            if (world1 == null)
            {
                Debug.LogError("找不到World1/Ball1");
                return;
            }

            Vector3 pos = world1.transform.position;
            pos.x = 10f;
            GameObject obj = GameObject.Instantiate(world1.gameObject);
            obj.name = "World2";
            obj.transform.position = pos;

            m_ball = GameObject.Find("World1/Ball1").GetComponent<SyncObject>();
            m_ball.IsSyncInterpolation = false;
            m_ballSyncTarget = GameObject.Find("World2/Ball1").GetComponent<SyncObject>();
            m_ballSyncTarget.IsSyncInterpolation = true;

           StartCoroutine(this.SimulateRecvPkg(m_syncQue, m_ballSyncTarget));
        }

        void Update()
        {
            if (m_ball == null)
            {
                return;
            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                this.Holding(10);
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                this.Holding(-10);
            }
        }

        private void FixedUpdate()
        {
            if (m_ball == null)
            {
                return;
            }

            // 定时发送同步包
            this.m_time += Time.fixedDeltaTime;
            while (this.m_time >= SYNC_INTERAL)
            {
                this.SendTransPkg();
                this.m_time -= SYNC_INTERAL;
            }
        }

        private void Holding(float force)
        {
            m_ball.AddForce(force);
            this.SendHoldPkg();
        }

        private void SendHoldPkg()
        {
            TransPkg pkg = m_TransPkgPool.Get();
            pkg.time = SyncTransUtils.Now;
            pkg.pos = m_ball.transform.localPosition;
            pkg.velocity = m_ball.Velocity;
            pkg.isHolding = true;

            // 模拟延时队列
            m_syncQue.AddLast(pkg);
        }

        private void SendTransPkg()
        {
            TransPkg pkg = m_TransPkgPool.Get();
            pkg.time = SyncTransUtils.Now;
            pkg.pos = m_ball.transform.localPosition;
            pkg.velocity = m_ball.Velocity;
            pkg.isHolding = false;

            // 模拟延时队列
            m_syncQue.AddLast(pkg);
        }

        public float RandRecvPkgTime()
        {
            // 正常网络延时100-300ms
            return SyncTransUtils.Now + UnityEngine.Random.Range(0.1f, 1f);
        }

        private IEnumerator SimulateRecvPkg(LinkedList<TransPkg> pkgList, SyncObject targetBall)
        {
            while (true)
            {
                if (pkgList.Count > 0)
                {
                    var itr = pkgList.First;
                    while (itr != null)
                    {
                        float now = SyncTransUtils.Now;

                        TransPkg pkg = itr.Value;
                        if (pkg.Time > now)
                        {
                            break;
                        }

                        this.m_ballSyncTarget.RecvPkg(pkg);

                        itr = itr.Next;
                        pkgList.RemoveFirst();

                        m_TransPkgPool.Release(pkg);
                    }
                }
                yield return null;
            }
        }
    }

}