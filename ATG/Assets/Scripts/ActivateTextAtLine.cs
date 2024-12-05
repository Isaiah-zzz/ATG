using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTextAtLine : MonoBehaviour
{

    public TextAsset theText;

    public int startLine;
    public int endLine;
    public TextBoxManager textBox;
    private bool playerInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        textBox = FindObjectOfType<TextBoxManager>();
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
        textBox.ReloadScript(theText, startLine, endLine);
        textBox.EnableTextBox();

    }
}
