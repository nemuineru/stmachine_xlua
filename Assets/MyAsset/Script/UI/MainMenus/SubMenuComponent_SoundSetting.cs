using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SubMenuComponent_SoundSetting : SubMenuComponent
{
    public AudioMixer mixer;
    public string ParamName;
    public bool isActive = true;

    void Update()
    {
        txt.text = ParamName + " " + (isActive == true ? "ON" : "OFF");
    }

    internal override void ExecuteOnly()
    {
        isActive = !isActive;
        Debug.Log("check!");
        mixer.SetFloat(ParamName, isActive == true ? 0f : -80f);
    }
}