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

        foreach (clssDef c in clssSetting.clssDefs)
        {
            c.setTransform(this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        clssSetting.clssPosUpdate();
        if (compareTo != null)
        {
            //Debug.Log("Checking");
            bool isCollided =
            clssSetting.clssCollided(out Vector3 v1, out Vector3 v2, out float dist,
            clssDef.ClssType.Hit, compareTo.clssSetting, 0.1f);
            if (isCollided)
            {
                //Debug.Log("Hit Detected");
                Debug.DrawLine(v1, v2 + Vector3.up, Color.cyan);
                Debug.DrawLine(v1, v2 + Vector3.left, Color.magenta);
            }
        }

        foreach (clssDef c in clssSetting.clssDefs)
        {
            c.getGlobalPos();
        }
        
    }
}