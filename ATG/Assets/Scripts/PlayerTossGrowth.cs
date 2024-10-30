using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTossGrowth : MonoBehaviour
{
    public GameObject tossablePrefab; // Reference to the tossable object prefab
    public GameObject cornPrefab;
    public float tossForce = 3f; // Upward force for the toss

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TossObject();
        }
    }

    void TossObject()
    {
        float playerDirection = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 dir = new Vector3(playerDirection, 0, 0);
        
        GameObject grain = Instantiate(tossablePrefab, transform.position + dir * 0.9f, Quaternion.identity);
        Rigidbody2D rb = grain.GetComponent<Rigidbody2D>(); 
        rb.AddForce(new Vector2(playerDirection * 1f, 0.7f) * tossForce, ForceMode2D.Impulse);

        // Start coroutine to handle object disappearance on ground impact
        StartCoroutine(HandleObjectDisappear(grain));
    }

    IEnumerator HandleObjectDisappear(GameObject grain)
    {
        // Wait until the object hits the ground
        Collider2D collider = grain.GetComponent<Collider2D>();
        
        while (collider != null && !collider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            yield return null;
        }


        yield return new WaitForSeconds(0.3f); // Optional delay for effect

        SpriteRenderer spriteRenderer = grain.GetComponent<SpriteRenderer>();
        Vector2 spriteSize = spriteRenderer.bounds.size;
        Vector3 grainPosition = new Vector3(grain.transform.position.x, grain.transform.position.y - spriteSize.y / 2, grain.transform.position.z);  

        Destroy(grain);
        Instantiate(cornPrefab, grainPosition, Quaternion.identity);
    }
}
