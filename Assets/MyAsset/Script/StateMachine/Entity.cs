using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
using System.ComponentModel;
using UnityEditor;
using DG.Tweening;
using System;
using BehaviorDesigner.Runtime;

public class Entity : MonoBehaviour
{
    //statetype : 体勢の設定. 
    public enum _StateType
    {
        S, //Standing, 立ち状態
        A, //Air, 空中
        L //Lying, 
    }

    public enum _PhysicsType
    {
        S, //Standing, 地上の動き
        A, //Air, 空中. 
        N //None, 定義なし(摩擦効果を受けない.)
    }

    public enum _MoveType
    {
        I, //Idle, 設定なし
        A, //Attack, 攻撃中
        H  //Hit, やられ
    }
    public _StateType stateType;
    public _PhysicsType physicsType;
    public _MoveType moveType;

    public class EntityInfo
    {
        public string name;
        public int ID;
    }


    public Rigidbody rigid;
    public AudioSource selfSource;

    public int stateTime;

    //アニメのフレーム時間.
    public float animationFrameTime, animationEndTime;

    //移動用の設定など. fwに設定した値・90度回転方向を考慮 - 
    public Vector3 targetTo_fw = Vector3.forward;

    public CinemachineVirtualCamera vCam;

    //ステートID.
    public int CurrentStateID = 0;

    //clssが設定されていない時のプレイヤー当たり判定設定
    public clssSetting defaultClss;

    internal Transform[] allChildTransforms;

    //status for UIs and more

    [SerializeField]
    public EntityStatus status;

    //Attr for GameSystems and more

    [SerializeField]
    public EntityAttr attrs;


    //親のEntity. Parent化されているならこのEntityの内容を読み出しておく.
    [SerializeField]
    public Entity parentEntity;

    //コントロール影響下のEntitiy. Selfstateが発生しない限りこの内容を読み出す.
    [SerializeField]
    public Entity controlledEntity;


    //アニメーション管理用.
    public int animID = 0;
    [SerializeField]
    List<AnimlistObject> animListObject;

    //List化されたanimListObjectからanimDefsのリストを作成する.
    //他Entityが自分のAnimを参照する際、パラメータ変更を共有してしまうことを懸念.

    internal List<AnimDef> animDefs = new List<AnimDef>();

    //アニメーターベース.
    Animator animator;

    //PlayableOutputとして主力となるものを選択.
    PlayableOutput PrimalPlayableOut;

    [SerializeField]
    public MainNodeConfigurator MainAnimMixer = new MainNodeConfigurator();

    public Color CurColor;

    //地上判定.
    public bool isOnGround;

    //ステート直後のステート時間を0にするため、追加.
    public bool isStateChanged = false;

    //メインマテリアルとメッシュ.
    Material mat;
    public List<Renderer> renderers;

    //stateDefListObjの本元.

    public List<StateDefListObject> DefLists;


    public Vector3 wishingVect;

    //この秒数が0にならない限り動きを止める. stateTimeなども同様.
    internal float HitPauseTime;

    //Input Manager for each entitys
    internal entityInputManager entityInput = new entityInputManager();

    //The Brain of character. not muscle or vertebrae. using BehaviorDesigner to do.
    [SerializeField]
    ExternalBehavior LoadedBehavior;
    BehaviorTree BTree;

    //CapsuleColliderの格納. キャラクターの地形との判定.
    CapsuleCollider capCol;

    //カメラ登録時、格納.
    CinemachineOrbitalTransposer transposer;
    // Start is called before the first frame update
    void Awake()
    {
        allChildTransforms = GetComponentsInChildren<Transform>(true);
        renderers = GetComponentsInChildren<Renderer>(true).ToList();
        mat = renderers[0].material;
        animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        capCol = GetComponent<CapsuleCollider>();
        //DefList = (StateDefListObject)StateDefListObject.CreateInstance(typeof(StateDefListObject));
        //DefSet();

        //StateDefはDeepCopyしないように.

        //DefList.stateDefs.ForEach(def => def.PriorCondition = def.PriorCondition);

        //アニメ設定.
        PrimalPlayableOut = new PlayableOutput();
        foreach (AnimlistObject f in animListObject)
        {
            animDefs.AddRange(f.animDef.ToList());
        }
        if (animListObject != null && animator != null)
        {
            MainAnimMixer.SetupGraph(ref animator, ref PrimalPlayableOut);
            PrimalPlayableOut.SetSourcePlayable(MainAnimMixer.mixMixer);
            ChangeAnim();
        }
        defaultClss.initClss(this);
        foreach (StateDefListObject dObj in DefLists)
        {
            foreach (StateDef state in dObj.stateDefs)
            {
                loadedDefs.Add(state.Clone());
            }
        }

        //initialize behaviors.
        if (LoadedBehavior != null)
        {
            BTree = gameObject.AddComponent<BehaviorTree>();
            BTree.ExternalBehavior = LoadedBehavior;
        }

        if (vCam != null)
        {
            transposer = vCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        }
        rigid.useGravity = false;
        selfSource = GetComponent<AudioSource>();
    }

    internal List<StateDef> loadedDefs = new List<StateDef>();

    string verd_1;
    [SerializeField]
    Vector3 raycenter = Vector3.down * 0.5f;

    Vector3 pausedVel = Vector3.zero;

    //Awake後に設定される. UIとか指定したい.
    void Start()
    {
        //プレイヤーとか指定されたタグに設定されていないなら、HP用のUI表示.
        if (gameObject.tag != "Player" && gameState.self.HPUI != null)
        {
            StatusBar_Minimal min = Instantiate(gameState.self.HPUI).GetComponent<StatusBar_Minimal>();
            min.SetEntity(this);
            min.transform.parent = transform;
            min.transform.localPosition = Vector3.up;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        status.setCurrentValue();
        defaultClss.clssPosUpdate();

        //後で消します. Player用にInputを記録する..
        //これはBehaviorDesignerに登録させる予定.
        //entityInput.RecordInput_Player(0);
        //play BehaviorDesigner.
        //isStateChangedはここで変更される. currentStateでstateChangeが発生した際,　リセットをかけるため.
        isStateChanged = false;
        entityPhisics();
        setanimPlay();
        setViewCams();
        mat.SetColor("_Color", CurColor);

        executeStates();

        capsuleRaydraw();

        HitPauseTime -= 1.0f;

        //これかぁ.. HitPauseTimeが設定されていてもそのまま続行 - HitPause分も引き継ぐ.
        stateTime = isStateChanged ? 0 : HitPauseTime >= 0 ? stateTime : stateTime + 1;
        //ステートが変更されなければそのまま.
        if (isStateChanged)
        {
            attrs.resetCombatStateTime();
        }
        else
        {
            attrs.addCombatStateTime();
        }
        statusAlign();
    }

    void statusAlign()
    {
        status.currentHP = Mathf.Clamp(status.currentHP , 0 , status.maxHP);
        status.currentEnergy = Mathf.Clamp(status.currentEnergy , 0 , status.maxEnergy);
    }

    //地面判定.
    void capsuleRaydraw()
    {
        //raycenter
        //Ray ray = new Ray(transform.position + Vector3.up * 0.009f, Vector3.down);
        //Debug.DrawRay(ray.origin, Mathf.Max(0, -rigid.velocity.y) * Vector3.down);

        RaycastHit hitInfo;

        //Physics.Raycast(ray, out hitInfo, Mathf.Max(0.01f, -rigid.velocity.y * Time.fixedDeltaTime), LayerMask.GetMask("Terrain"));

        //get Capsule Normals
        Vector3 norm =
            new Vector3
            (capCol.direction == 0 ? 1 : 0,
            capCol.direction == 1 ? 1 : 0,
            capCol.direction == 2 ? 1 : 0);

        //カプセル一端の組み合わせ
        Vector3 pos_1 =
            transform.position + Vector3.up * 0.001f + transform.rotation * (capCol.center + norm * (capCol.height / 2f - capCol.radius));
        Vector3 pos_2 =
            transform.position + Vector3.up * 0.001f + transform.rotation * (capCol.center - norm * (capCol.height / 2f - capCol.radius));

        //カプセルレイ.
        bool isCapsuleHit =
            Physics.CapsuleCast
            (pos_1, pos_2,
            capCol.radius, Vector3.down,
            out hitInfo, Mathf.Max(0.005f, -rigid.velocity.y * Time.fixedDeltaTime), LayerMask.GetMask("Terrain"));

        //Debug.Log(isCapsuleHit + " - capsuleSet?");

        //キャラと地形の当たり判定表示
        //clssDef.DrawCapsuleGizmo_Tool(pos_1,pos_2,capCol.radius,Color.cyan);


        isOnGround = (hitInfo.collider != null);
        setDestroy();
    }

    //ステータスの変更作業など
    void setStatusAlign()
    {
        if (status.currentHP < 0)
        {
            //stateChange
            isStateChanged = true;

            //5300をKOステートに変更.
            CurrentStateID = 5300;
            stateTime = 0;
        }
    }

    void executeStates()
    {

        if (CListQueue.Count > 0)
        {
            //Most Primal Queue is Most Biggest Number.
            CListQueue.Sort((CQ_L, CQ_M) => CQ_M.priority - CQ_L.priority);
            CurrentStateID = CListQueue[0].stateDefID;
            attrs.resetCombatStateTime();
            CListQueue.Clear();
        }

        //常時実行StateDef(-1, -2, -3)

        //-2はステート奪取されていても実行.
        StateDef AutoState_2 =
        loadedDefs.Find(stDef => stDef.StateDefID == -2);
        if (AutoState_2 != null)
        {
            //Debug.Log("auto checking -1 state");
            AutoState_2.Execute(this);
        }

        //-1はステート奪取されているなら実行しない.
        StateDef AutoState_1 =
        loadedDefs.Find(stDef => stDef.StateDefID == -1);
        if (AutoState_1 != null && controlledEntity == null)
        {
            //Debug.Log("auto checking -1 state");
            AutoState_1.Execute(this);
        }



        if (status.currentHP <= 0)
        {
            if (attrs.alive == true)
            {
                //倒したときの一瞬スローモー.
                StartCoroutine(gameState.self.OneShotSlo_mo(0.45f));
                Instantiate(gameState.self.defaultDeathEff,transform.position + Vector3.up * 0.5f,Quaternion.identity);
                //BOLD tagging gamechanger system that I hate to implement.
                if (gameObject.tag == "Player")
                {
                    gameState.self.gDesc = gameState.GameStateDesc.GameOver;
                }
            }
            attrs.alive = false;
        }

        //On death, Change to 5000 first, end at 5300(death state).
        if (attrs.alive == false && CurrentStateID < 5000 && CurrentStateID > 5300)
        {
            CurrentStateID = 5000;
        }
        
        onGameFinished();

            //state実行.. これは一つだけに実行されるはず.
            //ステート奪取したときの値を実行..
            StateDef currentState = null;
        if (controlledEntity == null)
        {
            currentState =
            loadedDefs.Find(stDef => stDef.StateDefID == CurrentStateID);
        }
        else
        {
            Debug.LogWarning("Parent Entity Loaded");
            StateDef findDef = controlledEntity.loadedDefs.Find(st => st.StateDefID == CurrentStateID).Clone();
            currentState = findDef;
        }
        if (currentState != null)
        {
            //Debug.Log("Executing stateDef - " + CurrentStateID + " at state time of - " + Time.frameCount + stateTime);
            // + " at time of " + stateTime            
            //the StateDef needs as deepcopy?

            currentState.Execute(this);
            //Debug.Log("Executed stateDef - " + CurrentStateID + " at state time of - "  + Time.frameCount + "/"+ stateTime +
            //" " + this.gameObject.name);
        }
        else
        {
            //Debug.LogError("Loaded State is null : " + CurrentStateID);
        }
    }

    void onGameFinished()
    {
        //プレイヤーなら、ゲームクリア時にこのステートに戻す.
        //180はMUGENでいうと勝利ポーズ.
        if (gameObject.tag == "Player" && gameState.self.gDesc == gameState.GameStateDesc.Finished)
        {
            CurrentStateID = 180;
        }

    }

    //カメラ設定.
    void setViewCams()
    {
        //At Control, wishinvect is Input by command Buffer
        //これ消したい.
        Vector2 wish = entityInput.commandBuffer[0].MoveAxis;//(InputInstance.self.inputValues.MovingAxisRead);
        Vector2 look = entityInput.commandBuffer[0].LookAxis;//(InputInstance.self.inputValues.MovingAxisRead);
                                                             //Debug.Log(entityInput.commandBuffer[0].MoveAxis);

        //Vcamが設定されているなら、Camera設定に従いfwを設定する.
        if (vCam != null)
        {
            targetTo_fw = Vector3.ProjectOnPlane(vCam.transform.forward, Vector3.up).normalized;
            transposer.m_XAxis.Value += look.x * 3.0f;
        }
        else
        {
            //Debug.Log(look.x);
            targetTo_fw = Quaternion.Euler(0f, look.x, 0f) * targetTo_fw;
        }
        if (targetTo_fw != null)
        {
            wishingVect = targetTo_fw * wish.y
            + Quaternion.Euler(0, 90, 0) * targetTo_fw * wish.x;
            //実際の視点.
            Debug.DrawLine(transform.position, transform.position + targetTo_fw * 3.0f);
        }
        //何も設定されていないときは世界基準として設定
        else
        {
            wishingVect = Vector3.forward * wish.y + Vector3.right * wish.x;
        }
    }

    //アニメーション設定
    void setanimPlay()
    {
        MainAnimMixer.PrimalGraph.Play();
        if (LoadedBehavior != null)
        {
            BTree.Start();
        }




        //SetAnimはHitPauseが0で無い限り毎フレーム更新する.
        MainAnimMixer.SetAnim((HitPauseTime <= 0));
        
        animationFrameTime = MainAnimMixer.CurrentAnimTime();
        animationEndTime = MainAnimMixer.EndAnimTime();
    }

    public void entityPhisics()
    {
        capCol.enabled = true;
        if (physicsType != _PhysicsType.N)
        {
            //set gravity.
            rigid.velocity += Physics.gravity * Time.fixedDeltaTime;
        }
        bool isPaused = (HitPauseTime > 0);
        if (isPaused)
        {
            pausedVel = pausedVel == Vector3.zero ? rigid.velocity : pausedVel;
            rigid.isKinematic = isPaused;
        }
        else if (pausedVel != Vector3.zero)
        {
            rigid.isKinematic = isPaused;
            //Debug.Log("unpaused");
            rigid.velocity = pausedVel;
            pausedVel = Vector3.zero;
        }
    }


    //アニメーション変更..
    //entityが指定されているときはそのEntityのAnimを呼び出すとする...がまだ未実装.
    public void ChangeAnim(float timeoffset = 0.0f)
    {
        AnimDef animFindByID = animDefs.Find(x => x.ID == animID);
        if (animFindByID != null)
        {
            MainAnimMixer.ChangeAnim(animFindByID, default, timeoffset);
        }
        MainAnimMixer.SetAnim(false);
    }

    //アニメーション変更 : ステート奪取側..
    public void ChangeAnim_Parent(float timeoffset = 0.0f)
    {
        AnimDef animFindByID = controlledEntity.animDefs.Find(x => x.ID == animID);
        if (animFindByID != null)
        {
            MainAnimMixer.ChangeAnim(animFindByID, default, timeoffset);
        }
        MainAnimMixer.SetAnim(false);
    }

    public bool hitCheck(Entity checkEntity, out Vector3 HitPoint)
    {
        bool resl = false;
        clssSetting cEnemy = checkEntity.MainAnimMixer.MainAnimDef.clssSetting;
        HitPoint = Vector3.zero;
        if (cEnemy != null && MainAnimMixer.MainAnimDef.clssSetting != null)
        {

            //比較対象のentityの時間が取れてなーい！！
            resl = MainAnimMixer.MainAnimDef.clssSetting.clssCollided
            (out Vector3 v1, out Vector3 v2, out float d,
            clssDef.ClssType.Attack, cEnemy, 0.1f);
            HitPoint = (v1 + v2) / 2f;
        }
        else
        {
            Debug.Log("cant find enemy Clss!");
        }
        return resl;
    }

    internal List<ChangeStateQueue> CListQueue = new List<ChangeStateQueue>();

    internal struct ChangeStateQueue
    {
        public int stateDefID;
        public int priority;
    }

    internal GameObject makeInstantiate(GameObject gObj)
    {
        return Instantiate(gObj, transform.position, Quaternion.identity);
    }

    internal char checkHitStates()
    {
        //Fall中ならFという特殊フラグを当てる.
        if (attrs.isFall == true)
        {
            return 'F';
        }
        else
        {
            return (char)stateType;
        }
    }

    const float destroTime_Max = 1.0f;
    const float destroTime_meshRev = 0.033f;
    float destroTime_Current = 0f;

    //点滅後に消滅したい.
    internal void setDestroy()
    {
        if (attrs.isEraseReady)
        {
            destroTime_Current += Time.fixedDeltaTime;
            float rep = Mathf.Repeat(destroTime_Current, destroTime_meshRev);
            bool isDisabled = (destroTime_meshRev * (1f / 2f) > rep);
            Debug.Log("entity renderer : " + isDisabled + " " + rep);
            foreach (Renderer r in renderers)
            {
                r.enabled = isDisabled;
            }
            //アニメーションとかを切断して、消し飛ばす.
            if (destroTime_Current >= destroTime_Max)
            {
                gameState.self.AddKillValue();
                Destroy(gameObject);
            }
        }
    }

    //ワンショットサウンドを鳴らす.
    internal void SetPlayOneShot(AudioClip audio)
    {
        if (selfSource != null && audio != null)
        {
            selfSource.PlayOneShot(audio);
        }
        attrs.isSoundNotPlayed = 1;
    }

    //LuaScript上で呼び出し可能にする.
    // 例えば、//掴み側(ステート奪取側Entity)の右手のボーンの位置から
    // 掴まれ側(ステート非奪取側Entity)の首のボーンの位置 を 除算することで
    // 掴みモーションに合わせて締められモーションを追随することができる.
    public Transform getBoneTransform(string F_str = "root")
    {
        //entity中のすべての階層のtransformを取得.
        List<Transform> tList = allChildTransforms.ToList();
        Transform findTransform = tList.Find(trs => trs.name == F_str);
        if (findTransform != null)
        {
            return findTransform;
        }
        //if not found, return root transform.
        return transform;
    }

    //ignoring self collider for what time.
    public void ignoreCollider()
    {
        capCol.enabled = false;
    }
}

//
public class hitTarget
{

}

