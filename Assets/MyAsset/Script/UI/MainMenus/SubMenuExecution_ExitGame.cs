using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class SubMenuComponent_ExitGame : SubMenuComponent
{
    override internal void ExecuteOnActive()
    { 
        Application.Quit();
    }
}