using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using UnityEngine;
using UnityEngine.Rendering;

//entityの持つInput管理用クラス.
//

public class entityInputManager
{
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
        public Vector2 LookAxis;
    }

    //コマンド記録用.
    public commandRecord[] commandBuffer = new commandRecord[1];
    //60個まで登録.
    int commandBuffer_max = 60;

    //手動キャラクター操作用の呼び出しクラス.
    //これ、接続したコントローラー別に読み出したいけど時間かかりそうなのでやめる.
    public int Execute_Entity_Player(int controllerID)
    {
        //ニュートラルを基準とする.
        //上下左右の値を考える - 

        int inputs = 5;
        commandRecord rec = new commandRecord();

        if (InputInstance.self != null)
        {
            rec.MoveAxis = InputInstance.self.inputValues.MovingAxis;
            rec.LookAxis = InputInstance.self.inputValues.LookingAxis;
            //rec.LookAxis = InputInstance.self.inputValues.Ax;
            inputs = InputInstance.self.inputValues.MovingAxis_Digital;

            //以下、MainButtonなどは10の倍数値が最初から足されているため、ボタンインプット値の読み出しの際は
            // inputsを10で割って計算する.
            inputs += (InputInstance.self.inputValues.MainButton_Read) * 0B_00000001; //a
            inputs += (InputInstance.self.inputValues.ActionButton_Read) * 0B_00000010; //b
            inputs += (InputInstance.self.inputValues.SubButton_Read) * 0B_00000100; //x
            inputs += (InputInstance.self.inputValues.UtilityButton_Read) * 0B_00001000; //y
            inputs += (InputInstance.self.inputValues.Extra1Button_Read) * 0B_00010000; //c
            inputs += (InputInstance.self.inputValues.Extra2Button_Read) * 0B_00100000; //z
            inputs += (InputInstance.self.inputValues.MenuButton_Read) * 0B_01000000; //start
            inputs += (InputInstance.self.inputValues.SubMenuButton_Read) * 0B_10000000; //menu
            rec.inputs = inputs;
            RecordInput_Core(rec);
        }
        return inputs;
    }

    //敵AI版. さて、どうやって判別させようか..
    //Buttonsの一番最後の文を読み込ませ, そのドベから
    //二番目以降のボタンに応じて入力するボタンを返す..みたいな感じ..
    //多分こうすれば押しっぱなしとかの判別が可能?

    //CommandParetteから読み出す.
    //forwardに設定した値を元に, stickの傾きを設定 - 
    //stick y軸が前方・後方方向とし、x軸は横軸方向とする.
    public void Execute_Entity_NPC(bool isPaused)
    {
        int inputs = 0;
        Vector2 lStick = Vector2.zero;
        Vector2 rStick = Vector2.zero;
        commandRecord rec = new commandRecord();

        if (cmdParettes.Count > 0)
        {
            //cmdParettes内に登録されたコマンドの経過時間を設定
            for (int i = 0; i < cmdParettes.Count; i++)
            {
                //待機を無視するか？
                if (!isPaused || !cmdParettes[i].parette.isPauseWait)
                    cmdParettes[i].currentElapsedFrame++;
            }
            //Debug.Log("cmdParette Count " + cmdParettes.Count);

            //Priority順に並べる
            cmdParettes.Sort((x, y) => y.parette.BasePriority - x.parette.BasePriority);
            bool isMoveStickOverride = true;
            bool isLookStickOverride = true;
            bool isButtonOverride = true;

            //現状、cmdParettesの値読み出し時
            for (int i = 0; i < cmdParettes.Count; i++)
            {
                //Debug.Log("stickoverride " + isMoveStickOverride);
                //最優先位置のコマンドを読み出す.
                CMD_Struct sel = cmdParettes[i];
                //Debug.Log("SOverride" + isStickOverride + " in index " + i);
                //Debug.Log("BOverride" + isButtonOverride + " in index " + i);
                (int inputs_calc, Vector2 lStick_calc, Vector2 rStick_calc) = sel.GetCommands(commandBuffer[0].MoveAxis, ref isButtonOverride,
                ref isMoveStickOverride, ref isLookStickOverride, lStick, rStick, inputs);
                inputs = inputs_calc;
                lStick = lStick_calc;
                rStick = rStick_calc;
                //Debug.Log(inputs.ToString() + " " + lStick.ToString());
            }
            //lStick = lStick.normalized * Mathf.Clamp01(lStick.sqrMagnitude);

            cmdParettes.RemoveAll(cd => cd.currentElapsedFrame > cd.parette.wholeFrame);
        }
        else
        {
            //MAGIC NUMBER!!!
            Debug.Log("MAGIC NUMBER LOADED");
            lStick = Vector2.Lerp(commandBuffer[0].MoveAxis, Vector2.zero, 0.8f);
            rStick = Vector2.zero;
        }

        rec.MoveAxis = lStick;
        rec.LookAxis = rStick;
        //int X_I = InputInstance.GetDigitalAxis(new(ForwardInput.x, refForwardInput.z));
        //inputs += X_I; //+ RawInput;



        rec.inputs = inputs;
        RecordInput_Core(rec);
    }

    public List<CMD_Struct> cmdParettes = new List<CMD_Struct>();

    //それぞれのコマンドをいわゆるTCGのカードみたいにする - 
    //CMDParetteの内容は変更しない.
    public class CMD_Struct
    {
        //現在経過時間. これはBehaviorDesignerでは変動させない.
        internal int currentElapsedFrame;
        //スティックの傾き時の基準方向 0ならWorld.Forward方向. 初期化時、代入
        internal Vector3 forwardRef;

        internal CMDParette parette = new CMDParette();

        //CMDparetteから指定したコマンドを取得.
        //今はボタンインプットと左スティックの移動のみ.
        internal (int, Vector2, Vector2) GetCommands(Vector2 B_Input, ref bool isBCommandOveridable,
        ref bool isMoveSCommandOveridable, ref bool isLookSCommandOveridable,   
            Vector2 lStick_f ,Vector2 rStick_f , int inputs_f)
        {
            //Debug.Log(parette.commandInput);
            Vector2 lStick = lStick_f;
            Vector2 rStick = rStick_f;
            int inputs = inputs_f;
            //移動スティックはforwardRefを考えるとする
            if (isMoveSCommandOveridable)
            {
                //前のコマンドとの比較. Lerp値はparette内で決定される.
                lStick += parette.findStickVel(forwardRef, B_Input, 'l', currentElapsedFrame);
                //Debug.Log(lStick.ToString() + " eFrame at " + currentElapsedFrame);
                isMoveSCommandOveridable = parette.isMoveSCommandOveridable;
            }

            //視点スティックはforwardRefを考えず, Vector.up(ワールド基準)で考える
            if (isLookSCommandOveridable)
            {
                //前のコマンドとの比較. Lerp値はparette内で決定される.
                if(parette.sCmds_R.Count > 0)
                rStick += parette.findStickVel(Vector2.right, B_Input, 'r', 0);
                //Debug.Log(rStick.ToString() + " eFrame at " + currentElapsedFrame);
                isLookSCommandOveridable = parette.isLookSCommandOveridable;
            }

            //ボタンインプット.. ','で区切る
            if (isBCommandOveridable && parette.commandInput != null)
            {
                isBCommandOveridable = parette.isBCommandOveridable;


                string[] commands = parette.commandInput.Split(',');
                //最低値のindexを取る
                int mIndex = Mathf.Min(currentElapsedFrame, commands.Length - 1);
                string cmd_strs = commands[mIndex];

                if (cmd_strs != null)
                {
                    cmd_strs.Trim();
                    //analysys the structInputs
                    foreach (structInputs stInput in anlInputs)
                    {
                        if (cmd_strs.Contains(stInput.drawStr))
                        {
                            //押したときはstInputの10の倍数を与える
                            inputs += stInput.bitNum * 10;
                        }
                    }
                }
            }
            return (inputs, lStick, rStick);
        }
    }

    //CMDParette - この配列通りに順繰り実行.
    //CMDParetteはすべて定数的に扱う.
    public class CMDParette
    {
        // コマンド全体のかかる時間
        internal int wholeFrame = 1;

        //Index事に読み込み - 
        internal List<stickCMD> sCmds_L = new List<stickCMD>(), sCmds_R = new List<stickCMD>();
        //Listとして読み出す
        internal struct stickCMD
        {
            //仮想スティックの登録
            internal Vector2 stickPos;
            //Stickの前ValueとのLerp値
            internal float lerpValue;
            //持続フレーム
            internal int frame;
            public stickCMD(Vector2 sPos, float lVal, int fr) {
                stickPos = sPos;
                lerpValue = lVal;
                frame = fr;
            }
        }

        //コマンドと言うかスティック傾きの検知っすねー
        internal Vector2 findStickVel(Vector2 fw_ref, Vector2 B_stickCMD, char LorR, int ElapsedFrame)
        {
            //返り値
            Vector2 v = Vector3.zero;

            //右・左のどっちのスティックを読み出す？
            List<stickCMD> selCMD = sCmds_L;
            if (LorR == 'l')
            {
                selCMD = sCmds_L;
            }
            else if (LorR == 'r')
            {
                selCMD = sCmds_R;
            }

            //経過フレームとの比較
            int fr_all = 0;
            foreach (stickCMD c_a in selCMD)
            {
                fr_all += c_a.frame;
                //Debug.Log(ElapsedFrame + " / " + fr_all + " vc " + c_a.stickPos);
                //実行コマンド時間が経過時間を超えたとき
                //forwardRefの値に従い、値の回転を行う.
                if (ElapsedFrame <= fr_all)
                {
                    Vector2 fw;
                    if (fw_ref != Vector2.zero)
                    {
                        fw = fw_ref.normalized;
                        //Debug.Log(fw);
                    }
                    else
                    {
                        fw = Vector2.up;
                    }
                    Vector2 stPos = c_a.stickPos;
                    //90方向の横 + 前方方向
                    stPos = fw * stPos.y +  -Vector2.Perpendicular(fw) * stPos.x;
                    v = Vector2.Lerp(B_stickCMD, stPos, c_a.lerpValue);

                    //Debug.Log(v + "Outputted");

                    return v;
                }
            }
            return v;
        }

        // /ボタンのインプット. ","でフレームごとに入力する.
        //"a,a,a,a, ,b"なら 4フレーム分 a押し込んで離した後 bを押す.
        internal string commandInput;
        // それぞれ - 
        // スティック情報を元にコマンドを入力可能か / ボタン情報を元にコマンドを入力可能か / ポーズ時間を待つか
        internal bool isMoveSCommandOveridable, isLookSCommandOveridable, isBCommandOveridable, isPauseWait;

        //コマンド優先度 - 高いほどそれが基礎として読み込まれる
        internal int BasePriority;
    }



    //behaviorDesignerに読み込ませるためのインプット
    public void RecordInput_AI()
    {

    }

    void RecordInput_Core(commandRecord b)
    {
        //一番古いコマンドが最後尾になるため、消し飛ばす
        if (commandBuffer.Length > commandBuffer_max)
        {
            commandRecord[] coms = commandBuffer;
            Array.Copy(coms, 0, commandBuffer, 1, commandBuffer_max - 1);
        }
        else
        {
            commandBuffer.CopyTo(commandBuffer = new commandRecord[commandBuffer.Length + 1], 0);
        }
        //一番最初にくっつける
        commandBuffer[0] = b;
    }

    //commandRecordのinputsは,下記に考慮される - 
    // 0000|0000|0
    // 2桁目は5桁目以降はボタンの離した時の値、

    //コマンド読み出し..
    // 下記のコマンド読み出しとして..
    // "A, B..等　単体"
    // InputValuesに 指定の入力値が有るか?
    // "A_, B_ 等 アンダーバーが付いているとき"
    // InputValueの入力に0の後に1が有るものを探す.
    // "A^ .. B^.. ハット記号"
    // A, Bを離したとき.. 10000以降を参照し 値を調査
    // コンマ(,)でコマンドごとを分ける.



    //2進数のボタン比較用.  
    static structInputs[] anlInputs = {
        new structInputs(0B_00000001, "a"),
        new structInputs(0B_00000010, "b"),
        new structInputs(0B_00000100, "c"),
        new structInputs(0B_00010000, "x"),
        new structInputs(0B_00001000, "y"),
        new structInputs(0B_00100000, "z"),
        new structInputs(0B_01000000, "s"),
        new structInputs(0B_10000000, "o")
        };

    public bool CheckInput(string command, int buffer)
    {
        bool result = false;
        string[] commands = command.Split(',');
        for (int i = 0; i < commands.Length; i++)
        {
            //前後及び余分な空白を消す.
            commands[i] = commands[i].Trim().Replace(" ", "");
        }

        //最新のコマンドバッファ値.
        int commandBuffer_max = commandBuffer.Length;


        //コンマで区切られたコマンド値の読み出し..
        //buffer値に基づき、commandBufferの配列を読み出す..
        //(~!)xxab(^_)
        //という感じで.
        //小文字と大文字は分ける.

        //時間かかるから先ずはbutton + conditionの一部だけ使う.
        foreach (string sCom in commands)
        {
            string conditions = Regex.Match(sCom, @"[~_^]").Value;
            string seconds = Regex.Match(sCom, @"[\d]").Value;
            string button = Regex.Match(sCom, @"[a-z]").Value;
            string stick = Regex.Match(sCom, @"[A-Z]").Value;

            structInputs checker = anlInputs[7];


            switch (button[0])
            {
                case 'a':
                    {
                        checker = anlInputs[0];
                        break;
                    }
                case 'b':
                    {
                        checker = anlInputs[1];
                        break;
                    }
                case 'c':
                    {
                        checker = anlInputs[4];
                        break;
                    }
                case 'x':
                    {
                        checker = anlInputs[2];
                        break;
                    }
                case 'y':
                    {
                        checker = anlInputs[3];
                        break;
                    }
                case 'z':
                    {
                        checker = anlInputs[5];
                        break;
                    }
            }

            int b_rn, b_bf;

            b_rn = commandBuffer[0].inputs;
            b_bf = b_rn;
            if (commandBuffer_max > 1)
            {
                b_bf = commandBuffer[1].inputs;
            }
            //Debug.Log(b_rn + " " + b_bf);


            //conditionの最初の文問のみ..
            if (conditions.Length != 0)
                switch (conditions[0])
                {
                    //押した瞬間を確認
                    //2フレーム以上必要.
                    case '_':
                        {
                            //Debug.Log("cmd check - pressPulse");
                            if (ButtonCheck(b_rn, checker.bitNum) == '+' && commandBuffer_max > 1 &&
                            (ButtonCheck(b_bf, checker.bitNum) == '.' || ButtonCheck(b_bf, checker.bitNum) == '-'))
                            {
                                result = true;
                            }
                            break;
                        }
                    //離された瞬間を確認
                    //2フレーム以上必要.
                    case '^':
                        {
                            if ((ButtonCheck(b_rn, checker.bitNum) == '.' || ButtonCheck(b_rn, checker.bitNum) == '-') && commandBuffer_max > 1 &&
                            ButtonCheck(b_bf, checker.bitNum) == '+')
                            {
                                result = true;
                            }
                            break;
                        }
                    //defaultは入力値のみを見るとして..
                    //1フレのみを計測
                    default:
                        {
                            if (ButtonCheck(b_rn, checker.bitNum) == '+')
                            {
                                result = true;
                            }
                            break;
                        }
                }
            else
            {
                if (ButtonCheck(b_rn, checker.bitNum) == '+')
                {
                    result = true;
                }
            }

        }
        return result;
    }

    //選択した
    char ButtonCheck(int button, int bitNum)
    {
        int Stick = button % 10;
        int buttonInput_Push = (button - Stick) % 10000;
        int buttonInput_Release = ((button - Stick) - buttonInput_Push) / 10000;

        if ((bitNum & buttonInput_Push / 10) != 0)
        {
            return '+';
        }
        if ((bitNum & buttonInput_Release) != 0)
        {
            return '-';
        }
        return '.';
    }

    struct structInputs
    {
        public structInputs(int bitSet, string strSet) { bitNum = bitSet; drawStr = strSet; }

        public int bitNum { get; set; }
        public string drawStr { get; set; }
    }
    
}

