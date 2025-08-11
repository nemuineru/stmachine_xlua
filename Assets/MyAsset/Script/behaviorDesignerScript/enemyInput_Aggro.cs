using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace enemyInput_Aggro
{
    [TaskCategory("MyAsset")]
    public class enemyCommand_Attack : Action
    {
        //ターゲット先・動作方向先
        [SerializeField]
        SharedVector3 v3_WalkTo;

        [SerializeField]
        SharedGameObject Target;
        
    //これ、仮想で1fと設定する.
        [SerializeField]
        SharedVector2[] virtualStickInput;


        //Commandに応じて、入力を与える.
        [SerializeField]
        SharedString Command;

        //動かすEntity. これが付いたExternalBehaviorは必ずこれが付いてるはず
        Entity AIEntity;

        //ランダム値がこれを超えない際..
        const float rand_Prov = 0.05f;

        const float AttackRange = 1.2f;

        public override void OnAwake()
        {
            if (AIEntity == null)
                AIEntity = gameObject.GetComponent<Entity>();
        }


        //指定したコマンドを押すだけのスクリプト.
        public override void OnStart()
        {
            //ターゲット先/エンティティが存在するなら
            if (v3_WalkTo != null && AIEntity != null)
            {
                Vector2 curPos = new Vector2(transform.position.x,transform.position.z);
                Vector2 xzref = new Vector2(AIEntity.targetTo_fw.x, AIEntity.targetTo_fw.z);
        
                //攻撃検知範囲に居るなら"x"コマンドを押す.
                if (Random.value < rand_Prov && AttackRange < (curPos - xzref).magnitude)
                {
                    entityInputManager.CMD_Struct InputStruct = new entityInputManager.CMD_Struct();
                    InputStruct.forwardRef = Vector2.up;
                    InputStruct.currentElapsedFrame = 0;
                    //一瞬だけ押すので2フレーム分.
                    entityInputManager.CMDParette CP = new entityInputManager.CMDParette();
                    CP.wholeFrame = 2;
                    CP.commandInput = "x,";

                    AIEntity.entityInput.cmdParettes.Add(InputStruct);
                }
            }
        }
    }
}