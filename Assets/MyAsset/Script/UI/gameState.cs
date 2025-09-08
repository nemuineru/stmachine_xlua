using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
//using UnityEditor.SearchService;
using System.Linq;
using UnityEngine.Serialization;

public class gameState : MonoBehaviour
{
    //全部のスクリプトからアクセスするように.
    //
    static public gameState self;

    //敵のHP管理バー.
    public GameObject HPUI;

    [FormerlySerializedAs("hitEff")]
    public GameObject defaultEff;
    void Awake()
    {
        if (self == null)
        {
            self = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        entityList = FindObjectsByType<Entity>(FindObjectsSortMode.InstanceID).ToList();
    }

    public List<Entity> entityList;

    //HitDefを発火する際のイベント
    public bool ProvokeHitDef(Entity calledEntity, hitDefParams hitDefParams)
    {
        bool ret = false;
        int refNumRemaining;
        hitDefParams useParam = new hitDefParams();
        if (hitDefParams != null)
        {
            useParam = hitDefParams;
        }
        refNumRemaining = useParam.maxEntityHits;
        foreach (Entity e in entityList)
        {
            //selfには反応しない. また当たる数が設定されているなら0にならない限り設定される.
            if (e != calledEntity && refNumRemaining > 0)
            {
                //それぞれのentityの現在再生中のAnimatorが持つClssに対して衝突判定.
                //また、entityの無敵判定に関しても考える.
                bool f = calledEntity.hitCheck(e, out Vector3 HitPt);


                //hitしたなら一先ずAnim番号を5000に飛ばしたい. ChangeState(5000)の最優先Queueとして組み込む.
                if (f == true)
                {
                    ret = true;
                    //stateChangeを設定..
                    e.isStateChanged = true;

                    //一先ず、プレースホルダーとして入れる
                    e.CurrentStateID = useParam.ChangeState_Enemy;
                    e.stateTime = 0;
                    //攻撃を当てた対象にコントロールされる場合は相手のステートマップの読み出しを設定
                    //設定されたEntityはselfStateが読み出されない限り読み出す.
                    if (useParam.enemyRefsPlayerNum == true)
                    {
                        e.controlledEntity = calledEntity;
                    }
                    //placeholder for velocity
                    Vector3 HitVect =
                    Vector3.ProjectOnPlane(e.transform.position - calledEntity.transform.position, Vector3.up);
                    e.rigid.velocity = HitVect.normalized * useParam.velset.x + Vector3.up * useParam.velset.y;

                    //hitpause
                    (calledEntity.HitPauseTime, e.HitPauseTime) = (useParam.hitStopTime.x, useParam.hitStopTime.y);

                    //shapepositions
                    e.transform.DOShakePosition(e.HitPauseTime * Time.fixedDeltaTime, 0.25f, 40, 45);
                    e.ChangeAnim(.1f);

                    e.status.currentHP -= useParam.Damage;




                    //placeholder for rotation
                    e.transform.rotation =
                    Quaternion.Lerp(e.transform.rotation, Quaternion.LookRotation(-HitVect, Vector3.up), 0.6f);
                    Debug.Log("Hit : " + e.gameObject.name);
                    Instantiate
                    ((useParam.HitEff != null ? useParam.HitEff : defaultEff), HitPt, Quaternion.identity);
                }
                //当てた分キャラ指定の値が減少..
                refNumRemaining--;
            }
        }
        return ret;
    }


    bool checkHit(string checkState, Entity checkEntity)
    {
        bool ret = false;
        if (checkState.Contains((char)checkEntity.checkHitStates()))
        {
            ret = true;
        }
        return ret;
    }
}


//後々にこのhitDefParamsの項目を表示・非表示設定可能にしたい
//非表示にした項目は値未設定ならデフォルト値を使用
[SerializeField]
[System.Serializable]
public class hitDefParams
{
    public float Damage;
    public Vector3 velset;
    [SerializeField]
    public Vector2 hitStopTime;
    [SerializeField]
    public GameObject HitEff;
    //当てた敵のステート変更情報(負の数以下で変更しない)
    public int ChangeState_Enemy = 5000;

    //当たる数(1がデフォ)
    public int maxEntityHits = 1;

    //敵がプレイヤーのステート名を参照するか？
    public bool enemyRefsPlayerNum = false;

    //プレイヤーのステート変更情報(負の数以下で変更しない)
    public int ChangeState_Player = -1;

    //どういう姿勢に当たるか？　など. "S"tanding "A"ir, "L"aying の頭文字指定
    //また、"F"は Fall状態のフラッグがあるキャラにHit, "E"veryはフラッグ問わず全部当たる.
    public string HitFlag = "SA";
}

