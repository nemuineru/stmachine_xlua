using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//三角形が成す平行四辺形の最少接面球を考える際は原点 - p1p2中点の距離 x p1p2の距離の半分との比較を考えてみる.
//多分、三角平面から求めるよりも手軽で済むはず

public class TrisUtil
{
    public class Triangle
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            setVs();
        }

        public Vector3 Normal
        {
            get
            {
                return Vector3.Cross(p1 - p0, p2 - p0);
            }
        }

        public Vector3[] vs = new Vector3[3];

        public void setVs()
        {
            if (vs.Length != 3)
            {
                throw new ArgumentException("Triangle must have exactly 3 vertices.");
            }
            vs[0] = p0;
            vs[1] = p1;
            vs[2] = p2;
        }

        public bool isValidPoint(Vector3 point)
        {
            return true;
        }

        public void DrawTris()
        {
            Debug.DrawLine(p0,p1);
            Debug.DrawLine(p1,p2);
            Debug.DrawLine(p0,p2);
        }
    }

    public (float, Vector3, Vector3) TrisDistance(Triangle t1, Triangle t2)
    {
        float minDistance = float.MaxValue;
        Vector3 T1_Nearest = Vector3.zero;
        Vector3 T2_Nearest = Vector3.zero;

        //先ず三角面が成す平面同士において、それぞれ面が平行であるかを確認
        bool isPallarel = Vector3.Cross(t1.Normal, t2.Normal).magnitude < Mathf.Epsilon;
        //平行でないなら、交点が有るか求める
        if (!isPallarel)
        {
            Vector3 GetPos;
            Debug.Log("Triangles are not parallel, checking for intersection.");
            //交点が存在するなら、距離は0
            if (GetTrisCrossPoint(t1, t2, out GetPos))
            {
                Debug.Log("Crossed At : " + GetPos);
                T1_Nearest = GetPos;
                T2_Nearest = GetPos;
                Debug.DrawLine(T1_Nearest,T2_Nearest,Color.red);
                return (0f, T1_Nearest, T2_Nearest);
            }
        }


        //頂点と面の距離を計算.
        //今回は三角_1の点 - 三角_2の面を対象に.
        for (int i = 0; i < t1.vs.Length; i++)
        {
            Plane plane = new Plane(t2.Normal, t2.p0);
            Vector3 NearestPoint = plane.ClosestPointOnPlane(t1.vs[i]);
            float dist = (NearestPoint - t1.vs[i]).magnitude;
            //Gizmos.DrawSphere(NearestPoint, 0.1f);

            //点・面同士の最短がその対象の面にあって、最短距離で有る場合なら登録.
            if (detectPointIsEnclosedByPolygon(NearestPoint, t2.p0, t2.p1, t2.p2) && dist < minDistance)
            {
                Debug.Log("Nearest Point at : " + NearestPoint);
                T1_Nearest = t1.vs[i];
                T2_Nearest = NearestPoint;
                minDistance = dist;
            }
        }

        //次に三角_2の点 - 三角_1の面を対象に.
        for (int i = 0; i < t2.vs.Length; i++)
        {
            Plane plane = new Plane(t1.Normal, t1.p0);
            Vector3 NearestPoint = plane.ClosestPointOnPlane(t2.vs[i]);
            float dist = (NearestPoint - t2.vs[i]).magnitude;
            //Gizmos.DrawSphere(NearestPoint, 0.1f);


            //点・面同士の最短がその対象の面にあって、最短距離で有る場合なら登録.
            if (detectPointIsEnclosedByPolygon(NearestPoint, t1.p0, t1.p1, t1.p2) && dist < minDistance)
            {
                Debug.Log("Nearest Point at : " + NearestPoint);
                T2_Nearest = t2.vs[i];
                T1_Nearest = NearestPoint;
                minDistance = dist;
            }
        }

        //線分と三角形の距離を計算.
        //3x3組ずつ..
        for (int i = 0; i < t1.vs.Length; i++)
        {
            int next_i = i + 1 < t1.vs.Length ? i + 1 : 0;
            for (int j = 0; j < t2.vs.Length; j++)
            {
                int next_j = j + 1 < t2.vs.Length ? j + 1 : 0;
                //三角1, 三角2のそれぞれの線分頂点
                Vector3 T1_pt1 = t1.vs[i];
                Vector3 T1_pt2 = t1.vs[next_i];
                Vector3 T2_pt1 = t2.vs[j];
                Vector3 T2_pt2 = t2.vs[next_j];

                //最短距離と頂点の一時格納..
                float dist = Mathf.Infinity;
                Vector3 T1_Pos = Vector3.zero;
                Vector3 T2_Pos = Vector3.zero;

                //導出.
                LineDist(T1_pt1, T1_pt2, T2_pt1, T2_pt2, out dist, out T1_Pos, out T2_Pos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    //T1,T2の最接近距離を格納.
                    T1_Nearest = T1_Pos;
                    T2_Nearest = T2_Pos;
                }
            }
        }

        Debug.DrawLine(T1_Nearest,T2_Nearest,Color.red);

        //最短距離の導出を完了.
        return (minDistance, T1_Nearest, T2_Nearest);
    }

    //三角面内に指定の点が存在するなら..
    //via https://gist.github.com/hiroakioishi/d56c78671f0b0d7c9e34
    bool detectPointIsEnclosedByPolygon(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 n = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v1));

        Vector3 n0 = Vector3.Normalize(Vector3.Cross(v1 - v0, p - v1));
        Vector3 n1 = Vector3.Normalize(Vector3.Cross(v2 - v1, p - v2));
        Vector3 n2 = Vector3.Normalize(Vector3.Cross(v0 - v2, p - v0));

        if ((1.0f - Vector3.Dot(n, n0)) > 0.001f) return false;
        if ((1.0f - Vector3.Dot(n, n1)) > 0.001f) return false;
        if ((1.0f - Vector3.Dot(n, n2)) > 0.001f) return false;

        return true;
    }

    //三角が成す頂点の組を出力し、交差しているかを調査したい.
    //2つの平面が成す線と三角辺の交点を求める.
    //http://marupeke296.com/COL_3D_No21_TriTri.html より拝借
    public bool GetTrisCrossPoint(Triangle Tri_1, Triangle Tri_2, out Vector3 crossPos)
    {
        //P1の3線分・正規化された対象の平面との比較距離を求める.
        //基準点は 三角形の原点としている点.
        float P1_d0 = (Vector3.Dot(Tri_2.Normal, Tri_1.p0 - Tri_2.p0));
        float P1_d1 = (Vector3.Dot(Tri_2.Normal, Tri_1.p1 - Tri_2.p0));
        float P1_d2 = (Vector3.Dot(Tri_2.Normal, Tri_1.p2 - Tri_2.p0));

        //P2も同様.        
        float P2_d0 = (Vector3.Dot(Tri_1.Normal, Tri_2.p0 - Tri_1.p0));
        float P2_d1 = (Vector3.Dot(Tri_1.Normal, Tri_2.p1 - Tri_1.p0));
        float P2_d2 = (Vector3.Dot(Tri_1.Normal, Tri_2.p2 - Tri_1.p0));

        Vector3[] P1_Find = new Vector3[0];
        Vector3[] P2_Find = new Vector3[0];

        //交点発見時、2つ組として発見されるなら比較する
        if (getPointToPoint_RatioFinder(Tri_1.p0, Tri_1.p1, P1_d0, P1_d1, out Vector3 v_f0))
        {
            Array.Resize(ref P1_Find, P1_Find.Length + 1);
            P1_Find[P1_Find.Length - 1] = v_f0;
        }
        if (getPointToPoint_RatioFinder(Tri_1.p0, Tri_1.p2, P1_d0, P1_d2, out Vector3 v_f1))
        {
            Array.Resize(ref P1_Find, P1_Find.Length + 1);
            P1_Find[P1_Find.Length - 1] = v_f1;
        }
        if (getPointToPoint_RatioFinder(Tri_1.p1, Tri_1.p2, P1_d1, P1_d2, out Vector3 v_f2))
        {
            Array.Resize(ref P1_Find, P1_Find.Length + 1);
            P1_Find[P1_Find.Length - 1] = v_f2;
        }

        if (getPointToPoint_RatioFinder(Tri_2.p0, Tri_2.p1, P2_d0, P2_d1, out Vector3 v_g0))
        {
            Array.Resize(ref P2_Find, P2_Find.Length + 1);
            P2_Find[P2_Find.Length - 1] = v_g0;
        }
        if (getPointToPoint_RatioFinder(Tri_2.p0, Tri_2.p2, P2_d0, P2_d2, out Vector3 v_g1))
        {
            Array.Resize(ref P2_Find, P2_Find.Length + 1);
            P2_Find[P2_Find.Length - 1]  = v_g1;
        }
        if (getPointToPoint_RatioFinder(Tri_2.p1, Tri_2.p2, P2_d1, P2_d2, out Vector3 v_g2))
        {
            Array.Resize(ref P2_Find, P2_Find.Length + 1);
            P2_Find[P2_Find.Length - 1]  = v_g2;
        }

        //偽の時、三角の交点が存在しなかったと考える.
        if (P1_Find.Length != 2 || P2_Find.Length != 2)
        {
            
            crossPos = Tri_1.p0;
            return false;
        }
        Debug.Log("Finding Intersects");
        //Gizmos.DrawWireSphere(P1_Find[0],0.05f);
        //Gizmos.DrawWireSphere(P1_Find[1],0.05f);
        //Gizmos.DrawWireSphere(P2_Find[0],0.05f);
        //Gizmos.DrawWireSphere(P1_Find[1],0.05f);
        //x軸の値から比較.
        if (!isRange_x(P1_Find[0], P1_Find[1], P2_Find[0]) && !isRange_x(P1_Find[0], P1_Find[1], P2_Find[1]) &&
        !isRange_x(P2_Find[0], P2_Find[1], P1_Find[0]) && !isRange_x(P2_Find[0], P2_Find[1], P1_Find[1]))
        {
            crossPos = Tri_1.p0;
            return false;
        }
        crossPos = (P1_Find[0] + P1_Find[1]) / 2f;
        return true;
    }

    //x軸での範囲内で評価する.
    bool isRange_x(Vector3 X_1, Vector3 X_2, Vector3 Y)
    {
        if (X_1.x > X_2.x)
        {
            return Y.x <= X_1.x && Y.x >= X_2.x;
        }
        else
        {
            return Y.x <= X_2.x && Y.x >= X_1.x;
        }
    }

    //WTF
    //"直線上の交点を比で求める"　要は、平面法線との内分比率を求める.
    //また、この値が0-1を超えていればV1-V2上に線分が存在しない、ということになる.
    public static bool getPointToPoint_RatioFinder(Vector3 V1, Vector3 V2, float V1_Cap_dist, float V2_Cap_dist, out Vector3 V3)
    {
        //縮退している際は0と設定する.
        if (V1_Cap_dist - V2_Cap_dist != 0)
        {
            Vector3 vs = V1 + (V2 - V1) * (V1_Cap_dist / (V1_Cap_dist - V2_Cap_dist));
            V3 = vs;
            float InnerDist = (vs - V1).magnitude / (V2 - V1).magnitude;
            float Sign = Vector3.Dot(vs - V1, V2 - V1);
            if (InnerDist <= 1f && Sign >= 0f)
            {
                return true;
            }
        }
        V3 = V1;
        return false;
    }

    static public Vector3 ClosestPointOnPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
    => point + DistanceFromPlane(planeOffset, planeNormal, point) * planeNormal;
static public float DistanceFromPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
    => Vector3.Dot(planeOffset - point, planeNormal);



    //S1-(p1 p2), S2-(p1, p2)で表記される線分の距離を演算する.
    public void LineDist(Vector3 S1_p1, Vector3 S1_p2, Vector3 S2_p1, Vector3 S2_p2,
    out float distance, out Vector3 closestPointOnLine1, out Vector3 closestPointOnLine2)
    {
        //最初は無限大
        distance = Mathf.Infinity;

        float S1_Dist = Vector3.Distance(S1_p1, S1_p2);
        float S2_Dist = Vector3.Distance(S2_p1, S2_p2);

        closestPointOnLine1 = S1_p1;
        closestPointOnLine2 = S2_p1;

        //係数を初期化
        float S1_VectVal = 0f;
        float S2_VectVal = 0f;

        //S1 , S2が縮退しているかを考える.
        //S1が縮退している場合
        if (S1_Dist < Mathf.Epsilon)
        {
            closestPointOnLine1 = S1_p1;
            //S2が縮退している場合は点と点の距離として処理.
            if (S2_Dist < Mathf.Epsilon)
            {
                distance = Vector3.Distance(S1_p1, S2_p1);
            }
            //そうでないなら、点と線分の距離を計算する.
            else
            {
                distance = calcPointOnLineSegmentDist(S2_p1, S2_p2, S1_p1, out closestPointOnLine2, out S2_VectVal);
                S2_VectVal = Mathf.Clamp01(S2_VectVal);
            }
            return;
        }
        //S2が縮退している場合は, S1の縮退が見られないため 点と線分の距離を計算する.
        else if (S2_Dist < Mathf.Epsilon)
        {
            distance = calcPointOnLineSegmentDist(S1_p1, S1_p2, S2_p1, out closestPointOnLine1, out S1_VectVal);
            S1_VectVal = Mathf.Clamp01(S1_VectVal);
            return;
        }

        Vector3 LineVect_S1 = S1_p2 - S1_p1;
        Vector3 LineVect_S2 = S2_p2 - S2_p1;

        //2線分が平行であるなら垂線の端点の一つをP1に決定する.
        //この時、計算されたベクトル系数が0-1の間にあるなら、そのまま使用
        if (Vector3.Cross(LineVect_S1, LineVect_S2) == Vector3.zero)
        {
            //Debug.Log("Lines are parallel.");
            distance = calcPointOnLineSegmentDist(S2_p1, S2_p2, S1_p1, out closestPointOnLine2, out S2_VectVal);
            if (S2_VectVal >= 0.0f && S2_VectVal <= 1.0f)
            {
                return;
            }
        }
        //この場合ねじれの位置に有るため、2直線の最短距離・位置を取得.
        else
        {
            distance = calcLinesDist(S1_p1, S1_p2, S2_p1, S2_p2,
            out closestPointOnLine1, out closestPointOnLine2, out S1_VectVal, out S2_VectVal);
            if (S1_VectVal >= 0.0f && S1_VectVal <= 1.0f &&
            S2_VectVal >= 0.0f && S2_VectVal <= 1.0f)
            {
                //Debug.Log("Nearest Found at inside line");
                return;
            }
        }


        //Debug.Log(string.Format("S1, {0}, S2 {1}",S1_VectVal , S2_VectVal));
        //S1が外縁に有ると考えてクランプして垂線を下ろす.
        //Debug.Log(S1_VectVal);
        S1_VectVal = Mathf.Clamp01(S1_VectVal);
        closestPointOnLine1 = S1_p2 - LineVect_S1 * S1_VectVal;
        //Gizmos.DrawCube(closestPointOnLine1,Vector3.one * 0.01f);
        
        distance = calcPointOnLineSegmentDist
        (S2_p1, S2_p2, closestPointOnLine1, out closestPointOnLine2, out S2_VectVal);
        if (S2_VectVal >= 0.0f && S2_VectVal <= 1.0f)
        {
            //Debug.Log("s2 - " + S2_VectVal);
            //Debug.Log("Nearest Found at S1 point");
            return;
        }

        //S2側が外に有るためクランプ・垂線を下ろす.
        //Debug.Log(S2_VectVal);
        S2_VectVal = Mathf.Clamp01(S2_VectVal);
        closestPointOnLine2 = S2_p1 + LineVect_S2 * S2_VectVal;
        
        //Gizmos.DrawWireSphere(closestPointOnLine2, 0.01f);
        distance = calcPointOnLineSegmentDist
        (S1_p1, S1_p2, closestPointOnLine2, out closestPointOnLine1, out S1_VectVal);
        if (S1_VectVal >= 0.0f && S1_VectVal <= 1.0f)
        {
            //Debug.Log("Nearest Found at S2 point");
            return;
        }

        //双方の端点が最短となる.
        S1_VectVal = Mathf.Clamp01(S1_VectVal);
            //Debug.Log(S2_VectVal + " " + S1_VectVal);
        closestPointOnLine1 = S1_p1 + LineVect_S1 * S1_VectVal;
        closestPointOnLine2 = S2_p1 + LineVect_S2 * S2_VectVal;
        distance = Vector3.Distance(closestPointOnLine1, closestPointOnLine2);
        return;

    }


    //2直線の最短距離・位置.
    //参考..
    //https://stackoverflow.com/questions/22303495/translate-python-in-to-unity-c-sharp-maths-or-how-to-find-shortest-distance-be
    public float calcLinesDist
    (Vector3 L1_Start, Vector3 L1_End,
    Vector3 L2_Start, Vector3 L2_End, out Vector3 closestPointLine1, out Vector3 closestPointLine2
    , out float L1_CPT_Ratio, out float L2_CPT_Ratio)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;
        Vector3 lineVec1 = L1_End - L1_Start;
        Vector3 lineVec2 = L2_End - L2_Start;

        float D_L1 = Vector3.Dot(lineVec1, lineVec1);
        float D_L2 = Vector3.Dot(lineVec2, lineVec2);

        float D_LF = Vector3.Dot(lineVec1, lineVec2);

        float d = D_L1 * D_L2 - D_LF * D_LF;

        //lines are not parallel
        if (d != 0.0f)
        {
            Vector3 r = L1_End - L2_End;
            float D_L1_StP = Vector3.Dot(lineVec1, r);
            float D_L2_StP = Vector3.Dot(lineVec2, r);

            float s = (D_LF * D_L2_StP - D_L1_StP * D_L2) / d;
            float t = (D_L1 * D_L2_StP - D_L1_StP * D_LF) / d;

            closestPointLine1 = L1_End + lineVec1 * s;
            closestPointLine2 = L2_End + lineVec2 * t;
            L1_CPT_Ratio = -s;
            L2_CPT_Ratio = -t;
        }
        else
        {
            float f = calcPointLineDist(L1_Start, L1_End, L2_Start, out closestPointLine2, out L2_CPT_Ratio);
            closestPointLine1 = L1_Start;
            L1_CPT_Ratio = 0.0f;
        }
        //Debug.Log("Ratio - " + L1_CPT_Ratio + " &" + L2_CPT_Ratio);
    return Vector3.Distance(closestPointLine1, closestPointLine2);
}

    // 頂点と線分距離
    //また、端点位置の値も考慮.
    // 点Pから最も近い線分AB上にある点o,係数をvt,距離をreturnで返す
    // https://sleepygamersmemo.blogspot.com/2019/03/perpendicular-foot-point.html より,拝借.
    public float calcPointOnLineSegmentDist(Vector3 a, Vector3 b, Vector3 p, out Vector3 o, out float vt)
    {
        Vector3 ab = b - a;
        float length = ab.magnitude;
        ab.Normalize();


        float k = Vector3.Dot(p - a, ab);
        //距離の計数値を考慮.
        vt = k / length;
        k = Mathf.Clamp(k, 0, length);
        o = a + k * ab;
        return (o - p).magnitude;
    }

    //直線と点の距離.
    //oは最接近点を表す.
    float calcPointLineDist(Vector3 a, Vector3 b, Vector3 p, out Vector3 o, out float vt)
    {
        vt = 0.0f;
        Vector3 line = (b - a);
        float length = line.magnitude;
        //線が縮退していないことを確認.
        if (length > 0.0f)
        {
            vt = Vector3.Dot(line, (p - a)) / length;
        }
        //係数を求めることで、線分計算時の係数計算が簡略化.
        o = a + line * vt;
        return (o - p).magnitude;
    }
}