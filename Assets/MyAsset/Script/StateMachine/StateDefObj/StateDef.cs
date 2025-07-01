using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using XLua;
using XLua.LuaDLL;

//StateControllerに入力されるジェネリックの属性値に合わせ、計算.
//Vector3とかstringとか入れられるようにしたい.
//2025-06-28
//I NEED TO MARK THIS WORK ON HITDEF
[System.Serializable]
public class stParams<Type>
{
    //MUGENのHITDEFをそのまま移植するとInspectorが大変なことになるので、
    //必須値以外は隠せるようにしたい.

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

    //この値が設定されているなら, 値が隠され、デフォルト値が読み出される.
    [SerializeField]
    bool _setHidden = true;

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


[System.Serializable]
public class StateDef
{
    public string StateDefName = "Default";
    public Entity entity;
    public int StateDefID = 0;

    //フレーム数で考慮
    internal int stateTime = 0;
    
    public lua_Read PriorCondition;

    //executing state is decided from this.
    public TextAsset LuaAsset;

    public string preStateVerdictName = "QueuedStateID";
    public string ParamLoadName = "LuaOutput";

    [SerializeReference, SerializeField]
    public List<StateController> StateList = new List<StateController>();

    //LuaCondition内でstateDefParamsで定義された値を受け取るためのクラス.
    //...Objectで良いのか？
    List<object> luaOutputParams = new List<object>();


    public void Execute()
    {
        if (PriorCondition != null)
        {
            //ステートIDを読み出すテーブルをLuaにて作成.
            LuaEnv env = Lua_OnLoad.main.LEnv;

            env.Global.Set("LC", new LC());

            //メインのLUA仮想マシンに読み出すテキストを以下に記述.
            if (LuaAsset != null)
            { 
                
            env.DoString(LuaAsset.text);

            //読み出しのQueueStateIDを記述するためのメソッドを作成
            lua_Read.CalcValues.QueuedStateID stateVerd =
            env.Global.Get<lua_Read.CalcValues.QueuedStateID>(preStateVerdictName);

            //ステート宣言パラメータのメソッド作成
            lua_Read.CalcValues.luaOutParams stateDefParams =
            env.Global.Get<lua_Read.CalcValues.luaOutParams>(ParamLoadName);

                //executeStateIDsにはQueuedStateIDの値を入力
            
            int[] ExecuteStateIDs = stateVerd.Invoke(entity);
            if (stateDefParams != null)
            {
                luaOutputParams = stateDefParams.Invoke(entity).ToList();
            }

            string executingStr = "";
            for (int i = 0; i < ExecuteStateIDs.Count(); i++)
            {
                executingStr += ExecuteStateIDs[i] + " , ";
            }
            // Debug.Log(executingStr);


            //def中にあるstateを全部リストアップ
            foreach (StateController state in StateList)
            {
                state.entity = entity;
                //idがステート読み出しリスト内・もしくはステート自体が読み出し処理を行う場合
                if (state.isIDValid(ExecuteStateIDs))
                {
                    //stateにluaOutputParamsを予め登録.
                    state.loadParams = luaOutputParams;
                    //Debug.Log("Executed " + state.ToString());

                    //実際に実行.
                    state.OnExecute();
                }
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
            }
        }
        else
        {
            Debug.LogWarning("PriorCondition is Null.");
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
    internal Entity entity;
    //事前計算済みのパラメータの格納.
    internal List<object> loadParams;

    //stateIDはLuaに送られた,事前計算での情報とLua読み出しのスクリプトで判別する.
    //これもLua事後計算のパラメータとして組み込んで考えるべきだろうか？
    [SerializeField]
    public stateID ID;

    public bool isIDValid(int[] ID)
    {
        return this.ID.valueGet(ID, entity);
    }
    public string stateControllerSubName;
    internal static string stControllerName = null;
    

    internal virtual void OnExecute()
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

    internal override void OnExecute()
    {
        entity.animID = changeAnimID.valueGet(loadParams,entity);
        AnimDef animFindByID = entity._animListObject_onGame.animDef.ToList().Find
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

    internal override void OnExecute()
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
}


[System.Serializable]
[SerializeField]
public class scMove : StateController
{   
    internal override void OnExecute()
    {
        entity.rigid.velocity += entity.wishingVect * 10.0f * Time.fixedDeltaTime;
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
    stParams<float> Damage;
    
    [SerializeField]
    stParams<Vector2> animParameter; 

    [SerializeField]
    stParams<Vector3> HitVect;
    
    [SerializeField]
    stParams<int> HitPause;


    internal override void OnExecute()
    {
        gameState.self.ProvokeHitDef(entity);
    }

}

//後で消します.
//y方向へのimpulse型加速。
[System.Serializable]
[SerializeField]
public class scJump : StateController
{        
    internal override void OnExecute()
    {
        entity.rigid.velocity += Vector3.up * 3.0f;
    }
}

[System.Serializable]
[SerializeField]
public class scColorChange : StateController
{        
    public Color color = Color.black;
    internal override void OnExecute()
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
    internal override void OnExecute()
    {
        // Debug.Log("stateTime set to 0");
        entity.isStateChanged = true;
        // Debug.Log("stateID changes to " + changeTo);
        entity.CurrentStateID = changeTo;
        // Debug.Log("stchanged END");
    }
}

//移動方向に回転を加える.
[System.Serializable]
[SerializeField]
public class scRotateTowards : StateController
{
    public float RotateWeight = 0;
    internal override void OnExecute()
    {
        Vector3 vect = Vector3.ProjectOnPlane(entity.rigid.velocity, Vector3.up);
        //比較がepsilonだとダメっぽそう
        if (vect.sqrMagnitude > 0.01f)
        {
            Quaternion RotateTowards = Quaternion.LookRotation(vect.normalized, Vector3.up);
            entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, RotateTowards, RotateWeight);
        }
    }
}