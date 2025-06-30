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
        if (self != null)
        {
            self = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Entity> entityList;
    void ProvokeHitDef(Entity calledEntity)
    {
        foreach (Entity e in entityList)
        {
            //selfには反応しない
            if (e != calledEntity)
            {
                bool f = calledEntity.MainAnimMixer.MainAnimDef.clssCollided(e);
                if (f == true)
                {

                }
            }
        }
    }
}