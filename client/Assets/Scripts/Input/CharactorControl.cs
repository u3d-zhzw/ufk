using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorControl : MonoBehaviour
{
    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    public float speed = 1.0f;

    protected Rigidbody2D rb2d = null;
    protected Vector2 move = Vector2.zero;
    protected ContactFilter2D contactFilter;
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    // Use this for initialization
    void Start()
    {
        this.rb2d = this.gameObject.GetComponent<Rigidbody2D>();

        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {
        move = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            move += Vector2.up * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            move += Vector2.left * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            move += Vector2.down * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            move += Vector2.right * speed * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                RaycastHit2D hit = hitBufferList[i];
                float modifiedDistance = hit.distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }
		
        this.rb2d.position = rb2d.position + move.normalized * distance;
    }
}
