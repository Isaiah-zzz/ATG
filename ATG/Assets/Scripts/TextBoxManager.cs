using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBoxManager : MonoBehaviour
{
    public GameObject textBox;

    public TMP_Text staticText;
    public TMP_Text theText;
    public TextAsset textfile;
    public string[] textLines;

    public int currentLine;
    public int endAtLine;
    public PlayerMovement player;

    public bool isActive = false;
    public bool stopPlayer;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();

        if(textfile != null) {
            textLines = textfile.text.Split("\n");
        }

        if(endAtLine == 0) {
            endAtLine = textLines.Length - 1;
        }

        if(isActive) {
            EnableTextBox();
        }  else {
            DisableTextBox();
        }
    }

    void Update() {
        if(!isActive) {
            return;
        }

        if (currentLine > endAtLine)
        {
            DisableTextBox();
            return;
        }

        theText.text = textLines[currentLine];

        if(Input.GetKeyDown(KeyCode.Space)) {
            currentLine += 1;
        }

    }

    public void EnableTextBox() {
        textBox.SetActive(true);
        isActive = true;

        if(stopPlayer) {
            player.canMove = false;
        }
    }

    public void DisableTextBox() {
        textBox.SetActive(false);
        isActive = false;

        player.canMove = true;
        player.alreadyInShoutBox = false;
    }

    public void ReloadScript(TextAsset newTextLines, int startLine, int endLine)
    {
        if (newTextLines != null)
        {
            textLines = newTextLines.text.Split('\n');
        }
        staticText.text = textLines[0]; // Set static NPC name or title
        currentLine = startLine + 1;
        endAtLine = endLine;
        player.canMove = false;
    }
}
