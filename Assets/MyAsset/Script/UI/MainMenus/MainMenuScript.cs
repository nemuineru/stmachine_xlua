using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    //サブメニュー. 開くオブジェクトを選択して選択して...という感じ
    [SerializeField]
    List<SubMenuComponent> menus;

    [SerializeField]
    AudioSource selectSnd, confirmSnd, cancelSnd;

    int Depth;

    int selectedInputFr = 0, cancelInputFr = 0, controlInputFr = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
