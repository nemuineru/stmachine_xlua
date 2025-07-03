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
                    //一先ず、プレースホルダーとして
                    e.CurrentStateID = 5000;
                    e.rigid.velocity = e.gameObject.transform.forward * -0.1f + Vector3.up * 6.0f;
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