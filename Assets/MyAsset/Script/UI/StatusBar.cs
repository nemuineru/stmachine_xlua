using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    [SerializeField]
    Shapes.Polyline lifeBar, lifeBar_Fill, lifeBar_Outer,
    energyBar, energyBar_Fill, energyBar_Outer;

    [SerializeField]
    Shapes.Disc chargerBar, chargerBar_Fill;

    [SerializeField]
    TMP_Text healthUI_txt;
    [SerializeField]
    TMP_Text energyUI_txt;
    
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
    float HighEnergy = 0.5f;

    List<Color> DefC_Health, DefC_Energy;

    bool isColorChanged = false;
    float UpdateTime = 0.12f;
    float hitPtUpdateTime = 0, enePtUpdateTime = 0, chargePtUpdateTime = 0;

    Entity refEntity;


    // Start is called before the first frame update
    void Start()
    {
        refEntity = gameState.self.Player;
        DefC_Health = lifeBar_Outer.points.Select(f => f.color).ToList();
        DefC_Energy = energyBar_Outer.points.Select(f => f.color).ToList();
    }

    void getRefEntityVals()
    {
        EntityStatus Status = refEntity.status;
        if (Status != null)
        {
            health = Mathf.Lerp(health , Status.currentHP / Status.maxHP, 0.3333f);
            energy = Status.currentEnergy / Status.maxEnergy;
            charge = Status.ChargeTime / Status.setChargeTime_Lv2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        getRefEntityVals();
        //ライフとエナジーの描写管理.
        HPUIUpdate();
        EPUIUpdate();
        ChargerFill(chargerBar,ref chargerBar_Fill, charge);
        if (hitPtUpdateTime > UpdateTime)
        {
            isColorChanged = !isColorChanged;
            hitPtUpdateTime = 0f;
        }
    }

    void HPUIUpdate()
    {
        BarFill(lifeBar, ref lifeBar_Fill, health);
        //ライフ点滅.
        for (int pt_health_Index = 0; pt_health_Index < lifeBar_Outer.Count; pt_health_Index++)
        {
            lifeBar_Outer.SetPointColor(pt_health_Index, health < LowHealth && isColorChanged ? Color.red : DefC_Health[pt_health_Index]);
        }
        if (healthUI_txt != null)
        {
            healthUI_txt.text = Mathf.CeilToInt((refEntity.status.currentHP / refEntity.status.maxHP) * 100f) + "%";
        }
    }

    void EPUIUpdate()
    { 
        BarFill(energyBar, ref energyBar_Fill, energy);
        //エナジー点滅
        for (int pt_energy_Index = 0; pt_energy_Index < energyBar_Outer.Count; pt_energy_Index++)
        {
            energyBar_Outer.SetPointColor(pt_energy_Index, energy > HighEnergy && isColorChanged ? Color.white : DefC_Energy[pt_energy_Index]);
        }
        if (healthUI_txt != null)
        {
            energyUI_txt.text = Mathf.CeilToInt(refEntity.status.currentEnergy).ToString();
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
        if (f > 0.01f && f < 1.0f)
        {
            if (f > 0.5f)
            {
                chargeUI_txt.color = chargeUI_txt.color == Color.red ? new Color(0.7f, 0.5f, 0.5f, 1) : Color.red;
                chargeUI_txt.text = "lv1";
            }
            else
            {
                chargeUI_txt.color = Color.gray;
                chargeUI_txt.text = "lv0";
            }
        }
        else if (f == 1.0f)
        {
            chargeUI_txt.color = chargeUI_txt.color == Color.yellow ? Color.red : Color.yellow;
            chargeUI_txt.text = "max!";
        }
        d_Fill.meshOutOfDate = true;
        //Debug.Log("disc changed - " +  degCalc);
    }
    
}

