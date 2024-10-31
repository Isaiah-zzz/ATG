using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{

    [SerializeField] float moveSpeed = 1f;

    Rigidbody2D body;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsFacingRight())
        {
            body.velocity = new Vector2(moveSpeed, 0f);
        }
        else
        {
            body.velocity = new Vector2(-moveSpeed, 0f);
        }

    }

    private bool IsFacingRight()
    {
        return transform.localScale.x > Mathf.Epsilon;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        transform.localScale = new Vector2(-((Mathf.Sign(body.velocity.x)) * Mathf.Abs(transform.localScale.x)), transform.localScale.y);
    }
}
