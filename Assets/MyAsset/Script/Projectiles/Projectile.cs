using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 pos_1, pos_2;
    public float radius;

    public float RemainTime = 1.0f;

    public GameObject EffectObject;

    //当たり判定 - 
    clssDef def = new clssDef();
    // Start is called before the first frame update
    void Start()
    {
        def.setTransform(this.transform);
        def.startPos = pos_1;
        def.endPos = pos_2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
