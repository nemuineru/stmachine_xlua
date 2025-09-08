using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Asset", menuName = "MyGenerator/Create Sound Lists")]
public class entitySoundLists : ScriptableObject
{
    [SerializeField]
    public List<AudioClip> audios;
}
