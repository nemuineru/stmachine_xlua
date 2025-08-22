using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    //サブメニュー. 開くオブジェクトを選択して選択して...という感じ
    [SerializeField]
    SubMenuComponent majorMenu;

    [SerializeField]
    AudioSource selectSnd, confirmSnd, cancelSnd;

    int Depth;

    [SerializeField]
    int selectedInputFr = 0, cancelInputFr = 0, controlInputFr = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        selectedInputFr = InputInstance.self.inputValues.MainButton_Read == 10 ? selectedInputFr + 1 : 0;
        cancelInputFr = InputInstance.self.inputValues.SubButton_Read == 10 ? cancelInputFr + 1 : 0;
    }
}
