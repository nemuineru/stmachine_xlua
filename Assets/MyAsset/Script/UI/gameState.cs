using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
//using UnityEditor.SearchService;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;

public class gameState : MonoBehaviour
{
    //全部のスクリプトからアクセスするように.
    //
    static public gameState self;

    //敵のHP管理バー.
    public GameObject HPUI;

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
    public bool ProvokeHitDef_Entity(Entity calledEntity, hitDefParams hitDefParams)
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
                    hitDefApply(e, calledEntity, useParam, HitPt);
                }
                //当てた分キャラ指定の値が減少..
                refNumRemaining--;
            }
        }
        return ret;
    }

    public bool ProvokeHitDef_Projs(Entity calledEntity, clssSetting sets , Transform trfs, hitDefParams H_params)
    {
        bool ret = false; int refNumRemaining;
        hitDefParams useParam = new hitDefParams();
        if (H_params != null)
        {
            useParam = H_params;
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
                    hitDefApply(e, trfs, useParam, HitPt);
                }
                //当てた分キャラ指定の値が減少..
                refNumRemaining--;
            }
        }
        return ret;
    }

    void hitDefApply(Entity beatenEntity, Entity calledEntity,
    hitDefParams calledEParam , Vector3 hitContactPoint)
    { 
        //stateChangeを設定..
        beatenEntity.isStateChanged = true;

        //一先ず、プレースホルダーとして入れる
        //stateTimeをリセット.
        beatenEntity.CurrentStateID = calledEParam.ChangeState_Enemy;
        beatenEntity.stateTime = 0;
        //攻撃を当てた対象にコントロールされる場合は相手のステートマップの読み出しを設定
        //設定されたEntityはselfStateが読み出されない限り読み出す.
        if (calledEParam.enemyRefsPlayerNum == true)
        {
            beatenEntity.controlledEntity = calledEntity;
        }
        //placeholder for velocity
        //currently its barebone
        Vector3 HitVect = Vector3.ProjectOnPlane
        (beatenEntity.transform.position - calledEntity.transform.position, Vector3.up);

        DamageApply(beatenEntity, HitVect, calledEParam);
        //hitpause
        (calledEntity.HitPauseTime, beatenEntity.HitPauseTime) = (calledEParam.hitStopTime.x, calledEParam.hitStopTime.y);
        Instantiate
        ((calledEParam.HitEff != null ? calledEParam.HitEff : defaultEff), hitContactPoint, Quaternion.identity);

        //playerのchangestateが0以上なら変更.
        if (calledEParam.ChangeState_Player > -1)
        {
            calledEntity.isStateChanged = true;
            calledEntity.CurrentStateID = calledEParam.ChangeState_Player;
        }
    }

    void hitDefApply(Entity beatenEntity, Transform calledPoint,
    hitDefParams calledEParam , Vector3 hitContactPoint)
    { 
        //stateChangeを設定..
        beatenEntity.isStateChanged = true;

        //一先ず、プレースホルダーとして入れる
        //stateTimeをリセット.
        beatenEntity.CurrentStateID = calledEParam.ChangeState_Enemy;
        beatenEntity.stateTime = 0;

        //現状、Projectileに関してはステート奪取を考えないことにする.
        // if (calledEParam.enemyRefsPlayerNum == true)
        // {
        //     beatenEntity.controlledEntity = calledEntity;
        // }

        //placeholder for velocity
        //currently its barebone
        Vector3 HitVect = Vector3.ProjectOnPlane
        (beatenEntity.transform.position - calledPoint.position, Vector3.up);

        DamageApply(beatenEntity, HitVect, calledEParam);
        //hitpause
        (beatenEntity.HitPauseTime) = (calledEParam.hitStopTime.y);
        Instantiate
        ((calledEParam.HitEff != null ? calledEParam.HitEff : defaultEff), hitContactPoint, Quaternion.identity);
    }



    void DamageApply(Entity beatenEntity, Vector3 HitVect, hitDefParams calledEParam)
    {
        //SetSpeed
        beatenEntity.rigid.velocity = HitVect.normalized * calledEParam.velset.x + Vector3.up * calledEParam.velset.y;


        //shapepositions
        beatenEntity.transform.DOShakePosition(beatenEntity.HitPauseTime * Time.fixedDeltaTime, 0.25f, 40, 45);
        beatenEntity.ChangeAnim(.1f);

        //hitpoint damage
        beatenEntity.status.currentHP -= calledEParam.Damage;

        //placeholder for rotation
        beatenEntity.transform.rotation =
        Quaternion.Lerp(beatenEntity.transform.rotation, Quaternion.LookRotation(-HitVect, Vector3.up), 0.6f);
        Debug.Log("Hit : " + beatenEntity.gameObject.name);
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

