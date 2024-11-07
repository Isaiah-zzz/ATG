using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class NpcTalk : MonoBehaviour
{
    public String message1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO: Make a textbox pop up and display the message
    //TODO: Allow player to cycle through text by pressing T
    public void Talk()
    {
        print(message1);
    }
}
