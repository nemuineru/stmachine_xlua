using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    //コマンド記録用.
    public commandRecord[] commandBuffer = new commandRecord[1];

    //手動キャラクター操作用の呼び出しクラス.
    void RecordInput_Player()
    {
        //ニュートラルを基準とする.
        //上下左右の値を考える - 

        int inputs = 5;
        commandRecord buff = new commandRecord();

        buff.MoveAxis = InputInstance.self.inputValues.MovingAxis;
        inputs = InputInstance.self.inputValues.MovingAxis_Digital;

        //以下、MainButtonなどは10の倍数値が最初から足されているため、ボタンインプット値の読み出しの際は
        // inputsを10で割って計算する.
        inputs += (InputInstance.self.inputValues.MainButton_Read) * 0B_00000001;
        inputs += (InputInstance.self.inputValues.ActionButton_Read) * 0B_00000010;
        inputs += (InputInstance.self.inputValues.SubButton_Read) * 0B_00000100;
        inputs += (InputInstance.self.inputValues.UtilityButton_Read) * 0B_00001000;
        inputs += (InputInstance.self.inputValues.Extra1Button_Read) * 0B_00010000;
        inputs += (InputInstance.self.inputValues.Extra2Button_Read) * 0B_00100000;
        inputs += (InputInstance.self.inputValues.MenuButton_Read) * 0B_01000000;
        inputs += (InputInstance.self.inputValues.SubMenuButton_Read) * 0B_10000000;
        buff.inputs = inputs;
        commandBuffer.CopyTo(commandBuffer = new commandRecord[commandBuffer.Length + 1], 0);
        commandBuffer[commandBuffer.Length - 1] = buff;
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
    // 

    void CheckInput()
    {

    }
}
