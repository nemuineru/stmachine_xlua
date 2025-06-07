using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class CheckTrisIntersect : MonoBehaviour
{
    public Transform pos1, pos2, pos3;
    public CheckTrisIntersect compareTo;


    void OnDrawGizmos()
    {
        if (pos1 == null || pos2 == null || pos3 == null)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos1.position, pos2.position);
        Gizmos.DrawLine(pos2.position, pos3.position);
        Gizmos.DrawLine(pos3.position, pos1.position);
        
        TrisUtil.Triangle triangle1 = this.GetTriangle();
        TrisUtil.Triangle triangle2 = compareTo.GetTriangle();

        if (triangle1 != null && compareTo != null && triangle2 != null)
        {
            float findNearest = Mathf.Infinity;
            Vector3 Near_1, Near_2;
            (findNearest, Near_1, Near_2) = new TrisUtil().TrisDistance(triangle1, triangle2);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Near_1, Near_2);
            Debug.LogWarning("Position Found at: " + Near_1 + " and " + Near_2 + ", Distance: " + findNearest);
        }
        else
        {
            Debug.LogWarning("Triangle is null, please check the positions.");
        }
    }
    
    TrisUtil.Triangle GetTriangle()
    {
        TrisUtil.Triangle triangle = new TrisUtil.Triangle(pos1.position, pos2.position, pos3.position);
        triangle.setVs();
        return triangle; 
    }
}
