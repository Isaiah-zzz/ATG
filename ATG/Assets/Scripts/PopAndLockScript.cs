using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopAndLockScript : MonoBehaviour
{
    public float throwForce = 5f;
    public float growDelay = 2f;
    public float growMultiplier = 6f;

    private Rigidbody2D rb;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        // Apply the throw force; adjust direction as needed
        rb.AddForce(transform.right * throwForce, ForceMode2D.Impulse);

        // Start the growth coroutine
        StartCoroutine(GrowAfterDelay());
    }

    IEnumerator GrowAfterDelay()
    {
        yield return new WaitForSeconds(growDelay);
        
        // Increase size
        transform.localScale = originalScale * growMultiplier;
    }
}
