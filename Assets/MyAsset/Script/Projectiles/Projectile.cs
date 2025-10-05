using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 pos_1, pos_2;
    public float radius;

    public float RemainTime = 1.0f;

    public GameObject EffectObject;

    internal int Damage = 20;

    //
    public Entity proj_Controller;

    
    //当たり判定 - 
    clssSetting cSet = new clssSetting();
    // Start is called before the first frame update
    void Start()
    {
        clssDef def = new clssDef();
        def.setTransform(this.transform);
        def.startPos = pos_1;
        def.endPos = pos_2;
        def.clssType = clssDef.ClssType.Hit;

        cSet.clssDefs.Add(def);
    }

    // Update is called once per frame
    void Update()
    {
        cSet.clssPosUpdate();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.GetMask("Terrain"))
        {
            destroyEmit();
            Destroy(gameObject);
        }
    }

    void destroyEmit()
    {
        Instantiate(EffectObject,transform.position,Quaternion.identity);
    }
}
