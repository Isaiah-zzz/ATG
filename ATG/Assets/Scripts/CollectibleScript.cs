using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleScript : MonoBehaviour
{
    public float respawnTime = 1.0f;
    private SpriteRenderer spriteRenderer;
    private Collider2D collide;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collide = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        spriteRenderer.enabled = false;
        collide.enabled = false;
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);
        spriteRenderer.enabled = true;
        collide.enabled = true;
    }
}
