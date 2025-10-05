using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 pos_1, pos_2;
    public float radius;

    public float RemainTime = 1.0f;

    public GameObject EffectObject;
    //
    public Entity proj_Controller;

    [SerializeField]
    public hitDefParams hitDefParams;

    
    //当たり判定 - 
    clssSetting cSet = new clssSetting();
    // Start is called before the first frame update
    void Start()
    {
        clssDef def = new clssDef();
        def.setTransform(this.transform);
        def.startPos = pos_1;
        def.endPos = pos_2;
        def.clssType = clssDef.ClssType.Attack;
        def.width = radius;

        cSet.clssDefs.Add(def);
    }

    // Update is called once per frame
    void Update()
    {
        cSet.clssPosUpdate();
        foreach (clssDef c in cSet.clssDefs)
        {
            Debug.Log("drawing clssDefs");
            c.getGlobalPos();
            c.DrawCapsule();
        }
        if (gameState.self.ProvokeHitDef_Projs(proj_Controller, cSet, transform, hitDefParams))
        {
            Debug.Log("HIT!");
            destroyEmit();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collided! + " + collision.gameObject.layer);
        if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            destroyEmit();
        }
    }

    void destroyEmit()
    {
        if (EffectObject != null)
        { 
            Instantiate(EffectObject,transform.position,Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
