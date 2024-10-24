using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornGrowth3 : MonoBehaviour
{
    public GameObject spriteMask; // Reference to the GameObject with Sprite Mask component
    public float shrinkSpeed = 0.5f; // Speed at which the mask shrinks
    private bool isShrinking = false;
    private float initY;

    void Start()
    {
        initY = transform.position.y + (transform.localScale.y / 2);
    }

    void Update()
    {
        // Detect if the "F" key is pressed once
        if (Input.GetKeyDown(KeyCode.F))
        {
            isShrinking = true; // Start shrinking the mask
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
                Debug.Log("done");
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
