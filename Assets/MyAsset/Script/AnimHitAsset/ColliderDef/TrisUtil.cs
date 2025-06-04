using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TrisUtil
{
    public struct Triangle
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }
    }

    public float TrisDistance(Triangle t1, Triangle t2)
    {
        // Calculate the distance between the two triangles
        Vector3[] points1 = { t1.p0, t1.p1, t1.p2 };
        Vector3[] points2 = { t2.p0, t2.p1, t2.p2 };

        float minDistance = float.MaxValue;

        //頂点同士の距離を計算.
        //全部の組み合わせを先ず計算する
        for (int i = 0; i < points1.Length; i++)
        {
            for (int j = 0; j < points2.Length; j++)
            {
                float distance = Vector3.Distance(points1[i], points2[j]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
        }

        //辺と三角形の距離を計算.
        //

        return minDistance;
    }

    void LineDist(Vector3 S1_p1, Vector3 S1_p2, Vector3 S2_p1, Vector3 S2_p2,
    out float distance, out Vector3 closestPointOnLine1, out Vector3 closestPointOnLine2)
    {
        //最初は無限大
        distance = Mathf.Infinity;

        float S1_Dist = 0f;
        float S2_Dist = 0f;
        closestPointOnLine1 = S1_p1;
        closestPointOnLine2 = S2_p1;

        //S1 , S2が縮退しているかを考える.
        //S1が縮退している場合
        if (S1_Dist < Mathf.Epsilon)
        {
            closestPointOnLine1 = S1_p1;
            //S2が縮退している場合は点と点の距離として処理.
            if (S2_Dist < Mathf.Epsilon)
            {
                distance = Vector3.Distance(S1_p1, S2_p1);
                closestPointOnLine2 = S2_p1;
            }
            //そうでないなら、点と線分の距離を計算する.
            else
            {
                var Point = NearestPointOnLineSegment(S2_p1, S2_p2, S1_p1);
                closestPointOnLine2 = Point;
            }
            return;
        }
        //S2が縮退している場合は, S1の縮退が見られないため 点と線分の距離を計算する.
        else if (S2_Dist < Mathf.Epsilon)
        {
            closestPointOnLine2 = S2_p1;
            var Point = NearestPointOnLineSegment(S1_p1, S1_p2, S2_p1);
            closestPointOnLine1 = Point;
            return;
        }

        //2線分が平行であるなら垂線の端点の一つをP1に決定する.
        //決定されなかった場合S2_P1を指定.
        if (Vector3.Cross(S1_p1 - S1_p2, S2_p1 - S2_p2) == Vector3.zero)
        {
            distance = 0f;
            //S1の端点をP1に決定.
            closestPointOnLine1 = S1_p1;
            //S2の端点をP2に決定.
            closestPointOnLine2 = S2_p1;
            //線分同士で、
            var Point_1 = NearestPointOnLineSegment(S2_p1, S2_p2, S1_p1);
            var Point_2 = NearestPointOnLineSegment(S1_p1, S1_p1, S2_p1);
            return;
        }

    }

    // 頂点と線分距離
    // 点Pから最も近い線分AB上にある点を返す
    // https://sleepygamersmemo.blogspot.com/2019/03/perpendicular-foot-point.html より,拝借.
    Vector3 NearestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float length = ab.magnitude;
        ab.Normalize();

        float k = Vector3.Dot(p - a, ab);
        k = Mathf.Clamp(k, 0, length);
        return a + k * ab;
    }
}