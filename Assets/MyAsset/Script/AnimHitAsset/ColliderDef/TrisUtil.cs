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
        //全部の組み合わせを先ず計算
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

        return minDistance;
    }
}