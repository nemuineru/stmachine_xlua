using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "AnimList",menuName = "CreateAnimList")]
public class AnimlistObject : ScriptableObject
{
    public AnimDef[] animDef;
}