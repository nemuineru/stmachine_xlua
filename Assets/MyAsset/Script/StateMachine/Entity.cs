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


public class Entity : MonoBehaviour
{
    public class EntityInfo
    {
        public string name;
        public int ID;
    }

    public Rigidbody rigid;

    public int stateTime;

    //移動用の設定など.
    public Transform targetTo;

    public CinemachineVirtualCamera vCam;

    //ステートID.
    public int CurrentStateID = 0;

    public clssSetting defaultClss;

    internal Transform[] allChildTransforms;


    //アニメーション管理用.
    public int animID = 0;
    [SerializeField]
    List<AnimlistObject> animListObject;

    //List化されたanimListObjectからanimDefsのリストを作成する.
    //他Entityが自分のAnimを参照する際、パラメータ変更を共有してしまうことを懸念.

    internal List<AnimDef> animDefs = new List<AnimDef>();

    Animator animator;

    PlayableOutput PrimalPlayableOut;
    public MainNodeConfigurator MainAnimMixer = new MainNodeConfigurator();

    public Color CurColor;

    public bool isOnGround;

    //ステート直後のステート時間を0にするため、追加.
    public bool isStateChanged = false;


    Material mat;
    public SkinnedMeshRenderer mesh;

    public List<StateDefListObject> DefLists;

    public Vector3 wishingVect;

    //この秒数が0にならない限り動きを止める. stateTimeなども同様.
    internal float HitPauseTime;

    //Input Manager for each entitys
    internal entityInputManager entityInput = new entityInputManager();

    //OnHit確認用. 後で整理したい.
    public bool isStateHit = false;

    // Start is called before the first frame update
    void Awake()
    {
        allChildTransforms = GetComponentsInChildren<Transform>(true);
        mat = mesh.material;
        animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
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
            loadedDefs.AddRange(dObj.stateDefs);            
        }
    }

    List<StateDef> loadedDefs = new List<StateDef>();

    string verd_1;
    [SerializeField]
    Vector3 raycenter = Vector3.down * 0.5f;

    Vector3 pausedVel = Vector3.zero;

    // Update is called once per frame
    void FixedUpdate()
    {
        defaultClss.clssPosUpdate();

        //後で消します.
        //
        entityInput.RecordInput_Player(0);


        isStateChanged = false;

        //SetAnimはHitPauseが0で無い限り毎フレーム更新する.
        {
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
            rigid.isKinematic = isPaused;
            MainAnimMixer.SetAnim((HitPauseTime <= 0));
            HitPauseTime -= 1.0f;
        }
        MainAnimMixer.PrimalGraph.Play();

        Vector2 wish = (InputInstance.self.inputValues.MovingAxisRead);
        if (vCam != null)
        {
            wishingVect = Vector3.ProjectOnPlane(vCam.transform.forward, Vector3.up) * wish.y
            + Vector3.ProjectOnPlane(vCam.transform.right, Vector3.up) * wish.x;
        }
        mat.SetColor("_Color", CurColor);

        if (CListQueue.Count > 0)
        {
            //Most Primal Queue is Most Biggest Number.
            CListQueue.Sort((CQ_L, CQ_M) => CQ_M.priority - CQ_L.priority);
            CurrentStateID = CListQueue[0].stateDefID;
            isStateHit = false;
            CListQueue.Clear();
        }


        //常時実行StateDef(-1, -2, -3)
            StateDef AutoState_1 =
        loadedDefs.Find(stDef => stDef.StateDefID == -1);
        if (AutoState_1 != null)
        {
            //Debug.Log("auto checking -1 state");
            AutoState_1.Execute(this);
        }


        //state実行..
        StateDef currentState =
        loadedDefs.Find(stDef => stDef.StateDefID == CurrentStateID);
        if (currentState != null)
        {
            //Debug.Log("Executed stateDef - " + CurrentStateID);
            // + " at time of " + stateTime            
            //the StateDef needs as deepcopy?
            currentState.Execute(this);
        }
        else
        {
            Debug.LogError("Loaded State is null : " + CurrentStateID);
        }

        //地面判定.
        //raycenter
        Ray ray = new Ray(transform.position + Vector3.up * 0.009f, Vector3.down);
        Debug.DrawRay(ray.origin, Mathf.Max(0, -rigid.velocity.y) * Vector3.down);

        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo, Mathf.Max(0.01f, -rigid.velocity.y * Time.fixedDeltaTime), LayerMask.GetMask("Terrain"));

        isOnGround = (hitInfo.collider != null);

        stateTime = isStateChanged && HitPauseTime <= 0 ? 0 : stateTime + 1;
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

    //前プロジェクトのように、スクリプト内でステートをとりあえず記述.
    //今回は最初のstatedefのLua内で読み出すステートを指定.

    /*
        void DefSet()
        {

            //StateDef_1

            //ステートタイムが0以上でカラーなどを変更.
            //また、ジャンプボタンが押されているならステート変更.
            string Lua_StateDef_0 = @"
                local Entity = CS.Entity;
                -- ステート変更のファンクション
                    function QueuedStateID(in_entity)

                        selfOnGrd = LC:isEntityOnGround(in_entity)
                        selfJump = LC:CheckButtonPressed()
                        selfStTime = LC:CheckStateTime(in_entity) 

                        verd = {}
                        if (selfOnGrd == true and selfJump == true) then 
                            table.insert( verd, 1 )
                        end

                        if(selfOnGrd == true) then
                            table.insert( verd, 2 )
                        end

                        if( LC:CheckStateTime(in_entity) > 0 ) then
                            table.insert( verd, 0 ) 
                        end
                    return verd
                end
            ";

            StateDef def_1 = new StateDef();
            def_1.StateDefName = "OnInitLoad";
            def_1.entity = this;
            def_1.StateDefID = 0;
            def_1.PriorCondition = new lua_Read(Lua_StateDef_0);


    //stateDef, StateList

            scJump _stJump= new scJump();
            _stJump.stateID = 1;

            scChangeState _CgState = new scChangeState();
            _CgState.stateID = 1;
            _CgState.changeTo = 1;

            scColorChange _stColorChange_1 = new scColorChange();
            _stColorChange_1.stateID = 0;
            _stColorChange_1.color = Color.white;

            scMove mov = new scMove();
            mov.stateID = 2;

            def_1.StateList = new List<StateController>{ _stJump, _stColorChange_1, _CgState, mov};


    //StateDef_2

            //ステートタイムが0以上でカラーなどを変更.
            //また、地面上ならステート変更.
            string Lua_StateDef_1 = @"
                -- ステート変更のファンクション
                    function QueuedStateID(in_entity)

                        verd = {}
                        if ( LC:isEntityOnGround(in_entity) == true ) then 
                            table.insert( verd, 1 )
                        end
                        if( LC:CheckStateTime(in_entity) > 0 ) then
                            table.insert( verd, 0 ) 
                        end
                    return verd
                end
            ";


            StateDef def_2 = new StateDef();
            def_2.StateDefName = "Jumping";
            def_2.StateDefID = 1;
            def_2.entity = this;
            def_2.PriorCondition = new lua_Read(Lua_StateDef_1);


    //stateDef_2, StateList

            scColorChange _stColorChange_2 = new scColorChange();
            _stColorChange_2.stateID = 0;
            _stColorChange_2.color = Color.red;

            scChangeState _CgState_2 = new scChangeState();
            _CgState_2.stateID = 1;
            _CgState_2.changeTo = 0;

            def_2.StateList = new List<StateController>{ _stColorChange_2, _CgState_2};


            DefList.stateDefs = new List<StateDef>{ def_1, def_2 };
        }
    */
}