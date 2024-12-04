using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBounceScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float bounceFactor = 1f;  // Controls the bounce strength when hitting a wall

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 collisionNormal = collision.contacts[0].normal;

        // Calculate the opposite force to apply
        Vector2 oppositeForce = -collisionNormal * bounceFactor * 2;

        // Apply the opposite force to the Rigidbody2D
        rb.AddForce(oppositeForce, ForceMode2D.Impulse);
    }
}
