
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

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

        public float MixWeight = 1f;



        [SerializeField]
        private AvatarMask _mask;

    //開始時刻を設定.
    public void SetStartTime(float setTime)
    {
        startAnimTime = setTime;
        currentAnimTime = setTime;
        Mixer.SetTime(0);
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

    public bool isEndTime()
    {
        return endAnimTime <= currentAnimTime;
    }

    public bool isEndTimeSet()
    {
        return endAnimTime != Mathf.Infinity;        
    }

    public void SetEnd()
    {
        endAnimTime = currentAnimTime + def.blendOutTime;
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
            SetStartTime(Time.time);
            //最初に設定されたinputWeightに合わせる.
            //Mixer.SetInputWeight(i, 1f / def.animClip.Length);//anims.MixWeightSet);

            i++;
        }
    }
        /*
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
        */

        //アニメのウェイトを自動的に設定する.
        public void changeMixerWeight()
        {
            if(def != null)
            {
                //全体のミキシングの時間を設定する.
                MixWeight = Mathf.Clamp01((currentAnimTime  - startAnimTime) / (Mathf.Epsilon + def.blendInTime)) * 
                Mathf.Clamp01((endAnimTime - currentAnimTime) / (Mathf.Epsilon + def.blendOutTime));
                //Debug.Log(this.def.ID + " MixWeight - " + MixWeight);
                def.ChangeWeight(1f);
                //時間設定.
                for (int i = 0; i < PlayList.Length; i++)
                {
                    var player = PlayList[i];
                    Mixer.SetInputWeight(i, def.animClip[i].MixWeightSet);
                }
            }
        }

        //現アニメーションの時間を現在時刻に設定。
        public void Animations()
        {
            SetCurrentTime(Time.time);
            changeMixerWeight();
            for (int i = 0; i < PlayList.Length; i++)
            {
                //var playClip = def.animClip[i];
                PlayList[i].SetTime(currentAnimTime - startAnimTime);
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
    public MixAnimNode[] Mixers = new MixAnimNode[8];

    public void SetupGraph(ref Animator animator, ref PlayableOutput PrimalPlayableOut)
    {
        //元のグラフを作成.
        PrimalGraph = PlayableGraph.Create("reference");
        //OutPutにanimatorを指定.

        PrimalPlayableOut = AnimationPlayableOutput.Create(PrimalGraph, "Output", animator);
        //初期はMixerの数分のノードのみ.
        MainMixer = AnimationLayerMixerPlayable.Create(PrimalGraph, Mixers.Length);
        
    }

    //アニメ変更時の挙動.
    //defを代入し、リセットする
    public void ChangeAnim(AnimDef def)
    {
        //接続先で、最も小さい値でソケットが空いているものを選択
        int indexOfEmpty = -1;
        MixAnimNode node;
        //定義されていないものがあるならそれを設定する.
        for(int i = 0;i < Mixers.Length ; i++)
        {
            if(Mixers[i] == null)
            {
                indexOfEmpty = i;
                Debug.Log("Mixer Changing to " + def.ID + " as input of " + indexOfEmpty);
                break;
            }
        }
        if (indexOfEmpty >= 0)
        {
            foreach (var m in Mixers)
            {
                if(m != null && !m.isEndTimeSet())
                m.SetEnd();
            }
            Mixers[indexOfEmpty] = new MixAnimNode();
            node = Mixers[indexOfEmpty];
            node.def = def;
            node.CreatePlayerNodes(ref PrimalGraph);

            //接続.
            PrimalGraph.Connect(node.Mixer, 0, MainMixer, indexOfEmpty);
            Debug.Log("Mixer Connected to " + indexOfEmpty);
            SetAnim();
        }
    }

    public void SetupMixer()
    {
        int i = 0;
        
        Mixers[0].CreatePlayerNodes(ref PrimalGraph);
        Debug.Log(Mixers[0].def.animClip[0].Clip.name);
        PrimalGraph.Connect(Mixers[0].Mixer, 0 , MainMixer, 0);
        MainMixer.SetInputWeight(0,1);
        /*
        foreach (var m in Mixers)
        {
            if (m != null)
            {
                i++;
            }
        }
        */
        //MainMixer.AddInput();
    }

    //アニメーションの設定・ミキサーのウェイト設定..
    public void SetAnim()
    {
        int i = 0;
        float All = 0;
        for (int ids = 0; ids < Mixers.Length; ids++)
        {
            if(Mixers[ids] != null)            
            All += Mixers[ids].MixWeight;
        }
        foreach (var m in Mixers)
        {
            if (m != null)
            {
                m.Animations();
                //In-Outの設定を反映させる.
                //Debug.Log(Mixers[i].def.ID + " MixerWeight " + Mixers[i].MixWeight / All);
                MainMixer.SetInputWeight(i, Mixers[i].MixWeight / All);
                if (m.isEndTime())
                {
                    string MixOf = m.def.ID.ToString();
                    PrimalGraph.Disconnect(MainMixer, i);
                    Mixers[i] = null;
                    //Debug.Log("Mixer Erased : " + MixOf);
                }
            }
            i++;
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

    struct clipPosStatement
    {
        internal clipPosStatement(int id, Vector2 p)
        {
            index = id;
            pos = p;
            WeightSet = 1f;
        }
        internal int index;
        internal float WeightSet;
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
        clipPosStatement[] animPos = new clipPosStatement[animClip.Length];
        for (int i = 0; i < animClip.Length; i++)
        {
            animClip[i].MixWeightSet = 0;
            animPos[i].index = i;
            animPos[i].pos = animClip[i].mixPosition;
        }
        animPos = animPos.OrderBy(v => v.pos.SqrMagnitude()).ToArray();
        //原点位置に有るモーションを取得.            
        clipPosStatement center = animPos[0];
        bool hasCenter = center.pos.magnitude < Mathf.Epsilon;


        switch (mixType)
        {
            //Dir1Dならxのみを参照し、その範囲内での距離補正など計算.最大最小はClampで.
            case MixType.Liner:
                {
                    animPos = animPos.OrderBy(v => Mathf.Abs(v.pos.x - CurrentParamPos.x)).ToArray<clipPosStatement>();
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
                //シンプルじゃない..
                case MixType.Simple2D : 
                {
                //角度が0に近い程最上位. 座標0,0が含まれるものは別とする.               
                animPos = animPos.ToList().FindAll(v => v.pos.sqrMagnitude > Mathf.Epsilon)
                .OrderBy(v => Vector2.Angle(v.pos , CurrentParamPos)).ToArray();

                    float x_distAll = 0;
                    float WeightRest = 1;

                    //角度 & 距離に応じた値を考える.
                    //基準が0,0に有るものと考える. 
                    //このため、距離が離れると全体の値を考慮することになる.           
                    foreach (clipPosStatement cl in animPos)
                    {
                        x_distAll += (cl.pos - CurrentParamPos).magnitude;
                    }
                    //角度が最も近いものにおいて、一番目ののVectorをProjectした際の値が1を超えるなら2つPosの角度

                    bool isReached = false;
                    clipPosStatement pos_1 = animPos[0]; 
                    clipPosStatement pos_2 = animPos[1];

                    //三角形の内外判定.
                    Vector2 pos_1To2 = pos_2.pos - pos_1.pos;
                    Vector2 pos_CurTo1 = CurrentParamPos - pos_1.pos;
                    Vector2 pos_CurTo2 = CurrentParamPos - pos_2.pos;

                    float C1 = pos_1To2.x * pos_CurTo2.y - pos_1To2.y * pos_CurTo2.x;
                    float C2 = -(pos_2.pos.x) * CurrentParamPos.y - -(pos_2.pos.y) * CurrentParamPos.x;
                    float C3 = pos_1.pos.x * pos_CurTo1.y - pos_1.pos.y * pos_CurTo1.x;
                    isReached = (C1 >= 0 && C2 >= 0 && C3 >= 0) || 
                                (C1 <= 0 && C2 <= 0 && C3 <= 0); // Vector2.Angle(animPos[0].pos , CurrentParamPos) == 0;

                    //角度で比較
                    if(!isReached)
                    {
                        Debug.Log(string.Format("{0} : {1} : {2}",C1,C2,C3));
                        //Debug.Log("IsReached : On");
                        float angle_0 = Vector2.Angle(animPos[0].pos,CurrentParamPos);
                        float angle_1 = Vector2.Angle(animPos[1].pos,CurrentParamPos);
                        //Debug.Log( animClip[animPos[0].index].Clip.name + " : " + angle_0);
                        //Debug.Log( animClip[animPos[1].index].Clip.name + " : " + angle_1);
                        animClip[animPos[0].index].MixWeightSet = BaseWeight * (angle_1 / ( angle_0 + angle_1));
                        animClip[animPos[1].index].MixWeightSet = BaseWeight * (angle_0 / ( angle_0 + angle_1));
                        break;
                    }
                    //(0,0)が存在するなら最短3角の位置を考慮..
                    //UPDATE : 原点を指定した時、[0] (1 , 0), [1](-1, 0)の時バグる. 多分直線だから..
                    else if(hasCenter)
                    {

                        float angle_0 = Vector2.Angle(animPos[0].pos,CurrentParamPos);
                        float angle_1 = Vector2.Angle(animPos[1].pos,CurrentParamPos);
                        Vector2 line = (animPos[1].pos - animPos[0].pos).normalized;
                        Vector2 retPoint_1 = animPos[0].pos + Vector2.Dot(CurrentParamPos - animPos[0].pos,line) * line;
                        Vector2 retPoint_2 = animPos[0].pos + Vector2.Dot(Vector2.zero - animPos[0].pos,line) * line;
                        float pWeight = (retPoint_1 - CurrentParamPos).magnitude / (retPoint_2).magnitude;
                        //Debug.Log("Weight - " + pWeight + animPos[0].pos + animPos[1].pos);
                        //Debug.Log( animClip[animPos[0].index].Clip.name + " : " + angle_0);
                        //Debug.Log( animClip[animPos[1].index].Clip.name + " : " + angle_1);
                        animClip[center.index].MixWeightSet = BaseWeight * pWeight;
                        animClip[animPos[0].index].MixWeightSet = BaseWeight * (angle_1 / ( angle_0 + angle_1)) * (1 - pWeight);
                        animClip[animPos[1].index].MixWeightSet = BaseWeight * (angle_0 / ( angle_0 + angle_1)) * (1 - pWeight);
                        break;
                    }                    
                    //最後に距離で比較
                    for (int i = 0; i < animClip.Length; i++)
                    {
                        //現在パラメータ位置の距離で比較    
                        float distSelect = (animPos[i].pos - CurrentParamPos).magnitude;
                        float curRest = (1f - distSelect/x_distAll);
                        Debug.Log( animClip[animPos[i].index].Clip.name + " : " + curRest);
                        animClip[animPos[i].index].MixWeightSet = BaseWeight * curRest * WeightRest;
                        WeightRest = Mathf.Clamp01(WeightRest - curRest);
                    }
                    break;
                }
            //極座標での相対値.
            //via Gradient Band Interpolation shader..;                
                case MixType.FreeForm2D:
                {                             
                    float weightAll = 0;
                    int i = 0;
                    foreach (clipPosStatement cl in animPos)
                    {
                        float weight = 1.0f;
                        for (int j = 0; j < animPos.Length; j++)
                        {
                            if(i == j)
                            {
                                continue;
                            }
                        //float point_mag_j = animPos[j].magnitude;
                        }                        
                        i++;
                    }
                    break;
                }

            //直交座標系.
            //via runevision's animation thesis.
                case MixType.Cartesian2D : 
            {
                float weightAll = 0;
                int i = 0;
                foreach (clipPosStatement cl in animPos)
                {
                    var Dir1 = cl.pos - CurrentParamPos;
                    float Weight = 1f;
                    for (int j = 0; j < animPos.Length; j++)
                    {
                        if (j == i)
                        {
                            continue;
                        }
                        var Dir2 = cl.pos - animPos[j].pos;
                        var newWeight = Mathf.Clamp01(1f - (Vector2.Dot(Dir1, Dir2) / Dir2.sqrMagnitude));
                        Weight = Mathf.Min(Weight, newWeight);
                    }                    
                    animPos[i].WeightSet  = Weight;
                    weightAll += animPos[i].WeightSet;
                    Debug.Log(animPos[i].WeightSet);
                    i++;
                }
                    for (int xl = 0; xl < animPos.Length; xl++)
                    {
                        animClip[animPos[xl].index].MixWeightSet =
                        BaseWeight * (animPos[xl].WeightSet / weightAll);
                    }                    
                    break;
            }
            //指定以外のもの
            default:
            {
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
    public float blendInTime, blendOutTime;

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