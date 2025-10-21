using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.UIElements;

[TaskCategory("MyAsset")]
public class enemyInput_Test : Action
{
    //ターゲット先・動作方向先
    [SerializeField]
    SharedVector3 v3_WalkTo;

    [SerializeField]
    SharedGameObject Target;

    //Commandに応じて、入力を与える.
    [SerializeField]
    SharedString Command;

    //これ、仮想で1fと設定する.
    [SerializeField]
    SharedVector2 virtualStickInput;

    [SerializeField]
    SharedFloat DistClamp = 1.0f;



    Vector3 v3_Target = Vector3.zero;

    int currentTick = 0;
    //パスの処理レート設定
    const int mapRouteFindTickRate = 5;
    const float rand_Prov = 0.05f;
    const float hitboxDist = 2f;

    Vector3 Dist = Vector3.zero;
    Vector2 Input = Vector2.zero;

    //動かすEntity. これが付いたExternalBehaviorは必ずこれが付いてるはず
    Entity AIEntity;

    public override void OnAwake()
    {
        if (AIEntity == null)
            AIEntity = gameObject.GetComponent<Entity>();
    }

    //とりあえず指定された位置に近づくだけのスクリプトを組む
    public override void OnStart()
    {
        commandPallette cmdRegisterOneFrame = new commandPallette();
        //スティックを前に(y = 0)
        virtualSticks stc =
        new virtualSticks(virtualStickInput.Value *
        Mathf.Clamp01(DistClamp.Value), 0, 1f);

        cmdRegisterOneFrame.MovAxisVecs.Add(stc);
        Vector3 forwardPos = v3_WalkTo.Value - AIEntity.transform.position;
        //現在視点を基準に...
        cmdRegisterOneFrame.movAxisRemap(AIEntity, forwardPos.normalized);
        cmdRegisterOneFrame.buttonCommands = "y";
        cmdRegisterOneFrame.CommandLength = 0;
        cmdRegisterOneFrame.CommandPriority = 0;
        cmdRegisterOneFrame.isMoveExclusive = true;

        AIEntity.entityInput.cmdPallettes.Add(cmdRegisterOneFrame);

        //Debug.Log("Current Input pall Count" + AIEntity.entityInput.cmdPallettes.Count);
        

        /*
        Vector3 fwRef = Vector3.zero;
        if (v3_WalkTo != null && AIEntity != null)
        {
            fwRef = Vector3.ProjectOnPlane((v3_WalkTo.Value - AIEntity.transform.position), Vector3.up);
            Vector2 xzref = new Vector2(AIEntity.targetTo_fw.x, AIEntity.targetTo_fw.z);
            //Debug.Log(fwRef);
            //currentTickが0なら実行..
            if (currentTick > mapRouteFindTickRate) //&& AIEntity.entityInput.cmdParettes.Count < 1)
            {
                entityInputManager.CMD_Struct str = new entityInputManager.CMD_Struct();
                //xzrefに従い、コマンドを出力する. ..要はどの目線を前向き方向にするか。
                //ここで定義されるのはちょっとまずいので後でFIXしたい
                str.forwardRef = Vector2.up;
                if (virtualStickInput.Length > 0)
                {
                    str.forwardRef = virtualStickInput[0].Value;
                }
                str.currentElapsedFrame = 0;
                entityInputManager.CMDParette CP = new entityInputManager.CMDParette();
                //全体所要時間. これに届くまで指定のコマンドが実行される.
                CP.wholeFrame = 7;
                //stickコマンド. これむっちゃ変.
                //今理解、全体のstickFrameがwholeFrameを下回る際、0として出力される => この値がoverride対象でないなら、そのまま0を受け継いでしまう.
                entityInputManager.CMDParette.stickCMD s_1 =
                new entityInputManager.CMDParette.stickCMD(Vector2.up, .5f, 10);

                //BCOMMAND,SCOMMANDをオーバーライド不可能に設定した場合、
                //最終出力時にこのコマンドが優先された場合実行されるコマンドはこれになる..はず
                CP.BasePriority = 0;
                CP.isMoveSCommandOveridable = false;
                CP.isLookSCommandOveridable = true;
                CP.isBCommandOveridable = true;
                
                CP.sCmds_L.Add(s_1);
                str.parette = CP;

                if (Random.value < rand_Prov)
                {
                    CP.commandInput = Command.Value;
                }

                AIEntity.entityInput.cmdParettes.Add(str);

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

[TaskCategory("MyAsset")]
public class enemyInput_LookTest : Action
{ 
    //ターゲット先・動作方向先
    [SerializeField]
    SharedVector3 v3_WalkTo;

    [SerializeField]
    SharedGameObject Target;

    //Commandに応じて、入力を与える.
    [SerializeField]
    SharedString Command;

    //これ、仮想で1fと設定する.
    [SerializeField]
    SharedVector2[] virtualStickInput;
    //動かすEntity. これが付いたExternalBehaviorは必ずこれが付いてるはず
    Entity AIEntity;

    public override void OnAwake()
    {
        if (AIEntity == null)
            AIEntity = gameObject.GetComponent<Entity>();
    }

    //とりあえず指定された位置の視線を変更するスクリプトを組む
    public override void OnStart()
    {
        
        //y軸周りの角度を取得. 視線方向を考える
        Vector3 forwardPos = v3_WalkTo.Value - AIEntity.transform.position;
        Vector3 solution = Vector3.ProjectOnPlane(forwardPos, Vector3.up).normalized;
        float AngleDiff = Vector3.SignedAngle(AIEntity.targetTo_fw, solution, Vector3.up);

        commandPallette cmdRegisterOneFrame = new commandPallette();
        //スティックを前に(y = 0)
        virtualSticks rstc =
        new virtualSticks(Vector2.right * AngleDiff * 0.01f,0,1);

        cmdRegisterOneFrame.LookAxisVecs.Add(rstc);
        cmdRegisterOneFrame.CommandLength = 0;
        cmdRegisterOneFrame.CommandPriority = -1;
        cmdRegisterOneFrame.isLookExclusive = true;

        AIEntity.entityInput.cmdPallettes.Add(cmdRegisterOneFrame);
        /*
        commandPallette cmdRegisterOneFrame = new commandPallette();
        //スティックを前に(y = 0)
        virtualSticks stc = new virtualSticks(Vector2.down, 0, 0.5f);
        cmdRegisterOneFrame.MovAxisVecs.Add(stc);
        Vector3 forwardPos = v3_Target - AIEntity.transform.position;
        //現在視点を基準に...
        cmdRegisterOneFrame.movAxisRemap(AIEntity, forwardPos);

        AIEntity.entityInput.cmdPallettes.Add(cmdRegisterOneFrame);
        */

        /*
        Vector3 fwRef = Vector3.zero;
        float RotRef = 0f;
        if (v3_WalkTo != null && AIEntity != null)
        {
            fwRef = Vector3.ProjectOnPlane((v3_WalkTo.Value - AIEntity.transform.position), Vector3.up);
            RotRef = Vector3.SignedAngle(AIEntity.targetTo_fw, fwRef,Vector3.up);
            Vector2 xzref = new Vector2(fwRef.x, fwRef.z);
            //それとは別に、視点移動のスクリプトも同様に考える..
            //視点移動なので毎回やる. 上書き可能.
            {
                entityInputManager.CMD_Struct lookCMD = new entityInputManager.CMD_Struct();
                lookCMD.forwardRef = Vector3.up;
                entityInputManager.CMDParette CP = new entityInputManager.CMDParette();
                CP.wholeFrame = 2;
                CP.BasePriority = -1;
                CP.isMoveSCommandOveridable = true;
                CP.isBCommandOveridable = true;
                CP.isLookSCommandOveridable = false;
                Vector2 pos = Vector2.up * RotRef * 0.1f;
                entityInputManager.CMDParette.stickCMD l_1 =
                new entityInputManager.CMDParette.stickCMD(pos, 1f, 1);
                //Debug.Log(pos);
                CP.sCmds_R.Add(l_1);

                lookCMD.parette = CP;
                AIEntity.entityInput.cmdParettes.Add(lookCMD);
            }
        }
        */
    }
}

[TaskCategory("MyAsset")]
public class ExecuteNPCCommands : Action
{

    //動かすEntity. これが付いたExternalBehaviorは必ずこれが付いてるはず
    Entity AIEntity;

    public override void OnAwake()
    {
        if (AIEntity == null)
            AIEntity = gameObject.GetComponent<Entity>();
    }

    public override void OnStart()
    {
        //Debug.Log("Execution Inputs");
        AIEntity.entityInput.Execute_Entity_NPC(false);
    }
}

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject
{
    [TaskCategory("Unity/GameObject")]
    [TaskDescription("Finds a GameObject by tag only on Awake. Returns success if an object is found. this method is Expanded by nem")]
    public class FindWithTag_OnAwake : Action
    {
        [Tooltip("The tag of the GameObject to find")]
        public SharedString tag;
        [Tooltip("Should a random GameObject be found?")]
        public SharedBool random;
        [Tooltip("The object found by name")]
        [RequiredField]
        public SharedGameObject storeValue;

        public override void OnAwake()
        {
            if (random.Value)
            {
                var gameObjects = GameObject.FindGameObjectsWithTag(tag.Value);
                if (gameObjects == null || gameObjects.Length == 0) { return; }
                storeValue.Value = gameObjects[Random.Range(0, gameObjects.Length)];
            }
            else
            {
                storeValue.Value = GameObject.FindWithTag(tag.Value);
            }
            return;
        }

        public override void OnReset()
        {
            tag.Value = null;
            storeValue.Value = null;
        }
    }
}