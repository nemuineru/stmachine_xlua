using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Asset", menuName = "Nemuineru/Create StateDef Asset")]
public class StateDefListObject : ScriptableObject
{
    public List<StateDef> stateDefs = new List<StateDef>(){ new StateDef()};
}
