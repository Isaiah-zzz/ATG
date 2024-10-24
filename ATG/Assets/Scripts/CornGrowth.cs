using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornGrowth : MonoBehaviour
{
    public float growthSpeed = 0.5f; // Speed of growth
    public float maxHeight = 1f;   // Maximum height limit
    private Vector3 initialScale;
    private bool isGrowing = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the sprite's scale to 0 on the y-axis to "despawn" it
        initialScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        transform.localScale = initialScale;
    }

    // Update is called once per frame
    void Update()
    {
        // Hold down the "E" key to start growing the sprite
        if (Input.GetKey(KeyCode.E))
        {
            isGrowing = true;
        }
        else
        {
            isGrowing = false; // Stop growing when "E" is released
        }

        // Grow the sprite upwards until the max height is reached
        if (isGrowing && transform.localScale.y < maxHeight)
        {
            // Gradually increase the height (y-axis) to make the sprite appear from the ground up
            transform.localScale += new Vector3(0, growthSpeed * Time.deltaTime, 0);

            // Stop growing when maxHeight is reached
            if (transform.localScale.y >= maxHeight)
            {
                transform.localScale = new Vector3(transform.localScale.x, maxHeight, transform.localScale.z);
            }
        }
    }
}
