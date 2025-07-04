using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameState : MonoBehaviour
{
    //全部のスクリプトからアクセスするように.
    //
    static public gameState self;
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
    public void ProvokeHitDef(Entity calledEntity)
    {
        foreach (Entity e in entityList)
        {
            //selfには反応しない
            if (e != calledEntity)
            {
                //それぞれのentityの現在再生中のAnimatorが持つClssに対して衝突判定.
                bool f = calledEntity.hitCheck(e);
                //hitしたなら一先ずAnim番号を5000に飛ばしたい. ChangeState(5000)の最優先Queueとして組み込む.
                if (f == true)
                {
                    e.isStateChanged = true;
                    //一先ず、プレースホルダーとして入れるが... 何か5000に移動していない. そもそも
                    //stateが機能していない..
                    e.CurrentStateID = 5000;
                    e.stateTime = 0;

                    //placeholder for velocity
                    Vector3 HitVect =
                    Vector3.ProjectOnPlane(e.transform.position - calledEntity.transform.position,Vector3.up);
                    e.rigid.velocity = HitVect.normalized * 0.2f + Vector3.up * 3.5f;


                    //placeholder for rotation
                    e.transform.rotation =
                    Quaternion.Lerp(e.transform.rotation,Quaternion.LookRotation(-HitVect,Vector3.up),0.6f);
                    Debug.Log( "Hit : " + e.gameObject.name);
                }
            }
        }
    }

}

class hitDefParams
{
    Vector3 velset;
    (float, float) hitStopTime = (0.3f, 0.3f);
}