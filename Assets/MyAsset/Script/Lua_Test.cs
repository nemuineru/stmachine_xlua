using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class Lua_Test : MonoBehaviour
{
    //table.insertで要素追加..なら
    //利用するStateの指定も簡単かもしれない？
    string LuaScript_1 = 
    @"
        function exampleMeth()
            verd = {}
            table.insert(verd, 1)
            table.insert(verd, 2)
            table.insert(verd, 3)
            return verd
        end
    ";
    
    string LuaScript_2 = 
    @"
        function exampleMeth()
            verd = {}
            table.insert(verd, 7)
            table.insert(verd, 6)
            table.insert(verd, 5)
            table.insert(verd, 4)
            return verd
        end
    ";

    [CSharpCallLua]
    public delegate int[] verd();

    // Start is called before the first frame update
    void Update()
    {
        LuaExecute(LuaScript_1);
        LuaExecute(LuaScript_2);
    }

    void LuaExecute(string stri)
    {        
        Lua_OnLoad.main.LEnv.DoString(stri);
        var exampleMethod = Lua_OnLoad.main.LEnv.Global.Get<verd>("exampleMeth");    
        int[] result = exampleMethod.Invoke();
        foreach (int result2 in result)
        {
            Debug.Log(result2);
        }
    }
}
