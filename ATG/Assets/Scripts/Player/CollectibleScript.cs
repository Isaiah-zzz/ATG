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
    [SerializeField] private AudioClip collectibleClip;

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

            if (this.gameObject.CompareTag("HealthCollectible") || this.gameObject.CompareTag("CatapultCollectible"))
            {
                playerMovement.AddCount(this.gameObject.tag);
            }
            else
            {
                playerTossGrowth.AddCount(this.gameObject.tag);
            }
        }
    }

    void Collect()
    {
        SoundFXManager.instance.PlaySoundFXClip(collectibleClip, transform, 1f);
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
