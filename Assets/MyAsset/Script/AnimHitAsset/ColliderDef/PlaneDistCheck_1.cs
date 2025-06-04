using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//平行四面が成す平面同士の距離・位置を求めるための関数
public class PlaneDistCheck_1
{
    
    // 平面の情報を保持する構造体
    public struct CheckPlane
    {
        //中心位置.
        public Vector3 Center
        {
            get 
            {
                return startPoint + (U + V) / 2f;
            }
        }  
        // 平面の法線方向.
        public Vector3 Normal
        {
            get 
            {
                return Vector3.Cross(U, V).normalized;
            }
        }

        public float Width
        {
            get 
            {
                return U.magnitude;
            }
        }
        
        public float Height
        {
            get 
            {
                return V.magnitude;
            }
        }
        public Vector3 startPoint, U, V;       // 空間内の一辺の基準位置、及びその距離、方向.

        public CheckPlane(Vector3 StartPoint, Vector3 U_Vector, Vector3 V_Vector)
        {
            startPoint = StartPoint;
            U = U_Vector;
            V = V_Vector;
        }
    }

    //本題の内容. ChatGPTより拝借...したけどダメっすね
    //４頂点の座標を出すなら大丈夫だが、平面の交点は求められていない.
    public static (float, Vector3, Vector3) ClosestDistanceBetweenFinitePlanes(CheckPlane P1, CheckPlane P2)
    {
        // 法線ベクトルの単位化
        Vector3 N1 = P1.Normal;
        Vector3 N2 = P2.Normal;

        //平行に近いならそのまま..
        if(Mathf.Abs(Vector3.Dot(N1,N2)) < Mathf.Epsilon)
        {           
            float lg = Mathf.Abs(Vector3.Project(P2.Center - P1.Center, N1).magnitude);
            return (lg,P1.Center,P2.Center);
        }

        // 無限平面の最接近距離を計算
        float d = Mathf.Abs(Vector3.Dot(N1, P2.startPoint - P1.startPoint));

        // 無限平面上の最接近点（法線方向に移動）
        Vector3 P1_closest = P1.Center - d * N1;
        Vector3 P2_closest = P2.Center - d * N2;

        GetTrisCrossPoint(P1,P2);
        

        // P1_closest が P1上の平面においての2D座標内で 有限な平面内にあるかチェック
        Vector2 localCoords_P1 = GetLocalCoordinates(P1, P1_closest);
        bool valid_P1 = Mathf.Abs(localCoords_P1.x) <= P1.Width && Mathf.Abs(localCoords_P1.y) <= P1.Height;

        // P2_closestも同様.
        Vector2 localCoords_P2 = GetLocalCoordinates(P2, P2_closest);
        bool valid_P2 = Mathf.Abs(localCoords_P2.x) <= P2.Width && Mathf.Abs(localCoords_P2.y) <= P2.Height;

        if (valid_P1 && valid_P2)
        {
            return (d, P1_closest, P2_closest);  // これが最接近距離と座標
        }

        // 端の点との距離を考慮
        float minDistance = float.MaxValue;
        Vector3 bestP1 = P1_closest;
        Vector3 bestP2 = P2_closest;

        // P1 の4頂点を取得
        Vector3[] P1_vertices = GetPlaneVertices(P1);
        Vector3[] P2_vertices = GetPlaneVertices(P2);

        // 各頂点から平面までの距離を計算
        foreach (var v1 in P1_vertices)
        {
            Vector3 proj_v1 = ProjectPointOntoPlane(v1, P2);
            float dist = Vector3.Distance(v1, proj_v1);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestP1 = v1;
                bestP2 = proj_v1;
            }
        }

        foreach (var v2 in P2_vertices)
        {
            Vector3 proj_v2 = ProjectPointOntoPlane(v2, P1);
            float dist = Vector3.Distance(v2, proj_v2);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestP1 = proj_v2;
                bestP2 = v2;
            }
        }

        return (minDistance, bestP1, bestP2);
    }

    // 指定の点を平面のローカル座標系(2D)に変換
    private static Vector2 GetLocalCoordinates(CheckPlane plane, Vector3 point)
    {
        Vector3 relative = point - plane.Center;
        return new Vector2(Vector3.Dot(relative, plane.U), Vector3.Dot(relative, plane.V));
    }

    // 平面の4つの頂点を計算
    private static Vector3[] GetPlaneVertices(CheckPlane plane)
    {
        /*
        以下として、表される.
        s+V  s+U+V
         +--+
         |  |
        .+--+
        s   s+U

        */
        return new Vector3[]
        {
            plane.startPoint,  // 左下
            plane.startPoint + plane.U,  // 右下
            plane.startPoint + plane.V,  // 左上
            plane.startPoint + plane.U + plane.V   // 右上
        };
    }

    // 点を別の平面上に投影
    private static Vector3 ProjectPointOntoPlane(Vector3 point, CheckPlane plane)
    {
        float distance = Vector3.Dot(point - plane.Center, plane.Normal);
        return point - distance * plane.Normal;
    }

    //平面の交点. ゲームつくろーから.
    public static Vector3 GetTrisCrossPoint(CheckPlane P1, CheckPlane P2)
    {
        var Vert_1 = GetPlaneVertices(P1);
        var Vert_2 = GetPlaneVertices(P2);        
        Vector3[] SearchVect_P1 = new Vector3[0];
        Vector3[] SearchVect_P2 = new Vector3[0];
        //P1の5線分求める. ()
        float P1_d0 = (Vector3.Dot(P2.Normal,Vert_1[0] - P2.startPoint));
        float P1_d1 = (Vector3.Dot(P2.Normal,Vert_1[1] - P2.startPoint));
        float P1_d2 = (Vector3.Dot(P2.Normal,Vert_1[2] - P2.startPoint));
        float P1_d3 = (Vector3.Dot(P2.Normal,Vert_1[3] - P2.startPoint));
        
        float P2_d0 = (Vector3.Dot(P1.Normal,Vert_2[0] - P1.startPoint));
        float P2_d1 = (Vector3.Dot(P1.Normal,Vert_2[1] - P1.startPoint));
        float P2_d2 = (Vector3.Dot(P1.Normal,Vert_2[2] - P1.startPoint));
        float P2_d3 = (Vector3.Dot(P1.Normal,Vert_2[3] - P1.startPoint));

        if(Finder(Vert_1[0],Vert_1[1],P1_d0,P1_d1,out Vector3 v_f0))
        {
            SearchVect_P1 = PushVect(SearchVect_P1,v_f0);
        }
        if(Finder(Vert_1[0],Vert_1[2],P1_d0,P1_d2,out Vector3 v_f1))
        {
            SearchVect_P1 = PushVect(SearchVect_P1,v_f1);
        }
        if(Finder(Vert_1[0],Vert_1[3],P1_d0,P1_d3,out Vector3 v_f2))
        {
            SearchVect_P1 = PushVect(SearchVect_P1,v_f2);
        }
        if(Finder(Vert_1[3],Vert_1[1],P1_d3,P1_d1,out Vector3 v_f3))
        {
            SearchVect_P1 = PushVect(SearchVect_P1,v_f3);
        }
        if(Finder(Vert_1[3],Vert_1[2],P1_d3,P1_d2,out Vector3 v_f4))
        {
            SearchVect_P1 = PushVect(SearchVect_P1,v_f4);
        }

        float drc = 0.03f;
        Gizmos.color = Color.green;
        foreach(var v in SearchVect_P1)
        {
            Gizmos.DrawWireCube(v, Vector3.one * drc);
        }
        
        if(Finder(Vert_2[0],Vert_2[1],P2_d0,P2_d1,out Vector3 v_g0))
        {
            SearchVect_P2 = PushVect(SearchVect_P2,v_g0);
        }
        if(Finder(Vert_2[0],Vert_2[2],P2_d0,P2_d2,out Vector3 v_g1))
        {
            SearchVect_P2 = PushVect(SearchVect_P2,v_g1);
        }
        if(Finder(Vert_2[0],Vert_2[3],P2_d0,P2_d3,out Vector3 v_g2))
        {
            SearchVect_P2 = PushVect(SearchVect_P2,v_g2);
        }
        if(Finder(Vert_2[3],Vert_2[1],P2_d3,P2_d1,out Vector3 v_g3))
        {
            SearchVect_P2 = PushVect(SearchVect_P2,v_g3);
        }
        if(Finder(Vert_2[3],Vert_2[2],P2_d3,P2_d2,out Vector3 v_g4))
        {
            SearchVect_P2 = PushVect(SearchVect_P2,v_g4);
        }

        Gizmos.color = Color.cyan;
        foreach(var v in SearchVect_P2)
        {
            Gizmos.DrawWireCube(v, Vector3.one * drc);
        }

        /*
        SearchVect_P1[0] = Vert_1[0] + (Vert_1[1] - Vert_1[0]) * (P1_d1 / (P1_d1 - P1_d2));
        SearchVect_P1[1] = Vert_1[0] + (Vert_1[2] - Vert_1[0]) * (P1_d1 / (P1_d1 - P1_d3));
        SearchVect_P1[2] = Vert_1[0] + (Vert_1[3] - Vert_1[0]) * (P1_d1 / (P1_d1 - P1_d4));
        SearchVect_P1[3] = Vert_1[3] + (Vert_1[1] - Vert_1[3]) * (P1_d4 / (P1_d4 - P1_d2));
        SearchVect_P1[4] = Vert_1[3] + (Vert_1[2] - Vert_1[3]) * (P1_d4 / (P1_d4 - P1_d3));
        float InnerDist = (SearchVect_P1[0] - Vert_1[0]).magnitude / (Vert_1[1] - Vert_1[0]).magnitude;
        //それぞれの頂点が指定の線分内なら登録..
        if(InnerDist <= 1f && Vector3.Dot(SearchVect_P1[0] - Vert_1[0],Vert_1[1] - Vert_1[0]) >= 0f)
        {
            
        }

        Gizmos.DrawWireCube(SearchVect_P1[0], Vector3.one * drc);
        Gizmos.DrawLine(SearchVect_P1[0], SearchVect_P1[2]);
        Gizmos.DrawWireCube(SearchVect_P1[1], Vector3.one * drc);
        Gizmos.DrawLine(SearchVect_P1[1], SearchVect_P1[2]);
        Gizmos.DrawWireCube(SearchVect_P1[2], Vector3.one * drc);    

        SearchVect_P2[0] = Vert_2[0] + (Vert_2[1] - Vert_2[0]) * (P2_d1 / (P2_d1 - P2_d2));
        SearchVect_P2[1] = Vert_2[0] + (Vert_2[2] - Vert_2[0]) * (P2_d1 / (P2_d1 - P2_d3));
        SearchVect_P2[2] = Vert_2[0] + (Vert_2[3] - Vert_2[0]) * (P2_d1 / (P2_d1 - P2_d4));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(SearchVect_P2[0], Vector3.one * drc);
        Gizmos.DrawLine(SearchVect_P2[0], SearchVect_P2[2]);
        Gizmos.DrawWireCube(SearchVect_P2[1], Vector3.one * drc);
        Gizmos.DrawLine(SearchVect_P2[1], SearchVect_P2[2]);
        Gizmos.DrawWireCube(SearchVect_P2[2], Vector3.one * drc);
        */

        return new Vector3();
    }

    public static Vector3[] PushVect(Vector3[] f, Vector3 pusher)
    {
        Vector3[] push = new Vector3[f.Length + 1];
        for(int i = 0; i < f.Length; i++)
        {
            push[i] = f[i];
        }
        push[push.Length - 1] = pusher;
        return push;
    }

    public static bool Finder(Vector3 V1, Vector3 V2, float V1_Cap_dist, float V2_Cap_dist, out Vector3 V3)
    {
        Vector3 vs = V1 + (V2 - V1) * (V1_Cap_dist / (V1_Cap_dist - V2_Cap_dist));
        V3 = vs;
        float InnerDist = (vs - V1).magnitude / (V2 - V1).magnitude;
        float Sign = Vector3.Dot(vs - V1,V2 - V1);        
        if(InnerDist <= 1f && Sign >= 0f)
        {
            return true;
        }
        return false;
    }

}
