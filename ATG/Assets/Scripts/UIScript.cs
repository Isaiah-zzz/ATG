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
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject resumeButton;

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Respawn()
    {
        ui.SetActive(true);
        gameOverScreen.SetActive(false);
        movementScript.Respawn();
    }

    public void DisplayPauseScreen()
    {
        Time.timeScale = 0f;
        pauseScreen.SetActive(true);
        pauseButton.SetActive(false);
        resumeButton.SetActive(true);
    }

    public void Resume()
    {
        pauseScreen.SetActive(false);
        pauseButton.SetActive(true);
        resumeButton.SetActive(false);
        Time.timeScale = 1f;
    }
}
