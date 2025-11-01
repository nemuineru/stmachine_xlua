using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Linq;
//using UnityEditor.Experimental.GraphView;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using Unity.VisualScripting;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityDebug;

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
        internal float startAnimTime = 0f, currentAnimTime = 0f, endAnimTime = Mathf.Infinity;

        public float MixWeight = 1f;

        public bool isAdditive;



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

    public void SetEnd(float offset)
    {
        endAnimTime = currentAnimTime + def.blendOutTime;
        currentAnimTime += offset;
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
            //ApplyFootIKを自動的にoffにする. これ、どうやら不具合の原因ぽそう.
            PlayList[PlayList.Length - 1].SetApplyFootIK(false);
            
            
            //Output先は固定のため0.
            Mixer.ConnectInput(i, PlayList[PlayList.Length - 1], 0);

            //startTimeを0に.            
            SetStartTime(0f);
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
            //currentAnimTimeが0の時、wgが0となるので..
            float InTime = Mathf.Clamp01((Mathf.Epsilon + currentAnimTime - startAnimTime) / (Mathf.Epsilon + def.blendInTime));
            float OutTime = Mathf.Clamp01((endAnimTime - currentAnimTime) / (Mathf.Epsilon + def.blendOutTime));
            float wg = InTime * OutTime;
            //Debug.Log(this.def.ID + " WeightTime : " + wg);
            //全体のミキシングの時間を設定する.
            MixWeight = wg;
            // デバッグ表示 - ミックスウェイト値の表示
            //Debug.Log(this.def.ID + " Time Calclation MixWeight - " + MixWeight + " calc As " + InTime + "," + OutTime );
            
            
            // AnimDef中のWeight値は1で固定とする.
            // MixWeightの時間を考慮してアニメーションの設定をしたいが、
            // ロード開始時に屈みポーズのままからスタートするので修正が必要..
            // おそらくはwgの値が0となっている際が原因だろうか
            
            def.ChangeWeight(1f);

            //defで設定したanimclipのMixWeightをここで指定する.
            for (int i = 0; i < PlayList.Length; i++)
            {
                var player = PlayList[i];
                Mixer.SetInputWeight(i, def.animClip[i].MixWeightSet);
                //Debug.Log(this.def.ID + " MixWeight - " + def.animClip[i].MixWeightSet);
            }
        }
    }

        //現アニメーションの時間を現在時刻に設定。
        public void Animations(bool TickDef)
        {
        //Time.timeだとpauseが聞かないため..
            if(TickDef)
            {
                SetCurrentTime(currentAnimTime + Time.fixedDeltaTime);
            }
            changeMixerWeight();
            for (int i = 0; i < PlayList.Length; i++)
            {
            //var playClip = def.animClip[i];
                float f = currentAnimTime - startAnimTime;
            Debug.Log("time - " + f);
                PlayList[i].SetTime(f);
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
    //MainAnimNodeが使うRootTransform
    //ここは必ず入れる..
    //setupGraphで自動的に適応
    public Entity root;

    //PlayableAPIの元グラフ.
    public PlayableGraph PrimalGraph;
    //ミックス先のミキサー。メインノードにつなぐため設定する。
    public AnimationLayerMixerPlayable mixMixer = new AnimationLayerMixerPlayable();

    //ミキシングされるアニメーションDef/アニメーションノード
    public MixAnimNode[] Mixers = new MixAnimNode[8];
    MixAnimNode MainMixer;

    //メインDef・アニメーションのノード.
    public AnimDef MainAnimDef;

    public void SetupGraph(ref Animator animator, ref PlayableOutput PrimalPlayableOut)
    {
        //元のグラフを作成.
        PrimalGraph = PlayableGraph.Create("reference");
        //OutPutにanimatorを指定.

        //rootにEntityを指定する.
        //このコンポーネントが呼び出す際animatorの付いた
        // gameobjectはEntityが存在するものとして扱っているが,
        //頭悪いなぁ..
        root = animator.gameObject.GetComponent<Entity>();

        PrimalPlayableOut = AnimationPlayableOutput.Create(PrimalGraph, "Output", animator);
        //初期はMixerの数分のノードのみ.
        mixMixer = AnimationLayerMixerPlayable.Create(PrimalGraph, Mixers.Length, false);
    }

    //AnimIDで検索し、アニメーションのパラメータを変更する.
    public void ChangeAnimParams(int AnimID, Vector2 paramSet)
    {
        for (int i = 0; i < Mixers.Length; i++)
        {
            if (Mixers[i] != null && Mixers[i].def.ID == AnimID)
            {
                Mixers[i].def.CurrentParamPos = paramSet;
            }
        }
    }

    //アニメ変更時の挙動.
    //defを代入し、リセットする.
    //この時、MainAnimは代数に変更.
    public void ChangeAnim(AnimDef def, bool AdditiveIsTrue = false, float timeoffset = 0.0f)
    {
        //接続先で、最も小さい値でソケットが空いているものを選択
        int indexOfEmpty = -1;
        MixAnimNode node;
        //定義されていないものがあるならそれを設定する.
        for (int i = 0; i < Mixers.Length; i++)
        {
            if (Mixers[i] == null)
            {
                indexOfEmpty = i;
                //Debug.Log("Mixer Changing to " + def.ID + " as input of " + indexOfEmpty);
                break;
            }
        }
        if (indexOfEmpty >= 0)
        {
            foreach (var m in Mixers)
            {
                if (m != null && !m.isEndTimeSet())
                {
                    m.SetEnd(timeoffset);
                }
            }
            Mixers[indexOfEmpty] = new MixAnimNode();
            node = Mixers[indexOfEmpty];
            node.isAdditive = AdditiveIsTrue;

            //DeepCopy defs, so that the Entity itself won't change the property of these.
            AnimDef cDef = def.Clone();

            node.def = cDef;
            node.CreatePlayerNodes(ref PrimalGraph);

            //init Entity on new Connection. Dont Forget it!
            node.def.initEntityAt(root);
            node.SetCurrentTime(timeoffset);
            MainMixer = node;
            MainAnimDef = node.def;
            //Additiveに設定するか？
            mixMixer.SetLayerAdditive((uint)indexOfEmpty, AdditiveIsTrue);

            //接続.
            PrimalGraph.Connect(node.Mixer, 0, mixMixer, indexOfEmpty);
            //Debug.Log("Mixer Connected to " + indexOfEmpty);
        }
    }

    public float CurrentAnimTime()
    {
        float val = 0;
        if (MainMixer != null)
        {
            val = MainMixer.currentAnimTime / (1 / MainAnimDef.animClip.Average(x => x.Clip.frameRate));
        }
        return val;
    }
    
    public float EndAnimTime()
    {
        float val = 0;
        if (MainMixer != null)
        {
            val = MainAnimDef.animClip.Average(x => x.Clip.length) / (1f / MainAnimDef.animClip.Average(x => x.Clip.frameRate));
            //Debug.Log(val);
        }
        return val;
    }

    public void Tick()
    {
        foreach (MixAnimNode mix in Mixers)
        {
            mix.Animations(true);
        }
    }

    //アニメーションの設定・ミキサーのウェイト設定..
    /*
    public void ___SetAnim()
    {
        int i = 0;
        float All = 0;
        for (int ids = 0; ids < Mixers.Length; ids++)
        {
            if (Mixers[ids] != null && !Mixers[ids].isEndTime())
            {
                Debug.Log(ids + " of MixerWeight " + Mixers[ids].MixWeight);
                All += Mixers[ids].MixWeight;
            }
        }
        foreach (var m in Mixers)
        {
            if (m != null)
            {
                //あれ,なんでこんな所にanimMixerを変えるスクリプトが付いてるんだ...?
                m.Animations();
                //In-Outの設定を反映させる..はず.
                //現段階だと、1 / 全部のMIXERの数となってるため、In-Out割合を考慮して取得する必要がある..
                float WeightSet = All != 0 ? Mixers[i].MixWeight / All : 1f;
                Debug.Log(Mixers[i].def.ID + " MixerWeight " + WeightSet);
                mixMixer.SetInputWeight(i, WeightSet);
                if (m.isEndTime())
                {
                    string MixOf = m.def.ID.ToString();
                    PrimalGraph.Disconnect(mixMixer, i);
                    Mixers[i] = null;
                    //Debug.Log("Mixer Erased : " + MixOf);
                }
            }
            i++;
        }
        MainAnimDef.initEntityAt(root);
        MainAnimDef.checkClssCapsule();
    }
    */

    //修正したい.
    //何かパラメータ値が足りないのだろうか。　トランジションの間に
    //変な屈みポーズが見られる.
    //Additiveに設定されていないなら..という感じだろうか?
    public void SetAnim(bool TickDef)
    {
        int i = 0;
        float All = 0;
        float Max = 0;
        foreach (var m in Mixers)
        {
            if (m != null)
            {
                m.Mixer.Play();
                //mixerのアニメーションを先ず更新
                //EndTimeに到達しているならAllから外す.
                m.Animations(TickDef);
                if (m.isEndTime())
                {
                    string MixOf = m.def.ID.ToString();
                    PrimalGraph.Disconnect(mixMixer, i);
                    Mixers[i] = null;
                    //Debug.Log("Mixer Erased : " + MixOf);
                }
                else
                {
                    if (m.isAdditive != true)
                    {
                        All += m.MixWeight;
                        Max = Mathf.Max(Max, m.MixWeight);
                    }
                }
                m.Mixer.Pause();
            }
            i++;
        }
        float F = 0;

        for (int ids = 0; ids < Mixers.Length && Mixers.Length != 0; ids++)
        {
            if (Mixers[ids] != null)
            {
                float Selected = Mixers[ids].MixWeight;
                float WeightSet = All > 0 ? Selected / Max : 1f;
                if (Mixers[ids].isAdditive)
                {
                    WeightSet = Selected;
                }
                //Debug.Log(string.Format("ID {0} - All : {1} - MixWeight {2}", Mixers[ids].def.ID, All, WeightSet));
                mixMixer.SetInputWeight(ids, WeightSet);
                F += WeightSet;
                //Debug.Log(F);
            }
        }
        MainAnimDef.initEntityAt(root);
        MainAnimDef.checkClssCapsule();
    }   
}




//animDefには以下を登録 - 
// アニメーションID, アニメーション名, アニメーションの速度, 
// アニメーションの基本ブレンド（イン・アウト）タイムなど
// また、基礎の当たり判定リスト(カプセル型)を登録.
// 2025-06-12
// clssの表示設定もやらねば.
[System.Serializable]
public class AnimDef
{
    [System.Serializable]

    //ミキシングするアニメのウェイトなど.
    public class Anims
    {
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
            Angle = 0f;
        }
        internal int index;
        internal float WeightSet;
        internal Vector2 pos;

        internal float Angle;
    }

    public void ChangeWeight(float BaseWeight)
    {
        //Debug.Log("Base Weight is " + BaseWeight);
        //AnimClipが1以下ならWeightは1のまま.
        if (animClip.Length <= 1)
        {
            animClip[0].MixWeightSet = BaseWeight;
            return;
        }

        //インデックス値などを登録.
        clipPosStatement[] animPos = new clipPosStatement[animClip.Length];
        for (int i = 0; i < animClip.Length; i++)
        {
            animClip[i].MixWeightSet = 0;
            animPos[i].index = i;
            animPos[i].pos = animClip[i].mixPosition;
            float angle = Vector2.SignedAngle(animPos[i].pos, Vector2.up);
            angle = angle >= 0 ? angle : 360 + angle;
            animPos[i].Angle = angle;
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

            //原点を基準に、角度が近い線分同士との原点との距離を求める.
            //現状シンプルじゃないので考える...
            case MixType.Simple2D:
                {
                    //角度を0基準でソート.  0でないものを選択..             
                    animPos = animPos.ToList().FindAll(v => v.pos.sqrMagnitude > Mathf.Epsilon)
                    .OrderBy(v => v.Angle).ToArray();

                    float x_distAll = 0;
                    float WeightRest = 1;

                    //角度 & 距離に応じた値を考える.
                    //基準が0,0に有るものと考える. 
                    //このため、距離が離れると全体の値を考慮することになる.           
                    foreach (clipPosStatement cl in animPos)
                    {
                        x_distAll += (cl.pos - CurrentParamPos).magnitude;

                        //Debug.Log(cl.Angle);
                    }
                    //角度が最も近いものにおいて、一番目ののVectorをProjectした際の値が1を超えるなら2つPosの角度

                    bool isReached = false;
                    float curPosAngle = Vector2.SignedAngle(CurrentParamPos, Vector2.up);

                    clipPosStatement pos_1 = animPos[0];
                    clipPosStatement pos_2 = animPos[1];

                    curPosAngle = curPosAngle >= 0 ? curPosAngle : 360 + curPosAngle;


                    for (int i = 0; i < animPos.Length; i++)
                    {
                        //Max値ならそのまま
                        if (i == animPos.Length - 1)
                        {
                            pos_1 = animPos[i];
                            pos_2 = animPos[0];
                            break;
                        }
                        if (animPos[i].Angle <= curPosAngle && animPos[i + 1].Angle >= curPosAngle)
                        {
                            pos_1 = animPos[i];
                            pos_2 = animPos[i + 1];
                            break;
                        }
                    }

                    //2つの線分の交差判定(原点 -> 基準点 X パラメータ1点 - パラメータ2点)
                    Vector2 vector1 = CurrentParamPos;
                    Vector2 vector2 = pos_2.pos - pos_1.pos;

                    isReached = (Cross(vector1, pos_1.pos - Vector2.zero) * Cross(vector1, pos_2.pos - Vector2.zero) < 0) &&
                    (Cross(vector2, Vector2.zero - pos_1.pos) * Cross(vector2, CurrentParamPos - pos_1.pos) < 0);

                    //isReachedがtrueなら、計算点は最接近した三角形の外にあると考える
                    //また、角度で比較
                    if (isReached)
                    {
                        //Debug.Log("IsReached : On");
                        float angle_0 = Vector2.Angle(pos_1.pos, CurrentParamPos);
                        float angle_1 = Vector2.Angle(pos_2.pos, CurrentParamPos);
                        //Debug.Log( animClip[animPos[0].index].Clip.name + " : " + angle_0);
                        //Debug.Log( animClip[animPos[1].index].Clip.name + " : " + angle_1);
                        animClip[pos_1.index].MixWeightSet = BaseWeight * (angle_1 / (angle_0 + angle_1));
                        animClip[pos_2.index].MixWeightSet = BaseWeight * (angle_0 / (angle_0 + angle_1));
                        break;
                    }
                    //交点が存在しない時..
                    //(0,0)が存在するなら最短3角の位置を考慮..
                    //UPDATE : 原点を指定した時、[0] (1 , 0), [1](-1, 0)の時バグる. 多分直線だから..
                    else if (hasCenter)
                    {
                        //Debug.Log("IsReached : Off");
                        float angle_0 = Vector2.Angle(pos_1.pos, CurrentParamPos);
                        float angle_1 = Vector2.Angle(pos_2.pos, CurrentParamPos);

                        float angleWeight_All = angle_0 + angle_1;
                        float angleWeight_0 = angle_0 / angleWeight_All;
                        float angleWeight_1 = angle_1 / angleWeight_All;

                        //Debug.Log(angleWeight_All);

                        //線分との垂直距離
                        Vector2 line = (pos_1.pos - pos_2.pos);
                        Vector2 animPos1_2PerpPos = PerpendicularFootPoint(pos_1.pos, pos_2.pos, CurrentParamPos);
                        Vector2 animPosBasePerpPos = PerpendicularFootPoint(pos_1.pos, pos_2.pos, Vector2.zero);
                        float Dist_PT = Vector2.Distance(animPos1_2PerpPos, CurrentParamPos);
                        float Dist_Cer = Vector2.Distance(animPosBasePerpPos, Vector2.zero);

                        //Debug.Log(Dist_PT + ":" + "a" + Dist_Cer);

                        float pAllWeight = Dist_PT / Dist_Cer;
                        //Debug.Log("Weight - " + pWeight + animPos[0].pos + animPos[1].pos);
                        //Debug.Log( animClip[animPos[0].index].Clip.name + " : " + angle_0);
                        //Debug.Log( animClip[animPos[1].index].Clip.name + " : " + angle_1);
                        animClip[center.index].MixWeightSet = BaseWeight * pAllWeight;
                        animClip[pos_1.index].MixWeightSet = BaseWeight * (angle_1 / (angle_0 + angle_1)) * (1 - pAllWeight);
                        animClip[pos_2.index].MixWeightSet = BaseWeight * (angle_0 / (angle_0 + angle_1)) * (1 - pAllWeight);
                        break;
                    }
                    //最後に距離で比較
                    for (int i = 0; i < animClip.Length; i++)
                    {
                        //現在パラメータ位置の距離で比較    
                        float distSelect = (animPos[i].pos - CurrentParamPos).magnitude;
                        float curRest = (1f - distSelect / x_distAll);
                        //Debug.Log(animClip[animPos[i].index].Clip.name + " : " + curRest);
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
                            if (i == j)
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
            case MixType.Cartesian2D:
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
                        animPos[i].WeightSet = Weight;
                        weightAll += animPos[i].WeightSet;
                        //Debug.Log(animPos[i].WeightSet);
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

        // 2次元ベクトルの外積を返す
        float Cross(Vector2 vector1, Vector2 vector2)
        {
            return vector1.x * vector2.y - vector1.y * vector2.x;
        }

        // 点Pから直線ABに下ろした垂線の足の座標を返す
        Vector2 PerpendicularFootPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = (b - a).normalized;
            return a + Vector2.Dot(p - a, ab) * ab;
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

    //public AnimatorControllerLayer playLayer;
    public int ID;
    public string Name;

    public Anims[] animClip;
    public float blendInTime, blendOutTime;

    float DefWeight = 1f;

    public bool useDefaultClss;    

    //アニメーションごとにオーバーライド・設定可能な判定をここで設定する.
    //null値が挿入されているならデフォルトを使用..と考える.
    [SerializeField]
    public clssSetting clssSetting = new clssSetting();


    //clsssettingにEntityを登録.
    public void initEntityAt(Entity init)
    {
        clssSetting.initClss(init);
    }

    //カプセルの描写
    public void checkClssCapsule()
    {
        clssSetting.clssPosUpdate();
        List<clssDef> ca = clssSetting.findclss(clssDef.ClssType.Hit, 0f);
        ca.AddRange(clssSetting.findclss(clssDef.ClssType.Attack, 0f));
        
        foreach (clssDef c in ca)
        {
            //Debug.Log("clssCapsule name : " + c.attachTo);
            //Debug.Log(c.attachTo);
            c.getGlobalPos();
            c.DrawCapsule();
            if (c.showGizmo == true)
            {
                if (c.clssType == clssDef.ClssType.Attack)
                {
                    //Debug.Log("attackCapsule");
                }
                //c.DrawCapsule();
            }
        }
    }

    //Cloneメソッドの拡張..
    public AnimDef Clone() => new AnimDef
    {
        //AnimClipはそのまま..
        animClip = this.animClip,
        mixType = this.mixType,
        MixParamName = this.MixParamName,
        ID = this.ID,
        Name = this.Name,
        blendInTime = this.blendInTime,
        blendOutTime = this.blendOutTime,
        useDefaultClss = this.useDefaultClss,
        clssSetting = this.clssSetting.Clone()
    };
}


[System.Serializable]
public class clssSetting
{
    //アニメーションごとにオーバーライド・設定可能な判定をここで設定する.
    //ここで記述されたclssは調整済みのデフォルトに挿入する形で設定される
    //null値が挿入されているならなにもしない.
    public List<clssDef> clssDefs = new List<clssDef>();

    //デフォルトClssを除去するためのリスト. 
    //除去指定をデフォルトのClssのオブジェ名称から消す.
    public List<string> disableClssList = new List<string>();
    public Entity root;

    //投げとかステート奪取時の挙動用. 一時的に当たり判定を除去する.
    public bool eraseAllClss;

    //entityで読み出すclssにentityを用意する.
    public void initClss(Entity entity)
    {
        root = entity;
        foreach (clssDef c in clssDefs)
        {
            c._entity = root;
            c.initTransform(c._entity);
        }
    }

    public void clssPosUpdate()
    {
        foreach (clssDef cls in clssDefs)
        {
            cls.updatelastPos();
        }
    }

    //使用する当たり判定を用意する.
    public List<clssDef> findclss(clssDef.ClssType useType, float frame)
    {
        //clssDef内のclssを用意.
        List<clssDef> findDefs = new List<clssDef>();
        foreach (clssDef cls in clssDefs)
        {
            if (cls.StartTime <= frame || cls.EndTime > frame && cls.clssType == useType)
            {
                findDefs.Add(cls);
            }
        }
        if (root != null)
        {
            //Entity内のclssを用意. 除外対象をdisableClssから検索して削除.
            //AnimClipのEraseAllClssが設定されているなら消し飛ばす.
            foreach (clssDef defaultcls in root.defaultClss.clssDefs)
                if (defaultcls.clssType == useType &&
                !disableClssList.Any(dz => dz == defaultcls.attachTo)
                && eraseAllClss == false)
                {
                    findDefs.Add(defaultcls);
                }
        }
        //初期ロード時rootが登録されていない. 何故だろう？
        else
        {
            Debug.Log("cant find default");
        }
        return findDefs;
    }

    //clss同士の接触判定を行う.
    public bool clssCollided(out Vector3 v1, out Vector3 v2, out float dist, clssDef.ClssType useType,
    clssSetting compareTo, float frame)
    {
        //clssRef <= 呼び出し側の比較するclssカプセル.
        List<clssDef> clssRef = new List<clssDef>();
        //clssCompareTO <= 比較対象の使用するclssカプセル.
        List<clssDef> clssCompareTo = new List<clssDef>();
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        dist = Mathf.Infinity;
        bool isCollided_any = false;
        if (useType == clssDef.ClssType.Attack)
        {
            clssRef = findclss(useType, frame);
            clssCompareTo = compareTo.findclss(clssDef.ClssType.Hit, frame);
        }
        else
        {
            clssRef = findclss(useType, frame);
            clssCompareTo = compareTo.findclss(clssDef.ClssType.Attack, frame);
        }

        foreach (clssDef cls in clssRef)
        {
            foreach (clssDef compcls in clssCompareTo)
            {
                //cls.DrawUPos(Color.red);
                float compareing;
                Vector3 v_a, v_b;
                bool isCollided = cls.isCollided(out v_a, out v_b, out compareing, compcls);
                if (isCollided && compareing < dist)
                {
                    v1 = v_a;
                    v2 = v_b;
                    compareing = dist;
                    isCollided_any = true;
                }
            }
        }
        return isCollided_any;
    }

    public bool clssCollided(out Vector3 v1, out Vector3 v2, out float dist, clssDef.ClssType useType,
    clssSetting compareTo, float frame, Entity entity_self, Entity entity_compareTo)
    {
        List<clssDef> clssRef = new List<clssDef>();
        List<clssDef> clssCompareTo = new List<clssDef>();
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        dist = Mathf.Infinity;
        bool isCollided_any = false;
        if (useType == clssDef.ClssType.Attack)
        {
            clssRef = findclss(useType, frame);
            clssCompareTo = compareTo.findclss(clssDef.ClssType.Hit, frame);
        }
        else
        {
            clssRef = findclss(useType, frame);
            clssCompareTo = compareTo.findclss(clssDef.ClssType.Attack, frame);
        }
        foreach (clssDef cls in clssRef)
        {
            foreach (clssDef compcls in clssCompareTo)
            {
                float compareing;
                Vector3 v_a, v_b;
                bool isCollided = cls.isCollided(out v_a, out v_b, out compareing, compcls);
                if (isCollided && compareing < dist)
                {
                    v1 = v_a;
                    v2 = v_b;
                    compareing = dist;
                    isCollided_any = true;
                }
            }
        }
        return isCollided_any;
    }


    //Cloneメソッドの拡張..
    public clssSetting Clone()
    {
        //新規に作成.
        var retDef = new clssSetting();
        foreach (clssDef c in this.clssDefs)
        {
            retDef.clssDefs.Add(c.Clone);
        }
        //string形式なので自動的にdeepcopyされるはず、というか参照でもいいのか？
        retDef.disableClssList.AddRange(this.disableClssList);
        retDef.eraseAllClss = this.eraseAllClss;

        return retDef;
    }
}

[System.Serializable]
[SerializeField]
public class clssDef
{
    //Cloneの拡張
    public clssDef Clone => new clssDef
    {
        clssType = this.clssType,
        attachTo = this.attachTo,
        showGizmo = this.showGizmo,
        startPos = this.startPos,
        endPos = this.endPos,
        width = this.width,
        StartTime = this.StartTime,
        EndTime = this.EndTime        
    };

    //やられ判定と当たり判定をenumで管理する
    public enum ClssType
    {
        Hit,
        Attack
    }
    public ClssType clssType;
    //対象キャラクターのどのゲームオブジェクトに追随するか..を取得し、下のattachTransformに値を入力.
    public string attachTo;
    public bool showGizmo;
    public Entity _entity;

    public Transform attachTransform;

    //wを半径とする.
    //スタートポジション・終了ポジションはtransformを基準とする.
    //attachTransformが存在しない際は
    [SerializeField]
    internal Vector3 startPos, endPos;
    [SerializeField]
    internal float width;

    //前回のスタート・終了ポジションを設定.
    public Vector3 _lastcalcStartPos = Vector3.zero;
    public Vector3 _lastcalcEndPos = Vector3.zero;

    //当たり判定設定時間 - 設定終了時間を表す.
    internal float StartTime = 0f, EndTime = 10f;

    //基準Transformを軸にしたカプセルコライダの設定
    public (Vector3, Vector3) getGlobalPos()
    {
        Vector3 start, end;
        if (attachTransform != null)
        {
            Transform t = attachTransform;

            start = t.position + t.rotation * startPos;
            end = t.position + t.rotation * endPos;
        }
        else
        {
            start = startPos;
            end = endPos;
        }
        return (start, end);
    }

    public void DrawUPos(UnityEngine.Color color)
    {
        Vector3 v0_1, v0_2;
        Vector3 v1_1 = _lastcalcStartPos, v1_2 = _lastcalcEndPos;
        (v0_1, v0_2) = getGlobalPos();
        
        Debug.DrawLine(v0_1,v0_2,color);
        Debug.DrawLine(v1_1,v1_2,color);
        Debug.DrawLine(v0_1,v1_1,color);
        Debug.DrawLine(v0_2,v1_2,color);
    }

    //当たり判定指定
    //Entityが無いやつに関して..
    public void setTransform(Transform tfm)
    {
        attachTransform = tfm;
    }

    //entity自体のトランスフォームを読み出し.
    //attachToが存在しない際はentityのルートを選択する.
    public void initTransform(Entity entity)
    {
        _entity = entity;
        attachTransform = entity.transform;
        if (attachTo != null)
        {
            //entity中のすべての階層のtransformを取得.
            //一度しかやらないことを想定..
            Transform[] transforms = entity.allChildTransforms;
            foreach (Transform t in transforms)
            {
                if (t.name == attachTo)
                {
                    attachTransform = t;
                    return;
                }
            }
        }
    }

    public void DrawCapsule()
    {
        Vector3 pos_1, pos_2;
        (pos_1, pos_2) = getGlobalPos();
        //Debug.Log(attachTo + " Drawin Capsules at" + pos_1.ToString() + pos_2.ToString());
        DrawCapsuleGizmo_Tool(pos_1, pos_2, width,
        clssType == ClssType.Hit ? Color.blue : Color.red);
        Debug.DrawLine(pos_1,pos_2);
    }

//Gizmoの描写
    static public void DrawCapsuleGizmo_Tool(Vector3 start, Vector3 end, float radius, Color col)
    {
        int size = (int)((end - start).magnitude / radius);
        DrawWireSphere_OnDebug(start, radius, col);
        DrawWireSphere_OnDebug(end, radius, col);
        if (radius > 0)
        {
            //Debug.Log("Width : " + radius);
        }
        for (int i = 1; i < size + 2; i++)
            {
                //Debug.Log("Drawin Capsules");
                Vector3 DrawAt = start + (end - start) * ((float)i / Mathf.Max(1, size + 2));
                //Debug.Log("Drawin Sphere at" + DrawAt);
                DrawWireSphere_OnDebug(DrawAt, radius, col);
            }
    }

    static public void DrawWireSphere_OnDebug(Vector3 pos, float radius, Color col)
    {
        int segs = 12;
        //Debug.Log(pos);
        for (int i = 0; i < segs; i++)
        {
            Vector3 drawer_1 = pos +
            Quaternion.AngleAxis(360f * i / segs, Vector3.up) * (Vector3.right * radius);
            Vector3 drawer_2 = pos +
            Quaternion.AngleAxis(360f * (i + 1) / segs, Vector3.up) * (Vector3.right * radius);
            Debug.DrawLine(drawer_1, drawer_2, col);
        }
        for (int i = 0; i < segs; i++)
        {
            Vector3 drawer_1 = pos +
            Quaternion.AngleAxis(360f * i / segs,Vector3.right) * (Vector3.up * radius);
            Vector3 drawer_2 = pos +
            Quaternion.AngleAxis(360f * (i + 1) / segs,Vector3.right) * (Vector3.up * radius);
            Debug.DrawLine(drawer_1,drawer_2, col);
        }
        for (int i = 0; i < segs; i++)
        {
            Vector3 drawer_1 = pos +
            Quaternion.AngleAxis(360f * i / segs,Vector3.forward) * (Vector3.right * radius);
            Vector3 drawer_2 = pos +
            Quaternion.AngleAxis(360f * (i + 1) / segs,Vector3.forward) * (Vector3.right * radius);
            Debug.DrawLine(drawer_1,drawer_2, col);
        }
    }

    //最後のポジションの計算
    public void updatelastPos()
    {
        (_lastcalcStartPos, _lastcalcEndPos) = getGlobalPos();
    }

    //当たり判定の計算
    public bool isCollided(out Vector3 v1 , out Vector3 v2 , out float dist , clssDef compareTo)
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        dist = Mathf.Infinity;
        (Vector3 midPos_1, float base_distToMid) = GetMid();
        (Vector3 midPos_2, float compare_distToMid) = compareTo.GetMid();
        if ((midPos_2 - midPos_1).magnitude <= (base_distToMid + width + compare_distToMid + compareTo.width))
        {
            //ボール型の当たり判定概形（要は平行四面 + カプセルスイープの範囲を考慮した最小最接球を導出.）
            //Gizmos.DrawSphere(midPos_1, base_distToMid + width);
            //Gizmos.DrawSphere(midPos_1, compare_distToMid + compareTo.width);

            TrisUtil.Triangle T1_0, T1_1 , T2_0 , T2_1;
            (T1_0, T1_1) = GetTriangle_MovedPlane();
            (T2_0, T2_1) = compareTo.GetTriangle_MovedPlane();

            T1_0.DrawTris();
            T1_1.DrawTris();
            T2_0.DrawTris();
            T2_1.DrawTris();


            /*
            //デバッグ用に表示.
            //
            //2025-06-11 : 三角形の配列形式表示で見たところ、全配列でvectorが0になってたのでsetvs()をコールするようにした.
            string g = "" , f = "";
            int i = 0;
            foreach (Vector3 posList_T1 in T1_0.vs)
            {
                f += string.Format("pos{0} : {1}", i, posList_T1);
                i++;
            }
            foreach (Vector3 posList_T2 in T2_0.vs)
            {
                g += string.Format(" pos{0} : {1}", i, posList_T2);
                i++;
            }
            Debug.Log(f + g);
            */

            //v1は自己の当たり判定位置　v2は相手の.
            getLeastPos(ref dist, ref v1, ref v2, T1_0, T2_0);
            getLeastPos(ref dist, ref v1, ref v2, T1_1, T2_0);
            getLeastPos(ref dist, ref v1, ref v2, T1_0, T2_1);
            getLeastPos(ref dist, ref v1, ref v2, T1_1, T2_1);

            //Debug.Log(dist);


            if (dist <= width + compareTo.width)
            {
                return true;
            }
        }
        return false;
    }
    
    //三角面同士の最短距離導出
    public void getLeastPos(ref float last, ref Vector3 v1, ref Vector3 v2, TrisUtil.Triangle t1, TrisUtil.Triangle t2)
    {
        float d1;
        Vector3 v1_ch, v2_ch;
        TrisUtil TU = new TrisUtil();
        (d1, v1_ch, v2_ch) = TU.TrisDistance(t1, t2);
        if (last > d1)
        {
            last = d1;
            v1 = v1_ch;
            v2 = v2_ch;
        }
    }

    //動作方向の平行四面近似する三角面の導出
    public (TrisUtil.Triangle, TrisUtil.Triangle) GetTriangle_MovedPlane()
    {
        Vector3 stPos, ePos;
        (stPos, ePos) = getGlobalPos();
        //TrisUtil.Triangle Compare_1_0 = new TrisUtil.Triangle(stPos, ePos, ePos);
        //TrisUtil.Triangle Compare_1_1 = new TrisUtil.Triangle(stPos, ePos, ePos);
        
        TrisUtil.Triangle Compare_1_0 = new TrisUtil.Triangle(stPos, ePos, _lastcalcStartPos);
        TrisUtil.Triangle Compare_1_1 = new TrisUtil.Triangle(ePos, _lastcalcStartPos, _lastcalcEndPos);
        
        //Debug.Log(_lastcalcEndPos.ToString() + _lastcalcStartPos.ToString());
        return (Compare_1_0, Compare_1_1);
    }

    //動作方向の平行四面の中点位置・及び頂点の最長距離を導出.
    public (Vector3, float) GetMid()
    {
        Vector3 st, end;
        (st, end) = getGlobalPos();
        float f_1 = (st - _lastcalcEndPos).sqrMagnitude;
        float f_2 = (end - _lastcalcStartPos).sqrMagnitude;
        float Dist = f_1 > f_2 ? f_1 : f_2;
        return ((st + _lastcalcEndPos) / 2f, Dist);
    }
    
}

