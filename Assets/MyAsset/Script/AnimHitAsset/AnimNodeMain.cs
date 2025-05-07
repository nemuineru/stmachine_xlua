using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Playables;

public class AnimNodeMain : MonoBehaviour
{
    public Animator animator;

    public AnimlistObject AnimList;
    
    public int ID;
    public string weightName;
    public float weightNum;


    //Animatorに対するアウトプット設定.
    PlayableOutput PrimalPlayableOut;
    MainNodeConfigurator MainAnimMixer;

    
    
    //番号0のアニメーションを最初に割り振る.
    //その後、Entityの指定アニメIDに読み出されたAnimList中のanimDef設定に基づきグラフを錬成する.
    
    private void Start()
    {
        animator = GetComponent<Animator>();

        MainAnimMixer.MakeGraph(ref animator, ref PrimalPlayableOut);

        MainAnimMixer.Mixers[0] = new MixAnimNode();

        MainAnimMixer.Mixers[0].def = AnimList.animDef[0];

        PrimalPlayableOut.SetSourcePlayable(MainAnimMixer.MainMixer);

        //初期は1ノードのみ.
        /*
        MainAnimMixer = AnimationMixerPlayable.Create(PrimalGraph,1);     

        //１番目のアニメを最初に割り当て
        PlayList = 
        AnimationClipPlayable.Create(PrimalGraph, AnimList.animDef[0].animClip[0].Clip);

        //MainPlayAnimとPlayListを組み合わせ、出力.
        PrimalGraph.Connect(PlayList,0,MainAnimMixer,0);


        */
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
