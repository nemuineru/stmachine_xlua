using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
using TMPro;

public class StatusBar : MonoBehaviour
{
    [SerializeField]
    Shapes.Polyline lifeBar, lifeBar_Fill, lifeBar_Outer,
    energyBar, energyBar_Fill, energyBar_Outer;

    [SerializeField]
    Shapes.Disc chargerBar, chargerBar_Fill;

    [SerializeField]
    TMP_Text chargeUI_txt;


    [SerializeField]
    [Range(0f, 1.0f)]
    float health = 0;
    [SerializeField]
    [Range(0f, 1.0f)]
    float energy = 0;

    [SerializeField]    
    [Range(0f, 1.0f)]
    float charge = 0;

    float LowHealth = 0.3f;
    float HighEnergy = 0.99f;

    List<Color> DefC_Health, DefC_Energy;

    bool isColorChanged = false;
    float UpdateTime = 0.12f;
    float hitPtUpdateTime = 0, enePtUpdateTime = 0, chargePtUpdateTime = 0;

    [SerializeField]
    Entity refEntity;


    // Start is called before the first frame update
    void Start()
    {
        DefC_Health = lifeBar_Outer.points.Select(f => f.color).ToList();
        DefC_Energy = energyBar_Outer.points.Select(f => f.color).ToList();
    }

    void getRefEntityVals()
    {
        EntityStatus Status = refEntity.status;
        if (Status != null)
        {
            health = Status.currentHP / Status.maxHP;
            energy = Status.currentEnergy / Status.maxEnergy;
            charge = Status.ChargeTime / Status.setChargeTime_Lv2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        getRefEntityVals();
        //ライフとエナジーの描写管理.
        BarFill(lifeBar, ref lifeBar_Fill, health);
        BarFill(energyBar, ref energyBar_Fill, energy);
        ChargerFill(chargerBar,ref chargerBar_Fill, charge);
        
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
        if (hitPtUpdateTime > UpdateTime)
        {
            isColorChanged = !isColorChanged;
            hitPtUpdateTime = 0f;
        }
    }

    void BarFill(Polyline BaseBar, ref Polyline FillBar, float ref_values)
    {
        hitPtUpdateTime += Time.deltaTime;
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
        //Debug.Log(index + " " + pos);

    }

    //vals must be in the range of (0-1).
    // if not, Clamp01 it.
    void ChargerFill(Disc d_Base, ref Disc d_Fill, float vals)
    {
        float f = Mathf.Clamp01(vals);
        float degCalc = 0;
        
        degCalc = d_Base.AngRadiansStart + (d_Base.AngRadiansEnd - d_Base.AngRadiansStart) * f;
        d_Fill.AngRadiansEnd = Mathf.Lerp(d_Fill.AngRadiansEnd,degCalc,0.3f);

        chargeUI_txt.text = "";
        d_Fill.meshOutOfDate = true;
        //Debug.Log("disc changed - " +  degCalc);
    }
    
}

