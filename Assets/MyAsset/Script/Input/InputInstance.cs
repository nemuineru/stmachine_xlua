using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputInstance : MonoBehaviour
{
    
    
    static public int GetDigitalAxis(Vector2 Axis)
    {
        string deb = "";
        int ret = 5;
        float range = 0.2f;
        if(Mathf.Abs(Axis.x) > range)
        {
            deb += ("x" + Mathf.Sign(Axis.x));
            ret += 1 * (int)Mathf.Sign(Axis.x);
        }

        if(Mathf.Abs(Axis.y) > range)
        {
            deb += ("y" + Mathf.Sign(Axis.y));
            ret += 3 * (int)Mathf.Sign(Axis.y);
        }
        //Debug.Log(deb);
        return ret;
    }

    public InputValueManagers inputValues = new InputValueManagers();
    static public InputInstance self;
    // Start is called before the first frame update
    
    Input_Basic inputBasic;

    [System.Serializable]
    public class InputValueManagers
    {
        public Vector2 MovingAxisRead = Vector2.zero;
        public Vector2 LookingAxisRead = Vector2.zero;
        public Vector2 mAx_Min = Vector2.one * 0.2f;        
        public Vector2 mAx_Max = Vector2.one * 0.95f;
        public int MainButton_Read = 0;
        public int ActionButton_Read = 0;
        public int SubButton_Read = 0;
        public int UtilityButton_Read = 0;
        public int Extra1Button_Read = 0;
        public int Extra2Button_Read = 0;
        public int MenuButton_Read = 0;
        public int SubMenuButton_Read = 0;
        public int DebugCMD_ClearALL_Read = 0;

        public int MovingAxis_Digital
        {
            get
            {
                string deb = "";
                int ret = 5;
                float range = 0.2f;
                if(Mathf.Abs(MovingAxisRead.x) > range)
                {
                    deb += ("x" + Mathf.Sign(MovingAxisRead.x));
                    ret += 1 * (int)Mathf.Sign(MovingAxisRead.x);
                }

                if(Mathf.Abs(MovingAxisRead.y) > range)
                {
                    deb += ("y" + Mathf.Sign(MovingAxisRead.y));
                    ret += 3 * (int)Mathf.Sign(MovingAxisRead.y);
                }
                //Debug.Log(deb);
                return ret;
            }
        }

        public Vector2 MovingAxis
        {
            get {
                    Vector2 vect = self.inputBasic.Base.Controller_MoveAxis.ReadValue<Vector2>();
                    MovingAxisRead = 
                    new Vector2(Mathf.Sign(vect.x) * Clamp_set_01(mAx_Min.x,mAx_Max.x, Mathf.Abs(vect.x)), 
                    Mathf.Sign(vect.y) * Clamp_set_01(mAx_Min.y,mAx_Max.y, Mathf.Abs(vect.y)));
                    return vect;
                }
        }
        public Vector2 LookingAxis
        {
            get {
                    Vector2 vect = self.inputBasic.Base.Controller_LookAxis.ReadValue<Vector2>();
                    LookingAxisRead = 
                    new Vector2(Mathf.Sign(vect.x) * Clamp_set_01(mAx_Min.x, mAx_Max.x, Mathf.Abs(vect.x)), 
                    Mathf.Sign(vect.y) * Clamp_set_01(mAx_Min.y, mAx_Max.y, Mathf.Abs(vect.y)));
                    return vect;
                }
        }

        float Clamp_set_01(float min, float max, float input)
        {
            float diff = input - min;
            float maxdiff = max - min;
            float ret = Mathf.Clamp01(diff / maxdiff);
            return ret;
        }
        
        public float MainButton
        {
            get {return self.inputBasic.Base.Main_Button.ReadValue<float>(); }
        }
        
        public float ActionButton
        {
            get {return self.inputBasic.Base.Action_Button.ReadValue<float>(); }
        }
        
        public float SubButton
        {
            get {return self.inputBasic.Base.Sub_Button.ReadValue<float>(); }
        }
        public float UtilityButton
        {
            get {return self.inputBasic.Base.Utility_Button.ReadValue<float>(); }
        }
        public float Extra1Button
        {
            get {return self.inputBasic.Base.Extra_1_Button.ReadValue<float>(); }
        }
        public float Extra2Button
        {
            get {return self.inputBasic.Base.Extra_2_Button.ReadValue<float>(); }
        }
        public float MenuButton
        {
            get {return self.inputBasic.Base.Menu_Button.ReadValue<float>(); }
        }
        public float SubMenuButton
        {
            get {return self.inputBasic.Base.SubMenu_Button.ReadValue<float>(); }
        }
        public float DebugCMD_Clear
        {
            get {return self.inputBasic.Base.DebugCMD_ClearAll.ReadValue<float>(); }
        }


        private Vector2 mousepos;

        public Vector2 ScreenMousePos
        {
            get 
            { 
                return self.inputBasic.Base.CursorPosition.ReadValue<Vector2>();                
            }
        }

        public void AnalogButtonSet(float vals, ref int inputNum)
        {                
            if(vals > 0)
            {
                if(inputNum == 0)
                {
                    inputNum = 10;
                }
            }
            else
            {
                if( inputNum == 10)
                {
                    inputNum = 10000;
                }
                else
                {
                    inputNum = 0;
                }
            }
        }
    }
    
    void Awake()
    {
        if (self != null)
        {
            Destroy(self);
        }
        else
        {
            self = this;
        }
        inputBasic = new Input_Basic();
        inputBasic.Enable();
    }

    int StartPressed = 0;
    int SelectPressed = 0;
    int ClearPressed = 0;

    // Update is called once per frame
    void Update()
    {
        inputValues.AnalogButtonSet(inputValues.MainButton, ref inputValues.MainButton_Read);
        inputValues.AnalogButtonSet(inputValues.ActionButton, ref inputValues.ActionButton_Read);
        inputValues.AnalogButtonSet(inputValues.SubButton, ref inputValues.SubButton_Read);
        inputValues.AnalogButtonSet(inputValues.UtilityButton, ref inputValues.UtilityButton_Read);
        inputValues.AnalogButtonSet(inputValues.Extra1Button, ref inputValues.Extra1Button_Read);
        inputValues.AnalogButtonSet(inputValues.Extra2Button, ref inputValues.Extra2Button_Read);
        inputValues.AnalogButtonSet(inputValues.MenuButton, ref inputValues.MenuButton_Read);
        inputValues.AnalogButtonSet(inputValues.SubMenuButton, ref inputValues.SubMenuButton_Read);
        inputValues.AnalogButtonSet(inputValues.DebugCMD_Clear, ref inputValues.DebugCMD_ClearALL_Read);
        Vector2 nullV = inputValues.MovingAxis;
        nullV = inputValues.LookingAxis;
        StartPressed = inputValues.MenuButton_Read > 0 ? StartPressed + 1 : 0;
        SelectPressed = inputValues.SubMenuButton_Read > 0 ? SelectPressed + 1 : 0;
        ClearPressed = inputValues.DebugCMD_ClearALL_Read > 0 ? ClearPressed + 1 : 0;

        if (gameState.self != null && StartPressed == 1)
        {
            gameState.self.TogglePauseMode();
        }
        if (gameState.self != null && SelectPressed == 1)
        {
            gameState.self.ReturnToMainMenu();
        }

        if (ClearPressed == 1)
        {
            gameState.self.ClearChars();
        }
    }
}
