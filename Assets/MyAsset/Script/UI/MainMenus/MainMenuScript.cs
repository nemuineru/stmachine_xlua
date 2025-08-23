using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;

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

    [SerializeField,ReadOnly(true)]
    int prevSelectInput = 0;

    float setInputVal = 0.75f;
    float resetInputVal = 0.5f;
    bool isSelectReset = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        selectedInputFr = InputInstance.self.inputValues.MainButton_Read == 10 ? selectedInputFr + 1 : 0;
        cancelInputFr = InputInstance.self.inputValues.SubButton_Read == 10 ? cancelInputFr + 1 : 0;
        //Digital Value of this. -1, 0, 1..
        int yAx =
        (int)Mathf.Sign(InputInstance.self.inputValues.MovingAxis.y) *
        Mathf.Min(Mathf.FloorToInt(Mathf.Abs(InputInstance.self.inputValues.MovingAxis.y) / setInputVal), 1);

        if (prevSelectInput != yAx && isSelectReset == true)
        {
            controlInputFr += yAx;
            isSelectReset = false;
        }
        //reset the value on certain point
        prevSelectInput = yAx;
        isSelectReset = !isSelectReset || Mathf.Abs(InputInstance.self.inputValues.MovingAxis.y) < resetInputVal;
    }
}
