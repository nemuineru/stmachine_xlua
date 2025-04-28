using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Asset", menuName = "MyGenerator/CreateScriptableObject")]
public class StateDefListObject : ScriptableObject
{
    public List<StateDef> stateDefs = new List<StateDef>(){ new StateDef()};
}
