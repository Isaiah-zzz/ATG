using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIScript : MonoBehaviour
{

    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerTossGrowth abilityScript;
    [SerializeField] private Text healthText;
    [SerializeField] private Text catapultText;
    [SerializeField] private Text cornText;
    [SerializeField] private Text kernelText;
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject gameOverScreen;

    // Update is called once per frame
    void Update()
    {
        healthText.text = "" + movementScript.CurrentHealth;
        catapultText.text = "" + movementScript.CurrentCatapultCount;
        cornText.text = "" + abilityScript.CurrentCornCount;
        kernelText.text = "" + abilityScript.CurrentPopcornCount;

        if (movementScript.CurrentHealth == 0)
        {
            DisplayGameOverScreen();
        }
    }

    void DisplayGameOverScreen()
    {
        ui.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Respawn()
    {
        ui.SetActive(true);
        gameOverScreen.SetActive(false);
        movementScript.Respawn();
    }
}
