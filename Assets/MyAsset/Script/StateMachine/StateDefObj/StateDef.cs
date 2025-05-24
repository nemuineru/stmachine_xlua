using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

//StateControllerに入力されるジェネリックの属性値に合わせ、計算.
//Vector3とかstringとか入れられるようにしたい.
[System.Serializable]
public class stParams<Type>
{
    //実行されたLuaCondition中の変数を読み出すかを後述するEnumに合わせて考慮.
    [SerializeField]
    loadType loadTypes;

    [SerializeField]
    //valueに入力された値を考慮して、ConditionElem等に代入
    Type stParamValue;

    //LuaConditionで読み出すパラメーターID
    [SerializeField]
    int useID = -1;

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
public class stIDs
{

    //読み出すステートID
    [SerializeField]
    internal int stateID = 0;

    //読み出すステートID
    [SerializeField]
    string stLuaLoads = "";
    delegate bool luaBooltoLoad(Entity entity);

    loadType loadTypes;

    //どの形式でIDを考えるかをenumで管理する.
    public enum loadType
    {
        Constant,
        Calclation
    }

    public bool valueGet(int loadedID, Entity entity)
    {
        bool retValue = stateID == loadedID;
        LuaEnv env = Lua_OnLoad.main.LEnv;
        switch (loadTypes)
        {
            //Calclationなら読み出すLuaCondition中に書かれたfunctionを実行しその値を読み出す.
            case loadType.Calclation:
                {
                    luaBooltoLoad calcParam =
                    env.Global.Get<luaBooltoLoad>(stLuaLoads);
                    retValue = calcParam.Invoke(entity);
                    break;
                }
            //コンスタント値または未定義ならstParamvalueをそのまま使用.
            case loadType.Constant:
            default:
                break;

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
    
    //executing state is decided from this.
    public lua_Read PriorCondition;

    public TextAsset LuaAsset;

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
            env.DoString(PriorCondition.LuaScript);

            //読み出しのQueueStateIDを記述. ここで
            lua_Read.CalcValues.QueuedStateID stateVerd =
            env.Global.Get<lua_Read.CalcValues.QueuedStateID>("QueuedStateID");

            lua_Read.CalcValues.luaOutParams stateDefParams =
            env.Global.Get<lua_Read.CalcValues.luaOutParams>("LuaOutput");

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

            //stateID内にLuaの設定値をセットアップ.
            if (ExecuteStateIDs.Count() > 0)
            {
                foreach (StateController state in StateList)
                {
                    // Debug.Log("Finding stateID " + state.stateID + "," + state.ToString());
                    if (ExecuteStateIDs.Any(i => state.isIDFind(i)))
                    {
                        state.entity = entity;
                        state.loadParams = luaOutputParams;
                        Debug.Log("Executed " + state.ToString());
                        state.OnExecute();
                    }
                }
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

    //stateIDはLuaに送られた,事前計算での情報を元に判別する.
    //これもLua事後計算のパラメータとして組み込んで考えるべきだろうか？
    public stIDs ID;

    public bool isIDFind(int ID)
    {
        return this.ID.valueGet(ID, entity);
    }

    public string stateControllerSubName = "";
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
        AnimDef animFindByID = entity.animListObject.animDef.ToList().Find(x => x.ID == changeAnimID.valueGet(loadParams,entity));
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
        AnimDef animFindByID =
        entity.MainAnimMixer.Mixers.First(x => x.def.ID ==  changeAnimID.valueGet(loadParams,entity)).def;
        //設定されたIDが見つかれば、そのParameterと同様に設定..
        if (animFindByID != null)
        {
            entity.MainAnimMixer.ChangeAnimParams(entity.animID, animParameter.valueGet(loadParams,entity));
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


//後で消します.
//y方向へのimpulse型加速。
[System.Serializable]
[SerializeField]
public class scJump : StateController
{        
    internal override void OnExecute()
    {
        entity.rigid.velocity += Vector3.up * 10.0f;
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
        entity.stateTime = 0;
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
        if (vect.sqrMagnitude > Mathf.Epsilon)
        {
            Quaternion RotateTowards = Quaternion.LookRotation(vect, Vector3.up);
            entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, RotateTowards, RotateWeight);
        }
    }
}