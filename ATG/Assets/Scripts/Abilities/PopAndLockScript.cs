using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopAndLockScript : MonoBehaviour
{
    public float growDelay = 1f;
    public float growMultiplier = 6f;

    private Rigidbody2D rb;
    private Vector3 originalScale;
    private float speed = 2f;
    private float direction = 1f;
    [SerializeField] LayerMask groundLayer;

    //Sound FX Clips
    [SerializeField] private AudioClip popClip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        // Apply the throw force; adjust direction as needed
        //rb.AddForce(transform.right * throwForce, ForceMode2D.Impulse);

        // Start the growth coroutine
        StartCoroutine(GrowAfterDelay());
    }

    IEnumerator GrowAfterDelay()
    {
        yield return new WaitForSeconds(growDelay);

        // Increase size
        transform.localScale = originalScale * growMultiplier;

        direction = rb.velocity.normalized.x;

        //Play Sound FX
        SoundFXManager.instance.PlaySoundFXClip(popClip, transform, .5f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (ColliderIsGround(other.collider) && speed == 2f)
        {
            speed = Mathf.Abs(rb.velocity.x);
            direction = rb.velocity.normalized.x;
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            rb.velocity = new Vector2(speed * direction, 0f);
        }
    }

    // checks if layer attached to collider is groundLayer
    private bool ColliderIsGround(Collider2D other) {
        return (groundLayer & (1 << other.gameObject.layer)) != 0;
    }
}
