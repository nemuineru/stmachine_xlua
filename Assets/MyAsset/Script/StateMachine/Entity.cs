using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
using System.ComponentModel;
using UnityEditor;


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
    AnimlistObject animListObject;

    //AnimListObject自体を変更しないとする.
    [ReadOnly(true)]
    public AnimlistObject _animListObject_onGame;
    Animator animator;

    PlayableOutput PrimalPlayableOut;
    public MainNodeConfigurator MainAnimMixer = new MainNodeConfigurator();

    public Color CurColor;

    public bool isOnGround;

    //ステート直後のステート時間を0にするため、追加.
    public bool isStateChanged = false;


    Material mat;
    public SkinnedMeshRenderer mesh;

    public StateDefListObject DefList;

    public Vector3 wishingVect;

    //この秒数が0にならない限り動きを止める. stateTimeなども同様.
    internal float HitPauseTime;

    //Input Manager for each entitys
    internal entityInputManager entityInput = new entityInputManager();

    // Start is called before the first frame update
    void Awake()
    {
        allChildTransforms = GetComponentsInChildren<Transform>(true);
        mat = mesh.material;
        animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        //DefList = (StateDefListObject)StateDefListObject.CreateInstance(typeof(StateDefListObject));
        //DefSet();
        //設定されたDefListに対しこのエンティティを指定する.
        DefList.stateDefs.ForEach(def => def.entity = this);
        //DefList.stateDefs.ForEach(def => def.PriorCondition = def.PriorCondition);

        //アニメ設定.
        PrimalPlayableOut = new PlayableOutput();
        if (animListObject != null && animator != null)
        {
            _animListObject_onGame = Instantiate(animListObject);
            MainAnimMixer.SetupGraph(ref animator, ref PrimalPlayableOut);
            PrimalPlayableOut.SetSourcePlayable(MainAnimMixer.mixMixer);
            ChangeAnim();
        }
        defaultClss.initClss(this);
    }

    string verd_1;
    [SerializeField]
    Vector3 raycenter = Vector3.down * 0.5f;

    // Update is called once per frame
    void FixedUpdate()
    {
        defaultClss.clssPosUpdate();
        //後で消します.
        //
        entityInput.RecordInput_Player(0);


        isStateChanged = false;

        //SetAnimは毎フレーム更新する.
        MainAnimMixer.SetAnim();
        MainAnimMixer.PrimalGraph.Play();


        Vector2 wish = (InputInstance.self.inputValues.MovingAxisRead);
        if (vCam != null)
        { 
            wishingVect = Vector3.ProjectOnPlane(vCam.transform.forward, Vector3.up) * wish.y
            + Vector3.ProjectOnPlane(vCam.transform.right, Vector3.up) * wish.x;
        }
        mat.SetColor("_Color", CurColor);

        //地面判定.
        Ray ray = new Ray(transform.position + raycenter + Vector3.up * Physics.defaultContactOffset, Vector3.down);

        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo, 0.05f, LayerMask.GetMask("Terrain"));

        isOnGround = (hitInfo.collider != null);

        StateDef currentState =
        DefList.stateDefs.Find(stDef => stDef.StateDefID == CurrentStateID);
        if (currentState != null)
        {
            //Debug.Log("Executed stateDef - " + CurrentStateID);
            // + " at time of " + stateTime
            currentState.Execute();
        }
        else
        {
            Debug.LogError("Loaded State is null : " + CurrentStateID);
        }
        stateTime = isStateChanged ? 0 : stateTime + 1;
    }

    //アニメーション変更..
    public void ChangeAnim()
    {
        AnimDef animFindByID = _animListObject_onGame.animDef.ToList().Find(x => x.ID == animID);
        if (animFindByID != null)
        {
            MainAnimMixer.ChangeAnim(animFindByID);
        }
    }

    public bool hitCheck(Entity checkEntity)
    {
        bool resl = false;
        clssSetting cEnemy = checkEntity.MainAnimMixer.MainAnimDef.clssSetting;
        if (cEnemy != null && MainAnimMixer.MainAnimDef.clssSetting != null)
        {

            //比較対象のentityの時間が取れてなーい！！
            resl = MainAnimMixer.MainAnimDef.clssSetting.clssCollided
            (out Vector3 v1, out Vector3 v2, out float d,
            clssDef.ClssType.Hit, cEnemy, 0.1f);
        }
        else
        {
            Debug.Log("cant find enemy Clss!");
        }
        return resl;
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