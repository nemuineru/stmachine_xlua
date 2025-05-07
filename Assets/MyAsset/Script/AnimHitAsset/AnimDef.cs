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

        }
    }

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
    public MixAnimNode[] Mixers = new MixAnimNode[4];

    public void MakeGraph(ref Animator animator, ref PlayableOutput PrimalPlayableOut)
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
                PrimalGraph.Connect(MainMixer, i, m.Mixer, 0);
                i++;
            }
        }
        //MainMixer.AddInput();
    }

        public void SetTime()
        { 
            
        }
    }

public class AnimDef_Game : MonoBehaviour
{

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