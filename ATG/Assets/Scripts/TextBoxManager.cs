using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBoxManager : MonoBehaviour
{
    public GameObject textBox;
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

        theText.text = textLines[currentLine];

        if(Input.GetKeyDown(KeyCode.Return)) {
            currentLine += 1;
        }

        if(currentLine > endAtLine) {
            DisableTextBox();
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
    }

    public void ReloadScript(TextAsset newTextLines) {
        if(newTextLines != null) {
            textLines = new string[1];
            textLines = (newTextLines.text.Split("\n"));
        }
    }
}
