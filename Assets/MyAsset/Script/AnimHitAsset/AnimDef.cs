using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Linq;

    //再生ノード組み立て.
    public class MixAnimNode
    {
        //AnimDefに決定された配列と同様のPlayableを作成する.
        //また、Mixer同士でMixを作成することも考える.
        //読み出しDefの名前を保持.
        public AnimDef def;
        public AnimationMixerPlayable Mixer;
        public AnimationClipPlayable[] PlayList = new AnimationClipPlayable[0];
        public string[] ParamName;


        //繋げたAnimの時間設定など. このイベントに応じ、LuaConditionで得られる値も変化する.
        //基本的に、currentAnimTimeをアニメーションの時間の主軸として変更.
        float startAnimTime = 0f, currentAnimTime = 0f, endAnimTime = Mathf.Infinity;

        float MixWeight = 1f;



        [SerializeField]
        private AvatarMask _mask;

    //開始時刻を設定.
    public void SetStartTime(float setTime)
    {
        startAnimTime = setTime;
        currentAnimTime = setTime;
    }

    //停止時刻を設定.
    public void SetCurrentTime(float setTime)
    {
        currentAnimTime = setTime;
        if (endAnimTime < currentAnimTime)
        {
            currentAnimTime = endAnimTime;
        }
    }

        //初期化.
    public void CreatePlayerNodes(ref PlayableGraph addGraphTo)
    {
        Mixer = AnimationMixerPlayable.Create(addGraphTo, def.animClip.Length);
        int i = 0;
        //ノードは定義するだけして、再生のときに色々細かく制御する.
        //PlayListを追加..
        foreach (AnimDef.Anims anims in def.animClip)
        {
            Array.Resize(ref PlayList, PlayList.Length + 1);
            //Mixer.SetLayerAdditive((uint)i, true);

            PlayList[PlayList.Length - 1] =
            AnimationClipPlayable.Create(addGraphTo, anims.Clip);
            
            
            //Output先は固定のため0.
            Mixer.ConnectInput(i, PlayList[PlayList.Length - 1], 0);
            //最初に設定されたinputWeightに合わせる.
            Mixer.SetInputWeight(i, 1f / def.animClip.Length);//anims.MixWeightSet);

            SetStartTime(Time.time);
            i++;
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

        //現アニメーションの時間を現在時刻に設定。
        public void Animations()
        {
            SetCurrentTime(Time.time);
            for (int i = 0; i < PlayList.Length; i++)
            {
                //var playClip = def.animClip[i];
                PlayList[i].SetTime(currentAnimTime);
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
                    //Mixer.SetLayerMaskFromAvatarMask((uint)i, mask[i]);
                }
            }
        }
    }

public class MainNodeConfigurator
{
    //PlayableAPIの元グラフ.
    public PlayableGraph PrimalGraph;
    //ミックス先のミキサー。メインノードにつなぐため設定する。
    public AnimationLayerMixerPlayable MainMixer = new AnimationLayerMixerPlayable();
    public MixAnimNode[] Mixers = new MixAnimNode[4];

    public void SetupGraph(ref Animator animator, ref PlayableOutput PrimalPlayableOut)
    {
        //元のグラフを作成.
        PrimalGraph = PlayableGraph.Create("reference");
        //OutPutにanimatorを指定.

        PrimalPlayableOut = AnimationPlayableOutput.Create(PrimalGraph, "Output", animator);
        //初期は4ノードのみ.
        MainMixer = AnimationLayerMixerPlayable.Create(PrimalGraph, 4);
        
    }

    public void MakeMix()
    {
        int i = 0;
        foreach (var m in Mixers)
        {
            if (m != null)
            {
                m.CreatePlayerNodes(ref PrimalGraph);
                PrimalGraph.Connect(m.Mixer, 0 , MainMixer, i);
                MainMixer.SetInputWeight(i,1);
                i++;
            }
        }
        //MainMixer.AddInput();
    }

    public void SetAnim()
    { 
        int i = 0;
        foreach (var m in Mixers)
        {
            if (m != null)
            {
                m.Animations();
            }
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
        public Vector2 mixPosition = Vector2.zero;

        public AnimationClipPlayable clipPlayable;

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
        float _MixWeight = 1f;
    }

    struct Clips
    {
        internal Clips(int id, Vector2 p)
        {
            index = id; 
            pos = p;
        }
        internal int index;
        internal Vector2 pos;   
    }

    public void ChangeWeight(float BaseWeight)
    {
        //AnimClipが1以下ならWeightは1のまま.
        if (animClip.Length <= 1)
        {
            animClip[0].MixWeightSet = BaseWeight;
            return;
        }

        //インデックス値などを登録.
        //この時、全ウェイトを0に設定.
        Clips[] animPos = new Clips[animClip.Length];
        for (int i = 0; i < animClip.Length; i++)
        {
            animClip[i].MixWeightSet = 0;
            animPos[i].index = i;
            animPos[i].pos = animClip[i].mixPosition;
        }

        switch (mixType)
        {
            //Dir1Dならxのみを参照し、その範囲内での距離補正など計算.最大最小はClampで.
            case MixType.Liner:
                {
                    animPos = animPos.OrderBy(v => Mathf.Abs(v.pos.x - CurrentParamPos.x)).ToArray<Clips>();
                    float x_0 = (animPos[0].pos - CurrentParamPos).x;
                    float x_1 = (animPos[1].pos - CurrentParamPos).x;

                    float x_distAll = (Mathf.Abs(x_0) + Mathf.Abs(x_1));

                    //ソート時のanimpos上位ふたつの値が++ or --で無いならその2つの距離に応じた値で,
                    //そうでない場合はindex[0]の値を考慮. 合計値 = 0なら最上位で対応可能.
                    if (x_0 * x_1 >= 0)
                    {
                        animClip[animPos[0].index].MixWeightSet = BaseWeight;
                    }
                    //もし違うなら、それぞれの距離に対しての値を考慮.
                    else
                    {
                        animClip[animPos[0].index].MixWeightSet = BaseWeight * Mathf.Abs((x_1) / x_distAll);
                        animClip[animPos[1].index].MixWeightSet = BaseWeight * Mathf.Abs((x_0) / x_distAll);
                    }
                    break;
                }
                
                case MixType.Simple2D :
                {
                    //至近距離の合計 + project.
                    animPos = animPos.OrderBy(v => (v.pos - CurrentParamPos).magnitude).ToArray<Clips>();
                    float x_distAll = 0;
                    float WeightRest = 1;

                    foreach (Clips cl in animPos)
                    {
                        x_distAll += (cl.pos - CurrentParamPos).magnitude;
                    }                    
                    for (int i = 0; i < animClip.Length; i++)
                    {
                        float distSelect = (animPos[i].pos - CurrentParamPos).magnitude;
                        animClip[animPos[i].index].MixWeightSet = BaseWeight * (1 - distSelect/x_distAll);
                        break;
                    }                    
                    break;
                }
                
            case MixType.FreeForm2D : 
            {
            //角度が0に近い程最上位. 座標0,0が含まれるものは(最下位)とする
                animPos = animPos.OrderBy(v => Vector3.Angle(v.pos , CurrentParamPos)).ToArray<Clips>();

                float x_distAll = 0;
                float WeightRest = 1;

                //角度 & 距離に応じた値を考える.
                //基準が0,0に有るものと考え,オプションに座標0,0にアニメーションが含まれることを許可. 
                //角度90度までの値を考える.
                //このため、距離が離れると全体の値を考慮することになる.           
                foreach (Clips cl in animPos)
                {
                    x_distAll += (cl.pos - CurrentParamPos).magnitude;
                }
                //距離が最も近い4つを対象に.
                
                for (int i = 0; i < animClip.Length; i++)
                {
                    float distSelect = (animPos[i].pos - CurrentParamPos).magnitude;
                    animClip[animPos[i].index].MixWeightSet = BaseWeight * (WeightRest);
                }
                break;
            }
        }
    }

    public enum MixType
    { 
        Liner,
        Simple2D,
        FreeForm2D,
        Cartesian2D
    }

    public MixType mixType = MixType.Liner;
    public string MixParamName;
    public Vector2 CurrentParamPos;

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