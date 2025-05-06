using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

    //再生ノード組み立て.
    public class MixAnimNode
    {
        //AnimDefに決定された配列と同様のPlayableを作成する.
        //また、Mixer同士でMixを作成することも考える.
        //読み出しDefの名前を保持.
        public AnimDef def;
        public AnimationLayerMixerPlayable Mixer;
        public AnimationClipPlayable[] PlayList = new AnimationClipPlayable[0];
        public string[] ParamName;


        //繋げたAnimの時間設定など. このイベントに応じ、LuaConditionで得られる値も変化する.
        //基本的に、currentAnimTimeをアニメーションの時間の主軸として変更.
        float currentAnimTime = 0;

        float MixWeight = 1f;



        [SerializeField]
        private AvatarMask _mask;

        //初期化.
        public void CreatePlayerNodes(ref PlayableGraph addGraphTo)
        {
            Mixer = AnimationLayerMixerPlayable.Create(addGraphTo, def.animClip.Length);
            //Mixer.SetLayerAdditive(1, true);
            int i = 0;
            //ノードは定義するだけして、再生のときに色々細かく制御する.
            //PlayListを追加..
            foreach (AnimDef.Anims anims in def.animClip)
            {
                i++;
                Array.Resize(ref PlayList, PlayList.Length + 1);
                var players = PlayList[PlayList.Length - 1];

                players =
                AnimationClipPlayable.Create(addGraphTo, anims.Clip);
                //Output先は固定のため0.
                Mixer.ConnectInput(i, players, 0);
                //最初に設定されたinputWeightに合わせる.
                Mixer.SetInputWeight(i, anims.MixWeightSet);
            }
        }
        
        //アニメのウェイトを設定する.
        public void ChangeWeight(ref PlayableGraph addGraphTo, params double[] weight)
        {
            //時間設定.
            for (int i = 0; i < PlayList.Length; i++)
            {
                var player = PlayList[i];
                if (weight.Length < i)
                {
                    Mixer.SetInputWeight(i, (float)weight[i]);
                }
            }
        }

        public void Animations()
        {

            for (int i = 0; i < PlayList.Length; i++)
            {
                var player = PlayList[i];
                var playClip = def.animClip[i];
                player.SetTime((playClip.startFrame + currentAnimTime) % playClip.Clip.length);
            }
        }

        public void ChangeAnimTime(ref PlayableGraph addGraphTo, params double[] time)
        {
            //時間設定.
            for (int i = 0; i < PlayList.Length; i++)
            {
                var player = PlayList[i];
                if (time.Length < i)
                {
                    player.SetTime(time[i]);
                }
            }
        }
        
        //マスクの設定.
        public void ChangeAnimMask(params AvatarMask[] mask)
        {
            //時間設定.
            for (int i = 0; i < PlayList.Length; i++)
            {
                //レイヤーにマスクを掛ける.
                if (mask.Length < i && mask[i] != null)
                {
                    Mixer.SetLayerMaskFromAvatarMask((uint)i, mask[i]);
                }
            }
        }
    }

    public class MainNodeConfigurator
    {
        //PlayableAPIの元グラフ.
        PlayableGraph PrimalGraph;
        //ミックス先のミキサー。メインノードにつなぐため設定する。
        public AnimationLayerMixerPlayable MainMixer = new AnimationLayerMixerPlayable();
        public MixAnimNode[] Mixers;

        public void MakeGraph(ref Animator animator, ref PlayableOutput PrimalPlayableOut)
        {
            //元のグラフを作成.
            PrimalGraph = PlayableGraph.Create("reference");
            //OutPutにanimatorを指定.

            PrimalPlayableOut = AnimationPlayableOutput.Create(PrimalGraph, "Output", animator);
            //初期は4ノードのみ.
            MainMixer = AnimationLayerMixerPlayable.Create(PrimalGraph,4);
        }

        public void MakeMix()
        {
            foreach (var m in Mixers)
            {
                m.CreatePlayerNodes(ref PrimalGraph);
            }
            //MainMixer.AddInput();
        }
    }

public class AnimDef_Game : MonoBehaviour
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

        //初期は1ノードのみ.
        /*
        MainAnimMixer = AnimationMixerPlayable.Create(PrimalGraph,1);     

        //１番目のアニメを最初に割り当て
        PlayList = 
        AnimationClipPlayable.Create(PrimalGraph, AnimList.animDef[0].animClip[0].Clip);

        //MainPlayAnimとPlayListを組み合わせ、出力.
        PrimalGraph.Connect(PlayList,0,MainAnimMixer,0);

        PlayableOut.SetSourcePlayable(MainAnimMixer);

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

//animDefには以下を登録 - 
// アニメーションID, アニメーション名, アニメーションの速度, 
// アニメーションの基本ブレンド（イン・アウト）タイムなど
// また、基礎の当たり判定リスト(カプセル型)を登録.
[System.Serializable]
public class AnimDef
{
    [System.Serializable]

    //ミキシングするアニメのウェイトなど.
    public class Anims
    {
        public Anims(){}
        public Anims(AnimationClip clip) => Clip = clip;
        public AnimationClip Clip;
        public float speed = 1f, startFrame, cycleOffset;
        string MixParamName;

        public float MixWeightSet
        {
            get
            {
                return _MixWeight;
            }
            set
            {
                _MixWeight = value;
            }
        }
        float _MixWeight;
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