using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


public class AnimDef_Game : MonoBehaviour
{
    GameObject refObj;

    public AnimlistObject AnimList;

    PlayableGraph PlayableAnimGraph;
    AnimationClipPlayable PlayList;

    //番号0のアニメーションを最初に割り振る.
    //その後、Entityの指定アニメIDに読み出されたAnimList中のanimDef設定に基づきグラフを錬成する.
    private void Start()
    {
        PlayableAnimGraph = PlayableGraph.Create("reference");

        PlayList = 
        AnimationClipPlayable.Create(PlayableAnimGraph, AnimList.animDef[0].animClip[0].Clip);
        
    }
}

//animDefには以下を登録 - 
// アニメーションID, アニメーション名, アニメーションの速度, 
// アニメーションの基本ブレンド（イン・アウト）タイムなど
// また、基礎の当たり判定リスト(カプセル型)を登録.
[System.Serializable]
public class AnimDef
{
    [System.Serializable]

    //ミキシングするアニメのウェイトなど.
    public struct Anims
    {        
        public AnimationClip Clip;
        float speed, startFrame, loopsFromFrame;

        string MixParamName;
        float MixWeight;
    }

    public AnimatorControllerLayer playLayer;
    public int ID;
    public string Name;

    public Anims[] animClip;   
    float blendInTime, blendOutTime;

    float DefWeight = 1f;

    clssDef[] clssDefs = new clssDef[0];

}

[System.Serializable]
class clssDef
{
    public enum ClssType
    {
        Hit,
        Attack
    }
    public ClssType clssType;
    //対象キャラクターのどのゲームオブジェクトに追随するか？
    public string attachTo;    

    //wを半径とする.
    Vector3 startPos, endPos;
    float w;

    float StartTime = 0f, EndTime = 10f;
}
