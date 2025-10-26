using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class SubMenuComponent_LoadScene : SubMenuComponent
{
    public string SceneName = "";
    override internal void ExecuteOnActive()
    {
        SceneManager.LoadScene(SceneName);
    }
}