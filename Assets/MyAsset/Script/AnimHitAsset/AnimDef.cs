using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


public class AnimDef_Game : MonoBehaviour
{
    //再生元
    public class MixAnimNode
    {
        //AnimDefに決定された配列と同様のPlayableを作成する.
        public AnimDef def;
        public AnimationMixerPlayable Mixer;
        public AnimationClipPlayable[] PlayList;

        //繋げたAnimの時間設定など. このイベントに応じ、LuaConditionで得られる値も変化する.
        float currentAnimTime = 0;

    
    }

    public Animator animator;
    GameObject refObj;

    public AnimlistObject AnimList;
    
    public int ID;

    public string weightName;
    public float weightNum;

    //原初のグラフ.
    PlayableGraph PrimalGraph;

    //Animatorに対するアウトプット設定.
    PlayableOutput PlayableOut;
    AnimationMixerPlayable MainPlayAnim;
    AnimationClipPlayable PlayList;

    //一先ず8つ登録.
    MixAnimNode[] newAnimNode = new MixAnimNode[8];

    
    
    //番号0のアニメーションを最初に割り振る.
    //その後、Entityの指定アニメIDに読み出されたAnimList中のanimDef設定に基づきグラフを錬成する.
    
    private void Start()
    {
        animator = GetComponent<Animator>();

        newAnimNode[0] = new MixAnimNode();
        newAnimNode[0].def = AnimList.animDef[0];

        //元のグラフを作成.
        PrimalGraph = PlayableGraph.Create("reference");

        //OutPutにanimatorを指定.

        PlayableOut = AnimationPlayableOutput.Create(PrimalGraph, "Output", animator);

        //初期は1ノードのみ.
        MainPlayAnim = AnimationMixerPlayable.Create(PrimalGraph,1);     

        //１番目のアニメを最初に割り当て
        PlayList = 
        AnimationClipPlayable.Create(PrimalGraph, AnimList.animDef[0].animClip[0].Clip);

        //MainPlayAnimとPlayListを組み合わせ、出力.
        PrimalGraph.Connect(PlayList,0,MainPlayAnim,0);

        PlayableOut.SetSourcePlayable(MainPlayAnim);

        
    }

    //Animのウェイトを設定値より決定する.
    private void AnimWeight(float weight)
    {
        AnimDef SelectedDef = Array.Find(AnimList.animDef, 
        item => { return item.ID == ID;}); 
        if(SelectedDef != null)
        {

        }
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
