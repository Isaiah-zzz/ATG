using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDirectionScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasLanded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!hasLanded)
        {
            // Rotate to face movement direction while in the air
            if (rb.velocity != Vector2.zero)
            {
                float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            transform.Rotate(0, 0, rb.angularVelocity * Time.deltaTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
        }
    }
}
