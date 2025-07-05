using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

public class AnimNodeMain : MonoBehaviour
{
    public Animator animator;

    public AnimlistObject AnimList;
    
    public int ID;
    public string weightName;
    public float weightNum;


    //Animatorに対するアウトプット設定.
    PlayableOutput PrimalPlayableOut;
    MainNodeConfigurator MainAnimMixer = new MainNodeConfigurator();

    
    
    //番号0のアニメーションを最初に割り振る.
    //その後、Entityの指定アニメIDに読み出されたAnimList中のanimDef設定に基づきグラフを錬成する.
    
    private void Start()
    {
        animator = GetComponent<Animator>();

        PrimalPlayableOut = new PlayableOutput();

        MainAnimMixer.SetupGraph(ref animator, ref PrimalPlayableOut);
        
        PrimalPlayableOut.SetSourcePlayable(MainAnimMixer.mixMixer);
        
        ChangeAnim();

        GraphVisualizerClient.Show(MainAnimMixer.PrimalGraph);

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


    int currentID = 0;

    void Update()
    {
        MainAnimMixer.SetAnim(true);
        MainAnimMixer.PrimalGraph.Play();
        //インプットが入力された時..
        /*
        int stickval = (InputCommandBuffer.self.commandBuffer[0].inputs % 10);
        int buttonval = (InputCommandBuffer.self.commandBuffer[0].inputs - stickval) % 10000;
        Debug.Log(buttonval);
        if((0B_00000001  & buttonval/ 10) != 0)
        {
            ChangeAnim();
        }
        */
        if(InputInstance.self.inputValues.MainButton_Read == 10000)
        {
            ChangeAnim();
        }
    }

    public void ChangeAnim()
    {
        currentID = ID;
        AnimDef animFindByID = AnimList.animDef.ToList().Find(x => x.ID == currentID);
        if(animFindByID != null)
        {
            MainAnimMixer.ChangeAnim(animFindByID);
        }
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