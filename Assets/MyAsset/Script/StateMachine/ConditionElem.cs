using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using XLua;

[System.Serializable]
public class lua_Read
{    
    public lua_Read(string insertScript)
    {
        LuaScript = insertScript;
    }
    public string LuaScript;
    
    public LC lua_Condition;
    public Entity entity;

    //QueuedStateIDsは必ずint型のテーブルを返す値として創出.
    //また、Lua内でステートを読み込む前にStateDefParamsに値を事前登録させる.

    [CSharpCallLua]
    public interface CalcValues
    {
        [CSharpCallLua]
        public delegate int[] QueuedStateID(Entity entity);
        
        [CSharpCallLua]
        public delegate object[] luaOutParams(Entity entity);

        public class Parameter
        {
            public int IntParam;
            public float FloatParam;
            public bool BoolParam;
            public string StringParam;
            public Vector3 Vector3Param;
            public Vector2 Vector2Param;
        }
        
    }



    public void Execute()
    {
        
    }


    //返されたい変数によりGeneric変数として返される.
    //The Value Dependency will change via generics  
    /*
    public T Value<T>()
    {            
            Script script = new Script();

            UserData.RegisterAssembly();
            script.Globals["Conditions"] = new Lua_Conditions(entity);
            //Lua中のGlobal関数に設定..
            DynValue val = script.DoString(LuaScript);
            
            if (typeof(T) == typeof(bool))
            {          
                bool item = val.CastToBool();                
                Debug.Log(item);
                return (T)(dynamic)item;
            }
            else if(typeof(T) == typeof(string))
            {                
                return (T)val.CastToString() as String;
            }
            else
            {
                return (T)(dynamic)val.CastToNumber();
            }
    }
    */
}

//Lua_Conditionに登録, 値を設定.
//Entityごとにこの値が設定されると考える.
[LuaCallCSharp]
public class LC
{
    public LC()
    {

    }

    public Vector3 entityPos(Entity et)
    {
        return et.transform.position;
    }

    public Vector3 TargetPos(Entity et)
    {
        return et.targetTo_fw;
    }

    public bool isEntityOnGround(Entity et)
    {
        return et.isOnGround;
    }

    public float CheckStateTime(Entity et)
    {
        //Debug.Log(et.stateTime);
        return et.stateTime;
    }

    public float CheckAnimTime(Entity et)
    {
        //Debug.Log(et.stateTime);
        return et.animationFrameTime;
    }
    
    public float CheckAnimEndTime(Entity et)
    {
        //Debug.Log(et.stateTime);
        return et.animationEndTime;
    }


    public bool CheckButtonPressed(Entity et, string command)
    {
        return et.entityInput.CheckInput(command, 1);
    }

    public int CheckStateDefID(Entity et)
    {
        return et.CurrentStateID;
    }
    
    public int CheckAnimID(Entity et)
    {
        return et.animID;
    }


    //Checker for MainAnimDef's registered list.
    public int CheckAnimationsListNum(Entity et)
    {
        return et.MainAnimMixer.Mixers.Length;
    }
}
