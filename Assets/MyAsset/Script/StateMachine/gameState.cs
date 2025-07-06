using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class gameState : MonoBehaviour
{
    //全部のスクリプトからアクセスするように.
    //
    static public gameState self;
    public GameObject hitEff;
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

    public List<Entity> entityList;

    //HitDefを発火する際のイベント
    public void ProvokeHitDef(Entity calledEntity, hitDefParams hitDefParams)
    {
        hitDefParams useParam = new hitDefParams();
        if (hitDefParams != null)
        {
            useParam = hitDefParams;
        }
        foreach (Entity e in entityList)
            {
                //selfには反応しない
                if (e != calledEntity)
                {
                    //それぞれのentityの現在再生中のAnimatorが持つClssに対して衝突判定.
                    bool f = calledEntity.hitCheck(e, out Vector3 HitPt);
                    //hitしたなら一先ずAnim番号を5000に飛ばしたい. ChangeState(5000)の最優先Queueとして組み込む.
                    if (f == true)
                    {
                        //stateChange
                        e.isStateChanged = true;
                        
                        //一先ず、プレースホルダーとして入れる
                        e.CurrentStateID = hitDefParams.ChangeState_Enemy;
                        e.stateTime = 0;

                        //placeholder for velocity
                        Vector3 HitVect =
                        Vector3.ProjectOnPlane(e.transform.position - calledEntity.transform.position, Vector3.up);
                        e.rigid.velocity = HitVect.normalized * useParam.velset.x + Vector3.up * useParam.velset.y;

                        //hitpause
                        (calledEntity.HitPauseTime, e.HitPauseTime) = (hitDefParams.hitStopTime.x, hitDefParams.hitStopTime.y);

                        //shapepositions
                        e.transform.DOShakePosition(e.HitPauseTime * Time.fixedDeltaTime, 0.25f, 40, 45);
                        e.ChangeAnim(.1f);



                        //placeholder for rotation
                        e.transform.rotation =
                        Quaternion.Lerp(e.transform.rotation, Quaternion.LookRotation(-HitVect, Vector3.up), 0.6f);
                        Debug.Log("Hit : " + e.gameObject.name);
                        Instantiate
                        (hitEff, HitPt, Quaternion.identity);
                    }
                }
            }
    }
}

[SerializeField]
[System.Serializable]
public class hitDefParams
{
    public float Damage;
    public Vector3 velset;
    [SerializeField]
    public Vector2 hitStopTime;
    public int ChangeState_Enemy = 5000;
}
