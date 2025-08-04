using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
public class StatusBar : MonoBehaviour
{
    [SerializeField]
    Shapes.Polyline lifeBar, lifeBar_Fill, lifeBar_Outer,
    energyBar, energyBar_Fill, energyBar_Outer;


    [SerializeField]
    [Range(0f, 1.0f)]
    float health = 0;
    [SerializeField]
    [Range(0f, 1.0f)]
    float energy = 0;

    float LowHealth = 0.3f;
    float HighEnergy = 0.99f;

    List<Color> DefC_Health, DefC_Energy;

    bool isColorChanged = false;
    float UpdateTime = 0.12f;
    float cUpdateTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        DefC_Health = lifeBar_Outer.points.Select(f => f.color).ToList();
        DefC_Energy = energyBar_Outer.points.Select(f => f.color).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        //ライフとエナジーの描写管理.
        ChangeFill(lifeBar, ref lifeBar_Fill, health);
        ChangeFill(energyBar, ref energyBar_Fill, energy);
    }

    void ChangeFill(Polyline BaseBar, ref Polyline FillBar, float ref_values)
    {
        cUpdateTime += Time.deltaTime;
        float Length = 0f;
        float calcHealth = ref_values;
        //セグメントの計算.
        List<float> sgLength = new List<float>();
        for (int t = 0; t < BaseBar.points.Count - 1; t++)
        {
            float f = (BaseBar.points[t].point - BaseBar.points[t + 1].point).sqrMagnitude;
            sgLength.Add(f);
            Length += f;
        }
        //セグメントごとの距離に変更.
        sgLength = sgLength.Select(f => f = f / Length).ToList();
        int index = 0;
        float nx = 1.0f;
        foreach (float f in sgLength)
        {
            nx = f;
            if (calcHealth <= f + 0.001f)
            {
                break;
            }
            index++;
            calcHealth -= f;
        }
        float X = calcHealth / nx;
        Vector3 pos = BaseBar.points[index].point * (1 - X) + BaseBar.points[index + 1].point * (X);
        FillBar.SetPointPosition(0, pos);
        FillBar.meshOutOfDate = true;
        Debug.Log(index + " " + pos);

        //ライフ点滅.
        for (int pt_health_Index = 0; pt_health_Index < lifeBar_Outer.Count; pt_health_Index++)
        {
            lifeBar_Outer.SetPointColor(pt_health_Index, health < LowHealth && isColorChanged ? Color.red : DefC_Health[pt_health_Index]);
        }
        //エナジー点滅
        for (int pt_energy_Index = 0; pt_energy_Index < energyBar_Outer.Count; pt_energy_Index++)
        {
            energyBar_Outer.SetPointColor(pt_energy_Index, energy > HighEnergy && isColorChanged ? Color.white : DefC_Energy[pt_energy_Index]);
        }
        if (cUpdateTime > UpdateTime)
        {
            isColorChanged = !isColorChanged;
            cUpdateTime = 0f;
        }
    }
    
}


