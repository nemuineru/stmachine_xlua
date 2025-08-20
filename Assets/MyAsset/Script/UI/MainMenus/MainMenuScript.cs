using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    List<TextMeshPro> menuTextObjs, gameModeTextObjs;

    [SerializeField]
    AudioSource selectSnd, confirmSnd, cancelSnd;

    int Depth;
    int Selected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
