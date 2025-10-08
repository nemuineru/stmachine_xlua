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

        //ランダム値がこれを超えない際..
        [SerializeField]
        SharedFloat Freqently;

        [SerializeField]
        SharedInt Priority = 0;


        const int mapRouteFindTickRate = 5;

        //Commandに応じて、入力を与える.
        [SerializeField]
        SharedString Command;

        //動かすEntity. これが付いたExternalBehaviorは必ずこれが付いてるはず
        Entity AIEntity;

        const float AttackRange = 1.2f;

        public override void OnAwake()
        {
            if (AIEntity == null)
                AIEntity = gameObject.GetComponent<Entity>();
        }

        int currentTick = 0;

        //指定したコマンドを押すだけのスクリプト.
        public override void OnStart()
        {
            if (Random.value < Freqently.Value)
            {                
                commandPallette cmdRegisterOneFrame = new commandPallette();
                cmdRegisterOneFrame.buttonCommands = Command.Value;
                cmdRegisterOneFrame.CommandLength = 0;
                cmdRegisterOneFrame.CommandPriority = Priority.Value;
                cmdRegisterOneFrame.isButtonCommandExclusive = true;

                AIEntity.entityInput.cmdPallettes.Add(cmdRegisterOneFrame);
            }
            /*
                //ターゲット先/エンティティが存在するなら
                if (v3_WalkTo != null && AIEntity != null)
                {
                    if (currentTick >= mapRouteFindTickRate)
                    {
                        var fwRef = Vector3.ProjectOnPlane((v3_WalkTo.Value - AIEntity.transform.position), Vector3.up);
                        Vector2 curPos = new Vector2(transform.position.x, transform.position.z);

                        //攻撃検知範囲に居るならコマンドを押す.
                        if (Random.value < Freqently.Value)
                        {
                            entityInputManager.CMD_Struct InputStruct = new entityInputManager.CMD_Struct();
                            InputStruct.forwardRef = Vector2.up;
                            InputStruct.currentElapsedFrame = 0;
                            //一瞬だけ押すので2フレーム分.
                            entityInputManager.CMDParette CP = new entityInputManager.CMDParette();
                            CP.wholeFrame = 5;
                            CP.BasePriority = 0;
                            CP.isMoveSCommandOveridable = true;
                            CP.isLookSCommandOveridable = true;
                            CP.isBCommandOveridable = false;

                            CP.commandInput = Command.GetValue().ToString();

                            InputStruct.parette = CP;

                            AIEntity.entityInput.cmdParettes.Add(InputStruct);
                            //Debug.Log(Command.GetValue().ToString());
                        }
                        currentTick = 0;
                    }
                    else
                    {
                        currentTick++;
                    }
                }
                */
            }
    }
}