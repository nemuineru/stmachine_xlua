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

        public Vector3 Normal
        {
            get
            {
                return Vector3.Cross(p1 - p0, p2 - p0);
            }
        }

        public bool isValidPoint(Vector3 point)
        {
            return true;
        }
    }

    public float TrisDistance(Triangle t1, Triangle t2)
    {
        // Calculate the distance between the two triangles
        Vector3[] points1 = { t1.p0, t1.p1, t1.p2 };
        Vector3[] points2 = { t2.p0, t2.p1, t2.p2 };

        float minDistance = float.MaxValue;

        //先ず三角面が成す平面同士において、それぞれ面が平行であるかを確認
        bool isPallarel = Vector3.Cross(t1.Normal, t2.Normal).magnitude > Mathf.Epsilon;


        //頂点と面の距離を計算.
        for (int i = 0; i < points1.Length; i++)
        {

        }

        //辺と三角形の距離を計算.
        //

        return minDistance;
    }


    //三角が成す頂点の組を出力し、交差しているかを調査したい.
    //2つの平面が成す線と三角辺の交点を求める.
    public bool GetTrisCrossPoint(Vector3[] P1, Vector3[] P2, out Vector3 crossPos)
    {
        var P1_Normal = GetPlaneVertices(P1);
        var P2_Normal = GetPlaneVertices(P2);
        P1_startPoint = Vert_1[0];
        P2_startPoint = Vert_2[0];
        //P1の3線分求める. 
        float P1_d0 = (Vector3.Dot(P2_Normal, P1[0] - P2_startPoint));
        float P1_d1 = (Vector3.Dot(P2_Normal, P1[1] - P2_startPoint));
        float P1_d2 = (Vector3.Dot(P2_Normal, P1[2] - P2_startPoint));

        //P2も同様.        
        float P2_d0 = (Vector3.Dot(P1_Normal, P2[0] - P1_startPoint));
        float P2_d1 = (Vector3.Dot(P1_Normal, P2[1] - P1_startPoint));
        float P2_d2 = (Vector3.Dot(P1_Normal, P2[2] - P1_startPoint));

        Vector3[] P1_Find = new Vector3[];
        Vector3[] P2_Find = new Vector3[];

        //交点発見時、2つ組として発見されるなら比較する
        if (Finder(Vert_1[0], Vert_1[1], P1_d0, P1_d1, out Vector3 v_f0))
        {
            P1_Find += v_f0;
        }
        if (Finder(Vert_1[0], Vert_1[2], P1_d0, P1_d2, out Vector3 v_f1))
        {
            P1_Find += v_f1;
        }
        if (Finder(Vert_1[1], Vert_1[2], P1_d2, P1_d0, out Vector3 v_f2))
        {
            P1_Find += v_f2;
        }

        if (Finder(Vert_2[0], Vert_2[1], P2_d0, P2_d1, out Vector3 v_g0))
        {
            P2_Find += v_g0;
        }
        if (Finder(Vert_2[0], Vert_2[2], P2_d0, P2_d2, out Vector3 v_g1))
        {
            P2_Find += v_g1;
        }
        if (Finder(Vert_1[1], Vert_1[2], P1_d2, P1_d0, out Vector3 v_f2))
        {
            P2_Find += v_g2;
        }

        //偽の時、三角の交点が存在しなかったと考える.
        if (P1_Find.Length != 2 || P2_Find.Length != 2)
        {
            return false;
        }
        //x軸の値から比較.
        if (!isRange_x(P1_Find[0], P1_Find[1], P2_Find[0]) && !isRange_x(P1_Find[0], P1_Find[1], P2_Find[1]) &&
        !isRange_x(P2_Find[0], P2_Find[1], P1_Find[0]) && !isRange_x(P2_Find[0], P2_Find[1], P1_Find[1]) )
        {
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
    public static bool Finder(Vector3 V1, Vector3 V2, float V1_Cap_dist, float V2_Cap_dist, out Vector3 V3)
    {
        Vector3 vs = V1 + (V2 - V1) * (V1_Cap_dist / (V1_Cap_dist - V2_Cap_dist));
        V3 = vs;
        float InnerDist = (vs - V1).magnitude / (V2 - V1).magnitude;
        float Sign = Vector3.Dot(vs - V1, V2 - V1);
        if (InnerDist <= 1f && Sign >= 0f)
        {
            return true;
        }
        return false;
    }



    //S1-(p1 p2), S2-(p1, p2)で表記される線分の距離を演算する.
    void LineDist(Vector3 S1_p1, Vector3 S1_p2, Vector3 S2_p1, Vector3 S2_p2,
    out float distance, out Vector3 closestPointOnLine1, out Vector3 closestPointOnLine2)
    {
        //最初は無限大
        distance = Mathf.Infinity;

        float S1_Dist = 0f;
        float S2_Dist = 0f;

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
                return;
            }
        }

        //S1が外縁に有ると考えてクランプして垂線を下ろす.
        S1_VectVal = Mathf.Clamp01(S1_VectVal);
        closestPointOnLine1 = S1_p1 + LineVect_S1 * S1_VectVal;
        distance = calcPointOnLineSegmentDist
        (S2_p1, S2_p2, closestPointOnLine1, out closestPointOnLine2, out S2_VectVal);
        if (S2_VectVal >= 0.0f && S2_VectVal <= 1.0f)
        {
            return;
        }

        //S2側が外に有るためクランプ・垂線を下ろす.
        S2_VectVal = Mathf.Clamp01(S2_VectVal);
        closestPointOnLine2 = S2_p1 + LineVect_S2 * S2_VectVal;
        distance = calcPointOnLineSegmentDist
        (S1_p1, S1_p2, closestPointOnLine2, out closestPointOnLine1, out S1_VectVal);
        if (S1_VectVal >= 0.0f && S1_VectVal <= 1.0f)
        {
            return;
        }

        //双方の端点が最短となる.
        S1_VectVal = Mathf.Clamp01(S1_VectVal);
        closestPointOnLine1 = S1_p1 + LineVect_S1 * S1_VectVal;
        distance = Vector3.Distance(closestPointOnLine1,closestPointOnLine2);
        return;

    }


    //2直線の最短距離・位置.
    //参考..
    //https://stackoverflow.com/questions/22303495/translate-python-in-to-unity-c-sharp-maths-or-how-to-find-shortest-distance-be

    float calcLinesDist(Vector3 L1_Start, Vector3 L1_End, Vector3 L2_Start, Vector3 L2_End,
    out Vector3 L1_CPT, out Vector3 L2_CPT, out float L1_CPT_Ratio, out float L2_CPT_Ratio)
    {
        Vector3 Line_1 = L1_End - L1_Start;
        Vector3 Line_2 = L2_End - L2_Start;

        //ライン方向ベクトル同士とそれ自体の内積の比較.
        float D_L1 = Vector3.Dot(Line_1, Line_1);
        float D_L2 = Vector3.Dot(Line_2, Line_2);

        float D_LF = Vector3.Dot(Line_1, Line_2);

        //この値が0である時は平行になるが..
        //2線分が平行であるなら垂線の端点の一つをP1に決定する.
        float Par = D_L1 * D_L2 - D_L2 * D_L2;
        if (Mathf.Abs(Par) < Mathf.Epsilon)
        { 
            Vector3 outVect;
            float f = calcPointLineDist(L1_Start, L1_End, L2_Start, out L2_CPT, out L2_CPT_Ratio);
            L1_CPT = L1_Start;
            L1_CPT_Ratio = 0.0f;
            return f;
        }

        //始点位置同士が成す内積値を求める
        Vector3 D_StartPoint = L1_Start - L2_Start;

        //始点位置同士の内積値が求まったなら、ライン成分値との内積を求める.
        float D_L1_StP = Vector3.Dot(Line_1, D_StartPoint);
        float D_L2_StP = Vector3.Dot(Line_2, D_StartPoint);

        L1_CPT_Ratio = (D_LF * D_L2_StP - D_L1_StP * D_L2) / Par;
        L2_CPT_Ratio = (D_L1 * D_L2_StP - D_L1_StP * D_LF) / Par;

        L1_CPT = L1_Start + Line_1 * L1_CPT_Ratio;
        L2_CPT = L2_Start + Line_2 * L2_CPT_Ratio;

        //最接近点の距離を計算
        return (L1_CPT - L2_CPT).magnitude;
    }

    // 頂点と線分距離
    //また、端点位置の値も考慮.
    // 点Pから最も近い線分AB上にある点o,係数をvt,距離をreturnで返す
    // https://sleepygamersmemo.blogspot.com/2019/03/perpendicular-foot-point.html より,拝借.
    float calcPointOnLineSegmentDist(Vector3 a, Vector3 b, Vector3 p, out Vector3 o, out float vt)
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
        if (length> 0.0f)
        {
            vt = Vector3.Dot(line,(p - a)) / length;
        }
        //係数を求めることで、線分計算時の係数計算が簡略化.
        o = a + line * vt;
        return (o - p).magnitude;
    }
}