using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;
using Cinemachine;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera Maincam;

    //サブメニュー. 開くオブジェクトを選択して選択して...という感じ
    [SerializeField]
    SubMenuComponent majorMenu;

    [SerializeField]
    AudioSource selectSnd, confirmSnd, cancelSnd;

    int Depth;

    [SerializeField]
    int selectedInputFr = 0, cancelInputFr = 0, controlInputFr = 0;

    [SerializeField, ReadOnly(true)]
    int prevSelectInput = 0;

    float setInputVal = 0.4f;
    float resetInputVal = 0.15f;

    [SerializeField, ReadOnly(true)]
    bool isSelectReset = true;

    //Main Menu - Indexed Text, and Descriptions
    [SerializeField, ReadOnly(true)]
    TMP_Text menuIndexText, descIndexText;

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1280, 720, true);
        Time.timeScale = 1.0f;
        Maincam.Priority = 10;
    }

    // Update is called once per frame
    void Update()
    {
        SelectInput();
    }

    void SelectInput()
    {
        //Detecting the longpress
        selectedInputFr = InputInstance.self.inputValues.MainButton_Read == 10 ? selectedInputFr + 1 : 0;
        cancelInputFr = InputInstance.self.inputValues.SubButton_Read == 10 ? cancelInputFr + 1 : 0;
        //Digital Value of this. -1, 0, 1..
        //Input on is like this - 
        // [       | ]
        //And resets at low
        // [  |      ]

        controlInputFr =
        (int)Mathf.Sign(InputInstance.self.inputValues.MovingAxis.y) *
        (int)Mathf.Clamp01(Mathf.Floor(Mathf.Abs((InputInstance.self.inputValues.MovingAxis.y) / setInputVal)));

        //0でないなら選択..
        if (prevSelectInput != controlInputFr && isSelectReset == true && controlInputFr != 0)
        {
            Debug.Log("selector inputted");
            majorMenu.IndexSelector(-controlInputFr);
            selectSnd.Play();

            //controlInputFr += yAx;
            isSelectReset = false;
        }
        isSelectReset = isSelectReset || Mathf.Abs(InputInstance.self.inputValues.MovingAxis.y) < resetInputVal;
        if (isSelectReset)
        {
            //reset the value on certain point
            prevSelectInput = controlInputFr;
        }

        //set Cancel First.
        if (cancelInputFr == 1 && majorMenu.subSelected != null)
        {
            cancelSnd.Play();
            majorMenu.setSubMenuBack();
        }
        if (selectedInputFr == 1)
        {
            confirmSnd.Play();
            majorMenu.setSubMenuActive();
        }

        menuIndexText.text = "./" + majorMenu.getSelectedMenuText();
        descIndexText.text = "> " + majorMenu.getSelectedMenuDescriptions();
    }

}
