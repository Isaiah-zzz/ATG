using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTextAtLine : MonoBehaviour
{

    public TextAsset theText;

    public int startLine;
    public int endLine;
    public TextBoxManager textBox;
    public bool activated;
    private bool playerInRange = false;
    private int count;

    // Start is called before the first frame update
    void Start()
    {
        textBox = FindObjectOfType<TextBoxManager>();
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.T))
        {
            ActivateTextBox();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ActivateTextBox();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void ActivateTextBox()
    {
        textBox.ReloadScript(theText);
        textBox.currentLine = startLine;

        if (endLine == 0)
        {
            endLine = textBox.textLines.Length - 1;
        }

        textBox.endAtLine = endLine;
        textBox.EnableTextBox();

        if (activated)
        {
            Destroy(gameObject); // Optionally destroy if only one-time activation is needed
        }
    }
    // void OnTriggerEnter2D(Collider2D other) {
    //     if(other.tag == "Player") {
    //         textBox.ReloadScript(theText);
    //         textBox.currentLine = startLine;
    //         if(endLine == 0) {
    //             endLine = textBox.textLines.Length - 1;
    //         }
    //         textBox.endAtLine = endLine;
    //         textBox.EnableTextBox();

    //         if(activated) {
    //             Destroy(gameObject);
    //         }
    //     }
    // }
}
