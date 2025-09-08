using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Analytics;
using XLua;
using XLua.LuaDLL;
using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;

//EditorExtention. for deepcopy.
public static class ObjectExtension
{
    // ディープコピーの複製を作る拡張メソッド
    public static T DeepClone<T>(this T src)
    {
        using (var memoryStream = new System.IO.MemoryStream())
        {
            var binaryFormatter
            = new System.Runtime.Serialization
                    .Formatters.Binary.BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, src); // シリアライズ
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            return (T)binaryFormatter.Deserialize(memoryStream); // デシリアライズ
        }
    }
}

//StateControllerに入力されるジェネリックの属性値に合わせ、計算.
//Vector3とかstringとか入れられるようにしたい.
//2025-06-28
//I NEED TO MARK THIS WORK ON HITDEF
[System.Serializable]
public class stParams<Type>
{
    //デフォルト値の設定.
    //stParamsを設定する際は必ず初期化を行うとする.
    internal Type defaultValue;

    //必須等設定されている場合
    public stParams(Type defValue, bool setEssential)
    {
        defaultValue = defValue;
        _setEssential = setEssential;
        _setHidden = false;
    }

    //デフォルト値が設定されている場合なら初期は隠す.
    public stParams(Type defValue)
    {
        defaultValue = defValue;
        _setEssential = false;
        _setHidden = true;
    }

    //何も設定されていない際...
    //はTypeの新規インスタンス作成時になんか不具合起こしそう..でもない？
    public stParams()
    {
        _setEssential = false;
        _setHidden = true;
    }
    
    //この値が設定されているなら, "必ず"隠されない.
    [SerializeField]
    bool _setEssential = false;


    //MUGENのHITDEFをそのまま移植するとInspectorが大変なことになるので、
    //必須値以外は隠せるようにしたい.

    //この値が設定されているなら, Inspector上では隠されるようになる.. 右クリックのメニューで解除される.
    bool _setHidden = true;

    private void toggleHidden()
    {
        _setHidden = !_setHidden;
    }

    //実行されたLuaCondition中の変数を読み出すかを後述するEnumに合わせて考慮.
    [SerializeField]
    loadType loadTypes;

    [SerializeField]
    //valueに入力された値を考慮して、ConditionElem等に代入
    Type stParamValue;

    //LuaConditionで読み出すパラメーターID
    [SerializeField]
    int useID = -1;

    //
    [SerializeField]
    LC LuaCondition = new LC();

    //Luaで読み出すメソッド名
    [SerializeField]
    string stLuaLoads = "";

    delegate object luaCalcParam(Entity entity);

    //どの形式で値を読み出すかをenumで管理する.
    public enum loadType
    {
        Constant,
        Condition,
        Calclation
    }

    //登録値を読み出す.
    public Type valueSet(Type val)
    {
        return val;
    }

    //実際に想定された値を読み出す.
    //Condition/Calclationではluaの内容を読み出したいが..

    public Type valueGet(List<object> loadParams, Entity entity)
    {
        LuaEnv env = Lua_OnLoad.main.LEnv;
        Type retValue = stParamValue;
        switch (loadTypes)
        {
            //Conditionなら読み出されたLuaConditionに登録されたvalue配列から..
            //としたい. 
            case loadType.Condition:
                {
                    retValue = (Type)loadParams[useID];
                    break;
                }
            //Calclationなら読み出すLuaCondition中に書かれたfunctionを実行しその値を読み出す.
            case loadType.Calclation:
                {
                    luaCalcParam calcParam =
                    env.Global.Get<luaCalcParam>(stLuaLoads);
                    retValue = (Type)calcParam.Invoke(entity);
                    break;
                }
            //コンスタント値または未定義ならstParamvalueをそのまま使用.
            case loadType.Constant:
            default:
                break;

        }
        return retValue;
    }

    //LuaEnvで実行されたLuaEnvの登録値を読み出して、それをvalueSetに実行.
    /*
        public Type getLuaElem()
        {
            Type type = new ;
            if (useLuaCondition && useID > -1)
            {

            }
        }
    */
}

//
[System.Serializable]
public class stateID
{
    [SerializeField]
    bool useLua = false;

    //読み出すステートID
    [SerializeField]
    internal int value = 0;

    //読み出すLuaのパラメータ名
    [SerializeField]
    string stLuaLoads = "";
    delegate bool luaBooltoLoad(Entity entity);

    //Luaの状態・もしくは入力されたIDがこのidと同様の時にtrueを返す。
    public bool valueGet(int[] loadID, Entity entity)
    {
        bool retValue = false;
        LuaEnv env = Lua_OnLoad.main.LEnv;
        //Luaを使用するなら
        if (useLua && stLuaLoads != "")
        {
            luaBooltoLoad calcParam =
            env.Global.Get<luaBooltoLoad>(stLuaLoads);
            retValue = calcParam.Invoke(entity);
        }
        else
        {
            retValue = (loadID.Count() > 0 && loadID.Any(i => i == value));
        }
        return retValue;
    }
}

//StateDefのクローンが必須.
//なんかEntityの指定が重複していそう.
[System.Serializable]
public class StateDef
{
    public string StateDefName = "Default";
    public int StateDefID = 0;

    //フレーム数で考慮
    internal int stateTime = 0;

    //executing state is decided from this.
    public TextAsset LuaAsset;

    public string preStateVerdictName;
    public string ParamLoadName;

    private LuaTable _stateLoadTables;
    private LuaTable _stateParamTables;

    //それぞれMoveType
    public char stateType;
    public char moveType;
    public char physType;

    [SerializeReference, SerializeField]
    public List<StateController> StateList = new List<StateController>();

    //LuaCondition内でstateDefParamsで定義された値を受け取るためのクラス.
    //...Objectで良いのか？
    List<object> luaOutputParams = new List<object>();

    //ctrlフラグの設定.
    public bool setCtrl = false;

    //これも結局Cloneが必須かぁ..
    public StateDef Clone()
    {
        var retDef = new StateDef();
        retDef.StateDefName = StateDefName;
        retDef.StateDefID = StateDefID;
        retDef.stateTime = stateTime;
        retDef.LuaAsset = LuaAsset;
        retDef.preStateVerdictName = preStateVerdictName;
        retDef.ParamLoadName = ParamLoadName;
        retDef.StateList = StateList;
        retDef.luaOutputParams = luaOutputParams;
        return retDef;
    }

    LuaEnv env;

    void OnInitDef()
    {
        Debug.Log("Generating Metatables on " + StateDefID);
        env = Lua_OnLoad.main.LEnv;
        env.Global.Set("LC", new LC());

        //StateID用にメタテーブルを作成.
        var _sLoadT = env.NewTable();
        _stateLoadTables = env.NewTable();
        _sLoadT.Set("__index", env.Global);
        //メタテーブルに登録.
        _stateLoadTables.SetMetaTable(_sLoadT);
        _sLoadT.Dispose();

        //パラメータテーブルも同様.
        var _sParamT = env.NewTable();
        _stateParamTables = env.NewTable();
        _sParamT.Set("__index", env.Global);
        //別のメタテーブルに登録.
        _stateParamTables.SetMetaTable(_sParamT);
        _sParamT.Dispose();
    }

    //Execute時のLuaのStateIDをそれぞれのStateDefに保存 - これ、掴みの時のEntity参照時の設定時に重複発生しそー..    
    //

    void entityTypeSet(Entity entity)
    {
        switch (stateType)
        {
            case 's':
            case 'S':
                {
                    entity.stateType = Entity._StateType.S;
                    break;
                }
            case 'a':
            case 'A':
                {
                    entity.stateType = Entity._StateType.A;
                    break;
                }
            case 'l':
            case 'L':
                {
                    entity.stateType = Entity._StateType.L;
                    break;
                }
        }
        switch (physType)
        {             
            case 's':
            case 'S':
                {
                    entity.physicsType = Entity._PhysicsType.S;
                    break;
                }
            case 'a':
            case 'A':
                {
                    entity.physicsType = Entity._PhysicsType.A;
                    break;
                }
            case 'n':
            case 'N':
                {
                    entity.physicsType = Entity._PhysicsType.N;
                    break;
                }
        }
        switch (moveType)
        {             
            case 'i':
            case 'I':
                {
                    entity.moveType = Entity._MoveType.I;
                    break;
                }
            case 'a':
            case 'A':
                {
                    entity.moveType = Entity._MoveType.A;
                    break;
                }
            case 'h':
            case 'H':
                {
                    entity.moveType = Entity._MoveType.H;
                    break;
                }
        }
    }

    public void Execute(Entity entity)
    {
        //stateTimeが0の時, 恒常設定されたステートパラメータを確認
        if (entity.stateTime == 0)
        {
            entityTypeSet(entity);
        }

        if (_stateLoadTables == null)
            OnInitDef();
        //メインのLUA仮想マシンに読み出すテキストを以下に記述.
        if (LuaAsset != null)
        {
            //env.DoString(LuaAsset.text);

            //QueueStateIDの呼び出し. _stateLoadTableに登録値を呼び出す.
            env.DoString(LuaAsset.text, preStateVerdictName, _stateLoadTables);

            //読み出しのQueueStateIDを記述するためのメソッドを作成
            lua_Read.CalcValues.QueuedStateID stateVerd =
            _stateLoadTables.Get<lua_Read.CalcValues.QueuedStateID>(preStateVerdictName);
            //env.Global.Get<lua_Read.CalcValues.QueuedStateID>(preStateVerdictName);

            //ParamTablesの呼び出し. _stateParamTablesにパラメータ値を呼び出す.
            env.DoString(LuaAsset.text, ParamLoadName, _stateParamTables);

            //ステート宣言パラメータのメソッド作成
            lua_Read.CalcValues.luaOutParams stateDefParams =
            _stateParamTables.Get<lua_Read.CalcValues.luaOutParams>(ParamLoadName);
            //env.Global.Get<lua_Read.CalcValues.luaOutParams>(ParamLoadName);

            //executeStateIDsにはQueuedStateIDの値を入力


            int[] ExecuteStateIDs;

            //読み出し時のEntityで、登録重複が見られる場合の誤作動をなんとかしなければ
            //LuaTableの別々の読み出しにしたいが、なんかおかしい..
            //StateDef自体をSingleTonにするべきか..
            if (stateVerd != null)
            {
                //xLUAの読み出しでなーぜーかー別のstateIDのスクリプト読み出し結果が呼び出されて追加項目になってる.
                //クソぉ！！
                //...しょーがないのでLight11氏のhttps://light11.hatenadiary.com/entry/2021/08/17/195914
                // を参考にリベイクしてみるか..

                ExecuteStateIDs = stateVerd.Invoke(entity);
                if (stateDefParams != null)
                {
                    luaOutputParams = stateDefParams.Invoke(entity).ToList();
                }




                string executingStr = "";
                if (ExecuteStateIDs != null)
                {
                    for (int i = 0; i < ExecuteStateIDs.Count(); i++)
                    {
                        executingStr += ExecuteStateIDs[i] + " , ";
                    }
                }
                //Debug.Log("State Def - " + StateDefID + " State List #s - " + StateList.Count);
                //def中にあるstateを全部リストアップ
                foreach (StateController state in StateList)
                {
                    //idがステート読み出しリスト内・もしくはステート自体が読み出し処理を行う場合
                    //Debug.LogWarning(entity.gameObject.name + " loads " + state.ID.value.ToString());
                    if (state.isIDValid(ExecuteStateIDs, entity))
                    {
                        //stateにluaOutputParamsを予め登録.
                        state.loadParams = luaOutputParams;

                        //実際に実行.
                        //state.Entityに直接登録すると、別キャラクターが参照するため変更..
                        state.OnExecute(entity);
                    }
                }
                //Debug.Log(entity.gameObject.name + " executes stateID " + executingStr + " at the stateDef of " + StateDefID);
            }
            else
            {
                //Debug.LogError("Execute ID/stateVerd is NULL!");
            }




            //stateID内にLuaの設定値をセットアップ.

            /*
            if (ExecuteStateIDs.Count() > 0)
            {
                //def中にあるstateを全部リストアップ
                foreach (StateController state in StateList)
                {
                    state.entity = entity;
                    // Debug.Log("Finding stateID " + state.stateID + "," + state.ToString());

                    //Lua設定値の中から..という
                    if (ExecuteStateIDs.Any(i => state.isIDValid(i)))
                    {
                        state.loadParams = luaOutputParams;
                        Debug.Log("Executed " + state.ToString());
                        state.OnExecute();
                    }
                }
            }
            */

            env.Tick();
        }

    }
}


//ステートベースクラス. ここから派生する. なお、実行判別式はLuaを用いることとする.
//Lua内にはここで用意された関数を流用する.

//2025-05-23
//ステコンのloadParamsにはLuaで"計算済み"の値を考える.
[System.Serializable]
[SerializeField]
public class StateController
{   
    [ReadOnly]
    //事前計算済みのパラメータの格納.
    internal List<object> loadParams;

    //stateIDはLuaに送られた,事前計算での情報とLua読み出しのスクリプトで判別する.
    //これもLua事後計算のパラメータとして組み込んで考えるべきだろうか？
    [SerializeField]
    public stateID ID;

    public bool isIDValid(int[] ID, Entity entity)
    {
        return this.ID.valueGet(ID, entity);
    }
    public string stateControllerSubName;
    internal static string stControllerName = null;
    

    internal virtual void OnExecute(Entity entity)
    {

    }

    public virtual string typeGet()
    {
        return this.ToString();
    }
}


//アニメーションの変更.
//ジェネリックメソッドの導入実験も兼ねて、やってみる.
[System.Serializable]
[SerializeField]
public class scAnimSet : StateController
{
    [SerializeField]
    stParams<int> changeAnimID;
    
    [SerializeField]
    stParams<Vector2> animParameter; 

    internal override void OnExecute(Entity entity)
    {
        entity.animID = changeAnimID.valueGet(loadParams,entity);
        AnimDef animFindByID = entity.animDefs.ToList().Find
        (x => x.ID == changeAnimID.valueGet(loadParams, entity));
        //設定されたIDが見つかれば、そのParameterと同様に設定..
        if (animFindByID != null)
        {
            entity.MainAnimMixer.ChangeAnim(animFindByID);
            entity.MainAnimMixer.ChangeAnimParams(entity.animID, animParameter.valueGet(loadParams,entity));
        }
    }
}


//アニメーションパラメータの変更.
[System.Serializable]
[SerializeField]
public class scAnimParamChange : StateController
{
    [SerializeField]
    stParams<int> changeAnimID;
    
    [SerializeField]
    stParams<Vector2> animParameter;

    internal override void OnExecute(Entity entity)
    {
        if (entity.MainAnimMixer.Mixers.Count() > 0)
        {
            AnimDef animFindByID = entity.MainAnimMixer.MainAnimDef;
            if (animFindByID != null)
            {
                entity.MainAnimMixer.ChangeAnimParams(entity.animID, animParameter.valueGet(loadParams, entity));
            }
        }
        //Sequence Not FoundErrorが出たときのソスコ. 
        //今も特定のアニメ再生時そこで止まるので解決が必須
        /*        
        if (entity.MainAnimMixer.Mixers.Count() > 0)
        {
            MixAnimNode findNode = entity.MainAnimMixer.Mixers.First(x => x != null
            && x.def.ID == changeAnimID.valueGet(loadParams, entity));
            if (findNode != null)
            {
                AnimDef animFindByID =
                findNode.def;
                //設定されたIDが見つかれば、そのParameterと同様に設定..
                if (animFindByID != null)
                {
                    entity.MainAnimMixer.ChangeAnimParams(entity.animID, animParameter.valueGet(loadParams, entity));
                }
            }
        }
        */
    }
}


[System.Serializable]
[SerializeField]
public class scMove : StateController
{   
    internal override void OnExecute(Entity entity)
    {
        entity.rigid.velocity = entity.wishingVect * 100.0f * Time.fixedDeltaTime;
    }

    public override string typeGet()
    {
        return "scMove";
    }
}

//攻撃判定設定 - 指定の攻撃をシステムに予約する
//攻撃があたった対象を予約されたステート番号5000..
//ぶっちゃけめんどくせー。
[System.Serializable]
[SerializeField]
public class scHitDef : StateController
{

    [SerializeField]
    stParams<hitDefParams> hitParams;


    internal override void OnExecute(Entity entity)
    {
        //HitCheckを行う.
        entity.attrs.isStateHit = gameState.self.ProvokeHitDef
        (entity, hitParams.valueGet(loadParams, entity));
    }

}

//後で消します.
//y方向へのimpulse型加速。
[System.Serializable]
[SerializeField]
public class scJump : StateController
{
    internal override void OnExecute(Entity entity)
    {
        entity.rigid.velocity = Vector3.ProjectOnPlane(entity.rigid.velocity,Vector3.up) + Vector3.up * 3.2f;
        entity.isOnGround = false;
        //Debug.Log("Executed " + "JumpState " + " in " + entity.name + " - " + entity.stateTime);
    }
}

[System.Serializable]
[SerializeField]
public class scColorChange : StateController
{        
    public Color color = Color.black;
    internal override void OnExecute(Entity entity)
    {
        // Debug.Log("Oncheck Phase of " + this.ToString());
        if(entity != null)
        {
            entity.CurColor = color;
        }
    }
}

[System.Serializable]
[SerializeField]
public class scChangeState : StateController
{
    public int changeTo = 0;
    public int priority = 0;
    internal override void OnExecute(Entity entity)
    {
        //this ChangeState Needs to change-Queues.
        entity.CListQueue.Add(new Entity.ChangeStateQueue() { stateDefID = changeTo, priority = priority });
        // Debug.Log("stateTime set to 0");
        entity.isStateChanged = true;
        // Debug.Log("stchanged END");
    }

    public scChangeState()
    {
        this.priority = 0;
        this.changeTo = 0;
    }

    public scChangeState(int changeToID, int priority)
    {
        this.priority = priority;
        this.changeTo = changeToID;
    }
}

//移動方向に回転を加える.
//現在はカメラ方向に向けるとしている
[System.Serializable]
[SerializeField]
public class scRotateTowards : StateController
{
    public float RotateWeight = 0;
    internal override void OnExecute(Entity entity)
    {
        Vector3 vect = Vector3.ProjectOnPlane(entity.rigid.velocity, Vector3.up);
        //比較がepsilonだとダメっぽそう
        if (vect.sqrMagnitude > 0.01f)
        {
            Quaternion RotateTowards = Quaternion.LookRotation(vect.normalized, Vector3.up);
            entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, RotateTowards, RotateWeight);
            //カメラ方向に回転.
            //Quaternion RotateTowards = Quaternion.LookRotation(entity.targetTo_fw, Vector3.up);
            //entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, RotateTowards, RotateWeight);
            
        }
    }
}


//設定位置にエフェクトを放出する.
[System.Serializable]
[SerializeField]
public class scEmitEffect : StateController
{
    [SerializeField]
    stParams<GameObject> EmitObject;
    internal override void OnExecute(Entity entity)
    {
        entity.makeInstantiate(EmitObject.valueGet(loadParams, entity));
    }
}


//自己のステートの返還作業.
[System.Serializable]
[SerializeField]
public class scSelfState : StateController
{
    [SerializeField]
    int changeTo = 0;

    [SerializeField]
    int priority = 0;
    internal override void OnExecute(Entity entity)
    {
        //親を戻し、StateChangeを行う.
        entity.parentEntity = null;
        scChangeState cState = new scChangeState(changeTo, priority);
        cState.OnExecute(entity);
    }
}

//status変更. (HP)
public class scAddHealth : StateController
{ 
    
    [SerializeField]
    int value = 0;

    [SerializeField]
    int priority = 0;
    internal override void OnExecute(Entity entity)
    {
        entity.status.currentHP += value;
    }
}

//status変更. (Energy)
public class scAddEnergy : StateController
{

    [SerializeField]
    int value = 0;

    [SerializeField]
    int priority = 0;
    internal override void OnExecute(Entity entity)
    {
        entity.status.currentEnergy += value;
    }
}


//status変更. (Charge)
public class scAddCharge : StateController
{ 
    
    [SerializeField]
    int value = 0;

    [SerializeField]
    int priority = 0;

    //これだけfixedDeltaTimeが乗算.
    internal override void OnExecute(Entity entity)
    {
        entity.status.ChargeTime += value * Time.fixedDeltaTime;
    }
}


//status変更. (Charge)
public class scSetCharge : StateController
{ 
    
    [SerializeField]
    int value = 0;

    [SerializeField]
    int priority = 0;

    //これだけfixedDeltaTimeが乗算.
    internal override void OnExecute(Entity entity)
    {
        entity.status.ChargeTime = value;
    }
}

//ctrlフラグのセット.
public class scSetCtrl : StateController
{     
    [SerializeField]
    stParams<bool> value;

    [SerializeField]
    int priority = 0;

    //これだけfixedDeltaTimeが乗算.
    internal override void OnExecute(Entity entity)
    {
        entity.attrs.ctrl = value.valueGet(loadParams,entity);
    }
}

//処理中・直後フレームの特殊操作など. string形式で操作される 
public class scSetAssertSpecial : StateController
{
    [SerializeField]
    stParams<string> value;

    [SerializeField]
    int priority = 0;

    internal override void OnExecute(Entity entity)
    {
        //entity.attrs.ctrl = value.valueGet(loadParams, entity);
    }
}

//ゲームシステムに死を組み込む.
public class scSendDeathMeesage : StateController
{
    [SerializeField]
    int priority = 0;

    internal override void OnExecute(Entity entity)
    {
        //entity.attrs.ctrl = value.valueGet(loadParams, entity);
        entity.attrs.isEraseReady = true;
    }
}

//NotHitBy : 特定攻撃に対しての無敵効果.
public class scNotHitBy : StateController
{
    [SerializeField]
    stParams<string> States;

    [SerializeField]
    stParams<int> time;

    internal override void OnExecute(Entity entity)
    {
        //entity.attrs.ctrl = value.valueGet(loadParams, entity);
    }
}



//StateTypeの変更など
public class scSetStatetype : StateController
{
    [SerializeField]
    stParams<char> value;

    [SerializeField]
    stParams<int> priority;

    internal override void OnExecute(Entity entity)
    {
        char val = value.valueGet(loadParams, entity);
        if (Enum.IsDefined(typeof(Entity._StateType), val))
        {
            entity.stateType = (Entity._StateType)val;
        }
    }
}

//PhysTypeの変更など
public class scSetStatePhystype : StateController
{
    [SerializeField]
    stParams<char> value;

    [SerializeField]
    stParams<int> priority;
    
    internal override void OnExecute(Entity entity)
    {
        char val = value.valueGet(loadParams, entity);
        if (Enum.IsDefined(typeof(Entity._PhysicsType), val))
        {
            entity.physicsType = (Entity._PhysicsType)val;
        }
    }
}

//MoveTypeの変更など
public class scSetStateMovetype : StateController
{
    [SerializeField]
    stParams<char> value;

    [SerializeField]
    stParams<int> priority;
    
    internal override void OnExecute(Entity entity)
    {
        char val = value.valueGet(loadParams, entity);
        if (Enum.IsDefined(typeof(Entity._MoveType), val))
        {
            entity.moveType = (Entity._MoveType)val;
        }
    }
}

