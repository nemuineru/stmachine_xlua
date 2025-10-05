using System.Collections;
using System.Collections.Generic;
//using TreeEditor;
using UnityEngine;

public class CheckTrisIntersect : MonoBehaviour
{
    public Transform pos1, pos2, pos3;
    public CheckTrisIntersect compareTo;
    enum checkTo
    {
        point,
        line,
        line_infinity,
        tris
    }
    [SerializeField]
    checkTo checkPoint = checkTo.point;


    void OnDrawGizmos()
    {
        if (pos1 == null || pos2 == null || pos3 == null)
            return;
        switch (checkPoint)
        {
            case checkTo.point:
            {
                lineNearestPointFind_Gizmo();
                break;
            }
            case checkTo.line:
            {
                NearestBetweenLines_Gizmo();
                break;
            }
            case checkTo.line_infinity:
            {
                NearestBetweenInfnityLines_Gizmo();
                break;
            }
            case checkTo.tris : 
            {
                trisNearestFind_Gizmo();
                break;
            }
        }
    }

    void lineNearestPointFind_Gizmo()
    {
        TrisUtil.Triangle triangle1 = this.GetTriangle();
        TrisUtil.Triangle triangle2 = compareTo.GetTriangle();
        Gizmos.DrawLine(pos1.position, pos2.position);
        Gizmos.DrawCube(pos1.position,Vector3.one * 0.01f);
        new TrisUtil().calcPointOnLineSegmentDist(triangle2.p0, triangle2.p1, triangle1.p0, out Vector3 o, out float vt);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(triangle1.p0, o);        
    }

    void NearestBetweenInfnityLines_Gizmo()
    {

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos1.position, pos2.position);

        
        TrisUtil.Triangle triangle1 = this.GetTriangle();
        TrisUtil.Triangle triangle2 = compareTo.GetTriangle();
        
        if (triangle1 != null && compareTo != null && triangle2 != null)
        {
            new TrisUtil().calcLinesDist(triangle1.p0, triangle1.p1 , triangle2.p0, triangle2.p1,
            out Vector3 Near_1, out Vector3 Near_2,out float findNearest_1, out float findNearest_2);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(Near_1, Near_2);
        }
    }
    void NearestBetweenLines_Gizmo()
    {

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos1.position, pos2.position);

        
        TrisUtil.Triangle triangle1 = this.GetTriangle();
        TrisUtil.Triangle triangle2 = compareTo.GetTriangle();
        
        if (triangle1 != null && compareTo != null && triangle2 != null)
        {
            new TrisUtil().LineDist(triangle1.p0, triangle1.p1 , triangle2.p0, triangle2.p1,
            out float findNearest, out Vector3 Near_1, out Vector3 Near_2);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(Near_1, Near_2);
        }
    }

    //Debug for finding the nearest point between two triangles
    void trisNearestFind_Gizmo()
    { 

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

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Near_1, Near_2);
            //Debug.LogWarning("Position Found at: " + Near_1 + " and " + Near_2 + ", Distance: " + findNearest);
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
