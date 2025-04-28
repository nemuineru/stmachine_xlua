using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ColliderSample : MonoBehaviour
{
    [System.Serializable]
    //ChatGPTより, 自動生成の一部を借用.
    public class TaperedCapsule
    {
        public TaperedCapsule(Vector3 start, Vector3 dir, float radius)
        {
            {
                StartPos = start;
                Dir = dir;
                _radius = radius;
            }
        }
        public float _radius;   // 始点側の半径

        public int _splitNum = 0;

        
        public Vector3 StartPos;       // 始点（中心軸の一端）
        public Vector3 Dir;         // 終点（中心軸の他端）
        public float Length => Dir.magnitude;
        public Vector3 MoveVect = Vector3.zero;
    }

    [SerializeField]
    TaperedCapsule capsule = new TaperedCapsule(Vector3.zero,Vector3.one, 0.2f);

    public ColliderSample sampleCapsule;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    (float, Vector3, Vector3) capsulesCheck()
    {
        if(sampleCapsule != null)
        {
            TaperedCapsule selectedCapsule = sampleCapsule.capsule;
            PlaneDistCheck_1.CheckPlane checkPlane = 
            new PlaneDistCheck_1.CheckPlane(capsule.StartPos,capsule.Dir,capsule.MoveVect);
            PlaneDistCheck_1.CheckPlane checkPlane_2 = 
            new PlaneDistCheck_1.CheckPlane(selectedCapsule.StartPos,selectedCapsule.Dir,selectedCapsule.MoveVect);
            return PlaneDistCheck_1.ClosestDistanceBetweenFinitePlanes(checkPlane, checkPlane_2);
        }
        else
        {
            return (Mathf.Infinity, Vector3.zero, Vector3.zero);
        }
    }

    
    bool CheckCapsuleCollision_OnPoint()
    {
        bool resl = false;
        if(sampleCapsule == null)
        {
            return false;
        }   
        
        var (dist, p_1, p_2 )= capsulesCheck();
        if(dist < capsule._radius + sampleCapsule.capsule._radius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(p_1, capsule._radius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(p_2, sampleCapsule.capsule._radius);
            resl = true;
        }      

        return resl;
    }

















    void OnDrawGizmos()
    {
        
    
        DrawTaperedCapsule(capsule.StartPos, capsule.Dir + capsule.StartPos, 
        capsule._radius, Color.magenta);

        DrawTaperedCapsule(capsule.StartPos + capsule.MoveVect, capsule.StartPos + capsule.Dir + capsule.MoveVect, 
        capsule._radius, Color.magenta);
        
        DrawSweepLines(capsule.StartPos, capsule.StartPos + capsule.Dir,capsule.StartPos + capsule.MoveVect, 
        capsule.StartPos + capsule.Dir + capsule.MoveVect, 
        capsule._radius, capsule._radius);
        CheckCapsuleCollision_OnPoint();
    }

    void DrawSweepLines(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, float r1, float r2)
    {
        Vector3 axis = a2 - a1;
        Vector3 forward = axis.normalized;

        Vector3 up = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f) up = Vector3.right;
        Vector3 right = Vector3.Cross(forward, up).normalized;
        Vector3 upDir = Vector3.Cross(right, forward).normalized;

        int segments = 12;
        Gizmos.color = new Color(1, 1, 0, 0.6f); // 半透明黄

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 dir = Mathf.Cos(angle) * right + Mathf.Sin(angle) * upDir;

            Vector3 pStartA = a1 + dir * r1;
            Vector3 pStartB = a2 + dir * r2;
            Vector3 pEndA = b1 + dir * r1;
            Vector3 pEndB = b2 + dir * r2;

            Gizmos.DrawLine(pStartA, pEndA); // ラグ線 始点
            Gizmos.DrawLine(pStartB, pEndB); // ラグ線 終点
        }
    }

    void DrawTaperedCapsule(Vector3 a, Vector3 b, float r1, Color col)
    {
        Gizmos.color = col;

        Vector3 axis = b - a;
        Vector3 forward = axis.normalized;
        float height = axis.magnitude;

        // ローカルな軸を作る（法線）
        Vector3 up = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f) up = Vector3.right;
        Vector3 right = Vector3.Cross(forward, up).normalized;
        Vector3 upDir = Vector3.Cross(right, forward).normalized;

        int segments = 8;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;

            Vector3 dir1 = Mathf.Cos(angle1) * right + Mathf.Sin(angle1) * upDir;
            Vector3 dir2 = Mathf.Cos(angle2) * right + Mathf.Sin(angle2) * upDir;

            Vector3 p1Start = a + dir1 * r1;
            Vector3 p1End   = b + dir1 * r1;
            Vector3 p2Start = a + dir2 * r1;
            Vector3 p2End   = b + dir2 * r1;

            Gizmos.DrawLine(p1Start, p1End);
            Gizmos.DrawLine(p1Start, p2Start);
            Gizmos.DrawLine(p1End, p2End);
        }

        // 円キャップを描く（始点・終点の輪郭）
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;

            Vector3 dir1 = Mathf.Cos(angle1) * right + Mathf.Sin(angle1) * upDir;
            Vector3 dir2 = Mathf.Cos(angle2) * right + Mathf.Sin(angle2) * upDir;

            Gizmos.DrawLine(a + dir1 * r1, a + dir2 * r1);
            Gizmos.DrawLine(b + dir1 * r1, b + dir2 * r1);
        }
        Gizmos.DrawWireSphere(a, r1);
        Gizmos.DrawWireSphere(b, r1);
        
    }

}
