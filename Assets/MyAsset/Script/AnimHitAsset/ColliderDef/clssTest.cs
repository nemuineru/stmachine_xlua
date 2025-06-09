using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clssTest : MonoBehaviour
{
    [SerializeField]
    clssSetting clssSetting;

    [SerializeField]
    clssTest compareTo;

    void OnDrawGizmos()
    {
        foreach (clssDef c in clssSetting.clssDefs)
        {
            if (c.showGizmo == true)
            {
                c.setTransform(this.transform);
                c.getGlobalPos();
                c.DrawCapsule();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
