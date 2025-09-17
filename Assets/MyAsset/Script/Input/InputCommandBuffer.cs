using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InputCommandBuffer : MonoBehaviour
{    
    public static InputCommandBuffer self;
    public struct commandRecord
    {
        /*
            コマンドインプットの仕様.
            1桁目に移動キーの入力(テンキーと対応)
            2桁目以降にボタンの入力を設定(0で入力無し. 2の乗数で管理する)
            10(1) - メイン キー
            20(2) - アクション キー
            40(3) - サブ1 キー
            80(4) - サブ2 キー
            160(5) - エクストラ1 キー
            320(6) - エクストラ2 キー
            640(7) - メニュー キー
            1280(8) - サブメニュー キー 
            キーが離された際は10^4の位の値を渡す.           
        

            なお、移動スティックの方向は毎フレームごとに別で記録される.;
        */



        public int inputs;
        public Vector2 MoveAxis;
    }    

    public commandRecord[] commandBuffer = new commandRecord[1];

    //60フレームごとに計算. そんなに多くなくて良いかも。
    float recordframes = 1f / 60f;

    //ゲーム開始してからの時間. rectimeは記録してからどれぐらい経ったかを表す.
    float startTime = 0f;
    float rectime = 0f;


    
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
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        startTime += Time.deltaTime;
        RecordInput();
        for(int i = 1;i < 11; i++)
        {
            string DrawDebug = "";
            int recentCMD = commandBuffer.Length - i;
            if(recentCMD > 0)
            {
                DrawDebug += 
                //string.Format("{0} : {1}",0,commandBuffer[recentCMD].inputs);
                AnalysisInput(commandBuffer[recentCMD].inputs);
            }
            //Debug.Log(DrawDebug);
        }
    }




    //コマンドのログを記録する.
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();
    

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }
    
    void HandleLog(string logString, string stackTrace, LogType type) {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

/*
    void OnGUI() {
        GUI.color = Color.magenta;
        GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
        GUILayout.EndArea();
    }
*/

    void RecordInput()
    {
        //ニュートラルを基準とする.
        //上下左右の値を考える - 
        
        int inputs = 5;
        commandRecord buff = new commandRecord();

        buff.MoveAxis = InputInstance.self.inputValues.MovingAxis;
        inputs = InputInstance.self.inputValues.MovingAxis_Digital;

        inputs += (InputInstance.self.inputValues.MainButton_Read) * 0B_00000001;
        inputs += (InputInstance.self.inputValues.ActionButton_Read) * 0B_00000010;
        inputs += (InputInstance.self.inputValues.SubButton_Read) *  0B_00000100;
        inputs += (InputInstance.self.inputValues.UtilityButton_Read) * 0B_00001000;
        inputs += (InputInstance.self.inputValues.Extra1Button_Read) * 0B_00010000;
        inputs += (InputInstance.self.inputValues.Extra2Button_Read) * 0B_00100000;
        inputs += (InputInstance.self.inputValues.MenuButton_Read) * 0B_01000000;
        inputs += (InputInstance.self.inputValues.SubMenuButton_Read) * 0B_10000000;
        buff.inputs = inputs;
        commandBuffer.CopyTo(commandBuffer = new commandRecord[commandBuffer.Length + 1], 0);
        commandBuffer[commandBuffer.Length - 1] = buff;
    }

    string AnalysisInput(int inputRaw)
    {
        string ret = "";
        strctInputs[] anlInputs = new strctInputs[8];
        //2進数のボタン比較用.
        anlInputs[0] = new strctInputs(0B_00000001,"a");
        anlInputs[1] = new strctInputs(0B_00000010,"b");
        anlInputs[2] = new strctInputs(0B_00000100,"x");
        anlInputs[3] = new strctInputs(0B_00001000,"y");
        anlInputs[4] = new strctInputs(0B_00010000,"c");
        anlInputs[5] = new strctInputs(0B_00100000,"z");
        anlInputs[6] = new strctInputs(0B_01000000,"st");
        anlInputs[7] = new strctInputs(0B_10000000,"sl");

        //if文で全部管理.
        int Stick = inputRaw % 10;
        int buttonInput_Push = (inputRaw - Stick) % 10000;
        int buttonInput_Release = ((inputRaw - Stick) - buttonInput_Push) / 10000;

        //switch文でやるしかねぇ.
        switch(Stick)
        {
            case 1:
            {
                ret += "↙";
                break;
            }
            case 2:
            {
                ret += "↓";
                break;
            }
            case 3:
            {
                ret += "↘";
                break;
            }
            case 4:
            {
                ret += "←";
                break;
            }
            case 5:
            {
                ret += "◯";
                break;
            }
            case 6:
            {
                ret += "→";
                break;
            }
            case 7:
            {
                ret += "↖";
                break;
            }
            case 8:
            {
                ret += "↑";
                break;
            }
            case 9:
            {
                ret += "↗";
                break;
            }
            default : 
            break;
        }
        
        ret += " : ";

        foreach(strctInputs al in anlInputs)
        {
            if((al.bitNum & buttonInput_Push / 10) != 0)
            {
                ret += al.drawStr + "_ ";
            }
            else if ((al.bitNum & buttonInput_Release) != 0)
            {
                ret += al.drawStr + "^ ";
            }
        }

        return ret;
    }    
    
    struct strctInputs
    {
        public strctInputs(int bitSet, string strSet){ bitNum = bitSet; drawStr = strSet; }
        
        public int bitNum {get; set;}
        public string drawStr {get; set;}
    }
}
