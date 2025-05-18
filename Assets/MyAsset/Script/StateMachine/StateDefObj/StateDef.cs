using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using XLua;

//StateControllerに入力されるジェネリックの属性値に合わせ、計算.
//Vector3とかstringとか入れられるようにしたい.
public class stParams<Type>
{
    //valueに入力された値を考慮して、ConditionElem等に代入
    Type stParamValue;

    //LuaConditionで読み出すパラメーターID
    int useID = -1;
    //実行されたLuaCondition中の変数を読み出すかを考慮.
    bool useLuaCondition;

    //登録値を読み出す.
    public Type valueSet(Type val)
    {
        return val;
    }

    public Type valueGet
    {
        get { return stParamValue; }
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

[System.Serializable]
public class StateDef
{
    public string StateDefName = "Default";
    public Entity entity;
    public int StateDefID = 0;
    
    //executing state is decided from this.
    public lua_Read PriorCondition;

    [SerializeReference, SerializeField]
    public List<StateController> StateList = new List<StateController>();


    public void Execute()
    {
        if(PriorCondition != null)
        {
            //ステートIDを読み出すテーブルをLuaにて作成.
            LuaEnv env = Lua_OnLoad.main.LEnv;

            env.Global.Set("LC", new LC());

            env.DoString(PriorCondition.LuaScript);     

            lua_Read.CalcValues.QueuedStateID stateVerd =
            env.Global.Get<lua_Read.CalcValues.QueuedStateID>("QueuedStateID");
            
            lua_Read.CalcValues.StateDefParams stateValues =
            env.Global.Get<lua_Read.CalcValues.StateDefParams>("stateValues");
            
            int[] ExecuteStateIDs = stateVerd.Invoke(entity);
            string executingStr = "";
            for(int i = 0;i < ExecuteStateIDs.Count(); i++)
            {
                executingStr += ExecuteStateIDs[i] + " , ";
            }
               // Debug.Log(executingStr);

            //stateID内にLuaの設定値をセットアップ.
            if(ExecuteStateIDs.Count() > 0)
            {
                foreach (StateController state in StateList)
                {
                    // Debug.Log("Finding stateID " + state.stateID + "," + state.ToString());
                    if (ExecuteStateIDs.Any(i => i == state.stateID))
                    {
                        state.entity = entity;
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
[System.Serializable]
[SerializeField]
public class StateController
{   
    [ReadOnly]
    internal Entity entity;
    public int stateID;

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
        entity.animID = changeAnimID.valueGet;
        AnimDef animFindByID = entity.animListObject.animDef.ToList().Find(x => x.ID == changeAnimID.valueGet);
        //設定されたIDが見つかれば、そのParameterと同様に設定..
        if (animFindByID != null)
        {
            entity.MainAnimMixer.ChangeAnim(animFindByID);
            entity.MainAnimMixer.ChangeAnimParams(entity.animID, animParameter.valueGet);
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
