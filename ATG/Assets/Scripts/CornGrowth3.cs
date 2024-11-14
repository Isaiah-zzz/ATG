using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornGrowth3 : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public GameObject spriteMask; // Reference to the GameObject with Sprite Mask component
    public float shrinkSpeed = 4f; // Speed at which the mask shrinks
    private bool isShrinking = true;
    private bool revealed = false;
    private float initY;

    void Start()
    {
        initY = transform.position.y + (transform.localScale.y / 2);
        spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     isShrinking = true; 
        // }

        if (revealed && Input.GetKeyDown(KeyCode.R)) {
            spriteRenderer.color = Color.grey;
        }

        // Shrink the mask downwards
        if (isShrinking && spriteMask.transform.localScale.y > 0)
        {
            // Decrease the height of the mask
            float newHeight = spriteMask.transform.localScale.y - (shrinkSpeed * Time.deltaTime);
            
            // Ensure it doesn't go below zero
            if (newHeight < 0)
            {
                newHeight = 0; // Cap it to zero
                isShrinking = false; // Stop shrinking when fully shrunk
                revealed = true;
            }

            // Apply the new scale
            spriteMask.transform.localScale = new Vector3(spriteMask.transform.localScale.x, newHeight, spriteMask.transform.localScale.z);

            Vector3 maskPosition = spriteMask.transform.position;

            // Adjust the Y position of the mask to keep the top at initialTopY
            float currentHeight = transform.localScale.y;
            maskPosition.y = initY - (currentHeight / 2); // Move mask up based on the current height

            spriteMask.transform.position = maskPosition;
        }
    }
}
