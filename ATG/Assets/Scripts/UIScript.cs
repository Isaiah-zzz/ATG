using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    [SerializeField] private PlayerMovement playerScript;
    [SerializeField] private Text healthText;
    [SerializeField] private Text cornText;
    [SerializeField] private Text kernelText;

    // Update is called once per frame
    void Update()
    {
        healthText.text = "" + playerScript.CurrentHealth;
    }

}
