using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerTossGrowth abilityScript;
    [SerializeField] private Text healthText;
    [SerializeField] private Text catapultText;
    [SerializeField] private Text cornText;
    [SerializeField] private Text kernelText;

    // Update is called once per frame
    void Update()
    {
        healthText.text = "" + movementScript.CurrentHealth;
        catapultText.text = "" + abilityScript.CurrentCatapulCount;
        cornText.text = "" + abilityScript.CurrentCornCount;
        kernelText.text = "" + abilityScript.CurrentPopcornCount;
    }

}
