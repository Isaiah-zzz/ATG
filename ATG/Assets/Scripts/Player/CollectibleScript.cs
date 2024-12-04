using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleScript : MonoBehaviour
{
    public float respawnTime = 30.0f;
    private SpriteRenderer spriteRenderer;
    private Collider2D collide;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerTossGrowth playerTossGrowth;
    private int collectCount;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collide = GetComponent<Collider2D>();
        playerTossGrowth = GameObject.FindWithTag("Player").GetComponent<PlayerTossGrowth>();
        playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();

            if (this.gameObject.CompareTag("HealthCollectible"))
            {
                playerMovement.AddHealth();
            }
            else
            {
                playerTossGrowth.AddCount(this.gameObject.tag);
            }
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
